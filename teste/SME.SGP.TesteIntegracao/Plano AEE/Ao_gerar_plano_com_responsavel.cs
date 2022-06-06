﻿using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using SME.SGP.Aplicacao;
using SME.SGP.Dominio;
using SME.SGP.Dominio.Enumerados;
using SME.SGP.Infra;
using SME.SGP.TesteIntegracao.Setup;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace SME.SGP.TesteIntegracao.Plano_AEE
{
    public class Ao_gerar_plano_com_responsavel : TesteBase
    {
        private readonly ItensBasicosBuilder _builder;

        public Ao_gerar_plano_com_responsavel(CollectionFixture collectionFixture) : base(collectionFixture)
        {
            _builder = new ItensBasicosBuilder(this);
        }

        [Fact]
        public async Task Deve_gerar_plano_com_responsavel()
        {
            await _builder.CriaItensComunsEja();

            var useCase = ServiceProvider.GetService<ISalvarPlanoAEEUseCase>();

            var dto = new PlanoAEEPersistenciaDto()
            {
                AlunoCodigo = "7128291",
                ResponsavelRF = "6926886",
                TurmaId = 1,
                TurmaCodigo = "1",
                Questoes = new List<PlanoAEEQuestaoDto>(),
                Situacao = SituacaoPlanoAEE.ParecerCP
            };
            
            var retorno = await useCase.Executar(dto);

            retorno.ShouldNotBeNull();

            retorno.PlanoId.ShouldBe(1);

            var listaUsuario = ObterTodos<Usuario>();
            var usuario = listaUsuario.Find(usuario => usuario.CodigoRf == "6926886");
            var listaPlano = ObterTodos<PlanoAEE>();

            listaPlano.ShouldNotBeNull();
            listaPlano.Exists(plano => plano.ResponsavelId == usuario.Id).ShouldBeTrue();

        }
    }
}
