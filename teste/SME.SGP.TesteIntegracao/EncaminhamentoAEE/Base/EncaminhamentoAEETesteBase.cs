﻿using SME.SGP.Dominio;
using SME.SGP.TesteIntegracao.Setup;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using SME.SGP.Aplicacao.Interfaces;
using SME.SGP.Aplicacao.Interfaces.CasosDeUso;
using SME.SGP.Dominio.Entidades;

namespace SME.SGP.TesteIntegracao.EncaminhamentoAee
{
    public class EncaminhamentoAEETesteBase : TesteBaseComuns
    {
        public EncaminhamentoAEETesteBase(CollectionFixture collectionFixture) : base(collectionFixture)
        {
        }

        protected async Task CriarDadosBase(FiltroAEEDto filtro)
        {
            await CriarDreUePerfilComponenteCurricular();

            CriarClaimUsuario(filtro.Perfil);

            await CriarUsuarios();

            await CriarTurmaTipoCalendario(filtro);

            if (filtro.CriarPeriodoEscolar)
                await CriarPeriodoEscolar(filtro.ConsiderarAnoAnterior);

            await CriarQuestionario();

            await CriarQuestoes();

            await CriarRespostas();
        }

        protected async Task CriarTurmaTipoCalendario(FiltroAEEDto filtro)
        {
            await CriarTipoCalendario(filtro.TipoCalendario, filtro.ConsiderarAnoAnterior);
            await CriarTurma(filtro.Modalidade, filtro.AnoTurma, filtro.ConsiderarAnoAnterior);
        }

        protected async Task CriarPeriodoEscolar(bool considerarAnoAnterior = false)
        {
            await CriarPeriodoEscolar(DATA_03_01_INICIO_BIMESTRE_1, DATA_29_04_FIM_BIMESTRE_1, BIMESTRE_1, TIPO_CALENDARIO_1, considerarAnoAnterior);

            await CriarPeriodoEscolar(DATA_02_05_INICIO_BIMESTRE_2, DATA_08_07_FIM_BIMESTRE_2, BIMESTRE_2, TIPO_CALENDARIO_1, considerarAnoAnterior);

            await CriarPeriodoEscolar(DATA_25_07_INICIO_BIMESTRE_3, DATA_30_09_FIM_BIMESTRE_3, BIMESTRE_3, TIPO_CALENDARIO_1, considerarAnoAnterior);

            await CriarPeriodoEscolar(DATA_03_10_INICIO_BIMESTRE_4, DATA_22_12_FIM_BIMESTRE_4, BIMESTRE_4, TIPO_CALENDARIO_1, considerarAnoAnterior);
        }

        protected IObterEncaminhamentosAEEUseCase ObterServicoListagemComFiltros()
        {
            return ServiceProvider.GetService<IObterEncaminhamentosAEEUseCase>();    
        }

        protected IRegistrarEncaminhamentoAEEUseCase RetornarUseCaseRegistrarEncaminhamento()
        {
            return ServiceProvider.GetService<IRegistrarEncaminhamentoAEEUseCase>();
        }
        
        protected IVerificaPodeCadastrarEncaminhamentoAEEParaEstudanteUseCase ObterServicoPodeCadastrarEncaminhamentoAee()
        {
            return ServiceProvider.GetService<IVerificaPodeCadastrarEncaminhamentoAEEParaEstudanteUseCase>();
        }

        private async Task CriarRespostasComplementares()
        {
            await InserirNaBase(new OpcaoQuestaoComplementar()
            {
                OpcaoRespostaId = 1,
                QuestaoComplementarId = 15,
                CriadoPor = SISTEMA_NOME,
                CriadoRF = SISTEMA_CODIGO_RF,
                CriadoEm = DateTime.Now
            });

            await InserirNaBase(new OpcaoQuestaoComplementar()
            {
                OpcaoRespostaId = 2,
                QuestaoComplementarId = 15,
                CriadoPor = SISTEMA_NOME,
                CriadoRF = SISTEMA_CODIGO_RF,
                CriadoEm = DateTime.Now
            });

            await InserirNaBase(new OpcaoQuestaoComplementar()
            {
                OpcaoRespostaId = 3,
                QuestaoComplementarId = 15,
                CriadoPor = SISTEMA_NOME,
                CriadoRF = SISTEMA_CODIGO_RF,
                CriadoEm = DateTime.Now
            });

            await InserirNaBase(new OpcaoQuestaoComplementar()
            {
                OpcaoRespostaId = 8,
                QuestaoComplementarId = 17,
                CriadoPor = SISTEMA_NOME,
                CriadoRF = SISTEMA_CODIGO_RF,
                CriadoEm = DateTime.Now
            });
          
            await InserirNaBase(new OpcaoQuestaoComplementar()
            {
                OpcaoRespostaId = 11,
                QuestaoComplementarId = 18,
                CriadoPor = SISTEMA_NOME,
                CriadoRF = SISTEMA_CODIGO_RF,
                CriadoEm = DateTime.Now
            });
            
            await InserirNaBase(new OpcaoQuestaoComplementar()
            {
                OpcaoRespostaId = 21,
                QuestaoComplementarId = 19,
                CriadoPor = SISTEMA_NOME,
                CriadoRF = SISTEMA_CODIGO_RF,
                CriadoEm = DateTime.Now
            });
        }

        private async Task CriarRespostas()
        {
            //1
            await InserirNaBase(new OpcaoResposta()
            {
                QuestaoId = 2,
                Ordem = 1,
                Nome = "Sim",
                CriadoPor = SISTEMA_NOME,
                CriadoRF = SISTEMA_CODIGO_RF,
                CriadoEm = DateTime.Now
            });
            //2
            await InserirNaBase(new OpcaoResposta()
            {
                QuestaoId = 2,
                Ordem = 2,
                Nome = "Não",
                CriadoPor = SISTEMA_NOME,
                CriadoRF = SISTEMA_CODIGO_RF,
                CriadoEm = DateTime.Now
            });
            //3
            await InserirNaBase(new OpcaoResposta()
            {
                QuestaoId = 2,
                Ordem = 3,
                Nome = "Não sei",
                CriadoPor = SISTEMA_NOME,
                CriadoRF = SISTEMA_CODIGO_RF,
                CriadoEm = DateTime.Now
            });
            //4
            await InserirNaBase(new OpcaoResposta()
            {
                QuestaoId = 6,
                Ordem = 1,
                Nome = "Leitura",
                CriadoPor = SISTEMA_NOME,
                CriadoRF = SISTEMA_CODIGO_RF,
                CriadoEm = DateTime.Now
            });
            //5
            await InserirNaBase(new OpcaoResposta()
            {
                QuestaoId = 6,
                Ordem = 2,
                Nome = "Escrita",
                CriadoPor = SISTEMA_NOME,
                CriadoRF = SISTEMA_CODIGO_RF,
                CriadoEm = DateTime.Now
            });
            //6
            await InserirNaBase(new OpcaoResposta()
            {
                QuestaoId = 6,
                Ordem = 3,
                Nome = "Atividades em grupo",
                CriadoPor = SISTEMA_NOME,
                CriadoRF = SISTEMA_CODIGO_RF,
                CriadoEm = DateTime.Now
            });
            //7
            await InserirNaBase(new OpcaoResposta()
            {
                QuestaoId = 6,
                Ordem = 4,
                Nome = "Outros",
                CriadoPor = SISTEMA_NOME,
                CriadoRF = SISTEMA_CODIGO_RF,
                CriadoEm = DateTime.Now
            });
            //8
            await InserirNaBase(new OpcaoResposta()
            {
                QuestaoId = 8,
                Ordem = 1,
                Nome = "Sim",
                CriadoPor = SISTEMA_NOME,
                CriadoRF = SISTEMA_CODIGO_RF,
                CriadoEm = DateTime.Now
            });
            //9
            await InserirNaBase(new OpcaoResposta()
            {
                QuestaoId = 8,
                Ordem = 2,
                Nome = "Não",
                CriadoPor = SISTEMA_NOME,
                CriadoRF = SISTEMA_CODIGO_RF,
                CriadoEm = DateTime.Now
            });
            //10
            await InserirNaBase(new OpcaoResposta()
            {
                QuestaoId = 8,
                Ordem = 3,
                Nome = "Não Sei",
                CriadoPor = SISTEMA_NOME,
                CriadoRF = SISTEMA_CODIGO_RF,
                CriadoEm = DateTime.Now
            });
            //11
            await InserirNaBase(new OpcaoResposta()
            {
                QuestaoId = 10,
                Ordem = 1,
                Nome = "Sim",
                CriadoPor = SISTEMA_NOME,
                CriadoRF = SISTEMA_CODIGO_RF,
                CriadoEm = DateTime.Now
            });
            //12
            await InserirNaBase(new OpcaoResposta()
            {
                QuestaoId = 10,
                Ordem = 2,
                Nome = "Não",
                CriadoPor = SISTEMA_NOME,
                CriadoRF = SISTEMA_CODIGO_RF,
                CriadoEm = DateTime.Now
            });
            //13
            await InserirNaBase(new OpcaoResposta()
            {
                QuestaoId = 10,
                Ordem = 2,
                Nome = "Não Sei",
                CriadoPor = SISTEMA_NOME,
                CriadoRF = SISTEMA_CODIGO_RF,
                CriadoEm = DateTime.Now
            });
            //14 
            await InserirNaBase(new OpcaoResposta()
            {
                QuestaoId = 18,
                Ordem = 1,
                Nome = "PAP",
                CriadoPor = SISTEMA_NOME,
                CriadoRF = SISTEMA_CODIGO_RF,
                CriadoEm = DateTime.Now
            });
            //15
            await InserirNaBase(new OpcaoResposta()
            {
                QuestaoId = 18,
                Ordem = 2,
                Nome = "SRM",
                CriadoPor = SISTEMA_NOME,
                CriadoRF = SISTEMA_CODIGO_RF,
                CriadoEm = DateTime.Now
            });
            //16
            await InserirNaBase(new OpcaoResposta()
            {
                QuestaoId = 18,
                Ordem = 3,
                Nome = "Mais educação (São Paulo Integral)",
                CriadoPor = SISTEMA_NOME,
                CriadoRF = SISTEMA_CODIGO_RF,
                CriadoEm = DateTime.Now
            });
            //17
            await InserirNaBase(new OpcaoResposta()
            {
                QuestaoId = 18,
                Ordem = 4,
                Nome = "Imprensa Jovem",
                CriadoPor = SISTEMA_NOME,
                CriadoRF = SISTEMA_CODIGO_RF,
                CriadoEm = DateTime.Now
            });
            //18
            await InserirNaBase(new OpcaoResposta()
            {
                QuestaoId = 18,
                Ordem = 5,
                Nome = "Academia Estudantil de Letras (AEL)",
                CriadoPor = SISTEMA_NOME,
                CriadoRF = SISTEMA_CODIGO_RF,
                CriadoEm = DateTime.Now
            });
            //19
            await InserirNaBase(new OpcaoResposta()
            {
                QuestaoId = 18,
                Ordem = 6,
                Nome = "Xadrez",
                CriadoPor = SISTEMA_NOME,
                CriadoRF = SISTEMA_CODIGO_RF,
                CriadoEm = DateTime.Now
            });
            //20
            await InserirNaBase(new OpcaoResposta()
            {
                QuestaoId = 18,
                Ordem = 7,
                Nome = "Robótica",
                CriadoPor = SISTEMA_NOME,
                CriadoRF = SISTEMA_CODIGO_RF,
                CriadoEm = DateTime.Now
            });
            //21
            await InserirNaBase(new OpcaoResposta()
            {
                QuestaoId = 18,
                Ordem = 8,
                Nome = "Outros",
                CriadoPor = SISTEMA_NOME,
                CriadoRF = SISTEMA_CODIGO_RF,
                CriadoEm = DateTime.Now
            });
        }

        private async Task CriarQuestionario()
        {
            await InserirNaBase(new Questionario()
            {
                Nome = "Questionário Encaminhamento AEE Etapa 1 Seção 2",
                Tipo = TipoQuestionario.EncaminhamentoAEE,
                CriadoPor = SISTEMA_NOME,
                CriadoRF = SISTEMA_CODIGO_RF,
                CriadoEm = DateTime.Now
            });
        }

        private async Task CriarQuestoes()
        {
            //1
            await InserirNaBase(new Questao()
            {
                QuestionarioId = 1,
                Ordem = 1,
                Nome = "Por qual(is) motivo(s) a sua unidade educacional está encaminhando o estudante ao Atendimento Educacional Especializado (AEE)?",
                SomenteLeitura = true,
                Obrigatorio = true,
                Tipo = TipoQuestao.Texto,
                CriadoPor = SISTEMA_NOME,
                CriadoRF = SISTEMA_CODIGO_RF,
                CriadoEm = DateTime.Now
            });
            //2
            await InserirNaBase(new Questao()
            {
                QuestionarioId = 1,
                Ordem = 2,
                Nome = "O estudante tem diagnóstico e/ou laudo?",
                Obrigatorio = true,
                Tipo = TipoQuestao.Radio,
                CriadoPor = SISTEMA_NOME,
                CriadoRF = SISTEMA_CODIGO_RF,
                CriadoEm = DateTime.Now
            });
            //3
            await InserirNaBase(new Questao()
            {
                QuestionarioId = 1,
                Ordem = 3,
                Nome = "Quais atividades escolares o estudante mais gosta de fazer?",
                Tipo = TipoQuestao.Texto,
                CriadoPor = SISTEMA_NOME,
                CriadoRF = SISTEMA_CODIGO_RF,
                CriadoEm = DateTime.Now
            });
            //4
            await InserirNaBase(new Questao()
            {
                QuestionarioId = 1,
                Ordem = 4,
                Nome = "O que o estudante faz que chama a sua atenção?",
                Obrigatorio = true,
                Tipo = TipoQuestao.Texto,
                CriadoPor = SISTEMA_NOME,
                CriadoRF = SISTEMA_CODIGO_RF,
                CriadoEm = DateTime.Now
            });
            //5
            await InserirNaBase(new Questao()
            {
                QuestionarioId = 1,
                Ordem = 1,
                Nome = "Justifique",
                Obrigatorio = true,
                Tipo = TipoQuestao.Texto,
                CriadoPor = SISTEMA_NOME,
                CriadoRF = SISTEMA_CODIGO_RF,
                CriadoEm = DateTime.Now
            });
            //6
            await InserirNaBase(new Questao()
            {
                QuestionarioId = 1,
                Ordem = 5,
                Nome = "Para o estudante, quais atividades escolares são mais difíceis?",
                Obrigatorio = true,
                Tipo = TipoQuestao.ComboMultiplaEscolha,
                CriadoPor = SISTEMA_NOME,
                CriadoRF = SISTEMA_CODIGO_RF,
                CriadoEm = DateTime.Now
            });
            //7
            await InserirNaBase(new Questao()
            {
                QuestionarioId = 1,
                Ordem = 6,
                Nome = "Diante das dificuldades apresentadas acima, quais estratégias pedagógicas foram feitas em sala de aula antes do encaminhamento ao AEE?",
                Obrigatorio = true,
                Tipo = TipoQuestao.Texto,
                CriadoPor = SISTEMA_NOME,
                CriadoRF = SISTEMA_CODIGO_RF,
                CriadoEm = DateTime.Now
            });
            //8
            await InserirNaBase(new Questao()
            {
                QuestionarioId = 1,
                Ordem = 7,
                Nome = "O estudante recebe algum atendimento clínico ou participa de outras atividades além da classe comum?",
                Obrigatorio = true,
                Tipo = TipoQuestao.Radio,
                CriadoPor = SISTEMA_NOME,
                CriadoRF = SISTEMA_CODIGO_RF,
                CriadoEm = DateTime.Now
            });
            //9
            await InserirNaBase(new Questao()
            {
                QuestionarioId = 1,
                Ordem = 1,
                Nome = "Justifique",
                Obrigatorio = true,
                Tipo = TipoQuestao.Texto,
                CriadoPor = SISTEMA_NOME,
                CriadoRF = SISTEMA_CODIGO_RF,
                CriadoEm = DateTime.Now
            });
            //10
            await InserirNaBase(new Questao()
            {
                QuestionarioId = 1,
                Ordem = 8,
                Nome = "Você acha que o estudante necessita de algum outro tipo de atendimento?",
                Obrigatorio = true,
                Tipo = TipoQuestao.Radio,
                CriadoPor = SISTEMA_NOME,
                CriadoRF = SISTEMA_CODIGO_RF,
                CriadoEm = DateTime.Now
            });
            //11
            await InserirNaBase(new Questao()
            {
                QuestionarioId = 1,
                Ordem = 9,
                Nome = "Quais informações relevantes a este encaminhamento foram levantadas junto à família do estudante?",
                Obrigatorio = true,
                Tipo = TipoQuestao.Texto,
                CriadoPor = SISTEMA_NOME,
                CriadoRF = SISTEMA_CODIGO_RF,
                CriadoEm = DateTime.Now
            });
            //12
            await InserirNaBase(new Questao()
            {
                QuestionarioId = 1,
                Ordem = 10,
                Nome = "Documentos relevantes a este encaminhamento",
                Obrigatorio = false,
                Tipo = TipoQuestao.Upload,
                CriadoPor = SISTEMA_NOME,
                CriadoRF = SISTEMA_CODIGO_RF,
                CriadoEm = DateTime.Now
            });
            //13
            await InserirNaBase(new Questao()
            {
                QuestionarioId = 1,
                Ordem = 11,
                Nome = "Observações adicionais (se necessário)",
                Obrigatorio = false,
                Tipo = TipoQuestao.Texto,
                CriadoPor = SISTEMA_NOME,
                CriadoRF = SISTEMA_CODIGO_RF,
                CriadoEm = DateTime.Now
            });
            //14
            await InserirNaBase(new Questao()
            {
                QuestionarioId = 1,
                Ordem = 0,
                Nome = String.Empty,
                Obrigatorio = false,
                Tipo = TipoQuestao.Upload,
                CriadoPor = SISTEMA_NOME,
                CriadoRF = SISTEMA_CODIGO_RF,
                CriadoEm = DateTime.Now
            });
            //15
            await InserirNaBase(new Questao()
            {
                QuestionarioId = 1,
                Ordem = 1,
                Nome = "Por quê?",
                Obrigatorio = false,
                Tipo = TipoQuestao.Texto,
                CriadoPor = SISTEMA_NOME,
                CriadoRF = SISTEMA_CODIGO_RF,
                CriadoEm = DateTime.Now
            });
            //16
            await InserirNaBase(new Questao()
            {
                QuestionarioId = 1,
                Ordem = 1,
                Nome = "Por quê?",
                Obrigatorio = true,
                Tipo = TipoQuestao.Texto,
                CriadoPor = SISTEMA_NOME,
                CriadoRF = SISTEMA_CODIGO_RF,
                CriadoEm = DateTime.Now
            });
            //17
            await InserirNaBase(new Questao()
            {
                QuestionarioId = 1,
                Ordem = 1,
                Nome = "Detalhamento de atendimento clínico",
                Obrigatorio = true,
                Tipo = TipoQuestao.AtendimentoClinico,
                CriadoPor = SISTEMA_NOME,
                CriadoRF = SISTEMA_CODIGO_RF,
                CriadoEm = DateTime.Now
            });
            //18
            await InserirNaBase(new Questao()
            {
                QuestionarioId = 1,
                Ordem = 1,
                Nome = "Selecione os tipos de atendimento",
                Obrigatorio = true,
                Tipo = TipoQuestao.ComboMultiplaEscolha,
                CriadoPor = SISTEMA_NOME,
                CriadoRF = SISTEMA_CODIGO_RF,
                CriadoEm = DateTime.Now
            });
            //19
            await InserirNaBase(new Questao()
            {
                QuestionarioId = 1,
                Ordem = 1,
                Nome = "Descreva o tipo de atendimento",
                Obrigatorio = true,
                Tipo = TipoQuestao.Texto,
                CriadoPor = SISTEMA_NOME,
                CriadoRF = SISTEMA_CODIGO_RF,
                CriadoEm = DateTime.Now
            });
            //20
            await InserirNaBase(new Questao()
            {
                QuestionarioId = 1,
                Ordem = 2,
                Nome = "Detalhe as atividades",
                Obrigatorio = true,
                Tipo = TipoQuestao.Texto,
                CriadoPor = SISTEMA_NOME,
                CriadoRF = SISTEMA_CODIGO_RF,
                CriadoEm = DateTime.Now
            });
        }
        protected class FiltroAEEDto
        {
            public FiltroAEEDto()
            {
                TipoCalendarioId = TIPO_CALENDARIO_1;
                ConsiderarAnoAnterior = false;
                CriarPeriodoEscolar = true;
            }
            public DateTime? DataReferencia { get; set; }
            public string Perfil { get; set; }
            public Modalidade Modalidade { get; set; }
            public ModalidadeTipoCalendario TipoCalendario { get; set; }
            public int Bimestre { get; set; }
            public string ComponenteCurricular { get; set; }
            public long TipoCalendarioId { get; set; }
            public bool CriarPeriodoEscolar { get; set; }
            public string AnoTurma { get; set; }
            public bool ConsiderarAnoAnterior { get; set; }
            public string ProfessorRf { get; set; }
        }
    }
}
