﻿using MediatR;
using SME.SGP.Dominio;
using SME.SGP.Infra;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SME.SGP.Aplicacao
{
    public class ObterFrequenciasPorBimestresAlunoTurmaComponenteCurricularUseCase : IObterFrequenciasPorBimestresAlunoTurmaComponenteCurricularUseCase
    {
        private readonly IMediator mediator;

        public ObterFrequenciasPorBimestresAlunoTurmaComponenteCurricularUseCase(IMediator mediator)
        {
            this.mediator = mediator ?? throw new System.ArgumentNullException(nameof(mediator));
        }

        public async Task<IEnumerable<FrequenciaAluno>> Executar(FrequenciaPorBimestresAlunoTurmaComponenteCurricularDto dto)
        {
            var frequenciasAlunoRetorno = new List<FrequenciaAluno>();

            var turma = await mediator
                .Send(new ObterTurmaPorCodigoQuery(dto.TurmaCodigo)) ?? throw new NegocioException("Turma não encontrada!");

            var tipoCalendarioId = await mediator
                .Send(new ObterTipoCalendarioIdPorTurmaQuery(turma));

            if (tipoCalendarioId <= 0)
                throw new NegocioException("Tipo calendário da turma não encontrada!");

            var componentesConsiderados = await DefinirComponentesConsiderados(dto.ComponenteCurricularId, turma.CodigoTurma);

            var frequenciasAluno = new List<FrequenciaAluno>();

            foreach (var componenteAtual in componentesConsiderados)
            {
                frequenciasAluno.AddRange(await mediator
                    .Send(new ObterFrequenciaAlunoTurmaPorComponenteCurricularPeriodosQuery(dto.AlunoCodigo,
                                                                                            componenteAtual,
                                                                                            turma.CodigoTurma,
                                                                                            dto.Bimestres)));
            }

            if (frequenciasAluno != null && frequenciasAluno.Any())
                frequenciasAlunoRetorno.AddRange(frequenciasAluno);

            var turmasCodigo = new string[] { turma.CodigoTurma };

            var aulasComponentesTurmas = await mediator
                .Send(new ObterAulasDadasTurmaEBimestreEComponenteCurricularQuery(turmasCodigo, tipoCalendarioId, componentesConsiderados.ToArray(), dto.Bimestres));            

            foreach (var aulaComponenteTurma in aulasComponentesTurmas)
            {
                if (!frequenciasAlunoRetorno.Any(a => a.TurmaId == aulaComponenteTurma.TurmaCodigo && a.DisciplinaId == aulaComponenteTurma.ComponenteCurricularCodigo && a.Bimestre == aulaComponenteTurma.Bimestre))
                {
                    frequenciasAlunoRetorno.Add(new FrequenciaAluno()
                    {
                        CodigoAluno = dto.AlunoCodigo,
                        DisciplinaId = aulaComponenteTurma.ComponenteCurricularCodigo,
                        TurmaId = aulaComponenteTurma.TurmaCodigo,
                        TotalAulas = aulaComponenteTurma.AulasQuantidade,
                        Bimestre = aulaComponenteTurma.Bimestre
                    });
                }
            }

            return frequenciasAlunoRetorno;
        }

        private async Task<List<string>> DefinirComponentesConsiderados(string codigoComponenteCurricular, string codigoTurma)
        {
            var componentesConsiderados = new List<string> { codigoComponenteCurricular };

            var componentesEquivalentesTerritorio = await mediator
                .Send(new ObterCodigosComponentesCurricularesTerritorioSaberEquivalentesPorTurmaQuery(long.Parse(codigoComponenteCurricular), codigoTurma, string.Empty));

            componentesConsiderados.AddRange(componentesEquivalentesTerritorio
                .Select(ct => ct.codigoComponente).Except(componentesConsiderados));

            return componentesConsiderados;
        }
    }
}
