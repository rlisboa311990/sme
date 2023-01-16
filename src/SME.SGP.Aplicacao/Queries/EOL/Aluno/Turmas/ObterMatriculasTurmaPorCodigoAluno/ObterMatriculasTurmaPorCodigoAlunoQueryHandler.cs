﻿using MediatR;
using Newtonsoft.Json;
using SME.SGP.Dominio;
using SME.SGP.Dominio.Enumerados;
using SME.SGP.Infra;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace SME.SGP.Aplicacao
{
    public class ObterMatriculasTurmaPorCodigoAlunoQueryHandler : IRequestHandler<ObterMatriculasTurmaPorCodigoAlunoQuery, IEnumerable<AlunoPorTurmaResposta>>
    {
        private readonly IHttpClientFactory httpClientFactory;
        private readonly IMediator mediator;

        public ObterMatriculasTurmaPorCodigoAlunoQueryHandler(IHttpClientFactory httpClientFactory, IMediator mediator)
        {
            this.httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
            this.mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        }
        public async Task<IEnumerable<AlunoPorTurmaResposta>> Handle(ObterMatriculasTurmaPorCodigoAlunoQuery request, CancellationToken cancellationToken)
        {
            var httpClient = httpClientFactory.CreateClient("servicoEOL");
            var queryParam = $"{(request.AnoLetivo.HasValue ? $"anoLetivo={request.AnoLetivo.Value}{(request.AnoLetivo.HasValue ? $"&" : string.Empty)}" : string.Empty)}";
            queryParam = queryParam + $"{(request.DataAula.HasValue ? $"dataAulaTicks={request.DataAula.Value.Ticks}" : string.Empty)}";
            var url = $"/api/turmas/alunos/{request.CodigoAluno}{(!string.IsNullOrEmpty(queryParam) ? $"?{queryParam}" : string.Empty)}";
            try
            {
                var resposta = await httpClient.GetAsync(url);
                if (resposta.IsSuccessStatusCode)
                {
                    var json = await resposta.Content.ReadAsStringAsync();
                    return JsonConvert.DeserializeObject<IEnumerable<AlunoPorTurmaResposta>>(json);
                }
                else
                {
                    string erro = $"Não foi possível obter as matrículas/turma do aluno no EOL - HttpCode {(int)resposta.StatusCode} - erro: {JsonConvert.SerializeObject(resposta.RequestMessage)}";
                    await mediator.Send(new SalvarLogViaRabbitCommand(erro, LogNivel.Negocio, LogContexto.Turma, string.Empty));
                    var respostaErro = resposta?.Content != null ? resposta?.Content?.ReadAsStringAsync()?.Result.ToString() : erro;
                    throw new Exception(respostaErro);
                }
            }
            catch (Exception e)
            {

                await mediator.Send(new SalvarLogViaRabbitCommand($"Erro ao obter as matrículas/turma do aluno no EOL - Código:{request.CodigoAluno}, Ano:{request.AnoLetivo}, Data Aula:{request.DataAula} - Erro:{e.Message}", LogNivel.Negocio, LogContexto.Turma, e.Message));
                throw e;
            }
        }
    }
}
