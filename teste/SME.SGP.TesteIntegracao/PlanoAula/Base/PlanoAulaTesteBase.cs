﻿using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using SME.SGP.Aplicacao;
using SME.SGP.Dominio;
using SME.SGP.Infra;
using SME.SGP.TesteIntegracao.Setup;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SME.SGP.TesteIntegracao.PlanoAula.Base
{
    public abstract class PlanoAulaTesteBase : TesteBaseComuns
    {
        private const int QUANTIDADE_3 = 3;
        protected const long AULA_ID_1 = 1;
        private const string OBJETIVO_APRENDIZAGEM_DESCRICAO_1 = "OBJETIVO APRENDIZAGEM 1";
        private const string OBJETIVO_APRENDIZAGEM_CODIGO_1 = "CDGAPRE1";
        private const string OBJETIVO_APRENDIZAGEM_DESCRICAO_2 = "OBJETIVO APRENDIZAGEM 2";
        private const string OBJETIVO_APRENDIZAGEM_CODIGO_2 = "CDGAPRE2";
        private const string OBJETIVO_APRENDIZAGEM_DESCRICAO_3 = "OBJETIVO APRENDIZAGEM 3";
        private const string OBJETIVO_APRENDIZAGEM_CODIGO_3 = "CDGAPRE3";
        private const string OBJETIVO_APRENDIZAGEM_DESCRICAO_4 = "OBJETIVO APRENDIZAGEM 4";
        private const string OBJETIVO_APRENDIZAGEM_CODIGO_4 = "CDGAPRE4";
        private const string OBJETIVO_APRENDIZAGEM_DESCRICAO_5 = "OBJETIVO APRENDIZAGEM 5";
        private const string OBJETIVO_APRENDIZAGEM_CODIGO_5 = "CDGAPRE5";
        protected PlanoAulaTesteBase(CollectionFixture collectionFixture) : base(collectionFixture)
        {
        }

        protected override void RegistrarFakes(IServiceCollection services)
        {
            base.RegistrarFakes(services);

            services.Replace(new ServiceDescriptor(typeof(IRequestHandler<ObterFuncionarioCoreSSOPorPerfilDreQuery, IEnumerable<UsuarioEolRetornoDto>>), typeof(ObterFuncionarioCoreSSOPorPerfilDreQueryHandlerFake), ServiceLifetime.Scoped));
            services.Replace(new ServiceDescriptor(typeof(IRequestHandler<ObterFuncionariosPorPerfilDreQuery, IEnumerable<UsuarioEolRetornoDto>>), typeof(ObterFuncionariosPorPerfilDreQueryHandlerFake), ServiceLifetime.Scoped));
        }

        protected ISalvarPlanoAulaUseCase ObterServicoISalvarPlanoAulaUseCase()
        {
            return ServiceProvider.GetService<ISalvarPlanoAulaUseCase>();
        }

        protected ISalvarPlanoAulaUseCase ObterServicoSalvarPlanoAulaUseCase()
        {
            return ServiceProvider.GetService<ISalvarPlanoAulaUseCase>();
        }
        protected IObterPlanoAulaUseCase ObterServicoObterPlanoAulaUseCase()
        {
            return ServiceProvider.GetService<IObterPlanoAulaUseCase>();
        }

        protected IObterPlanoAulasPorTurmaEComponentePeriodoUseCase ObterServicoObterPlanoAulasPorTurmaEComponentePeriodoUseCase()
        {
            return ServiceProvider.GetService<IObterPlanoAulasPorTurmaEComponentePeriodoUseCase>();
        }

        protected IMigrarPlanoAulaUseCase ObterServicoMigrarPlanoAulaUseCase(MigrarPlanoAulaDto migrarPlanoAulaDto)
        {
            return ServiceProvider.GetService<IMigrarPlanoAulaUseCase>();
        }

        protected IConsultasPlanoAula ObterServicoIConsultasPlanoAula()
        {
            return ServiceProvider.GetService<IConsultasPlanoAula>();
        }

        protected async Task CriarDadosBasicos(FiltroPlanoAula filtroPlanoAula)
        {
            await CriarTipoCalendario(filtroPlanoAula.TipoCalendario);
            
            await CriarItensComuns(filtroPlanoAula.CriarPeriodo, filtroPlanoAula.DataInicio, filtroPlanoAula.DataFim, filtroPlanoAula.Bimestre, filtroPlanoAula.TipoCalendarioId);
            
            CriarClaimUsuario(filtroPlanoAula.Perfil);
            
            await CriarUsuarios();
            
            await CriarTurma(filtroPlanoAula.Modalidade);
            
            await CriarAula(filtroPlanoAula.ComponenteCurricularCodigo, filtroPlanoAula.DataAula, RecorrenciaAula.AulaUnica, filtroPlanoAula.quantidadeAula);
            
            if (filtroPlanoAula.CriarPeriodoEscolarEAbertura)
                await CriarPeriodoEscolarEAbertura();

            await CriarObjetivoAprendizagem(filtroPlanoAula.ComponenteCurricularCodigo);
        }

        private async Task CriarObjetivoAprendizagem(string componenteCurricularCodigo)
        {
            await InserirNaBase(new ObjetivoAprendizagem()
            {
                Descricao = OBJETIVO_APRENDIZAGEM_DESCRICAO_1,
                CodigoCompleto = OBJETIVO_APRENDIZAGEM_CODIGO_1,
                AnoTurma = "first",
                ComponenteCurricularId = long.Parse(componenteCurricularCodigo),
                CriadoEm = DateTime.Now,
                AtualizadoEm = DateTime.Now
            });
            
            await InserirNaBase(new ObjetivoAprendizagem()
            {
                Descricao = OBJETIVO_APRENDIZAGEM_DESCRICAO_2,
                CodigoCompleto = OBJETIVO_APRENDIZAGEM_CODIGO_2,
                AnoTurma = "first",
                ComponenteCurricularId = long.Parse(componenteCurricularCodigo),
                CriadoEm = DateTime.Now,
                AtualizadoEm = DateTime.Now
            });
            
            await InserirNaBase(new ObjetivoAprendizagem()
            {
                Descricao = OBJETIVO_APRENDIZAGEM_DESCRICAO_3,
                CodigoCompleto = OBJETIVO_APRENDIZAGEM_CODIGO_3,
                AnoTurma = "first",
                ComponenteCurricularId = long.Parse(componenteCurricularCodigo),
                CriadoEm = DateTime.Now,
                AtualizadoEm = DateTime.Now
            });
            
            await InserirNaBase(new ObjetivoAprendizagem()
            {
                Descricao = OBJETIVO_APRENDIZAGEM_DESCRICAO_4,
                CodigoCompleto = OBJETIVO_APRENDIZAGEM_CODIGO_4,
                AnoTurma = "first",
                ComponenteCurricularId = long.Parse(componenteCurricularCodigo),
                CriadoEm = DateTime.Now,
                AtualizadoEm = DateTime.Now
            });
            
            await InserirNaBase(new ObjetivoAprendizagem()
            {
                Descricao = OBJETIVO_APRENDIZAGEM_DESCRICAO_5,
                CodigoCompleto = OBJETIVO_APRENDIZAGEM_CODIGO_5,
                AnoTurma = "first",
                ComponenteCurricularId = long.Parse(componenteCurricularCodigo),
                CriadoEm = DateTime.Now,
                AtualizadoEm = DateTime.Now
            });
        }

        protected async Task CriarAula(string componenteCurricularCodigo, DateTime dataAula, RecorrenciaAula recorrencia, int quantidadeAula = QUANTIDADE_3, string rf = USUARIO_PROFESSOR_LOGIN_2222222)
        {
            await InserirNaBase(ObterAula(componenteCurricularCodigo, dataAula, recorrencia, quantidadeAula, rf));
        }

        protected async Task CriarPeriodoEscolarEAbertura()
        {
            await CriarPeriodoEscolar(DATA_01_02_INICIO_BIMESTRE_1, DATA_25_04_FIM_BIMESTRE_1, BIMESTRE_1);

            await CriarPeriodoEscolar(DATA_02_05_INICIO_BIMESTRE_2, DATA_08_07_FIM_BIMESTRE_2, BIMESTRE_2);

            await CriarPeriodoEscolar(DATA_25_07_INICIO_BIMESTRE_3, DATA_30_09_FIM_BIMESTRE_3, BIMESTRE_3);

            await CriarPeriodoEscolar(DATA_03_10_INICIO_BIMESTRE_4, DATA_22_12_FIM_BIMESTRE_4, BIMESTRE_4);

            await CriarPeriodoReabertura(TIPO_CALENDARIO_1);
        }

        private Dominio.Aula ObterAula(string componenteCurricularCodigo, DateTime dataAula, RecorrenciaAula recorrencia, int quantidadeAula, string rf = USUARIO_PROFESSOR_LOGIN_2222222)
        {
            return new Dominio.Aula
            {
                UeId = UE_CODIGO_1,
                DisciplinaId = componenteCurricularCodigo,
                TurmaId = TURMA_CODIGO_1,
                TipoCalendarioId = 1,
                ProfessorRf = rf,
                Quantidade = quantidadeAula,
                DataAula = dataAula,
                RecorrenciaAula = recorrencia,
                TipoAula = TipoAula.Normal,
                CriadoEm = DateTime.Now,
                CriadoPor = SISTEMA_NOME,
                CriadoRF = SISTEMA_CODIGO_RF,
                Excluido = false,
                Migrado = false,
                AulaCJ = false
            };
        }

        protected async Task CriarPeriodoEscolarEAberturaPadrao()
        {
            await CriarPeriodoEscolar(DATA_01_02_INICIO_BIMESTRE_1, DATA_25_04_FIM_BIMESTRE_1, BIMESTRE_1);

            await CriarPeriodoEscolar(DATA_02_05_INICIO_BIMESTRE_2, DATA_08_07_FIM_BIMESTRE_2, BIMESTRE_2);

            await CriarPeriodoEscolar(DATA_25_07_INICIO_BIMESTRE_3, DATA_30_09_FIM_BIMESTRE_3, BIMESTRE_3);

            await CriarPeriodoEscolar(DATA_03_10_INICIO_BIMESTRE_4, DATA_22_12_FIM_BIMESTRE_4, BIMESTRE_4);

            await CriarPeriodoReabertura(TIPO_CALENDARIO_1);
        }
    }
}