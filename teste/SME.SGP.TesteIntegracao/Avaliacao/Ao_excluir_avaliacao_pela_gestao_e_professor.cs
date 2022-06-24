﻿using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using SME.SGP.Aplicacao;
using SME.SGP.Dominio;
using SME.SGP.TesteIntegracao.Setup;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace SME.SGP.TesteIntegracao.TestarAvaliacaoAula
{
    public class Ao_excluir_avaliacao_pela_gestao_e_professor : TesteAvaliacao
    {
        public Ao_excluir_avaliacao_pela_gestao_e_professor(CollectionFixture collectionFixture) : base(collectionFixture)
        {
        }

        [Fact]
        public async Task Excluir_avaliacao_pelo_cp()
        {
            await ExecuteExclusao(ObterPerfilCP(), COMPONENTE_CURRICULAR_PORTUGUES_ID_138.ToString());
        }

        [Fact]
        public async Task Excluir_avaliacao_pelo_diretor()
        {
            await ExecuteExclusao(ObterPerfilDiretor(), COMPONENTE_CURRICULAR_PORTUGUES_ID_138.ToString());
        }

        [Fact]
        public async Task Excluir_avaliacao_pelo_professor()
        {
            await ExecuteExclusao(ObterPerfilProfessor(), COMPONENTE_CURRICULAR_PORTUGUES_ID_138.ToString());
        }

        [Fact]
        public async Task Excluir_avaliacao_pelo_professor_regente_diferente()
        {
            await CriarDadosBasicos(ObterCriacaoDeDadosDto(ObterPerfilProfessor()));
            await CriarAtividadeAvaliativaFundamental(DATA_02_05, COMPONENTE_REGENCIA_CLASSE_FUND_I_5H_ID_1105.ToString(), TipoAvaliacaoCodigo.AvaliacaoMensal, true, false, USUARIO_PROFESSOR_CODIGO_RF_1111111);
            await CriarAtividadeAvaliativaRegencia(COMPONENTE_REGENCIA_CLASSE_FUND_I_5H_ID_1105.ToString(), COMPONENTE_REGENCIA_CLASSE_FUND_I_5H_NOME_1105);

            var comando = ServiceProvider.GetService<IComandosAtividadeAvaliativa>();

            await comando.Excluir(1);

            ExecuteValidacao();

            var atividadeRegencia = ObterTodos<AtividadeAvaliativaRegencia>();

            atividadeRegencia.ShouldNotBeEmpty();
            atividadeRegencia.FirstOrDefault().Excluido.ShouldBe(true);
        }

        [Fact]
        public async Task Nao_foi_possivel_localizar_avaliacao()
        {
            await CriarDadosBasicos(ObterCriacaoDeDadosDto(ObterPerfilDiretor()));
            var comando = ServiceProvider.GetService<IComandosAtividadeAvaliativa>();
            var excecao = await Assert.ThrowsAsync<NegocioException>(() => comando.Excluir(1));

            excecao.Message.ShouldBe("Não foi possível localizar esta avaliação.");
        }

        private async Task ExecuteExclusao(string perfil, string componente)
        {
            await CriarDadosBasicos(ObterCriacaoDeDadosDto(perfil));
            await CrieAula(componente, DATA_02_05);
            await CriarAtividadeAvaliativaFundamental(DATA_02_05, componente, TipoAvaliacaoCodigo.AvaliacaoBimestral);

            var comando = ServiceProvider.GetService<IComandosAtividadeAvaliativa>();

            await comando.Excluir(1);

            ExecuteValidacao();
        }

        private void ExecuteValidacao()
        {
            var atividadeAvaliativas = ObterTodos<AtividadeAvaliativa>();

            atividadeAvaliativas.ShouldNotBeEmpty();
            atividadeAvaliativas.FirstOrDefault().Excluido.ShouldBe(true);

            var atividadeDisciplina = ObterTodos<AtividadeAvaliativaDisciplina>();

            atividadeDisciplina.ShouldNotBeEmpty();
            atividadeDisciplina.FirstOrDefault().Excluido.ShouldBe(true);
        }

        private CriacaoDeDadosDto ObterCriacaoDeDadosDto(string perfil)
        {
            return new CriacaoDeDadosDto()
            {
                Perfil = perfil,
                ModalidadeTurma = Modalidade.Fundamental,
                TipoCalendario = ModalidadeTipoCalendario.FundamentalMedio,
                TipoCalendarioId = TIPO_CALENDARIO_ID,
                DataInicio = DATA_02_05,
                DataFim = DATA_08_07,
                TipoAvaliacao = TipoAvaliacaoCodigo.AvaliacaoBimestral,
                Bimestre = BIMESTRE_2
            };
        }
    }
}
