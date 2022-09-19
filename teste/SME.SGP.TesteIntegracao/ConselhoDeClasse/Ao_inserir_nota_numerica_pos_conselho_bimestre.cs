﻿using System.Collections.Generic;
using SME.SGP.Dominio;
using SME.SGP.Infra;
using SME.SGP.TesteIntegracao.Setup;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using SME.SGP.Aplicacao;
using SME.SGP.TesteIntegracao.ConselhoDeClasse.ServicosFakes;
using Xunit;
using SME.SGP.TesteIntegracao.ServicosFakes;
using ObterAlunosAtivosPorTurmaCodigoQueryHandlerFake = SME.SGP.TesteIntegracao.ConselhoDeClasse.ServicosFakes.ObterAlunosAtivosPorTurmaCodigoQueryHandlerFake;
using ObterTurmaItinerarioEnsinoMedioQueryHandlerFake = SME.SGP.TesteIntegracao.ConselhoDeClasse.ServicosFakes.ObterTurmaItinerarioEnsinoMedioQueryHandlerFake;

namespace SME.SGP.TesteIntegracao.ConselhoDeClasse
{
    public class Ao_inserir_nota_numerica_pos_conselho_bimestre : ConselhoDeClasseTesteBase
    {
        public Ao_inserir_nota_numerica_pos_conselho_bimestre(CollectionFixture collectionFixture) : base(collectionFixture)
        {
            
        }

        protected override void RegistrarFakes(IServiceCollection services)
        {
            base.RegistrarFakes(services);

            services.Replace(new ServiceDescriptor(typeof(IRequestHandler<ObterComponentesCurricularesEOLPorTurmasCodigoQuery, IEnumerable<ComponenteCurricularDto>>), typeof(ObterComponentesCurricularesEOLPorTurmasCodigoQueryHandlerFake), ServiceLifetime.Scoped));
            services.Replace(new ServiceDescriptor(typeof(IRequestHandler<ObterTurmaItinerarioEnsinoMedioQuery, IEnumerable<TurmaItinerarioEnsinoMedioDto>>), typeof(ObterTurmaItinerarioEnsinoMedioQueryHandlerFake), ServiceLifetime.Scoped));
            services.Replace(new ServiceDescriptor(typeof(IRequestHandler<ObterAlunosAtivosPorTurmaCodigoQuery, IEnumerable<AlunoPorTurmaResposta>>), typeof(ObterAlunosAtivosPorTurmaCodigoQueryHandlerFake), ServiceLifetime.Scoped));
            services.Replace(new ServiceDescriptor(typeof(IRequestHandler<ProfessorPodePersistirTurmaQuery, bool>), typeof(ProfessorPodePersistirTurmaQueryHandlerComPermissaoFake), ServiceLifetime.Scoped));
        }
        
        [Theory]
        [InlineData(false)]
        //[InlineData(true)]
        public async Task Ao_lancar_nota_numerica_pos_conselho_bimestre_fundamental(bool anoAnterior) 
        {
            var salvarConselhoClasseAlunoNotaDto = ObterSalvarConselhoClasseAlunoNotaDto(COMPONENTE_CURRICULAR_PORTUGUES_ID_138,TipoNota.Nota);
            
            await CriarDados(ObterPerfilProfessor(), 
                            salvarConselhoClasseAlunoNotaDto.ConselhoClasseNotaDto.CodigoComponenteCurricular, 
                            ANO_7, 
                            Modalidade.Fundamental, 
                            ModalidadeTipoCalendario.FundamentalMedio,
                            anoAnterior, 
                            SituacaoConselhoClasse.EmAndamento, 
                            true);

            await ExecutarTeste(salvarConselhoClasseAlunoNotaDto, anoAnterior, TipoNota.Nota);
        }

        [Theory]
        [InlineData(false)]
        //[InlineData(true)]
        public async Task Ao_lancar_nota_numerica_pos_conselho_bimestre_fundamental_cp(bool anoAnterior)
        {
            var salvarConselhoClasseAlunoNotaDto = ObterSalvarConselhoClasseAlunoNotaDto(COMPONENTE_CURRICULAR_PORTUGUES_ID_138,TipoNota.Nota);
            
            await CriarDados(ObterPerfilCP(),
                            salvarConselhoClasseAlunoNotaDto.ConselhoClasseNotaDto.CodigoComponenteCurricular,
                            ANO_5,
                            Modalidade.Fundamental,
                            ModalidadeTipoCalendario.FundamentalMedio,
                            anoAnterior, 
                            SituacaoConselhoClasse.EmAndamento, 
                            true);

            await ExecutarTeste(salvarConselhoClasseAlunoNotaDto, anoAnterior, TipoNota.Nota,SituacaoConselhoClasse.EmAndamento,true);
        }

        [Theory]
        [InlineData(false)]
        //[InlineData(true)]
        public async Task Ao_lancar_nota_numerica_pos_conselho_bimestre_medio(bool anoAnterior)
        {
            var salvarConselhoClasseAlunoNotaDto = ObterSalvarConselhoClasseAlunoNotaDto(COMPONENTE_CURRICULAR_PORTUGUES_ID_138,TipoNota.Nota);
            
            await CriarDados(ObterPerfilProfessor(), 
                            salvarConselhoClasseAlunoNotaDto.ConselhoClasseNotaDto.CodigoComponenteCurricular, 
                            ANO_7, 
                            Modalidade.Medio, 
                            ModalidadeTipoCalendario.FundamentalMedio,
                            anoAnterior, 
                            SituacaoConselhoClasse.EmAndamento, 
                            true);

            await ExecutarTeste(salvarConselhoClasseAlunoNotaDto, anoAnterior, TipoNota.Nota);
        }

        [Theory]
        [InlineData(false)]
        //[InlineData(true)]
        public async Task Ao_lancar_nota_pos_conselho_bimestre_numerica_medio_diretor(bool anoAnterior)
        {
            var salvarConselhoClasseAlunoNotaDto = ObterSalvarConselhoClasseAlunoNotaDto(COMPONENTE_CURRICULAR_PORTUGUES_ID_138,TipoNota.Nota);
            
            await CriarDados(
                ObterPerfilDiretor(),
                salvarConselhoClasseAlunoNotaDto.ConselhoClasseNotaDto.CodigoComponenteCurricular,
                ANO_5,
                Modalidade.Medio,
                ModalidadeTipoCalendario.FundamentalMedio,
                anoAnterior, 
                SituacaoConselhoClasse.EmAndamento, 
                true);

            await ExecutarTeste(salvarConselhoClasseAlunoNotaDto, anoAnterior, TipoNota.Nota,SituacaoConselhoClasse.EmAndamento,true);
        }

        [Theory]
        [InlineData(false)]
        //[InlineData(true)]
        public async Task Ao_lancar_nota_pos_conselho_bimestre_numerica_eja(bool anoAnterior)
        {
            var salvarConselhoClasseAlunoNotaDto = ObterSalvarConselhoClasseAlunoNotaDto(COMPONENTE_CURRICULAR_PORTUGUES_ID_138,TipoNota.Nota);
            
            await CriarDados(ObterPerfilProfessor(), 
                            salvarConselhoClasseAlunoNotaDto.ConselhoClasseNotaDto.CodigoComponenteCurricular, 
                            ANO_9, 
                            Modalidade.EJA, 
                            ModalidadeTipoCalendario.EJA,
                            anoAnterior, 
                            SituacaoConselhoClasse.EmAndamento, 
                            true);

            await ExecutarTeste(salvarConselhoClasseAlunoNotaDto, anoAnterior, TipoNota.Nota);
        }

        [Theory]
        [InlineData(false)]
        //[InlineData(true)]
        public async Task Ao_lancar_nota_pos_conselho_bimestre_numerica_regencia_classe(bool anoAnterior)
        {
            var salvarConselhoClasseAlunoNotaDto = ObterSalvarConselhoClasseAlunoNotaDto(COMPONENTE_CURRICULAR_PORTUGUES_ID_138,TipoNota.Nota);
            
            await CriarDados(ObterPerfilProfessor(), 
                            salvarConselhoClasseAlunoNotaDto.ConselhoClasseNotaDto.CodigoComponenteCurricular, 
                            ANO_6, 
                            Modalidade.Fundamental, 
                            ModalidadeTipoCalendario.FundamentalMedio,
                            anoAnterior, 
                            SituacaoConselhoClasse.EmAndamento, 
                            true);

            await ExecutarTeste(salvarConselhoClasseAlunoNotaDto, anoAnterior, TipoNota.Nota);
        }

        private async Task CriarDados(
                        string perfil, 
                        long componente,
                        string anoTurma, 
                        Modalidade modalidade,
                        ModalidadeTipoCalendario modalidadeTipoCalendario,
                        bool anoAnterior, 
                        SituacaoConselhoClasse situacaoConselhoClasse = SituacaoConselhoClasse.NaoIniciado, 
                        bool criarFechamentoDisciplinaAlunoNota = false)
        {
            var dataAula = anoAnterior ? DATA_02_05_INICIO_BIMESTRE_2.AddYears(-1) : DATA_02_05_INICIO_BIMESTRE_2;

            var filtroNota = new FiltroConselhoClasseDto()
            {
                Perfil = perfil,
                Modalidade = modalidade,
                TipoCalendario = modalidadeTipoCalendario,
                Bimestre = BIMESTRE_2,
                ComponenteCurricular = componente.ToString(),
                AnoTurma = anoTurma,
                ConsiderarAnoAnterior = anoAnterior,
                DataAula = dataAula,
                CriarFechamentoDisciplinaAlunoNota = criarFechamentoDisciplinaAlunoNota,
                SituacaoConselhoClasse = situacaoConselhoClasse
            };
            
            await CriarDadosBase(filtroNota);
        }
    }
}