﻿using MediatR;
using SME.SGP.Dominio;
using SME.SGP.Dominio.Interfaces;
using SME.SGP.Infra;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SME.SGP.Aplicacao
{
    public class ObterPodeCadastrarAulaPorDataQueryHandler : IRequestHandler<ObterPodeCadastrarAulaPorDataQuery, PodeCadastrarAulaPorDataRetornoDto>
    {
        private readonly IRepositorioEvento repositorioEvento;
        private readonly IRepositorioTipoCalendarioConsulta repositorioTipoCalendario;
        private readonly IMediator mediator;

        public ObterPodeCadastrarAulaPorDataQueryHandler(IRepositorioEvento repositorioEvento,
                                                         IRepositorioTipoCalendarioConsulta repositorioTipoCalendario,
                                                         IMediator mediator)
        {
            this.repositorioEvento = repositorioEvento ?? throw new ArgumentNullException(nameof(repositorioEvento));
            this.repositorioTipoCalendario = repositorioTipoCalendario ?? throw new ArgumentNullException(nameof(repositorioTipoCalendario));
            this.mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        }

        public async Task<PodeCadastrarAulaPorDataRetornoDto> Handle(ObterPodeCadastrarAulaPorDataQuery request, CancellationToken cancellationToken)
        {
            var turma = await mediator.Send(new ObterTurmaComUeEDrePorCodigoQuery(request.Turma.CodigoTurma), cancellationToken);

            // Periodo Escolar
            var periodoEscolar = await repositorioTipoCalendario.ObterPeriodoEscolarPorCalendarioEData(request.TipoCalendarioId, request.DataAula);
            
            if (periodoEscolar == null)
            {
                var eventoReposicaoAulaNoDia = await repositorioEvento
                    .EventosNosDiasETipo(request.DataAula, request.DataAula, TipoEvento.ReposicaoDoDia, request.TipoCalendarioId, turma.Ue.CodigoUe, string.Empty);

                var eventoReposicaoDeAula = await repositorioEvento
                    .EventosNosDiasETipo(request.DataAula, request.DataAula, TipoEvento.ReposicaoDeAula, request.TipoCalendarioId, turma.Ue.CodigoUe, string.Empty);

                if (!eventoReposicaoAulaNoDia.Any() && !eventoReposicaoDeAula.Any())
                    return new PodeCadastrarAulaPorDataRetornoDto(false, "Não é possível cadastrar aula fora do periodo escolar");
            }

            // Domingo
            if (request.DataAula.FimDeSemana())
            {
                var temEventoLetivoDeLiberacao = await repositorioEvento.DataPossuiEventoDeLiberacaoEoutroEventoLetivo(request.TipoCalendarioId, request.DataAula, request.UeCodigo);
                
                if (!temEventoLetivoDeLiberacao)
                    return new PodeCadastrarAulaPorDataRetornoDto(false, "Não é possível cadastrar aula no final de semana");
            }

            // Evento não letivo
            var temEventoNaoLetivoNoDia = await repositorioEvento.EhEventoNaoLetivoPorTipoDeCalendarioDataDreUe(request.TipoCalendarioId, request.DataAula, request.DreCodigo, request.UeCodigo);
            // Evento Letivo
            var temEventoLetivoNoDia = await repositorioEvento.EhEventoLetivoPorTipoDeCalendarioDataDreUe(request.TipoCalendarioId, request.DataAula, request.DreCodigo, request.UeCodigo);

            if (!temEventoLetivoNoDia && temEventoNaoLetivoNoDia)
                return new PodeCadastrarAulaPorDataRetornoDto(false, "Apenas é possível consultar este registro pois existe um evento de dia não letivo");

            var mesmoAnoLetivo = DateTime.Today.Year == request.DataAula.Year;

            var temPeriodoAberto = await mediator.Send(new TurmaEmPeriodoAbertoQuery(turma, DateTime.Today, periodoEscolar.Bimestre, mesmoAnoLetivo), cancellationToken);

            return temPeriodoAberto
                ? new PodeCadastrarAulaPorDataRetornoDto(true)
                : new PodeCadastrarAulaPorDataRetornoDto(false, "Apenas é possível consultar este registro pois o período deste bimestre não está aberto.");
        }

    }
}