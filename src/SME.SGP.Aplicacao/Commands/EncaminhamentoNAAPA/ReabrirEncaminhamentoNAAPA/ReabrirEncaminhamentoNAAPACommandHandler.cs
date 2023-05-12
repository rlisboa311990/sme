﻿using MediatR;
using SME.SGP.Dominio;
using SME.SGP.Dominio.Constantes.MensagensNegocio;
using SME.SGP.Dominio.Interfaces;
using System;
using System.Threading;
using System.Threading.Tasks;
using Dominio.Enumerados;
using SME.SGP.Dominio.Enumerados;
using SME.SGP.Infra;
using System.Collections.Generic;
using System.Linq;
using SME.SGP.Dados;
using System.ComponentModel.DataAnnotations;

namespace SME.SGP.Aplicacao
{
    public class ReabrirEncaminhamentoNAAPACommandHandler : IRequestHandler<ReabrirEncaminhamentoNAAPACommand, SituacaoDto>
    {
        private readonly IUnitOfWork unitOfWork;
        public ReabrirEncaminhamentoNAAPACommandHandler(IUnitOfWork unitOfWork, IMediator mediator, IRepositorioEncaminhamentoNAAPA repositorioEncaminhamentoNAAPA)
        {
            this.mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            this.repositorioEncaminhamentoNAAPA = repositorioEncaminhamentoNAAPA ?? throw new ArgumentNullException(nameof(repositorioEncaminhamentoNAAPA));
            this.unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        }

        public IMediator mediator { get; }
        public IRepositorioEncaminhamentoNAAPA repositorioEncaminhamentoNAAPA { get; }

        public async Task<SituacaoDto> Handle(ReabrirEncaminhamentoNAAPACommand request, CancellationToken cancellationToken)
        {
            var encaminhamentoNAAPA = await mediator.Send(new ObterCabecalhoEncaminhamentoNAAPAQuery(request.EncaminhamentoId));
            if (encaminhamentoNAAPA == null || encaminhamentoNAAPA.Id == 0)
                throw new NegocioException(MensagemNegocioEncaminhamentoNAAPA.ENCAMINHAMENTO_NAO_ENCONTRADO);

            if (encaminhamentoNAAPA.Situacao != SituacaoNAAPA.Encerrado)
                throw new NegocioException(MensagemNegocioEncaminhamentoNAAPA.ENCAMINHAMENTO_NAO_PODE_SER_REABERTO_NESTA_SITUACAO);

            var matriculasAlunoEol = (await mediator.Send(new ObterAlunosEolPorCodigosQuery(long.Parse(encaminhamentoNAAPA.AlunoCodigo), true)));
            var matriculaVigenteAluno = FiltrarMatriculaVigenteAluno(matriculasAlunoEol);
            if (matriculaVigenteAluno == null || matriculaVigenteAluno.Inativo)
                throw new NegocioException(MensagemNegocioEncaminhamentoNAAPA.ENCAMINHAMENTO_ALUNO_INATIVO_NAO_PODE_SER_REABERTO);

            var situacaoDTO = new SituacaoDto() { Codigo = (int)encaminhamentoNAAPA.Situacao, Descricao = encaminhamentoNAAPA.Situacao.GetAttribute<DisplayAttribute>().Name };
            encaminhamentoNAAPA.Situacao = (await repositorioEncaminhamentoNAAPA.EncaminhamentoContemAtendimentosItinerancia(request.EncaminhamentoId)) 
                                            ? SituacaoNAAPA.EmAtendimento : SituacaoNAAPA.AguardandoAtendimento;

            using (var transacao = unitOfWork.IniciarTransacao())
            {
                try
                {
                    await repositorioEncaminhamentoNAAPA.SalvarAsync(encaminhamentoNAAPA);
                }
                catch (Exception)
                {
                    unitOfWork.Rollback();
                    throw;
                }
            }

            return situacaoDTO;
        }

        private TurmasDoAlunoDto FiltrarMatriculaVigenteAluno(IEnumerable<TurmasDoAlunoDto> matriculasAluno)
        {
            return matriculasAluno.Where(turma => turma.CodigoTipoTurma == (int)TipoTurma.Regular
                                                     && turma.AnoLetivo <= DateTimeExtension.HorarioBrasilia().Year
                                                     && turma.DataSituacao.Date <= DateTimeExtension.HorarioBrasilia().Date)
                                     .OrderByDescending(turma => turma.AnoLetivo)
                                     .ThenByDescending(turma => turma.DataSituacao)
                                     .FirstOrDefault();
        }
    }
}
