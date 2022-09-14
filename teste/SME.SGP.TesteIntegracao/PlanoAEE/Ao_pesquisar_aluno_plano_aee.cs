﻿using System.Linq;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Shouldly;
using SME.SGP.Aplicacao;
using SME.SGP.Dominio;
using SME.SGP.Infra;
using SME.SGP.TesteIntegracao.PlanoAEE.ServicosFakes;
using SME.SGP.TesteIntegracao.Setup;
using Xunit;

namespace SME.SGP.TesteIntegracao.PlanoAEE
{
    public class Ao_pesquisar_aluno_plano_aee : PlanoAEETesteBase
    {
        public Ao_pesquisar_aluno_plano_aee(CollectionFixture collectionFixture) : base(collectionFixture)
        {
            
        }

        protected override void RegistrarFakes(IServiceCollection services)
        {
            base.RegistrarFakes(services);
            services.Replace(new ServiceDescriptor(typeof(IRequestHandler<VerificarExistenciaPlanoAEEPorEstudanteQuery, PlanoAEEResumoDto>), typeof(VerificarExistenciaPlanoAEEPorEstudanteQueryHandlerFake), ServiceLifetime.Scoped));
        }


        [Fact(DisplayName = "Selecionar aluno que já possua plano validado (deve apresentar mensagem de erro)")]
        public async Task Selecionar_aluno_com_plano_validado()
        {
            var filtro = new FiltroBuscaEstudanteDto()
            {
                AnoLetivo = DateTimeExtension.HorarioBrasilia().Year.ToString(),
                CodigoUe = "1",
                CodigoTurma = 1,
                Codigo = int.Parse(ALUNO_CODIGO_10),
                Nome = ALUNO_CODIGO_10
            };
            var obterAlunosServico = ObterAlunosPorCodigoEolNomeUseCase();
            var aluno = await obterAlunosServico.Executar(filtro);

            aluno.ShouldNotBeNull();
            aluno.Items.Count().ShouldBeGreaterThanOrEqualTo(1);

            var verificarExistenciaPlanoAEE = ObterServicoVerificarExistenciaPlanoAEEPorEstudanteUseCase();
            var ex = await Assert.ThrowsAsync<NegocioException>(() => verificarExistenciaPlanoAEE.Executar(aluno.Items.FirstOrDefault().Codigo));
            ex.Message.ShouldNotBeNullOrEmpty();
        }
        
    }
}