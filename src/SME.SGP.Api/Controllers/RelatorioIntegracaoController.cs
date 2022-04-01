﻿using Microsoft.AspNetCore.Mvc;
using SME.SGP.Api.Middlewares;
using SME.SGP.Aplicacao;
using SME.SGP.Infra;
using System;
using System.Threading.Tasks;

namespace SME.SGP.Api.Controllers
{
    [Route("api/v1/relatorios/integracoes")]
    [ApiController]
    [ChaveIntegracaoSgpApi]
    public class RelatorioIntegracaoController : ControllerBase
    {
        [HttpGet("existe")]
        [ProducesResponseType(typeof(Boolean), 200)]
        [ProducesResponseType(typeof(RetornoBaseDto), 500)]
        [ChaveIntegracaoSgpApi]
        public async Task<IActionResult> VerificarSeRelatorioExiste([FromQuery] Guid codigoRelatorio, [FromServices] IObterDataCriacaoRelatorioUseCase useCase)
        {
            return Ok(await useCase.Executar(codigoRelatorio));
        }

        [HttpPost("boletim")]
        [ChaveIntegracaoSgpApi]
        public async Task<IActionResult> SolicitarBoletimEscolaAqui([FromBody] FiltroRelatorioEscolaAquiDto filtroRelatorioBoletimDto, [FromServices] IBoletimEscolaAquiUseCase boletimUseCase)
        {
            return Ok(await boletimUseCase.Executar(filtroRelatorioBoletimDto));
        }

        [HttpPost("raa")]
        [ChaveIntegracaoSgpApi]
        public async Task<IActionResult> SolicitarRelatrioRaaEscolaAqui([FromBody] FiltroRelatorioEscolaAquiDto filtro, [FromServices] IRelatorioRaaEscolaAquiUseCase useCase)
        {
            return Ok(await useCase.Executar(filtro));
        }
    }
}
