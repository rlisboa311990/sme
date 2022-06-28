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
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace SME.SGP.TesteIntegracao.TestarAulaRecorrencia
{
    public class Ao_alterar_aula_com_recorrencia : AulaTeste
    {
        private DateTime DATA_02_05 = new(DateTimeExtension.HorarioBrasilia().Year, 05, 02);
        private DateTime DATA_08_07 = new(DateTimeExtension.HorarioBrasilia().Year, 07, 08);

        public Ao_alterar_aula_com_recorrencia(CollectionFixture collectionFixture) : base(collectionFixture) { }

        protected override void RegistrarFakes(IServiceCollection services)
        {
            base.RegistrarFakes(services);

            services.Replace(new ServiceDescriptor(typeof(IRequestHandler<ObterUsuarioPossuiPermissaoNaTurmaEDisciplinaQuery, bool>), typeof(ObterUsuarioPossuiPermissaoNaTurmaEDisciplinaQueryHandlerComPermissaoFake), ServiceLifetime.Scoped));
        }

        [Fact]
        public async Task Altera_quantidade_de_aulas_com_recorrencia_no_bimestre_atual()
        {
            await CriarDadosBasicosAula(ObterPerfilProfessor(), Modalidade.Fundamental, ModalidadeTipoCalendario.FundamentalMedio, DATA_02_05, DATA_08_07, BIMESTRE_2, false);

            await CriaAulaRecorrentePortugues(RecorrenciaAula.RepetirBimestreAtual);

            var usecase = ServiceProvider.GetService<IAlterarAulaUseCase>();

            var aula = ObterAula(TipoAula.Normal, RecorrenciaAula.RepetirBimestreAtual, 138, DATA_02_05);

            aula.DataAula = new DateTime(DateTimeExtension.HorarioBrasilia().Year, 06, 27);

            aula.Id = 1;

            await CriarPeriodoEscolarEAbertura();

            var retorno = await usecase.Executar(aula);

            var listaNotificao = ObterTodos<Notificacao>();

            retorno.ShouldNotBeNull();

            listaNotificao.FirstOrDefault().Mensagem.ShouldContain("Foram alteradas 2 aulas do componente curricular Português para a turma Turma Nome 1 da Nome da UE (DRE 1).");
        }

        [Fact]
        public async Task Altera_quantidade_de_aulas_com_recorrencia_para_todos_bimestres()
        {
            await CriarDadosBasicosAula(ObterPerfilProfessor(), Modalidade.Fundamental, ModalidadeTipoCalendario.FundamentalMedio, DATA_02_05, DATA_08_07, BIMESTRE_1, false);

            await CriaAulaRecorrentePortugues(RecorrenciaAula.RepetirTodosBimestres);

            var usecase = ServiceProvider.GetService<IAlterarAulaUseCase>();
            
            var aula = ObterAula(TipoAula.Normal, RecorrenciaAula.RepetirTodosBimestres, 138, DATA_02_05);
            
            aula.DataAula = new DateTime(DateTimeExtension.HorarioBrasilia().Year, 08, 26);
            
            aula.Id = 1;

            await CriarPeriodoEscolarEAbertura();

            var retorno = await usecase.Executar(aula);
            
            var listaNotificao = ObterTodos<Notificacao>();

            retorno.ShouldNotBeNull();

            listaNotificao.ShouldNotBeEmpty();

            listaNotificao.FirstOrDefault().Mensagem.ShouldContain("Foram alteradas 17 aulas do componente curricular Português para a turma Turma Nome 1 da Nome da UE (DRE 1).");

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
