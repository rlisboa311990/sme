﻿using MediatR;
using SME.SGP.Aplicacao.Integracoes;
using SME.SGP.Dominio;
using SME.SGP.Dominio.Enumerados;
using SME.SGP.Dominio.Interfaces;
using SME.SGP.Infra;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SME.SGP.Aplicacao
{
    public class NotificarFechamentoReaberturaDREUseCase : AbstractUseCase, INotificarFechamentoReaberturaDREUseCase
    {
        private readonly IServicoEol servicoEOL;
        private readonly IServicoUsuario servicoUsuario;
        private readonly IRepositorioFechamentoReabertura repositorioFechamentoReabertura;
        private readonly IServicoNotificacao servicoNotificacao;
        public NotificarFechamentoReaberturaDREUseCase(IMediator mediator, IServicoUsuario servicoUsuario,
                                                    IServicoEol servicoEOL, IRepositorioFechamentoReabertura repositorioFechamentoReabertura, IServicoNotificacao servicoNotificacao) : base(mediator)
        {
            this.repositorioFechamentoReabertura = repositorioFechamentoReabertura ?? throw new System.ArgumentNullException(nameof(repositorioFechamentoReabertura));
            this.servicoUsuario = servicoUsuario ?? throw new ArgumentNullException(nameof(servicoUsuario));
            this.servicoEOL = servicoEOL ?? throw new ArgumentNullException(nameof(servicoEOL));
            this.servicoNotificacao = servicoNotificacao ?? throw new ArgumentNullException(nameof(servicoNotificacao));
        }
        public async Task<bool> Executar(MensagemRabbit mensagem)
        {
            var filtro = mensagem.ObterObjetoMensagem<FiltroNotificacaoFechamentoReaberturaDREDto>();

            if (filtro == null)
            {
                await mediator.Send(new SalvarLogViaRabbitCommand($"Não foi possível gerar as notificações, pois o fechamento reabertura não possui dados", LogNivel.Informacao, LogContexto.Fechamento));
                return false;
            }

            var adminsSgpDre = await servicoEOL.ObterAdministradoresSGP(filtro.Dre);
            if (adminsSgpDre != null && adminsSgpDre.Any())
            {
                foreach (var adminSgpUe in adminsSgpDre)
                    await mediator.Send(new ExecutaNotificacaoCadastroFechamentoReaberturaCommand(filtro.FechamentoReabertura, filtro.Dre, null,adminSgpUe));
            }

            foreach (var ue in filtro.Ues)
                await mediator.Send(new PublicarFilaSgpCommand(RotasRabbitSgp.RotaNotificacaoFechamentoReaberturaUE, new FiltroNotificacaoFechamentoReaberturaUEDto(filtro.Dre, ue, filtro.FechamentoReabertura), new Guid(), null));
                    
            return true;
        }
    }
}
