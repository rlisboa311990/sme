﻿using MediatR;
using SME.SGP.Dominio;
using SME.SGP.Dominio.Interfaces;
using SME.SGP.Infra;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using static SME.SGP.Dominio.DateTimeExtension;

namespace SME.SGP.Aplicacao
{
    public class CalcularFrequenciaPorTurmaCommandHandler : IRequestHandler<CalcularFrequenciaPorTurmaCommand, bool>
    {
        public readonly IRepositorioRegistroAusenciaAluno repositorioRegistroAusenciaAluno;
        public readonly IRepositorioFrequenciaAlunoDisciplinaPeriodo repositorioFrequenciaAlunoDisciplinaPeriodo;
        private readonly IRepositorioCompensacaoAusenciaAluno repositorioCompensacaoAusenciaAluno;
        private readonly IRepositorioProcessoExecutando repositorioProcessoExecutando;
        private readonly IUnitOfWork unitOfWork;
        private readonly IMediator mediator;

        public CalcularFrequenciaPorTurmaCommandHandler(IRepositorioRegistroAusenciaAluno repositorioRegistroAusenciaAluno,
            IRepositorioFrequenciaAlunoDisciplinaPeriodo repositorioFrequenciaAlunoDisciplinaPeriodo, IRepositorioCompensacaoAusenciaAluno repositorioCompensacaoAusenciaAluno,
            IRepositorioProcessoExecutando repositorioProcessoExecutando, IUnitOfWork unitOfWork, IMediator mediator)
        {
            this.repositorioRegistroAusenciaAluno = repositorioRegistroAusenciaAluno ?? throw new ArgumentNullException(nameof(repositorioRegistroAusenciaAluno));
            this.repositorioFrequenciaAlunoDisciplinaPeriodo = repositorioFrequenciaAlunoDisciplinaPeriodo ?? throw new ArgumentNullException(nameof(repositorioFrequenciaAlunoDisciplinaPeriodo));
            this.repositorioCompensacaoAusenciaAluno = repositorioCompensacaoAusenciaAluno ?? throw new ArgumentNullException(nameof(repositorioCompensacaoAusenciaAluno));
            this.repositorioProcessoExecutando = repositorioProcessoExecutando ?? throw new ArgumentNullException(nameof(repositorioProcessoExecutando));
            this.unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            this.mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        }
        public async Task<bool> Handle(CalcularFrequenciaPorTurmaCommand request, CancellationToken cancellationToken)
        {
            await repositorioProcessoExecutando.SalvarAsync(new ProcessoExecutando()
            {
                Bimestre = request.Bimestre,
                DisciplinaId = request.DisciplinaId,
                TipoProcesso = TipoProcesso.CalculoFrequencia,
                TurmaId = request.TurmaId,
                CriadoEm = HorarioBrasilia()
            });

            try
            {

                if (request.Alunos == null || !request.Alunos.Any())
                {
                    var alunosDaTurma = await mediator.Send(new ObterAlunosPorTurmaQuery(request.TurmaId));
                    if (alunosDaTurma.Any())
                    {
                        request.Alunos = alunosDaTurma.Select(a => a.CodigoAluno).Distinct().ToList();
                    }
                    else
                    {
                        //TODO: LOGAR NO SENTRY
                        return false;
                    }
                }

                var ausenciasDosAlunos = await repositorioRegistroAusenciaAluno.ObterTotalAusenciasPorAlunosETurmaAsync(request.DataAula, request.Alunos, request.TurmaId);

                var periodosEscolaresParaFiltro = ausenciasDosAlunos.Select(a => a.PeriodoEscolarId).Distinct().ToList();

                var frequenciaDosAlunos = await repositorioFrequenciaAlunoDisciplinaPeriodo.ObterPorAlunosAsync(request.Alunos, periodosEscolaresParaFiltro, request.TurmaId);

                var frequenciasParaRemover = new List<FrequenciaAluno>();
                var frequenciasParaPersistir = new List<FrequenciaAluno>();

                if (ausenciasDosAlunos != null && ausenciasDosAlunos.Any())
                {
                    //Transformar em uma query única?
                    var totalAulasNaDisciplina = await repositorioRegistroAusenciaAluno.ObterTotalAulasPorDisciplinaETurmaAsync(request.DataAula, request.DisciplinaId, request.TurmaId);
                    var totalAulasDaTurmaGeral = await repositorioRegistroAusenciaAluno.ObterTotalAulasPorDisciplinaETurmaAsync(request.DataAula, string.Empty, request.TurmaId);
                    //

                    var alunosComAusencias = ausenciasDosAlunos.Select(a => a.AlunoCodigo).Distinct().ToList();
                    var bimestresParaFiltro = ausenciasDosAlunos.Select(a => a.Bimestre).Distinct().ToList();

                    var totalCompensacoesDisciplinaAlunos = await repositorioCompensacaoAusenciaAluno.ObterTotalCompensacoesPorAlunosETurmaAsync(bimestresParaFiltro, alunosComAusencias, request.TurmaId);

                    foreach (var codigoAluno in alunosComAusencias)
                    {
                        var ausenciasDoAluno = ausenciasDosAlunos.Where(a => a.AlunoCodigo == codigoAluno).ToList();

                        TrataFrequenciaAlunoComponente(request, frequenciaDosAlunos, frequenciasParaPersistir, totalAulasNaDisciplina, totalCompensacoesDisciplinaAlunos, codigoAluno, ausenciasDoAluno);
                        TrataFrequenciaAlunoGlobal(request, frequenciaDosAlunos, frequenciasParaPersistir, totalAulasDaTurmaGeral, totalCompensacoesDisciplinaAlunos, codigoAluno, ausenciasDoAluno);
                    }

                }

                var alunosParaTratar = ausenciasDosAlunos.Select(a => a.AlunoCodigo)?.Distinct();
                IEnumerable<string> alunosSemAusencia;

                if (alunosParaTratar.Any())
                {
                    alunosSemAusencia = request.Alunos.Where(a => !alunosParaTratar.Contains(a));
                }
                else
                {
                    alunosSemAusencia = request.Alunos;
                }

                frequenciasParaRemover.AddRange(frequenciaDosAlunos.Where(a => alunosSemAusencia.Contains(a.CodigoAluno)).ToList());

                await TrataPersistencia(frequenciasParaRemover, frequenciasParaPersistir);
            }
            finally
            {
                var idsParaRemover = await repositorioProcessoExecutando.ObterIdsPorFiltrosAsync(request.Bimestre, request.DisciplinaId, request.TurmaId);
                if (idsParaRemover != null && idsParaRemover.Any())
                    await repositorioProcessoExecutando.RemoverIdsAsync(idsParaRemover.ToArray());
            }

            return true;
        }

        private void TrataFrequenciaAlunoGlobal(CalcularFrequenciaPorTurmaCommand request, IEnumerable<FrequenciaAluno> frequenciaDosAlunos, List<FrequenciaAluno> frequenciasParaPersistir, int totalAulasDaTurmaGeral, IEnumerable<CompensacaoAusenciaAlunoCalculoFrequenciaDto> totalCompensacoesDisciplinaAlunos, string codigoAluno, List<AusenciaPorDisciplinaAlunoDto> ausenciasDoAluno)
        {
            var frequenciaGlobalAluno = TrataFrequenciaGlobalAluno(codigoAluno, totalAulasDaTurmaGeral, ausenciasDoAluno, frequenciaDosAlunos,
                                         totalCompensacoesDisciplinaAlunos, request.TurmaId);

            if (frequenciaGlobalAluno != null)
                frequenciasParaPersistir.Add(frequenciaGlobalAluno);
        }

        private void TrataFrequenciaAlunoComponente(CalcularFrequenciaPorTurmaCommand request, IEnumerable<FrequenciaAluno> frequenciaDosAlunos, List<FrequenciaAluno> frequenciasParaPersistir, int totalAulasNaDisciplina, IEnumerable<CompensacaoAusenciaAlunoCalculoFrequenciaDto> totalCompensacoesDisciplinaAlunos, string codigoAluno, List<AusenciaPorDisciplinaAlunoDto> ausenciasDoAluno)
        {
            var frequenciaDisciplinaAluno = TrataFrequenciaPorDisciplinaAluno(codigoAluno, totalAulasNaDisciplina, ausenciasDoAluno, frequenciaDosAlunos,
                totalCompensacoesDisciplinaAlunos, request.TurmaId, request.DisciplinaId);

            if (frequenciaDisciplinaAluno != null)
                frequenciasParaPersistir.Add(frequenciaDisciplinaAluno);
        }

        private async Task TrataPersistencia(List<FrequenciaAluno> frequenciasParaRemover, List<FrequenciaAluno> frequenciasParaPersistir)
        {
            var idsParaRemover = new List<long>();

            if (frequenciasParaPersistir.Any())
            {
                idsParaRemover.AddRange(frequenciasParaPersistir
                  .Where(a => a.Id != 0)
                  .Select(a => a.Id)
                  .ToList());
            }

            if (frequenciasParaRemover.Any())
            {
                idsParaRemover.AddRange(frequenciasParaRemover
                 .Where(a => a.Id != 0)
                 .Select(a => a.Id)
                 .ToList());
            }

            var idsFinaisParaRemover = idsParaRemover.Distinct().ToArray();

            
            //TODO: BOTAR EM TRANSAÇÃO
            if (idsFinaisParaRemover != null && idsFinaisParaRemover.Any())
                await repositorioFrequenciaAlunoDisciplinaPeriodo.RemoverVariosAsync(idsFinaisParaRemover);

            if (frequenciasParaPersistir != null && frequenciasParaPersistir.Any())
                await repositorioFrequenciaAlunoDisciplinaPeriodo.SalvarVariosAsync(frequenciasParaPersistir);

            
        }

        private FrequenciaAluno TrataFrequenciaPorDisciplinaAluno(string alunoCodigo, int totalAulasNaDisciplina, IEnumerable<Infra.AusenciaPorDisciplinaAlunoDto> ausenciasDosAlunos,
            IEnumerable<FrequenciaAluno> frequenciaDosAlunos, IEnumerable<CompensacaoAusenciaAlunoCalculoFrequenciaDto> compensacoesDisciplinasAlunos,
            string turmaId, string componenteCurricularId)
        {
            FrequenciaAluno frequenciaFinal;

            var ausenciasDoAlunoPorDisciplina = ausenciasDosAlunos.FirstOrDefault(a => a.ComponenteCurricularId == componenteCurricularId);

            if (ausenciasDoAlunoPorDisciplina == null || ausenciasDoAlunoPorDisciplina.TotalAusencias == 0)
            {
                return null;
            }
            else
            {
                var frequenciaParaTratar = frequenciaDosAlunos.FirstOrDefault(a => a.CodigoAluno == alunoCodigo && a.DisciplinaId == componenteCurricularId);
                var totalCompensacoes = 0;

                var totalCompensacoesDisciplinaAluno = compensacoesDisciplinasAlunos.FirstOrDefault(a => a.AlunoCodigo == alunoCodigo && a.ComponenteCurricularId == componenteCurricularId);
                if (totalCompensacoesDisciplinaAluno != null)
                    totalCompensacoes = totalCompensacoesDisciplinaAluno.Compensacoes;


                if (frequenciaParaTratar == null)
                {
                    frequenciaFinal = new FrequenciaAluno
                             (
                                 alunoCodigo,
                                 turmaId,
                                 componenteCurricularId,
                                 ausenciasDoAlunoPorDisciplina.PeriodoEscolarId,
                                 ausenciasDoAlunoPorDisciplina.PeriodoInicio,
                                 ausenciasDoAlunoPorDisciplina.PeriodoFim,
                                 ausenciasDoAlunoPorDisciplina.Bimestre,
                                 ausenciasDoAlunoPorDisciplina.TotalAusencias,
                                 totalAulasNaDisciplina,
                                 totalCompensacoes,
                                 TipoFrequenciaAluno.PorDisciplina);
                }
                else
                {
                    frequenciaFinal = frequenciaParaTratar.DefinirFrequencia(ausenciasDoAlunoPorDisciplina.TotalAusencias, totalAulasNaDisciplina, (totalCompensacoesDisciplinaAluno?.Compensacoes ?? 0), TipoFrequenciaAluno.PorDisciplina);
                }
            }
            return frequenciaFinal;
        }

        private FrequenciaAluno TrataFrequenciaGlobalAluno(string alunoCodigo, int totalAulasDaTurmaGeral,
        IEnumerable<Infra.AusenciaPorDisciplinaAlunoDto> ausenciasDoAlunos, IEnumerable<FrequenciaAluno> frequenciaDosAlunos, IEnumerable<CompensacaoAusenciaAlunoCalculoFrequenciaDto> compensacoesDisciplinasAlunos,
        string turmaId)
        {
            FrequenciaAluno frequenciaGlobal;

            if (ausenciasDoAlunos == null || !ausenciasDoAlunos.Any())
            {
                return null;
            }
            else
            {
                var ausenciaParaSeBasear = ausenciasDoAlunos.FirstOrDefault();

                int totalCompensacoesDoAlunoGeral = 0, totalAusencias = 0;

                totalAusencias = ausenciasDoAlunos.Sum(a => a.TotalAusencias);

                var totaisDoAluno = compensacoesDisciplinasAlunos.Where(a => a.AlunoCodigo == alunoCodigo).ToList();
                if (totaisDoAluno.Any())
                {
                    totalCompensacoesDoAlunoGeral = totaisDoAluno.Sum(a => a.Compensacoes);
                }

                var frequenciaParaTratar = frequenciaDosAlunos.FirstOrDefault(a => a.CodigoAluno == alunoCodigo && string.IsNullOrEmpty(a.DisciplinaId));
                if (frequenciaParaTratar == null)
                {
                    frequenciaGlobal = new FrequenciaAluno
                             (
                                 alunoCodigo,
                                 turmaId,
                                 string.Empty,
                                 ausenciaParaSeBasear.PeriodoEscolarId,
                                 ausenciaParaSeBasear.PeriodoInicio,
                                 ausenciaParaSeBasear.PeriodoFim,
                                 ausenciaParaSeBasear.Bimestre,
                                 totalAusencias,
                                 totalAulasDaTurmaGeral,
                                 totalCompensacoesDoAlunoGeral,
                                 TipoFrequenciaAluno.Geral);
                }
                else
                {
                    frequenciaGlobal = frequenciaParaTratar.DefinirFrequencia(totalAusencias, totalAulasDaTurmaGeral, totalCompensacoesDoAlunoGeral, TipoFrequenciaAluno.Geral);
                }
            }
            return frequenciaGlobal;
        }

    }
}