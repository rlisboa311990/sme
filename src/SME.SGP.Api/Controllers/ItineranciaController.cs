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
        public async Task<IActionResult> ObterRegistroItinerancia(long id)
        {
            var itinerancia = new ItineranciaDto()
            {
                DataRetornoVerificacao = new DateTime(),
                DataVisita = new DateTime(),
                Alunos = new List<ItineranciaAlunoDto>()
                {
                    new ItineranciaAlunoDto()
                    {
                        CodigoAluno = "123456",
                        Id = 1,
                        Nome = "João Carlos Almeida",
                        Questoes = new List<ItineranciaAlunoQuestaoDto>()
                        {
                            new ItineranciaAlunoQuestaoDto() {
                                Id=1,
                                QuestaoId = 1,
                                Descricao = "Descritivo do estudante",
                                RegistroItineranciaAlunoId = 1,
                                Resposta = "Teste",
                                Obrigatorio =  true,
                            } ,
                            new ItineranciaAlunoQuestaoDto() {
                                Id=2,
                                QuestaoId = 2,
                                Descricao = "Acompanhamento da situação",
                                RegistroItineranciaAlunoId = 1,
                                Resposta = "Teste",
                                Obrigatorio =  false,
                            } ,
                            new ItineranciaAlunoQuestaoDto() {
                                Id = 3,
                                QuestaoId = 3,
                                Descricao = "Encaminhamentos",
                                RegistroItineranciaAlunoId = 1,
                                Resposta = "Teste",
                                Obrigatorio =  false,
                            } ,
                        }
                    },
                    new ItineranciaAlunoDto()
                    {
                        CodigoAluno = "654321",
                        Id = 1,
                        Nome = "Aline Oliveira"
                        ,
                        Questoes = new List<ItineranciaAlunoQuestaoDto>()
                        {
                            new ItineranciaAlunoQuestaoDto() {
                                Id=1,
                                QuestaoId = 1,
                                Descricao = "Descritivo do estudante",
                                RegistroItineranciaAlunoId = 1,
                                Resposta = "Teste",
                                Obrigatorio =  true,
                            } ,
                            new ItineranciaAlunoQuestaoDto() {
                                Id=2,
                                QuestaoId = 2,
                                Descricao = "Acompanhamento da situação",
                                RegistroItineranciaAlunoId = 1,
                                Resposta = "Teste",
                                Obrigatorio =  false,
                            } ,
                            new ItineranciaAlunoQuestaoDto() {
                                Id = 3,
                                QuestaoId = 3,
                                Descricao = "Encaminhamentos",
                                RegistroItineranciaAlunoId = 1,
                                Resposta = "Teste",
                                Obrigatorio =  false,
                            } ,
                        }
                    }
                },
                ObjetivosVisita = new List<ItineranciaObjetivoDto> {
                    new ItineranciaObjetivoDto(1, "Mapeamento dos estudantes público da Educação Especial", false, false, true, "Teste"),
                    new ItineranciaObjetivoDto(2, "Reorganização e/ou remanejamento de apoios e serviços", false, false, true, "teste 1"),
                    new ItineranciaObjetivoDto(3, "Atendimento de solicitação da U.E", true, false, true, "teste 2"),
                },
                Questoes = new List<ItineranciaQuestaoDto>() { 
                    new ItineranciaQuestaoDto() { 
                        Id=1,
                        QuestaoId = 1,
                        Descricao = "Acompanhamento da situação",
                        RegistroItineranciaId = 1,
                        Resposta = "Teste",
                        Obrigatorio = true,
                    } ,
                    new ItineranciaQuestaoDto() {
                        Id = 2,
                        QuestaoId = 2,
                        Descricao = "Encaminhamentos",
                        RegistroItineranciaId = 1,
                        Resposta = "Teste",
                        Obrigatorio = false,
                    } ,
                },
                Ues = new List<ItineranciaUeDto>()
                {
                    new ItineranciaUeDto()
                    {
                        Id = 1,
                        UeId = 1,
                        Descricao = "JT - Máximo de Moura"
                    },
                    new ItineranciaUeDto()
                    {
                        Id = 1,
                        UeId = 1,
                        Descricao = "JT - Jaçanã"
                    }
                }
            };

            return Ok(itinerancia);
        }

        [HttpPost]
        [ProducesResponseType(typeof(RetornoBaseDto), 200)]
        [ProducesResponseType(typeof(RetornoBaseDto), 500)]
        //[Permissao(Permissao.AEE_A, Policy = "Bearer")]
        public async Task<IActionResult> Salvar([FromBody] ItineranciaDto parametros)
        {
            return Ok(new AuditoriaDto() 
            { Id = 1, 
              CriadoPor = "ALINE LIMA CARVALHO",
              CriadoEm = DateTime.Now,
              CriadoRF = "8240787"
            });
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