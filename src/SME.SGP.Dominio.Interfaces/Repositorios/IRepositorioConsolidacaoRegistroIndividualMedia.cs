﻿using SME.SGP.Infra;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SME.SGP.Dominio
{
    public interface IRepositorioConsolidacaoRegistroIndividualMedia
    {
        Task<IEnumerable<RegistroItineranciaMediaPorAnoDto>> ObterRegistrosItineranciasMediaPorAnoAsync(int anoLetivo, long dreId, Modalidade modalidade);
        Task<IEnumerable<GraficoBaseDto>> ObterRegistrosItineranciasMediaPorTurmaAsync(int anoLetivo, long dreId, long ueId, Modalidade modalidade);
    }
}
