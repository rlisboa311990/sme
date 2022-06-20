﻿using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using SME.SGP.Aplicacao;
using SME.SGP.Aplicacao.Interfaces;
using SME.SGP.Dados.Repositorios;
using SME.SGP.Dominio.Interfaces;

namespace SME.SGP.IoC
{
    internal static partial class RegistrarCasoDeUsoRabbitSgp
    {
        internal static void RegistrarAEEUseCaseRabbitSgp(this IServiceCollection services)
        {
            services.TryAddScoped<INotificacaoConclusaoEncaminhamentoAEEUseCase, NotificacaoConclusaoEncaminhamentoAEEUseCase>();
            services.TryAddScoped<INotificacaoEncerramentoEncaminhamentoAEEUseCase, NotificacaoEncerramentoEncaminhamentoAEEUseCase>();
            services.TryAddScoped<INotificacaoDevolucaoEncaminhamentoAEEUseCase, NotificacaoDevolucaoEncaminhamentoAEEUseCase>();
            services.TryAddScoped<IEncerrarEncaminhamentoAEEAutomaticoSyncUseCase, EncerrarEncaminhamentoAEEAutomaticoSyncUseCase>();
            services.TryAddScoped<IValidarEncerrarEncaminhamentoAEEAutomaticoUseCase, ValidarEncerrarEncaminhamentoAEEAutomaticoUseCase>();
            services.TryAddScoped<IEncerrarEncaminhamentoAEEAutomaticoUseCase, EncerrarEncaminhamentoAEEAutomaticoUseCase>();
            services.TryAddScoped<IEncerrarPlanosAEEEstudantesInativosUseCase, EncerrarPlanosAEEEstudantesInativosUseCase>();
            services.TryAddScoped<IGerarPendenciaValidadePlanoAEEUseCase, GerarPendenciaValidadePlanoAEEUseCase>();
            services.TryAddScoped<INotificarPlanosAEEExpiradosUseCase, NotificarPlanosAEEExpiradosUseCase>();
            services.TryAddScoped<INotificarPlanosAEEEmAbertoUseCase, NotificarPlanosAEEEmAbertoUseCase>();
            services.TryAddScoped<IEnviarNotificacaoReestruturacaoPlanoAEEUseCase, EnviarNotificacaoReestruturacaoPlanoAEEUseCase>();
            services.TryAddScoped<IEnviarNotificacaoCriacaoPlanoAEEUseCase, EnviarNotificacaoCriacaoPlanoAEEUseCase>();
            services.TryAddScoped<IEnviarNotificacaoEncerramentoPlanoAEEUseCase, EnviarNotificacaoEncerramentoPlanoAEEUseCase>();
            services.TryAddScoped<INotificacaoSalvarItineranciaUseCase, NotificacaoSalvarItineranciaUseCase>();

            #region ATENÇÃO - Use Cases injetados em processos consumidos pelo o worker
            // TODO
            services.TryAddScoped<IObterDataCriacaoRelatorioUseCase, ObterDataCriacaoRelatorioUseCase>();
            services.TryAddScoped<IRepositorioTipoRelatorio, RepositorioTipoRelatorio>();

            #endregion
        }
    }
}
