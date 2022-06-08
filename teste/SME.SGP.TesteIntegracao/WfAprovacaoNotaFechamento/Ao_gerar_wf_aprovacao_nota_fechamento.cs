﻿using MediatR;
using Shouldly;
using SME.SGP.Aplicacao;
using SME.SGP.Dominio;
using SME.SGP.TesteIntegracao.Setup;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using Microsoft.Extensions.DependencyInjection;
using MediatR;
using SME.SGP.Infra;
using System.Text.Json;
using System.Collections.Generic;

namespace SME.SGP.TesteIntegracao
{
    public class Ao_gerar_wf_aprovacao_nota_fechamento : TesteBase
    {
        private const string DRE_NOME = "DIRETORIA REGIONAL DE EDUCACAO JACANA/TREMEMBE";
        private const string DRE_CODIGO = "108800";
        private const string DRE_ABREVIACAO = "DRE - JT";

        private const string UE_NOME = "MAXIMO DE MOURA SANTOS, PROF.";
        private const string UE_CODIGO = "094765";

        private const string TURMA_NOME = "7B";
        private const string TURMA_FILTRO = "7B - 7º ANO";
        private const string TURMA_CODIGO = "2261179";
        private const string TURMA_ANO = "7";

        private const string TIPO_CALDENDARIO_NOME = "Calendário Escolar de 2022";

        private const string SISTEMA = "Sistema";
        private const string SISTEMA_CRIADO_RF = "1";

        private const int DISCIPLINA_ID = 1;

        private const int NOTA_5 = 5;
        private const int NOTA_8 = 8;
        private const int NOTA_7 = 7;
        private const int NOTA_9 = 9;

        private const string ALUNO_CODIGO_4182555 = "4182555";
        private const string ALUNO_CODIGO_4182556 = "4182556";
        private const string ALUNO_CODIGO_4182557 = "4182557";

        private const string MENSAGEM_NOTIFICACAO_WF_APROVACAO = "Foram criadas 4 aula(s) de reposição de Língua Portuguesa na turma 7B da DERVILLE ALLEGRETTI, PROF. (DIRETORIA REGIONAL DE EDUCACAO JACANA/TREMEMBE). Para que esta aula seja considerada válida você precisa aceitar esta notificação. Para visualizar a aula clique  <a href='https://dev-novosgp.sme.prefeitura.sp.gov.br/calendario-escolar/calendario-professor/cadastro-aula/editar/:0/'>aqui</a>.";

        private const string MENSAGEM_TITULO_WF_APROVACAO = "Criação de Aula de Reposição na turma 7B";

        private const string USUARIO_LOGIN = "9999999";
        private const string USUARIO_CODIGO_RF = "9999999";
        private const string USUARIO_NOME = "NOME DO USUARIO LOGADO";

        public Ao_gerar_wf_aprovacao_nota_fechamento(CollectionFixture collectionFixture) : base(collectionFixture)
        {
        }

        [Fact]
        public async Task Deve_consumir_primeira_fila_wf_notificacao_nota_fechamento_com_sucesso()
        {
            await CirarDadosBasicos();
            await InserirNaBase(new WfAprovacaoNotaFechamento()
            {
                FechamentoNotaId = 1,
                Nota = NOTA_5,
                CriadoEm = System.DateTime.Now,
                CriadoPor = SISTEMA,
                CriadoRF = SISTEMA,
            });

            await InserirNaBase(new WfAprovacaoNotaFechamento()
            {
                FechamentoNotaId = 2,
                Nota = NOTA_8,
                CriadoEm = System.DateTime.Now,
                CriadoPor = SISTEMA,
                CriadoRF = SISTEMA,
            });

            var useCase = ServiceProvider.GetService<INotificarAlteracaoNotaFechamentoAgrupadaUseCase>();
            var mensagem = new WfAprovacaoNotaFechamentoTurmaDto() { TurmaId = 1 };
            var jsonMensagem = JsonSerializer.Serialize(mensagem);
            bool validaFila = await useCase.Executar(new MensagemRabbit(jsonMensagem));

            var wfAprovacao = ObterTodos<WfAprovacaoNotaFechamento>();

            validaFila.ShouldBeTrue();
            wfAprovacao.ShouldNotBeEmpty();
        }


        [Fact]
        public async Task Deve_gerar_notificacao_com_dados_wf_aprovacao_nota_sem_wf_aprovacao_id()
        {
            await CirarDadosBasicos();
            await InserirNaBase(new WfAprovacaoNotaFechamento()
            {
                FechamentoNotaId = 1,
                Nota = NOTA_5,
                CriadoEm = System.DateTime.Now,
                CriadoPor = SISTEMA,
                CriadoRF = SISTEMA,
               
            });

            await InserirNaBase(new WfAprovacaoNotaFechamento()
            {
                FechamentoNotaId = 2,
                Nota = NOTA_8,
                CriadoEm = System.DateTime.Now,
                CriadoPor = SISTEMA,
                CriadoRF = SISTEMA,
                
            });

            var useCase = ServiceProvider.GetService<INotificarAlteracaoNotaFechamentoAgrupadaTurmaUseCase>();
            var wfAprovacaoNotaFechamento = ObterTodos<WfAprovacaoNotaFechamento>();
            var componenteCurricular = new ComponenteCurricular()
            {
                Descricao = "Matemática",
                EhRegenciaClasse = false
            };

            var listaTurmasWfAprovacao = new List<WfAprovacaoNotaFechamentoTurmaDto>();
            listaTurmasWfAprovacao.Add(new WfAprovacaoNotaFechamentoTurmaDto() { WfAprovacao = wfAprovacaoNotaFechamento.FirstOrDefault(), TurmaId = 1, Bimestre = 1, CodigoAluno = "7128291", ComponenteCurricular = componenteCurricular, NotaAnterior = 4, FechamentoTurmaDisciplinaId = 1 });
            
            var jsonMensagem = JsonSerializer.Serialize(listaTurmasWfAprovacao);
            bool validaFila = await useCase.Executar(new MensagemRabbit(jsonMensagem));

            var wfAprovacao = ObterTodos<WorkflowAprovacao>();
            var wfAprovacaoNivel = ObterTodos<WorkflowAprovacaoNivel>();

            validaFila.ShouldBeTrue();
            wfAprovacao.ShouldNotBeNull();
            wfAprovacaoNivel.ShouldNotBeNull();
            wfAprovacaoNivel.Any(w => w.Cargo == Cargo.CP).ShouldBeTrue();
            wfAprovacaoNivel.Any(w => w.Cargo == Cargo.Supervisor).ShouldBeTrue();
            wfAprovacao.Any(w => w.NotificacaoCategoria == NotificacaoCategoria.Workflow_Aprovacao).ShouldBeTrue();
        }

        [Fact]
        public async Task Deve_permitir_inserir_wf_sem_aprovacao_id()
        {
            await CirarDadosBasicos();

            await InserirNaBase(new WfAprovacaoNotaFechamento()
            {
                FechamentoNotaId = 1,
                Nota = NOTA_5,
                CriadoEm = System.DateTime.Now,
                CriadoPor = SISTEMA,
                CriadoRF = SISTEMA,
            });

            var resultadoWfAprovacao = ObterTodos<WfAprovacaoNotaFechamento>();

            resultadoWfAprovacao.ShouldNotBeEmpty();
            resultadoWfAprovacao.Count().ShouldBe(1);
            resultadoWfAprovacao.Any(a => a.WfAprovacaoId is null).ShouldBeTrue();
        }

        [Fact]
        public async Task Deve_permitir_inserir_wf_com_aprovacao_id()
        {
            await CirarDadosBasicos();

            await CriarWfAprovacao();

            await InserirNaBase(new WfAprovacaoNotaFechamento()
            {
                FechamentoNotaId = 1,
                Nota = NOTA_5,
                CriadoEm = System.DateTime.Now,
                CriadoPor = SISTEMA,
                CriadoRF = SISTEMA,
                WfAprovacaoId = 1
            });

            var resultadoWfAprovacao = ObterTodos<WfAprovacaoNotaFechamento>();

            resultadoWfAprovacao.ShouldNotBeEmpty();
            resultadoWfAprovacao.Count().ShouldBe(1);
            resultadoWfAprovacao.Any(a => a.WfAprovacaoId is null).ShouldBeFalse();
            resultadoWfAprovacao.Any(a => a.WfAprovacaoId is not null).ShouldBeTrue();
        }        

        [Fact]
        public async Task Deve_permitir_salvar_nota_fechamento_bimestral_final_tela_sem_aprovacao_id()
        {
            var mediator = ServiceProvider.GetService<IMediator>();

            await CirarDadosBasicos();

            await CriarWfAprovacaoNotaFechamento(null);

            var fechamentosNota = new List<FechamentoNotaDto>()
            {
                new FechamentoNotaDto()
                {
                    Id = 1,
                    Nota = NOTA_9,
                },
                new FechamentoNotaDto()
                {
                    Id = 2,
                    Nota = NOTA_5,
                },
                new FechamentoNotaDto()
                {
                    Id = 3,
                    Nota = NOTA_8,
                },

            };

            var usuarioLogado = ObterTodos<Usuario>().FirstOrDefault();

            await mediator.Send(new EnviarNotasFechamentoParaAprovacaoCommand(fechamentosNota, usuarioLogado));

            var resultadoWfAprovacao = ObterTodos<WfAprovacaoNotaFechamento>();

            resultadoWfAprovacao.ShouldNotBeEmpty();
            resultadoWfAprovacao.Count().ShouldBe(3);

            resultadoWfAprovacao.Any(a => a.WfAprovacaoId is not null).ShouldBeFalse();
            resultadoWfAprovacao.Any(a => a.WfAprovacaoId is null).ShouldBeTrue();

            resultadoWfAprovacao.FirstOrDefault(a => a.FechamentoNotaId == 1).Nota.ShouldBe(NOTA_9);
            resultadoWfAprovacao.FirstOrDefault(a => a.FechamentoNotaId == 2).Nota.ShouldBe(NOTA_5);
            resultadoWfAprovacao.FirstOrDefault(a => a.FechamentoNotaId == 3).Nota.ShouldBe(NOTA_8);
        }

        [Fact]
        public async Task Deve_permitir_salvar_nota_fechamento_bimestral_final_tela_com_aprovacao_id()
        {
            
            var mediator = ServiceProvider.GetService<IMediator>();

            await CirarDadosBasicos();

            await CriarWfAprovacao();

            await CriarWfAprovacaoNotaFechamento(1);

            var fechamentosNota = new List<FechamentoNotaDto>()
            {
                new FechamentoNotaDto()
                {
                    Id = 1,
                    Nota = NOTA_9,                    
                },
                new FechamentoNotaDto()
                {
                    Id = 2,
                    Nota = NOTA_5,
                },
                new FechamentoNotaDto()
                {
                    Id = 3,
                    Nota = NOTA_8,
                },

            };

            var usuarioLogado = ObterTodos<Usuario>().FirstOrDefault();

            var resultadoWfAprovacaoTeste = ObterTodos<WorkflowAprovacao>();

            await mediator.Send(new EnviarNotasFechamentoParaAprovacaoCommand(fechamentosNota, usuarioLogado));

            var resultadoWfAprovacao = ObterTodos<WfAprovacaoNotaFechamento>();

            resultadoWfAprovacao.ShouldNotBeEmpty();
            resultadoWfAprovacao.Count().ShouldBe(3);

            resultadoWfAprovacao.Any(a => a.WfAprovacaoId is not null).ShouldBeFalse();
            resultadoWfAprovacao.Any(a => a.WfAprovacaoId is null).ShouldBeTrue();

            resultadoWfAprovacao.FirstOrDefault(a => a.FechamentoNotaId == 1).Nota.ShouldBe(NOTA_9);
            resultadoWfAprovacao.FirstOrDefault(a => a.FechamentoNotaId == 2).Nota.ShouldBe(NOTA_5);
            resultadoWfAprovacao.FirstOrDefault(a => a.FechamentoNotaId == 3).Nota.ShouldBe(NOTA_8);
        }

        private async Task CriarWfAprovacao()
        {
            await InserirNaBase(new WorkflowAprovacao()
            {
                UeId = UE_CODIGO,
                DreId = DRE_CODIGO,
                Ano = 2022,
                NotificacaoTipo = NotificacaoTipo.Fechamento,
                NotifacaoMensagem = MENSAGEM_NOTIFICACAO_WF_APROVACAO,
                NotifacaoTitulo = MENSAGEM_TITULO_WF_APROVACAO,
                CriadoPor = SISTEMA,
                CriadoEm = DateTime.Now,
                CriadoRF = SISTEMA,
                Tipo = WorkflowAprovacaoTipo.Basica
            });
        }

        private async Task CriarWfAprovacaoNotaFechamento(long? wfAprovacaoId)
        {
            await InserirNaBase(new WfAprovacaoNotaFechamento()
            {
                FechamentoNotaId = 1,
                Nota = NOTA_5,
                CriadoEm = System.DateTime.Now,
                CriadoPor = SISTEMA,
                CriadoRF = SISTEMA,
                WfAprovacaoId = wfAprovacaoId
            });

            await InserirNaBase(new WfAprovacaoNotaFechamento()
            {
                FechamentoNotaId = 2,
                Nota = NOTA_8,
                CriadoEm = System.DateTime.Now,
                CriadoPor = SISTEMA,
                CriadoRF = SISTEMA,
                WfAprovacaoId = wfAprovacaoId
            });

            await InserirNaBase(new WfAprovacaoNotaFechamento()
            {
                FechamentoNotaId = 3,
                Nota = NOTA_9,
                CriadoEm = System.DateTime.Now,
                CriadoPor = SISTEMA,
                CriadoRF = SISTEMA,
                WfAprovacaoId = wfAprovacaoId
            });
        }

        private async Task CirarDadosBasicos()
        {
            await InserirNaBase(new Usuario
            {
                Login = USUARIO_LOGIN,
                CodigoRf = USUARIO_CODIGO_RF,
                Nome = USUARIO_NOME,
                CriadoPor = SISTEMA,
                CriadoRF = SISTEMA_CRIADO_RF
            });

            await InserirNaBase(new Dre()
            {
                Nome = DRE_NOME,
                CodigoDre = DRE_CODIGO,
                Abreviacao = DRE_ABREVIACAO
            });

            await InserirNaBase(new Ue()
            {
                Nome = UE_NOME,
                DreId = 1,
                TipoEscola = TipoEscola.EMEF,
                CodigoUe = UE_CODIGO
            });

            await InserirNaBase(new Turma()
            {
                Nome = TURMA_NOME,
                CodigoTurma = TURMA_CODIGO,
                Ano = TURMA_ANO,
                AnoLetivo = 2021,
                TipoTurma = Dominio.Enumerados.TipoTurma.Regular,
                ModalidadeCodigo = Modalidade.Fundamental,
                UeId = 1,
                NomeFiltro = TURMA_FILTRO
            });

            await InserirNaBase(new TipoCalendario()
            {
                Situacao = true,
                Modalidade = ModalidadeTipoCalendario.FundamentalMedio,
                CriadoPor = "",
                CriadoRF = "",
                CriadoEm = new DateTime(2022, 06, 06),
                Nome = TIPO_CALDENDARIO_NOME,
                Periodo = Periodo.Anual,
                AnoLetivo = 2022,
                Excluido = false
            });

            await InserirNaBase(new PeriodoEscolar()
            {
                Bimestre = 1,
                PeriodoFim = new DateTime(2022, 08, 20),
                PeriodoInicio = new DateTime(2022, 02, 01),
                TipoCalendarioId = 1,
                CriadoPor = "",
                CriadoRF = "",
                CriadoEm = new DateTime(2022, 01, 01),
            });

            await InserirNaBase(new FechamentoTurma()
            {
                TurmaId = 1,
                PeriodoEscolarId = 1,
                CriadoEm = DateTime.Now,
                CriadoPor = SISTEMA,
                CriadoRF = SISTEMA
            });

            await InserirNaBase(new FechamentoTurmaDisciplina()
            {
                DisciplinaId = DISCIPLINA_ID,
                CriadoEm = DateTime.Now,
                CriadoPor = SISTEMA,
                CriadoRF = SISTEMA,
                Situacao = SituacaoFechamento.ProcessadoComSucesso,
                FechamentoTurmaId = 1
            });

            await InserirNaBase(new FechamentoAluno()
            {
                FechamentoTurmaDisciplinaId = 1,
                AlunoCodigo = ALUNO_CODIGO_4182555,
                CriadoEm = DateTime.Now,
                CriadoPor = SISTEMA,
                CriadoRF = SISTEMA,
            });

            await InserirNaBase(new FechamentoNota()
            {
                DisciplinaId = DISCIPLINA_ID,
                Nota = NOTA_5,
                CriadoEm = DateTime.Now,
                CriadoPor = SISTEMA,
                CriadoRF = SISTEMA,
                FechamentoAlunoId = 1
            });

            await InserirNaBase(new FechamentoAluno()
            {
                FechamentoTurmaDisciplinaId = 1,
                AlunoCodigo = ALUNO_CODIGO_4182556,
                CriadoEm = DateTime.Now,
                CriadoPor = SISTEMA,
                CriadoRF = SISTEMA,
            });

            await InserirNaBase(new FechamentoNota()
            {
                DisciplinaId = DISCIPLINA_ID,
                Nota = NOTA_8,
                CriadoEm = DateTime.Now,
                CriadoPor = SISTEMA,
                CriadoRF = SISTEMA,
                FechamentoAlunoId = 2
            });

            await InserirNaBase(new FechamentoAluno()
            {
                FechamentoTurmaDisciplinaId = 1,
                AlunoCodigo = ALUNO_CODIGO_4182557,
                CriadoEm = DateTime.Now,
                CriadoPor = SISTEMA,
                CriadoRF = SISTEMA,
            });

            await InserirNaBase(new FechamentoNota()
            {
                DisciplinaId = DISCIPLINA_ID,
                Nota = NOTA_9,
                CriadoEm = DateTime.Now,
                CriadoPor = SISTEMA,
                CriadoRF = SISTEMA,
                FechamentoAlunoId = 3
            });

            await InserirNaBase(new FechamentoAluno()
            {
                FechamentoTurmaDisciplinaId = 1,
                AlunoCodigo = "7128291",
                CriadoEm = DateTime.Now,
                CriadoPor = SISTEMA,
                CriadoRF = SISTEMA,
            });

            await InserirNaBase(new FechamentoNota()
            {
                DisciplinaId = DISCIPLINA_ID,
                Nota = NOTA_7,
                CriadoEm = DateTime.Now,
                CriadoPor = SISTEMA,
                CriadoRF = SISTEMA,
                FechamentoAlunoId = 2
            });

            await InserirNaBase("componente_curricular_area_conhecimento", "1", "'Área de conhecimento 1'");
            await InserirNaBase("componente_curricular_grupo_matriz", "1", "'Grupo matriz 1'");
            await InserirNaBase("componente_curricular", "1", "512", "1", "1", "'MAT'", "false", "false", "true", "false", "false", "true", "'MATEMATICA'", "'MATEMATICA'");
        }
    }
}
