using System.Collections.Generic;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using SME.SGP.Aplicacao;
using SME.SGP.Dominio;
using SME.SGP.Infra;
using SME.SGP.TesteIntegracao.ServicosFakes;
using SME.SGP.TesteIntegracao.Setup;
using Xunit;

namespace SME.SGP.TesteIntegracao.ConselhoDeClasse.ServicosFakes
{
    public class Ao_lancar_periodo_abertura_reabertura : ConselhoDeClasseTesteBase
    {
        public Ao_lancar_periodo_abertura_reabertura(CollectionFixture collectionFixture) : base(collectionFixture)
        {
        }
        
        protected override void RegistrarFakes(IServiceCollection services)
        {
            base.RegistrarFakes(services);

            services.Replace(new ServiceDescriptor(typeof(IRequestHandler<ObterUsuarioPossuiPermissaoNaTurmaEDisciplinaQuery, bool>), typeof(ObterUsuarioPossuiPermissaoNaTurmaEDisciplinaQueryHandlerComPermissaoFake), ServiceLifetime.Scoped));
            services.Replace(new ServiceDescriptor(typeof(IRequestHandler<ObterComponentesCurricularesEOLPorTurmasCodigoQuery, IEnumerable<ComponenteCurricularDto>>), typeof(ObterComponentesCurricularesEOLPorTurmasCodigoQueryHandlerFakeValidarSituacaoConselho), ServiceLifetime.Scoped));
            
        }

        [Fact]
        public async Task Nao_deve_lancar_nota_numerica_pos_conselho_sem_periodo_abertura_apos_encerramento_bimestre()
        {
            var salvarConselhoClasseAlunoNotaDto = ObterSalvarConselhoClasseAlunoNotaDto(COMPONENTE_CURRICULAR_PORTUGUES_ID_138,TipoNota.Nota,FECHAMENTO_TURMA_ID_4,BIMESTRE_4);
            
            var obterFiltroConselhoClasse = ObterFiltroConselhoClasse(ObterPerfilProfessor(), 
                salvarConselhoClasseAlunoNotaDto.ConselhoClasseNotaDto.CodigoComponenteCurricular, 
                TipoNota.Conceito, 
                ANO_4, 
                Modalidade.Fundamental, 
                ModalidadeTipoCalendario.FundamentalMedio, 
                false);

            await CriarDadosBaseSemFechamentoTurmaSemAberturaReabertura(obterFiltroConselhoClasse);
            
            await InserirPeriodoEscolarCustomizadoQuartoBimestre();

            await CriarFechamentoTurmaDisciplinaAlunoNota(obterFiltroConselhoClasse);

            await ValidarTesteComExcecao(salvarConselhoClasseAlunoNotaDto);
        }
        
        [Fact]
        public async Task Deve_lancar_nota_numerica_pos_conselho_sem_periodo_abertura_em_periodo_reabertura_pos_encerramento_bimestre()
        {
            var salvarConselhoClasseAlunoNotaDto = ObterSalvarConselhoClasseAlunoNotaDto(COMPONENTE_CURRICULAR_PORTUGUES_ID_138,TipoNota.Nota,FECHAMENTO_TURMA_ID_4,BIMESTRE_4);
            
            var obterFiltroConselhoClasse = ObterFiltroConselhoClasse(ObterPerfilProfessor(), 
                salvarConselhoClasseAlunoNotaDto.ConselhoClasseNotaDto.CodigoComponenteCurricular, 
                TipoNota.Conceito, 
                ANO_4, 
                Modalidade.Fundamental, 
                ModalidadeTipoCalendario.FundamentalMedio, 
                false);

            await CriarDadosBaseSemFechamentoTurmaSemAberturaReabertura(obterFiltroConselhoClasse);
            
            await InserirPeriodoEscolarCustomizadoQuartoBimestre();

            await CriarFechamentoTurmaDisciplinaAlunoNota(obterFiltroConselhoClasse);

            await CriarPeriodoReaberturaCustomizadoQuartoBimestre(obterFiltroConselhoClasse);

            await ExecutarTeste(salvarConselhoClasseAlunoNotaDto, false,TipoNota.Nota);
        }
        
        [Fact]
        public async Task Deve_lancar_nota_numerica_pos_conselho_durante_periodo_abertura_pos_encerramento_bimestre()
        {
            var salvarConselhoClasseAlunoNotaDto = ObterSalvarConselhoClasseAlunoNotaDto(COMPONENTE_CURRICULAR_PORTUGUES_ID_138,TipoNota.Nota,FECHAMENTO_TURMA_ID_4,BIMESTRE_4);
            
            var obterFiltroConselhoClasse = ObterFiltroConselhoClasse(ObterPerfilProfessor(), 
                salvarConselhoClasseAlunoNotaDto.ConselhoClasseNotaDto.CodigoComponenteCurricular, 
                TipoNota.Conceito, 
                ANO_4, 
                Modalidade.Fundamental, 
                ModalidadeTipoCalendario.FundamentalMedio, 
                false);

            await CriarDadosBaseSemFechamentoTurmaSemAberturaReabertura(obterFiltroConselhoClasse);
            
            await InserirPeriodoEscolarCustomizadoQuartoBimestre();

            await CriarFechamentoTurmaDisciplinaAlunoNota(obterFiltroConselhoClasse);

            await CriarPeriodoAberturaCustomizado(obterFiltroConselhoClasse);

            await ExecutarTeste(salvarConselhoClasseAlunoNotaDto, false,TipoNota.Nota);
        }
        
        
        private FiltroConselhoClasseDto ObterFiltroConselhoClasse(string perfil, long componente, TipoNota tipo, string anoTurma, Modalidade modalidade, ModalidadeTipoCalendario modalidadeTipoCalendario, bool anoAnterior, SituacaoConselhoClasse situacaoConselhoClasse = SituacaoConselhoClasse.NaoIniciado, bool criarFechamentoDisciplinaAlunoNota = false)
        {
            var dataAula = anoAnterior ? DATA_03_10_INICIO_BIMESTRE_4.AddYears(-1) : DATA_03_10_INICIO_BIMESTRE_4;

            return new FiltroConselhoClasseDto()
            {
                Perfil = perfil,
                Modalidade = modalidade,
                TipoCalendario = modalidadeTipoCalendario,
                Bimestre = BIMESTRE_4,
                ComponenteCurricular = componente.ToString(),
                TipoNota = tipo,
                AnoTurma = anoTurma,
                ConsiderarAnoAnterior = anoAnterior,
                DataAula = dataAula,
                CriarFechamentoDisciplinaAlunoNota = criarFechamentoDisciplinaAlunoNota,
                SituacaoConselhoClasse = situacaoConselhoClasse
            };
        }
    }
}