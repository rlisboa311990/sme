﻿using MediatR;
using SME.SGP.Aplicacao.Interfaces;
using SME.SGP.Dominio;
using SME.SGP.Dominio.Enumerados;
using SME.SGP.Infra;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SME.SGP.Aplicacao
{
    public class ExecutarConsolidacaoTurmaConselhoClasseAlunoUseCase : AbstractUseCase, IExecutarConsolidacaoTurmaConselhoClasseAlunoUseCase
    {
        private readonly IRepositorioConselhoClasseConsolidado repositorioConselhoClasseConsolidado;

        public ExecutarConsolidacaoTurmaConselhoClasseAlunoUseCase(IMediator mediator, IRepositorioConselhoClasseConsolidado repositorioConselhoClasseConsolidado) : base(mediator)
        {
            this.repositorioConselhoClasseConsolidado = repositorioConselhoClasseConsolidado ?? throw new System.ArgumentNullException(nameof(repositorioConselhoClasseConsolidado));
        }

        public async Task<bool> Executar(MensagemRabbit mensagemRabbit)
        {


            var filtro = mensagemRabbit
                        .ObterObjetoMensagem<MensagemConsolidacaoConselhoClasseAlunoDto>();

            if (filtro == null)
            {
                await mediator.Send(new SalvarLogViaRabbitCommand($"Não foi possível iniciar a consolidação do conselho de clase da turma -> aluno. O id da turma bimestre aluno não foram informados", LogNivel.Critico, LogContexto.ConselhoClasse));
                return false;
            }

            SituacaoConselhoClasse statusNovo = SituacaoConselhoClasse.NaoIniciado;

            var consolidadoTurmaAluno = await repositorioConselhoClasseConsolidado
                    .ObterConselhoClasseConsolidadoPorTurmaBimestreAlunoAsync(filtro.TurmaId, filtro.Bimestre, filtro.AlunoCodigo);

            if (consolidadoTurmaAluno == null)
            {
                consolidadoTurmaAluno = new ConselhoClasseConsolidadoTurmaAluno
                {
                    AlunoCodigo = filtro.AlunoCodigo,
                    //Bimestre = filtro.Bimestre,
                    TurmaId = filtro.TurmaId,
                    Status = statusNovo
                };
            }

            if (!filtro.Inativo)
            {
                var componentesDoAluno = await mediator
                    .Send(new ObterComponentesParaFechamentoAcompanhamentoCCAlunoQuery(filtro.AlunoCodigo, filtro.Bimestre, filtro.TurmaId));

                if (componentesDoAluno != null && componentesDoAluno.Any())
                {
                    var turma = await mediator.Send(new ObterTurmaPorIdQuery(filtro.TurmaId));

                    if (filtro.Bimestre == 0)
                    {
                        var fechamento = await mediator.Send(new ObterFechamentoPorTurmaPeriodoQuery() { TurmaId = filtro.TurmaId });
                        var conselhoClasse = await mediator.Send(new ObterConselhoClassePorFechamentoIdQuery(fechamento.Id));
                        var conselhoClasseAluno = await mediator.Send(new ObterConselhoClasseAlunoPorAlunoCodigoConselhoIdQuery(conselhoClasse.Id, filtro.AlunoCodigo));
                        consolidadoTurmaAluno.ParecerConclusivoId = conselhoClasseAluno?.ConselhoClasseParecerId;
                    }

                    var turmasCodigos = new string[] { };
                    var turmasitinerarioEnsinoMedio = await mediator.Send(new ObterTurmaItinerarioEnsinoMedioQuery());

                    if (turma.DeveVerificarRegraRegulares() || turmasitinerarioEnsinoMedio.Any(a => a.Id == (int)turma.TipoTurma))
                    {
                        var turmasCodigosParaConsulta = new List<int>() { (int)turma.TipoTurma };
                        turmasCodigosParaConsulta.AddRange(turma.ObterTiposRegularesDiferentes());
                        turmasCodigosParaConsulta.AddRange(turmasitinerarioEnsinoMedio.Select(s => s.Id));
                        turmasCodigos = await mediator.Send(new ObterTurmaCodigosAlunoPorAnoLetivoAlunoTipoTurmaQuery(turma.AnoLetivo, filtro.AlunoCodigo, turmasCodigosParaConsulta));
                    }

                    if (turmasCodigos.Length == 0)
                        turmasCodigos = new string[1] { turma.CodigoTurma };

                    var componentesComNotaFechamentoOuConselho = await mediator
                        .Send(new ObterComponentesComNotaDeFechamentoOuConselhoQuery(turma.AnoLetivo, filtro.TurmaId, filtro.Bimestre, filtro.AlunoCodigo));

                    var componentesDaTurmaEol = await mediator
                        .Send(new ObterComponentesCurricularesEOLPorTurmasCodigoQuery(turmasCodigos));

                    var possuiComponentesSemNotaConceito = componentesDaTurmaEol
                        .Where(ct => ct.LancaNota && !ct.TerritorioSaber)
                        .Select(ct => ct.Codigo)
                        .Except(componentesComNotaFechamentoOuConselho.Select(cn => cn.Codigo))
                        .Any();

                    if (possuiComponentesSemNotaConceito)
                        statusNovo = SituacaoConselhoClasse.EmAndamento;
                    else
                        statusNovo = SituacaoConselhoClasse.Concluido;
                }

                if (consolidadoTurmaAluno.ParecerConclusivoId != null)
                    statusNovo = SituacaoConselhoClasse.Concluido;
            }

            consolidadoTurmaAluno.Status = statusNovo;

            consolidadoTurmaAluno.DataAtualizacao = DateTime.Now;

            //if (filtro.ComponenteCurricularId.HasValue)//Quando parecer conclusivo, não altera a nota, atualiza somente o parecerId
                //consolidadoTurmaAluno.ComponenteCurricularId = filtro.ComponenteCurricularId;

            //if (filtro.Nota.HasValue) //Quando parecer conclusivo, não altera a nota, atualiza somente o parecerId
            //    consolidadoTurmaAluno.Nota = filtro.Nota;

            //if (filtro.ConceitoId.HasValue)//Quando parecer conclusivo, não altera a nota, atualiza somente o parecerId
            //    consolidadoTurmaAluno.ConceitoId = filtro.ConceitoId;

            return (await repositorioConselhoClasseConsolidado.SalvarAsync(consolidadoTurmaAluno)) > 0;
            
        }
    }
}
