﻿using MediatR;
using SME.SGP.Dominio;
using SME.SGP.Dominio.Interfaces;
using SME.SGP.Infra;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SME.SGP.Aplicacao.Commands
{
    public class RegistrarEncaminhamentoAEESecaoCommandHandler : IRequestHandler<RegistrarEncaminhamentoAEESecaoCommand, long>
    {
        private readonly IRepositorioEncaminhamentoAEESecao repositorioEncaminhamentoAEESecao;

        public RegistrarEncaminhamentoAEESecaoCommandHandler(IRepositorioEncaminhamentoAEESecao repositorioEncaminhamentoAEESecao)
        {
            this.repositorioEncaminhamentoAEESecao = repositorioEncaminhamentoAEESecao ?? throw new ArgumentNullException(nameof(repositorioEncaminhamentoAEESecao));
        }

        public async Task<long> Handle(RegistrarEncaminhamentoAEESecaoCommand request, CancellationToken cancellationToken)
        {
            var secao = MapearParaEntidade(request);
            var id = await repositorioEncaminhamentoAEESecao.SalvarAsync(secao);
            return id;
        }

        private EncaminhamentoAEESecao MapearParaEntidade(RegistrarEncaminhamentoAEESecaoCommand request)
            => new EncaminhamentoAEESecao()
            {
                SecaoEncaminhamentoAEEId = request.SecaoId,
                Concluido = request.Concluido,
                EncaminhamentoAEEId = request.EncaminhamentoAEEId
            };
    }
}
