﻿using MediatR;
using SME.SGP.Aplicacao.Integracoes;
using SME.SGP.Aplicacao.Integracoes.Respostas;
using SME.SGP.Dominio;
using SME.SGP.Dominio.Enumerados;
using SME.SGP.Infra;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SME.SGP.Aplicacao
{
    public class ObterNotasParaAvaliacoesListaoUseCase : IObterNotasParaAvaliacoesListaoUseCase
    {
        private readonly IMediator mediator;
        private readonly IConsultasDisciplina consultasDisciplina;
        private readonly IServicoEol servicoEOL;
        public ObterNotasParaAvaliacoesListaoUseCase(IMediator mediator, IConsultasDisciplina consultasDisciplina, IServicoEol servicoEOL)
        {
            this.mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            this.consultasDisciplina = consultasDisciplina ?? throw new ArgumentNullException(nameof(consultasDisciplina));
            this.servicoEOL = servicoEOL ?? throw new ArgumentNullException(nameof(servicoEOL));
        }
        public async Task<NotasConceitosListaoRetornoDto> Executar(ListaNotasConceitosConsultaRefatoradaDto filtro)
        {
            var retorno = new NotasConceitosListaoRetornoDto();

            var turmaCompleta = await mediator
                .Send(new ObterTurmaComUeEDrePorCodigoQuery(filtro.TurmaCodigo));

            if (turmaCompleta == null)
                throw new NegocioException("Não foi possível obter a turma.");


            var disciplinasDoProfessorLogado = await consultasDisciplina.ObterComponentesCurricularesPorProfessorETurma(filtro.TurmaCodigo, true);

            if (disciplinasDoProfessorLogado == null || !disciplinasDoProfessorLogado.Any())
                throw new NegocioException("Não foi possível obter os componentes curriculares do usuário logado.");

            var periodoFechamentoBimestre = await mediator.Send(new ObterPeriodoFechamentoPorTurmaBimestrePeriodoEscolarQuery(turmaCompleta, filtro.Bimestre, filtro.PeriodoEscolarId));

            var periodoInicio = new DateTime(filtro.PeriodoInicioTicks);
            var periodoFim = new DateTime(filtro.PeriodoFimTicks);

            var componentesCurriculares = ObterComponentesCurricularesParaConsulta(filtro.DisciplinaCodigo, disciplinasDoProfessorLogado);
            var atividadesAvaliativaEBimestres = await mediator
                .Send(new ObterAtividadesAvaliativasPorCCTurmaPeriodoQuery(componentesCurriculares.Select(a => a.ToString()).ToArray(), filtro.TurmaCodigo, periodoInicio, periodoFim));

            var alunos = await mediator.Send(new ObterAlunosPorTurmaEAnoLetivoQuery(filtro.TurmaCodigo));

            if (alunos == null || !alunos.Any())
                throw new NegocioException("Não foi encontrado alunos para a turma informada");

            var tipoAvaliacaoBimestral = await mediator.Send(new ObterTipoAvaliacaoBimestralQuery());

            retorno.BimestreAtual = filtro.Bimestre;
            retorno.MediaAprovacaoBimestre = double.Parse(await mediator.Send(new ObterValorParametroSistemaTipoEAnoQuery(TipoParametroSistema.MediaBimestre, DateTime.Today.Year)));
            retorno.MinimoAvaliacoesBimestrais = tipoAvaliacaoBimestral.AvaliacoesNecessariasPorBimestre;
            retorno.PercentualAlunosInsuficientes = double.Parse(await mediator.Send(new ObterValorParametroSistemaTipoEAnoQuery(TipoParametroSistema.PercentualAlunosInsuficientes, DateTime.Today.Year)));

            DateTime? dataUltimaNotaConceitoInserida = null;
            DateTime? dataUltimaNotaConceitoAlterada = null;
            var usuarioRfUltimaNotaConceitoInserida = string.Empty;
            var usuarioRfUltimaNotaConceitoAlterada = string.Empty;
            var nomeAvaliacaoAuditoriaInclusao = string.Empty;
            var nomeAvaliacaoAuditoriaAlteracao = string.Empty;

            //TODO  REVER
            AtividadeAvaliativa atividadeAvaliativaParaObterTipoNota = null;

            var bimestreParaAdicionar = new NotasConceitosBimestreListaoRetornoDto()
            {
                Descricao = $"{filtro.Bimestre}º Bimestre",
                Numero = filtro.Bimestre
            };
            var listaAlunosDoBimestre = new List<NotasConceitosAlunoListaoRetornoDto>();
            var atividadesAvaliativasdoBimestre = atividadesAvaliativaEBimestres
                                    .Where(a => a.DataAvaliacao.Date >= periodoInicio && periodoFim >= a.DataAvaliacao.Date)
                                    .OrderBy(a => a.DataAvaliacao)
                                    .ToList();

            var alunosIds = alunos.Select(a => a.CodigoAluno).Distinct().ToArray();
            IEnumerable<NotaConceito> notas = null;
            IEnumerable<AusenciaAlunoDto> ausenciasDasAtividadesAvaliativas = null;

            long[] atividadesAvaliativasId = atividadesAvaliativasdoBimestre.Select(a => a.Id)?.Distinct().ToArray() ?? new long[0];
            notas = await mediator.Send(new ObterNotasPorAlunosAtividadesAvaliativasQuery(atividadesAvaliativasId, alunosIds, filtro.DisciplinaCodigo.ToString()));
            var datasDasAtividadesAvaliativas = atividadesAvaliativasdoBimestre.Select(a => a.DataAvaliacao).Distinct().ToArray();
            ausenciasDasAtividadesAvaliativas = await mediator.Send(new ObterAusenciasDaAtividadesAvaliativasQuery(filtro.TurmaCodigo, datasDasAtividadesAvaliativas, filtro.DisciplinaCodigo.ToString(), alunosIds));

            var componentesCurricularesCompletos = await mediator.Send(new ObterComponentesCurricularesPorIdsQuery(new long[] { filtro.DisciplinaCodigo }));
            if (componentesCurricularesCompletos == null || !componentesCurricularesCompletos.Any())
                throw new NegocioException("Componente curricular informado não encontrado no EOL");

            var componenteReferencia = componentesCurricularesCompletos.FirstOrDefault(a => a.CodigoComponenteCurricular == filtro.DisciplinaCodigo);

            IEnumerable<DisciplinaResposta> disciplinasRegencia = null;
            if (componenteReferencia.Regencia)
            {
                var usuario = await mediator.Send(new ObterUsuarioLogadoQuery());
                if (usuario.EhProfessorCj())
                {
                    IEnumerable<DisciplinaDto> disciplinasRegenciaCJ = await consultasDisciplina.ObterComponentesCurricularesPorProfessorETurmaParaPlanejamento(filtro.DisciplinaCodigo, filtro.TurmaCodigo, false, componenteReferencia.Regencia);
                    if (disciplinasRegenciaCJ == null || !disciplinasRegenciaCJ.Any())
                        throw new NegocioException("Não foram encontradas as disciplinas de regência");
                    disciplinasRegencia = MapearParaDto(disciplinasRegenciaCJ);
                }
                else
                {
                    IEnumerable<ComponenteCurricularEol> disciplinasRegenciaEol = await servicoEOL.ObterComponentesCurricularesPorCodigoTurmaLoginEPerfilParaPlanejamento(filtro.TurmaCodigo, usuario.CodigoRf, usuario.PerfilAtual);
                    if (disciplinasRegenciaEol == null || !disciplinasRegenciaEol.Any(d => !d.TerritorioSaber && d.Regencia))
                        throw new NegocioException("Não foram encontradas disciplinas de regência no EOL");
                    disciplinasRegencia = MapearParaDto(disciplinasRegenciaEol.Where(d => !d.TerritorioSaber && d.Regencia));
                }

            }
            var fechamentosNotasDaTurma = await mediator.Send(new ObterFechamentosPorTurmaPeriodoCCQuery(filtro.PeriodoEscolarId, filtro.TurmaId, filtro.DisciplinaCodigo));


            IOrderedEnumerable<AlunoPorTurmaResposta> alunosAtivos = null;
            if (filtro.TurmaHistorico)
            {
                alunosAtivos = from a in alunos
                               where a.EstaAtivo(periodoFim) ||
                                     (a.EstaInativo(periodoFim) && a.DataSituacao.Date >= periodoInicio.Date && a.DataSituacao.Date <= periodoFim.Date) &&
                                      (a.CodigoSituacaoMatricula == SituacaoMatriculaAluno.Concluido || a.CodigoSituacaoMatricula == SituacaoMatriculaAluno.Transferido)
                               orderby a.NomeValido(), a.NumeroAlunoChamada
                               select a;
            }
            else
            {
                alunosAtivos = from a in alunos
                               where (a.EstaAtivo(periodoFim) ||
                                     (a.EstaInativo(periodoFim) && a.DataSituacao.Date >= periodoInicio.Date)) &&
                                     a.DataMatricula.Date <= periodoFim.Date
                               orderby a.NomeValido(), a.NumeroAlunoChamada
                               select a;
            }

            var alunosAtivosCodigos = alunosAtivos.Select(a => a.CodigoAluno).Distinct().ToArray();

            var frequenciasDosAlunos = await mediator
                .Send(new ObterFrequenciasPorAlunosTurmaCCDataQuery(alunosAtivosCodigos, periodoFim, TipoFrequenciaAluno.PorDisciplina, filtro.TurmaCodigo, filtro.DisciplinaCodigo.ToString()));

            var turmaPossuiFrequenciaRegistrada = await mediator.Send(new ExisteFrequenciaRegistradaPorTurmaComponenteCurricularQuery(filtro.TurmaCodigo, filtro.DisciplinaCodigo.ToString(), filtro.PeriodoEscolarId));

            foreach (var aluno in alunosAtivos)
            {
                var notaConceitoAluno = new NotasConceitosAlunoListaoRetornoDto()
                {
                    Id = aluno.CodigoAluno,
                    Nome = aluno.NomeValido(),
                    NumeroChamada = aluno.NumeroAlunoChamada
                };

                var notasAvaliacoes = new List<NotasConceitosNotaAvaliacaoRetornoDto>();
                foreach (var atividadeAvaliativa in atividadesAvaliativasdoBimestre)
                {
                    var notaDoAluno = ObterNotaParaVisualizacao(notas, aluno, atividadeAvaliativa);
                    var notaParaVisualizar = string.Empty;

                    if (notaDoAluno != null)
                    {
                        notaParaVisualizar = notaDoAluno.ObterNota();

                        if (!dataUltimaNotaConceitoInserida.HasValue || notaDoAluno.CriadoEm > dataUltimaNotaConceitoInserida.Value)
                        {
                            usuarioRfUltimaNotaConceitoInserida = $"{notaDoAluno.CriadoPor}({notaDoAluno.CriadoRF})";
                            dataUltimaNotaConceitoInserida = notaDoAluno.CriadoEm;
                            nomeAvaliacaoAuditoriaInclusao = atividadeAvaliativa.NomeAvaliacao;
                        }

                        if (notaDoAluno.AlteradoEm.HasValue && (!dataUltimaNotaConceitoAlterada.HasValue || notaDoAluno.AlteradoEm.Value > dataUltimaNotaConceitoAlterada.Value))
                        {
                            usuarioRfUltimaNotaConceitoAlterada = $"{notaDoAluno.AlteradoPor}({notaDoAluno.AlteradoRF})";
                            dataUltimaNotaConceitoAlterada = notaDoAluno.AlteradoEm;
                            nomeAvaliacaoAuditoriaAlteracao = atividadeAvaliativa.NomeAvaliacao;
                        }
                    }

                    var ausente = ausenciasDasAtividadesAvaliativas
                        .Any(a => a.AlunoCodigo == aluno.CodigoAluno && a.AulaData.Date == atividadeAvaliativa.DataAvaliacao.Date);

                    var notaAvaliacao = new NotasConceitosNotaAvaliacaoRetornoDto()
                    {
                        AtividadeAvaliativaId = atividadeAvaliativa.Id,
                        NotaConceito = notaParaVisualizar,
                        Ausente = ausente,
                        PodeEditar = aluno.EstaAtivo(atividadeAvaliativa.DataAvaliacao) ||
                                     (aluno.EstaInativo(atividadeAvaliativa.DataAvaliacao) && atividadeAvaliativa.DataAvaliacao.Date <= aluno.DataSituacao.Date),
                        StatusGsa = notaDoAluno?.StatusGsa
                    };

                    notasAvaliacoes.Add(notaAvaliacao);
                }

                notaConceitoAluno.PodeEditar =
                        (notasAvaliacoes.Any(na => na.PodeEditar) || (atividadesAvaliativaEBimestres is null || !atividadesAvaliativaEBimestres.Any())) &&
                        (aluno.Inativo == false || (aluno.Inativo && aluno.DataSituacao >= periodoFechamentoBimestre?.InicioDoFechamento.Date));


                var fechamentoTurma = (from ft in fechamentosNotasDaTurma
                                       from fa in ft.FechamentoAlunos
                                       where fa.AlunoCodigo.Equals(aluno.CodigoAluno)
                                       select ft).FirstOrDefault();

                // Carrega Notas do Bimestre
                if (fechamentoTurma != null)
                {
                    retorno.AuditoriaBimestreInserido = $"Nota final do bimestre inserida por {fechamentoTurma.CriadoPor}({fechamentoTurma.CriadoRF}) em {fechamentoTurma.CriadoEm.ToString("dd/MM/yyyy")}, às {fechamentoTurma.CriadoEm.ToString("HH:mm")}.";

                    var notasConceitoBimestre = fechamentoTurma.FechamentoAlunos
                        .Where(a => a.AlunoCodigo == aluno.CodigoAluno)
                        .SelectMany(a => a.FechamentoNotas).ToList();

                    if (notasConceitoBimestre.Any())
                    {
                        var dadosAuditoriaAlteracaoBimestre = notasConceitoBimestre
                            .Where(o => o.AlteradoEm.HasValue)
                            .ToList();

                        if (dadosAuditoriaAlteracaoBimestre.Any())
                        {
                            var ultimoDadoDeAuditoria = dadosAuditoriaAlteracaoBimestre
                                                                .OrderByDescending(nc => nc.AlteradoEm)
                                                                .Select(nc => new
                                                                {
                                                                    AlteradoPor = nc.AlteradoRF.Equals(0) ? nc.CriadoPor : nc.AlteradoPor,
                                                                    AlteradoRf = nc.AlteradoRF.Equals(0) ? nc.CriadoRF : nc.AlteradoRF,
                                                                    AlteradoEm = nc.AlteradoRF.Equals(0) ? nc.CriadoEm : nc.AlteradoEm.Value,
                                                                })
                                                                .First();

                            retorno.AuditoriaBimestreAlterado = $"Nota final do bimestre alterada por {(ultimoDadoDeAuditoria.AlteradoPor)}({ultimoDadoDeAuditoria.AlteradoRf}) em {ultimoDadoDeAuditoria.AlteradoEm.ToString("dd/MM/yyyy")}, às {ultimoDadoDeAuditoria.AlteradoEm.ToString("HH:mm")}.";
                        }
                    }


                    if (componenteReferencia.Regencia)
                    {
                        // Regencia carrega disciplinas mesmo sem nota de fechamento
                        foreach (var disciplinaRegencia in disciplinasRegencia)
                        {
                            var nota = new FechamentoNotaListaoRetornoDto()
                            {
                                DisciplinaId = disciplinaRegencia.CodigoComponenteCurricular,
                                Disciplina = disciplinaRegencia.Nome,
                            };
                            var notaRegencia = notasConceitoBimestre?.FirstOrDefault(c => c.DisciplinaId == disciplinaRegencia.CodigoComponenteCurricular);
                            if (notaRegencia != null)
                            {
                                nota.NotaConceito = (notaRegencia.ConceitoId.HasValue ? notaRegencia.ConceitoId.Value : notaRegencia.Nota);
                                nota.EhConceito = notaRegencia.ConceitoId.HasValue;
                            }

                            await VerificaNotaEmAprovacao(aluno.CodigoAluno, fechamentoTurma.FechamentoTurmaId, nota.DisciplinaId, nota);

                            notaConceitoAluno.NotasBimestre.Add(nota);
                        }
                    }
                    else
                    {

                        foreach (var notaConceitoBimestre in notasConceitoBimestre)
                        {
                            var nota = new FechamentoNotaListaoRetornoDto()
                            {
                                DisciplinaId = notaConceitoBimestre.DisciplinaId,
                                Disciplina = componenteReferencia.Nome,
                                NotaConceito = notaConceitoBimestre.ConceitoId.HasValue ?
                                   notaConceitoBimestre.ConceitoId.Value :
                                   notaConceitoBimestre.Nota,
                                EhConceito = notaConceitoBimestre.ConceitoId.HasValue
                            };

                            await VerificaNotaEmAprovacao(aluno.CodigoAluno, fechamentoTurma.FechamentoTurmaId, nota.DisciplinaId, nota);

                            notaConceitoAluno.NotasBimestre.Add(nota);
                        }

                    }
                }
                else if (componenteReferencia.Regencia && componenteReferencia != null)
                {
                    // Regencia carrega disciplinas mesmo sem nota de fechamento
                    foreach (var disciplinaRegencia in disciplinasRegencia)
                    {
                        notaConceitoAluno.NotasBimestre.Add(new FechamentoNotaListaoRetornoDto()
                        {
                            DisciplinaId = disciplinaRegencia.CodigoComponenteCurricular,
                            Disciplina = disciplinaRegencia.Nome,
                        });
                    }
                }

                listaAlunosDoBimestre.Add(notaConceitoAluno);
            }

            foreach (var avaliacao in atividadesAvaliativasdoBimestre)
            {
                var avaliacaoDoBimestre = new NotasConceitosAvaliacaoListaoRetornoDto()
                {
                    Id = avaliacao.Id,
                    Data = avaliacao.DataAvaliacao,
                    Descricao = avaliacao.DescricaoAvaliacao,
                    Nome = avaliacao.NomeAvaliacao,
                    EhCJ = avaliacao.EhCj
                };

                avaliacaoDoBimestre.EhInterdisciplinar = avaliacao.Categoria.Equals(CategoriaAtividadeAvaliativa.Interdisciplinar);

                if (componenteReferencia.Regencia)
                {
                    var atividadeDisciplinas = await ObterDisciplinasAtividadeAvaliativa(avaliacao.Id, avaliacao.EhRegencia);
                    var idsDisciplinas = atividadeDisciplinas?.Select(a => long.Parse(a.DisciplinaId)).ToArray();
                    IEnumerable<DisciplinaDto> disciplinas;
                    if (idsDisciplinas != null && idsDisciplinas.Any())
                        disciplinas = await ObterDisciplinasPorIds(idsDisciplinas);
                    else
                    {
                        disciplinas = await consultasDisciplina
                            .ObterComponentesCurricularesPorProfessorETurmaParaPlanejamento(componenteReferencia.CodigoComponenteCurricular,
                                                                                            turmaCompleta.CodigoTurma,
                                                                                            turmaCompleta.TipoTurma == TipoTurma.Programa,
                                                                                            componenteReferencia.Regencia);
                    }
                    var nomesDisciplinas = disciplinas?.Select(d => d.Nome).ToArray();
                    avaliacaoDoBimestre.Disciplinas = nomesDisciplinas;
                }

                bimestreParaAdicionar.Avaliacoes.Add(avaliacaoDoBimestre);

                if (atividadeAvaliativaParaObterTipoNota == null)
                    atividadeAvaliativaParaObterTipoNota = avaliacao;
            }

            return retorno;
        }

        public async Task<IEnumerable<AtividadeAvaliativaDisciplina>> ObterDisciplinasAtividadeAvaliativa(long avaliacao_id, bool ehRegencia)
        {
            return await mediator.Send(new ObterDisciplinasAtividadeAvaliativaQuery(avaliacao_id, ehRegencia));
        }
        public async Task<IEnumerable<DisciplinaDto>> ObterDisciplinasPorIds(long[] ids)
        {
            return await mediator.Send(new ObterDisciplinasPorIdsQuery(ids));
        }

        private async Task VerificaNotaEmAprovacao(string codigoAluno, long turmaFechamentoId, long disciplinaId, FechamentoNotaListaoRetornoDto notasConceito)
        {
            double nota = await mediator.Send(new ObterNotaEmAprovacaoQuery(codigoAluno, turmaFechamentoId, disciplinaId));

            if (nota > 0)
            {
                notasConceito.NotaConceito = nota;
                notasConceito.EmAprovacao = true;
            }
            else
            {
                notasConceito.EmAprovacao = false;
            }
        }

        private static NotaConceito ObterNotaParaVisualizacao(IEnumerable<NotaConceito> notas, AlunoPorTurmaResposta aluno, AtividadeAvaliativa atividadeAvaliativa)
        {
            var notaDoAluno = notas.FirstOrDefault(a => a.AlunoId == aluno.CodigoAluno && a.AtividadeAvaliativaID == atividadeAvaliativa.Id);

            return notaDoAluno;
        }
        private IEnumerable<DisciplinaResposta> MapearParaDto(IEnumerable<ComponenteCurricularEol> disciplinasRegenciaEol)
        {
            foreach (var disciplina in disciplinasRegenciaEol)
            {
                yield return new DisciplinaResposta()
                {
                    CodigoComponenteCurricular = disciplina.Codigo,
                    Compartilhada = disciplina.Compartilhada,
                    CodigoComponenteCurricularPai = disciplina.CodigoComponenteCurricularPai,
                    Nome = disciplina.Descricao,
                    Regencia = disciplina.Regencia,
                    RegistroFrequencia = disciplina.RegistraFrequencia,
                    TerritorioSaber = disciplina.TerritorioSaber,
                    LancaNota = disciplina.LancaNota,
                    BaseNacional = disciplina.BaseNacional,
                    GrupoMatriz = new Integracoes.Respostas.GrupoMatriz { Id = disciplina.GrupoMatriz.Id, Nome = disciplina.GrupoMatriz.Nome }
                };
            }
        }
        private IEnumerable<DisciplinaResposta> MapearParaDto(IEnumerable<DisciplinaDto> disciplinasRegenciaCJ)
        {
            foreach (var disciplina in disciplinasRegenciaCJ)
            {
                yield return new DisciplinaResposta()
                {
                    CodigoComponenteCurricular = disciplina.CodigoComponenteCurricular,
                    Compartilhada = disciplina.Compartilhada,
                    CodigoComponenteCurricularPai = disciplina.CdComponenteCurricularPai,
                    Nome = disciplina.Nome,
                    Regencia = disciplina.Regencia,
                    RegistroFrequencia = disciplina.RegistraFrequencia,
                    TerritorioSaber = disciplina.TerritorioSaber,
                    LancaNota = disciplina.LancaNota,
                    GrupoMatriz = new Integracoes.Respostas.GrupoMatriz { Id = disciplina.GrupoMatrizId, Nome = disciplina.GrupoMatrizNome }
                };
            }
        }
        private static List<long> ObterComponentesCurricularesParaConsulta(long disciplinaId, List<DisciplinaDto> disciplinasDoProfessorLogado)
        {
            var disciplinasFilha = disciplinasDoProfessorLogado.Where(d => d.CdComponenteCurricularPai == disciplinaId).ToList();
            var componentesCurriculares = new List<long>();

            if (disciplinasFilha.Any())
            {
                componentesCurriculares.AddRange(disciplinasFilha.Select(a => a.CodigoComponenteCurricular).ToList());
            }
            else
            {
                componentesCurriculares.Add(disciplinaId);
            }

            return componentesCurriculares;
        }
    }
}
