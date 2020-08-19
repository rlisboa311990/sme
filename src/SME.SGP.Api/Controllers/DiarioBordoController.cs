﻿using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using SME.SGP.Api.Filtros;
using SME.SGP.Aplicacao;
using SME.SGP.Aplicacao.Interfaces;
using SME.SGP.Infra;
using SME.SGP.Infra.Dtos;
using System;
using System.Threading.Tasks;

namespace SME.SGP.Api.Controllers
{
    [ApiController]
    [Route("api/v1/diarios-bordo")]
    [Authorize("Bearer")]
    public class DiarioBordoController : ControllerBase
    {

        [HttpGet("{aulaId}")]
        [ProducesResponseType(typeof(DiarioBordoDto), 200)]
        [ProducesResponseType(typeof(RetornoBaseDto), 500)]
        [Permissao(Permissao.DDB_C, Policy = "Bearer")]
        public async Task<IActionResult> Obter([FromServices] IObterDiarioBordoUseCase useCase, long aulaId)
        {
            var result = await useCase.Executar(aulaId);
            if (result == null)
                return NoContent();

            return Ok(result);
        }

        [HttpPost]
        [ProducesResponseType(typeof(AuditoriaDto), 200)]
        [ProducesResponseType(typeof(RetornoBaseDto), 500)]
        [Permissao(Permissao.DDB_I, Policy = "Bearer")]
        public async Task<IActionResult> Salvar([FromServices] IInserirDiarioBordoUseCase useCase, [FromBody] InserirDiarioBordoDto diarioBordoDto)
        {
            return Ok(await useCase.Executar(diarioBordoDto));
        }

        [HttpPut]
        [ProducesResponseType(typeof(AuditoriaDto), 200)]
        [ProducesResponseType(typeof(RetornoBaseDto), 500)]
        [Permissao(Permissao.DDB_A, Policy = "Bearer")]
        public async Task<IActionResult> Alterar([FromServices] IAlterarDiarioBordoUseCase useCase, [FromBody] AlterarDiarioBordoDto diarioBordoDto)
        {
            return Ok(await useCase.Executar(diarioBordoDto));
        }

        [HttpGet("devolutivas/{id}")]
        [ProducesResponseType(typeof(DiarioBordoDto), 200)]
        [ProducesResponseType(typeof(RetornoBaseDto), 500)]
        //[Permissao(Permissao.DDB_C, Policy = "Bearer")]
        public async Task<IActionResult> ObterPorDevolutiva([FromServices] IObterDiarioBordoUseCase useCase, long devolutivaId, int numeroPagina, int numeroRegistros)
        {
            var text = "";
            if (numeroPagina == 1)
            {
            text = @"
                        {
                        totalPaginas: 5,
                        totalRegistros: 20,
                        itens : [
                          {
                            cj: false,
                            data: ""2020-08-05T00:00:00.000000"",
                            planejamento: ""planejamento do diario de bordo PAGINA 11111"",
                          },
                          {
                            cj: false,
                            data: ""2020-08-06T00:00:00.000000"",
                            planejamento: ""planejamento do diario de bordo PAGINA 11111"",
                          },
                          {
                            cj: true,
                            data: ""2020-08-07T00:00:00.000000"",
                            planejamento: ""planejamento do diario de bordo planejamento do diario de bordoplanejamento do diario de bordo planejamento do diario de bordo planejamento do diario de bordo planejamento do diario de bordo planejamento do diario de bordo planejamento do diario de bordoplanejamento do diario de bordo planejamento do diario de bordo planejamento do diario de bordoplanejamento do diario de bordoplanejamento do diario de bordoplanejamento do diario de bordoplanejamento do diario de bordoplanejamento do diario de bordoplanejamento do diario de bordoplanejamento do diario de bordoplanejamento do diario de bordoplanejamento do diario de bordoplanejamento do diario de bordoplanejamento do diario de bordoplanejamento do diario de bordoplanejamento do diario de bordoplanejamento do diario de bordoplanejamento do diario de bordoplanejamento do diario de bordoplanejamento do diario de bordoplanejamento do diario de bordoplanejamento do diario de bordoplanejamento do diario de bordoplanejamento do diario de bordoplanejamento do diario de bordoplanejamento do diario de bordoplanejamento do diario de bordoplanejamento do diario de bordoplanejamento do diario de bordoplanejamento do diario de bordoplanejamento do diario de bordoplanejamento do diario de bordoplanejamento do diario de bordoplanejamento do diario de bordoplanejamento do diario de bordoplanejamento do diario de bordoplanejamento do diario de bordoplanejamento do diario de bordoplanejamento do diario de bordoplanejamento do diario de bordoplanejamento do diario de bordoplanejamento do diario de bordoplanejamento do diario de bordoplanejamento do diario de bordoplanejamento do diario de bordoplanejamento do diario de bordoplanejamento do diario de bordoplanejamento do diario de bordoplanejamento do diario de bordo"",
                          },
                        ]}";
            }

            if ( numeroPagina == 2)
            {
                text = @"
                        {
                        totalPaginas: 5,
                        totalRegistros: 20,
                        itens : [
                          {
                            cj: false,
                            data: ""2020-03-05T00:00:00.000000"",
                            planejamento: ""planejamento do diario de bordo PAGINA 222222"",
                          },
                          {
                            cj: false,
                            data: ""2020-03-06T00:00:00.000000"",
                            planejamento: ""planejamento do diario de bordo PAGINA 222222"",
                          },
                          {
                            cj: true,
                            data: ""2020-03-07T00:00:00.000000"",
                            planejamento: ""planejamento do diario de bordo planejamento do diario de bordoplanejamento do diario de bordo planejamento do diario de bordo planejamento do diario de bordo planejamento do diario de bordo planejamento do diario de bordo planejamento do diario de bordoplanejamento do diario de bordo planejamento do diario de bordo planejamento do diario de bordoplanejamento do diario de bordoplanejamento do diario de bordoplanejamento do diario de bordoplanejamento do diario de bordoplanejamento do diario de bordoplanejamento do diario de bordoplanejamento do diario de bordoplanejamento do diario de bordoplanejamento do diario de bordoplanejamento do diario de bordoplanejamento do diario de bordoplanejamento do diario de bordoplanejamento do diario de bordoplanejamento do diario de bordoplanejamento do diario de bordoplanejamento do diario de bordoplanejamento do diario de bordoplanejamento do diario de bordoplanejamento do diario de bordoplanejamento do diario de bordoplanejamento do diario de bordoplanejamento do diario de bordoplanejamento do diario de bordoplanejamento do diario de bordoplanejamento do diario de bordoplanejamento do diario de bordoplanejamento do diario de bordoplanejamento do diario de bordoplanejamento do diario de bordoplanejamento do diario de bordoplanejamento do diario de bordoplanejamento do diario de bordoplanejamento do diario de bordoplanejamento do diario de bordoplanejamento do diario de bordoplanejamento do diario de bordoplanejamento do diario de bordoplanejamento do diario de bordoplanejamento do diario de bordoplanejamento do diario de bordoplanejamento do diario de bordoplanejamento do diario de bordoplanejamento do diario de bordoplanejamento do diario de bordoplanejamento do diario de bordoplanejamento do diario de bordo"",
                          },
                        ]}";
            }

            var json = JObject.Parse(text);

            return Ok(json);
        }

        [HttpGet("turmas/{turmaCodigo}/componentes-curriculares/{componenteCurricularId}/inicio/{dataInicio}/fim/{dataFim}")]
        [ProducesResponseType(typeof(PaginacaoResultadoDto<DiarioBordoDevolutivaDto>), 200)]
        [ProducesResponseType(204)]
        [ProducesResponseType(typeof(RetornoBaseDto), 500)]
        //[Permissao(Permissao.DDB_C, Policy = "Bearer")]
        public async Task<IActionResult> ObterPorIntervalo([FromServices] IObterDiariosDeBordoPorPeriodoUseCase useCase, string turmaCodigo, long componenteCurricularId, DateTime dataInicio, DateTime dataFim)
        {
            return Ok(await useCase.Executar(new FiltroTurmaComponentePeriodoDto(turmaCodigo, componenteCurricularId, dataInicio, dataFim)));
        }
    }
}
