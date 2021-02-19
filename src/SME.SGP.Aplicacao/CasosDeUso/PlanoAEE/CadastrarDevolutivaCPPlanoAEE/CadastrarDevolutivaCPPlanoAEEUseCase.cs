﻿using MediatR;
using SME.SGP.Dominio;
using SME.SGP.Infra;
using System;
using System.Threading.Tasks;

namespace SME.SGP.Aplicacao.CasosDeUso
{
    public class CadastrarDevolutivaCPPlanoAEEUseCase : ICadastrarDevolutivaCPPlanoAEEUseCase
    {
        private readonly IMediator mediator;

        public CadastrarDevolutivaCPPlanoAEEUseCase(IMediator mediator)
        {
            this.mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        }

        public async Task<bool> Executar(long planoAEEId, PlanoAEECadastroDevolutivaDto planoDto)
        {
            if (planoDto.ParecerCoordenacao == "")
                throw new NegocioException("O motivo da devolutiva deve ser cadastrado");

            var retorno = await mediator.Send(new CadastrarDevolutivaCPCommand(planoAEEId, planoDto.ParecerCoordenacao));
            return retorno;
        }
    }
}