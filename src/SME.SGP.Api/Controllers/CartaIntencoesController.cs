﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SME.SGP.Api.Filtros;
using SME.SGP.Aplicacao.Interfaces;
using SME.SGP.Infra;
using SME.SGP.Infra.Dtos;
using System.Threading.Tasks;

namespace SME.SGP.Api.Controllers
{
    [ApiController]
    [Route("api/v1/carta-intencoes")]
    [Authorize("Bearer")]
    public class CartaIntencoesController : ControllerBase
    {
        [HttpPost]
        [ProducesResponseType(typeof(AuditoriaDto), 200)]
        [ProducesResponseType(typeof(RetornoBaseDto), 500)]
        [Permissao(Permissao.CI_I, Policy = "Bearer")]
        public async Task<IActionResult> Salvar([FromServices] ICartaIntencoesPersistenciaUseCase useCase, [FromBody] CartaIntencoesPersistenciaDto dto)
        {
            return Ok(await useCase.Executar(dto));
        }
    }
}
