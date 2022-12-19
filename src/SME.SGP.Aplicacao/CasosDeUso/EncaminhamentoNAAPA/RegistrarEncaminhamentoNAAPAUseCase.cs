﻿using MediatR;
using SME.SGP.Aplicacao.Interfaces.CasosDeUso;
using SME.SGP.Aplicacao.Queries;
using SME.SGP.Dominio;
using SME.SGP.Dominio.Constantes.MensagensNegocio;
using SME.SGP.Dominio.Enumerados;
using SME.SGP.Infra;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SME.SGP.Aplicacao
{
    public class RegistrarEncaminhamentoNAAPAUseCase : IRegistrarEncaminhamentoNAAPAUseCase
    {
        private const string SECAO_QUESTOES_APRESENTADAS = "QUESTOES_APRESENTADAS_INFANTIL";
        private const string QUESTAO_OBSERVACOES_AGRUPAMENTO_PROMOCAO_CUIDADOS = "OBS_AGRUPAMENTO_PROMOCAO_CUIDADOS";
        private const string QUESTAO_AGRUPAMENTO_PROMOCAO_CUIDADOS = "AGRUPAMENTO_PROMOCAO_CUIDADOS";
        private const string QUESTAO_TIPO_ADOECE_COM_FREQUENCIA_SEM_CUIDADOS_MEDICOS = "TIPO_ADOECE_COM_FREQUENCIA_SEM_CUIDADOS_MEDICOS";
        private const string QUESTAO_TIPO_DOENCA_CRONICA_TRATAMENTO_LONGA_DURACAO = "TIPO_DOENCA_CRONICA_TRATAMENTO_LONGA_DURACAO";

        private readonly IMediator mediator;

        public RegistrarEncaminhamentoNAAPAUseCase(IMediator mediator)
        {
            this.mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        }

        public async Task<ResultadoEncaminhamentoNAAPADto> Executar(EncaminhamentoNAAPADto encaminhamentoNAAPADto)
        {
            var turma = await mediator.Send(new ObterTurmaPorIdQuery(encaminhamentoNAAPADto.TurmaId));
            if (turma == null)
                throw new NegocioException(MensagemNegocioTurma.TURMA_NAO_ENCONTRADA);

            var aluno = await mediator.Send(new ObterAlunoPorCodigoEolQuery(encaminhamentoNAAPADto.AlunoCodigo, DateTime.Now.Year));
            if (aluno == null)
                throw new NegocioException(MensagemNegocioAluno.ESTUDANTE_NAO_ENCONTRADO);

            ICollection<dynamic> questoesObrigatoriasAConsistir = new List<dynamic>();
            await ObterQuestoesObrigatoriasNaoPreechidas(encaminhamentoNAAPADto, questoesObrigatoriasAConsistir);
            
            if (questoesObrigatoriasAConsistir.Any() && encaminhamentoNAAPADto.Situacao != SituacaoNAAPA.Rascunho)
            {
                var mensagem = questoesObrigatoriasAConsistir.GroupBy(questao => questao.Secao).Select(secao =>
                        $"Seção: {secao.Key} Questões: [{string.Join(", ", secao.Select(questao => questao.Ordem).Distinct().ToArray())}]")
                    .ToList();

                throw new NegocioException(string.Format(
                    MensagemNegocioEncaminhamentoNAAPA.EXISTEM_QUESTOES_OBRIGATORIAS_NAO_PREENCHIDAS,
                    string.Join(", ", mensagem)));
            }

            var secoesComQuestoesObrigatoriasAConsistir = questoesObrigatoriasAConsistir
                .Select(questao => questao.SecaoId).Distinct().ToArray();

            foreach (var secao in encaminhamentoNAAPADto.Secoes)
                secao.Concluido = !secoesComQuestoesObrigatoriasAConsistir.Contains(secao.SecaoId); 

            if (encaminhamentoNAAPADto.Id.GetValueOrDefault() > 0)
            {
                var encaminhamentoNAAPA = await mediator.Send(new ObterEncaminhamentoNAAPAPorIdQuery(encaminhamentoNAAPADto.Id.GetValueOrDefault()));

                if (encaminhamentoNAAPA != null)
                {
                    await AlterarEncaminhamento(encaminhamentoNAAPADto, encaminhamentoNAAPA);
                    await RemoverArquivosNaoUtilizados(encaminhamentoNAAPADto.Secoes);

                    return new ResultadoEncaminhamentoNAAPADto
                        { Id = encaminhamentoNAAPA.Id, Auditoria = (AuditoriaDto)encaminhamentoNAAPA };
                }
            }

            var resultadoEncaminhamento = await mediator.Send(new RegistrarEncaminhamentoNAAPACommand(
                encaminhamentoNAAPADto.TurmaId, aluno.NomeAluno, aluno.CodigoAluno,
                encaminhamentoNAAPADto.Situacao));

            await SalvarEncaminhamento(encaminhamentoNAAPADto, resultadoEncaminhamento);

            return resultadoEncaminhamento;
        }
      
        private async Task RemoverArquivosNaoUtilizados(List<EncaminhamentoNAAPASecaoDto> secoes)
        {
            var resposta = new List<EncaminhamentoNAAPASecaoQuestaoDto>();

            foreach (var s in secoes)
            {
                foreach (var q in s.Questoes)
                {
                    if (string.IsNullOrEmpty(q.Resposta) && q.TipoQuestao == TipoQuestao.Upload)
                        resposta.Add(q);
                }
            }

            if (resposta.Any())
            {
                foreach (var item in resposta)
                {
                    var entidadeResposta = await mediator.Send(new ObterRespostaEncaminhamentoNAAPAPorIdQuery(item.RespostaEncaminhamentoId));
                    if (entidadeResposta != null)
                        await mediator.Send(new ExcluirRespostaEncaminhamentoNAAPACommand(entidadeResposta));
                }
            }
        }
        public async Task AlterarEncaminhamento(EncaminhamentoNAAPADto encaminhamentoNAAPADto, EncaminhamentoNAAPA encaminhamentoNAAPA)
        {
            encaminhamentoNAAPA.Situacao = encaminhamentoNAAPADto.Situacao;
            await mediator.Send(new SalvarEncaminhamentoNAAPACommand(encaminhamentoNAAPA));

            foreach (var secao in encaminhamentoNAAPADto.Secoes)
            {
                if (!secao.Questoes.Any())
                    throw new NegocioException(string.Format(MensagemNegocioComuns.NENHUMA_QUESTAO_FOI_ENCONTRADA_NA_SECAO_X,secao.SecaoId));

                var secaoExistente = encaminhamentoNAAPA.Secoes.FirstOrDefault(s => s.SecaoEncaminhamentoNAAPAId == secao.SecaoId);

                if (secaoExistente == null)
                    secaoExistente = await mediator.Send(new RegistrarEncaminhamentoNAAPASecaoCommand(encaminhamentoNAAPA.Id, secao.SecaoId, secao.Concluido));
                else
                {
                    secaoExistente.Concluido = secao.Concluido;
                    await mediator.Send(new AlterarEncaminhamentoNAAPASecaoCommand(secaoExistente));
                }

                var resultadoEncaminhamentoSecao = secaoExistente.Id;

                foreach (var questoes in secao.Questoes.GroupBy(q => q.QuestaoId))
                {
                    var questaoExistente = secaoExistente.Questoes.FirstOrDefault(q => q.QuestaoId == questoes.FirstOrDefault().QuestaoId);

                    if (questaoExistente == null)
                    {
                        var resultadoEncaminhamentoQuestao = await mediator.Send(new RegistrarEncaminhamentoNAAPASecaoQuestaoCommand(resultadoEncaminhamentoSecao, questoes.FirstOrDefault().QuestaoId));
                        await RegistrarRespostaEncaminhamento(questoes, resultadoEncaminhamentoQuestao);
                    }
                    else
                    {
                        if (questaoExistente.Excluido)
                            await AlterarQuestaoExcluida(questaoExistente);

                        await ExcluirRespostasEncaminhamento(questaoExistente, questoes);

                        await AlterarRespostasEncaminhamento(questaoExistente, questoes);

                        await IncluirRespostasEncaminhamento(questaoExistente, questoes);
                    }
                }

                foreach (var questao in secaoExistente.Questoes.Where(x => !secao.Questoes.Any(s => s.QuestaoId == x.QuestaoId)))
                    await mediator.Send(new ExcluirQuestaoEncaminhamentoNAAPAPorIdCommand(questao.Id));
            }
        }

        private async Task AlterarQuestaoExcluida(QuestaoEncaminhamentoNAAPA questao)
        {
            questao.Excluido = false;
            await mediator.Send(new AlterarQuestaoEncaminhamentoNAAPACommand(questao));
        }

        private async Task IncluirRespostasEncaminhamento(QuestaoEncaminhamentoNAAPA questaoExistente, IGrouping<long, EncaminhamentoNAAPASecaoQuestaoDto> respostas)
            => await RegistrarRespostaEncaminhamento(ObterRespostasAIncluir(questaoExistente, respostas), questaoExistente.Id);

        private async Task RegistrarRespostaEncaminhamento(IEnumerable<EncaminhamentoNAAPASecaoQuestaoDto> questoes, long questaoEncaminhamentoId)
        {
            foreach (var questao in questoes)
            {
                await mediator.Send(new RegistrarEncaminhamentoNAAPASecaoQuestaoRespostaCommand(questao.Resposta, questaoEncaminhamentoId, questao.TipoQuestao));
            }
        }

        private async Task AlterarRespostasEncaminhamento(QuestaoEncaminhamentoNAAPA questaoExistente, IGrouping<long, EncaminhamentoNAAPASecaoQuestaoDto> respostas)
        {
            foreach (var respostaAlterar in ObterRespostasAAlterar(questaoExistente, respostas))
                await mediator.Send(new AlterarEncaminhamentoNAAPASecaoQuestaoRespostaCommand(respostaAlterar, respostas.FirstOrDefault(c => c.RespostaEncaminhamentoId == respostaAlterar.Id)));
        }

        private async Task ExcluirRespostasEncaminhamento(QuestaoEncaminhamentoNAAPA questoesExistentes, IGrouping<long, EncaminhamentoNAAPASecaoQuestaoDto> respostas)
        {
            foreach (var respostasExcluir in ObterRespostasAExcluir(questoesExistentes, respostas))
                await mediator.Send(new ExcluirRespostaEncaminhamentoNAAPACommand(respostasExcluir));
        }

        private IEnumerable<EncaminhamentoNAAPASecaoQuestaoDto> ObterRespostasAIncluir(QuestaoEncaminhamentoNAAPA questaoExistente, IGrouping<long, EncaminhamentoNAAPASecaoQuestaoDto> respostas)
            => respostas.Where(c => c.RespostaEncaminhamentoId == 0);

        private IEnumerable<RespostaEncaminhamentoNAAPA> ObterRespostasAExcluir(QuestaoEncaminhamentoNAAPA questaoExistente, IGrouping<long, EncaminhamentoNAAPASecaoQuestaoDto> respostasEncaminhamento)
        {
            var retorno = questaoExistente.Respostas.Where(s => !respostasEncaminhamento.Any(c => c.RespostaEncaminhamentoId == s.Id));
            return retorno;
        }

        private IEnumerable<RespostaEncaminhamentoNAAPA> ObterRespostasAAlterar(QuestaoEncaminhamentoNAAPA questaoExistente, IGrouping<long, EncaminhamentoNAAPASecaoQuestaoDto> respostasEncaminhamento)
            => questaoExistente.Respostas.Where(s => respostasEncaminhamento.Any(c => c.RespostaEncaminhamentoId == s.Id));

        public async Task SalvarEncaminhamento(EncaminhamentoNAAPADto encaminhamentoNAAPADto, ResultadoEncaminhamentoNAAPADto resultadoEncaminhamento)
        {
            foreach (var secao in encaminhamentoNAAPADto.Secoes)
            {
                if (!secao.Questoes.Any())
                    throw new NegocioException(string.Format(MensagemNegocioComuns.NENHUMA_QUESTAO_FOI_ENCONTRADA_NA_SECAO_X,secao.SecaoId));

                var secaoEncaminhamento = await mediator.Send(new RegistrarEncaminhamentoNAAPASecaoCommand(resultadoEncaminhamento.Id, secao.SecaoId, secao.Concluido));

                foreach (var questoes in secao.Questoes.GroupBy(q => q.QuestaoId))
                {
                    var resultadoEncaminhamentoQuestao = await mediator.Send(new RegistrarEncaminhamentoNAAPASecaoQuestaoCommand(secaoEncaminhamento.Id, questoes.FirstOrDefault().QuestaoId));
                    await RegistrarRespostaEncaminhamento(questoes, resultadoEncaminhamentoQuestao);
                }
            }
        }

        private bool EhQuestaoObrigatoriaNaoRespondida(QuestaoDto questao)
        {
            return questao.Obrigatorio && !questao.Resposta.NaoNuloEContemRegistrosRespondidos();
        }
        
        private void ValidaRecursivo(SecaoQuestionarioDto secao, string questaoPaiOrdem, IEnumerable<QuestaoDto> questoes, ICollection<dynamic> questoesObrigatoriasAConsistir)
        {
            foreach (var questao in questoes)
            {
                var ordem = questaoPaiOrdem != "" ? $"{questaoPaiOrdem}.{questao.Ordem.ToString()}" : questao.Ordem.ToString();

                if (EhQuestaoObrigatoriaNaoRespondida(questao)) {
                    questoesObrigatoriasAConsistir.Add(new { SecaoId = secao.Id, Secao = secao.Nome, Ordem = ordem });
                }
                else
                if (questao.OpcaoResposta.NaoNuloEContemRegistros() && questao.Resposta.NaoNuloEContemRegistrosRespondidos())
                {
                    foreach (var resposta in questao.Resposta)
                    {
                        var opcao = questao.OpcaoResposta.FirstOrDefault(opcao => opcao.Id == Convert.ToInt64(resposta.Texto));
                        
                        if (opcao != null && opcao.QuestoesComplementares.Any())
                        {
                            ValidaRecursivo(secao, ordem, opcao.QuestoesComplementares, questoesObrigatoriasAConsistir);
                        }
                    }
                }
            }
        }

        private async Task ValidaQuestaoObservacaoAgrupamentoPromocaoCuidados(SecaoQuestionarioDto secao, IEnumerable<QuestaoDto> questoes, ICollection<dynamic> questoesObrigatoriasNaoRespondidas)
        {
            var questaoObservacoes = ObterQuestaoObservacoesAgrupamentoPromocaoCuidadosNaoPreenchida(questoes);
            if (questaoObservacoes != null)
            {
                var questaoAgrupamentoPromocaoCuidados = ObterQuestaoAgrupamentoPromocaoCuidados(questoes);
                var questoesComplementares = ObterQuestoesComplementaresAgrupamentoPromocaoCuidados(questaoAgrupamentoPromocaoCuidados);
                var questaoAdoeceComFrequencia = ObterQuestaoComplementarTipoAdoeceComFrequenciaSemCuidadosMedicos(questoesComplementares);
                var questaoDoencaCronica = ObterQuestaoComplementarTipoDoencaCronicaTratamentoLongaDuracao(questoesComplementares);

                if (questaoAdoeceComFrequencia != null)
                {
                    var opcaoOutras_QuestaoAdoeceComFrequencia = (await mediator.Send(new ObterOpcoesRespostaPorQuestaoIdQuery(questaoAdoeceComFrequencia.Id))).FirstOrDefault(opcao => opcao.Nome == "Outras");
                    if (questaoAdoeceComFrequencia.Resposta.Any(resposta => resposta.Texto == opcaoOutras_QuestaoAdoeceComFrequencia.Id.ToString()))
                        questoesObrigatoriasNaoRespondidas.Add(new { SecaoId = secao.Id, Secao = secao.Nome, Ordem = questaoObservacoes.Ordem });
                }

                if (questaoDoencaCronica != null)
                { 
                    var opcaoOutras_QuestaoDoencaCronica = (await mediator.Send(new ObterOpcoesRespostaPorQuestaoIdQuery(questaoDoencaCronica.Id))).FirstOrDefault(opcao => opcao.Nome == "Outras");
                    if (questaoDoencaCronica.Resposta.Any(resposta => resposta.Texto == opcaoOutras_QuestaoDoencaCronica.Id.ToString()))
                        questoesObrigatoriasNaoRespondidas.Add(new { SecaoId = secao.Id, Secao = secao.Nome, Ordem = questaoObservacoes.Ordem });
                }
            }
        }

        private QuestaoDto ObterQuestaoObservacoesAgrupamentoPromocaoCuidadosNaoPreenchida(IEnumerable<QuestaoDto> questoes)
        {
            return questoes.Where(questao => (questao.NomeComponente == QUESTAO_OBSERVACOES_AGRUPAMENTO_PROMOCAO_CUIDADOS)
                                              && !questao.Resposta.NaoNuloEContemRegistrosRespondidos()).FirstOrDefault();
        }

        private QuestaoDto ObterQuestaoAgrupamentoPromocaoCuidados(IEnumerable<QuestaoDto> questoes)
        {
            return questoes.Where(questao => (questao.NomeComponente == QUESTAO_AGRUPAMENTO_PROMOCAO_CUIDADOS)).FirstOrDefault();
        }

        private IEnumerable<QuestaoDto> ObterQuestoesComplementaresAgrupamentoPromocaoCuidados(QuestaoDto questaoAgrupamentoPromocaoCuidados)
        {
            return questaoAgrupamentoPromocaoCuidados.OpcaoResposta.SelectMany(opcao => opcao.QuestoesComplementares)
                                                  .Where(questaoComplementar => questaoComplementar.NomeComponente == QUESTAO_TIPO_ADOECE_COM_FREQUENCIA_SEM_CUIDADOS_MEDICOS
                                                                                || questaoComplementar.NomeComponente == QUESTAO_TIPO_DOENCA_CRONICA_TRATAMENTO_LONGA_DURACAO).Distinct();
        }

        private QuestaoDto ObterQuestaoComplementarTipoAdoeceComFrequenciaSemCuidadosMedicos(IEnumerable<QuestaoDto> questoesComplementares)
        {
            return questoesComplementares.Where(questaoComplementar => questaoComplementar.NomeComponente == QUESTAO_TIPO_ADOECE_COM_FREQUENCIA_SEM_CUIDADOS_MEDICOS).FirstOrDefault();
        }

        private QuestaoDto ObterQuestaoComplementarTipoDoencaCronicaTratamentoLongaDuracao(IEnumerable<QuestaoDto> questoesComplementares)
        {
            return questoesComplementares.Where(questaoComplementar => questaoComplementar.NomeComponente == QUESTAO_TIPO_DOENCA_CRONICA_TRATAMENTO_LONGA_DURACAO).FirstOrDefault();
        }

        private async Task ObterQuestoesObrigatoriasNaoPreechidas(EncaminhamentoNAAPADto encaminhamentoNAAPADto, ICollection<dynamic> questoesObrigatoriasAConsistir)
        {
            var secoesEtapa = await mediator.Send(new ObterSecoesEncaminhamentoNAAPADtoPorEtapaQuery(1));

            var respostasPersistidas = encaminhamentoNAAPADto.Id.HasValue
                ? (await mediator.Send(
                    new ObterQuestaoRespostaEncaminhamentoNAAPAPorIdQuery(encaminhamentoNAAPADto.Id.Value)))
                .Select(resposta => new EncaminhamentoNAAPASecaoQuestaoDto
                {
                    QuestaoId = resposta.QuestaoId,
                    Resposta = (resposta.RespostaId ?? 0) != 0 ? resposta.RespostaId?.ToString() : resposta.Texto,
                    RespostaEncaminhamentoId = resposta.Id
                })
                : Enumerable.Empty<EncaminhamentoNAAPASecaoQuestaoDto>();

            foreach (var secao in secoesEtapa)
            {
                var secaoPresenteDto = encaminhamentoNAAPADto.Secoes.FirstOrDefault(secaoDto => secaoDto.SecaoId == secao.Id);

                IEnumerable<EncaminhamentoNAAPASecaoQuestaoDto> respostasEncaminhamento;
                if (secaoPresenteDto != null && secaoPresenteDto.Questoes.Any())
                {
                    respostasEncaminhamento = secaoPresenteDto.Questoes
                        .Select(questao => new EncaminhamentoNAAPASecaoQuestaoDto()
                        {
                            QuestaoId = questao.QuestaoId,
                            Resposta = questao.Resposta,
                            RespostaEncaminhamentoId = questao.RespostaEncaminhamentoId
                        })
                        .Where(questao => !string.IsNullOrEmpty(questao.Resposta));
                }
                else respostasEncaminhamento = respostasPersistidas;

                var respostaEncaminhamentoQuestao = respostasEncaminhamento;
                var questoes = await mediator.Send(new ObterQuestoesPorQuestionarioPorIdQuery(secao.QuestionarioId,
                    questaoId =>
                        respostaEncaminhamentoQuestao.Where(c => c.QuestaoId == questaoId)
                            .Select(respostaQuestao => new RespostaQuestaoDto
                            {
                                Id = GetHashCode(),
                                OpcaoRespostaId = 0,
                                Texto = respostaQuestao.Resposta,
                                Arquivo = null
                            })));

                if (secao.NomeComponente == SECAO_QUESTOES_APRESENTADAS)
                    await ValidaQuestaoObservacaoAgrupamentoPromocaoCuidados(secao, questoes, questoesObrigatoriasAConsistir);
                
                ValidaRecursivo(secao, "", questoes, questoesObrigatoriasAConsistir);
            }
        }
    }
}

