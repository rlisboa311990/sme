﻿using SME.SGP.Infra;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SME.SGP.Dominio.Interfaces
{
    public interface IRepositorioFechamentoNotaConsulta : IRepositorioBase<FechamentoNota>
    {
        Task<IEnumerable<FechamentoNotaAlunoAprovacaoDto>> ObterPorFechamentosTurma(long[] fechamentosTurmaDisciplinaId);
        Task<IEnumerable<WfAprovacaoNotaFechamentoTurmaDto>> ObterNotasEmAprovacaoWf(long wfAprovacaoId);
        Task<IEnumerable<WfAprovacaoNotaFechamento>> ObterNotasEmAprovacaoPorFechamento(long fechamentoTurmaDisciplinaId);
        Task<IEnumerable<NotaConceitoBimestreComponenteDto>> ObterNotasAlunoBimestreAsync(long fechamentoTurmaId, string alunoCodigo);
        Task<IEnumerable<NotaConceitoBimestreComponenteDto>> ObterNotasFinaisAlunoAsync(string[] turmasCodigos, string alunoCodigo);
        Task<IEnumerable<NotaConceitoBimestreComponenteDto>> ObterNotasAlunoAnoAsync(string turmaCodigo, string alunoCodigo);
        Task<IEnumerable<AlunosFechamentoNotaDto>> ObterComNotaLancadaPorPeriodoEscolarUE(long ueId, long periodoEscolarId);
        Task<IEnumerable<FechamentoNotaAprovacaoDto>> ObterNotasEmAprovacaoPorIdsFechamento(IEnumerable<long> Ids);
        Task<IEnumerable<NotaConceitoComponenteBimestreAlunoDto>> ObterNotasPorTurmaIdAsync(long turmaId, DateTime? dataMatricula = null,
            DateTime? dataSituacao = null, int? anoLetivo = null);
    }
}