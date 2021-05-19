﻿using MediatR;
using Newtonsoft.Json;
using SME.SGP.Infra;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace SME.SGP.Aplicacao
{
    public class ObterMatriculasPorTurmaConsolidacaoQueryHandler : IRequestHandler<ObterMatriculasPorTurmaConsolidacaoQuery, IEnumerable<ConsolidacaoMatriculaTurmaDto>>
    {
        private readonly IHttpClientFactory httpClientFactory;

        public ObterMatriculasPorTurmaConsolidacaoQueryHandler(IHttpClientFactory httpClientFactory)
        {
            this.httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
        }

        public async Task<IEnumerable<ConsolidacaoMatriculaTurmaDto>> Handle(ObterMatriculasPorTurmaConsolidacaoQuery request, CancellationToken cancellationToken)
        {
            var matriculasConsolidadas = new List<ConsolidacaoMatriculaTurmaDto>();

            var httpClient = httpClientFactory.CreateClient("servicoEOL");
            var resposta = await httpClient.GetAsync($"api/matriculas?anoLetivo={request.AnoLetivo}&ueCodigo={request.UeCodigo}");
            if (resposta.IsSuccessStatusCode)
            {
                var json = await resposta.Content.ReadAsStringAsync();
                matriculasConsolidadas = JsonConvert.DeserializeObject<List<ConsolidacaoMatriculaTurmaDto>>(json);
            }
            return matriculasConsolidadas;
        }
    }
}
