﻿using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using SME.SGP.Aplicacao;
using SME.SGP.Dominio;
using SME.SGP.Dominio.Constantes.MensagensNegocio;
using SME.SGP.Infra;
using SME.SGP.TesteIntegracao.Setup;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace SME.SGP.TesteIntegracao.Nota
{
    public class Ao_registrar_nota_para_professor_cj : NotaBase
    {
        public Ao_registrar_nota_para_professor_cj(CollectionFixture collectionFixture) : base(collectionFixture)
        {
        }

        [Fact]
        public async Task Ao_lancar_nota_numerica_pelo_professor_cj_com_avaliacoes_do_professor_titular_do_cj()
        {
            await CrieDados();

            var dto = new NotaConceitoListaDto()
            {
                DisciplinaId = COMPONENTE_REG_CLASSE_SP_INTEGRAL_1A5_ANOS_ID_1213.ToString(),
                TurmaId = TURMA_CODIGO_1,
                NotasConceitos = new List<NotaConceitoDto>()
                {
                    new NotaConceitoDto()
                    {
                        AlunoId = ALUNO_CODIGO_1,
                        Nota = 7,
                        AtividadeAvaliativaId = ATIVIDADE_AVALIATIVA_1
                    }
                }
            };

            var comando = ServiceProvider.GetService<IComandosNotasConceitos>();

            await comando.Salvar(dto);

            var notas = ObterTodos<NotaConceito>();

            notas.ShouldNotBeEmpty();
            notas.Count().ShouldBeGreaterThanOrEqualTo(1);
            notas.Exists(nota => nota.TipoNota == TipoNota.Nota).ShouldBe(true);
        }

        [Fact]
        public async Task Nao_pode_lancar_nota_numerica_pelo_professor_cj_com_avaliacoes_do_professor_diferente()
        {
            await CrieDados();

            var dto = new NotaConceitoListaDto()
            {
                DisciplinaId = COMPONENTE_REG_CLASSE_SP_INTEGRAL_1A5_ANOS_ID_1213.ToString(),
                TurmaId = TURMA_CODIGO_1,
                NotasConceitos = new List<NotaConceitoDto>()
                {
                    new NotaConceitoDto()
                    {
                        AlunoId = ALUNO_CODIGO_1,
                        Nota = 7,
                        AtividadeAvaliativaId = ATIVIDADE_AVALIATIVA_2
                    }
                }
            };

            await ExecuteExcecao(dto);
        }

        [Fact]
        public async Task Ao_lancar_nota_conceito_pelo_professor_cj_com_avaliacoes_do_professor_titular_do_cj()
        {
            await CrieDados();
            await CriaConceito();

            var dto = new NotaConceitoListaDto()
            {
                DisciplinaId = COMPONENTE_REG_CLASSE_SP_INTEGRAL_1A5_ANOS_ID_1213.ToString(),
                TurmaId = TURMA_CODIGO_1,
                NotasConceitos = new List<NotaConceitoDto>()
                {
                    new NotaConceitoDto()
                    {
                        AlunoId = ALUNO_CODIGO_1,
                        Conceito = 1,
                        AtividadeAvaliativaId = ATIVIDADE_AVALIATIVA_1
                    },
                    new NotaConceitoDto()
                    {
                        AlunoId = ALUNO_CODIGO_2,
                        Conceito = 2,
                        AtividadeAvaliativaId = ATIVIDADE_AVALIATIVA_1
                    },
                     new NotaConceitoDto()
                    {
                        AlunoId = ALUNO_CODIGO_3,
                        Conceito = 3,
                        AtividadeAvaliativaId = ATIVIDADE_AVALIATIVA_1
                    },
                }
            };

            var comando = ServiceProvider.GetService<IComandosNotasConceitos>();

            await comando.Salvar(dto);

            var notas = ObterTodos<NotaConceito>();

            notas.ShouldNotBeEmpty();
            notas.Count().ShouldBeGreaterThanOrEqualTo(1);
            notas.Exists(nota => nota.TipoNota == TipoNota.Conceito).ShouldBe(true);
        }

        [Fact]
        public async Task Nao_pode_lancar_nota_conceito_pelo_professor_cj_com_avaliacoes_do_professor_diferente()
        {
            await CrieDados();
            await CriaConceito();

            var dto = new NotaConceitoListaDto()
            {
                DisciplinaId = COMPONENTE_REG_CLASSE_SP_INTEGRAL_1A5_ANOS_ID_1213.ToString(),
                TurmaId = TURMA_CODIGO_1,
                NotasConceitos = new List<NotaConceitoDto>()
                {
                    new NotaConceitoDto()
                    {
                        AlunoId = ALUNO_CODIGO_1,
                        Conceito = 1,
                        AtividadeAvaliativaId = ATIVIDADE_AVALIATIVA_1
                    },
                    new NotaConceitoDto()
                    {
                        AlunoId = ALUNO_CODIGO_2,
                        Conceito = 2,
                        AtividadeAvaliativaId = ATIVIDADE_AVALIATIVA_2
                    },
                     new NotaConceitoDto()
                    {
                        AlunoId = ALUNO_CODIGO_3,
                        Conceito = 3,
                        AtividadeAvaliativaId = ATIVIDADE_AVALIATIVA_1
                    },
                }
            };

            await ExecuteExcecao(dto);
        }

        private async Task CrieDados()
        {
            var filtroNota = new FiltroNotasDto()
            {
                Perfil = ObterPerfilCJ(),
                Modalidade = Modalidade.Fundamental,
                TipoCalendario = ModalidadeTipoCalendario.FundamentalMedio,
                Bimestre = BIMESTRE_2,
                ComponenteCurricular = COMPONENTE_REG_CLASSE_SP_INTEGRAL_1A5_ANOS_ID_1213.ToString(),
                TipoNota = TipoNota.Nota
            };

            await CriarDadosBase(filtroNota);
            await CriarAula(filtroNota.ComponenteCurricular, DATA_02_05_INICIO_BIMESTRE_2, RecorrenciaAula.AulaUnica, NUMERO_AULA_1);
            await CrieTipoAtividade();
            await CriarAtividadeAvaliativa(DATA_02_05_INICIO_BIMESTRE_2, filtroNota.ComponenteCurricular, USUARIO_PROFESSOR_LOGIN_2222222, true, ATIVIDADE_AVALIATIVA_1);
            await CriarAtividadeAvaliativa(DATA_02_05_INICIO_BIMESTRE_2, filtroNota.ComponenteCurricular, USUARIO_PROFESSOR_LOGIN_1111111, true, ATIVIDADE_AVALIATIVA_2);


            var notas = ObterTodos<AtividadeAvaliativa>();
        }

        private async Task ExecuteExcecao(NotaConceitoListaDto dto)
        {
            var comando = ServiceProvider.GetService<IComandosNotasConceitos>();

            var excecao = await Assert.ThrowsAsync<NegocioException>(() => comando.Salvar(dto));

            excecao.Message.ShouldBe(MensagensNegocioLancamentoNota.Somente_o_professor_que_criou_a_avaliacao_pode_atribuir_nota);
        }
    }
}
