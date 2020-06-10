﻿using MediatR;
using SME.SGP.Infra;
using SME.SGP.Infra.Dtos;
using SME.SGP.Infra.Interfaces;
using System.Threading;
using System.Threading.Tasks;

namespace SME.SGP.Aplicacao
{
    public class GerarRelatorioCommandHandler : IRequestHandler<GerarRelatorioCommand, bool>
    {
        private readonly IServicoFila servicoFila;

        public GerarRelatorioCommandHandler(IServicoFila servicoFila)
        {
            this.servicoFila = servicoFila ?? throw new System.ArgumentNullException(nameof(servicoFila));
        }

        public Task<bool> Handle(GerarRelatorioCommand request, CancellationToken cancellationToken)
        {
            //TODO: VARIAVEL PARA NOME DA FILA
            servicoFila.AdicionaFilaWorkerRelatorios(new AdicionaFilaDto(RotasRabbit.RotaRelatoriosSolicitados, request.Filtros, request.TipoRelatorio.Name()));
            return Task.FromResult(true);
        }
    }
}
