﻿using MediatR;
using Sentry;
using SME.SGP.Dominio.Interfaces;
using SME.SGP.Infra;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SME.SGP.Aplicacao
{
    public class TrataSincronizacaoInstitucionalTurmaCommandHandler : IRequestHandler<TrataSincronizacaoInstitucionalTurmaCommand, bool>
    {
        private readonly IRepositorioTurma repositorioTurma;
        private readonly IMediator mediator;

        public TrataSincronizacaoInstitucionalTurmaCommandHandler(IRepositorioTurma repositorioTurma, IMediator mediator)
        {
            this.repositorioTurma = repositorioTurma ?? throw new ArgumentNullException(nameof(repositorioTurma));
            this.mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        }

        public async Task<bool> Handle(TrataSincronizacaoInstitucionalTurmaCommand request, CancellationToken cancellationToken)
        {
            var turma = request.Turma;

            if (turma.Situacao == "C")
                return await AtualizarTurmaParaHistoricaAsync(turma.Codigo.ToString());

            if (turma.Situacao == "E")
                return await VerificarTurmaExtintaAsync(turma);

            if (turma.Situacao == "O" || turma.Situacao == "A")
                return await IncluirTurmaAsync(turma);

            return true;
        }        

        private async Task<bool> VerificarTurmaExtintaAsync(TurmaParaSyncInstitucionalDto turma)
        {
            var anoAtual = DateTime.Now.Year;
            var tipoCalendarioId = await mediator.Send(new ObterIdTipoCalendarioPorAnoLetivoEModalidadeQuery(turma.CodigoModalidade, anoAtual, null));

            if (tipoCalendarioId > 0)
            {
                var periodosEscolares = await mediator.Send(new ObterPeriodosEscolaresPorTipoCalendarioIdQuery(tipoCalendarioId));
                if (periodosEscolares != null && periodosEscolares.Any())
                {
                    var primeiroPeriodo = periodosEscolares.OrderBy(x => x.Bimestre).First();

                    if (turma.DataStatusTurmaEscola.Date < primeiroPeriodo.PeriodoInicio.Date)
                    {
                        await ExcluirTurnaAsync(turma.Codigo.ToString());
                        return true;
                    }
                    else
                    {
                        return await AtualizarTurmaParaHistoricaAsync(turma.Codigo.ToString());
                    }
                }
                else
                {
                    await ExcluirTurnaAsync(turma.Codigo.ToString());
                    return true;
                }
            }
            else
            {
                await ExcluirTurnaAsync(turma.Codigo.ToString());
                return true;
            }
        }

        private async Task<bool> AtualizarTurmaParaHistoricaAsync(string turmaId)
        {
            var turmaAtualizada = await repositorioTurma.AtualizarTurmaParaHistorica(turmaId);

            if (!turmaAtualizada)
            {
                SentrySdk.CaptureMessage($"Não foi possível atualizar a turma id {turmaId} para histórica.");
                return false;
            }
            return true;
        }

        private async Task<bool> IncluirTurmaAsync(TurmaParaSyncInstitucionalDto turma)
        {
            var turmaSgp = await mediator.Send(new ObterTurmaPorCodigoQuery(turma.Codigo.ToString()));

            if(turmaSgp != null)
            {
                SentrySdk.CaptureMessage($"Não foi possível Incluir a turma de código {turma.Codigo}. Turma já existe na base Sgp");
                return false;
            }

            var ue = await mediator.Send(new ObterUeComDrePorCodigoQuery(turma.UeCodigo));           

            if (ue == null)
            {
                SentrySdk.CaptureMessage($"Não foi possível Incluir a turma de código {turma.Codigo}. Pois não foi encontrado a UE {turma.UeCodigo}.");
                return false;
            }                   

            return await repositorioTurma.SalvarAsync(turma, ue.Id);
        }

        private async Task ExcluirTurnaAsync(string turmaId)
            => await repositorioTurma.ExcluirTurmaExtintaAsync(turmaId);
    }
}
