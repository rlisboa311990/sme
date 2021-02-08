﻿using MediatR;
using SME.SGP.Dominio;
using SME.SGP.Dominio.Entidades;
using SME.SGP.Dominio.Interfaces;
using SME.SGP.Infra;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SME.SGP.Aplicacao
{
    public class ObterQuestionarioEncaminhamentoAeeQueryHandler : IRequestHandler<ObterQuestionarioEncaminhamentoAeeQuery, IEnumerable<QuestaoDto>>
    {
        private readonly IMediator mediator;
        private readonly IRepositorioQuestaoEncaminhamentoAEE repositorioQuestaoEncaminhamento;

        public ObterQuestionarioEncaminhamentoAeeQueryHandler(IMediator mediator, IRepositorioQuestaoEncaminhamentoAEE repositorioQuestaoEncaminhamento, IRepositorioQuestionario repositorioQuestionario)
        {
            this.mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            this.repositorioQuestaoEncaminhamento = repositorioQuestaoEncaminhamento ?? throw new ArgumentNullException(nameof(repositorioQuestaoEncaminhamento));
        }

        public async Task<IEnumerable<QuestaoDto>> Handle(ObterQuestionarioEncaminhamentoAeeQuery request, CancellationToken cancellationToken)
        {
            var respostasEncaminhamento = request.EncaminhamentoId.HasValue ?
                await repositorioQuestaoEncaminhamento.ObterRespostasEncaminhamento(request.EncaminhamentoId.Value) :
                Enumerable.Empty<RespostaQuestaoEncaminhamentoAEEDto>();

            var questoes = await mediator.Send(new ObterQuestoesPorQuestionarioPorIdQuery(request.QuestionarioId , questaoId =>
                respostasEncaminhamento.Where(c => c.QuestaoId == questaoId)
                .Select(respostaEncaminhamento =>
                {
                    return new RespostaQuestaoDto()
                    {
                        Id = respostaEncaminhamento.Id,
                        OpcaoRespostaId = respostaEncaminhamento.RespostaId,
                        Texto = respostaEncaminhamento.Texto,
                        Arquivo = respostaEncaminhamento.Arquivo
                    };
                })));

            await AplicarRegrasEncaminhamento(request.QuestionarioId, questoes, request.CodigoAluno, request.CodigoTurma);

            return questoes;
        }

        private async Task AplicarRegrasEncaminhamento(long questionarioId, IEnumerable<QuestaoDto> questoes, string codigoAluno, string codigoTurma)
        {
            if (questionarioId == 1 && await ValidarFrequenciaGlobalAlunoInsuficiente(codigoAluno, codigoTurma))
            {
                var questaoJustificativa = ObterQuestaoJustificativa(questoes);
                questaoJustificativa.Obrigatorio = true;
            }
        }

        private QuestaoDto ObterQuestaoJustificativa(IEnumerable<QuestaoDto> questoes)
            => questoes.FirstOrDefault(c => c.Id == 2);

        private async Task<bool> ValidarFrequenciaGlobalAlunoInsuficiente(string codigoAluno, string codigoTurma)
        {
            var frequenciaGlobal = await mediator.Send(new ObterFrequenciaGeralAlunoQuery(codigoAluno, codigoTurma));
            var parametroPercentualFrequenciaCritico = await mediator.Send(new ObterParametroSistemaPorTipoEAnoQuery(TipoParametroSistema.PercentualFrequenciaCritico, DateTime.Now.Year));
            var percentualFrequenciaCritico = double.Parse(parametroPercentualFrequenciaCritico.Valor);

            return frequenciaGlobal < percentualFrequenciaCritico;
        }
    }
}
