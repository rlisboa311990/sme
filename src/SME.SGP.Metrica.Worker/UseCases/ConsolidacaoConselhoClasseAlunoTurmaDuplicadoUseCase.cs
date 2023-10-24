﻿using MediatR;
using SME.SGP.Infra;
using SME.SGP.Metrica.Worker.Commands;
using SME.SGP.Metrica.Worker.Repositorios.Interfaces;
using SME.SGP.Metrica.Worker.UseCases.Interfaces;
using System.Threading.Tasks;

namespace SME.SGP.Metrica.Worker.UseCases
{
    public class ConsolidacaoConselhoClasseAlunoTurmaDuplicadoUseCase : IConsolidacaoConselhoClasseAlunoTurmaDuplicadoUseCase
    {
        private readonly IRepositorioConsolidacaoConselhoClasseAlunoTurmaDuplicado repositorioDuplicados;
        private readonly IRepositorioSGPConsulta repositorioSGP;
        private readonly IMediator mediator;

        public ConsolidacaoConselhoClasseAlunoTurmaDuplicadoUseCase(IRepositorioConsolidacaoConselhoClasseAlunoTurmaDuplicado repositorioDuplicados,
            IRepositorioSGPConsulta repositorioSGP,
            IMediator mediator)
        {
            this.repositorioDuplicados = repositorioDuplicados ?? throw new System.ArgumentNullException(nameof(repositorioDuplicados));
            this.repositorioSGP = repositorioSGP ?? throw new System.ArgumentNullException(nameof(repositorioSGP));
            this.mediator = mediator ?? throw new System.ArgumentNullException(nameof(mediator));
        }

        public async Task<bool> Executar(MensagemRabbit param)
        {
            await repositorioDuplicados.ExcluirTodos();

            var ues = await repositorioSGP.ObterUesIds();
            foreach (var ue in ues)
                await mediator.Send(new PublicarFilaCommand(Rotas.RotasRabbitMetrica.DuplicacaoConsolidacaoCCAlunoTurmaUE, new FiltroIdDto(ue)));

            return true;
        }
    }
}
