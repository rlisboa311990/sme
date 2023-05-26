﻿using System.Threading.Tasks;
using MediatR;
using SME.SGP.Dominio;
using SME.SGP.Infra;

namespace SME.SGP.Aplicacao
{
    public class ExecutarInserirConsolidadoAtendimentoProfissionalEncaminhamentoNAAPAUseCase : AbstractUseCase, IExecutarInserirConsolidadoAtendimentoProfissionalEncaminhamentoNAAPAUseCase

    {
        public ExecutarInserirConsolidadoAtendimentoProfissionalEncaminhamentoNAAPAUseCase(IMediator mediator) : base(mediator)
        {
        }

        public async Task<bool> Executar(MensagemRabbit param)
        {
            var consolidado = param.ObterObjetoMensagem<ConsolidadoAtendimentoNAAPA>();
            var profissionalAtendimentoConsolidado = await mediator.Send(new ObterAtendimentoProfissionalEncaminhamentosNAAPAConsolidadoQuery(consolidado.UeId, consolidado.Mes, consolidado.AnoLetivo, consolidado.Profissional));
            if (profissionalAtendimentoConsolidado != null)
                consolidado.Id = profissionalAtendimentoConsolidado.Id;
            await mediator.Send(new SalvarConsolidadoAtendimentoProfissionalEncaminhamentoNAAPACommand(consolidado));
            return true;
        }
    }
}