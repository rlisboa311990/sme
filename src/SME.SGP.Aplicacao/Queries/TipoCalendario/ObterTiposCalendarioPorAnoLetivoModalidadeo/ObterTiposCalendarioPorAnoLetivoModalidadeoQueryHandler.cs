﻿using MediatR;
using SME.SGP.Dominio;
using SME.SGP.Dominio.Interfaces;
using SME.SGP.Infra;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace SME.SGP.Aplicacao
{
    public class ObterTiposCalendarioPorAnoLetivoModalidadeoQueryHandler : IRequestHandler<ObterTiposCalendarioPorAnoLetivoModalidadeoQuery, IEnumerable<TipoCalendario>>
    {
        private IRepositorioTipoCalendario repositorioTipoCalendario;

        public ObterTiposCalendarioPorAnoLetivoModalidadeoQueryHandler(IRepositorioTipoCalendario repositorioTipoCalendario)
        {
            this.repositorioTipoCalendario = repositorioTipoCalendario ?? throw new ArgumentNullException(nameof(repositorioTipoCalendario));
        }
        public async Task<IEnumerable<TipoCalendario>> Handle(ObterTiposCalendarioPorAnoLetivoModalidadeoQuery request, CancellationToken cancellationToken)
        {
            return await repositorioTipoCalendario.ListarPorAnoLetivoEModalidades(request.AnoLetivo, new int[] { (int)(request.Modalidade.ObterModalidadeTipoCalendario()) });
        }
    }
}
