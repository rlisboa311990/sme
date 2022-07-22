﻿using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using SME.SGP.Dominio;
using SME.SGP.Infra;
using SME.SGP.Infra.Interface;
using SME.SGP.TesteIntegracao.ServicosFakes;
using SME.SGP.TesteIntegracao.Setup;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SME.SGP.TesteIntegracao.CompensacaoDeAusencia.Base
{
    public abstract class CompensacaoDeAusenciaTesteBase : TesteBaseComuns
    {
        private const string DESCRICAO_COMPENSACAO = "Compensação de ausência teste";
        private const string ATIVIDADE_COMPENSACAO = "Atividade teste";
        protected CompensacaoDeAusenciaTesteBase(CollectionFixture collectionFixture) : base(collectionFixture)
        {
            
        }

        protected override void RegistrarFakes(IServiceCollection services)
        {
            base.RegistrarFakes(services);

            services.Replace(new ServiceDescriptor(typeof(IServicoAuditoria), typeof(ServicoAuditoriaFake), ServiceLifetime.Scoped));
        }

        protected async Task CriarDadosBase(CompensacaoDeAusenciaDBDto dtoDB)
        {
            await CriarDreUePerfilComponenteCurricular();

            CriarClaimUsuario(dtoDB.Perfil);

            await CriarUsuarios();

            await CriarTurmaTipoCalendario(dtoDB);

            await CriarAula(dtoDB);

            if (dtoDB.CriarPeriodoEscolar)
                await CriarPeriodoEscolar();

            if (dtoDB.CriarPeriodoAbertura)
                await CriarPeriodoReabertura(dtoDB.TipoCalendarioId);

            await CriarAbrangencia(dtoDB.Perfil);

            await CriarParametrosSistema();
        }

        protected async Task CriarAula(CompensacaoDeAusenciaDBDto dtoDB)
        {
            await InserirNaBase(
                            new Dominio.Aula
                            {
                                UeId = UE_CODIGO_1,
                                DisciplinaId = dtoDB.ComponenteCurricular,
                                TurmaId = TURMA_CODIGO_1,
                                TipoCalendarioId = dtoDB.TipoCalendarioId,
                                ProfessorRf = USUARIO_PROFESSOR_LOGIN_2222222,
                                Quantidade = dtoDB.QuantidadeAula,
                                DataAula = dtoDB.DataReferencia.GetValueOrDefault(),
                                RecorrenciaAula = RecorrenciaAula.AulaUnica,
                                TipoAula = TipoAula.Normal,
                                CriadoEm = DateTime.Now,
                                CriadoPor = SISTEMA_NOME,
                                CriadoRF = SISTEMA_CODIGO_RF,
                                AulaCJ = dtoDB.AulaCj
                            });
        }

        protected CompensacaoAusenciaDto ObtenhaCompensacaoAusenciaDto(
                                    CompensacaoDeAusenciaDBDto dtoDadoBase,
                                    List<CompensacaoAusenciaAlunoDto> listaAlunos,
                                    List<string> listaDisciplinaRegente = null)
        {
            return new CompensacaoAusenciaDto()
            {
                TurmaId = TURMA_CODIGO_1,
                Alunos = listaAlunos,
                Bimestre = dtoDadoBase.Bimestre,
                Atividade = ATIVIDADE_COMPENSACAO,
                Descricao = DESCRICAO_COMPENSACAO,
                DisciplinaId = dtoDadoBase.ComponenteCurricular,
                DisciplinasRegenciaIds = listaDisciplinaRegente
            };
        }

        protected async Task CriaFrequenciaAluno(
                                CompensacaoDeAusenciaDBDto dbDto,
                                DateTime periodoInicio,
                                DateTime periodoFim,
                                string codigoAluno,
                                int totalPresenca,
                                int totalAusencia,
                                long PeriodoEscolarId)
        {
            await InserirNaBase(new Dominio.FrequenciaAluno
            {
                PeriodoInicio = periodoInicio,
                PeriodoFim = periodoFim,
                Bimestre = dbDto.Bimestre,
                TotalAulas = dbDto.QuantidadeAula,
                TotalAusencias = totalAusencia,
                TotalCompensacoes = 0,
                PeriodoEscolarId = PeriodoEscolarId,
                TotalPresencas = totalPresenca,
                TotalRemotos = 0,
                DisciplinaId = dbDto.ComponenteCurricular,
                CodigoAluno = codigoAluno,
                TurmaId = TURMA_CODIGO_1,
                Tipo = TipoFrequenciaAluno.PorDisciplina,
                CriadoEm = DateTimeExtension.HorarioBrasilia(),
                CriadoPor = SISTEMA_NOME,
                CriadoRF = SISTEMA_CODIGO_RF,

            });
        }
        protected class CompensacaoDeAusenciaDBDto
        {
            public CompensacaoDeAusenciaDBDto()
            {
                CriarPeriodoEscolar = true;
                TipoCalendarioId = TIPO_CALENDARIO_1;
                CriarPeriodoAbertura = true;
            }
            public DateTime? DataReferencia { get; set; }
            public string Perfil { get; set; }
            public Modalidade Modalidade { get; set; }
            public ModalidadeTipoCalendario TipoCalendario { get; set; }
            public int Bimestre { get; set; }
            public string ComponenteCurricular { get; set; }
            public long TipoCalendarioId { get; set; }
            public bool CriarPeriodoEscolar { get; set; }
            public bool CriarPeriodoAbertura { get; set; }
            public string AnoTurma { get; set; }
            public int QuantidadeAula { get; set; }
            public bool AulaCj { get; set; }
        }

        private async Task CriarTurmaTipoCalendario(CompensacaoDeAusenciaDBDto dtoDB)
        {
            await CriarTipoCalendario(dtoDB.TipoCalendario);
            await CriarTurma(dtoDB.Modalidade, dtoDB.AnoTurma);
        }
        private async Task CriarPeriodoEscolar()
        {
            await CriarPeriodoEscolar(DATA_03_01_INICIO_BIMESTRE_1, DATA_29_04_FIM_BIMESTRE_1, BIMESTRE_1, TIPO_CALENDARIO_1);

            await CriarPeriodoEscolar(DATA_02_05_INICIO_BIMESTRE_2, DATA_08_07_FIM_BIMESTRE_2, BIMESTRE_2, TIPO_CALENDARIO_1);

            await CriarPeriodoEscolar(DATA_25_07_INICIO_BIMESTRE_3, DATA_30_09_FIM_BIMESTRE_3, BIMESTRE_3, TIPO_CALENDARIO_1);

            await CriarPeriodoEscolar(DATA_03_10_INICIO_BIMESTRE_4, DATA_22_12_FIM_BIMESTRE_4, BIMESTRE_4, TIPO_CALENDARIO_1);
        }

        private async Task CriarAbrangencia(string perfil)
        {
            await InserirNaBase(new Abrangencia()
            {
                DreId = DRE_ID_1,
                Historico = false,
                Perfil = new Guid(perfil),
                TurmaId = TURMA_ID_1,
                UeId = UE_ID_1,
                UsuarioId = USUARIO_ID_1
            });
        }

        private async Task CriarParametrosSistema()
        {
            await InserirNaBase(new ParametrosSistema
            {
                Nome = "PermiteCompensacaoForaPeriodo",
                Tipo = TipoParametroSistema.PermiteCompensacaoForaPeriodo,
                Descricao = "Permite compensação fora do periodo",
                Valor = string.Empty,
                Ano = DateTimeExtension.HorarioBrasilia().Year,
                CriadoEm = DateTimeExtension.HorarioBrasilia(),
                CriadoPor = SISTEMA_NOME,
                CriadoRF = SISTEMA_CODIGO_RF,
                Ativo = true
            });
        }
    }
}
