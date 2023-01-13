﻿using MediatR;
using SME.SGP.Aplicacao.Interfaces;
using SME.SGP.Dominio;
using SME.SGP.Infra;
using System;
using System.Globalization;
using System.Threading.Tasks;

namespace SME.SGP.Aplicacao
{
    public class PendenciaAulaUseCase : AbstractUseCase, IPendenciaAulaUseCase
    {
        public PendenciaAulaUseCase(IMediator mediator) : base(mediator)
        {
        }

        public async Task<bool> Executar(MensagemRabbit param)
        {
            var dadosParametroGeracaoPendencias = await mediator.Send(new ObterParametroSistemaPorTipoEAnoQuery(TipoParametroSistema.DataInicioGeracaoPendencias, DateTimeExtension.HorarioBrasilia().Year));
            
            if(dadosParametroGeracaoPendencias != null)
            {
                var dataDefinida = Convert.ToDateTime(dadosParametroGeracaoPendencias.Valor, CultureInfo.GetCultureInfo("pt-BR"));

                if (DateTimeExtension.HorarioBrasilia().Date >= dataDefinida)
                {
                    var dres = await mediator.Send(new ObterIdsDresQuery());

                    foreach (var dreId in dres)
                        await mediator.Send(new PublicarFilaSgpCommand(RotasRabbitSgpAula.RotaExecutaPendenciasAulaDre, new DreDto(dreId)));

                    return true;
                }
            }

            return false;
        }
    }
}
