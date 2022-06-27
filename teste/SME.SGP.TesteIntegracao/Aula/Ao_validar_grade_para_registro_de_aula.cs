﻿using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Shouldly;
using SME.SGP.Aplicacao;
using SME.SGP.Aplicacao.Interfaces;
using SME.SGP.Dominio;
using SME.SGP.TesteIntegracao.ServicosFakes;
using SME.SGP.TesteIntegracao.Setup;
using System;
using System.Threading.Tasks;
using Xunit;

namespace SME.SGP.TesteIntegracao.TestarAulaUnicaGrade
{
    public class Ao_validar_grade_para_registro_de_aula : AulaTeste
    {
        private DateTime dataInicio = new(DateTimeExtension.HorarioBrasilia().Year, 05, 02);
        private DateTime dataFim = new(DateTimeExtension.HorarioBrasilia().Year, 07, 08);

        public Ao_validar_grade_para_registro_de_aula(CollectionFixture collectionFixture) : base(collectionFixture)
        {

        }

        protected override void RegistrarFakes(IServiceCollection services)
        {
            base.RegistrarFakes(services);

            services.Replace(new ServiceDescriptor(typeof(IRequestHandler<ObterUsuarioPossuiPermissaoNaTurmaEDisciplinaQuery, bool>), typeof(ObterUsuarioPossuiPermissaoNaTurmaEDisciplinaQueryHandlerComPermissaoFake), ServiceLifetime.Scoped));
        }

        [Fact]
        public async Task Quantidade_aulas_superior_ao_limite_de_aulas_da_grade()
        {
            await CriarDadosBasicosAula(ObterPerfilProfessor(), Modalidade.Fundamental, ModalidadeTipoCalendario.FundamentalMedio, dataInicio, dataFim, BIMESTRE_2);
            await CriarGrade();

            var useCase = ServiceProvider.GetService<IInserirAulaUseCase>();
            var dto = ObterAula(TipoAula.Normal, RecorrenciaAula.AulaUnica, COMPONENTE_CURRICULAR_PORTUGUES_ID_138, dataInicio);
            dto.Quantidade = 2;

            await CriarPeriodoEscolarEAbertura();

            var excecao = await Assert.ThrowsAsync<NegocioException>(() => useCase.Executar(dto));

            excecao.Message.ShouldBe("Quantidade de aulas superior ao limíte de aulas da grade.");
        }

        [Fact]
        public async Task EJA_so_permite_criacao_de_5_aulas()
        {
            await CriarDadosBasicosAula(ObterPerfilProfessor(), Modalidade.EJA, ModalidadeTipoCalendario.EJA, dataInicio, dataFim, BIMESTRE_2);
            await CriarAula(COMPONENTE_CURRICULAR_PORTUGUES_ID_138.ToString(), dataInicio, RecorrenciaAula.AulaUnica, "1111111");
            await CriarGrade(5);

            var useCase = ServiceProvider.GetService<IInserirAulaUseCase>();
            var dto = ObterAula(TipoAula.Normal, RecorrenciaAula.AulaUnica, COMPONENTE_CURRICULAR_PORTUGUES_ID_138, dataInicio);
            dto.EhRegencia = true;

            await CriarPeriodoEscolarEAbertura();

            var excecao = await Assert.ThrowsAsync<NegocioException>(() => useCase.Executar(dto));

            excecao.Message.ShouldBe("Para regência de EJA só é permitido a criação de 5 aulas por dia.");
        }

        [Fact]
        public async Task Regencia_classe_permite_criacao_de_uma_aula()
        {
            await CriarDadosBasicosAula(ObterPerfilProfessor(), Modalidade.Fundamental, ModalidadeTipoCalendario.FundamentalMedio, dataInicio, dataFim, BIMESTRE_2);
            await CriarAula(COMPONENTE_CURRICULAR_PORTUGUES_ID_138.ToString(), dataInicio, RecorrenciaAula.AulaUnica, "1111111");
            await CriarGrade(5);

            var useCase = ServiceProvider.GetService<IInserirAulaUseCase>();
            var dto = ObterAula(TipoAula.Normal, RecorrenciaAula.AulaUnica, COMPONENTE_CURRICULAR_PORTUGUES_ID_138, dataInicio);
            dto.EhRegencia = true;

            await CriarPeriodoEscolarEAbertura();

            var excecao = await Assert.ThrowsAsync<NegocioException>(() => useCase.Executar(dto));

            excecao.Message.ShouldBe("Para regência de classe só é permitido a criação de 1 (uma) aula por dia.");
        }

        private async Task CriarGrade(int quantidadeAula = 1)
        {
            await InserirNaBase(new Grade
            {
                Nome = "Grade",
                CriadoPor = "Sistema",
                CriadoRF = "1",
                CriadoEm = DateTime.Now
            });

            await InserirNaBase(new GradeFiltro
            {
                GradeId = 1,
                Modalidade = Modalidade.Fundamental,
                TipoEscola = TipoEscola.Nenhum,
                DuracaoTurno = 0,
                CriadoPor = "Sistema",
                CriadoRF = "1",
                CriadoEm = DateTime.Now
            });

            await InserirNaBase(new GradeDisciplina
            {
                GradeId = 1,
                Ano = 2,
                QuantidadeAulas = quantidadeAula,
                ComponenteCurricularId = COMPONENTE_CURRICULAR_PORTUGUES_ID_138,
                CriadoPor = "Sistema",
                CriadoRF = "1",
                CriadoEm = DateTime.Now
            });
        }

        private async Task CriarPeriodoEscolarEAbertura()
        {
            await CriarPeriodoEscolar(DATA_INICIO_BIMESTRE_1, DATA_FIM_BIMESTRE_1, BIMESTRE_1);
            await CriarPeriodoEscolar(DATA_INICIO_BIMESTRE_2, DATA_FIM_BIMESTRE_2, BIMESTRE_2);
            await CriarPeriodoEscolar(DATA_INICIO_BIMESTRE_3, DATA_FIM_BIMESTRE_3, BIMESTRE_3);
            await CriarPeriodoEscolar(DATA_INICIO_BIMESTRE_4, DATA_FIM_BIMESTRE_4, BIMESTRE_4);
            await CriarPeriodoReabertura(TIPO_CALENDARIO_1);
        }
    }
}
