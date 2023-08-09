﻿using SME.SGP.Dominio;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SME.SGP.Aplicacao
{
    public interface IObterDresUseCase
    {
        Task<IEnumerable<Dre>> Executar();

    }
}
