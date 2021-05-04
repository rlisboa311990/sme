﻿using SME.SGP.Dominio;
using SME.SGP.Infra;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SME.SGP.Aplicacao
{
    public interface IObterDadosDashboardAusenciasComJustificativaUseCase
    {
        Task<IEnumerable<GraficoAusenciasComJustificativaPorAnoDto>> Executar(int anoLetivo, long dreId, long ueId, Modalidade modalidade);
    }
}
