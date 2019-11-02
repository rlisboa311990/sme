﻿using SME.SGP.Aplicacao.Interfaces;
using SME.SGP.Dominio;
using SME.SGP.Dominio.Interfaces;
using SME.SGP.Infra;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SME.SGP.Aplicacao
{
    public class ComandosEvento : IComandosEvento
    {
        private readonly IRepositorioEvento repositorioEvento;
        private readonly IServicoEvento servicoEvento;

        public ComandosEvento(IRepositorioEvento repositorioEvento,
                              IServicoEvento servicoEvento)
        {
            this.repositorioEvento = repositorioEvento ?? throw new System.ArgumentNullException(nameof(repositorioEvento));
            this.servicoEvento = servicoEvento ?? throw new System.ArgumentNullException(nameof(servicoEvento));
        }

        public async Task<IEnumerable<RetornoCopiarEventoDto>> Alterar(long id, EventoDto eventoDto)
        {
            var evento = repositorioEvento.ObterPorId(id);

            evento = MapearParaEntidade(evento, eventoDto);
            await servicoEvento.Salvar(evento);
            return await CopiarEventos(eventoDto);
        }

        public async Task<IEnumerable<RetornoCopiarEventoDto>> Criar(EventoDto eventoDto)
        {
            var evento = MapearParaEntidade(new Evento(), eventoDto);
            await servicoEvento.Salvar(evento);
            return await CopiarEventos(eventoDto);
        }

        public void Excluir(long[] idsEventos)
        {
            List<long> idsComErroAoExcluir = new List<long>();

            foreach (var idEvento in idsEventos)
            {
                try
                {
                    var evento = repositorioEvento.ObterPorId(idEvento);

                    evento.Excluir();

                    repositorioEvento.Salvar(evento);
                }
                catch (Exception)
                {
                    idsComErroAoExcluir.Add(idEvento);
                }
            }

            if (idsComErroAoExcluir.Any())
                throw new NegocioException($"Não foi possível excluir os eventos de ids {string.Join(",", idsComErroAoExcluir)}");
        }

        private async Task<IEnumerable<RetornoCopiarEventoDto>> CopiarEventos(EventoDto eventoDto)
        {
            var mensagens = new List<RetornoCopiarEventoDto>();
            if (eventoDto.TiposCalendarioParaCopiar != null && eventoDto.TiposCalendarioParaCopiar.Any())
            {
                foreach (var tipoCalendario in eventoDto.TiposCalendarioParaCopiar)
                {
                    eventoDto.TipoCalendarioId = tipoCalendario.TipoCalendarioId;
                    try
                    {
                        var eventoParaCopiar = MapearParaEntidade(new Evento(), eventoDto);
                        await servicoEvento.Salvar(eventoParaCopiar);

                        mensagens.Add(new RetornoCopiarEventoDto($"Evento copiado para o calendário: '{tipoCalendario.NomeCalendario}'.", true));
                    }
                    catch (NegocioException nex)
                    {
                        mensagens.Add(new RetornoCopiarEventoDto($"Erro ao copiar para o calendário: '{tipoCalendario.NomeCalendario}'. Erro: {nex.Message}"));
                    }
                }
            }
            return mensagens;
        }

        private Evento MapearParaEntidade(Evento evento, EventoDto eventoDto)
        {
            evento.DataFim = eventoDto.DataFim;
            evento.DataInicio = eventoDto.DataInicio.Value;
            evento.Descricao = eventoDto.Descricao;
            evento.DreId = eventoDto.DreId;
            evento.FeriadoId = eventoDto.FeriadoId;
            evento.Letivo = eventoDto.Letivo;
            evento.Nome = eventoDto.Nome;
            evento.TipoCalendarioId = eventoDto.TipoCalendarioId;
            evento.TipoEventoId = eventoDto.TipoEventoId;
            evento.UeId = eventoDto.UeId;
            return evento;
        }
    }
}