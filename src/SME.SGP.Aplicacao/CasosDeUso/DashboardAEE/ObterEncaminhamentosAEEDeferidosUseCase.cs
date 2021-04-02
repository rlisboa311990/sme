﻿using MediatR;
using SME.SGP.Infra.Dtos;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace SME.SGP.Aplicacao
{
    public class ObterEncaminhamentosAEEDeferidosUseCase : AbstractUseCase, IObterEncaminhamentosAEEDeferidosUseCase
    {
        public ObterEncaminhamentosAEEDeferidosUseCase(IMediator mediator) : base(mediator)
        {
        }

        public async Task<IEnumerable<AEETurmaDto>> Executar(FiltroDashboardAEEDto param)
        {
            if (param.Ano == 0)
                param.Ano = DateTime.Now.Year;
            return await mediator.Send(new ObterEncaminhamentosAEEDeferidosQuery(param.Ano, param.DreId, param.UeId));
        }
    }
}
