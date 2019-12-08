﻿using Microsoft.AspNetCore.Mvc;
using SME.SGP.Api.Filtros;
using SME.SGP.Aplicacao;
using SME.SGP.Aplicacao.Interfaces;
using SME.SGP.Infra;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SME.SGP.Api.Controllers
{
    [ApiController]
    [Route("api/v1/aula-prevista")]
    [ValidaDto]
    public class AulaPrevistaController : ControllerBase
    {
        [HttpGet("turma/{turmaId}/disciplina/{disciplinaId}")]
        [ProducesResponseType(typeof(IEnumerable<AulasPrevistasDadasDto>), 200)]
        [ProducesResponseType(typeof(RetornoBaseDto), 500)]
        [ProducesResponseType(typeof(RetornoBaseDto), 601)]
        [Permissao(Permissao.CP_I, Policy = "Bearer")]
        public async Task<IActionResult> ObterAulaPrevistaDada(string turmaId, string disciplinaId, [FromServices]IConsultasAulaPrevista consultas)
        {
            return Ok(await consultas.ObterAulaPrevistaDada(turmaId, disciplinaId));
        }

        [HttpPost]
        [ProducesResponseType(typeof(string), 200)]
        [ProducesResponseType(typeof(RetornoBaseDto), 500)]
        [Permissao(Permissao.CP_I, Policy = "Bearer")]
        public async Task<IActionResult> Inserir([FromBody]IEnumerable<AulaPrevistaDto> dtos, [FromServices]IComandosAulaPrevista comandos)
        {
            await comandos.Inserir(dtos);
            return Ok();
        }
    }
}
