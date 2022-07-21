﻿using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using SME.SGP.Aplicacao;
using SME.SGP.Dominio;
using SME.SGP.Infra;
using SME.SGP.TesteIntegracao.Nota.ServicosFakes;
using SME.SGP.TesteIntegracao.ServicosFakes;
using SME.SGP.TesteIntegracao.Setup;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SME.SGP.TesteIntegracao.NotaFechamentoBimestre
{
    public class NotaFechamentoBimestreTesteBase : TesteBaseComuns
    {
        private const string PARAMETRO_APROVACAO_ALTERACAO_NOTA_FECHAMENTO_NOME = "AprovacaoAlteracaoNotaFechamento";
        private const string PARAMETRO_APROVACAO_ALTERACAO_NOTA_FECHAMENTO_DESCRICAO = "Solicita aprovação nas alterações de notas de fechamento";
        private const string PARAMETRO_COMPENSACAO_AUSENCIA_PERCENTUAL_REGENCIA_CLASSE_NOME = "CompensacaoAusenciaPercentualRegenciaClasse";
        private const string PARAMETRO_COMPENSACAO_AUSENCIA_PERCENTUAL_REGENCIA_CLASSE_DESCRICAO = "Percentual de frequência onde a compensação de ausência considera abaixo do limite para regência de classe";
        private const string PARAMETRO_COMPENSACAO_AUSENCIA_PERCENTUAL_REGENCIA_CLASSE_VALOR_75 = "75";
        private const string PARAMETRO_COMPENSACAO_AUSENCIA_PERCENTUAL_FUND2_NOME = "CompensacaoAusenciaPercentualFund2";
        private const string PARAMETRO_COMPENSACAO_AUSENCIA_PERCENTUAL_FUND2_DESCRICAO = "Percentual de frequência onde a compensação de ausência considera abaixo do limite para Fund2";
        private const string PARAMETRO_COMPENSACAO_AUSENCIA_PERCENTUAL_FUND2_VALOR_50 = "50";
        private const string PARAMETRO_QUANTIDADE_DIAS_ALTERACAO_NOTA_FINAL_NOME = "QuantidadeDiasAlteracaoNotaFinal";
        private const string PARAMETRO_QUANTIDADE_DIAS_ALTERACAO_NOTA_FINAL_DESCRICAO = "Quantidade de dias para gerar notificação caso a nota final seja alterada";
        private const string PARAMETRO_QUANTIDADE_DIAS_ALTERACAO_NOTA_FINAL_VALOR_30 = "30";
        private const string PARAMETRO_MEDIA_BIMESTRE_NOME = "MediaBimestre";
        private const string PARAMETRO_MEDIA_BIMESTRE_DESCRICAO = "Média final para aprovação no bimestre";
        private const string PARAMETRO_MEDIA_BIMESTRE_VALOR_5 = "5";

        public NotaFechamentoBimestreTesteBase(CollectionFixture collectionFixture) : base(collectionFixture)
        {
        }

        protected override void RegistrarFakes(IServiceCollection services)
        {
            base.RegistrarFakes(services);
            services.Replace(new ServiceDescriptor(typeof(IRequestHandler<ObterUsuarioPossuiPermissaoNaTurmaEDisciplinaQuery, bool>), typeof(ObterUsuarioPossuiPermissaoNaTurmaEDisciplinaQueryHandlerFake), ServiceLifetime.Scoped));
            services.Replace(new ServiceDescriptor(typeof(IRequestHandler<ObterAlunosEolPorTurmaQuery, IEnumerable<AlunoPorTurmaResposta>>), typeof(ObterAlunosEolPorTurmaQueryHandlerFake), ServiceLifetime.Scoped));
        }

        protected class FiltroFechamentoNotaDto
        {
            public string Perfil { get; set; }
            public ModalidadeTipoCalendario TipoCalendario { get; set; }
            public bool ConsiderarAnoAnterior { get; set; }
            public Modalidade Modalidade { get; set; }
            public string AnoTurma { get; set; }
            public TipoFrequenciaAluno TipoFrequenciaAluno { get; set; }
        }

        protected async Task CriarDadosBase(FiltroFechamentoNotaDto filtroFechamentoNota)
        {
            await CriarDreUePerfilComponenteCurricular();

            CriarClaimUsuario(filtroFechamentoNota.Perfil);
            await CriarUsuarios();

            await CriarTipoCalendario(filtroFechamentoNota.TipoCalendario, filtroFechamentoNota.ConsiderarAnoAnterior);
            await CriarTurma(filtroFechamentoNota.Modalidade, filtroFechamentoNota.AnoTurma, filtroFechamentoNota.ConsiderarAnoAnterior);

            await CriarParametrosNotaFechamento();

            await CriarPeriodoEscolar(filtroFechamentoNota.ConsiderarAnoAnterior);

            await CriarPeriodoFechamento();

            await CriarFrequenciaAluno(filtroFechamentoNota.TipoFrequenciaAluno);
            await CriarSintese();
        }

        private async Task CriarPeriodoEscolar(bool considerarAnoAnterior = false)
        {
            await CriarPeriodoEscolar(DATA_03_01_INICIO_BIMESTRE_1, DATA_29_04_FIM_BIMESTRE_1, BIMESTRE_1, TIPO_CALENDARIO_1, considerarAnoAnterior);
            await CriarPeriodoEscolar(DATA_02_05_INICIO_BIMESTRE_2, DATA_08_07_FIM_BIMESTRE_2, BIMESTRE_2, TIPO_CALENDARIO_1, considerarAnoAnterior);
            await CriarPeriodoEscolar(DATA_25_07_INICIO_BIMESTRE_3, DATA_30_09_FIM_BIMESTRE_3, BIMESTRE_3, TIPO_CALENDARIO_1, considerarAnoAnterior);
            await CriarPeriodoEscolar(DATA_03_10_INICIO_BIMESTRE_4, DATA_22_12_FIM_BIMESTRE_4, BIMESTRE_4, TIPO_CALENDARIO_1, considerarAnoAnterior);
        }

        private async Task CriarParametrosNotaFechamento()
        {
            await InserirNaBase(new ParametrosSistema
            {
                Nome = PARAMETRO_APROVACAO_ALTERACAO_NOTA_FECHAMENTO_NOME,
                Tipo = TipoParametroSistema.AprovacaoAlteracaoNotaFechamento,
                Descricao = PARAMETRO_APROVACAO_ALTERACAO_NOTA_FECHAMENTO_DESCRICAO,
                Valor = "",
                Ano = DateTimeExtension.HorarioBrasilia().Year,
                CriadoEm = DateTimeExtension.HorarioBrasilia(),
                CriadoPor = SISTEMA_NOME,
                CriadoRF = SISTEMA_CODIGO_RF,
                Ativo = true
            });

            await InserirNaBase(new ParametrosSistema
            {
                Nome = PARAMETRO_COMPENSACAO_AUSENCIA_PERCENTUAL_REGENCIA_CLASSE_NOME,
                Tipo = TipoParametroSistema.CompensacaoAusenciaPercentualRegenciaClasse,
                Descricao = PARAMETRO_COMPENSACAO_AUSENCIA_PERCENTUAL_REGENCIA_CLASSE_DESCRICAO,
                Valor = PARAMETRO_COMPENSACAO_AUSENCIA_PERCENTUAL_REGENCIA_CLASSE_VALOR_75,
                Ano = DateTimeExtension.HorarioBrasilia().Year,
                CriadoEm = DateTimeExtension.HorarioBrasilia(),
                CriadoPor = SISTEMA_NOME,
                CriadoRF = SISTEMA_CODIGO_RF,
                Ativo = true
            });

            await InserirNaBase(new ParametrosSistema
            {
                Nome = PARAMETRO_COMPENSACAO_AUSENCIA_PERCENTUAL_FUND2_NOME,
                Tipo = TipoParametroSistema.CompensacaoAusenciaPercentualFund2,
                Descricao = PARAMETRO_COMPENSACAO_AUSENCIA_PERCENTUAL_FUND2_DESCRICAO,
                Valor = PARAMETRO_COMPENSACAO_AUSENCIA_PERCENTUAL_FUND2_VALOR_50,
                Ano = DateTimeExtension.HorarioBrasilia().Year,
                CriadoEm = DateTimeExtension.HorarioBrasilia(),
                CriadoPor = SISTEMA_NOME,
                CriadoRF = SISTEMA_CODIGO_RF,
                Ativo = true
            });

            await InserirNaBase(new ParametrosSistema
            {
                Nome = PARAMETRO_QUANTIDADE_DIAS_ALTERACAO_NOTA_FINAL_NOME,
                Tipo = TipoParametroSistema.QuantidadeDiasAlteracaoNotaFinal,
                Descricao = PARAMETRO_QUANTIDADE_DIAS_ALTERACAO_NOTA_FINAL_DESCRICAO,
                Valor = PARAMETRO_QUANTIDADE_DIAS_ALTERACAO_NOTA_FINAL_VALOR_30,
                Ano = DateTimeExtension.HorarioBrasilia().Year,
                CriadoEm = DateTimeExtension.HorarioBrasilia(),
                CriadoPor = SISTEMA_NOME,
                CriadoRF = SISTEMA_CODIGO_RF,
                Ativo = true
            });

            await InserirNaBase(new ParametrosSistema
            {
                Nome = PARAMETRO_MEDIA_BIMESTRE_NOME,
                Tipo = TipoParametroSistema.MediaBimestre,
                Descricao = PARAMETRO_MEDIA_BIMESTRE_DESCRICAO,
                Valor = PARAMETRO_MEDIA_BIMESTRE_VALOR_5,
                Ano = DateTimeExtension.HorarioBrasilia().Year,
                CriadoEm = DateTimeExtension.HorarioBrasilia(),
                CriadoPor = SISTEMA_NOME,
                CriadoRF = SISTEMA_CODIGO_RF,
                Ativo = true
            });
        }

        private async Task CriarFrequenciaAluno(TipoFrequenciaAluno tipoFrequenciaAluno)
        {
            await InserirNaBase(new Dominio.FrequenciaAluno
            {
                Id = 1,
                CodigoAluno = CODIGO_ALUNO_1,
                Tipo = tipoFrequenciaAluno,
                DisciplinaId = COMPONENTE_CURRICULAR_PORTUGUES_ID_138.ToString(),
                PeriodoInicio = new DateTime(DateTimeExtension.HorarioBrasilia().Year, 02, 05),
                PeriodoFim = new DateTime(DateTimeExtension.HorarioBrasilia().Year, 04, 30),
                Bimestre = BIMESTRE_1,
                TotalAulas = 20,
                TotalCompensacoes = 4,
                CriadoEm = DateTimeExtension.HorarioBrasilia(),
                CriadoPor = SISTEMA_NOME,
                CriadoRF = SISTEMA_CODIGO_RF,
                TurmaId = TURMA_CODIGO_1,
                PeriodoEscolarId = 1,
                TotalPresencas = 16,
                TotalRemotos = 0
            });

            await InserirNaBase(new Dominio.FrequenciaAluno
            {
                Id = 2,
                CodigoAluno = CODIGO_ALUNO_2,
                Tipo = tipoFrequenciaAluno,
                DisciplinaId = COMPONENTE_CURRICULAR_PORTUGUES_ID_138.ToString(),
                PeriodoInicio = new DateTime(DateTimeExtension.HorarioBrasilia().Year, 02, 05),
                PeriodoFim = new DateTime(DateTimeExtension.HorarioBrasilia().Year, 04, 30),
                Bimestre = BIMESTRE_1,
                TotalAulas = 20,
                TotalCompensacoes = 1,
                CriadoEm = DateTimeExtension.HorarioBrasilia(),
                CriadoPor = SISTEMA_NOME,
                CriadoRF = SISTEMA_CODIGO_RF,
                TurmaId = TURMA_CODIGO_1,
                PeriodoEscolarId = 1,
                TotalPresencas = 19,
                TotalRemotos = 0
            });

            await InserirNaBase(new Dominio.FrequenciaAluno
            {
                Id = 3,
                CodigoAluno = CODIGO_ALUNO_3,
                Tipo = tipoFrequenciaAluno,
                DisciplinaId = COMPONENTE_CURRICULAR_PORTUGUES_ID_138.ToString(),
                PeriodoInicio = new DateTime(DateTimeExtension.HorarioBrasilia().Year, 02, 05),
                PeriodoFim = new DateTime(DateTimeExtension.HorarioBrasilia().Year, 04, 30),
                Bimestre = BIMESTRE_1,
                TotalAulas = 20,
                TotalCompensacoes = 5,
                CriadoEm = DateTimeExtension.HorarioBrasilia(),
                CriadoPor = SISTEMA_NOME,
                CriadoRF = SISTEMA_CODIGO_RF,
                TurmaId = TURMA_CODIGO_1,
                PeriodoEscolarId = 1,
                TotalPresencas = 15,
                TotalRemotos = 0
            });

            await InserirNaBase(new Dominio.FrequenciaAluno
            {
                Id = 4,
                CodigoAluno = CODIGO_ALUNO_4,
                Tipo = tipoFrequenciaAluno,
                DisciplinaId = COMPONENTE_CURRICULAR_PORTUGUES_ID_138.ToString(),
                PeriodoInicio = new DateTime(DateTimeExtension.HorarioBrasilia().Year, 02, 05),
                PeriodoFim = new DateTime(DateTimeExtension.HorarioBrasilia().Year, 04, 30),
                Bimestre = BIMESTRE_1,
                TotalAulas = 20,
                TotalCompensacoes = 0,
                CriadoEm = DateTimeExtension.HorarioBrasilia(),
                CriadoPor = SISTEMA_NOME,
                CriadoRF = SISTEMA_CODIGO_RF,
                TurmaId = TURMA_CODIGO_1,
                PeriodoEscolarId = 1,
                TotalPresencas = 20,
                TotalRemotos = 0
            });
        }

        private async Task CriarPeriodoFechamento()
        {
            var periodoFechamento = new PeriodoFechamento
            {
                Id = 1,
                Migrado = false,
                CriadoEm = DateTimeExtension.HorarioBrasilia(),
                CriadoPor = SISTEMA_NOME,
                CriadoRF = SISTEMA_CODIGO_RF
            };

            var periodosEscolares = ObterTodos<PeriodoEscolar>();

            foreach (var periodoEscolar in periodosEscolares)
                periodoFechamento.AdicionarFechamentoBimestre(new PeriodoFechamentoBimestre(1, periodoEscolar, periodoEscolar.PeriodoFim, periodoEscolar.PeriodoFim.AddDays(10)));

            await InserirNaBase(periodoFechamento);
        }    

        private async Task CriarSintese()
        {
            await InserirNaBase(new Sintese()
            {
                AlteradoEm = DateTimeExtension.HorarioBrasilia(),
                Aprovado = true,
                Ativo = true,
                CriadoEm = DateTimeExtension.HorarioBrasilia(),
                CriadoPor = SISTEMA_NOME,
                CriadoRF = SISTEMA_CODIGO_RF,
                Descricao = "",
                FimVigencia = DateTimeExtension.HorarioBrasilia().AddDays(1),
                Id = 1,
                InicioVigencia = DateTimeExtension.HorarioBrasilia(),
                Valor = SinteseEnum.Frequente.Name()
            });
        }
    }
}
