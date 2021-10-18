﻿using SME.SGP.Dominio;
using SME.SGP.Dominio.Interfaces;
using SME.SGP.Infra;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SME.SGP.Aplicacao
{
    public class ConsultasPeriodoFechamento : IConsultasPeriodoFechamento
    {
        private readonly IServicoPeriodoFechamento servicoPeriodoFechamento;
        private readonly IRepositorioTurma repositorioTurma;
        private readonly IRepositorioUe repositorioUe;
        private readonly IRepositorioDre repositorioDre;
        private readonly IRepositorioEvento repositorioEvento;
        private readonly IRepositorioEventoFechamento repositorioEventoFechamento;
        private readonly IRepositorioFechamentoReabertura repositorioFechamentoReabertura;
        private readonly IRepositorioPeriodoFechamento repositorioPeriodoFechamento;
        private readonly IConsultasTipoCalendario consultasTipoCalendario;

        public ConsultasPeriodoFechamento(IServicoPeriodoFechamento servicoPeriodoFechamento,
                                IRepositorioTurma repositorioTurma,
                                IRepositorioUe repositorioUe,
                                IRepositorioDre repositorioDre,
                                IConsultasTipoCalendario consultasTipoCalendario,
                                IRepositorioEvento repositorioEvento,
                                IRepositorioEventoFechamento repositorioEventoFechamento,
                                IRepositorioFechamentoReabertura repositorioFechamentoReabertura,
                                IRepositorioPeriodoFechamento repositorioPeriodoFechamento)
        {
            this.servicoPeriodoFechamento = servicoPeriodoFechamento ?? throw new System.ArgumentNullException(nameof(servicoPeriodoFechamento));
            this.repositorioTurma = repositorioTurma ?? throw new System.ArgumentNullException(nameof(repositorioTurma));
            this.repositorioUe = repositorioUe ?? throw new System.ArgumentNullException(nameof(repositorioUe));
            this.repositorioDre = repositorioDre ?? throw new System.ArgumentNullException(nameof(repositorioDre));
            this.consultasTipoCalendario = consultasTipoCalendario ?? throw new System.ArgumentNullException(nameof(consultasTipoCalendario));
            this.repositorioEvento = repositorioEvento ?? throw new System.ArgumentNullException(nameof(repositorioEvento));
            this.repositorioEventoFechamento = repositorioEventoFechamento ?? throw new System.ArgumentNullException(nameof(repositorioEventoFechamento));
            this.repositorioFechamentoReabertura = repositorioFechamentoReabertura ?? throw new System.ArgumentNullException(nameof(repositorioFechamentoReabertura));
            this.repositorioPeriodoFechamento = repositorioPeriodoFechamento ?? throw new System.ArgumentNullException(nameof(repositorioPeriodoFechamento));
        }

        public async Task<PeriodoFechamentoBimestre> ObterPeriodoFechamentoTurmaAsync(Turma turma, int bimestre, long? periodoEscolarId)
            => await repositorioPeriodoFechamento.ObterPeriodoFechamentoTurmaAsync(turma.Ue.Id, turma.Ue.DreId, turma.AnoLetivo, bimestre, periodoEscolarId);

        public async Task<IEnumerable<PeriodoEscolar>> ObterPeriodosComFechamentoEmAberto(long ueId)
            => await repositorioEventoFechamento.ObterPeriodosFechamentoEmAberto(ueId, DateTime.Now.Date);

        public async Task<FechamentoDto> ObterPorTipoCalendarioDreEUe(FiltroFechamentoDto fechamentoDto)
        {
            return await servicoPeriodoFechamento.ObterPorTipoCalendarioDreEUe(fechamentoDto.TipoCalendarioId, fechamentoDto.DreId, fechamentoDto.UeId);
        }

        public async Task<bool> TurmaEmPeriodoDeFechamento(string turmaCodigo, DateTime dataReferencia, int bimestre = 0)
        {
            var turma = await repositorioTurma.ObterTurmaComUeEDrePorCodigo(turmaCodigo);
            var tipoCalendario = await consultasTipoCalendario.ObterPorTurma(turma);

            return await TurmaEmPeriodoDeFechamento(turma, tipoCalendario, dataReferencia, bimestre);
        }

        public async Task<bool> TurmaEmPeriodoDeFechamento(Turma turma, DateTime dataReferencia, int bimestre = 0)
        {
            var tipoCalendario = await consultasTipoCalendario.ObterPorTurma(turma);

            return await TurmaEmPeriodoDeFechamento(turma, tipoCalendario, dataReferencia, bimestre);
        }

        public async Task<bool> TurmaEmPeriodoDeFechamentoAula(Turma turma, DateTime dataReferencia, int bimestre = 0, int bimestreAlteracao = 0)
        {
            var tipoCalendario = await consultasTipoCalendario.ObterPorTurma(turma);
            var ueEmFechamento = await SmeEmFechamento(turma, tipoCalendario, bimestre, dataReferencia);
            return ueEmFechamento || await SmeEmReaberturaDeFechamento(tipoCalendario, bimestreAlteracao, dataReferencia);
        }

        public async Task<bool> TurmaEmPeriodoDeFechamento(Turma turma, TipoCalendario tipoCalendario, DateTime dataReferencia, int bimestre = 0)
        {
            var ueEmFechamento = await SmeEmFechamento(turma, tipoCalendario, bimestre, dataReferencia);

            bool retorno = ueEmFechamento || await SmeEmReaberturaDeFechamento(tipoCalendario, bimestre, dataReferencia);
            return retorno;
        }

        private async Task<bool> SmeEmFechamento(Turma turma, TipoCalendario tipoCalendario, int bimestre, DateTime dataReferencia)
        {
            return await repositorioEventoFechamento.SmeEmFechamento(dataReferencia, tipoCalendario.Id, bimestre);
        }

        private async Task<bool> SmeEmReaberturaDeFechamento(TipoCalendario tipoCalendario, int bimestre, DateTime dataReferencia)
        {
            var reaberturaPeriodo = await repositorioFechamentoReabertura.ObterReaberturaFechamentoBimestrePorDataReferencia(bimestre,dataReferencia,tipoCalendario.Id);
            return reaberturaPeriodo != null;
        }
    }
}