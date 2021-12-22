﻿using SME.SGP.Infra;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SME.SGP.Dominio.Interfaces
{
    public interface IRepositorioRegistroFrequenciaAlunoConsulta
    {
        Task<IEnumerable<AusenciaPorDisciplinaAlunoDto>> ObterAusenciasAlunosPorAlunosETurmaIdEDataAula(DateTime dataAula, string turmaId, IEnumerable<string> codigoAlunos);
        Task<IEnumerable<FrequenciaAlunoSimplificadoDto>> ObterFrequenciasPorAulaId(long aulaId);
    }
}
