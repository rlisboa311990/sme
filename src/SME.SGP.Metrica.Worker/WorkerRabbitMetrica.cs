﻿using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using SME.SGP.Infra;
using SME.SGP.Infra.Interface;
using SME.SGP.Infra.Utilitarios;
using SME.SGP.Infra.Worker;
using SME.SGP.Metrica.Worker.Rotas;
using SME.SGP.Metrica.Worker.UseCases.Interfaces;

namespace SME.SGP.Metrica.Worker
{
    public class WorkerRabbitMetrica : WorkerRabbitSGP
    {
        public WorkerRabbitMetrica(IServiceScopeFactory serviceScopeFactory,
            IServicoTelemetria servicoTelemetria,
            IServicoMensageriaSGP servicoMensageria,
            IServicoMensageriaMetricas servicoMensageriaMetricas,
            IOptions<TelemetriaOptions> telemetriaOptions,
            IOptions<ConsumoFilasOptions> consumoFilasOptions,
            IConnectionFactory factory)
            : base(serviceScopeFactory, servicoTelemetria, servicoMensageria, servicoMensageriaMetricas, telemetriaOptions, consumoFilasOptions, factory, "WorkerRabbitMetrica", typeof(RotasRabbitMetrica), false)
        {
        }

        protected override void RegistrarUseCases()
        {
            Comandos.Add(RotasRabbitMetrica.AcessosSGP, new ComandoRabbit("Quantidade de Acessos Diario no SGP", typeof(IAcessosDiarioSGPUseCase)));
        }
    }
}
