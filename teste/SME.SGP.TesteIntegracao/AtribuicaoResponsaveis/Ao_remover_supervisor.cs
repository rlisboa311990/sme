﻿using Microsoft.Extensions.DependencyInjection;
using SME.SGP.Aplicacao.Interfaces;
using SME.SGP.Dominio;
using SME.SGP.Infra;
using System;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace SME.SGP.TesteIntegracao.AtribuicaoResponsaveis
{
    [Collection("AtribuicaoResponsaveis")]
    public class Ao_remover_supervisor : TesteBase
    {
        #region Constantes
        const string DRE_CODIGO_1 = "1";
        const string DRE_CODIGO_2 = "2";
        const string SUPERVISOR_ID_1 = "1";
        const string SUPERVISOR_ID_2 = "2";
        const string SUPERVISOR_ID_3 = "3";
        const string SUPERVISOR_RF_01 = "1";
        const string SUPERVISOR_RF_02 = "2";
        const string SUPERVISOR_RF_03 = "3";
        #endregion

        public Ao_remover_supervisor(AtribuicaoResponsavelFixture collectionFixture) : base(collectionFixture) { }

        [Fact]
        public async Task Deve_retornar_true_quando_excluir_alguns_mas_nao_todos_responsaveis()
        {
            //Arrange
            await InserirDre(DRE_CODIGO_1);
            await InserirSupervisor(SUPERVISOR_ID_1, SUPERVISOR_RF_01);
            await InserirSupervisor(SUPERVISOR_ID_2, SUPERVISOR_RF_02);
            await InserirSupervisor(SUPERVISOR_ID_3, SUPERVISOR_RF_03);

            var useCase = ServiceProvider.GetService<IRemoverAtribuicaoResponsaveisSupervisorPorDreUseCase>();

            //Act
            var retorno = await useCase.Executar(new MensagemRabbit(DRE_CODIGO_1));

            var registrosAposuseCase = ObterTodos<SupervisorEscolaDre>();

            //Assert
            Assert.True(retorno);
            Assert.True(registrosAposuseCase.Count(x => x.Excluido) == 1);
        }

        [Fact]
        public async Task Deve_retornar_true_quando_excluir_responsaveis_nao_estao_no_eol()
        {
            //Arrange
            await InserirDre(DRE_CODIGO_1);
            await InserirSupervisor(SUPERVISOR_ID_3, SUPERVISOR_RF_03);

            var useCase = ServiceProvider.GetService<IRemoverAtribuicaoResponsaveisSupervisorPorDreUseCase>();

            //Act
            var retorno = await useCase.Executar(new MensagemRabbit(DRE_CODIGO_1));

            var registrosAposuseCase = ObterTodos<SupervisorEscolaDre>();

            //Assert
            Assert.True(retorno);
            Assert.True(registrosAposuseCase.Count(x => x.Excluido) == 1);
        }

        [Fact]
        public async Task Deve_retornar_true_quando_nao_excluir_supervisores()
        {
            //Arrange
            await InserirDre(DRE_CODIGO_2);
            await InserirSupervisor(SUPERVISOR_ID_2, SUPERVISOR_RF_03);

            var useCase = ServiceProvider.GetService<IRemoverAtribuicaoResponsaveisSupervisorPorDreUseCase>();
            //Act
            var registrosAposUseCase = ObterTodos<SupervisorEscolaDre>();
            //Assert
            Assert.True(registrosAposUseCase.Count(x => x.Excluido) == 0);

        }

        [Fact]
        public async Task Deve_retornar_true_quando_nao_excluir_nada_no_SGP()
        {
            //Arrange
            await InserirDre(DRE_CODIGO_1);
            var useCase = ServiceProvider.GetService<IRemoverAtribuicaoResponsaveisSupervisorPorDreUseCase>();

            //Act
            var retorno = await useCase.Executar(new MensagemRabbit(DRE_CODIGO_1));
            //Assert
            Assert.True(retorno);
        }

        #region Cargas
        public async Task InserirDre(string codigoDre)
        {
            await InserirNaBase(new Dre()
            {
                Abreviacao = "DT",
                CodigoDre = codigoDre,
                DataAtualizacao = DateTime.Now,
                Nome = "Dre Teste"
            });
        }


        public async Task InserirSupervisor(string id, string rf)
        {
            await InserirNaBase(new SupervisorEscolaDre()
            {
                DreId = "1",
                EscolaId = "1",
                SupervisorId = id,
                Tipo = 1,
                CriadoEm = DateTime.Now,
                CriadoPor = "Teste",
                CriadoRF = rf,
                Excluido = false
            });

        }
        #endregion
    }
}
