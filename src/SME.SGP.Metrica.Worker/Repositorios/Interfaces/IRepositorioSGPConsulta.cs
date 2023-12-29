﻿using SME.SGP.Metrica.Worker.Entidade;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SME.SGP.Metrica.Worker.Repositorios.Interfaces
{
    public interface IRepositorioSGPConsulta
    {
        Task<IEnumerable<long>> ObterUesIds();
        Task<IEnumerable<long>> ObterTurmasIds(int[] modalidades);
        Task<IEnumerable<long>> ObterTurmasIdsPorUE(long ueId);
        Task<int> ObterQuantidadeAcessosDia(DateTime data);
        Task<IEnumerable<ConselhoClasseDuplicado>> ObterConselhosClasseDuplicados();
        Task<IEnumerable<ConselhoClasseAlunoDuplicado>> ObterConselhosClasseAlunoDuplicados(long ueId);
        Task<IEnumerable<ConselhoClasseNotaDuplicado>> ObterConselhosClasseNotaDuplicados();
        Task<IEnumerable<FechamentoTurmaDuplicado>> ObterFechamentosTurmaDuplicados();
        Task<IEnumerable<FechamentoTurmaDisciplinaDuplicado>> ObterFechamentosTurmaDisciplinaDuplicados();
        Task<IEnumerable<FechamentoAlunoDuplicado>> ObterFechamentosAlunoDuplicados(long ueId);
        Task<IEnumerable<FechamentoNotaDuplicado>> ObterFechamentosNotaDuplicados(long turmaId);
        Task<IEnumerable<ConsolidacaoConselhoClasseNotaNulos>> ObterConsolidacaoCCNotasNulos();
        Task<IEnumerable<ConsolidacaoConselhoClasseAlunoTurmaDuplicado>> ObterConsolidacaoCCAlunoTurmaDuplicados(long ueId);
        Task<IEnumerable<ConsolidacaoCCNotaDuplicado>> ObterConsolidacaoCCNotasDuplicados();
        Task<IEnumerable<ConselhoClasseNaoConsolidado>> ObterConselhosClasseNaoConsolidados(long ueId);
        Task<IEnumerable<FrequenciaAlunoInconsistente>> ObterFrequenciaAlunoInconsistente(long turmaId);
        Task<IEnumerable<FrequenciaAlunoDuplicado>> ObterFrequenciaAlunoDuplicados(long ueId);
        Task<IEnumerable<RegistroFrequenciaDuplicado>> ObterRegistroFrequenciaDuplicados(long ueId);
        Task<IEnumerable<RegistroFrequenciaAlunoDuplicado>> ObterRegistroFrequenciaAlunoDuplicados(long turmaId);
        Task<IEnumerable<ConsolidacaoFrequenciaAlunoMensalInconsistente>> ObterConsolidacaoFrequenciaAlunoMensalInconsistente(long turmaId);
        Task<IEnumerable<DiarioBordoDuplicado>> ObterDiariosBordoDuplicados();
        Task<int> ObterQuantidadeRegistrosFrequenciaDia(DateTime data);
        Task<int> ObterQuantidadeDiariosBordoDia(DateTime data);
        Task<int> ObterQuantidadeDevolutivasDiarioBordoMes(DateTime data);
        Task<int> ObterQuantidadeAulasCJMes(DateTime data);
        Task<int> ObterQuantidadePlanosAulaDia(DateTime data);
        Task<int> ObterQuantidadeEncaminhamentosAEEMes(DateTime data);
        Task<int> ObterQuantidadePlanosAEEMes(DateTime data);
    }
}