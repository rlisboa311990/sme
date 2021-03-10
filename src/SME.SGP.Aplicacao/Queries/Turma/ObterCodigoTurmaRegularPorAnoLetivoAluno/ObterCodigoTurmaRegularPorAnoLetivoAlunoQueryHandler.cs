﻿using MediatR;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace SME.SGP.Aplicacao
{
    public class ObterCodigoTurmaRegularPorAnoLetivoAlunoQueryHandler : IRequestHandler<ObterCodigoTurmaRegularPorAnoLetivoAlunoQuery, IEnumerable<string>>
    {
        private readonly IHttpClientFactory httpClientFactory;

        public ObterCodigoTurmaRegularPorAnoLetivoAlunoQueryHandler(IHttpClientFactory httpClientFactory)
        {
            this.httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
        }

        public async Task<IEnumerable<string>> Handle(ObterCodigoTurmaRegularPorAnoLetivoAlunoQuery request, CancellationToken cancellationToken)
        {
            var tiposTurma = String.Join("&tiposTurma=", request.TiposTurmas);
            var httpClient = httpClientFactory.CreateClient("servicoEOL");
            var resposta = await httpClient.GetAsync($"turmas/anos-letivos/{request.AnoLetivo}/alunos/{request.CodigoAluno}/regulares?tiposTurma={tiposTurma}");
            if (resposta.IsSuccessStatusCode)
            {
                var json = await resposta.Content.ReadAsStringAsync();
                return  JsonConvert.DeserializeObject<IEnumerable<string>>(json);
            }
            return Enumerable.Empty<string>(); ;
        }
    }
}
