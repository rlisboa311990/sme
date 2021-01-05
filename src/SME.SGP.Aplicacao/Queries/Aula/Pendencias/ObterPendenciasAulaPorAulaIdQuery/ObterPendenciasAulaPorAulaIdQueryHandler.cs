﻿using MediatR;
using SME.SGP.Dominio.Interfaces;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SME.SGP.Aplicacao
{
    public class ObterPendenciasAulaPorAulaIdQueryHandler : IRequestHandler<ObterPendenciasAulaPorAulaIdQuery, long[]>
    {
        private readonly IRepositorioPendenciaAula repositorioPendenciaAula;

        public ObterPendenciasAulaPorAulaIdQueryHandler(IRepositorioPendenciaAula repositorioPendenciaAula)
        {
            this.repositorioPendenciaAula = repositorioPendenciaAula ?? throw new ArgumentNullException(nameof(repositorioPendenciaAula));
        }
        public async Task<long[]> Handle(ObterPendenciasAulaPorAulaIdQuery request, CancellationToken cancellationToken)
        {
            var valor = await repositorioPendenciaAula.ListarPendenciasPorAulaId(request.AulaId);

            return valor;
        }
    }
}
