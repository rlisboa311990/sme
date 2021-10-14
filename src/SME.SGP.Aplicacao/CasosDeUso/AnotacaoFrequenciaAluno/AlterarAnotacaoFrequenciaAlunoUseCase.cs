﻿using MediatR;
using SME.SGP.Dominio;
using SME.SGP.Infra;
using System;
using System.Threading.Tasks;

namespace SME.SGP.Aplicacao
{
    public class AlterarAnotacaoFrequenciaAlunoUseCase : AbstractUseCase, IAlterarAnotacaoFrequenciaAlunoUseCase
    {
        public AlterarAnotacaoFrequenciaAlunoUseCase(IMediator mediator) : base(mediator)
        {
        }

        public async Task<bool> Executar(AlterarAnotacaoFrequenciaAlunoDto param)
        {
            var anotacao = await ObterAnotacao(param.Id);

            var aula = await mediator.Send(new ObterAulaPorIdQuery(anotacao.AulaId));
            var usuario = await mediator.Send(new ObterUsuarioLogadoQuery());

            if (!usuario.EhProfessorCj() && !usuario.EhGestorEscolar())
                await ValidarAtribuicaoUsuario(long.Parse(aula.DisciplinaId), aula.TurmaId, aula.DataAula, usuario);
            MoverRemoverExcluidos(param,anotacao);
            return await AtualizarAnotacaoFrequenciaAluno(anotacao, param);
        }
        private void MoverRemoverExcluidos(AlterarAnotacaoFrequenciaAlunoDto anotacaoAluno, AnotacaoFrequenciaAluno anotacao)
        {
            if (!string.IsNullOrEmpty(anotacaoAluno.Anotacao))
            {
                var moverArquivo = mediator.Send(new MoverArquivosTemporariosCommand(TipoArquivo.FrequenciaAnotacaoEstudante, anotacao.Anotacao, anotacaoAluno.Anotacao));
                anotacaoAluno.Anotacao = moverArquivo.Result;
            }
            if (!string.IsNullOrEmpty(anotacao.Anotacao))
            {
                var deletarArquivosNaoUtilziados = mediator.Send(new RemoverArquivosExcluidosCommand(anotacao.Anotacao, anotacaoAluno.Anotacao, TipoArquivo.FrequenciaAnotacaoEstudante.Name()));
            }
        }
        private async Task<bool> AtualizarAnotacaoFrequenciaAluno(AnotacaoFrequenciaAluno anotacao, AlterarAnotacaoFrequenciaAlunoDto param)
        {
            anotacao.MotivoAusenciaId = param.MotivoAusenciaId;
            anotacao.Anotacao = param.Anotacao;

            return await mediator.Send(new AlterarAnotacaoFrequenciaAlunoCommand(anotacao));
        }

        private async Task ValidarAtribuicaoUsuario(long componenteCurricularId, string turmaId, DateTime dataAula, Usuario usuarioLogado)
        {
            var usuarioPossuiAtribuicaoNaTurmaNaData = await mediator.Send(new ObterUsuarioPossuiPermissaoNaTurmaEDisciplinaQuery(componenteCurricularId, turmaId, dataAula, usuarioLogado));
            if (!usuarioPossuiAtribuicaoNaTurmaNaData)
                throw new NegocioException("Você não pode fazer alterações ou inclusões nesta turma, componente e data.");
        }

        private async Task<AnotacaoFrequenciaAluno> ObterAnotacao(long id)
        {
            var anotacao = await mediator.Send(new ObterAnotacaoFrequenciaAlunoPorIdQuery(id));
            if (anotacao == null)
                throw new NegocioException("Anotação não localizada com o Id informado");

            return anotacao;
        }
    }
}
