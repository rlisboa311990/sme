﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using SME.SGP.Api.Filtros;
using SME.SGP.Aplicacao;
using SME.SGP.Aplicacao.Interfaces;
using SME.SGP.Infra;
using SME.SGP.Infra.Dtos;

namespace SME.SGP.Api.Controllers
{
    [ApiController]
    [Route("api/v1/fechamentos/turmas")]
    public class FechamentoTurmaController : ControllerBase
    {
        [HttpPost()]
        [ProducesResponseType(typeof(AuditoriaPersistenciaDto), 200)]
        [ProducesResponseType(typeof(RetornoBaseDto), 500)]
        [ProducesResponseType(typeof(RetornoBaseDto), 601)]
        [Permissao(Permissao.CP_I, Policy = "Bearer")]
        public async Task<IActionResult> Salvar([FromBody] IEnumerable<FechamentoTurmaDisciplinaDto> fechamentoTurma, [FromServices] IComandosFechamentoTurmaDisciplina comandos)
            => Ok(await comandos.Salvar(fechamentoTurma));

        [HttpGet()]
        [ProducesResponseType(typeof(FechamentoTurmaDisciplinaBimestreDto), 200)]
        [ProducesResponseType(typeof(RetornoBaseDto), 500)]
        [ProducesResponseType(typeof(RetornoBaseDto), 601)]
        [Permissao(Permissao.FB_C, Policy = "Bearer")]
        public async Task<IActionResult> Listar(string turmaCodigo, long disciplinaCodigo, int? bimestre, int semestre, [FromServices] IConsultasFechamentoTurmaDisciplina consultas)
            => Ok(await consultas.ObterNotasFechamentoTurmaDisciplina(turmaCodigo, disciplinaCodigo, bimestre, semestre));

        [HttpPost("reprocessar/{fechamentoId}")]
        [ProducesResponseType(200)]
        [ProducesResponseType(typeof(RetornoBaseDto), 500)]
        [ProducesResponseType(typeof(RetornoBaseDto), 601)]
        [Permissao(Permissao.FB_A, Policy = "Bearer")]
        public async Task<IActionResult> Reprocessar(long fechamentoId, [FromServices] IComandosFechamentoTurmaDisciplina comandos)
        {
            await comandos.Reprocessar(fechamentoId);
            return Ok();
        }

        [HttpPost("{turmaId}/bimestres/{bimestre}")]
        [ProducesResponseType(typeof(FechamentoTurmaPeriodoEscolarDto), 200)]
        [ProducesResponseType(204)]
        [ProducesResponseType(typeof(RetornoBaseDto), 500)]
        [ProducesResponseType(typeof(RetornoBaseDto), 601)]
        [Permissao(Permissao.FB_A, Policy = "Bearer")]
        public async Task<IActionResult> ObterFechamentoId(long turmaId, int bimestre, [FromServices] IObterFechamentoIdPorTurmaBimestreUseCase useCase)
        {
            var fechamento = await useCase.Executar(new Infra.Dtos.TurmaBimestreDto(turmaId, bimestre));
            if (fechamento is null)
                return NoContent();

            return Ok(fechamento);
        }

        [HttpPost("reprocessar")]
        [ProducesResponseType(200)]
        [ProducesResponseType(typeof(RetornoBaseDto), 500)]
        [ProducesResponseType(typeof(RetornoBaseDto), 601)]
        [Permissao(Permissao.FB_A, Policy = "Bearer")]
        public async Task<IActionResult> Reprocessar(IEnumerable<long> fechamentoId, [FromServices] IComandosFechamentoTurmaDisciplina comandos)
        {
            comandos.Reprocessar(fechamentoId);
            return Ok();
        }

        [HttpPost("processar")]
        [ProducesResponseType(200)]
        [ProducesResponseType(typeof(RetornoBaseDto), 500)]
        [ProducesResponseType(typeof(RetornoBaseDto), 601)]
        [Permissao(Permissao.CP_I, Policy = "Bearer")]
        public async Task<IActionResult> Processar([FromBody] IEnumerable<FechamentoTurmaDisciplinaDto> fechamentoTurma, [FromServices] IComandosFechamentoTurmaDisciplina comandos)
            => Ok(await comandos.Salvar(fechamentoTurma, true));

        [HttpPost("anotacoes/alunos/")]
        [ProducesResponseType(typeof(AuditoriaPersistenciaDto), 200)]
        [ProducesResponseType(typeof(RetornoBaseDto), 500)]
        [ProducesResponseType(typeof(RetornoBaseDto), 601)]
        [Permissao(Permissao.CP_I, Policy = "Bearer")]
        public async Task<IActionResult> SalvarAnotacao([FromBody] AnotacaoAlunoDto anotacaoAluno, [FromServices] ISalvarAnotacaoFechamentoAlunoUseCase useCase)
            => Ok(await useCase.Executar(anotacaoAluno));

        [HttpGet("anotacoes/alunos/{codigoAluno}/fechamentos/{fechamentoId}/turmas/{codigoTurma}/anos/{anoLetivo}")]
        [ProducesResponseType(typeof(FechamentoAlunoCompletoDto), 200)]
        [ProducesResponseType(typeof(RetornoBaseDto), 500)]
        [ProducesResponseType(typeof(RetornoBaseDto), 601)]
        [Permissao(Permissao.FB_C, Policy = "Bearer")]
        public async Task<IActionResult> ObterAnotacaoAluno(string codigoAluno, long fechamentoId, string codigoTurma, int anoLetivo, [FromServices] IConsultasFechamentoAluno consultas)
           => Ok(await consultas.ObterAnotacaoAluno(codigoAluno, fechamentoId, codigoTurma, anoLetivo));

        [HttpGet("{codigoTurma}/alunos/anos/{anoLetivo}/semestres/{semestre}")]
        [ProducesResponseType(typeof(IEnumerable<AlunoDadosBasicosDto>), 200)]
        [ProducesResponseType(typeof(RetornoBaseDto), 500)]
        [ProducesResponseType(typeof(RetornoBaseDto), 601)]
        public async Task<IActionResult> ObterAlunos(string codigoTurma, int anoLetivo, int semestre, [FromServices] IConsultasFechamentoTurmaDisciplina consultas)
           => Ok(await consultas.ObterDadosAlunos(codigoTurma, anoLetivo, semestre));

        [HttpPost("processar-pendentes/{anoLetivo}")]
        [ProducesResponseType(200)]
        [ProducesResponseType(typeof(RetornoBaseDto), 500)]
        [ProducesResponseType(typeof(RetornoBaseDto), 601)]        
        public async Task<IActionResult> ProcessarPendentes(int anoLetivo, [FromServices] IComandosFechamentoTurmaDisciplina comandos)
        {
            await comandos.ProcessarPendentes(anoLetivo);
            return Ok();
        }

        [HttpPost("consolidar")]
        [ProducesResponseType(200)]
        public async Task<IActionResult> ProcessarPendentes([FromQuery] string turmaCodigo, [FromQuery] int? bimestre, [FromServices] IIniciaConsolidacaoTurmaGeralUseCase useCase)
        {
            await useCase.Executar(turmaCodigo, bimestre);
            return Ok();
        }

        [HttpGet("listar")]
        [ProducesResponseType(typeof(FechamentoNotaConceitoTurmaDto), 200)]
        [ProducesResponseType(typeof(RetornoBaseDto), 500)]
        [ProducesResponseType(typeof(RetornoBaseDto), 601)]
        [Permissao(Permissao.FB_C, Policy = "Bearer")]
        public async Task<IActionResult> ListarFechamentoTurma(string turmaCodigo, long componenteCurricularCodigo, int bimestre, int? semestre, [FromServices] IListarFechamentoTurmaBimestreUseCase useCase)
        {
            return Ok(await useCase.Executar(turmaCodigo, componenteCurricularCodigo, bimestre, semestre));
        }

        [HttpPost("salvar-fechamento")]
        [ProducesResponseType(typeof(AuditoriaPersistenciaDto), 200)]
        [ProducesResponseType(typeof(RetornoBaseDto), 500)]
        [ProducesResponseType(typeof(RetornoBaseDto), 601)]
        [Permissao(Permissao.CP_I, Policy = "Bearer")]
        public async Task<IActionResult> SalvarFechamento([FromBody] FechamentoFinalTurmaDisciplinaDto fechamentoTurma, [FromServices] IInserirFechamentoTurmaDisciplinaUseCase useCase)
        {
            return Ok(await useCase.Executar(fechamentoTurma));
        }
    }
}