﻿using MediatR;
using SME.SGP.Dominio;
using SME.SGP.Infra;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SME.SGP.Aplicacao
{
    public class ConsolidarRegistrosPedagogicosUseCase : AbstractUseCase, IConsolidarRegistrosPedagogicosUseCase
    {
        public ConsolidarRegistrosPedagogicosUseCase(IMediator mediator) : base(mediator)
        {
        }
        public async Task<bool> Executar(MensagemRabbit mensagem)
        {
            var anosComParametroAtivo = await VerificaAnosAtivosRegistrosPedagogicos();
            if (anosComParametroAtivo.Count > 0)
            {
                var anosParaConsolidar = await VerificaAnosAnterioresComConsolidacao(anosComParametroAtivo);
                if (anosParaConsolidar.Count > 0)
                {
                    await ConsolidarRegistrosPedagogicos(anosParaConsolidar);
                    return true;
                }
                return false;
            }
            return false;
        }

        private async Task<List<int>> VerificaAnosAtivosRegistrosPedagogicos()
        {
            var listaAnosAtivos = new List<int>();
            for (var ano = 2014; ano <= DateTime.Now.Year; ano++)
            {
                var parametroExecucao = await BuscarDadosParametroConsolidacao(ano);
                if (parametroExecucao != null)
                    if (parametroExecucao.Ativo) { listaAnosAtivos.Add(ano); };
            }
            return listaAnosAtivos;
        }

        private async Task<List<int>> VerificaAnosAnterioresComConsolidacao(List<int> anosAtivos)
        {
            var anosAtivosRemover = new List<int>();

            foreach (var ano in anosAtivos.Where(ano => ano < DateTimeExtension.HorarioBrasilia().Year))
            {
                var existeConsolidacao = await mediator.Send(new ExisteConsolidacaoRegistroPedagogicoPorAnoQuery(ano));
                    
                if (existeConsolidacao)
                    anosAtivosRemover.Add(ano);
            }

            foreach (var anoRemover in anosAtivosRemover)
                anosAtivos.Remove(anoRemover);

            return anosAtivos;
        }

        private async Task ConsolidarRegistrosPedagogicos(List<int> anosParaConsolidar)
        {
            foreach (var ano in anosParaConsolidar)
            {
                var dados = await BuscarDadosParametroConsolidacao(ano);

                var ues = await mediator.Send(new ObterTodasUesIdsQuery());
                foreach (var ue in ues)
                    await mediator.Send(new PublicarFilaSgpCommand(RotasRabbitSgp.ConsolidarRegistrosPedagogicosPorUeTratar, new FiltroConsolidacaoRegistrosPedagogicosPorUeDto(ue, ano), new System.Guid(), null));

                await AtualizarDataExecucao(dados);
            }
        }

        private async Task AtualizarDataExecucao(ParametrosSistema parametroConsolidacao)
        {
            parametroConsolidacao.Valor = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss}";
            await mediator.Send(new AtualizarParametroSistemaCommand(parametroConsolidacao));
        }
        private async Task<ParametrosSistema> BuscarDadosParametroConsolidacao(int anoLetivo)
        {
            return await mediator.Send(new ObterParametroSistemaPorTipoEAnoQuery(TipoParametroSistema.ExecucaoConsolidacaoRegistrosPedagogicos, anoLetivo));
        }
    }
}
