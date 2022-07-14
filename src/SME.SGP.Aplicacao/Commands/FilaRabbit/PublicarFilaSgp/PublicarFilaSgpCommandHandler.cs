﻿using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.ObjectPool;
using Newtonsoft.Json;
using Polly;
using Polly.Registry;
using RabbitMQ.Client;
using SME.GoogleClassroom.Infra;
using SME.SGP.Aplicacao.Integracoes;
using SME.SGP.Dominio;
using SME.SGP.Dominio.Enumerados;
using SME.SGP.Infra;
using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SME.SGP.Aplicacao
{
    public class PublicarFilaSgpCommandHandler : IRequestHandler<PublicarFilaSgpCommand, bool>
    {
        private readonly IConexoesRabbitFilasSGP conexaoRabbit;
        private readonly IServicoTelemetria servicoTelemetria;
        private readonly IAsyncPolicy policy;
        private readonly IMediator mediator;

        public PublicarFilaSgpCommandHandler(IConexoesRabbitFilasSGP conexaoRabbit, IReadOnlyPolicyRegistry<string> registry, IServicoTelemetria servicoTelemetria, IMediator mediator)
        {
            this.conexaoRabbit = conexaoRabbit ?? throw new ArgumentNullException(nameof(conexaoRabbit));
            this.servicoTelemetria = servicoTelemetria ?? throw new ArgumentNullException(nameof(servicoTelemetria));
            this.policy = registry.Get<IAsyncPolicy>(PoliticaPolly.PublicaFila);
            this.mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        }

        public async Task<bool> Handle(PublicarFilaSgpCommand command, CancellationToken cancellationToken)
        {
            var usuario = command.Usuario ?? await ObtenhaUsuario();

            var administrador = await mediator.Send(new ObterAdministradorDoSuporteQuery());

            var request = new MensagemRabbit(command.Filtros,
                                             command.CodigoCorrelacao,
                                             command.Usuario?.Nome,
                                             command.Usuario?.CodigoRf,
                                             command.Usuario?.PerfilAtual,
                                             command.NotificarErroUsuario,
                                             administrador.Login);

            var mensagem = JsonConvert.SerializeObject(request, new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore
            });
            var body = Encoding.UTF8.GetBytes(mensagem);

            servicoTelemetria.Registrar(() => 
                    policy.ExecuteAsync(() => PublicarMensagem(command.Rota, body, command.Exchange)), 
                            "RabbitMQ", "PublicarFilaSgp", command.Rota);

            return true;
        }

        private async Task<Usuario> ObtenhaUsuario()
        {
            try
            {
                return await mediator.Send(new ObterUsuarioLogadoQuery());
            } 
            catch
            {
                return new Usuario();
            }
        }

        private async Task PublicarMensagem(string rota, byte[] body, string exchange = null)
        {
            var _channel = conexaoRabbit.Get();
            try
            {
                var props = _channel.CreateBasicProperties();
                props.Persistent = true;

                _channel.BasicPublish(exchange ?? ExchangeSgpRabbit.Sgp, rota, props, body);
            }
            finally
            {
                conexaoRabbit.Return(_channel);
            }
        }

    }
}
