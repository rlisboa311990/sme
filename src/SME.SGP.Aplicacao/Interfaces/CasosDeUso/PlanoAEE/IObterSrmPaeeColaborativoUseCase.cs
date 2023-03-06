﻿using System.Collections.Generic;
using SME.SGP.Infra;

namespace SME.SGP.Aplicacao
{
    public interface IObterSrmPaeeColaborativoUseCase : IUseCase<FiltroSrmPaeeColaborativoDto, IEnumerable<SrmPaeeColaborativoSgpDto>>
    {
    
    }
}