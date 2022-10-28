using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using SME.SGP.Aplicacao;
using SME.SGP.Aplicacao.Interfaces;
using SME.SGP.Dominio;
using SME.SGP.Infra;
using SME.SGP.TesteIntegracao.ServicosFakes;
using SME.SGP.TesteIntegracao.Setup;

namespace SME.SGP.TesteIntegracao.RelatorioAcompanhamentoAprendizagem
{
    public class RelatorioAcompanhamentoAprendizagemTesteBase : TesteBaseComuns
    {
        private const string PARAMETRO_QUANTIDADE_IMAGENS_PERCURSO_TURMA_NOME = "QuantidadeImagensPercursoTurma";
        private const string PARAMETRO_QUANTIDADE_IMAGENS_PERCURSO_TURMA_DESCRICAO = "Quantidade de Imagens Permitidas na Seção Percurso Coletivo da Turma";
        private const string PARAMETRO_QUANTIDADE_IMAGENS_PERCURSO_TURMA_VALOR = "2";
        private const int QUANTIDADE_3 = 3;
        
        private const string PARAMETRO_QUANTIDADE_IMAGENS_PERCURSO_INDIVIDUAL_CRIANCA_NOME = "QuantidadeImagensPercursoIndividualCrianca";
        private const string PARAMETRO_QUANTIDADE_IMAGENS_PERCURSO_INDIVIDUAL_CRIANCA_DESCRICAO = "Quantidade de Imagens permitiras no percurso individual da criança";
        private const string PARAMETRO_QUANTIDADE_IMAGENS_PERCURSO_INDIVIDUAL_CRIANCA_VALOR = "3";

        public RelatorioAcompanhamentoAprendizagemTesteBase(CollectionFixture collectionFixture) : base(collectionFixture)
        {
        }

        protected override void RegistrarFakes(IServiceCollection services)
        {
            base.RegistrarFakes(services);
            
            services.Replace(new ServiceDescriptor(typeof(IRequestHandler<ObterComponentesCurricularesEolPorCodigoTurmaLoginEPerfilQuery, IEnumerable<ComponenteCurricularEol>>),
                typeof(ObterComponentesCurricularesEolPorCodigoTurmaLoginEPerfilQueryHandlerFakeOutras), ServiceLifetime.Scoped));
            
            services.Replace(new ServiceDescriptor(typeof(IRequestHandler<ObterAlunosPorTurmaQuery, IEnumerable<AlunoPorTurmaResposta>>),
                typeof(SME.SGP.TesteIntegracao.Nota.ServicosFakes.ObterAlunosPorTurmaQueryHandlerFake), ServiceLifetime.Scoped));
        }
        protected ISalvarAcompanhamentoTurmaUseCase SalvarAcompanhamentoTurmaUseCase()
        {
            return ServiceProvider.GetService<ISalvarAcompanhamentoTurmaUseCase>();
        }        
        protected IObterOcorrenciasPorAlunoUseCase ObterOcorrenciasPorAlunoUseCase()
        {
            return ServiceProvider.GetService<IObterOcorrenciasPorAlunoUseCase>();
        }        
        protected IObterAcompanhamentoAlunoUseCase ObterAcompanhamentoAlunoUseCase()
        {
            return ServiceProvider.GetService<IObterAcompanhamentoAlunoUseCase>();
        }        
        protected IObterInformacoesDeFrequenciaAlunoPorSemestreUseCase ObterInformacoesDeFrequenciaAlunoPorSemestreUseCase()
        {
            return ServiceProvider.GetService<IObterInformacoesDeFrequenciaAlunoPorSemestreUseCase>();
        }

        protected async Task CriarAula(string componenteCurricularCodigo, DateTime dataAula, RecorrenciaAula recorrencia, string rf = USUARIO_PROFESSOR_LOGIN_2222222)
        {
            await InserirNaBase(ObterAula(componenteCurricularCodigo, dataAula, recorrencia, rf));
        }
        private Dominio.Aula ObterAula(string componenteCurricularCodigo, DateTime dataAula, RecorrenciaAula recorrencia, string rf = USUARIO_PROFESSOR_LOGIN_2222222)
        {
            return new Dominio.Aula
            {
                UeId = UE_CODIGO_1,
                DisciplinaId = componenteCurricularCodigo,
                TurmaId = TURMA_CODIGO_1,
                TipoCalendarioId = 1,
                ProfessorRf = rf,
                Quantidade = QUANTIDADE_3,
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
        protected IInserirOcorrenciaUseCase InserirOcorrenciaUseCase()
        {
            return ServiceProvider.GetService<IInserirOcorrenciaUseCase>();
        }
        protected async Task CriarDadosBasicos(bool abrirPeriodos = true)
        {
            await CriarDreUePerfil();
            await CriarComponenteCurricular();
            if(abrirPeriodos)
              await CriarPeriodoEscolarTodosBimestres();
            await CriarTipoCalendario(ModalidadeTipoCalendario.Infantil);
            CriarClaimUsuario(ObterPerfilProfessorInfantil());
            await CriarUsuarios();
            await CriarTurma(Modalidade.EducacaoInfantil);
            await CriarParametrosSistema();
            await CriarOcorrenciaTipo();
        }
        
        private async Task CriarPeriodoEscolarTodosBimestres()
        {
            await CriarPeriodoEscolar(DATA_01_02_INICIO_BIMESTRE_1, DATA_25_04_FIM_BIMESTRE_1, BIMESTRE_1);
            await CriarPeriodoEscolar(DATA_02_05_INICIO_BIMESTRE_2, DATA_08_07_FIM_BIMESTRE_2, BIMESTRE_2);
            await CriarPeriodoEscolar(DATA_25_07_INICIO_BIMESTRE_3, DATA_30_09_FIM_BIMESTRE_3, BIMESTRE_3);
            await CriarPeriodoEscolar(DATA_03_10_INICIO_BIMESTRE_4, DATA_22_12_FIM_BIMESTRE_4, BIMESTRE_4);
        }

        private async Task CriarOcorrenciaTipo()
        {
            await InserirNaBase(new OcorrenciaTipo
            {
                Descricao = "Descricao Da Ocorrencia",
                Excluido = false,
                CriadoPor = "Sistema",
                CriadoEm = DateTime.Now,
                CriadoRF = "1"
            });
        }
        private async Task CriarParametrosSistema()
        {
            await InserirNaBase(new ParametrosSistema
            {
                Nome = PARAMETRO_QUANTIDADE_IMAGENS_PERCURSO_TURMA_NOME,
                Tipo = TipoParametroSistema.QuantidadeImagensPercursoTurma,
                Descricao = PARAMETRO_QUANTIDADE_IMAGENS_PERCURSO_TURMA_DESCRICAO,
                Valor = PARAMETRO_QUANTIDADE_IMAGENS_PERCURSO_TURMA_VALOR,
                Ano = DateTimeExtension.HorarioBrasilia().Year,
                CriadoEm = DateTimeExtension.HorarioBrasilia(),
                CriadoPor = SISTEMA_NOME,
                CriadoRF = SISTEMA_CODIGO_RF,
                Ativo = true
            });
            
            await InserirNaBase(new ParametrosSistema
            {
                Nome = PARAMETRO_QUANTIDADE_IMAGENS_PERCURSO_INDIVIDUAL_CRIANCA_NOME,
                Tipo = TipoParametroSistema.QuantidadeImagensPercursoIndividualCrianca,
                Descricao = PARAMETRO_QUANTIDADE_IMAGENS_PERCURSO_INDIVIDUAL_CRIANCA_DESCRICAO,
                Valor = PARAMETRO_QUANTIDADE_IMAGENS_PERCURSO_INDIVIDUAL_CRIANCA_VALOR,
                Ano = DateTimeExtension.HorarioBrasilia().Year,
                CriadoEm = DateTimeExtension.HorarioBrasilia(),
                CriadoPor = SISTEMA_NOME,
                CriadoRF = SISTEMA_CODIGO_RF,
                Ativo = true
            });
            
            await InserirNaBase(new ParametrosSistema
            {
                Nome = DATA_INICIO_SGP,
                Tipo = TipoParametroSistema.DataInicioSGP,
                Descricao = DATA_INICIO_SGP,
                Valor = DateTimeExtension.HorarioBrasilia().Year.ToString(),
                Ano = DateTimeExtension.HorarioBrasilia().Year,
                CriadoEm = DateTimeExtension.HorarioBrasilia(),
                CriadoPor = SISTEMA_NOME,
                CriadoRF = SISTEMA_CODIGO_RF,
                Ativo = true
            });            
        }        
    }
}