﻿using MediatR;
using SME.SGP.Aplicacao;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SME.SGP.TesteIntegracao.ServicosFakes
{
    public class ObterUsuarioPossuiPermissaoNaTurmaEDisciplinaQueryHandlerSemPermissaoFake : IRequestHandler<ObterUsuarioPossuiPermissaoNaTurmaEDisciplinaQuery, bool>
    {
        private readonly IMediator mediator;

        public ObterUsuarioPossuiPermissaoNaTurmaEDisciplinaQueryHandlerSemPermissaoFake(IMediator mediator)
        {
            this.mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        }

        public async Task<bool> Handle(ObterUsuarioPossuiPermissaoNaTurmaEDisciplinaQuery request, CancellationToken cancellationToken)
        {
            return false;
        }
    }
}
