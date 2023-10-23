﻿using System.Threading.Tasks;

namespace SME.SGP.Metrica.Worker.Repositorios.Interfaces
{
    public interface IRepositorioSGP
    {
        // ConselhoClasse
        Task AtualizarConselhosClasseDuplicados(long fechamentoTurmaId, long ultimoId);
        Task ExcluirConselhosClasseDuplicados(long fechamentoTurmaId, long ultimoId);
        // ConselhoClasseAluno
        Task AtualizarNotasConselhoClasseAlunoDuplicado(long conselhoClasseId, string alunoCodigo, long ultimoId);
        Task AtualizarRecomendacoesConselhoClasseAlunoDuplicado(long conselhoClasseId, string alunoCodigo, long ultimoId);
        Task ExcluirTurmasComplementaresConselhoClasseAlunoDuplicado(long conselhoClasseId, string alunoCodigo, long ultimoId);
        Task AtualizarWfAprovacaoConselhoClasseAlunoDuplicado(long conselhoClasseId, string alunoCodigo, long ultimoId);
        Task AtualizarParecerConclusivoConselhoClasseAlunoDuplicado(long conselhoClasseId, string alunoCodigo, long ultimoId);
        Task ExcluirConselhoClasseAlunoDuplicado(long conselhoClasseId, string alunoCodigo, long ultimoId);
        // ConselhoClasseNotas
        Task AtualizarWfAprovacaoConselhoClasseNotaDuplicado(long conselhoClasseAlunoId, long componenteCurricularId, long ultimoId);
        Task AtualizarHistoricoNotaConselhoClasseNotaDuplicado(long conselhoClasseAlunoId, long componenteCurricularId, long ultimoId);
        Task AtualizarMaiorNotaConselhoClasseNotaDuplicado(long conselhoClasseAlunoId, long componenteCurricularId, long ultimoId);
        Task ExcluirConselhoClasseNotaDuplicado(long conselhoClasseAlunoId, long componenteCurricularId, long ultimoId);
        // FechamentoTurma
        Task AtualizarConselhoClasseFechamentoTurmaDuplicados(long turmaId, long periodoEscolarId, long ultimoId);
        Task AtualizarComponenteFechamentoTurmaDuplicados(long turmaId, long periodoEscolarId, long ultimoId);
        Task ExcluirFechamentoTurmaDuplicados(long turmaId, long periodoEscolarId, long ultimoId);
    }
}
