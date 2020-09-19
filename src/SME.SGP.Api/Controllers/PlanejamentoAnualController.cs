﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SME.SGP.Api.Filtros;
using SME.SGP.Aplicacao;
using SME.SGP.Infra;
using System.Threading.Tasks;

namespace SME.SGP.Api.Controllers
{
    [ApiController]
    [Route("api/v1/planejamento/anual")]
    [ValidaDto]
    //[Authorize("Bearer")]
    public class PlanejamentoAnualController : ControllerBase
    {
        [HttpPost("turmas/{turmaId}/componentes-curriculares/{componenteCurricularId}")]
        [ProducesResponseType(204)]
        [ProducesResponseType(typeof(GradeComponenteTurmaAulasDto), 200)]
        [ProducesResponseType(typeof(RetornoBaseDto), 500)]
        public async Task<IActionResult> Salvar(long turmaId, long componenteCurricularId, [FromBody] SalvarPlanejamentoAnualDto dto, [FromServices] ISalvarPlanejamentoAnualUseCase useCase)
        {
            return Ok(await useCase.Executar(turmaId, componenteCurricularId, dto));
        }

        [HttpPost("turmas/{turmaId}/componentes-curriculares/{componenteCurricularId}")]
        [ProducesResponseType(204)]
        [ProducesResponseType(typeof(GradeComponenteTurmaAulasDto), 200)]
        [ProducesResponseType(typeof(RetornoBaseDto), 500)]
        public async Task<IActionResult> Obter(long turmaId, long componenteCurricularId, [FromServices] ISalvarPlanejamentoAnualUseCase useCase)
        {
            return Ok(await useCase.Executar(turmaId, componenteCurricularId, dto));
        }
    }
}