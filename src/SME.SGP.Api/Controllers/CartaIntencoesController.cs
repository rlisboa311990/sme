﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SME.SGP.Api.Filtros;
using SME.SGP.Aplicacao.Interfaces;
using SME.SGP.Infra;
using SME.SGP.Infra.Dtos;
using System.Collections.Generic;
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

        [HttpGet("turmas/{turmaCodigo}/componente-curricular/{componenteCurricularId}")]
        [ProducesResponseType(typeof(IEnumerable<CartaIntencoesRetornoDto>), 200)]
        [ProducesResponseType(typeof(RetornoBaseDto), 500)]
        [Permissao(Permissao.CI_C, Policy = "Bearer")]
        public async Task<IActionResult> Obter([FromServices] IObterCartasDeIntencoesPorTurmaEComponenteUseCase useCase, string turmaCodigo, long componenteCurricularId)
        {
            return Ok(await useCase.Executar(new ObterCartaIntencoesDto(turmaCodigo, componenteCurricularId)));
        }

        [HttpGet("turmas/{turmaId}/componente-curricular/{componenteCurricularId}/observacoes")]
        [ProducesResponseType(typeof(IEnumerable<CartaIntencoesObservacaoDto>), 200)]
        [ProducesResponseType(typeof(RetornoBaseDto), 500)]
        [Permissao(Permissao.CI_C, Policy = "Bearer")]
        public async Task<IActionResult> ListarObservacoes([FromServices] IListarCartaIntencoesObservacoesPorTurmaEComponenteUseCase useCase, long turmaId, long componenteCurricularId)
        {
            return Ok(await useCase.Executar(new BuscaCartaIntencoesObservacaoDto(turmaId, componenteCurricularId)));
        }


        [HttpPost("turmas/{turmaId}/componente-curricular/{componenteCurricularId}/observacoes")]
        [ProducesResponseType(typeof(AuditoriaDto), 200)]
        [ProducesResponseType(typeof(RetornoBaseDto), 500)]
        [Permissao(Permissao.CI_C, Policy = "Bearer")]
        public async Task<IActionResult> SalvarObservacao([FromBody] SalvarCartaIntencoesObservacaoDto dto, [FromServices] ISalvarCartaIntencoesObservacaoUseCase useCase, long turmaId, long componenteCurricularId)
        {
            return Ok(await useCase.Executar(turmaId, componenteCurricularId, dto));
        }

        [HttpPut("turmas/{turmaId}/componente-curricular/{componenteCurricularId}/observacoes/{observacaoId}")]
        [ProducesResponseType(typeof(AuditoriaDto), 200)]
        [ProducesResponseType(typeof(RetornoBaseDto), 500)]
        [Permissao(Permissao.CI_C, Policy = "Bearer")]
        public async Task<IActionResult> AlterarObservacao([FromBody] AlterarCartaIntencoesObservacaoDto dto, [FromServices] IAlterarCartaIntencoesObservacaoUseCase useCase, long turmaId, long componenteCurricularId, long observacaoId)
        {
            return Ok(await useCase.Executar(observacaoId, dto));
        }
    }
}
