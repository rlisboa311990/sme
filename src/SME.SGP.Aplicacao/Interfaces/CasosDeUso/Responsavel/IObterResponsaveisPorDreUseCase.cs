﻿using SME.SGP.Infra;
using System.Collections.Generic;

namespace SME.SGP.Aplicacao.Interfaces
{
    public interface IObterResponsaveisPorDreUseCase : IUseCase<ObterResponsaveisPorDreDto, IEnumerable<SupervisorDto>>
    {
    }
}
