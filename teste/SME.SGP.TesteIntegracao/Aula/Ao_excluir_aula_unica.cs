﻿using Microsoft.Extensions.DependencyInjection;
using Moq;
using Shouldly;
using SME.SGP.Aplicacao.Interfaces;
using SME.SGP.Dominio;
using SME.SGP.Dominio.Interfaces;
using SME.SGP.TesteIntegracao.Setup;
using System;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace SME.SGP.TesteIntegracao.AulaUnica
{
    public class Ao_excluir_aula_unica : AulaTeste
    {
        private DateTime DATA_02_05 = new(DateTimeExtension.HorarioBrasilia().Year, 05, 02);
        private DateTime DATA_08_07 = new(DateTimeExtension.HorarioBrasilia().Year, 07, 08);
         private readonly Mock<IRepositorioAtividadeAvaliativa> repositorioAtividadeAvaliativa;

        public Ao_excluir_aula_unica(CollectionFixture collectionFixture) : base(collectionFixture)
        {
            repositorioAtividadeAvaliativa = new Mock<IRepositorioAtividadeAvaliativa>();
        }

        [Fact]
        public async Task Aula_nao_encontrada()
        {
            CriarClaimUsuario(ObterPerfilProfessor());

            await CriarUsuarios();

            var useCase = ServiceProvider.GetService<IExcluirAulaUseCase>();

            var dto = ObterExcluirAulaDto(RecorrenciaAula.AulaUnica);

            await Assert.ThrowsAsync<NegocioException>(() => useCase.Executar(dto));
        }

        [Fact]
        public async Task Exclui_aula_unica()
        {
            
            await CriarDadosBasicosAula_Exclusao(ObterPerfilProfessor(), Modalidade.Fundamental, ModalidadeTipoCalendario.FundamentalMedio, DATA_02_05, DATA_08_07, BIMESTRE_2, false);

            await CriarAula_Exclusao(COMPONENTE_CURRICULAR_PORTUGUES_ID_138.ToString(), DATA_02_05, RecorrenciaAula.AulaUnica);

            var useCase = ServiceProvider.GetService<IExcluirAulaUseCase>();

            var dto = ObterExcluirAulaDto(RecorrenciaAula.AulaUnica);

            await CriarPeriodoEscolarEAbertura();

            var retorno = await useCase.Executar(dto);

            retorno.ShouldNotBeNull();

            var lista = ObterTodos<Aula>();

            lista.ShouldNotBeEmpty();

            lista.FirstOrDefault().Excluido.ShouldBe(true);
        }

        [Fact]
        public async Task Aula_possui_avaliacao()
        {
            var repo = repositorioAtividadeAvaliativa.Setup(a => a.VerificarSeExisteAvaliacao(It.IsAny<DateTime>(), It.IsAny<string>(),It.IsAny<string>(),It.IsAny<string>(),It.IsAny<string>())).Returns(Task.FromResult(true));
            await CriarDadosBasicosAula_Exclusao(ObterPerfilProfessor(), Modalidade.Fundamental, ModalidadeTipoCalendario.FundamentalMedio, DATA_02_05, DATA_08_07, BIMESTRE_2, false);

            await CriarAula_Exclusao(COMPONENTE_CURRICULAR_PORTUGUES_ID_138.ToString(), DATA_02_05, RecorrenciaAula.AulaUnica);

            await CriarAtividadeAvaliativaFundamental(DATA_02_05, COMPONENTE_CURRICULAR_PORTUGUES_ID_138.ToString());

            await CriarPeriodoEscolarEAbertura();

            var useCase = ServiceProvider.GetService<IExcluirAulaUseCase>();

            var dto = ObterExcluirAulaDto(RecorrenciaAula.AulaUnica);

            var excecao = await Assert.ThrowsAsync<NegocioException>(() => useCase.Executar(dto));

            excecao.Message.ShouldBe("Aula com avaliação vinculada. Para excluir esta aula primeiro deverá ser excluída a avaliação.");
        }

        private async Task CriarPeriodoEscolarEAbertura()
        {
            await CriarPeriodoEscolar(DATA_01_02_INICIO_BIMESTRE_1, DATA_25_04_FIM_BIMESTRE_1, BIMESTRE_1);

            await CriarPeriodoEscolar(DATA_02_05_INICIO_BIMESTRE_2, DATA_08_07_FIM_BIMESTRE_2, BIMESTRE_2);

            await CriarPeriodoEscolar(DATA_25_07_INICIO_BIMESTRE_3, DATA_30_09_FIM_BIMESTRE_3, BIMESTRE_3);

            await CriarPeriodoEscolar(DATA_03_10_INICIO_BIMESTRE_4, DATA_22_12_FIM_BIMESTRE_4, BIMESTRE_4);

            await CriarPeriodoReabertura(TIPO_CALENDARIO_1);
        }
    }
}
