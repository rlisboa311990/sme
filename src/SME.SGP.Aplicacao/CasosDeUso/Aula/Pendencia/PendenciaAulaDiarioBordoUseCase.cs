﻿using MediatR;
using SME.SGP.Aplicacao.Interfaces;
using SME.SGP.Dominio;
using SME.SGP.Infra;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SME.SGP.Aplicacao
{
    public class PendenciaAulaDiarioBordoUseCase : AbstractUseCase, IPendenciaAulaDiarioBordoUseCase
    {
        public PendenciaAulaDiarioBordoUseCase(IMediator mediator) : base(mediator)
        {
        }

        public async Task<bool> Executar(MensagemRabbit param)
        {
            var filtro = param.ObterObjetoMensagem<DreUeDto>();
            var aulas = await mediator.Send(new ObterPendenciasDiarioBordoQuery(filtro.DreId));

            if (aulas != null && aulas.Any())
                await RegistraPendencia(aulas);

            return true;
        }

        private async Task RegistraPendencia(IEnumerable<Aula> aulas)
         => await mediator.Send(new SalvarPendenciaDiarioBordoCommand(aulas));

    }
}
