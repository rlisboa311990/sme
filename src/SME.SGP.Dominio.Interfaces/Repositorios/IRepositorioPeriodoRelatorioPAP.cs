﻿using SME.SGP.Infra;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SME.SGP.Dominio.Interfaces
{
    public interface IRepositorioPeriodoRelatorioPAP : IRepositorioBase<PeriodoRelatorioPAP>
    {
        Task<IEnumerable<PeriodosPAPDto>> ObterPeriodos(int anoLetivo);
        Task<PeriodoRelatorioPAP> ObterComPeriodosEscolares(long id);
    }
}
