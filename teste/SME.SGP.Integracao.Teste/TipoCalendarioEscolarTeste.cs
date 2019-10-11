﻿using Newtonsoft.Json;
using SME.SGP.Dominio;
using SME.SGP.Dto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using Xunit;
using Xunit.Extensions.Ordering;

namespace SME.SGP.Integracao.Teste
{
    [Collection("Testserver collection")]
    public class TipoCalendarioEscolarTeste
    {
        private readonly TestServerFixture _fixture;

        public TipoCalendarioEscolarTeste(TestServerFixture fixture)
        {
            _fixture = fixture;
        }

        [Fact, Order(1)]
        public void Deve_Incluir_Consultar_Tipo_Calendario()
        {
            _fixture._clientApi.DefaultRequestHeaders.Clear();

            _fixture._clientApi.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", _fixture.GerarToken(new Permissao[] { Permissao.C_C, Permissao.C_I }));

            var tipoCalendarioDto = new TipoCalendarioEscolarDto();
            tipoCalendarioDto.AnoLetivo = 2019;
            tipoCalendarioDto.Nome = "Teste 1";
            tipoCalendarioDto.Periodo = Periodo.Anual;
            tipoCalendarioDto.Modalidade = Modalidade.Medio;
            tipoCalendarioDto.Situacao = true;

            var tipoCalendarioDto2 = new TipoCalendarioEscolarDto();
            tipoCalendarioDto2.AnoLetivo = 2019;
            tipoCalendarioDto2.Nome = "Teste 2";
            tipoCalendarioDto2.Periodo = Periodo.Semestral;
            tipoCalendarioDto2.Modalidade = Modalidade.EJA;
            tipoCalendarioDto2.Situacao = true;

            var jsonParaPost = new StringContent(TransformarEmJson(tipoCalendarioDto), UnicodeEncoding.UTF8, "application/json");

            var postResult = _fixture._clientApi.PostAsync("api/v1/tipo-calendario-escolar/", jsonParaPost).Result;

            Assert.True(postResult.IsSuccessStatusCode);


            if (postResult.IsSuccessStatusCode)
            {
                var getAllResult = _fixture._clientApi.GetAsync($"api/v1/tipo-calendario-escolar").Result;
                var dtoTodos = JsonConvert.DeserializeObject<IEnumerable<TipoCalendarioEscolarDto>>(getAllResult.Content.ReadAsStringAsync().Result);

                Assert.True(dtoTodos.Count() == 1);

                var getOneResult = _fixture._clientApi.GetAsync($"api/v1/tipo-calendario-escolar/1").Result;
                var dtoUm = JsonConvert.DeserializeObject<TipoCalendarioEscolarCompletoDto>(getOneResult.Content.ReadAsStringAsync().Result);

                Assert.Equal(dtoUm.Nome, tipoCalendarioDto.Nome);
                Assert.Equal(dtoUm.AnoLetivo, tipoCalendarioDto.AnoLetivo);
                Assert.Equal(dtoUm.Modalidade, tipoCalendarioDto.Modalidade);
                Assert.Equal(dtoUm.Periodo, tipoCalendarioDto.Periodo);
                Assert.Equal(dtoUm.Situacao, tipoCalendarioDto.Situacao);
            }

        }

        [Fact, Order(2)]
        public void Deve_Incluir_Excluir_Consular_Tipo_Calendario()
        {
            _fixture._clientApi.DefaultRequestHeaders.Clear();

            _fixture._clientApi.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", _fixture.GerarToken(new Permissao[] { Permissao.C_C, Permissao.C_I, Permissao.C_E }));

            var tipoCalendarioDto = new TipoCalendarioEscolarDto();
            tipoCalendarioDto.AnoLetivo = 2019;
            tipoCalendarioDto.Nome = "Teste 1";
            tipoCalendarioDto.Periodo = Periodo.Anual;
            tipoCalendarioDto.Modalidade = Modalidade.Medio;
            tipoCalendarioDto.Situacao = true;

            var tipoCalendarioDto2 = new TipoCalendarioEscolarDto();
            tipoCalendarioDto2.AnoLetivo = 2019;
            tipoCalendarioDto2.Nome = "Teste 2";
            tipoCalendarioDto2.Periodo = Periodo.Semestral;
            tipoCalendarioDto2.Modalidade = Modalidade.EJA;
            tipoCalendarioDto2.Situacao = true;

            var jsonParaPost = new StringContent(TransformarEmJson(tipoCalendarioDto), UnicodeEncoding.UTF8, "application/json");
            var postResult = _fixture._clientApi.PostAsync("api/v1/tipo-calendario-escolar/", jsonParaPost).Result;


            var jsonParaPost2 = new StringContent(TransformarEmJson(tipoCalendarioDto2), UnicodeEncoding.UTF8, "application/json");
            var postResult2 = _fixture._clientApi.PostAsync("api/v1/tipo-calendario-escolar/", jsonParaPost2).Result;

            Assert.True(postResult.IsSuccessStatusCode);
            Assert.True(postResult2.IsSuccessStatusCode);


            if (postResult.IsSuccessStatusCode && postResult2.IsSuccessStatusCode)
            {
                var ids = new int[1];
                ids[0] = 1;
                var jsonDelete = new StringContent(JsonConvert.SerializeObject(ids), UnicodeEncoding.UTF8, "application/json");
                HttpRequestMessage request = new HttpRequestMessage
                {
                    Content = jsonDelete,
                    Method = HttpMethod.Delete,
                    RequestUri = new Uri($"{ _fixture._clientApi.BaseAddress}api/v1/tipo-calendario-escolar/")
                };

                var deleteResult = _fixture._clientApi.SendAsync(request).Result;

                Assert.True(deleteResult.IsSuccessStatusCode);

                var getAllResult = _fixture._clientApi.GetAsync($"api/v1/tipo-calendario-escolar").Result;
                var dtoTodos = JsonConvert.DeserializeObject<IEnumerable<TipoCalendarioEscolarDto>>(getAllResult.Content.ReadAsStringAsync().Result);

                Assert.True(dtoTodos.Count() == 1);

                var getOneResult = _fixture._clientApi.GetAsync($"api/v1/tipo-calendario-escolar/1").Result;
                var dtoUm = JsonConvert.DeserializeObject<TipoCalendarioEscolarCompletoDto>(getOneResult.Content.ReadAsStringAsync().Result);

                Assert.Null(dtoUm.Nome);
            }

        }

        private string TransformarEmJson(object model)
        {
            return JsonConvert.SerializeObject(model);
        }
    }
}
