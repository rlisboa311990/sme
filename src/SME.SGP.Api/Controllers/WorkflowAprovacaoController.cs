﻿using Microsoft.AspNetCore.Mvc;
using SME.SGP.Api.Filtros;
using SME.SGP.Aplicacao;
using SME.SGP.Dto;
using System.Collections.Generic;

namespace SME.SGP.Api.Controllers
{
    [ApiController]
    [Route("api/v1/workflows/aprovacoes")]
    [ValidaDto]
    public class WorkflowAprovacaoController : ControllerBase
    {
        private readonly IComandosWorkflowAprovacao comandosWorkflowAprovacao;
        private readonly IConsultasWorkflowAprovacao consultasWorkflowAprovacao;

        public WorkflowAprovacaoController(IComandosWorkflowAprovacao comandosWorkflowAprovacao, IConsultasWorkflowAprovacao consultasWorkflowAprovacao)
        {
            this.comandosWorkflowAprovacao = comandosWorkflowAprovacao ?? throw new System.ArgumentNullException(nameof(comandosWorkflowAprovacao));
            this.consultasWorkflowAprovacao = consultasWorkflowAprovacao ?? throw new System.ArgumentNullException(nameof(consultasWorkflowAprovacao));
        }

        //[HttpGet]
        //[Route("{id}")]
        //[ProducesResponseType(200)]
        //[ProducesResponseType(typeof(RetornoBaseDto), 500)]
        //public IActionResult Get(long id)
        //{
        //    return Ok(consultasWorkflowAprovacao.ObtemPorId(id));
        //}

        [HttpGet]
        [Route("notificacoes/{id}/linha-tempo")]
        [ProducesResponseType(typeof(IEnumerable<WorkflowAprovacaoTimeRespostaDto>), 200)]
        [ProducesResponseType(typeof(RetornoBaseDto), 500)]
        public IActionResult ObterLinhaDoTempo(long id)
        {
            return Ok(consultasWorkflowAprovacao.ObtemTimelinePorCodigoNotificacao(id));
        }

        [HttpPost]
        [ProducesResponseType(200)]
        [ProducesResponseType(typeof(RetornoBaseDto), 500)]
        public IActionResult Post(WorkflowAprovacaoDto workflowAprovaNivelDto)
        {
            comandosWorkflowAprovacao.Salvar(workflowAprovaNivelDto);
            return Ok();
        }
    }
}