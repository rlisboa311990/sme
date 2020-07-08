﻿using MediatR;
using SME.SGP.Dominio;
using SME.SGP.Dominio.Interfaces;
using SME.SGP.Infra;
using System;
using System.ComponentModel.DataAnnotations;
using System.Threading;
using System.Threading.Tasks;

namespace SME.SGP.Aplicacao
{
    public class EnviaNotificacaoCriadorCommandHandler : IRequestHandler<EnviaNotificacaoCriadorCommand, bool>
    {
        private readonly IServicoNotificacao servicoNotificacao;

        public EnviaNotificacaoCriadorCommandHandler(IServicoNotificacao servicoNotificacao)
        {
            
            this.servicoNotificacao = servicoNotificacao ?? throw new ArgumentNullException(nameof(servicoNotificacao));
            
        }

        public Task<bool> Handle(EnviaNotificacaoCriadorCommand request, CancellationToken cancellationToken)
        {
            var urlNotificacao = $"{request.UrlRedirecionamentoBase}api/v1/downloads/sgp/{request.RelatorioCorrelacao.Formato.Name()}/{request.RelatorioCorrelacao.Codigo}";

            var descricaoDoRelatorio = request.RelatorioCorrelacao.TipoRelatorio.GetAttribute<DisplayAttribute>().Description;

            var notificacao = new Notificacao()
            {
                Titulo = descricaoDoRelatorio,
                Ano = request.RelatorioCorrelacao.CriadoEm.Year,
                Categoria = NotificacaoCategoria.Aviso,
                Mensagem = $"O {descricaoDoRelatorio} está pronto para download. <br /> Clique <a href='{urlNotificacao}' target='_blank'>aqui</a> <br /> Observação: Este link é válido por 24 horas. ",
                Tipo = NotificacaoTipo.Relatorio,
                UsuarioId = request.RelatorioCorrelacao.UsuarioSolicitanteId
            };

            servicoNotificacao.Salvar(notificacao);

            return Task.FromResult(true);
        }
    }
}
