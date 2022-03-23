﻿using MediatR;
using Newtonsoft.Json;
using SME.SGP.Infra;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;

namespace SME.SGP.Aplicacao
{
    public class ObterMatriculasAlunoNaUEQueryHandler : IRequestHandler<ObterMatriculasAlunoNaUEQuery, IEnumerable<AlunosPorUeDto>>
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public ObterMatriculasAlunoNaUEQueryHandler(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
        }

        public async System.Threading.Tasks.Task<IEnumerable<AlunosPorUeDto>> Handle(ObterMatriculasAlunoNaUEQuery request, CancellationToken cancellationToken)
        {
            var alunos = new List<AlunosPorUeDto>();
            var httpClient = _httpClientFactory.CreateClient("servicoEOL");
            var resposta = await httpClient.GetAsync($"escolas/{request.UeCodigo}/aluno/{request.AlunoCodigo}/matriculas");

            if (resposta.IsSuccessStatusCode)
            {
                var json = await resposta.Content.ReadAsStringAsync();
                alunos = JsonConvert.DeserializeObject<List<AlunosPorUeDto>>(json);
            }

            return alunos;
        }
    }
}
