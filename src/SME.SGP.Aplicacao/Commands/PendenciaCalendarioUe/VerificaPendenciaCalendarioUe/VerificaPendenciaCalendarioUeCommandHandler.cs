﻿using MediatR;
using SME.SGP.Dominio;
using SME.SGP.Infra;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SME.SGP.Aplicacao
{
    public class VerificaPendenciaCalendarioUeCommandHandler : IRequestHandler<VerificaPendenciaCalendarioUeCommand, bool>
    {
        private readonly IMediator mediator;

        public VerificaPendenciaCalendarioUeCommandHandler(IMediator mediator)
        {
            this.mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        }

        public async Task<bool> Handle(VerificaPendenciaCalendarioUeCommand request, CancellationToken cancellationToken)
        {
            var anoAtual = DateTime.Now.Year;
            var diasLetivosEja = int.Parse(await mediator.Send(new ObterValorParametroSistemaTipoEAnoQuery(Dominio.TipoParametroSistema.EjaDiasLetivos, anoAtual)));
            var diasLetivosFundamentalMedio = int.Parse(await mediator.Send(new ObterValorParametroSistemaTipoEAnoQuery(Dominio.TipoParametroSistema.EjaDiasLetivos, anoAtual)));

            var tipoCalendarioId = await mediator.Send(new ObterIdTipoCalendarioPorAnoLetivoEModalidadeQuery(Dominio.Modalidade.Fundamental, anoAtual, 0));
            if (tipoCalendarioId > 0)
                await VerificaPendenciasNoCalendario(tipoCalendarioId, diasLetivosFundamentalMedio, Dominio.ModalidadeTipoCalendario.FundamentalMedio);

            tipoCalendarioId = await mediator.Send(new ObterIdTipoCalendarioPorAnoLetivoEModalidadeQuery(Dominio.Modalidade.EJA, anoAtual, 1));
            if (tipoCalendarioId > 0)
                await VerificaPendenciasNoCalendario(tipoCalendarioId, diasLetivosEja, Dominio.ModalidadeTipoCalendario.EJA);

            tipoCalendarioId = await mediator.Send(new ObterIdTipoCalendarioPorAnoLetivoEModalidadeQuery(Dominio.Modalidade.EJA, anoAtual, 2));
            if (tipoCalendarioId > 0)
                await VerificaPendenciasNoCalendario(tipoCalendarioId, diasLetivosEja, Dominio.ModalidadeTipoCalendario.EJA);

            return true;
        }

        private async Task VerificaPendenciasNoCalendario(long tipoCalendarioId, int diasLetivosParametro, Dominio.ModalidadeTipoCalendario modalidadeCalendario)
        {
            var periodosEscolares = await mediator.Send(new ObterPeridosEscolaresPorTipoCalendarioIdQuery(tipoCalendarioId));
            var diasLetivosENaoLetivos = await mediator.Send(new ObterDiasPorPeriodosEscolaresComEventosLetivosENaoLetivosQuery(periodosEscolares, tipoCalendarioId));

            var ues = await mediator.Send(new ObterUEsPorModalidadeCalendarioQuery(modalidadeCalendario));
            foreach(var ue in ues)
            {
                if (!(await mediator.Send(new ExistePendenciaDiasLetivosCalendarioUeQuery(tipoCalendarioId, ue.Id))))
                {
                    var diasLetivos = await ObterDiasLetivosUe(ue, diasLetivosENaoLetivos);
                    if (diasLetivos < diasLetivosParametro)
                        await GerarPendenciaCalendarioUe(ue, diasLetivos, tipoCalendarioId);
                }
            }
        }

        private async Task<int> ObterDiasLetivosUe(Ue ue, List<Infra.DiaLetivoDto> diasLetivosENaoLetivos)
        {
            return await mediator.Send(new ObterDiasLetivosDaUeQuery(diasLetivosENaoLetivos, ue.Dre.CodigoDre, ue.CodigoUe));
        }

        private async Task GerarPendenciaCalendarioUe(Ue ue, int diasLetivos, long tipoCalendarioId)
        {
            var nomeTipoCalendario = await mediator.Send(new ObterNomeTipoCalendarioPorIdQuery(tipoCalendarioId));

            var descricao = new StringBuilder();
            descricao.AppendLine($"DRE: DRE - {ue.Dre.Abreviacao}<br />");
            descricao.AppendLine($"UE: {ue.TipoEscola.ShortName()} - {ue.Nome}<br />");
            descricao.AppendLine($"Calendário: {nomeTipoCalendario}<br />");
            descricao.AppendLine($"Quantidade de dias letivos: {diasLetivos}<br />");
            
            var instrucao = "Acesse a tela de Calendário Escolar e confira os eventos da sua UE.";

            await mediator.Send(new SalvarPendenciaCalendarioUeCommand(tipoCalendarioId, ue.Id, descricao.ToString(), instrucao, TipoPendencia.CalendarioLetivoInsuficiente));
        }
    }
}
