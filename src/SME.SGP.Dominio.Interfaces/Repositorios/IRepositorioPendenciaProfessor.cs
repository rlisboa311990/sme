﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using SME.SGP.Infra;

namespace SME.SGP.Dominio.Interfaces
{
    public interface IRepositorioPendenciaProfessor
    {
        Task<long> Inserir(long pendenciaId, long turmaId, long componenteCurricularId, string professorRf, long? periodoEscolarId);
        Task<bool> ExistePendenciaProfessorPorTurmaEComponente(long turmaId, long componenteCurricularId, long? periodoEscolarId, string professorRf, TipoPendencia tipoPendencia);
        Task<long> ObterPendenciaIdPorTurma(long turmaId, TipoPendencia tipoPendencia);
        Task<IEnumerable<PendenciaProfessorDto>> ObterPendenciasPorPendenciaId(long pendenciaId);
        Task<IEnumerable<PendenciaProfessor>> ObterPendenciasProfessorPorTurmaEComponente(string turmaCodigo, long[] componentesCurriculares, long periodoEscolarId, TipoPendencia tipoPendencia);
        Task Remover(PendenciaProfessor pendenciaProfessor);
        Task<bool> ExistePendenciaProfessorPorPendenciaId(long pendenciaId);
    }
}
