﻿using Microsoft.AspNetCore.Mvc;
using SME.SGP.Aplicacao;
using SME.SGP.Aplicacao.Interfaces;
using SME.SGP.Infra;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SME.SGP.Api
{
    [ApiController]
    [Route("api/v1/itinerancias")]
    //[Authorize("Bearer")]
    public class ItineranciaController : ControllerBase
    {

        [HttpGet("objetivos")]
        [ProducesResponseType(typeof(RegistroIndividualDto), 200)]
        [ProducesResponseType(typeof(RetornoBaseDto), 500)]
        //[Permissao(Permissao.REI_C, Policy = "Bearer")]
        public async Task<IActionResult> ObterObjetivos([FromServices] IObterObjetivosBaseUseCase useCase)
        {
            return Ok(await useCase.Executar());            
        }

        [HttpGet("{id}")]
        [ProducesResponseType(typeof(RegistroIndividualDto), 200)]
        [ProducesResponseType(typeof(RetornoBaseDto), 500)]
        //[Permissao(Permissao.REI_C, Policy = "Bearer")]
        public async Task<IActionResult> ObterRegistroItinerancia(long id, [FromServices] IObterItineranciaPorIdUseCase useCase)
        {
            return Ok(await useCase.Executar(id));            
        }

        [HttpPost]
        [ProducesResponseType(typeof(RetornoBaseDto), 200)]
        [ProducesResponseType(typeof(RetornoBaseDto), 500)]
        //[Permissao(Permissao.REI_I, Policy = "Bearer")]
        public async Task<IActionResult> Salvar([FromBody] ItineranciaDto itineranciaDto, [FromServices] ISalvarItineranciaUseCase useCase)
        {
            return Ok(await useCase.Executar(itineranciaDto));
        }

        [HttpDelete("{id}")]
        [ProducesResponseType(typeof(RetornoBaseDto), 200)]
        [ProducesResponseType(typeof(RetornoBaseDto), 500)]
        //[Permissao(Permissao.REI_A, Policy = "Bearer")]
        public async Task<IActionResult> Excluir(long id, [FromServices] IExcluirItineranciaUseCase useCase)
        {
            return Ok(await useCase.Executar(id));
        }

        [HttpGet("alunos/questoes/{id}")]
        [ProducesResponseType(typeof(RegistroIndividualDto), 200)]
        [ProducesResponseType(typeof(RetornoBaseDto), 500)]
        //[Permissao(Permissao.REI_C, Policy = "Bearer")]
        public async Task<IActionResult> ObterQuestoesItineranciaAluno(long id, [FromServices] IObterQuestoesItineranciaAlunoUseCase useCase)
        {
            return Ok(await useCase.Executar(id));            
        }

        [HttpGet("questoes")]
        [ProducesResponseType(typeof(RegistroIndividualDto), 200)]
        [ProducesResponseType(typeof(RetornoBaseDto), 500)]
        //[Permissao(Permissao.REI_C, Policy = "Bearer")]
        public async Task<IActionResult> ObterQuestoes([FromServices] IObterQuestoesBaseUseCase useCase)
        {
            return Ok(await useCase.Executar());
        }
    }
}