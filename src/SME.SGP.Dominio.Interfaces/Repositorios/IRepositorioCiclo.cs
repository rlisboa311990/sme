﻿using SME.SGP.Infra;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SME.SGP.Dominio.Interfaces
{
    public interface IRepositorioCiclo : IRepositorioBase<Ciclo>
    {
        CicloDto ObterCicloPorAno(int ano);

        CicloDto ObterCicloPorAnoModalidade(string ano, Modalidade modalidade);

        IEnumerable<CicloDto> ObterCiclosPorAnoModalidade(FiltroCicloDto filtroCicloDto);

        Task<IEnumerable<RetornoCicloDto>> ObterCiclosPorAnoModalidadeECodigoUe(FiltroCicloPorModalidadeECodigoUeDto filtroCicloPorModalidadeECodigoUeDto);
    }
}