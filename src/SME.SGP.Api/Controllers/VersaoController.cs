﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SME.SGP.Aplicacao;
using System.Threading.Tasks;

namespace SME.SGP.Api.Controllers
{
    [ApiController]
    [Route("api/v1/versoes")]
    [Authorize("Bearer")]
    public class VersaoController : ControllerBase
    {
        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> ObterUltimaVersao([FromServices]IObterUltimaVersaoUseCase obterUltimaVersaoUseCase)
        {
            return Ok(await obterUltimaVersaoUseCase.Executar());
        }

    }
}