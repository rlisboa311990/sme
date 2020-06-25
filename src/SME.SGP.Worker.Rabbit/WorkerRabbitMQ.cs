﻿using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Sentry;
using Sentry.Protocol;
using SME.SGP.Aplicacao;
using SME.SGP.Dominio;
using SME.SGP.Infra;
using SME.SGP.Infra.Contexto;
using SME.SGP.Infra.Excecoes;
using SME.SGP.Infra.Interfaces;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace SME.SGP.Worker.Rabbit
{
    public class WorkerRabbitMQ : BackgroundService
    //public class WorkerRabbitMQ : IHostedService
    {
        private readonly IModel canalRabbit;
        private readonly string sentryDSN;
        private readonly IConnection conexaoRabbit;
        private readonly IServiceScopeFactory serviceScopeFactory;

        /// <summary>
        /// configuração da lista de tipos para a fila do rabbit instanciar, seguindo a ordem de propriedades:
        /// rota do rabbit, usaMediatr?, tipo
        /// </summary>
        private readonly Dictionary<string, (bool, Type)> comandos;


        public WorkerRabbitMQ(IConnection conexaoRabbit, IServiceScopeFactory serviceScopeFactory, IConfiguration configuration)
        {
            sentryDSN = configuration.GetValue<string>("Sentry:DSN");
            this.conexaoRabbit = conexaoRabbit ?? throw new ArgumentNullException(nameof(conexaoRabbit));
            this.serviceScopeFactory = serviceScopeFactory ?? throw new ArgumentNullException(nameof(serviceScopeFactory));
            canalRabbit = conexaoRabbit.CreateModel();

            canalRabbit.ExchangeDeclare(RotasRabbit.ExchangeServidorRelatorios, ExchangeType.Topic);
            canalRabbit.QueueDeclare(RotasRabbit.FilaSgp, false, false, false, null);
            canalRabbit.QueueBind(RotasRabbit.FilaSgp, RotasRabbit.ExchangeServidorRelatorios, "*", null);

            comandos = new Dictionary<string, (bool, Type)>();
            RegistrarUseCases();
        }

        private void RegistrarUseCases()
        {
            comandos.Add(RotasRabbit.RotaRelatoriosProntos, (false, typeof(IReceberRelatorioProntoUseCase)));
            comandos.Add(RotasRabbit.RotaExcluirAulaRecorrencia, (false, typeof(IExcluirAulaRecorrenteUseCase)));
            comandos.Add(RotasRabbit.RotaInserirAulaRecorrencia, (false, typeof(IInserirAulaRecorrenteUseCase)));
            comandos.Add(RotasRabbit.RotaAlterarAulaRecorrencia, (false, typeof(IAlterarAulaRecorrenteUseCase)));
        }

        private async Task TratarMensagem(BasicDeliverEventArgs ea)
        {
            var mensagem = System.Text.Encoding.UTF8.GetString(ea.Body.Span);
            var rota = ea.RoutingKey;
            if (comandos.ContainsKey(rota))
            {
                using (SentrySdk.Init(sentryDSN))
                {
                    var mensagemRabbit = JsonConvert.DeserializeObject<MensagemRabbit>(mensagem);
                    SentrySdk.AddBreadcrumb($"Dados: {mensagemRabbit.Filtros}");
                    try
                    {
                        using (var scope = serviceScopeFactory.CreateScope())
                        {
                            var tipoComando = comandos[rota];

                            AtribuirContextoAplicacao(mensagemRabbit, scope);

                            //usar mediatr?
                            SentrySdk.CaptureMessage($"{mensagemRabbit.UsuarioLogadoRF} - {mensagemRabbit.CodigoCorrelacao.ToString().Substring(0, 3)} - EXECUTANDO - {ea.RoutingKey}", SentryLevel.Debug);
                            if (tipoComando.Item1)
                            {
                                var comando = JsonConvert.DeserializeObject(mensagemRabbit.Filtros.ToString(), tipoComando.Item2);
                                var mediatr = scope.ServiceProvider.GetService<IMediator>();
                                await mediatr.Send(comando);
                            }
                            else
                            {
                                var casoDeUso = scope.ServiceProvider.GetService(tipoComando.Item2);
                                await tipoComando.Item2.GetMethod("Executar").InvokeAsync(casoDeUso, new object[] { mensagemRabbit });
                            }
                            SentrySdk.CaptureMessage($"{mensagemRabbit.UsuarioLogadoRF} - {mensagemRabbit.CodigoCorrelacao.ToString().Substring(0, 3)} - SUCESSO - {ea.RoutingKey}", SentryLevel.Info);
                            canalRabbit.BasicAck(ea.DeliveryTag, false);
                        }
                    }
                    catch (NegocioException nex)
                    {
                        SentrySdk.AddBreadcrumb($"Erros: {nex.Message}");
                        RegistrarSentry(ea, mensagemRabbit, nex);
                    }
                    catch (ValidacaoException vex)
                    {
                        SentrySdk.AddBreadcrumb($"Erros: {JsonConvert.SerializeObject(vex.Mensagens())}");
                        RegistrarSentry(ea, mensagemRabbit, vex);
                    }
                    catch (Exception ex)
                    {
                        SentrySdk.AddBreadcrumb($"Erros: {ex.Message}");
                        RegistrarSentry(ea, mensagemRabbit, ex);
                    }
                }
            }
        }

        private void RegistrarSentry(BasicDeliverEventArgs ea, MensagemRabbit mensagemRabbit, Exception ex)
        {
            SentrySdk.CaptureMessage($"{mensagemRabbit.UsuarioLogadoRF} - {mensagemRabbit.CodigoCorrelacao.ToString().Substring(0, 3)} - ERRO - {ea.RoutingKey}", SentryLevel.Error);
            SentrySdk.CaptureException(ex);
            canalRabbit.BasicReject(ea.DeliveryTag, false);
        }

        private static void AtribuirContextoAplicacao(MensagemRabbit mensagemRabbit, IServiceScope scope)
        {
            if (!string.IsNullOrWhiteSpace(mensagemRabbit.UsuarioLogadoRF))
            {
                var contextoAplicacao = scope.ServiceProvider.GetService<IContextoAplicacao>();
                var variaveis = new Dictionary<string, object>();
                variaveis.Add("NomeUsuario", mensagemRabbit.UsuarioLogadoNomeCompleto);
                variaveis.Add("UsuarioLogado", mensagemRabbit.UsuarioLogadoRF);
                variaveis.Add("RF", mensagemRabbit.UsuarioLogadoRF);
                variaveis.Add("login", mensagemRabbit.UsuarioLogadoRF);
                variaveis.Add("Claims", new List<InternalClaim> { new InternalClaim { Value = mensagemRabbit.PerfilUsuario, Type = "perfil" } });
                contextoAplicacao.AdicionarVariaveis(variaveis);
            }
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            canalRabbit.Close();
            conexaoRabbit.Close();
            return Task.CompletedTask;
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            stoppingToken.ThrowIfCancellationRequested();
            var consumer = new EventingBasicConsumer(canalRabbit);
            consumer.Received += async (ch, ea) =>
            {

                await TratarMensagem(ea);
            };

            canalRabbit.BasicConsume(RotasRabbit.FilaSgp, false, consumer);
            return Task.CompletedTask;
        }
    }
}
