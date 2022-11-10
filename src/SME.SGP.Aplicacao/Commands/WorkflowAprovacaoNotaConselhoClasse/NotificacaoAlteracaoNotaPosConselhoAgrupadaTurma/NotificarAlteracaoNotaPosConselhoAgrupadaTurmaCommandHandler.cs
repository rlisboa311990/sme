﻿using MediatR;
using SME.SGP.Dominio;
using SME.SGP.Dominio.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SME.SGP.Aplicacao
{
    public class NotificarAlteracaoNotaPosConselhoAgrupadaTurmaCommandHandler : AprovacaoNotaConselhoCommandBase<NotificarAlteracaoNotaPosConselhoAgrupadaTurmaCommand>
    {
        private readonly IRepositorioWFAprovacaoNotaConselho repositorioWFAprovacaoNotaConselho;

        public NotificarAlteracaoNotaPosConselhoAgrupadaTurmaCommandHandler(IMediator mediator, IRepositorioWFAprovacaoNotaConselho repositorioWFAprovacaoNotaConselho) 
                    : base(mediator)
        {
            this.repositorioWFAprovacaoNotaConselho = repositorioWFAprovacaoNotaConselho ?? throw new ArgumentNullException(nameof(repositorioWFAprovacaoNotaConselho));
        }

        protected override async Task Handle(NotificarAlteracaoNotaPosConselhoAgrupadaTurmaCommand request, CancellationToken cancellationToken)
        {
            await IniciarAprovacao(await repositorioWFAprovacaoNotaConselho.ObterNotasAguardandoAprovacaoSemWorkflow());

            var agrupamentoPorTurma = WFAprovacoes.GroupBy(wf => new { wf.ConselhoClasseNota.ConselhoClasseAluno.ConselhoClasse.FechamentoTurma.Turma, 
                                                                       wf.ConselhoClasseNota.ConselhoClasseAluno.ConselhoClasse.FechamentoTurma.PeriodoEscolar.Bimestre } );

            foreach (var grupoTurma in agrupamentoPorTurma)
            {
                var idAprovacao = await EnviarNotificacao(grupoTurma.Key.Turma, grupoTurma.ToList());

                await ExecuteAlteracoesDasAprovacoes(grupoTurma.ToList(), idAprovacao);
            }
        }

        protected override string ObterTexto(Ue ue, Turma turma, PeriodoEscolar periodoEscolar)
        {
            return $@"A alteração de notas/conceitos pós-conselho do bimestre { periodoEscolar.Bimestre } 
                      de { turma.AnoLetivo } da turma { turma.NomeFiltro } da { ue.Nome } ({ ue.Dre.Abreviacao }) foram alteradas.";
        }

        protected override string ObterTitulo(Ue ue, Turma turma)
        {
            return $@"Alteração em nota/conceito pós-conselho - { ue.Nome } ({ ue.Dre.Abreviacao }) - { turma.NomeFiltro } (ano anterior)";
        }

        private async Task ExecuteAlteracoesDasAprovacoes(List<WFAprovacaoNotaConselho> aprovacoesPorTurma, long idAprovacao)
        {
            foreach(var aprovacao in aprovacoesPorTurma)
            {
                aprovacao.WfAprovacaoId = idAprovacao;

                await repositorioWFAprovacaoNotaConselho.SalvarAsync(aprovacao);
            }
        }

        private async Task<long> EnviarNotificacao(Turma turma, List<WFAprovacaoNotaConselho> aprovacoesPorTurma)
        {
            var ue = Ues.Find(ue => ue.Id == turma.UeId);
            var titulo = ObterTitulo(ue, turma);
            var mensagem = ObterMensagem(ue, turma, aprovacoesPorTurma);
            var conselhoClasseId = aprovacoesPorTurma.FirstOrDefault().ConselhoClasseNota.ConselhoClasseAluno.ConselhoClasseId;

            return await mediator.Send(new EnviarNotificacaoCommand(
                                                                    titulo,
                                                                    mensagem,
                                                                    NotificacaoCategoria.Workflow_Aprovacao,
                                                                    NotificacaoTipo.Fechamento,
                                                                    new Cargo[] { Cargo.CP, Cargo.Supervisor },
                                                                    ue.Dre.CodigoDre,
                                                                    ue.CodigoUe,
                                                                    turma.CodigoTurma,
                                                                    WorkflowAprovacaoTipo.AlteracaoNotaConselho,
                                                                    conselhoClasseId)); 
        }
    }
}
