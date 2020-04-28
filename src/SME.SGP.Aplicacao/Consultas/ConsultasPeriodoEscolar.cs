﻿using SME.SGP.Dominio;
using SME.SGP.Dominio.Interfaces;
using SME.SGP.Infra;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SME.SGP.Aplicacao.Consultas
{
    public class ConsultasPeriodoEscolar : IConsultasPeriodoEscolar
    {
        private readonly IRepositorioPeriodoEscolar repositorio;
        private readonly IConsultasPeriodoFechamento consultasPeriodoFechamento;
        private readonly IConsultasTipoCalendario consultasTipoCalendario;

        public ConsultasPeriodoEscolar(IRepositorioPeriodoEscolar repositorio,
                                    IConsultasPeriodoFechamento consultasPeriodoFechamento,
                                    IConsultasTipoCalendario consultasTipoCalendario)
        {
            this.repositorio = repositorio ?? throw new ArgumentNullException(nameof(repositorio));
            this.consultasPeriodoFechamento = consultasPeriodoFechamento ?? throw new ArgumentNullException(nameof(consultasPeriodoFechamento));
            this.consultasTipoCalendario = consultasTipoCalendario ?? throw new ArgumentNullException(nameof(consultasTipoCalendario));
        }

        public PeriodoEscolarListaDto ObterPorTipoCalendario(long tipoCalendarioId)
        {
            var lista = repositorio.ObterPorTipoCalendario(tipoCalendarioId);

            if (lista == null || !lista.Any())
                return null;

            return EntidadeParaDto(lista);
        }

        public DateTime ObterFimPeriodoRecorrencia(long tipoCalendarioId, DateTime inicioRecorrencia, RecorrenciaAula recorrencia)
        {
            var periodos = repositorio.ObterPorTipoCalendario(tipoCalendarioId);
            if (periodos == null || !periodos.Any())
                throw new NegocioException("Não foi possível obter os períodos deste tipo de calendário.");

            DateTime fimRecorrencia = DateTime.MinValue;
            if (recorrencia == RecorrenciaAula.RepetirBimestreAtual)
            {
                // Busca ultimo dia do periodo atual
                fimRecorrencia = periodos.Where(a => a.PeriodoFim >= inicioRecorrencia)
                    .OrderBy(a => a.PeriodoInicio)
                    .FirstOrDefault().PeriodoFim;
            }
            else
            if (recorrencia == RecorrenciaAula.RepetirTodosBimestres)
            {
                // Busca ultimo dia do ultimo periodo
                fimRecorrencia = periodos.Max(a => a.PeriodoFim);
            }

            return fimRecorrencia;
        }

        public PeriodoEscolar ObterPeriodoEscolarPorData(long tipoCalendarioId, DateTime dataPeriodo)
            => repositorio.ObterPorTipoCalendarioData(tipoCalendarioId, dataPeriodo);

        private PeriodoEscolarDto MapearParaDto(PeriodoEscolar periodo)
            => periodo == null ? null : new PeriodoEscolarDto()
            {
                Id = periodo.Id,
                Bimestre = periodo.Bimestre,
                Migrado = periodo.Migrado,
                PeriodoInicio = periodo.PeriodoInicio,
                PeriodoFim = periodo.PeriodoFim
            };

        private static PeriodoEscolarListaDto EntidadeParaDto(IEnumerable<PeriodoEscolar> lista)
        {
            return new PeriodoEscolarListaDto
            {
                TipoCalendario = lista.ElementAt(0).TipoCalendarioId,
                Periodos = lista.Select(x => new PeriodoEscolarDto
                {
                    Bimestre = x.Bimestre,
                    PeriodoInicio = x.PeriodoInicio,
                    PeriodoFim = x.PeriodoFim,
                    Migrado = x.Migrado,
                    Id = x.Id
                }).ToList()
            };
        }

        public int ObterBimestre(DateTime data, Modalidade modalidade, int semestre = 0)
        {
            var periodoEscolar = ObterPeriodoPorModalidade(modalidade, data, semestre);

            return periodoEscolar?.Bimestre ?? 0;
        }

        public async Task<IEnumerable<PeriodoEscolar>> ObterPeriodosEmAberto(long ueId, Modalidade modalidadeCodigo, int anoLetivo)
        {
            var dataAtual = DateTime.Today;
            var tipoCalendario = consultasTipoCalendario.BuscarPorAnoLetivoEModalidade(anoLetivo,
                                                                modalidadeCodigo == Modalidade.EJA ?
                                                                    ModalidadeTipoCalendario.EJA :
                                                                    ModalidadeTipoCalendario.FundamentalMedio,
                                                                dataAtual.Semestre());

            var periodos = new List<PeriodoEscolar>();
            var periodoAtual = ObterPeriodoEscolarPorData(tipoCalendario.Id, dataAtual);
            if (periodoAtual != null)
                periodos.Add(periodoAtual);
            periodos.AddRange(await consultasPeriodoFechamento.ObterPeriodosComFechamentoEmAberto(ueId));

            return periodos;
        }

        public async Task<PeriodoEscolar> ObterUltimoPeriodoAsync(int anoLetivo, ModalidadeTipoCalendario modalidade, int semestre)
            => await repositorio.ObterUltimoBimestreAsync(anoLetivo, modalidade, semestre);

        public PeriodoEscolar ObterPeriodoAtualPorModalidade(Modalidade modalidade)
            => ObterPeriodoPorModalidade(modalidade, DateTime.Today);

        public PeriodoEscolar ObterPeriodoPorModalidade(Modalidade modalidade, DateTime data, int semestre = 0)
        {
            var tipoCalendario = ObterTipoCalendario(modalidade, data.Year, semestre);
            var periodosEscolares = ObterPeriodosEscolares(tipoCalendario.Id);

            return periodosEscolares.FirstOrDefault(x => x.PeriodoInicio <= data && x.PeriodoFim >= data);
        }

        public IEnumerable<PeriodoEscolar> ObterPeriodosEscolares(long tipoCalendarioId)
        {
            var periodosEscolares = repositorio.ObterPorTipoCalendario(tipoCalendarioId);
            if (periodosEscolares == null || !periodosEscolares.Any())
                throw new NegocioException("Não encontrado periodo escolar cadastrado");

            return periodosEscolares;
        }

        private TipoCalendarioCompletoDto ObterTipoCalendario(Modalidade modalidade, int anoLetivo, int semestre = 0)
        {
            var modalidadeCalendario = modalidade == Modalidade.EJA ? ModalidadeTipoCalendario.EJA : ModalidadeTipoCalendario.FundamentalMedio;

            var tipoCalendario = consultasTipoCalendario.BuscarPorAnoLetivoEModalidade(anoLetivo, modalidadeCalendario, semestre);
            if (tipoCalendario == null)
                throw new NegocioException("Não encontrado calendario escolar cadastrado");

            return tipoCalendario;
        }

        public PeriodoEscolar ObterPeriodoPorData(IEnumerable<PeriodoEscolar> periodosEscolares, DateTime data)
            => periodosEscolares?.FirstOrDefault(p => p.DataDentroPeriodo(data));

        public PeriodoEscolar ObterUltimoPeriodoPorData(IEnumerable<PeriodoEscolar> periodosEscolares, DateTime data)
            => periodosEscolares.OrderByDescending(o => o.PeriodoInicio)
                .FirstOrDefault(p => p.PeriodoFim <= data);

        public async Task<PeriodoEscolar> ObterUltimoPeriodoAbertoAsync(Turma turma)
        {
            var periodosAberto = await consultasPeriodoFechamento.ObterPeriodosComFechamentoEmAberto(turma.UeId);

            PeriodoEscolar periodoEscolar = null;
            if (periodosAberto != null && periodosAberto.Any())
            {
                // caso tenha mais de um periodo em aberto (abertura e reabertura) usa o ultimo bimestre
                periodoEscolar = periodosAberto.OrderBy(c => c.Bimestre).Last();
            }
            else
            {
                // Caso não esteja em periodo de fechamento ou escolar busca o ultimo existente
                var tipoCalendario = consultasTipoCalendario.BuscarPorAnoLetivoEModalidade(turma.AnoLetivo, turma.ModalidadeTipoCalendario, turma.Semestre);
                if (tipoCalendario == null)
                    throw new NegocioException("Não foi encontrado calendário cadastrado para a turma");
                var periodosEscolares = ObterPeriodosEscolares(tipoCalendario.Id);
                if (periodosEscolares == null)
                    throw new NegocioException("Não foram encontrados periodos escolares cadastrados para a turma");

                periodoEscolar = ObterPeriodoPorData(periodosEscolares, DateTime.Today);
                if (periodoEscolar == null)
                    periodoEscolar = ObterUltimoPeriodoPorData(periodosEscolares, DateTime.Today);
            }

            return periodoEscolar;
        }
    }
}