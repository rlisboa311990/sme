﻿using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using SME.SGP.Dominio;
using SME.SGP.Dominio.Interfaces;

namespace SME.SGP.Aplicacao
{
    public class
        SalvarNotaAtividadeAvaliativaGsaCommandHandler : AsyncRequestHandler<SalvarNotaAtividadeAvaliativaGsaCommand>
    {
        private readonly IRepositorioNotasConceitos repositorioConceitos;


        public SalvarNotaAtividadeAvaliativaGsaCommandHandler(
            IRepositorioNotasConceitos repositorioConceitos)
        {
            this.repositorioConceitos =
                repositorioConceitos ?? throw new ArgumentNullException(nameof(repositorioConceitos));
        }

        protected override async Task Handle(SalvarNotaAtividadeAvaliativaGsaCommand request,
            CancellationToken cancellationToken)
        {
            var notaConceito =
                await repositorioConceitos.ObterNotasPorId(request.NotaConceitoId);

            await AlterarAtividade(notaConceito, request);
        }


        private async Task AlterarAtividade(NotaConceito conceito,
            SalvarNotaAtividadeAvaliativaGsaCommand request)
        {
            if (conceito.TipoNota == TipoNota.Conceito)
            {
                conceito.ConceitoId = (long?)request.Nota;
            }
            else
            {
                conceito.Nota = request.Nota;
            }

            conceito.StatusGsa = request.StatusGsa;

            await repositorioConceitos.SalvarAsync(conceito);
        }
    }
}