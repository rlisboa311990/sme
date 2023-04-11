﻿using System.Collections.Generic;
using System.Threading.Tasks;
using SME.SGP.Dominio.Entidades;
using SME.SGP.Infra;

namespace SME.SGP.Dominio.Interfaces
{
    public interface IRepositorioCompensacaoAusenciaAlunoAula : IRepositorioBase<CompensacaoAusenciaAlunoAula>
    {
        Task<bool> ExclusaoLogicaCompensacaoAusenciaAlunoAulaPorIds(long[] ids);
        Task<IEnumerable<CompensacaoAusenciaAlunoAula>> ObterPorCompensacaoIdAsync(long compensacaoId);
    }
}
