﻿using MediatR;
using SME.SGP.Dominio;
using SME.SGP.Dominio.Enumerados;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SME.SGP.Aplicacao
{
    public class ConsolidacaoTurmaConselhoClasseAlunoAnosAnterioresCommandHandler : IRequestHandler<ConsolidacaoTurmaConselhoClasseAlunoAnosAnterioresCommand, bool>
    {
        private readonly IUnitOfWork unitOfWork;
        private readonly IMediator mediator;
        private readonly IRepositorioConselhoClasseConsolidadoNota repositorioConselhoClasseConsolidadoNota;

        public ConsolidacaoTurmaConselhoClasseAlunoAnosAnterioresCommandHandler(IUnitOfWork unitOfWork, IMediator mediator, IRepositorioConselhoClasseConsolidadoNota repositorioConselhoClasseConsolidadoNota)
        {
            this.unitOfWork = unitOfWork ?? throw new System.ArgumentNullException(nameof(unitOfWork));
            this.mediator = mediator ?? throw new System.ArgumentNullException(nameof(mediator));
            this.repositorioConselhoClasseConsolidadoNota = repositorioConselhoClasseConsolidadoNota ?? throw new System.ArgumentNullException(nameof(repositorioConselhoClasseConsolidadoNota));
        }


        public async Task<bool> Handle(ConsolidacaoTurmaConselhoClasseAlunoAnosAnterioresCommand request, CancellationToken cancellationToken)
        {
            try
            {
                foreach (var alunoNota in request.AlunoNotas)
                {
                    var consolidadoNota = await repositorioConselhoClasseConsolidadoNota.ObterConselhoClasseConsolidadoAlunoNotaPorConsolidadoBimestreDisciplinaAsync(request.ConsolidacaoId, alunoNota.Bimestre, alunoNota.DisciplinaId);
                    if (consolidadoNota == null)
                    {
                        var consolidadoAlunoNota = new ConselhoClasseConsolidadoTurmaAlunoNota()
                        {
                            Bimestre = alunoNota.Bimestre,
                            ComponenteCurricularId = alunoNota.DisciplinaId,
                            ConceitoId = alunoNota.ConceitoId,
                            Nota = alunoNota.Nota,
                            ConselhoClasseConsolidadoTurmaAlunoId = request.ConsolidacaoId
                        };
                        await repositorioConselhoClasseConsolidadoNota.SalvarAsync(consolidadoAlunoNota);
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                await mediator.Send(new SalvarLogViaRabbitCommand($"Não foi possível inserir o consolidado de conselho de classe aluno turma nota - Aluno Codigo: {request.AlunoNotas.FirstOrDefault().AlunoCodigo} - Consolidado: {request.ConsolidacaoId}", LogNivel.Critico, LogContexto.ConselhoClasse, ex.Message));
                return false;
            }
        }
    }
}
