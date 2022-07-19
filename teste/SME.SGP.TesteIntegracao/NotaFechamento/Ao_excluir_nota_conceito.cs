﻿using System.Collections.Generic;
using System.Linq;
using SME.SGP.Dominio;
using SME.SGP.TesteIntegracao.Setup;
using System.Threading.Tasks;
using SME.SGP.Infra;
using SME.SGP.TesteIntegracao.NotaFechamento.Base;
using Xunit;

namespace SME.SGP.TesteIntegracao.NotaFechamento
{
    public class Ao_excluir_nota_conceito : NotaFechamentoTesteBase
    {
        public Ao_excluir_nota_conceito(CollectionFixture collectionFixture) : base(collectionFixture)
        { }

        [Fact]
        public async Task Deve_permitir_excluir_nota_numerica_titular_fundamental()
        {
            var filtroNotaFechamento = ObterFiltroNotasFechamento(
                ObterPerfilProfessor(),
                TipoNota.Conceito, ANO_7,
                Modalidade.Fundamental,
                ModalidadeTipoCalendario.FundamentalMedio,
                COMPONENTE_CURRICULAR_PORTUGUES_ID_138.ToString());
            
            await ExecutarTesteInsercaoELimpeza(filtroNotaFechamento);
        }

        [Fact]
        public async Task Deve_permitir_excluir_nota_numerica_titular_medio()
        {
            var filtroNotaFechamento = ObterFiltroNotasFechamento(
                ObterPerfilProfessor(),
                TipoNota.Conceito, ANO_1,
                Modalidade.Medio,
                ModalidadeTipoCalendario.FundamentalMedio,
                COMPONENTE_CURRICULAR_PORTUGUES_ID_138.ToString());
            
            await ExecutarTesteInsercaoELimpeza(filtroNotaFechamento);
        }
        
        [Fact]
        public async Task Deve_permitir_excluir_nota_numerica_titular_eja()
        {
            var filtroNotaFechamento = ObterFiltroNotasFechamento(
                ObterPerfilProfessor(),
                TipoNota.Conceito, ANO_3,
                Modalidade.EJA,
                ModalidadeTipoCalendario.EJA,
                COMPONENTE_HISTORIA_ID_7);
            
            await ExecutarTesteInsercaoELimpeza(filtroNotaFechamento);
        }
        
        [Fact]
        public async Task Deve_permitir_excluir_nota_numerica_titular_regencia_classe_fundamental()
        {
            var filtroNotaFechamento = ObterFiltroNotasFechamento(
                ObterPerfilProfessor(),
                TipoNota.Conceito, ANO_1,
                Modalidade.EJA,
                ModalidadeTipoCalendario.EJA,
                COMPONENTE_REGENCIA_CLASSE_FUND_I_5H_ID_1105.ToString());
            
            await ExecutarTesteInsercaoELimpeza(filtroNotaFechamento);
        }

        [Fact]
        public async Task Deve_permitir_excluir_nota_numerica_cp_fundamental()
        {
            var filtroNotaFechamento = ObterFiltroNotasFechamento(
                ObterPerfilCP(),
                TipoNota.Conceito, ANO_7,
                Modalidade.Fundamental,
                ModalidadeTipoCalendario.FundamentalMedio,
                COMPONENTE_CURRICULAR_PORTUGUES_ID_138.ToString());
            
            await ExecutarTesteInsercaoELimpeza(filtroNotaFechamento);
        }

        [Fact]
        public async Task Deve_permitir_excluir_nota_numerica_diretor_fundamental()
        {
            var filtroNotaFechamento = ObterFiltroNotasFechamento(
                ObterPerfilDiretor(),
                TipoNota.Conceito, ANO_7,
                Modalidade.Fundamental,
                ModalidadeTipoCalendario.FundamentalMedio,
                COMPONENTE_CURRICULAR_PORTUGUES_ID_138.ToString());
            
            await ExecutarTesteInsercaoELimpeza(filtroNotaFechamento);
        }

        private async Task ExecutarTesteInsercaoELimpeza(FiltroNotaFechamentoDto filtroNotaFechamentoDto)
        {
            await CriarDadosBase(filtroNotaFechamentoDto);

            var fechamentoFinalSalvarParaInserir = ObterFechamentoNotaFinalConceitoParaSalvar(filtroNotaFechamentoDto);
            
            await ExecutarComandosFechamentoFinalComValidacaoNota(fechamentoFinalSalvarParaInserir);
            
            var fechamentoFinalSalvarParaExcluir = ObterFechamentoNotaFinalConceitoParaExcluir(filtroNotaFechamentoDto);
            
            await ExecutarComandosFechamentoFinalComValidacaoNota(fechamentoFinalSalvarParaExcluir);
        }
        
        private FechamentoFinalSalvarDto ObterFechamentoNotaFinalConceitoParaExcluir(FiltroNotaFechamentoDto filtroNotaFechamento)
        {
            var fechamentoAlunoInserido = ObterTodos<FechamentoAluno>();

            var fechamentoNotaFinalNumericaParaExcluir = new FechamentoFinalSalvarDto
            {
                DisciplinaId = filtroNotaFechamento.ComponenteCurricular,
                EhRegencia = false,
                TurmaCodigo = TURMA_CODIGO_1,
                Itens = new List<FechamentoFinalSalvarItemDto>()
            };

            foreach (var fechamentoAluno in fechamentoAlunoInserido)
            {
                fechamentoNotaFinalNumericaParaExcluir.Itens.Add(new FechamentoFinalSalvarItemDto
                {
                    AlunoRf = fechamentoAluno.AlunoCodigo,
                    ComponenteCurricularCodigo = long.Parse(filtroNotaFechamento.ComponenteCurricular),
                    Nota = null
                });
            }

            return fechamentoNotaFinalNumericaParaExcluir;
        }
        
        private FechamentoFinalSalvarDto ObterFechamentoNotaFinalConceitoParaSalvar(FiltroNotaFechamentoDto filtroNotaFechamento)
        {
            return new FechamentoFinalSalvarDto()
            {
                DisciplinaId = filtroNotaFechamento.ComponenteCurricular,
                EhRegencia = false,
                TurmaCodigo = TURMA_CODIGO_1,
                Itens = new List<FechamentoFinalSalvarItemDto>()
                {
                    new ()
                    {
                        AlunoRf = ALUNO_CODIGO_1,
                        ComponenteCurricularCodigo = long.Parse(filtroNotaFechamento.ComponenteCurricular),
                        ConceitoId = (int)ConceitoValores.NS
                    },
                    new ()
                    {
                        AlunoRf = ALUNO_CODIGO_2,
                        ComponenteCurricularCodigo = long.Parse(filtroNotaFechamento.ComponenteCurricular),
                        ConceitoId = (int)ConceitoValores.P
                    },
                    new ()
                    {
                        AlunoRf = ALUNO_CODIGO_3,
                        ComponenteCurricularCodigo = long.Parse(filtroNotaFechamento.ComponenteCurricular),
                        ConceitoId = (int)ConceitoValores.S
                    },
                    new ()
                    {
                        AlunoRf = ALUNO_CODIGO_4,
                        ComponenteCurricularCodigo = long.Parse(filtroNotaFechamento.ComponenteCurricular),
                        ConceitoId = (int)ConceitoValores.P
                    },
                    new ()
                    {
                        AlunoRf = ALUNO_CODIGO_5,
                        ComponenteCurricularCodigo = long.Parse(filtroNotaFechamento.ComponenteCurricular),
                        ConceitoId = (int)ConceitoValores.S
                    }
                }
            };
        }

        private FiltroNotaFechamentoDto ObterFiltroNotasFechamento(string perfil, TipoNota tipoNota, string anoTurma,Modalidade modalidade, ModalidadeTipoCalendario modalidadeTipoCalendario, string componenteCurricular , bool considerarAnoAnterior = false)
        {
            return new FiltroNotaFechamentoDto()
            {
                Perfil = perfil,
                Modalidade = modalidade,
                TipoCalendario = modalidadeTipoCalendario,
                Bimestre = BIMESTRE_1,
                ComponenteCurricular = componenteCurricular,
                TipoCalendarioId = TIPO_CALENDARIO_1,
                CriarPeriodoEscolar = true,
                CriarPeriodoAbertura = true,
                TipoNota = tipoNota,
                AnoTurma = anoTurma,
                ConsiderarAnoAnterior = considerarAnoAnterior,
                ProfessorRf = USUARIO_PROFESSOR_LOGIN_2222222
            };
        }
    }
}