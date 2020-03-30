﻿using Microsoft.AspNetCore.Mvc;
using SME.SGP.Api.Filtros;
using SME.SGP.Aplicacao;
using SME.SGP.Infra;
using System;

namespace SME.SGP.Api.Controllers
{
    [ApiController]
    [Route("api/v1/periodo-escolar")]
    [ValidaDto]
    public class PeriodoEscolarController : ControllerBase
    {
        [HttpGet]
        [ProducesResponseType(204)]
        [ProducesResponseType(typeof(PeriodoEscolarDto), 200)]
        [ProducesResponseType(typeof(RetornoBaseDto), 500)]
        [Permissao(Permissao.PE_C, Policy = "Bearer")]
        public IActionResult Get(long codigoTipoCalendario, [FromServices]IConsultasPeriodoEscolar consultas)
        {
            var periodoEscolar = consultas.ObterPorTipoCalendario(codigoTipoCalendario);

            if (periodoEscolar == null)
                return NoContent();

            return Ok(periodoEscolar);
        }

        [HttpPost]
        [ProducesResponseType(200)]
        [ProducesResponseType(typeof(RetornoBaseDto), 500)]
        [Permissao(Permissao.PE_I, Permissao.PE_A, Policy = "Bearer")]
        public IActionResult Post([FromBody]PeriodoEscolarListaDto periodos, [FromServices]IComandosPeriodoEscolar comandoPeriodo)
        {
            comandoPeriodo.Salvar(periodos);
            return Ok();
        }

        [HttpGet("modalidades/{modalidade}/bimestres/atual")]
        [ProducesResponseType(204)]
        [ProducesResponseType(typeof(int), 200)]
        [ProducesResponseType(typeof(RetornoBaseDto), 500)]
        public IActionResult ObterAtual(int modalidade, [FromServices]IConsultasPeriodoEscolar consultas)
        {
            return Ok(consultas.ObterBimestre(DateTime.Today, (Dominio.Modalidade)modalidade));
        }
    }
}