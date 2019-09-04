﻿using Newtonsoft.Json;
using SME.SGP.Dto;
using System.Collections.Generic;
using Xunit;
using Xunit.Extensions.Ordering;

namespace SME.SGP.Integracao.Teste
{
    [Collection("Testserver collection")]
    public class SupervisorTeste
    {
        private readonly TestServerFixture _fixture;

        public SupervisorTeste(TestServerFixture fixture)
        {
            _fixture = fixture;
        }

        [Fact, Order(1)]
        public void Deve_Consultar_Escolas_Por_Dre()
        {
            _fixture._clientApi.DefaultRequestHeaders.Clear();

            var postResult = _fixture._clientApi.GetAsync("api/v1/supervisores/dre/108100").Result;

            if (postResult.IsSuccessStatusCode)
            {
                var supervisorEscolasDto = JsonConvert.DeserializeObject<List<SupervisorEscolasDto>>(postResult.Content.ReadAsStringAsync().Result);
                Assert.True(supervisorEscolasDto.Count > 0);
            }
        }

        [Theory, Order(2)]
        [InlineData("108100", "an", true, true)]
        [InlineData("108100", "ma", true, true)]
        [InlineData("108100", "xy", false, true)]
        [InlineData("108100", "", true, true)]
        [InlineData("108100", "a", false, false)]
        public void Deve_Consultar_Supervisores_Por_Nome_e_Dre(string dreId, string parteNome, bool temSupervisores, bool sucesso)
        {
            _fixture._clientApi.DefaultRequestHeaders.Clear();

            var postResult = _fixture._clientApi.GetAsync($"api/v1/supervisores/dre/{dreId}?nome={parteNome}").Result;

            Assert.Equal(sucesso, postResult.IsSuccessStatusCode);

            if (postResult.IsSuccessStatusCode)
            {
                var supervisorEscolasDto = JsonConvert.DeserializeObject<List<SupervisorDto>>(postResult.Content.ReadAsStringAsync().Result);
                Assert.Equal(temSupervisores, supervisorEscolasDto.Count > 0);
            }
        }

        //TODO FAZER TESTE COM CONSULTA DO SUPERVISOR, POREM AINDA NÃO HÁ ENDPOINT DE CADASTRO.

        private string TransformarEmJson(object model)
        {
            return JsonConvert.SerializeObject(model);
        }
    }
}