﻿using SME.SGP.Dominio;
using SME.SGP.Infra;
using SME.SGP.TesteIntegracao.Setup;
using System.Threading.Tasks;
using Xunit;

namespace SME.SGP.TesteIntegracao.ConselhoDeClasse
{
    public class Ao_inserir_conceito_pos_conselho_final : ConselhoDeClasseTesteBase
    {
        public Ao_inserir_conceito_pos_conselho_final(CollectionFixture collectionFixture) : base(collectionFixture)
        {
        }

        // [Theory]
        // [InlineData(false)]
        // [InlineData(true)]
        // public async Task Deve_lancar_conceito_pos_conselho_final(bool anoAnterior)
        // {
        //     await CrieDados(ObterPerfilProfessor(), COMPONENTE_CURRICULAR_PORTUGUES_ID_138, TipoNota.Conceito, ANO_4, Modalidade.Fundamental, ModalidadeTipoCalendario.FundamentalMedio, anoAnterior, SituacaoConselhoClasse.EmAndamento, true);
        //     var fechamento = ObterTodos<FechamentoTurma>();
        //     await ExecutarTeste(ObtenhaDto(COMPONENTE_CURRICULAR_PORTUGUES_ID_138), 0, anoAnterior, ALUNO_CODIGO_1, TipoNota.Conceito, BIMESTRE_1, SituacaoConselhoClasse.EmAndamento, FECHAMENTO_TURMA_ID_1);
        // }
        //
        // [Theory]
        // [InlineData(false)]
        // [InlineData(true)]
        // public async Task Deve_lancar_conceito_pos_conselho_bimestre_regencia_fundamental_final(bool anoAnterior)
        // {
        //     await CrieDados(ObterPerfilProfessor(), COMPONENTE_REGENCIA_CLASSE_FUND_I_5H_ID_1105, TipoNota.Conceito, ANO_4, Modalidade.Fundamental, ModalidadeTipoCalendario.FundamentalMedio, anoAnterior);
        //     await ExecutarTeste(ObtenhaDto(COMPONENTE_REGENCIA_CLASSE_FUND_I_5H_ID_1105), 0, anoAnterior, ALUNO_CODIGO_1, TipoNota.Conceito, BIMESTRE_FINAL, SituacaoConselhoClasse.EmAndamento, FECHAMENTO_TURMA_ID_2);
        // }
        //
        // [Theory]
        // [InlineData(false)]
        // [InlineData(true)]
        // public async Task Deve_lancar_conceito_pos_conselho_bimestre_regencia_EJA_final(bool anoAnterior)
        // {
        //     await CrieDados(ObterPerfilProfessor(), COMPONENTE_CURRICULAR_PORTUGUES_ID_138, TipoNota.Conceito, ANO_4, Modalidade.EJA, ModalidadeTipoCalendario.EJA, anoAnterior);
        //     await ExecutarTeste(ObtenhaDto(COMPONENTE_CURRICULAR_PORTUGUES_ID_138), 0, anoAnterior, ALUNO_CODIGO_1, TipoNota.Conceito, BIMESTRE_FINAL, SituacaoConselhoClasse.EmAndamento, FECHAMENTO_TURMA_ID_2);
        // }

        private async Task CrieDados(string perfil, long componente, TipoNota tipo, string anoTurma, Modalidade modalidade, ModalidadeTipoCalendario modalidadeTipoCalendario, bool anoAnterior, SituacaoConselhoClasse situacaoConselhoClasse = SituacaoConselhoClasse.NaoIniciado, bool criarFechamentoDisciplinaAlunoNota = false)
        {
            var dataAula = anoAnterior ? DATA_03_10_INICIO_BIMESTRE_4.AddYears(-1) : DATA_03_10_INICIO_BIMESTRE_4;

            var filtroNota = new FiltroNotasDto()
            {
                Perfil = perfil,
                Modalidade = modalidade,
                TipoCalendario = modalidadeTipoCalendario,
                Bimestre = BIMESTRE_FINAL,
                ComponenteCurricular = componente.ToString(),
                TipoNota = tipo,
                AnoTurma = anoTurma,
                ConsiderarAnoAnterior = anoAnterior,
                DataAula = dataAula,
                CriarFechamentoDisciplinaAlunoNota = criarFechamentoDisciplinaAlunoNota,
                SituacaoConselhoClasse = situacaoConselhoClasse
            };

            await CriarDadosBase(filtroNota);
        }

        // private async Task ExecuteTeste(long componente, bool anoAnterior)
        // {
        //     await ExecutarTeste(ObtenhaDto(componente), 0, anoAnterior, ALUNO_CODIGO_1, TipoNota.Conceito, BIMESTRE_FINAL);
        // }

        private ConselhoClasseNotaDto ObtenhaDto(long componente)
        {
            return new ConselhoClasseNotaDto()
            {
                CodigoComponenteCurricular = componente,
                Justificativa = JUSTIFICATIVA,
                Conceito = 1
            };
        }
    }
}