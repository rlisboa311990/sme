using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Humanizer;
using Shouldly;
using SME.SGP.Aplicacao;
using SME.SGP.Dominio;
using SME.SGP.Dominio.Constantes.MensagensNegocio;
using SME.SGP.Dominio.Enumerados;
using SME.SGP.Infra;
using SME.SGP.TesteIntegracao.Setup;
using Xunit;

namespace SME.SGP.TesteIntegracao.EncaminhamentoNAAPA
{
    public class Ao_cadastrar_editar_encaminhamento_naapa_aguardandoatendimento : EncaminhamentoNAAPATesteBase
    {
        
        public Ao_cadastrar_editar_encaminhamento_naapa_aguardandoatendimento(CollectionFixture collectionFixture) : base(collectionFixture)
        { }


        [Fact(DisplayName = "Encaminhamento NAAPA - Alterar encaminhamento NAAPA para Aguardando Atendimento (observa��o obrigat�ria n�o preenchida)")]
        public async Task Ao_editar_encaminhamento_para_aguardandoatendimento_consistir_observacao_obrigatoria_nao_preenchida()
        {
            var filtroNAAPA = new FiltroNAAPADto()
            {
                Perfil = ObterPerfilCP(),
                TipoCalendario = ModalidadeTipoCalendario.FundamentalMedio,
                Modalidade = Modalidade.Fundamental,
                AnoTurma = "8",
                DreId = 1,
                CodigoUe = "1",
                TurmaId = TURMA_ID_1,
                Situacao = (int)SituacaoNAAPA.Rascunho,
                Prioridade = NORMAL
            };

            await CriarDadosBase(filtroNAAPA);

            var registrarEncaminhamentoNaapaUseCase = ObterServicoRegistrarEncaminhamento();

            var dataAtual = DateTimeExtension.HorarioBrasilia().Date;
            var dataQueixa = new DateTime(dataAtual.Year, 11, 18);
            
            await GerarDadosEncaminhamentoNAAPA(dataQueixa);

            dataQueixa.AddDays(4);
            
            var encaminhamentosNaapaDto = new EncaminhamentoNAAPADto()
            {
                Id = 1,
                TurmaId = TURMA_ID_1,
                Situacao = SituacaoNAAPA.AguardandoAtendimento,
                AlunoCodigo = ALUNO_CODIGO_1,
                AlunoNome = "Nome do aluno do naapa",
                Secoes = new List<EncaminhamentoNAAPASecaoDto>()
                {
                    new ()
                    {
                        SecaoId = 2,
                        Questoes = new List<EncaminhamentoNAAPASecaoQuestaoDto>()
                        {
                            new ()
                            {
                                QuestaoId = ID_QUESTAO_AGRUPAMENTO_PROMOCAO_CUIDADOS,
                                Resposta = ID_OPCAO_RESPOSTA_DOENCA_CRONICA.ToString(),
                                TipoQuestao = TipoQuestao.ComboMultiplaEscolha,

                            },
                            new ()
                            {
                                QuestaoId = ID_QUESTAO_AGRUPAMENTO_PROMOCAO_CUIDADOS,
                                Resposta = ID_OPCAO_RESPOSTA_ADOECE_COM_FREQUENCIA.ToString(),
                                TipoQuestao = TipoQuestao.ComboMultiplaEscolha,

                            },
                            new ()
                            {
                                QuestaoId = ID_QUESTAO_TIPO_ADOECE_COM_FREQUENCIA,
                                Resposta = ID_OPCAO_RESPOSTA_OUTRAS_QUESTAO_TIPO_ADOECE_COM_FREQUENCIA.ToString(),
                                TipoQuestao = TipoQuestao.ComboMultiplaEscolha,

                            },
                            new ()
                            {
                                QuestaoId = ID_QUESTAO_TIPO_DOENCA_CRONICA,
                                Resposta = ID_OPCAO_RESPOSTA_OUTRAS_QUESTAO_TIPO_DOENCA_CRONICA.ToString(),
                                TipoQuestao = TipoQuestao.ComboMultiplaEscolha,

                            }
                        }
                    }
                }
            };
            var excecao = await Assert.ThrowsAsync<NegocioException>(() => registrarEncaminhamentoNaapaUseCase.Executar(encaminhamentosNaapaDto));

            excecao.Message.ShouldBe(String.Format(MensagemNegocioEncaminhamentoNAAPA.EXISTEM_QUESTOES_OBRIGATORIAS_NAO_PREENCHIDAS, 
                                                    "Se��o: Quest�es apresentadas Quest�es: [5]"));
        }


        [Fact(DisplayName = "Encaminhamento NAAPA - Alterar encaminhamento NAAPA para Aguardando Atendimento (observa��o obrigat�ria preenchida)")]
        public async Task Ao_editar_encaminhamento_para_aguardandoatendimento_nao_consistir_observacao_obrigatoria_preenchida()
        {
            var filtroNAAPA = new FiltroNAAPADto()
            {
                Perfil = ObterPerfilCP(),
                TipoCalendario = ModalidadeTipoCalendario.FundamentalMedio,
                Modalidade = Modalidade.Fundamental,
                AnoTurma = "8",
                DreId = 1,
                CodigoUe = "1",
                TurmaId = TURMA_ID_1,
                Situacao = (int)SituacaoNAAPA.Rascunho,
                Prioridade = NORMAL
            };

            await CriarDadosBase(filtroNAAPA);

            var registrarEncaminhamentoNaapaUseCase = ObterServicoRegistrarEncaminhamento();

            var dataAtual = DateTimeExtension.HorarioBrasilia().Date;
            var dataQueixa = new DateTime(dataAtual.Year, 11, 18);

            await GerarDadosEncaminhamentoNAAPA(dataQueixa);

            dataQueixa.AddDays(4);

            var encaminhamentosNaapaDto = new EncaminhamentoNAAPADto()
            {
                Id = 1,
                TurmaId = TURMA_ID_1,
                Situacao = SituacaoNAAPA.AguardandoAtendimento,
                AlunoCodigo = ALUNO_CODIGO_1,
                AlunoNome = "Nome do aluno do naapa",
                Secoes = new List<EncaminhamentoNAAPASecaoDto>()
                {
                    new ()
                    {
                        SecaoId = 2,
                        Questoes = new List<EncaminhamentoNAAPASecaoQuestaoDto>()
                        {
                            new ()
                            {
                                QuestaoId = ID_QUESTAO_AGRUPAMENTO_PROMOCAO_CUIDADOS,
                                Resposta = ID_OPCAO_RESPOSTA_DOENCA_CRONICA.ToString(),
                                TipoQuestao = TipoQuestao.ComboMultiplaEscolha,

                            },
                            new ()
                            {
                                QuestaoId = ID_QUESTAO_AGRUPAMENTO_PROMOCAO_CUIDADOS,
                                Resposta = ID_OPCAO_RESPOSTA_ADOECE_COM_FREQUENCIA.ToString(),
                                TipoQuestao = TipoQuestao.ComboMultiplaEscolha,

                            },
                            new ()
                            {
                                QuestaoId = ID_QUESTAO_TIPO_ADOECE_COM_FREQUENCIA,
                                Resposta = ID_OPCAO_RESPOSTA_OUTRAS_QUESTAO_TIPO_ADOECE_COM_FREQUENCIA.ToString(),
                                TipoQuestao = TipoQuestao.ComboMultiplaEscolha,

                            },
                            new ()
                            {
                                QuestaoId = ID_QUESTAO_TIPO_DOENCA_CRONICA,
                                Resposta = ID_OPCAO_RESPOSTA_OUTRAS_QUESTAO_TIPO_DOENCA_CRONICA.ToString(),
                                TipoQuestao = TipoQuestao.ComboMultiplaEscolha,

                            },
                            new ()
                            {
                                QuestaoId = ID_QUESTAO_OBS_AGRUPAMENTO_PROMOCAO_CUIDADOS,
                                Resposta = "Observa��es preenchidas para [Adoece com frequ�ncia sem receber cuidados m�dicos] e [Doen�a cr�nica ou em tratamento de longa dura��o]",
                                TipoQuestao = TipoQuestao.Texto,

                            }
                        }
                    }
                }
            };
            var retorno = await registrarEncaminhamentoNaapaUseCase.Executar(encaminhamentosNaapaDto);
            
            retorno.ShouldNotBeNull("Nenhum ResultadoEncaminhamentoNAAPADto obtido ao registrar Encaminhamento NAAPA");
            retorno.Id.ShouldBe(1, "Id inv�lido obtido ao registrar Encaminhamento NAAPA");
            retorno.Auditoria.ShouldNotBeNull("Nenhum registro de Auditoria obtido ao registrar Encaminhamento NAAPA");
            retorno.Auditoria.AlteradoEm.HasValue.ShouldBeTrue("Auditoria obtida n�o cont�m data/hora altera��o");
            
            var encaminhamentoNAAPA = ObterTodos<Dominio.EncaminhamentoNAAPA>();
            encaminhamentoNAAPA.FirstOrDefault().Situacao.Equals(SituacaoNAAPA.AguardandoAtendimento).ShouldBeTrue("Ap�s registrar Encaminhamento NAAPA o status n�o foi alterado");
            encaminhamentoNAAPA.Count().ShouldBe(1, "Qdade registros Encaminhamento NAAPA inv�lidos");
        }

        [Fact(DisplayName = "Encaminhamento NAAPA - Alterar encaminhamento NAAPA para Aguardando Atendimento (observa��o n�o obrigat�ria n�o preenchida)")]
        public async Task Ao_editar_encaminhamento_para_aguardandoatendimento_nao_consistir_observacao_nao_obrigatoria_nao_preenchida()
        {
            var filtroNAAPA = new FiltroNAAPADto()
            {
                Perfil = ObterPerfilCP(),
                TipoCalendario = ModalidadeTipoCalendario.FundamentalMedio,
                Modalidade = Modalidade.Fundamental,
                AnoTurma = "8",
                DreId = 1,
                CodigoUe = "1",
                TurmaId = TURMA_ID_1,
                Situacao = (int)SituacaoNAAPA.Rascunho,
                Prioridade = NORMAL
            };

            await CriarDadosBase(filtroNAAPA);

            var registrarEncaminhamentoNaapaUseCase = ObterServicoRegistrarEncaminhamento();

            var dataAtual = DateTimeExtension.HorarioBrasilia().Date;
            var dataQueixa = new DateTime(dataAtual.Year, 11, 18);

            await GerarDadosEncaminhamentoNAAPA(dataQueixa);

            dataQueixa.AddDays(4);

            var encaminhamentosNaapaDto = new EncaminhamentoNAAPADto()
            {
                Id = 1,
                TurmaId = TURMA_ID_1,
                Situacao = SituacaoNAAPA.AguardandoAtendimento,
                AlunoCodigo = ALUNO_CODIGO_1,
                AlunoNome = "Nome do aluno do naapa",
                Secoes = new List<EncaminhamentoNAAPASecaoDto>()
                {
                    new ()
                    {
                        SecaoId = 2,
                        Questoes = new List<EncaminhamentoNAAPASecaoQuestaoDto>()
                        {
                            new ()
                            {
                                QuestaoId = ID_QUESTAO_AGRUPAMENTO_PROMOCAO_CUIDADOS,
                                Resposta = ID_OPCAO_RESPOSTA_DOENCA_CRONICA.ToString(),
                                TipoQuestao = TipoQuestao.ComboMultiplaEscolha,

                            },
                            new ()
                            {
                                QuestaoId = ID_QUESTAO_AGRUPAMENTO_PROMOCAO_CUIDADOS,
                                Resposta = ID_OPCAO_RESPOSTA_ADOECE_COM_FREQUENCIA.ToString(),
                                TipoQuestao = TipoQuestao.ComboMultiplaEscolha,

                            },
                            new ()
                            {
                                QuestaoId = ID_QUESTAO_TIPO_ADOECE_COM_FREQUENCIA,
                                Resposta = ID_OPCAO_RESPOSTA_ASSADURA_QUESTAO_TIPO_ADOECE_COM_FREQUENCIA.ToString(),
                                TipoQuestao = TipoQuestao.ComboMultiplaEscolha,

                            },
                            new ()
                            {
                                QuestaoId = ID_QUESTAO_TIPO_DOENCA_CRONICA,
                                Resposta = ID_OPCAO_RESPOSTA_ANEMIA_FALCIFORME_QUESTAO_TIPO_DOENCA_CRONICA.ToString(),
                                TipoQuestao = TipoQuestao.ComboMultiplaEscolha,

                            }
                        }
                    }
                }
            };
            var retorno = await registrarEncaminhamentoNaapaUseCase.Executar(encaminhamentosNaapaDto);

            retorno.ShouldNotBeNull("Nenhum ResultadoEncaminhamentoNAAPADto obtido ao registrar Encaminhamento NAAPA");
            retorno.Id.ShouldBe(1, "Id inv�lido obtido ao registrar Encaminhamento NAAPA");
            retorno.Auditoria.ShouldNotBeNull("Nenhum registro de Auditoria obtido ao registrar Encaminhamento NAAPA");
            retorno.Auditoria.AlteradoEm.HasValue.ShouldBeTrue("Auditoria obtida n�o cont�m data / hora altera��o");

            var encaminhamentoNAAPA = ObterTodos<Dominio.EncaminhamentoNAAPA>();
            encaminhamentoNAAPA.FirstOrDefault().Situacao.Equals(SituacaoNAAPA.AguardandoAtendimento).ShouldBeTrue("Ap�s registrar Encaminhamento NAAPA o status n�o foi alterado");
            encaminhamentoNAAPA.Count().ShouldBe(1, "Qdade registros Encaminhamento NAAPA inv�lidos");
        }

        [Fact(DisplayName = "Encaminhamento NAAPA - Alterar encaminhamento NAAPA para Aguardando Atendimento persistindo respostas")]
        public async Task Ao_editar_encaminhamento_para_aguardandoatendimento_persistir_novo_status_e_respostas()
        {
            var filtroNAAPA = new FiltroNAAPADto()
            {
                Perfil = ObterPerfilCP(),
                TipoCalendario = ModalidadeTipoCalendario.FundamentalMedio,
                Modalidade = Modalidade.Fundamental,
                AnoTurma = "8",
                DreId = 1,
                CodigoUe = "1",
                TurmaId = TURMA_ID_1,
                Situacao = (int)SituacaoNAAPA.Rascunho,
                Prioridade = NORMAL
            };

            await CriarDadosBase(filtroNAAPA);

            var registrarEncaminhamentoNaapaUseCase = ObterServicoRegistrarEncaminhamento();

            var dataAtual = DateTimeExtension.HorarioBrasilia().Date;
            var dataQueixa = new DateTime(dataAtual.Year, 11, 18);

            await GerarDadosEncaminhamentoNAAPA(dataQueixa);

            dataQueixa.AddDays(4);

            var encaminhamentosNaapaDto = new EncaminhamentoNAAPADto()
            {
                Id = 1,
                TurmaId = TURMA_ID_1,
                Situacao = SituacaoNAAPA.AguardandoAtendimento,
                AlunoCodigo = ALUNO_CODIGO_1,
                AlunoNome = "Nome do aluno do naapa",
                Secoes = new List<EncaminhamentoNAAPASecaoDto>()
                {
                    new ()
                    {
                        SecaoId = 2,
                        Questoes = new List<EncaminhamentoNAAPASecaoQuestaoDto>()
                        {
                            new ()
                            {
                                QuestaoId = ID_QUESTAO_AGRUPAMENTO_PROMOCAO_CUIDADOS,
                                Resposta = ID_OPCAO_RESPOSTA_DOENCA_CRONICA.ToString(),
                                TipoQuestao = TipoQuestao.ComboMultiplaEscolha,

                            },
                            new ()
                            {
                                QuestaoId = ID_QUESTAO_AGRUPAMENTO_PROMOCAO_CUIDADOS,
                                Resposta = ID_OPCAO_RESPOSTA_ADOECE_COM_FREQUENCIA.ToString(),
                                TipoQuestao = TipoQuestao.ComboMultiplaEscolha,

                            },
                            new ()
                            {
                                QuestaoId = ID_QUESTAO_TIPO_ADOECE_COM_FREQUENCIA,
                                Resposta = ID_OPCAO_RESPOSTA_OUTRAS_QUESTAO_TIPO_ADOECE_COM_FREQUENCIA.ToString(),
                                TipoQuestao = TipoQuestao.ComboMultiplaEscolha,

                            },
                            new ()
                            {
                                QuestaoId = ID_QUESTAO_TIPO_DOENCA_CRONICA,
                                Resposta = ID_OPCAO_RESPOSTA_OUTRAS_QUESTAO_TIPO_DOENCA_CRONICA.ToString(),
                                TipoQuestao = TipoQuestao.ComboMultiplaEscolha,

                            },
                            new ()
                            {
                                QuestaoId = ID_QUESTAO_OBS_AGRUPAMENTO_PROMOCAO_CUIDADOS,
                                Resposta = "Observa��es preenchidas para [Adoece com frequ�ncia sem receber cuidados m�dicos] e [Doen�a cr�nica ou em tratamento de longa dura��o]",
                                TipoQuestao = TipoQuestao.Texto,

                            }
                        }
                    }
                }
            };
            var retorno = await registrarEncaminhamentoNaapaUseCase.Executar(encaminhamentosNaapaDto);
            retorno.ShouldNotBeNull("Nenhum ResultadoEncaminhamentoNAAPADto obtido ao registrar Encaminhamento NAAPA");
            retorno.Id.ShouldBe(1, "Id inv�lido obtido ao registrar Encaminhamento NAAPA");
            retorno.Auditoria.ShouldNotBeNull("Nenhum registro de Auditoria obtido ao registrar Encaminhamento NAAPA");
            retorno.Auditoria.AlteradoEm.HasValue.ShouldBeTrue("Auditoria obtida n�o cont�m data/hora altera��o");

            var encaminhamentoNAAPA = ObterTodos<Dominio.EncaminhamentoNAAPA>();
            encaminhamentoNAAPA.FirstOrDefault().Situacao.Equals(SituacaoNAAPA.AguardandoAtendimento).ShouldBeTrue("Ap�s registrar Encaminhamento NAAPA o status n�o foi alterado");
            encaminhamentoNAAPA.Count().ShouldBe(1, "Qdade registros Encaminhamento NAAPA inv�lidos");

            var encaminhamentoNAAPASecao = ObterTodos<Dominio.EncaminhamentoNAAPASecao>();
            encaminhamentoNAAPASecao.Count().ShouldBe(2, "Qdade registros Encaminhamento NAAPA Se��o inv�lidos");

            var questoesQuestionarioNAAPA = ObterTodos<Dominio.Questao>().Where(questao => questao.NomeComponente == NOME_COMPONENTE_QUESTAO_AGRUPAMENTO_PROMOCAO_CUIDADOS);
            questoesQuestionarioNAAPA.ShouldBeUnique("Qdade registros Quest�o [Agrupamento promo��o de cuidados] inv�lidos");

            var questoesEncaminhamentoNAAPA = ObterTodos<Dominio.QuestaoEncaminhamentoNAAPA>().Where(questao => questao.QuestaoId == questoesQuestionarioNAAPA.FirstOrDefault().Id);
            questoesEncaminhamentoNAAPA.ShouldBeUnique("Qdade registros Quest�o Encaminhamento NAAPA [Agrupamento promo��o de cuidados] inv�lidos");

            var respostasEncaminhamentoNAAPASecao2 = ObterTodos<Dominio.RespostaEncaminhamentoNAAPA>();
            respostasEncaminhamentoNAAPASecao2 = respostasEncaminhamentoNAAPASecao2.Where(resposta => resposta.QuestaoEncaminhamentoId == questoesEncaminhamentoNAAPA.FirstOrDefault().Id).ToList();
            respostasEncaminhamentoNAAPASecao2.Count().ShouldBe(2, "Qdade registros Resposta Encaminhamento NAAPA [Agrupamento promo��o de cuidados] inv�lidos");
            respostasEncaminhamentoNAAPASecao2.Select(resposta => resposta.RespostaId).ShouldContain(ID_OPCAO_RESPOSTA_DOENCA_CRONICA);
            respostasEncaminhamentoNAAPASecao2.Select(resposta => resposta.RespostaId).ShouldContain(ID_OPCAO_RESPOSTA_ADOECE_COM_FREQUENCIA);
        }

        private async Task GerarDadosEncaminhamentoNAAPA(DateTime dataQueixa)
        {
            await CriarEncaminhamentoNAAPA();
            await CriarEncaminhamentoNAAPASecao();
            await CriarQuestoesEncaminhamentoNAAPA();
            await CriarRespostasEncaminhamentoNAAPA(dataQueixa);
        }

        private async Task CriarRespostasEncaminhamentoNAAPA(DateTime dataQueixa)
        {
            await InserirNaBase(new Dominio.RespostaEncaminhamentoNAAPA()
            {
                QuestaoEncaminhamentoId = 1,
                Texto = dataQueixa.ToString("dd/MM/yyyy"),
                CriadoEm = DateTimeExtension.HorarioBrasilia(), CriadoPor = SISTEMA_NOME, CriadoRF = SISTEMA_CODIGO_RF
            });

            await InserirNaBase(new Dominio.RespostaEncaminhamentoNAAPA()
            {
                QuestaoEncaminhamentoId = 2,
                Texto = "1",
                CriadoEm = DateTimeExtension.HorarioBrasilia(), CriadoPor = SISTEMA_NOME, CriadoRF = SISTEMA_CODIGO_RF
            });
        }

        private async Task CriarQuestoesEncaminhamentoNAAPA()
        {
            await InserirNaBase(new Dominio.QuestaoEncaminhamentoNAAPA()
            {
                EncaminhamentoNAAPASecaoId = 1,
                QuestaoId = 1,
                CriadoEm = DateTimeExtension.HorarioBrasilia(), CriadoPor = SISTEMA_NOME, CriadoRF = SISTEMA_CODIGO_RF
            });

            await InserirNaBase(new Dominio.QuestaoEncaminhamentoNAAPA()
            {
                EncaminhamentoNAAPASecaoId = 1,
                QuestaoId = 2,
                CriadoEm = DateTimeExtension.HorarioBrasilia(), CriadoPor = SISTEMA_NOME, CriadoRF = SISTEMA_CODIGO_RF
            });
        }

        private async Task CriarEncaminhamentoNAAPASecao()
        {
            await InserirNaBase(new Dominio.EncaminhamentoNAAPASecao()
            {
                EncaminhamentoNAAPAId = 1,
                SecaoEncaminhamentoNAAPAId = 1,
                CriadoEm = DateTimeExtension.HorarioBrasilia(), CriadoPor = SISTEMA_NOME, CriadoRF = SISTEMA_CODIGO_RF
            });
        }

        private async Task CriarEncaminhamentoNAAPA()
        {
            await InserirNaBase(new Dominio.EncaminhamentoNAAPA()
            {
                TurmaId = TURMA_ID_1,
                AlunoCodigo = ALUNO_CODIGO_1,
                Situacao = SituacaoNAAPA.Rascunho,
                AlunoNome = "Nome do aluno 1",
                CriadoEm = DateTimeExtension.HorarioBrasilia(), CriadoPor = SISTEMA_NOME, CriadoRF = SISTEMA_CODIGO_RF
            });
        }
    }
}

