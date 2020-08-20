﻿using MediatR;
using Moq;
using SME.SGP.Dominio;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace SME.SGP.Aplicacao.Teste.CasosDeUso
{
    public class AlterarDevolutivaUseCaseTeste
    {
        private readonly AlterarDevolutivaUseCase inserirDevolutivaUseCase;
        private readonly Mock<IMediator> mediator;

        public AlterarDevolutivaUseCaseTeste()
        {
            mediator = new Mock<IMediator>();
            inserirDevolutivaUseCase = new AlterarDevolutivaUseCase(mediator.Object);
        }

        [Fact]
        public async Task Deve_Alterar_Devolutiva()
        {
            //Arrange
            mediator.Setup(a => a.Send(It.IsAny<ObterDevolutivaPorIdQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new Devolutiva
                {
                    Id = 1,
                    CodigoComponenteCurricular = 1,
                    Descricao = "teste",
                    PeriodoInicio = DateTime.Today.AddDays(-15),
                    PeriodoFim = DateTime.Today.AddDays(15)
                });

            mediator.Setup(a => a.Send(It.IsAny<AlterarDevolutivaCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new Infra.AuditoriaDto()
                {
                    Id = 1
                });

            mediator.Setup(a => a.Send(It.IsAny<AtualizarDiarioBordoComDevolutivaCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            mediator.Setup(a => a.Send(It.IsAny<ObterDatasEfetivasDiariosQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<Tuple<long, DateTime>> {
                    new Tuple<long, DateTime>(1, DateTime.Today.AddDays(-15)),
                    new Tuple<long, DateTime>(2, DateTime.Today.AddDays(-5)),
                    new Tuple<long, DateTime>(3, DateTime.Today.AddDays(-10)),
                    new Tuple<long, DateTime>(4, DateTime.Today.AddDays(5)),
                    new Tuple<long, DateTime>(5, DateTime.Today.AddDays(10)),
                    new Tuple<long, DateTime>(6, DateTime.Today.AddDays(15))
                });

            //Act
            var auditoriaDto = await inserirDevolutivaUseCase.Executar(new Infra.AlterarDevolutivaDto()
            {
                Id = 1,
                CodigoComponenteCurricular = 1,
                Descricao = "teste",
                PeriodoInicio = DateTime.Today.AddDays(-15),
                PeriodoFim = DateTime.Today.AddDays(15),
            });

            //Asert
            mediator.Verify(x => x.Send(It.IsAny<AlterarDevolutivaCommand>(), It.IsAny<CancellationToken>()), Times.Once);

            Assert.True(auditoriaDto.Id == 1);
        }
    }
}
