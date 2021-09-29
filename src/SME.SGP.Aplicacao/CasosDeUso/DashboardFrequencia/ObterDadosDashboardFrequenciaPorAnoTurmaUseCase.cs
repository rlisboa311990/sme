﻿using MediatR;
using SME.SGP.Dominio;
using SME.SGP.Dominio.Enumerados;
using SME.SGP.Infra;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SME.SGP.Aplicacao
{
    public class ObterDadosDashboardFrequenciaPorAnoTurmaUseCase : AbstractUseCase, IObterDadosDashboardFrequenciaPorAnoTurmaUseCase
    {
        public ObterDadosDashboardFrequenciaPorAnoTurmaUseCase(IMediator mediator) : base(mediator)
        {
        }

        public async Task<GraficoFrequenciaAlunoDto> Executar(int anoLetivo, long dreId, long ueId, int modalidade, int semestre, string anoTurma, DateTime dataInicio, DateTime datafim, int mes, int tipoPeriodoDashboard, bool visaoDre = false)
        {
            var dadosFrequenciaAlunos = await mediator.Send(new ObterDadosDashboardFrequenciaPorAnoTurmaQuery(anoLetivo,
                                                                                                              dreId,
                                                                                                              ueId,
                                                                                                              modalidade,
                                                                                                              semestre,
                                                                                                              anoTurma,
                                                                                                              dataInicio,
                                                                                                              datafim,
                                                                                                              mes,
                                                                                                              tipoPeriodoDashboard,
                                                                                                              visaoDre));

            if ((!string.IsNullOrEmpty(anoTurma) && anoTurma != "-99") && dadosFrequenciaAlunos != null)
                dadosFrequenciaAlunos = dadosFrequenciaAlunos.Where(a => a.Ano == anoTurma).ToList();

            if (dadosFrequenciaAlunos == null || !dadosFrequenciaAlunos.Any())
                return null;


            var dadosTotais = await mediator.Send(new ObterTotalFrequenciaEAulasPorPeriodoQuery(anoLetivo,
                                                                                                dreId,
                                                                                                ueId,
                                                                                                modalidade,
                                                                                                semestre,
                                                                                                anoTurma,
                                                                                                dataInicio,
                                                                                                datafim,
                                                                                                mes,
                                                                                                tipoPeriodoDashboard));

            if ((!string.IsNullOrEmpty(anoTurma) && anoTurma != "-99") && dadosTotais != null)
                dadosTotais = dadosTotais.Where(a => a.DescricaoAnoTurma == anoTurma).ToList();

            var dadosTotal = new TotalFrequenciaEAulasPorPeriodoDto()
            {
                TotalAulas = dadosTotais.Select(a => a.TotalAulas).Sum(),
                TotalFrequencias = dadosTotais.Select(a => a.TotalFrequencias).Sum(),
            };
            var totalFrequencia = dadosTotal != null ? dadosTotal.TotalFrequenciaFormatado : "";

            var dreCodigo = "";
            var ueCodigo = "";


            if (ueId != -99)
            {
                var ue = await mediator.Send(new ObterUeComDrePorIdQuery(ueId));
                ueCodigo = ue.CodigoUe;
                dreCodigo = ue.Dre.CodigoDre;
            }
            else if (dreId != -99)
                dreCodigo = await mediator.Send(new ObterCodigoDREPorUeIdQuery(dreId));

            var totalEstudantesAgrupado = await ObterQuantidadeAlunosMatriculadosEol(anoLetivo, ueId, modalidade, anoTurma, dreCodigo, ueCodigo, visaoDre);
            return MapearParaDto(dadosFrequenciaAlunos, totalFrequencia, totalEstudantesAgrupado);
        }

        private async Task<IEnumerable<IGrouping<string, QuantidadeAlunoMatriculadoDTO>>> ObterQuantidadeAlunosMatriculadosEol(int anoLetivo, long ueId, int modalidade, string anoTurma, string dreCodigo, string ueCodigo, bool visaoDre)
        {
            var totalAlunos = await mediator.Send(new ObterQuantidadeAlunosEolMatriculadosQuery(anoLetivo, dreCodigo, ueCodigo, modalidade, anoTurma));

            if (totalAlunos == null || !totalAlunos.Any())
                return null;

            IEnumerable<IGrouping<string, QuantidadeAlunoMatriculadoDTO>> totalEstudantesAgrupado;

            if (visaoDre)
                totalEstudantesAgrupado = totalAlunos.GroupBy(c => c.DreCodigo);
            else if (ueId != -99)
                totalEstudantesAgrupado = totalAlunos.GroupBy(c => c.Turma);
            else
                totalEstudantesAgrupado = totalAlunos.GroupBy(c => c.Ano);

            return totalEstudantesAgrupado;
        }

        private GraficoFrequenciaAlunoDto MapearParaDto(IEnumerable<FrequenciaAlunoDashboardDto> frequenciasAlunos, string tagTotalFrequencia, IEnumerable<IGrouping<string, QuantidadeAlunoMatriculadoDTO>> totalEstudantesAgrupado)
        {
            var dadosFrequenciaDashboard = new List<DadosRetornoFrequenciaAlunoDashboardDto>();

            foreach (var frequencias in frequenciasAlunos.GroupBy(f => f.DescricaoAnoTurma))
            {
                var dreAbreviacao = frequencias.First().DreAbreviacao;
                var descricaoAnoTurmaFormatado = frequencias.First().DescricaoAnoTurmaFormatado;

                dadosFrequenciaDashboard.Add(new DadosRetornoFrequenciaAlunoDashboardDto()
                {
                    Descricao = TipoFrequenciaDashboard.Presentes.Name(),
                    TurmaAno = !string.IsNullOrEmpty(dreAbreviacao) ?
                    FormatarAbreviacaoDre(dreAbreviacao) :
                    descricaoAnoTurmaFormatado,
                    Quantidade = frequencias.Select(f => f.Presentes).Sum()
                });

                dadosFrequenciaDashboard.Add(new DadosRetornoFrequenciaAlunoDashboardDto()
                {
                    Descricao = TipoFrequenciaDashboard.Remotos.Name(),
                    TurmaAno = !string.IsNullOrEmpty(dreAbreviacao) ?
                    FormatarAbreviacaoDre(dreAbreviacao) :
                    descricaoAnoTurmaFormatado,
                    Quantidade = frequencias.Select(f => f.Remotos).Sum()
                });

                dadosFrequenciaDashboard.Add(new DadosRetornoFrequenciaAlunoDashboardDto()
                {
                    Descricao = TipoFrequenciaDashboard.Ausentes.Name(),
                    TurmaAno = !string.IsNullOrEmpty(dreAbreviacao) ?
                    FormatarAbreviacaoDre(dreAbreviacao) :
                    descricaoAnoTurmaFormatado,
                    Quantidade = frequencias.Select(f => f.Ausentes).Sum()
                });

                dadosFrequenciaDashboard.Add(new DadosRetornoFrequenciaAlunoDashboardDto()
                {
                    Descricao = TipoFrequenciaDashboard.TotalEstudantes.Name(),
                    TurmaAno = !string.IsNullOrEmpty(dreAbreviacao) ?
                    FormatarAbreviacaoDre(dreAbreviacao) :
                    descricaoAnoTurmaFormatado,
                    Quantidade = totalEstudantesAgrupado != null ? (totalEstudantesAgrupado.FirstOrDefault(c => c.Key == frequencias.First().DescricaoAnoTurma) != null ? totalEstudantesAgrupado.First(c => c.Key == frequencias.First().DescricaoAnoTurma).Select(x => x.Quantidade).Sum() : 0) : 0
                });
            }

            return new GraficoFrequenciaAlunoDto()
            {
                TagTotalFrequencia = tagTotalFrequencia,
                TotalFrequenciaFormatado = tagTotalFrequencia,
                DadosFrequenciaDashboard = dadosFrequenciaDashboard
            };
        }

        private static string FormatarAbreviacaoDre(string abreviacaoDre)
            => abreviacaoDre.Replace(DashboardConstants.PrefixoDreParaSerRemovido, string.Empty).Trim();
    }
}
