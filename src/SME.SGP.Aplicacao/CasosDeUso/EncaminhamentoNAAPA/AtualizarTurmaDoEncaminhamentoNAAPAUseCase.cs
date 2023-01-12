﻿using MediatR;
using SME.SGP.Dominio;
using SME.SGP.Dominio.Enumerados;
using SME.SGP.Infra;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace SME.SGP.Aplicacao
{
    public class AtualizarTurmaDoEncaminhamentoNAAPAUseCase : AbstractUseCase, IAtualizarTurmaDoEncaminhamentoNAAPAUseCase
    {
        public AtualizarTurmaDoEncaminhamentoNAAPAUseCase(IMediator mediator) : base(mediator)
        {
        }

        public async Task<bool> Executar(MensagemRabbit param)
        {
            var encaminhamento = param.ObterObjetoMensagem<EncaminhamentoNAAPADto>();
            var alunosEol = await mediator.Send(new ObterAlunosEolPorCodigosQuery(new[] { long.Parse(encaminhamento.AlunoCodigo) } ));
            var alunoTurma = alunosEol.Where(turma => turma.CodigoTipoTurma == (int)TipoTurma.Regular 
                                                      && turma.AnoLetivo <= DateTimeExtension.HorarioBrasilia().Year
                                                      && turma.DataSituacao.Date <= DateTimeExtension.HorarioBrasilia().Date)
                                      .OrderByDescending(turma => turma.AnoLetivo)
                                      .ThenByDescending(turma => turma.DataSituacao)
                                      .FirstOrDefault(); 

            if (alunoTurma != null) 
               await AtualizarTurmaDoEncaminhamento(encaminhamento, alunoTurma);

            return true;
        }

        private async Task AtualizarTurmaDoEncaminhamento(EncaminhamentoNAAPADto encaminhamento, TurmasDoAlunoDto alunoTurma)
        {
            var turmaId = await mediator.Send(new ObterTurmaIdPorCodigoQuery(alunoTurma.CodigoTurma.ToString()));

            if (turmaId != 0 && turmaId != encaminhamento.TurmaId)
            {
                await AtualizarEncaminhamento(encaminhamento.Id.GetValueOrDefault(), turmaId);
            }
        }

        private async Task AtualizarEncaminhamento(long encaminhamentoNAAPAId, long turmaId)
        {
            var encaminhamentoNAAPA = await mediator.Send(new ObterEncaminhamentoNAAPAComTurmaPorIdQuery(encaminhamentoNAAPAId));

            encaminhamentoNAAPA.TurmaId = turmaId;

            await mediator.Send(new SalvarEncaminhamentoNAAPACommand(encaminhamentoNAAPA));
        }
    }
}