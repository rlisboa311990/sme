using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Shouldly;
using SME.SGP.Aplicacao;
using SME.SGP.Dominio;
using SME.SGP.Infra;
using SME.SGP.TesteIntegracao.ServicosFakes;
using SME.SGP.TesteIntegracao.Setup;

namespace SME.SGP.TesteIntegracao.Listao
{
    public class ListaoTesteBase : TesteBaseComuns
    {
        private const string ATESTADO_MEDICO_DO_ALUNO = "Atestado Médico do Aluno";
        private const string ATESTADO_MEDIDO_PESSOA_FAMILIA = "Atestado Médico de pessoa da Família";
        private const string ENCHENTE = "Enchente";
        private const string FALTA_TRANSPORTE = "Falta de transporte";
        
        private readonly string[] listaDescricaoMotivoAusencia =
        {
            ATESTADO_MEDICO_DO_ALUNO,
            ATESTADO_MEDIDO_PESSOA_FAMILIA,
            ENCHENTE,
            FALTA_TRANSPORTE
        };
        
        private readonly string[] codigosAlunos = { CODIGO_ALUNO_1, CODIGO_ALUNO_2, CODIGO_ALUNO_3, CODIGO_ALUNO_4 };
        private readonly TipoFrequencia[] tiposFrequencias = { TipoFrequencia.C, TipoFrequencia.F, TipoFrequencia.R };

        protected ListaoTesteBase(CollectionFixture collectionFixture) : base(collectionFixture)
        {
        }
        
        protected override void RegistrarFakes(IServiceCollection services)
        {
            base.RegistrarFakes(services);
            
            services.Replace(new ServiceDescriptor(typeof(IRequestHandler<ObterAlunosPorTurmaEDataMatriculaQuery, IEnumerable<AlunoPorTurmaResposta>>),
                typeof(ObterAlunosPorTurmaEDataMatriculaQueryHandlerFake), ServiceLifetime.Scoped));
        }

        protected async Task CriarDadosBasicos(FiltroListao filtroListao)
        {
            await CriarPadrao();
            await CriarComponenteCurricular();
            
            await CriarUsuarios();
            CriarClaimUsuario(filtroListao.Perfil);

            await CriarTurma(filtroListao.Modalidade, filtroListao.AnoTurma, filtroListao.TurmaHistorica,
                filtroListao.TipoTurma);
            
            await CriarTipoCalendario(filtroListao.TipoCalendario);
            
            await CriarAula(filtroListao.DataAula, RecorrenciaAula.AulaUnica, TipoAula.Normal,
                USUARIO_PROFESSOR_LOGIN_2222222, TURMA_CODIGO_1, UE_CODIGO_1,
                filtroListao.ComponenteCurricularId.ToString(), TIPO_CALENDARIO_1);

            await CriarPeriodoEscolarTodosBimestres();
            await InserirParametroSistema();
            await CriarMotivoAusencia();
            await CriarFrequenciaPreDefinida(filtroListao.ComponenteCurricularId);
            await CriarRegistroFrenquencia();
        }
        
        private async Task CriarPeriodoEscolarTodosBimestres()
        {
            await CriarPeriodoEscolar(DATA_01_02_INICIO_BIMESTRE_1, DATA_25_04_FIM_BIMESTRE_1, BIMESTRE_1);
            await CriarPeriodoEscolar(DATA_02_05_INICIO_BIMESTRE_2, DATA_08_07_FIM_BIMESTRE_2, BIMESTRE_2);
            await CriarPeriodoEscolar(DATA_25_07_INICIO_BIMESTRE_3, DATA_30_09_FIM_BIMESTRE_3, BIMESTRE_3);
            await CriarPeriodoEscolar(DATA_03_10_INICIO_BIMESTRE_4, DATA_22_12_FIM_BIMESTRE_4, BIMESTRE_4);
        }

        private async Task InserirParametroSistema()
        {
            await InserirNaBase(new ParametrosSistema()
            {
                Nome = "PercentualFrequenciaCritico",
                Tipo = TipoParametroSistema.PercentualFrequenciaCritico,
                Descricao = "Percentual de frequência para definir aluno em situação crítica",
                Valor = "75",
                Ano = DateTimeExtension.HorarioBrasilia().Year,
                Ativo = true,
                CriadoEm = DateTimeExtension.HorarioBrasilia(),
                CriadoPor = SISTEMA_NOME,
                CriadoRF = SISTEMA_CODIGO_RF
            });

            await InserirNaBase(new ParametrosSistema()
            {
                Nome = "PercentualFrequenciaAlerta",
                Tipo = TipoParametroSistema.PercentualFrequenciaAlerta,
                Descricao = "Percentual de frequência para definir aluno em situação de alerta",
                Valor = "80",
                Ano = DateTimeExtension.HorarioBrasilia().Year,
                Ativo = true,
                CriadoEm = DateTimeExtension.HorarioBrasilia(),
                CriadoPor = SISTEMA_NOME,
                CriadoRF = SISTEMA_CODIGO_RF
            });
        }

        private async Task CriarRegistroFrenquencia()
        {
            var aulaId = (ObterTodos<Dominio.Aula>().FirstOrDefault()?.Id).GetValueOrDefault();
            aulaId.ShouldBeGreaterThan(0);
            
            await InserirNaBase(new RegistroFrequencia
            {
                AulaId = aulaId,
                CriadoPor = SISTEMA_NOME,
                CriadoRF = SISTEMA_CODIGO_RF
            });

            var registroFrequenciaId = (ObterTodos<RegistroFrequencia>().FirstOrDefault()?.Id).GetValueOrDefault();
            registroFrequenciaId.ShouldBeGreaterThan(0);

            int[] quantidadesAulas = { QUANTIDADE_AULA, QUANTIDADE_AULA_2, QUANTIDADE_AULA_3, QUANTIDADE_AULA_4 };
            string[] codigosAlunosAnotacaoFrequencia = { CODIGO_ALUNO_2, CODIGO_ALUNO_4 };

            foreach (var codigoAluno in codigosAlunos)
            {
                var rand = new Random();
                var index = rand.Next(quantidadesAulas.Length);
                await CriarRegistroFrequenciaAluno(registroFrequenciaId, codigoAluno, quantidadesAulas[index], aulaId);

                if (codigosAlunosAnotacaoFrequencia.Contains(codigoAluno))
                    await CriarAnotacaoFrequencia(aulaId, codigoAluno);
            }
        }        
        
        private async Task CriarRegistroFrequenciaAluno(long registroFrequenciaId, string codigoAluno, int numeroAula,
            long aulaId)
        {
            var rand = new Random();
            var index = rand.Next(tiposFrequencias.Length);
            
            await InserirNaBase(new RegistroFrequenciaAluno
            {
                CodigoAluno = codigoAluno,
                RegistroFrequenciaId = registroFrequenciaId,
                CriadoPor = SISTEMA_NOME,
                CriadoRF = SISTEMA_CODIGO_RF,
                Valor = (int)tiposFrequencias[index],
                NumeroAula = numeroAula,
                AulaId = aulaId
            });
        }

        private async Task CriarMotivoAusencia()
        {
            foreach (var descricaoMotivoAusencia in listaDescricaoMotivoAusencia)
            {
                await InserirNaBase(new MotivoAusencia
                {
                    Descricao = descricaoMotivoAusencia
                });
            }
        }
        
        private async Task CriarAnotacaoFrequencia(long aulaId, string codigoAluno)
        {
            var motivosAunsencias = ObterTodos<MotivoAusencia>();
            motivosAunsencias.ShouldNotBeNull();

            var idsMotivosAusencias = motivosAunsencias.Select(c => c.Id).ToArray();
            
            var rand = new Random();
            var index = rand.Next(idsMotivosAusencias.Length);
            
            await InserirNaBase(new AnotacaoFrequenciaAluno
            {
                AulaId = aulaId,
                CodigoAluno = codigoAluno,
                MotivoAusenciaId = idsMotivosAusencias[index],
                Anotacao = "Teste de integração do Listão.",
                CriadoEm = DateTimeExtension.HorarioBrasilia(),
                CriadoPor = SISTEMA_NOME,
                CriadoRF = SISTEMA_CODIGO_RF
            });
        }

        private async Task CriarFrequenciaPreDefinida(long componenteCurricularId)
        {
            var turmaId = ObterTodos<Turma>().Select(c => c.Id).FirstOrDefault();

            foreach (var codigoAluno in codigosAlunos)
            {
                var rand = new Random();
                var index = rand.Next(tiposFrequencias.Length);
                
                await InserirNaBase(new FrequenciaPreDefinida()
                {
                    CodigoAluno = codigoAluno,
                    TipoFrequencia = tiposFrequencias[index],
                    ComponenteCurricularId = componenteCurricularId,
                    TurmaId = turmaId
                });                
            }
        }        
    }
}