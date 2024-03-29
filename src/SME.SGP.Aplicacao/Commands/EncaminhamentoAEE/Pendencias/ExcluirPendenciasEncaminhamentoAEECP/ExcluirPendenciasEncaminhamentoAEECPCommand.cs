﻿using MediatR;

namespace SME.SGP.Aplicacao
{
    public class ExcluirPendenciasEncaminhamentoAEECPCommand : IRequest<bool>
    {
        public ExcluirPendenciasEncaminhamentoAEECPCommand(long turmaId, long encaminhamentoId)
        {
            EncaminhamentoId = encaminhamentoId;
            TurmaId = turmaId;
        }

        public long TurmaId { get; }
        public long EncaminhamentoId { get; }
    }
}
