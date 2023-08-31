﻿using Elasticsearch.Net.Specification.IndicesApi;
using MediatR;
using SME.SGP.Dominio;
using SME.SGP.Dominio.Enumerados;
using SME.SGP.Dominio.Interfaces;
using SME.SGP.Infra;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SME.SGP.Aplicacao
{
    public class ObterParecerConclusivoAlunoQueryHandler : IRequestHandler<ObterParecerConclusivoAlunoQuery, ConselhoClasseParecerConclusivo>
    {
        protected IEnumerable<ConselhoClasseParecerConclusivo> pareceresDoServico;
        private readonly IRepositorioFrequenciaAlunoDisciplinaPeriodoConsulta repositorioFrequenciaAlunoDisciplinaPeriodo;

        private readonly IMediator mediator;

        public ObterParecerConclusivoAlunoQueryHandler(IMediator mediator, IRepositorioFrequenciaAlunoDisciplinaPeriodoConsulta repositorioFrequenciaAlunoDisciplinaPeriodo)
        {
            this.mediator = mediator ?? throw new System.ArgumentNullException(nameof(mediator));
            this.repositorioFrequenciaAlunoDisciplinaPeriodo = repositorioFrequenciaAlunoDisciplinaPeriodo ?? throw new ArgumentNullException(nameof(repositorioFrequenciaAlunoDisciplinaPeriodo));
        }

        public bool Filtrar(IEnumerable<ConselhoClasseParecerConclusivo> pareceresDoServico, string nomeClasseCalculo)
        {
            this.pareceresDoServico = pareceresDoServico;

            // Verifica se retornou 1 verdadeiro e 1 falso
            if (pareceresDoServico == null || !pareceresDoServico.Any())
                return false;

            if (!pareceresDoServico.Where(c => c.Aprovado).Any())
                throw new NegocioException($"Não localizado parecer conclusivo aprovado para o calculo por {nomeClasseCalculo}");
            if (pareceresDoServico.Where(c => c.Aprovado).Count() > 1)
                throw new NegocioException($"Encontrado mais de 1 parecer conclusivo aprovado para o calculo por {nomeClasseCalculo}");

            if (!pareceresDoServico.Where(c => !c.Aprovado).Any())
                throw new NegocioException($"Não localizado parecer conclusivo reprovado para o calculo por {nomeClasseCalculo}");
            if (pareceresDoServico.Where(c => !c.Aprovado).Count() > 1)
                throw new NegocioException($"Encontrado mais de 1 parecer conclusivo reprovado para o calculo por {nomeClasseCalculo}");

            return true;
        }

        private ConselhoClasseParecerConclusivo ObterParecerValidacao(bool retornoValidacao)
            => pareceresDoServico.FirstOrDefault(c => c.Aprovado == retornoValidacao);

        public async Task<ConselhoClasseParecerConclusivo> Handle(ObterParecerConclusivoAlunoQuery request, CancellationToken cancellationToken)
        {
            var turma = await mediator.Send(new ObterTurmaComUeEDrePorCodigoQuery(request.TurmaCodigo));
            var turmasItinerarioEnsinoMedio = await mediator.Send(ObterTurmaItinerarioEnsinoMedioQuery.Instance);

            var alunosEol = await mediator.Send(new ObterTodosAlunosNaTurmaQuery(int.Parse(turma.CodigoTurma), int.Parse(request.AlunoCodigo)));           

            string[] turmasCodigos;

            if (!turma.Historica && (turma.DeveVerificarRegraRegulares() || turmasItinerarioEnsinoMedio.Any(a => a.Id == (int)turma.TipoTurma)))
            {
                var tiposParaConsulta = new List<int> { (int)turma.TipoTurma };
                var tiposRegularesDiferentes = turma.ObterTiposRegularesDiferentes();
                    
                tiposParaConsulta.AddRange(tiposRegularesDiferentes.Where(c => tiposParaConsulta.All(x => x != c)));
                tiposParaConsulta.AddRange(turmasItinerarioEnsinoMedio.Select(s => s.Id).Where(c => tiposParaConsulta.All(x => x != c)));                
                
                turmasCodigos = await mediator.Send(new ObterTurmaCodigosAlunoPorAnoLetivoAlunoTipoTurmaQuery(turma.AnoLetivo, request.AlunoCodigo, tiposParaConsulta, turma.Historica, ueCodigo: turma.Ue.CodigoUe, semestre: turma.Semestre));
            }
            else
                turmasCodigos = new string[1] { turma.CodigoTurma };

            if (!turmasCodigos.Any())
                turmasCodigos = new string[] { turma.CodigoTurma };

            // Frequencia
            Filtrar(request.PareceresDaTurma.Where(c => c.Frequencia), "Frequência");
            if (!await ValidarParecerPorFrequencia(request.AlunoCodigo, turma, turmasCodigos, alunosEol.ToArray()))
                return ObterParecerValidacao(false);

            var parecerFrequencia = ObterParecerValidacao(true);

            // Nota

            if (!Filtrar(request.PareceresDaTurma.Where(c => c.Nota), "Nota"))
                return parecerFrequencia;

            var parecerNota = ObterParecerValidacao(await ValidarParecerPorNota(request.AlunoCodigo, turmasCodigos, turma.AnoLetivo));

            // Conselho
            if (!Filtrar(request.PareceresDaTurma.Where(c => c.Conselho), "Conselho"))
                return parecerNota;

            var validacaoConselho = await ValidarParecerPorConselho(request.AlunoCodigo, turmasCodigos, turma.AnoLetivo, turma);
            if (!validacaoConselho.ExisteNotaConselho)
                return parecerNota;

            var parecerValidacao = ObterParecerValidacao(validacaoConselho.ValidacaoNotaConselho);

            if (parecerValidacao.Aprovado && parecerNota.Aprovado)
                return parecerNota;

            return parecerValidacao;
        }

        #region Frequência
        private async Task<bool> ValidarParecerPorFrequencia(string alunoCodigo, Turma turma, string[] turmasCodigos, AlunoPorTurmaResposta[] informacoesAluno)
        {
            if (!await ValidarFrequenciaGeralAluno(alunoCodigo, turmasCodigos, turma.AnoLetivo))
                return false;

            var parametroFrequenciaBaseNacional = await ObterFrequenciaBaseNacional(turma.AnoLetivo);
            var tipoCalendarioId = await mediator.Send(new ObterTipoCalendarioIdPorTurmaQuery(turma));

            var periodos = new long[] { };
            var periodosEscolaresTipoCalendario = await mediator.Send(new ObterPeriodosEscolaresPorTipoCalendarioIdQuery(tipoCalendarioId));

            if (periodosEscolaresTipoCalendario.Any())
                periodos = periodosEscolaresTipoCalendario.Select(p => p.Id).ToArray();

            Usuario usuarioAtual = await mediator.Send(ObterUsuarioLogadoQuery.Instance);

            var componentesCurriculares = await mediator.Send(new ObterComponentesCurricularesPorTurmasCodigoQuery(turmasCodigos, usuarioAtual.PerfilAtual, usuarioAtual.Login, turma.EnsinoEspecial, turma.TurnoParaComponentesCurriculares));

            // Filtra componentes que lançam frequência
            var componentesCurriculareslancaFrequencia = componentesCurriculares.Where(c => c.RegistraFrequencia);
            var componentesCurricularesCodigos = componentesCurriculareslancaFrequencia.Select(c => c.CodigoComponenteCurricular.ToString()).ToArray();

            var frequenciasAluno = await repositorioFrequenciaAlunoDisciplinaPeriodo.ObterPorAlunoTurmasDisciplinasDataAsync(alunoCodigo, TipoFrequenciaAluno.PorDisciplina,
                                         componentesCurricularesCodigos, turmasCodigos, new int[] { }, !periodos.Any() ? null : periodos);

            frequenciasAluno = await VerificaFrequenciaNaoRegistradaMasComAulaCriada(componentesCurricularesCodigos, periodosEscolaresTipoCalendario, frequenciasAluno, informacoesAluno);

            var frequencias = frequenciasAluno.Where(a => componentesCurricularesCodigos.Contains(a.DisciplinaId));            

            if (informacoesAluno != null && informacoesAluno.Any())
            {
                frequencias = from f in frequencias
                              from dm in informacoesAluno.Select(ia => ia.DataMatricula)
                              where dm.Date < f.PeriodoFim.Date
                              select f;
            }   

            if (FrequenciaAnualPorComponenteCritica(AgruparValoresPorDisciplina(frequencias), parametroFrequenciaBaseNacional, turma.AnoLetivo))
                return false;

            return true;
        }

        private async Task<IEnumerable<FrequenciaAluno>> VerificaFrequenciaNaoRegistradaMasComAulaCriada(string[] componentesCurriculares, IEnumerable<PeriodoEscolar> periodosEscolares
                                                                                                         , IEnumerable<FrequenciaAluno> frequenciasConsolidadas, AlunoPorTurmaResposta[] informacoesAluno)
        {
            var frequenciasAjustadasParaParecerConclusivo = new List<FrequenciaAluno>();

            var periodosEscolaresFrequentadosPeloEstudante = informacoesAluno != null && informacoesAluno.Any() ?
                                                             from p in periodosEscolares
                                                             from ia in informacoesAluno
                                                             where ia.DataMatricula < p.PeriodoFim.Date
                                                             select p : Enumerable.Empty<PeriodoEscolar>();

            if (periodosEscolaresFrequentadosPeloEstudante.Any())
            {
                bool possuiTodasFrequenciasConsolidadas = (periodosEscolaresFrequentadosPeloEstudante.Count() * componentesCurriculares.Length) == frequenciasConsolidadas.Count();

                if (!possuiTodasFrequenciasConsolidadas && frequenciasConsolidadas.Any())
                {
                    var aulasComponentesTurmas = await mediator
                                .Send(new ObterTotalAulasTurmaEBimestreEComponenteCurricularQuery(frequenciasConsolidadas.Select(f => f.TurmaId).Distinct().ToArray()
                                , periodosEscolaresFrequentadosPeloEstudante.FirstOrDefault().TipoCalendarioId, componentesCurriculares, periodosEscolaresFrequentadosPeloEstudante.Select(p => p.Bimestre).ToArray()));

                    foreach (var aula in aulasComponentesTurmas)
                    {
                        var possuiFrequenciaConsolidada = frequenciasConsolidadas.Any(f => f.Bimestre == aula.Bimestre && f.DisciplinaId == aula.ComponenteCurricularCodigo);
                        if (!possuiFrequenciaConsolidada)
                        {
                            var valorAulaRegistrada = aulasComponentesTurmas.FirstOrDefault(a => a.Bimestre == aula.Bimestre && a.ComponenteCurricularCodigo == aula.ComponenteCurricularCodigo);
                            if (valorAulaRegistrada != null)
                            {
                                frequenciasAjustadasParaParecerConclusivo.Add(new FrequenciaAluno()
                                {
                                    CodigoAluno = informacoesAluno.First().CodigoAluno,
                                    DisciplinaId = valorAulaRegistrada.ComponenteCurricularCodigo,
                                    TurmaId = valorAulaRegistrada.TurmaCodigo,
                                    TotalAulas = 0,
                                    Bimestre = valorAulaRegistrada.Bimestre,
                                    PeriodoEscolarId = valorAulaRegistrada.PeriodoEscolarId,
                                    PeriodoFim = periodosEscolares.Where(p => valorAulaRegistrada.Bimestre == p.Bimestre).Select(p => p.PeriodoFim).FirstOrDefault(),
                                    PeriodoInicio = periodosEscolares.Where(p => valorAulaRegistrada.Bimestre == p.Bimestre).Select(p => p.PeriodoInicio).FirstOrDefault()
                                });
                            }

                        }
                    }

                    frequenciasAjustadasParaParecerConclusivo.AddRange(frequenciasConsolidadas);
                    return frequenciasAjustadasParaParecerConclusivo;
                }
            }

            return frequenciasConsolidadas;
        }

        private async Task<double> ObterFrequenciaBaseNacional(int anoLetivo)
            => double.Parse(
                await mediator.Send(
                    new ObterValorParametroSistemaTipoEAnoQuery(TipoParametroSistema.PercentualFrequenciaCriticoBaseNacional, anoLetivo)));

        private bool FrequenciaAnualPorComponenteCritica(IEnumerable<FrequenciaAluno> frequenciasComponentes, double parametroFrequenciaBaseNacional, int anoLetivo)
        {
            if (anoLetivo != 2020)
                return frequenciasComponentes.Any(f => f.TotalAulas > 0 && f.PercentualFrequencia < parametroFrequenciaBaseNacional);
            else
                return false;
        }

        private IEnumerable<FrequenciaAluno> AgruparValoresPorDisciplina(IEnumerable<FrequenciaAluno> frequenciasAluno)
         => frequenciasAluno
                .GroupBy(a => a.DisciplinaId)
                .Select(f => new FrequenciaAluno()
                {
                    DisciplinaId = f.Key,
                    TotalAulas = f.Sum(x => x.TotalAulas),
                    TotalAusencias = f.Sum(x => x.TotalAusencias),
                    TotalCompensacoes = f.Sum(x => x.TotalCompensacoes),
                });


        private async Task<bool> ValidarFrequenciaGeralAluno(string alunoCodigo, string[] turmasCodigos, int anoLetivo)
        {
            var frequenciaAluno = await mediator.Send(new ObterConsultaFrequenciaGeralAlunoPorTurmasQuery(alunoCodigo, turmasCodigos));
            double valorFrequenciaAluno = 0;
            if (frequenciaAluno != "")
                valorFrequenciaAluno = Convert.ToDouble(frequenciaAluno);

            var parametroFrequenciaGeral = double.Parse(await mediator.Send(new ObterValorParametroSistemaTipoEAnoQuery(TipoParametroSistema.PercentualFrequenciaCritico, anoLetivo)));
            return !(valorFrequenciaAluno < parametroFrequenciaGeral);
        }
        #endregion

        #region Nota
        private async Task<bool> ValidarParecerPorNota(string alunoCodigo, string[] turmasCodigos, int anoLetivo)
        {
            var notasFechamentoAluno = await mediator.Send(new ObterNotasFinaisPorAlunoTurmasQuery(alunoCodigo, turmasCodigos));

            if (notasFechamentoAluno == null || !notasFechamentoAluno.Any())
                return true;

            var tipoNota = notasFechamentoAluno.First().ConceitoId.HasValue ? TipoNota.Conceito : TipoNota.Nota;
            return tipoNota == TipoNota.Nota ?
                await ValidarParecerPorNota(notasFechamentoAluno, anoLetivo) :
                await ValidarParecerPorConceito(notasFechamentoAluno);
        }

        private async Task<bool> ValidarParecerPorNota(IEnumerable<NotaConceitoBimestreComponenteDto> notasFechamentoAluno, int anoLetivo)
        {
            var notaMedia = double.Parse(await mediator.Send(new ObterValorParametroSistemaTipoEAnoQuery(TipoParametroSistema.MediaBimestre, anoLetivo)));
            foreach (var notaFechamentoAluno in notasFechamentoAluno)
                if (notaFechamentoAluno.Nota < notaMedia)
                    return false;

            return true;
        }

        private async Task<bool> ValidarParecerPorConceito(IEnumerable<NotaConceitoBimestreComponenteDto> conceitosFechamentoAluno)
        {
            var conceitosVigentes = await mediator.Send(new ObterConceitoPorDataQuery(DateTime.Today));
            foreach (var conceitoFechamentoAluno in conceitosFechamentoAluno)
            {
                var conceitoAluno = conceitosVigentes.FirstOrDefault(c => c.Id == conceitoFechamentoAluno.ConceitoId);
                if (conceitoAluno != null && !conceitoAluno.Aprovado)
                    return false;
            }

            return true;
        }
        #endregion

        #region Conselho
        private async Task<(bool ExisteNotaConselho, bool ValidacaoNotaConselho)> ValidarParecerPorConselho(string alunoCodigo, string[] turmasCodigos, int anoLetivo, Turma turma)
        {
            bool validaNota = false;
            bool validaConceito = false;

            if (turmasCodigos.Count() == 1)
            {
                var tipoNotaTurma = await mediator.Send(new ObterTipoNotaPorTurmaIdQuery(turma.Id, turma.TipoTurma));
                
                if(tipoNotaTurma != null)
                {
                    validaConceito = tipoNotaTurma.TipoNota == TipoNota.Conceito;
                    validaNota = tipoNotaTurma.TipoNota == TipoNota.Nota;
                }
            }

            var notasConselhoClasse = await mediator.Send(new ObterNotasFinaisConselhoFechamentoPorAlunoTurmasQuery(turmasCodigos, alunoCodigo, validaNota, validaConceito));
            if (notasConselhoClasse == null || !notasConselhoClasse.Any())
                return (false, false);
            else
                notasConselhoClasse = notasConselhoClasse.Any(x=>x.ConselhoClasseAlunoId != 0) ? 
                    notasConselhoClasse.OrderByDescending(c=> c.ConselhoClasseAlunoId).DistinctBy(c => c.ComponenteCurricularCodigo) 
                    : notasConselhoClasse.OrderByDescending(c => c.FechamentoNotaId).DistinctBy(c => c.ComponenteCurricularCodigo);

            var tipoNota = notasConselhoClasse.First().ConceitoId.HasValue ? TipoNota.Conceito : TipoNota.Nota;
            return (true, tipoNota == TipoNota.Nota ?
               await ValidarParecerConselhoPorNota(notasConselhoClasse, anoLetivo) :
               await ValidarParecerConselhoPorConceito(notasConselhoClasse));
        }

        private async Task<bool> ValidarParecerConselhoPorNota(IEnumerable<NotaConceitoFechamentoConselhoFinalDto> notasConselhoClasse, int anoLetivo)
        {
            var notaMedia = double.Parse(await mediator.Send(new ObterValorParametroSistemaTipoEAnoQuery(TipoParametroSistema.MediaBimestre, anoLetivo)));
            foreach (var notaConcelhoClasse in notasConselhoClasse)
            {
                var nota = notaConcelhoClasse.Nota;
                var notaPosConselho = notasConselhoClasse.FirstOrDefault(n => n.ComponenteCurricularCodigo == notaConcelhoClasse.ComponenteCurricularCodigo && n.ConselhoClasseAlunoId > 0);
                if (notaPosConselho != null)
                {
                    nota = notaPosConselho.Nota;
                }
                if (nota < notaMedia)
                    return false;
            }

            return true;
        }

        private async Task<bool> ValidarParecerConselhoPorConceito(IEnumerable<NotaConceitoFechamentoConselhoFinalDto> notasConselhoClasse)
        {
            var conceitosVigentes = await mediator.Send(new ObterConceitoPorDataQuery(DateTime.Today));
            foreach (var conceitoConselhoClasseAluno in notasConselhoClasse)
            {
                var conceitoId = conceitoConselhoClasseAluno.ConceitoId;

                var conceitoPosConselho = notasConselhoClasse.FirstOrDefault(n => n.ComponenteCurricularCodigo == conceitoConselhoClasseAluno.ComponenteCurricularCodigo && n.ConselhoClasseAlunoId > 0);
                if (conceitoPosConselho != null)
                {
                    conceitoId = conceitoPosConselho.ConceitoId;
                }

                var conceitoAluno = conceitosVigentes.FirstOrDefault(c => c.Id == conceitoId);
                if (conceitoAluno != null && !conceitoAluno.Aprovado)
                    return false;
            }

            return true;
        }
        #endregion
    }
}
