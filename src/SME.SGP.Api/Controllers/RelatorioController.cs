﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SME.SGP.Api.Filtros;
using SME.SGP.Api.Middlewares;
using SME.SGP.Aplicacao;
using SME.SGP.Aplicacao.Integracoes;
using SME.SGP.Aplicacao.Interfaces;
using SME.SGP.Aplicacao.Interfaces.CasosDeUso;
using SME.SGP.Dominio;
using SME.SGP.Infra;
using SME.SGP.Infra.Dtos.Relatorios;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SME.SGP.Api.Controllers
{
    [ApiController]
    [Route("api/v1/relatorios")]
    [Authorize("Bearer")]
    [ChaveIntegracaoSgpApi]
    public class RelatorioController : ControllerBase
    {
        [HttpGet("{codigoCorrelacao}")]
        [ProducesResponseType(typeof(Boolean), 200)]
        [ProducesResponseType(typeof(RetornoBaseDto), 500)]
        public async Task<IActionResult> Download(Guid codigoCorrelacao, [FromServices] IReceberDadosDownloadRelatorioUseCase downloadRelatorioUseCase, [FromServices] ISevicoJasper servicoJasper)
        {
            var (relatorio, contentType, nomeArquivo) = await downloadRelatorioUseCase.Executar(codigoCorrelacao);

            return File(relatorio, contentType, nomeArquivo);
        }
        
        [HttpPost("conselhos-classe/atas-finais")]
        [ProducesResponseType(typeof(Boolean), 200)]
        [ProducesResponseType(typeof(RetornoBaseDto), 500)]
        public async Task<IActionResult> ConselhoClasseAtaFinal([FromBody]FiltroRelatorioConselhoClasseAtaFinalDto filtroRelatorioConselhoClasseAtaFinalDto, [FromServices] IRelatorioConselhoClasseAtaFinalUseCase relatorioConselhoClasseAtaFinalUseCase)
        {
            return Ok(await relatorioConselhoClasseAtaFinalUseCase.Executar(filtroRelatorioConselhoClasseAtaFinalDto));
        }
     
        [HttpPost("faltas-frequencia")]
        [ProducesResponseType(typeof(Boolean), 200)]
        [ProducesResponseType(typeof(RetornoBaseDto), 500)]
        public async Task<IActionResult> Frequencia([FromBody] FiltroRelatorioFrequenciaDto filtroRelatorioFaltasFrequenciaDto, [FromServices] IGerarRelatorioFrequenciaUseCase gerarRelatorioFrequenciaUseCase)
        {
            return Ok(await gerarRelatorioFrequenciaUseCase.Executar(filtroRelatorioFaltasFrequenciaDto));
        }

        [HttpPost("calendarios/impressao")]
        [ProducesResponseType(typeof(Boolean), 200)]
        [ProducesResponseType(typeof(RetornoBaseDto), 500)]
        public async Task<IActionResult> Calendario([FromBody] FiltroRelatorioCalendarioDto filtroRelatorioCalendarioDto, [FromServices] IRelatorioCalendarioUseCase relatorioCalendarioUseCase)
        {
            return Ok(await relatorioCalendarioUseCase.Executar(filtroRelatorioCalendarioDto));
        }

        [HttpPost("resumopap/impressao")]
        [ProducesResponseType(typeof(Boolean), 200)]
        [ProducesResponseType(typeof(RetornoBaseDto), 500)]
        public async Task<IActionResult> ResumoPAP([FromBody] FiltroRelatorioResumoPAPDto filtroRelatorioResumoPAPDto, [FromServices] IRelatorioResumoPAPUseCase relatorioResumoPAPUseCase)
        {
            return Ok(await relatorioResumoPAPUseCase.Executar(filtroRelatorioResumoPAPDto));
        }

        [HttpPost("graficopap/impressao")]
        [ProducesResponseType(typeof(Boolean), 200)]
        [ProducesResponseType(typeof(RetornoBaseDto), 500)]
        public async Task<IActionResult> GraficoPAP([FromBody] FiltroRelatorioResumoPAPDto filtroRelatorioGraficoPAPDto, [FromServices] IRelatorioGraficoPAPUseCase relatorioGraficoPAPUseCase)
        {
            return Ok(await relatorioGraficoPAPUseCase.Executar(filtroRelatorioGraficoPAPDto));
        }

        [HttpPost("plano-aula")]
        [ProducesResponseType(typeof(Boolean), 200)]
        [ProducesResponseType(typeof(RetornoBaseDto), 500)]
        public async Task<IActionResult> PlanoAula([FromBody] FiltroRelatorioPlanoAulaDto filtro, [FromServices] IRelatorioPlanoAulaUseCase useCase)
        {
            return Ok(await useCase.Executar(filtro));
        }

        [HttpPost("controle-grade/impressao")]
        [ProducesResponseType(typeof(Boolean), 200)]
        [ProducesResponseType(typeof(RetornoBaseDto), 500)]
        public async Task<IActionResult> ControleGrade([FromBody] FiltroRelatorioControleGrade filtro, [FromServices] IRelatorioControleGradeUseCase relatorioUseCase)
        {
            return Ok(await relatorioUseCase.Executar(filtro));
        }

        
        [HttpPost("notificacoes/impressao")]
        [ProducesResponseType(typeof(Boolean), 200)]
        [ProducesResponseType(typeof(RetornoBaseDto), 500)]
        [Permissao(Permissao.RDN_C, Policy = "Bearer")]
        public async Task<IActionResult> Notificacoes([FromBody] FiltroRelatorioNotificacao filtro, [FromServices] IRelatorioNotificacaoUseCase useCase)
        {
            return Ok(await useCase.Executar(filtro));
        }

        [HttpPost("usuarios/impressao")]
        [ProducesResponseType(typeof(Boolean), 200)]
        [ProducesResponseType(typeof(RetornoBaseDto), 500)]
        public async Task<IActionResult> Usuarios([FromBody] FiltroRelatorioUsuarios filtro, [FromServices] IRelatorioUsuariosUseCase relatorioUseCase)
        {
            return Ok(await relatorioUseCase.Executar(filtro));
        }

        [HttpPost("atribuicoes/cjs")]
        [ProducesResponseType(typeof(Boolean), 200)]
        [ProducesResponseType(typeof(RetornoBaseDto), 500)]
        [ProducesResponseType(typeof(RetornoBaseDto), 601)]
        public async Task<IActionResult> Gerar(FiltroRelatorioAtribuicaoCJDto filtros, [FromServices] IRelatorioAtribuicaoCJUseCase relatorioAtribuicaoCJUseCase)
        {
            return Ok(await relatorioAtribuicaoCJUseCase.Executar(filtros));
        }
        
        [HttpPost("historico-alteracao-notas")]
        [ProducesResponseType(typeof(Boolean), 200)]
        [ProducesResponseType(typeof(RetornoBaseDto), 500)]
        public async Task<IActionResult> AlteracaoNotas([FromBody] FiltroRelatorioAlteracaoNotas filtro, [FromServices] IRelatorioAlteracaoNotasUseCase relatorioUseCase)
        {
            if (filtro.ModalidadeTurma == Dominio.Modalidade.EducacaoInfantil)
                throw new NegocioException("Não é possível gerar este relatório para a modalidade infantil.");
            return Ok(await relatorioUseCase.Executar(filtro));
        }

        [HttpPost("ae/adesao")]
        [ProducesResponseType(typeof(bool), 200)]
        [ProducesResponseType(typeof(RetornoBaseDto), 500)]
        public async Task<IActionResult> AdesaoApp([FromBody] FiltroRelatorioAEAdesaoDto filtro, [FromServices] IRelatorioAEAdesaoUseCase relatorioUseCase)
        {
            return Ok(await relatorioUseCase.Executar(filtro));
        }

        [HttpPost("escola-aqui/dados-leitura")]
        [ProducesResponseType(typeof(Boolean), 200)]
        [ProducesResponseType(typeof(RetornoBaseDto), 500)]
        public async Task<IActionResult> LeituraComunicados([FromBody] FiltroRelatorioLeituraComunicados filtro, [FromServices] IRelatorioLeituraComunicadosUseCase relatorioUseCase)
        {            
            return Ok(await relatorioUseCase.Executar(filtro));
        }

        [HttpPost("diario-classe/planejamento-diario")]
        [ProducesResponseType(typeof(Boolean), 200)]
        [ProducesResponseType(typeof(RetornoBaseDto), 500)]
        public async Task<IActionResult> PlanejamentoDiario([FromBody] FiltroRelatorioPlanejamentoDiario filtro, [FromServices] IRelatorioPlanejamentoDiarioUseCase relatorioUseCase)
        {
            return Ok(await relatorioUseCase.Executar(filtro));
        }


        [HttpPost("devolutivas")]
        [ProducesResponseType(typeof(Boolean), 200)]
        [ProducesResponseType(typeof(RetornoBaseDto), 500)]
        public async Task<IActionResult> Devolutivas([FromBody] FiltroRelatorioDevolutivas filtro, [FromServices] IRelatorioDevolutivasUseCase relatorioUseCase)
        {
            return Ok(await relatorioUseCase.Executar(filtro));
        }

        [HttpPost("itinerancias")]
        [ProducesResponseType(typeof(Boolean), 200)]
        [ProducesResponseType(typeof(RetornoBaseDto), 500)]
        public async Task<IActionResult> Itinerancias([FromBody] IEnumerable<long> itinerancias, [FromServices] IRelatorioItineranciasUseCase relatorioUseCase)
        {
            return Ok(await relatorioUseCase.Executar(itinerancias));
        }

        [HttpPost("registros-individuais")]
        [ProducesResponseType(typeof(Boolean), 200)]
        [ProducesResponseType(typeof(RetornoBaseDto), 500)]
        public async Task<IActionResult> RegistroIndividual([FromBody] FiltroRelatorioRegistroIndividualDto filtro, [FromServices] IRelatorioRegistroIndividualUseCase relatorioUseCase)
        {
            return Ok(await relatorioUseCase.Executar(filtro));
        }

        [HttpPost("acompanhamento-aprendizagem")]
        [ProducesResponseType(typeof(Boolean), 200)]
        [ProducesResponseType(typeof(RetornoBaseDto), 500)]
        public async Task<IActionResult> AcompanhamentoAprendizagem([FromBody] FiltroRelatorioAcompanhamentoAprendizagemDto filtro, [FromServices] IRelatorioAcompanhamentoAprendizagemUseCase relatorioUseCase)
        {
            return Ok(await relatorioUseCase.Executar(filtro));
        }

        [HttpPost("acompanhamento-fechamento")]
        [ProducesResponseType(typeof(Boolean), 200)]
        [ProducesResponseType(typeof(RetornoBaseDto), 500)]
        public async Task<IActionResult> AcompanhamentoFechamento([FromBody] FiltroRelatorioAcompanhamentoFechamentoDto filtro, [FromServices] IRelatorioAcompanhamentoFechamentoUseCase relatorioUseCase)
        {
            return Ok(await relatorioUseCase.Executar(filtro));
        }

        [HttpPost("pendencias")]
        [ProducesResponseType(typeof(Boolean), 200)]
        [ProducesResponseType(typeof(RetornoBaseDto), 500)]
        [ProducesResponseType(typeof(RetornoBaseDto), 601)]
        public async Task<IActionResult> Gerar(FiltroRelatorioPendenciasDto filtroRelatorioPendenciasFechamentoDto, [FromServices] IRelatorioPendenciasUseCase relatorioPendenciasFechamentoUseCase)
        {
            return Ok(await relatorioPendenciasFechamentoUseCase.Executar(filtroRelatorioPendenciasFechamentoDto));
        }

        [HttpGet("pendencias/tipos")]
        [ProducesResponseType(typeof(RetornoBaseDto), 500)]
        [ProducesResponseType(typeof(RetornoBaseDto), 601)]
        public IActionResult ObterTipoPendencias([FromQuery] bool opcaoTodos, [FromServices] IRelatorioPendenciasUseCase relatorioPendenciasFechamentoUseCase)
        {
            return Ok(relatorioPendenciasFechamentoUseCase.ListarTodosTipos(opcaoTodos));
        }
        
        [HttpPost("atas-bimestrais")]
        [ProducesResponseType(typeof(Boolean), 200)]
        [ProducesResponseType(typeof(RetornoBaseDto), 500)]
        [ProducesResponseType(typeof(RetornoBaseDto), 601)]
        public async Task<IActionResult> Gerar(FiltroRelatorioAtaBimestralDto filtro, [FromServices] IRelatorioAtaBimestralUseCase useCase)
        {
            return Ok(await useCase.Executar(filtro));
        }

        [HttpPost("acompanhamento-registros-pedagogicos")]
        [ProducesResponseType(typeof(Boolean), 200)]
        [ProducesResponseType(typeof(RetornoBaseDto), 500)]
        public async Task<IActionResult> AcompanhamentoRegistrosPedagogicos(FiltroRelatorioAcompanhamentoRegistrosPedagogicosDto filtro, [FromServices] IRelatorioAcompanhamentoRegistrosPedagogicosUseCase relatorioRegistrosPedagogicos)
        {
            return Ok(await relatorioRegistrosPedagogicos.Executar(filtro));
        }


        [HttpPost("acompanhamento-frequencia")]
        [ProducesResponseType(typeof(Boolean), 200)]
        [ProducesResponseType(typeof(RetornoBaseDto), 500)]
        public async Task<IActionResult> ImprimirAcompanhamentoFrequencia(FiltroAcompanhamentoFrequenciaJustificativaDto filtro,[FromServices] IRelatorioAcompanhamentoDeFrequênciaUseCase useCase)
        {
            return Ok(await useCase.Executar(filtro));
        }


        [HttpPost("ocorrencias")]
        [ProducesResponseType(typeof(Boolean), 200)]
        [ProducesResponseType(typeof(RetornoBaseDto), 500)]
        public async Task<IActionResult> ImprimirRelatorioOcorrencias(FiltroImpressaoOcorrenciaDto filtro, [FromServices] IRelatorioOcorrenciasUseCase useCase)
        {
            return Ok(await useCase.Executar(filtro));
        }


        [HttpGet("existe")]
        [ProducesResponseType(typeof(Boolean), 200)]
        [ProducesResponseType(typeof(RetornoBaseDto), 500)]
        public async Task<IActionResult> VerificarSeRelatorioExiste([FromQuery] Guid codigoRelatorio, [FromServices] IObterDataCriacaoRelatorioUseCase useCase)
        {
            return Ok(await useCase.Executar(codigoRelatorio));
        }
    }
}