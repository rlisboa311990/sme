﻿using MediatR;
using Newtonsoft.Json;
using Sentry;
using SME.SGP.Dominio;
using SME.SGP.Dto;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SME.SGP.Aplicacao
{
    public class ObterEstruturaInstitucionalVigenteQueryHandler : IRequestHandler<ObterEstruturaInstitucionalVigenteQuery, EstruturaInstitucionalRetornoEolDTO>
    {
        private readonly IHttpClientFactory httpClientFactory;
        private const string BaseUrl = "abrangencia/estrutura-vigente";

        public ObterEstruturaInstitucionalVigenteQueryHandler(IHttpClientFactory httpClientFactory)
        {
            this.httpClientFactory = httpClientFactory ?? throw new System.ArgumentNullException(nameof(httpClientFactory));
        }

        public async Task<EstruturaInstitucionalRetornoEolDTO> Handle(ObterEstruturaInstitucionalVigenteQuery request, CancellationToken cancellationToken)
        {
            EstruturaInstitucionalRetornoEolDTO resultado = null;

            var httpClient = httpClientFactory.CreateClient("servicoEOL");

            var url = new StringBuilder(BaseUrl);

            resultado = new EstruturaInstitucionalRetornoEolDTO();

            var resposta = await httpClient.GetAsync($"{url}/{request.CodigoDre}", cancellationToken);

            if (resposta.IsSuccessStatusCode)
            {
                var json = resposta.Content.ReadAsStringAsync().Result;
                var parcial = JsonConvert.DeserializeObject<EstruturaInstitucionalRetornoEolDTO>(json);

                if (parcial != null)
                    resultado.Dres.AddRange(parcial.Dres);
            }
            else
            {
                SentrySdk.AddBreadcrumb($"Ocorreu um erro na tentativa de buscar os dados de Estrutura Institucional Vigente por Dre: {request.CodigoDre} - HttpCode {resposta.StatusCode} - Body {resposta.Content?.ReadAsStringAsync()?.Result ?? string.Empty}");
                throw new NegocioException($"Erro ao obter a estrutura organizacional vigente no EOL. URL base: {httpClient.BaseAddress}");
            }

            return resultado;
        }
    }
}