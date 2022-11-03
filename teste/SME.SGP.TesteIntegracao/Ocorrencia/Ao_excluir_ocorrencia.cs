﻿using System.Linq;
using System.Threading.Tasks;
using Shouldly;
using SME.SGP.Dominio;
using SME.SGP.Infra;
using SME.SGP.TesteIntegracao.Ocorrencia.Base;
using SME.SGP.TesteIntegracao.Setup;
using Xunit;

namespace SME.SGP.TesteIntegracao.Ocorrencia
{
    public class Ao_excluir_ocorrencia : OcorrenciaTesteBase
    {
        public Ao_excluir_ocorrencia(CollectionFixture collectionFixture) : base(collectionFixture)
        {
        }

        [Fact(DisplayName = "Ocorrencia - Excluir Ocorrencia com Turma")]
        public async Task ExcluirOcorrenciaComTurma()
        {
             await CriarDadosBasicos();
            // var dtoIncluir = new InserirOcorrenciaDto
            // {
            //     AnoLetivo = DateTimeExtension.HorarioBrasilia().Year,
            //     DreId = 1,
            //     UeId = 1,
            //     Modalidade = 5,
            //     Semestre = 3,
            //     TurmaId = 1,
            //     DataOcorrencia = DateTimeExtension.HorarioBrasilia(),
            //     Titulo = "Lorem ipsum",
            //     Descricao = "Lorem Ipsum é simplesmente uma simulação de texto da",
            //     OcorrenciaTipoId = 1,
            //     HoraOcorrencia = DateTimeExtension.HorarioBrasilia().Hour.ToString(),
            // };
            // var useCaseIncluir = InserirOcorrenciaUseCase();
            // await useCaseIncluir.Executar(dtoIncluir);
            //
            // var obterTodasOcorrencias = ObterTodos<Dominio.Ocorrencia>();
            // obterTodasOcorrencias.ShouldNotBeNull();
            // obterTodasOcorrencias.Count.ShouldBeEquivalentTo(1);
            //
            //
            // var useCaseExcluir = ExcluirOcorrenciaUseCase();
            // await useCaseExcluir.Executar(obterTodasOcorrencias.Select(x => x.Id));
            //
            // var obterTodasOcorrenciasAposExcluir = ObterTodos<Dominio.Ocorrencia>();
            // obterTodasOcorrenciasAposExcluir.ShouldNotBeNull();
            // obterTodasOcorrenciasAposExcluir.Count.ShouldBeEquivalentTo(1);
        }
    }
}