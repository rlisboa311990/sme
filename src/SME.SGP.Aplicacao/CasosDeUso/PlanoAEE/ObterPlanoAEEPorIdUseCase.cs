﻿using MediatR;
using SME.SGP.Aplicacao.Integracoes;
using SME.SGP.Dominio;
using SME.SGP.Dominio.Enumerados;
using SME.SGP.Infra;
using SME.SGP.Infra.Dtos;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace SME.SGP.Aplicacao
{
    public class ObterPlanoAEEPorIdUseCase : AbstractUseCase, IObterPlanoAEEPorIdUseCase
    {
        private readonly IServicoEol _servicoEol;
        public ObterPlanoAEEPorIdUseCase(IMediator mediator,
            IServicoEol servicoEol) : base(mediator)

        {
            _servicoEol = servicoEol;
        }

        public async Task<PlanoAEEDto> Executar(FiltroPesquisaQuestoesPorPlanoAEEIdDto filtro)
        {
            var plano = new PlanoAEEDto();
            var respostasPlano = Enumerable.Empty<RespostaQuestaoDto>();

            PlanoAEEVersaoDto ultimaVersao = null;

            if (filtro.PlanoAEEId.HasValue && filtro.PlanoAEEId > 0)
            {
                var entidadePlano = await mediator.Send(new ObterPlanoAEEComTurmaPorIdQuery(filtro.PlanoAEEId.Value));
                var alunosTurma = await _servicoEol.ObterAlunosPorTurma(entidadePlano.Turma.CodigoTurma);

                var codigoSituacaoMatricula = alunosTurma.OrderByDescending(c => c.DataSituacao)
                    .FirstOrDefault(c => c.CodigoAluno.ToString() == entidadePlano.AlunoCodigo)
                    .CodigoSituacaoMatricula;

                var anoLetivo = entidadePlano.Turma.AnoLetivo;

                switch (codigoSituacaoMatricula)
                {
                    case SituacaoMatriculaAluno.Ativo:
                    case SituacaoMatriculaAluno.Rematriculado:
                        {
                            if (entidadePlano.AlteradoEm?.Year != null)
                                anoLetivo = (int)entidadePlano.AlteradoEm?.Year;
    
                            break;
                        }
                }

                var alunoPorTurmaResposta = await mediator.Send(new ObterAlunoPorCodigoEolQuery(entidadePlano.AlunoCodigo, anoLetivo, entidadePlano.Turma.EhTurmaHistorica, false));

                if (alunoPorTurmaResposta == null)
                    throw new NegocioException("Aluno não localizado");

                var aluno = new AlunoReduzidoDto()
                {
                    Nome = !string.IsNullOrEmpty(alunoPorTurmaResposta.NomeAluno) ? alunoPorTurmaResposta.NomeAluno : alunoPorTurmaResposta.NomeSocialAluno,
                    NumeroAlunoChamada = alunoPorTurmaResposta.NumeroAlunoChamada,
                    DataNascimento = alunoPorTurmaResposta.DataNascimento,
                    DataSituacao = alunoPorTurmaResposta.DataSituacao,
                    CodigoAluno = alunoPorTurmaResposta.CodigoAluno,
                    Situacao = alunoPorTurmaResposta.SituacaoMatricula,
                    TurmaEscola = await OberterNomeTurmaFormatado(alunoPorTurmaResposta.CodigoTurma.ToString()),
                    NomeResponsavel = alunoPorTurmaResposta.NomeResponsavel,
                    TipoResponsavel = alunoPorTurmaResposta.TipoResponsavel,
                    CelularResponsavel = alunoPorTurmaResposta.CelularResponsavel,
                    DataAtualizacaoContato = alunoPorTurmaResposta.DataAtualizacaoContato,
                    EhAtendidoAEE = entidadePlano.Situacao != SituacaoPlanoAEE.Encerrado && entidadePlano.Situacao != SituacaoPlanoAEE.EncerradoAutomaticamente
                };

                plano.Id = filtro.PlanoAEEId.Value;
                plano.Auditoria = (AuditoriaDto)entidadePlano;
                plano.Versoes = await mediator.Send(new ObterVersoesPlanoAEEQuery(filtro.PlanoAEEId.Value));
                plano.Aluno = aluno;
                plano.Situacao = entidadePlano.Situacao;
                plano.SituacaoDescricao = entidadePlano.Situacao.Name();

                var turma = await mediator.Send(new ObterTurmaPorCodigoQuery(alunoPorTurmaResposta.CodigoTurma.ToString()));
                var ue = await mediator.Send(new ObterUeComDrePorIdQuery(turma.UeId));

                plano.Turma = new TurmaAnoDto()
                {
                    Id = turma.Id,
                    Codigo = turma.CodigoTurma,
                    AnoLetivo = turma.AnoLetivo,
                    CodigoUE = ue.CodigoUe
                };

                filtro.TurmaCodigo = turma.CodigoTurma;

                ultimaVersao = plano.Versoes.OrderByDescending(a => a.Numero).First();

                plano.Versoes = plano.Versoes.Where(a => a.Id != ultimaVersao.Id).ToList();
                plano.UltimaVersao = ultimaVersao;
                plano.PodeDevolverPlanoAEE = await PodeDevolverPlanoAEE(entidadePlano.SituacaoPodeDevolverPlanoAEE());
            }

            var questionarioId = await mediator.Send(new ObterQuestionarioPlanoAEEIdQuery());
            var ultimaVersaoId = ultimaVersao?.Id ?? 0;

            plano.Questoes = await mediator.Send(new ObterQuestoesPlanoAEEPorVersaoQuery(questionarioId, ultimaVersaoId, filtro.TurmaCodigo));

            plano.QuestionarioId = questionarioId;

            return plano;
        }

        private async Task<string> OberterNomeTurmaFormatado(string turmaId)
        {
            var turmaNome = "";
            var turma = await mediator.Send(new ObterTurmaPorCodigoQuery(turmaId));

            if (turma != null)
                turmaNome = $"{turma.ModalidadeCodigo.ShortName()} - {turma.Nome}";

            return turmaNome;
        }

        private async Task<bool> PodeDevolverPlanoAEE(bool situacaoPodeDevolverPlanoAEE)
        {
            var usuario = await mediator.Send(new ObterUsuarioLogadoQuery());

            if (usuario == null)
                throw new NegocioException("Usuário não localizado");

            if (usuario.EhPerfilProfessor())
                return false;

            if (!situacaoPodeDevolverPlanoAEE)
                return false;

            return true;
        }
    }
}
