﻿using SME.SGP.Infra;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SME.SGP.Dominio.Interfaces
{
    public interface IRepositorioConsolidacaoFrequenciaTurma
    {
        Task<IEnumerable<FrequenciaGlobalPorAnoDto>> ObterFrequenciaGlobalPorAnoAsync(int anoLetivo, long dreId, long ueId, Modalidade? modalidade, int semestre);
        Task<IEnumerable<FrequenciaGlobalPorDreDto>> ObterFrequenciaGlobalPorDreAsync(int anoLetivo);
        Task<bool> ExisteConsolidacaoFrequenciaTurmaPorAno(int ano);
        Task<long> Inserir(ConsolidacaoFrequenciaTurma consolidacao);
        Task LimparConsolidacaoFrequenciasTurmasPorAno(int ano);
    }
}
