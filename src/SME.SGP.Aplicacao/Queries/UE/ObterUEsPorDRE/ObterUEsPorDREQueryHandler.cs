﻿using MediatR;
using SME.SGP.Dominio.Interfaces;
using SME.SGP.Dto;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SME.SGP.Aplicacao.Queries.UE.ObterUEsPorDRE
{
    public class ObterUEsPorDREQueryHandler : IRequestHandler<ObterUEsPorDREQuery, IEnumerable<AbrangenciaUeRetorno>>
    {
        private readonly IMediator mediator;
        private readonly IRepositorioAbrangencia repositorioAbrangencia;

        public ObterUEsPorDREQueryHandler(IMediator mediator, IRepositorioAbrangencia repositorioAbrangencia)
        {
            this.mediator = mediator ?? throw new System.ArgumentNullException(nameof(mediator));
            this.repositorioAbrangencia = repositorioAbrangencia ?? throw new System.ArgumentNullException(nameof(repositorioAbrangencia));
        }

        public async Task<IEnumerable<AbrangenciaUeRetorno>> Handle(ObterUEsPorDREQuery request, CancellationToken cancellationToken)
        {
            var anoNovosTiposUE = request.ConsideraNovasUEs ? request.AnoLetivo + 1 : request.AnoLetivo;
            var parametroNovosTiposUE = await mediator.Send(new ObterNovosTiposUEPorAnoQuery(anoNovosTiposUE));
            var novosTiposUE = parametroNovosTiposUE?.Split(',').Select(a => int.Parse(a)).ToArray();

            return (await repositorioAbrangencia.ObterUes(request.CodigoDre, request.Login, request.Perfil, request.Modalidade, request.Periodo, request.ConsideraHistorico, request.AnoLetivo, novosTiposUE)).OrderBy(c => c.Nome);
        }
    }
}
