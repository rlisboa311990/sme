﻿using MediatR;
using SME.SGP.Aplicacao.Queries;
using SME.SGP.Dominio;
using SME.SGP.Dominio.Interfaces;
using SME.SGP.Infra;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SME.SGP.Aplicacao
{
    public class GravarConselhoClasseCommadHandler : IRequestHandler<GravarConselhoClasseCommad, ConselhoClasseNotaRetornoDto>
    {
        private readonly IMediator mediator;
        private readonly IRepositorioConselhoClasseAlunoConsulta repositorioConselhoClasseAlunoConsulta;

        public GravarConselhoClasseCommadHandler(
                        IMediator mediator, 
                        IRepositorioConselhoClasseAlunoConsulta repositorioConselhoClasseAlunoConsulta)
        {
            this.mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            this.repositorioConselhoClasseAlunoConsulta = repositorioConselhoClasseAlunoConsulta ?? throw new ArgumentNullException(nameof(repositorioConselhoClasseAlunoConsulta));
        }

        public async Task<ConselhoClasseNotaRetornoDto> Handle(GravarConselhoClasseCommad request, CancellationToken cancellationToken)
        {
            var usuarioLogado = await mediator.Send(new ObterUsuarioLogadoQuery());

            var conselhoClasseNotaRetorno = request.ConselhoClasseId == 0 ?
                await mediator.Send(new InserirConselhoClasseNotaCommad(
                                            request.FechamentoTurma,
                                            request.CodigoAluno,
                                            request.ConselhoClasseNotaDto,
                                            request.Bimestre,
                                            usuarioLogado)) :
                await mediator.Send(new AlterarConselhoClasseCommad(
                                            request.ConselhoClasseId,
                                            request.FechamentoTurma.Id,
                                            request.CodigoAluno,
                                            request.FechamentoTurma.Turma,
                                            request.ConselhoClasseNotaDto,
                                            request.Bimestre,
                                            usuarioLogado));

            // TODO Verificar se o fechamentoTurma.Turma carregou UE
            if (await mediator.Send(new VerificaNotasTodosComponentesCurricularesQuery(
                                                        request.CodigoAluno, 
                                                        request.FechamentoTurma.Turma, 
                                                        request.FechamentoTurma.PeriodoEscolarId)))
            {
                var conselhoClasseAluno = await repositorioConselhoClasseAlunoConsulta.ObterPorIdAsync(conselhoClasseNotaRetorno.ConselhoClasseAlunoId);
                await VerificaRecomendacoesAluno(conselhoClasseAluno);
            }

            if (!await mediator.Send(new AtualizaSituacaoConselhoClasseCommand(conselhoClasseNotaRetorno.ConselhoClasseId)))
                throw new NegocioException("Erro ao atualizar situação do conselho de classe");

            return conselhoClasseNotaRetorno;
        }

        private async Task<ConselhoClasseAluno> VerificaRecomendacoesAluno(ConselhoClasseAluno conselhoClasseAluno)
        {
            if (string.IsNullOrEmpty(conselhoClasseAluno.RecomendacoesAluno) || string.IsNullOrEmpty(conselhoClasseAluno.RecomendacoesFamilia))
            {
                var recomendacoes = await mediator.Send(new ObterTextoRecomendacoesAlunoFamiliaQuery());

                conselhoClasseAluno.RecomendacoesAluno = string.IsNullOrEmpty(conselhoClasseAluno.RecomendacoesAluno) ? recomendacoes.recomendacoesAluno : conselhoClasseAluno.RecomendacoesAluno;
                conselhoClasseAluno.RecomendacoesFamilia = string.IsNullOrEmpty(conselhoClasseAluno.RecomendacoesFamilia) ? recomendacoes.recomendacoesFamilia : conselhoClasseAluno.RecomendacoesFamilia;

            }

            return conselhoClasseAluno;
        }
    }
}
