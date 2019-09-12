﻿using SME.SGP.Dominio;
using SME.SGP.Dominio.Interfaces;
using SME.SGP.Dto;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace SME.SGP.Aplicacao
{
    public class ConsultasNotificacao : IConsultasNotificacao
    {
        private readonly IRepositorioNotificacao repositorioNotificacao;
        private readonly IRepositorioUsuario repositorioUsuario;

        public ConsultasNotificacao(IRepositorioNotificacao repositorioNotificacao, IRepositorioUsuario repositorioUsuario)
        {
            this.repositorioNotificacao = repositorioNotificacao ?? throw new System.ArgumentNullException(nameof(repositorioNotificacao));
            this.repositorioUsuario = repositorioUsuario ?? throw new System.ArgumentNullException(nameof(repositorioUsuario));
        }

        public IEnumerable<NotificacaoBasicaDto> Listar(NotificacaoFiltroDto filtroNotificacaoDto)
        {
            var retorno = repositorioNotificacao.ObterPorDreOuEscolaOuStatusOuTurmoOuUsuarioOuTipo(filtroNotificacaoDto.DreId,
                filtroNotificacaoDto.EscolaId, (int)filtroNotificacaoDto.Status, filtroNotificacaoDto.TurmaId, filtroNotificacaoDto.UsuarioId,
                (int)filtroNotificacaoDto.Tipo, (int)filtroNotificacaoDto.Categoria);

            return from r in retorno
                   select new NotificacaoBasicaDto()
                   {
                       Id = r.Id,
                       Titulo = r.Titulo,
                       Data = r.CriadoEm.ToString(),
                       Status = r.Status.ToString(),
                       Tipo = r.Tipo.GetAttribute<DisplayAttribute>().Name
                   };
        }

        public NotificacaoDetalheDto Obter(long notificacaoId)
        {
            var notificacao = repositorioNotificacao.ObterPorId(notificacaoId);

            if (notificacao == null)
                throw new NegocioException($"Notificação de Id: '{notificacaoId}' não localizada.");

            if (notificacao.Status != NotificacaoStatus.Lida && notificacao.MarcarComoLidaAoObterDetalhe())
                repositorioNotificacao.Salvar(notificacao);

           var retorno =  MapearEntidadeParaDetalheDto(notificacao);
           if (notificacao.UsuarioId.HasValue)
            {
                notificacao.Usuario = repositorioUsuario.ObterPorId(notificacao.UsuarioId.Value);
                retorno.UsuarioRf = notificacao.Usuario.CodigoRf;
            }                

            return retorno;            
        }

        private static NotificacaoDetalheDto MapearEntidadeParaDetalheDto(Dominio.Notificacao retorno)
        {
            return new NotificacaoDetalheDto()
            {
                AlteradoEm = retorno.AlteradoEm.ToString(),
                AlteradoPor = retorno.AlteradoPor,
                CriadoEm = retorno.CriadoEm.ToString(),
                CriadoPor = retorno.CriadoPor,
                Id = retorno.Id,
                Mensagem = retorno.Mensagem,
                Situacao = retorno.Status.ToString(),
                Tipo = retorno.Tipo.GetAttribute<DisplayAttribute>().Name,
                Titulo = retorno.Titulo,
                MostrarBotaoRemover = retorno.PodeRemover,
                MostrarBotoesDeAprovacao = retorno.DeveAprovar,
                MostrarBotaoMarcarComoLido = retorno.DeveMarcarComoLido
            };
        }
    }
}