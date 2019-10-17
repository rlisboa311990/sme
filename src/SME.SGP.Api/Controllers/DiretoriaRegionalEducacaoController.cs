﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SME.SGP.Aplicacao;
using SME.SGP.Infra;
using System.Collections.Generic;

namespace SME.SGP.Api.Controllers
{
    [ApiController]
    [Route("api/v1/dres")]
    [Authorize("Bearer")]
    public class DiretoriaRegionalEducacaoController : ControllerBase
    {
        private readonly IConsultaDres consultaDres;

        public DiretoriaRegionalEducacaoController(IConsultaDres consultaDres)
        {
            this.consultaDres = consultaDres ?? throw new System.ArgumentNullException(nameof(consultaDres));
        }

        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<CicloDto>), 200)]
        [ProducesResponseType(typeof(RetornoBaseDto), 500)]
        public IActionResult Get()
        {
            return Ok(consultaDres.ObterTodos());
        }

        [HttpGet("{dreId}/ues/sem-atribuicao")]
        [ProducesResponseType(typeof(IEnumerable<UnidadeEscolarDto>), 200)]
        [ProducesResponseType(typeof(RetornoBaseDto), 500)]
        public IActionResult ObterEscolasSemAtribuicao(string dreId)
        {
            return Ok(consultaDres.ObterEscolasSemAtribuicao(dreId));
        }

        [HttpGet("{dreId}/ues")]
        [ProducesResponseType(typeof(IEnumerable<UnidadeEscolarDto>), 200)]
        [ProducesResponseType(typeof(RetornoBaseDto), 500)]
        public IActionResult ObterUesPorDre(string dreId)
        {
            return Ok(consultaDres.ObterEscolasPorDre(dreId));
        }
    }
}