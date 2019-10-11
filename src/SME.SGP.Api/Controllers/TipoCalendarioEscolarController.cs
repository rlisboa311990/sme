﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SME.SGP.Api.Filtros;
using SME.SGP.Aplicacao;
using SME.SGP.Dominio;
using SME.SGP.Dto;
using System.Collections.Generic;

namespace SME.SGP.Api.Controllers
{
    [ApiController]
    [Route("api/v1/tipo-calendario-escolar")]
    [ValidaDto]
    [Authorize("Bearer")]
    public class TipoCalendarioEscolarController : ControllerBase
    {
        private readonly IConsultasTipoCalendarioEscolar consultas;
        private readonly IComandosTipoCalendarioEscolar comandos;
        public TipoCalendarioEscolarController(IConsultasTipoCalendarioEscolar consultas,
            IComandosTipoCalendarioEscolar comandos)
        {
            this.consultas = consultas ?? throw new System.ArgumentNullException(nameof(consultas));
            this.comandos = comandos ?? throw new System.ArgumentNullException(nameof(comandos));
        }

        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<TipoCalendarioEscolarDto>), 200)]
        [ProducesResponseType(typeof(RetornoBaseDto), 500)]
        public IActionResult BuscarTodos()
        {
            return Ok(consultas.Listar());
        }

        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<TipoCalendarioEscolarDto>), 200)]
        [ProducesResponseType(typeof(RetornoBaseDto), 500)]
        [Route("{id}")]
        public IActionResult BuscarUm(long id)
        {
            return Ok(consultas.BuscarPorId(id));
        }

        [HttpPost]
        [ProducesResponseType(200)]
        [ProducesResponseType(typeof(RetornoBaseDto), 500)]
        public IActionResult Salvar([FromBody]TipoCalendarioEscolarDto dto)
        {
            comandos.Salvar(dto);
            return Ok();
        }

        [HttpDelete]
        [ProducesResponseType(200)]
        [ProducesResponseType(typeof(RetornoBaseDto), 500)]
        public IActionResult MarcarExcluidos([FromBody]long[] ids)
        {
            comandos.MarcarExcluidos(ids);
            return Ok();
        }
    }

}
