﻿using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Shouldly;
using SME.SGP.Aplicacao;
using SME.SGP.Dominio;
using SME.SGP.Dto;
using SME.SGP.Infra;
using SME.SGP.TesteIntegracao.PlanoAula.Base;
using SME.SGP.TesteIntegracao.PlanoAula.ServicosFakes;
using SME.SGP.TesteIntegracao.ServicosFakes;
using SME.SGP.TesteIntegracao.Setup;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace SME.SGP.TesteIntegracao.PlanoAula
{
    public class Ao_cadastrar_plano_aula_para_componente_territorio_do_saber : PlanoAulaTesteBase
    {
        //protected const string COMPONENTE_CURRICULAR_TERRIT_SABER_ARTE_ID_1520 = "1520";
        //protected const string COMPONENTE_CURRICULAR_TERRIT_SABER_ARTE_NOME = "TERRIT SABER / ARTE";
        public Ao_cadastrar_plano_aula_para_componente_territorio_do_saber(CollectionFixture collectionFixture) : base(collectionFixture)
        {
        }

        protected override void RegistrarFakes(IServiceCollection services)
        {
            base.RegistrarFakes(services);

            services.Replace(new ServiceDescriptor(typeof(IRequestHandler<ObterAbrangenciaPorTurmaEConsideraHistoricoQuery, AbrangenciaFiltroRetorno>), typeof(ObterAbrangenciaPorTurmaEConsideraHistoricoQueryHandlerFakeFundamental6A), ServiceLifetime.Scoped));
            services.Replace(new ServiceDescriptor(typeof(IRequestHandler<ObterUsuarioPossuiPermissaoNaTurmaEDisciplinaQuery, bool>), typeof(ObterUsuarioPossuiPermissaoNaTurmaEDisciplinaQueryHandlerComPermissaoFake), ServiceLifetime.Scoped));
        }

        [Fact]
        public async Task Deve_cadastrar_plano_aula()
        {
            var planoAulaDto = ObterPlanoAula();

            await CriarDadosBasicos(new FiltroPlanoAula()
            {
                Bimestre = BIMESTRE_2,
                Modalidade = Modalidade.Fundamental,
                Perfil = ObterPerfilCJ(),
                QuantidadeAula = 1,
                DataAula = new DateTime(DateTimeExtension.HorarioBrasilia().Year, 5, 2),
                DataInicio = DATA_02_05_INICIO_BIMESTRE_2,
                DataFim = DATA_08_07_FIM_BIMESTRE_2,
                CriarPeriodoEscolarBimestre = false,
                TipoCalendario = ModalidadeTipoCalendario.FundamentalMedio,
                ComponenteCurricularCodigo = COMPONENTE_TERRITORIO_SABER_EXP_PEDAG_ID_1214.ToString(),
                TipoCalendarioId = TIPO_CALENDARIO_1,
                CriarPeriodoEscolarEAberturaTodosBimestres = true
            });

            var salvarPlanoAulaUseCase = ObterServicoSalvarPlanoAulaUseCase();

            var retorno = await salvarPlanoAulaUseCase.Executar(planoAulaDto);

            var objetivosAprendizagem = ObterTodos<ObjetivoAprendizagemAula>();

            objetivosAprendizagem.ShouldNotBeNull();
            objetivosAprendizagem.Count.ShouldBe(3);
        }

        private PlanoAulaDto ObterPlanoAula()
        {
            return new PlanoAulaDto()
            {
                ComponenteCurricularId = COMPONENTE_TERRITORIO_SABER_EXP_PEDAG_ID_1214,
                ConsideraHistorico = false,
                AulaId = AULA_ID_1,
                Descricao = "<p><span>Objetivos específicos e desenvolvimento da aula</span></p>",
                LicaoCasa = null,
                ObjetivosAprendizagemComponente = new List<ObjetivoAprendizagemComponenteDto>()
                {
                    new()
                    {
                        ComponenteCurricularId = COMPONENTE_TERRITORIO_SABER_EXP_PEDAG_ID_1214,
                        Id = 1
                    },
                    new()
                    {
                        ComponenteCurricularId = COMPONENTE_TERRITORIO_SABER_EXP_PEDAG_ID_1214,
                        Id = 2
                    },
                    new()
                    {
                        ComponenteCurricularId = COMPONENTE_TERRITORIO_SABER_EXP_PEDAG_ID_1214,
                        Id = 3
                    },
                },
                RecuperacaoAula = null
            };
        }
    }
}