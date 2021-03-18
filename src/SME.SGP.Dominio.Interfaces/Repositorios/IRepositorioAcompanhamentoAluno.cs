﻿using SME.SGP.Infra;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SME.SGP.Dominio.Interfaces
{
    public interface IRepositorioAcompanhamentoAluno : IRepositorioBase<AcompanhamentoAluno>
    {
        Task<IEnumerable<AcompanhamentoAlunoDto>> ObterAcompanhamentoPorTurmaAlunoESemestre(string turmaCodigo, string alunoCodigo, int semestre);
    }
}
