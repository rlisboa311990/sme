﻿using SME.SGP.Infra;
using System.Threading.Tasks;

namespace SME.SGP.Aplicacao
{
    public interface IComandosConselhoClasse
    {
        Task<string> Alterar(long conselhoClasseId, long fechmamentoTurmaId);
    }
}
