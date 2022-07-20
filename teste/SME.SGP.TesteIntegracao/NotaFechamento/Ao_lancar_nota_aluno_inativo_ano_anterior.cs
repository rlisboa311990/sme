﻿using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Shouldly;
using SME.SGP.Aplicacao;
using SME.SGP.Dominio;
using SME.SGP.Infra;
using SME.SGP.Infra.Dtos;
using SME.SGP.TesteIntegracao.Nota.ServicosFakes;
using SME.SGP.TesteIntegracao.NotaFechamento.Base;
using SME.SGP.TesteIntegracao.ServicosFakes;
using SME.SGP.TesteIntegracao.ServicosFakes.Query;
using SME.SGP.TesteIntegracao.Setup;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace SME.SGP.TesteIntegracao.NotaFechamento
{
    public class Ao_lancar_nota_aluno_inativo_ano_anterior : NotaFechamentoTesteBase
    {
        private const long PERIODO_ESCOLAR_CODIGO_4 = 4;
        private const string ALUNO_INATIVO_11 = "11"; 
        public Ao_lancar_nota_aluno_inativo_ano_anterior(CollectionFixture collectionFixture) : base(collectionFixture)
        {
        }

        protected override void RegistrarFakes(IServiceCollection services)
        {
            base.RegistrarFakes(services);

            services.Replace(new ServiceDescriptor(typeof(IRequestHandler<PodePersistirTurmaDisciplinaQuery, bool>), typeof(PodePersistirTurmaDisciplinaQueryHandlerFakeRetornaTrue), ServiceLifetime.Scoped));
            services.Replace(new ServiceDescriptor(typeof(IRequestHandler<ObterAlunosPorTurmaEAnoLetivoQuery, IEnumerable<AlunoPorTurmaResposta>>), typeof(ObterAlunosPorTurmaEAnoLetivoQueryHandlerFake), ServiceLifetime.Scoped));
            services.Replace(new ServiceDescriptor(typeof(IRequestHandler<ObterDadosTurmaEolPorCodigoQuery, DadosTurmaEolDto>), typeof(ObterDadosTurmaEolPorCodigoQueryHandlerFakeRegular), ServiceLifetime.Scoped));
            services.Replace(new ServiceDescriptor(typeof(IRequestHandler<ObterValorParametroSistemaTipoEAnoQuery, string>), typeof(ObterValorParametroSistemaTipoEAnoQueryHandlerFake), ServiceLifetime.Scoped));
            services.Replace(new ServiceDescriptor(typeof(IRequestHandler<ObterUsuarioPossuiPermissaoNaTurmaEDisciplinaQuery, bool>), typeof(ObterUsuarioPossuiPermissaoNaTurmaEDisciplinaQueryHandlerComPermissaoFake), ServiceLifetime.Scoped));
        }

        [Fact]
        public async Task Nao_deve_lancar_nota_para_aluno_inativo()
        {
            await CriarDadosBase(ObterFiltroNotas(ObterPerfilProfessor(), ANO_3, COMPONENTE_CURRICULAR_ARTES_ID_139.ToString(), TipoNota.Conceito, Modalidade.Fundamental, ModalidadeTipoCalendario.FundamentalMedio, false));
            await CriaFechamento();

            var consulta = ServiceProvider.GetService<IConsultasFechamentoFinal>();
            var dto = new FechamentoFinalConsultaFiltroDto()
            {
                DisciplinaCodigo = COMPONENTE_CURRICULAR_ARTES_ID_139,
                EhRegencia = false,
                TurmaCodigo = TURMA_CODIGO_1,
                semestre = SEMESTRE_1
            };
            var retorno = await consulta.ObterFechamentos(dto);
            retorno.ShouldNotBeNull();
            var aluno = retorno.Alunos.FirstOrDefault(aluno => aluno.Codigo == ALUNO_INATIVO_11);
            aluno.ShouldNotBeNull();
            aluno.PodeEditar.ShouldBeFalse();
        }

        [Fact]
        public async Task Deve_lancar_nota_do_ano_anterior_em_aprovacao()
        {
            await CriarDadosBase(ObterFiltroNotas(ObterPerfilProfessor(), ANO_3, COMPONENTE_CURRICULAR_ARTES_ID_139.ToString(), TipoNota.Conceito, Modalidade.Fundamental, ModalidadeTipoCalendario.FundamentalMedio, true));
            await CriaFechamento();

            await ExecutarComandosFechamentoFinal(ObtenhaFechamentoFinalConceitoDto(COMPONENTE_CURRICULAR_ARTES_ID_139, false));

            var consulta = ServiceProvider.GetService<IConsultasFechamentoFinal>();
            var dto = new FechamentoFinalConsultaFiltroDto()
            {
                DisciplinaCodigo = COMPONENTE_CURRICULAR_ARTES_ID_139,
                EhRegencia = false,
                TurmaCodigo = TURMA_CODIGO_1,
                semestre = SEMESTRE_1
            };

            var retorno = await consulta.ObterFechamentos(dto);
            retorno.ShouldNotBeNull();
            var aluno = retorno.Alunos.FirstOrDefault(aluno => aluno.Codigo == ALUNO_CODIGO_1);
            aluno.ShouldNotBeNull();
            aluno.NotasConceitoFinal.FirstOrDefault().EmAprovacao.ShouldBeTrue();
        }

        private async Task CriaFechamento()
        {
            await InserirNaBase(new FechamentoTurma()
            {
                TurmaId = TURMA_ID_1,
                PeriodoEscolarId = PERIODO_ESCOLAR_CODIGO_4,
                CriadoEm = DateTime.Now,
                CriadoPor = SISTEMA_NOME,
                CriadoRF = SISTEMA_CODIGO_RF
            });

            await InserirNaBase(new FechamentoTurmaDisciplina()
            {
                DisciplinaId = COMPONENTE_CURRICULAR_ARTES_ID_139,
                FechamentoTurmaId = FECHAMENTO_TURMA_ID_1,
                Situacao = SituacaoFechamento.ProcessadoComSucesso,
                CriadoEm = DateTime.Now,
                CriadoPor = SISTEMA_NOME,
                CriadoRF = SISTEMA_CODIGO_RF
            });

            await InserirNaBase(new FechamentoAluno()
            {
                AlunoCodigo = CODIGO_ALUNO_1,
                FechamentoTurmaDisciplinaId = FECHAMENTO_TURMA_DISCIPLINA_ID_1,
                CriadoEm = DateTime.Now,
                CriadoPor = SISTEMA_NOME,
                CriadoRF = SISTEMA_CODIGO_RF
            });
        }
    }
}
