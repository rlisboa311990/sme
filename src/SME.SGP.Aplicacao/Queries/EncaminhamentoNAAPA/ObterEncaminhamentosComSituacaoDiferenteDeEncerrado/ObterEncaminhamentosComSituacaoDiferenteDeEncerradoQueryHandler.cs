﻿using MediatR;
using SME.SGP.Dominio.Interfaces;
using SME.SGP.Infra;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace SME.SGP.Aplicacao
{
    internal class ObterEncaminhamentosComSituacaoDiferenteDeEncerradoQueryHandler : IRequestHandler<ObterEncaminhamentosComSituacaoDiferenteDeEncerradoQuery, IEnumerable<EncaminhamentoNAAPADto>>
    {
        private readonly IRepositorioEncaminhamentoNAAPA repositorioEncaminhamentoNaapa;

        public ObterEncaminhamentosComSituacaoDiferenteDeEncerradoQueryHandler(IRepositorioEncaminhamentoNAAPA repositorioEncaminhamentoNaapa)
        {
            this.repositorioEncaminhamentoNaapa = repositorioEncaminhamentoNaapa ?? throw new ArgumentNullException(nameof(repositorioEncaminhamentoNaapa));
        }

        public async Task<IEnumerable<EncaminhamentoNAAPADto>> Handle(ObterEncaminhamentosComSituacaoDiferenteDeEncerradoQuery request, CancellationToken cancellationToken)
        {
            return await this.repositorioEncaminhamentoNaapa.ObterEncaminhamentosComSituacaoDiferenteDeEncerrado();
        }
    }
}
