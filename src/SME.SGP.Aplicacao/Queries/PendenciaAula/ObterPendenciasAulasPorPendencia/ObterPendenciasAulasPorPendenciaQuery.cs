﻿using MediatR;
using SME.SGP.Dominio;
using SME.SGP.Infra;
using System.Collections.Generic;

namespace SME.SGP.Aplicacao
{
    public class ObterPendenciasAulasPorPendenciaQuery : IRequest<IEnumerable<PendenciaAulaDto>>
    {
        public ObterPendenciasAulasPorPendenciaQuery(long pendenciaId)
        {
            PendenciaId = pendenciaId;
        }

        public long PendenciaId { get; set; }
    }
}
