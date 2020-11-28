﻿using MediatR;
using SME.SGP.Dominio;
using SME.SGP.Dominio.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SME.SGP.Aplicacao
{
    public class ObterTurmasComFechamentoOuConselhoNaoFinalizadosQueryHandler : IRequestHandler<ObterTurmasComFechamentoOuConselhoNaoFinalizadosQuery, IEnumerable<Turma>>
    {
        private readonly IRepositorioTurma repositorioTurma;

        public ObterTurmasComFechamentoOuConselhoNaoFinalizadosQueryHandler(IRepositorioTurma repositorioTurma)
        {
            this.repositorioTurma = repositorioTurma ?? throw new ArgumentNullException(nameof(repositorioTurma));
        }

        public async Task<IEnumerable<Turma>> Handle(ObterTurmasComFechamentoOuConselhoNaoFinalizadosQuery request, CancellationToken cancellationToken)
            => await repositorioTurma.ObterTurmasComFechamentoOuConselhoNaoFinalizados(request.UeId, request.PeriodoEscolarId, request.Modalidades);
    }
}
