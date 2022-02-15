﻿using MediatR;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Polly;
using Polly.Registry;
using RabbitMQ.Client;
using SME.GoogleClassroom.Infra;
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
        private readonly IConfiguration configuration;
        private readonly IServicoTelemetria servicoTelemetria;
        private readonly IAsyncPolicy policy;
        private readonly IMediator mediator;

        public PublicarFilaSgpCommandHandler(IConfiguration configuration, IReadOnlyPolicyRegistry<string> registry, IServicoTelemetria servicoTelemetria, IMediator mediator)
        {
            this.configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            this.servicoTelemetria = servicoTelemetria ?? throw new ArgumentNullException(nameof(servicoTelemetria));
            this.policy = registry.Get<IAsyncPolicy>(PoliticaPolly.PublicaFila);
            this.mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        }

        public async Task<bool> Handle(PublicarFilaSgpCommand command, CancellationToken cancellationToken)
        {
            var request = new MensagemRabbit(command.Filtros,
                                             command.CodigoCorrelacao,
                                             command.Usuario?.Nome,
                                             command.Usuario?.CodigoRf,
                                             command.Usuario?.PerfilAtual,
                                             command.NotificarErroUsuario);

            var mensagem = JsonConvert.SerializeObject(request, new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore
            });
            var body = Encoding.UTF8.GetBytes(mensagem);

            servicoTelemetria.Registrar(() => 
                    policy.ExecuteAsync(() => PublicarMensagem(command.Rota, body)), 
                            "RabbitMQ", "PublicarFilaSgp", command.Rota);

            return true;
        }

        private async Task PublicarMensagem(string rota, byte[] body)
        {
            var factory = new ConnectionFactory
            {
                HostName = configuration.GetSection("ConfiguracaoRabbitOptions:HostName").Value,
                UserName = configuration.GetSection("ConfiguracaoRabbitOptions:UserName").Value,
                Password = configuration.GetSection("ConfiguracaoRabbitOptions:Password").Value,
                VirtualHost = configuration.GetSection("ConfiguracaoRabbitOption:Virtualhost").Value
            };

            await mediator
                .Send(new SalvarLogViaRabbitCommand($"{DateTime.Now:dd/MM/yyyy HH:mm:ss} - Alocação da mensagem em fila. Conf.: host={factory.HostName}/user={factory.UserName}/password={factory.Password}/virtual_host={factory.VirtualHost}", LogNivel.Negocio, LogContexto.Infantil));

            using (var conexaoRabbit = factory.CreateConnection())
            {
                using (IModel _channel = conexaoRabbit.CreateModel())
                {
                    var props = _channel.CreateBasicProperties();
                    props.Persistent = true;

                    _channel.BasicPublish(ExchangeSgpRabbit.Sgp, rota, props, body);
                }
            }
        }

    }
}
