﻿using MediatR;
using SME.SGP.Dominio;
using SME.SGP.Dominio.Interfaces;
using SME.SGP.Infra;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SME.SGP.Aplicacao
{
    public class AprovarItineranciaCommandHandler : IRequestHandler<AprovarItineranciaCommand, bool>
    {
        private readonly IRepositorioWfAprovacaoItinerancia repositorioWfAprovacaoItinerancia;
        private readonly IRepositorioItinerancia repositorioItinerancia;
        private readonly IUnitOfWork unitOfWork;
        private readonly IMediator mediator;

        public AprovarItineranciaCommandHandler(IRepositorioWfAprovacaoItinerancia repositorioWfAprovacaoItinerancia, IRepositorioItinerancia repositorioItinerancia, IUnitOfWork unitOfWork, IMediator mediator)
        {
            this.repositorioWfAprovacaoItinerancia = repositorioWfAprovacaoItinerancia ?? throw new ArgumentNullException(nameof(repositorioWfAprovacaoItinerancia));
            this.repositorioItinerancia = repositorioItinerancia ?? throw new ArgumentNullException(nameof(repositorioItinerancia));
            this.unitOfWork = unitOfWork;
            this.mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        }

        public async Task<bool> Handle(AprovarItineranciaCommand request, CancellationToken cancellationToken)
        {
            var wfAprovacaoItinerancia = await repositorioWfAprovacaoItinerancia.ObterPorWorkflowId(request.WorkflowId);
            var itinerancia = await repositorioItinerancia.ObterComUesPorId(request.ItineranciaId);
            var objetivos = await repositorioItinerancia.ObterDecricaoObjetivosPorId(request.ItineranciaId);

            using (var transacao = unitOfWork.IniciarTransacao())
            {
                try
                {
                    await repositorioWfAprovacaoItinerancia.SalvarAsync(wfAprovacaoItinerancia);

                    if (itinerancia.DataRetornoVerificacao.HasValue)
                        await CriarEvento(itinerancia, objetivos);

                    unitOfWork.PersistirTransacao();
                }
                catch (Exception e)
                {
                    unitOfWork.Rollback();
                    throw;
                }            
            }

            return true;
        }

        private async Task CriarEvento(Itinerancia itinerancia, IEnumerable<ItineranciaObjetivoDescricaoDto> objetivos)
        {
            foreach(var ue in itinerancia.Ues)
            {
                await mediator.Send(new CriarEventoItineranciaPAAICommand(
                    itinerancia.Id,
                    ue.Ue.Dre.CodigoDre,
                    ue.Ue.CodigoUe,
                    itinerancia.DataRetornoVerificacao.Value,
                    itinerancia.DataVisita,
                    objetivos));
            }
        }
    }
}
