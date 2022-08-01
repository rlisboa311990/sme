﻿using MediatR;
using SME.SGP.Dominio;
using SME.SGP.Dominio.Interfaces;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SME.SGP.Aplicacao
{
    public class ObterUeComDrePorCodigoQueryHandler : IRequestHandler<ObterUeComDrePorCodigoQuery, Ue>
    {
        private readonly IRepositorioUeConsulta repositorioUe;
        private readonly IMediator mediator;

        public ObterUeComDrePorCodigoQueryHandler(IRepositorioUeConsulta repositorioUe,IMediator mediator)
        {
            this.repositorioUe = repositorioUe ?? throw new ArgumentNullException(nameof(repositorioUe));
            this.mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        }

        public async Task<Ue> Handle(ObterUeComDrePorCodigoQuery request, CancellationToken cancellationToken)
        {
            return await mediator.Send(new ObterCacheObjetoQuery<Ue>($"codigo-ue-${request.UeCodigo}", async () => await repositorioUe.ObterUeComDrePorCodigo(request.UeCodigo)), cancellationToken);
        }
    }
}