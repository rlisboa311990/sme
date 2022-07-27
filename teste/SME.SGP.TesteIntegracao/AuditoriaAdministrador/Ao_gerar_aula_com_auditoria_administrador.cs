﻿using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Shouldly;
using SME.SGP.Aplicacao;
using SME.SGP.Aplicacao.Interfaces;
using SME.SGP.Dominio;
using SME.SGP.Infra;
using SME.SGP.TesteIntegracao.ServicosFakes;
using SME.SGP.TesteIntegracao.Setup;
using System;
using System.Threading.Tasks;
using SME.SGP.Infra.Interface;
using SME.SGP.TesteIntegracao.Aula.ServicosFakes;
using Xunit;

namespace SME.SGP.TesteIntegracao.AulaUnica
{
    public class Ao_gerar_aula_com_auditoria_administrador : TesteBase
    {
        private ItensBasicosBuilder _buider;
        public Ao_gerar_aula_com_auditoria_administrador(CollectionFixture testFixture) : base(testFixture)
        {
            _buider = new ItensBasicosBuilder(this);
        }

        protected override void RegistrarFakes(IServiceCollection services)
        {
            base.RegistrarFakes(services);

            services.Replace(new ServiceDescriptor(typeof(IRequestHandler<ObterUsuarioPossuiPermissaoNaTurmaEDisciplinaQuery, bool>), typeof(ObterUsuarioPossuiPermissaoNaTurmaEDisciplinaQueryHandlerComPermissaoFake), ServiceLifetime.Scoped));
            services.Replace(new ServiceDescriptor(typeof(IServicoAuditoria),typeof(ServicoAuditoriaFakeAdministrador), ServiceLifetime.Scoped));
            
        }

        [Fact]
        public async Task Deve_gravar_aula_com_auditoria_para_administrador()
        {

            await _buider.CriaItensComunsEja(true);

            var useCase = ServiceProvider.GetService<IInserirAulaUseCase>();

            var dto = new PersistirAulaDto()
            {
                CodigoTurma = "1",
                CodigoComponenteCurricular = 1106,
                DataAula = new (DateTimeExtension.HorarioBrasilia().Year, 02, 10),
                Quantidade = 1,
                CodigoUe = "1",
                TipoAula = TipoAula.Normal,
                TipoCalendarioId = 1,
                RecorrenciaAula = RecorrenciaAula.AulaUnica,
                NomeComponenteCurricular = "teste"
            };

            var retorno = await useCase.Executar(dto);

            Assert.IsType<RetornoBaseDto>(retorno);
        }
    }
}
