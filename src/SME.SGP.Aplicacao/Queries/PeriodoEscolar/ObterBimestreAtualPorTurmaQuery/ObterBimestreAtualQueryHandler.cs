﻿using MediatR;
using SME.SGP.Dominio;
using SME.SGP.Dominio.Interfaces;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SME.SGP.Aplicacao
{
    public class ObterBimestreAtualQueryHandler : IRequestHandler<ObterBimestreAtualQuery, int>
    {
        private readonly IRepositorioPeriodoEscolar repositorioPeriodoEscolar;

        public ObterBimestreAtualQueryHandler(IRepositorioPeriodoEscolar repositorioPeriodoEscolar)
        {
            this.repositorioPeriodoEscolar = repositorioPeriodoEscolar ?? throw new ArgumentNullException(nameof(repositorioPeriodoEscolar));
        }
        public async Task<int> Handle(ObterBimestreAtualQuery request, CancellationToken cancellationToken)
        {
            Turma turma = request.Turma;

            return await repositorioPeriodoEscolar.ObterBimestreAtualAsync(turma.CodigoTurma, turma.ModalidadeTipoCalendario, request.DataReferencia);
        }
    }
}
