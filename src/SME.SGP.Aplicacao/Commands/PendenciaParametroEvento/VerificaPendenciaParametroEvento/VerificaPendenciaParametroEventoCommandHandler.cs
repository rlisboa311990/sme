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
    public class VerificaPendenciaParametroEventoCommandHandler : IRequestHandler<VerificaPendenciaParametroEventoCommand, bool>
    {
        private readonly IMediator mediator;

        public VerificaPendenciaParametroEventoCommandHandler(IMediator mediator)
        {
            this.mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        }

        public async Task<bool> Handle(VerificaPendenciaParametroEventoCommand request, CancellationToken cancellationToken)
        {
            var anoAtual = DateTime.Now.Year;

            var dataInicioGeracaoPendencia = DateTime.Parse(await mediator.Send(new ObterValorParametroSistemaTipoEAnoQuery(Dominio.TipoParametroSistema.DataInicioGeracaoPendencias, anoAtual)));
            if (DateTime.Now >= dataInicioGeracaoPendencia)
            {
                var tipoCalendarioId = await mediator.Send(new ObterIdTipoCalendarioPorAnoLetivoEModalidadeQuery(Dominio.Modalidade.Fundamental, anoAtual, 0));
                if (tipoCalendarioId > 0)
                    await VerificaPendenciaEventosCalendario(tipoCalendarioId, anoAtual);
            }

            return true;
        }

        private async Task VerificaPendenciaEventosCalendario(long tipoCalendarioId, int anoAtual)
        {
            var ues = await mediator.Send(new ObterUEsPorModalidadeCalendarioQuery(Dominio.ModalidadeTipoCalendario.FundamentalMedio));
            foreach(var ue in ues)
            {
                var pendenciaCalendarioUe = await mediator.Send(new ObterPendenciaCalendarioUeQuery(tipoCalendarioId, ue.Id, Dominio.TipoPendencia.CadastroEventoPendente));
                var pendenciasParametroEventoUe = await ObterPendenciasParametroEventoPorPendenciaId(pendenciaCalendarioUe?.PendenciaId);

                var listaValidacoesEvento = new List<(bool gerarPedencia, long parametroSistemaId, int quantidadeEventos)>();
                listaValidacoesEvento.Add(await ValidarQuantidadeEventosPorTipo(tipoCalendarioId, ue, anoAtual, pendenciasParametroEventoUe, TipoEvento.ConselhoDeClasse));
                listaValidacoesEvento.Add(await ValidarQuantidadeEventosPorTipo(tipoCalendarioId, ue, anoAtual, pendenciasParametroEventoUe, TipoEvento.ReuniaoAPM));
                listaValidacoesEvento.Add(await ValidarQuantidadeEventosPorTipo(tipoCalendarioId, ue, anoAtual, pendenciasParametroEventoUe, TipoEvento.ReuniaoConselhoEscola));
                listaValidacoesEvento.Add(await ValidarQuantidadeEventosPorTipo(tipoCalendarioId, ue, anoAtual, pendenciasParametroEventoUe, TipoEvento.ReuniaoPedagogica));

                if (listaValidacoesEvento.Any(a => a.gerarPedencia))
                {
                    var pendenciaCalendarioUeId = pendenciaCalendarioUe == null ? await GerarPendenciaCalendarioUe(tipoCalendarioId, ue) : pendenciaCalendarioUe.Id;

                    await GerarPendenciaParametroEvento(pendenciaCalendarioUeId, listaValidacoesEvento.Where(a => a.gerarPedencia));

                }

            }
        }

        private async Task<long> GerarPendenciaCalendarioUe(long tipoCalendarioId, Ue ue)
        {
            var nomeTipoCalendario = await mediator.Send(new ObterNomeTipoCalendarioPorIdQuery(tipoCalendarioId));
            var descricao = new StringBuilder();

            descricao.AppendLine($"DRE: DRE - {ue.Dre.Abreviacao}<br />");
            descricao.AppendLine($"UE: {ue.TipoEscola.ShortName()} - {ue.Nome}<br />");
            descricao.AppendLine($"Calendário: {nomeTipoCalendario}<br />");
            descricao.AppendLine($"Eventos pendentes de cadastro:<br />");

            var instrucao = "Acesse a tela de Calendário Escolar e confira os eventos da sua UE.";

            return await mediator.Send(new SalvarPendenciaCalendarioUeCommand(tipoCalendarioId, ue.Id, descricao.ToString(), instrucao, TipoPendencia.CadastroEventoPendente));
        }

        private async Task<IEnumerable<PendenciaParametroEventoDto>> ObterPendenciasParametroEventoPorPendenciaId(long? pendenciaId)
        {
            var pendenciasParametrosEvento = Enumerable.Empty<PendenciaParametroEventoDto>();
            if (pendenciaId.HasValue && pendenciaId > 0)
                pendenciasParametrosEvento = await mediator.Send(new ObterPendenciasParametroEventoPorPendenciaQuery(pendenciaId.Value));

            return pendenciasParametrosEvento;
        }

        private async Task<(bool gerarPedencia, long parametroSistemaId, int quantidadeEventos)> ValidarQuantidadeEventosPorTipo(long tipoCalendarioId, Ue ue, int anoAtual, IEnumerable<PendenciaParametroEventoDto> pendenciasParametroEventoUe, TipoEvento tipoEvento)
        {
            var parametroQuantidadeEventos = await mediator.Send(new ObterParametroSistemaPorTipoEAnoQuery(ObterTipoParametroPorTipoEvento(tipoEvento), anoAtual));
            var eventos = await mediator.Send(new ObterEventosPorTipoECalendarioUeQuery(tipoCalendarioId, ue.CodigoUe, tipoEvento));

            if (EventosInsuficientes(eventos, int.Parse(parametroQuantidadeEventos.Valor)))
            {
                var pendenciaParametroEvento = pendenciasParametroEventoUe.FirstOrDefault(c => c.ParametroSistemaId == parametroQuantidadeEventos.Id);
                return (pendenciaParametroEvento == null, parametroQuantidadeEventos.Id, eventos.Count());
            }

            return (false, 0, 0);
        }

        private bool EventosInsuficientes(IEnumerable<Evento> eventos, int quantidadeEventosParametro)
        {
            return eventos == null
                || eventos.Count() < quantidadeEventosParametro;
        }

        private TipoParametroSistema ObterTipoParametroPorTipoEvento(TipoEvento tipoEvento)
        {
            switch (tipoEvento)
            {
                case TipoEvento.ConselhoDeClasse:
                    return TipoParametroSistema.QuantidadeEventosConselhoClasse;
                case TipoEvento.ReuniaoAPM:
                    return TipoParametroSistema.QuantidadeEventosAPM;
                case TipoEvento.ReuniaoConselhoEscola:
                    return TipoParametroSistema.QuantidadeEventosConselhoEscolar;
                case TipoEvento.ReuniaoPedagogica:
                    return TipoParametroSistema.QuantidadeEventosPedagogicos;
                default:
                    throw new NegocioException("Tipo de evento não relacionado com tipo de parâmetro do sistema!");
            }
        }

        private async Task GerarPendenciaParametroEvento(long pendenciaCalendarioUeId, IEnumerable<(bool gerarPedencia, long parametroSistemaId, int quantidadeEventos)> pendenciasEventos)
        {
            foreach (var pendenciaEvento in pendenciasEventos)
                await mediator.Send(new SalvarPendenciaParametroEventoCommand(pendenciaCalendarioUeId, pendenciaEvento.parametroSistemaId, pendenciaEvento.quantidadeEventos));;
        }
    }
}
