﻿using SME.SGP.Dominio;
using SME.SGP.Dominio.Interfaces;
using SME.SGP.Infra;

namespace SME.SGP.Aplicacao.Consultas
{
    public class ConsultasEvento : IConsultasEvento
    {
        private readonly IRepositorioEvento repositorioEvento;

        public ConsultasEvento(IRepositorioEvento repositorioEvento)
        {
            this.repositorioEvento = repositorioEvento ?? throw new System.ArgumentNullException(nameof(repositorioEvento));
        }

        public EventoDto ObterPorId(long id)
        {
            return MapearParaDto(repositorioEvento.ObterPorId(id));
        }

        private EventoDto MapearParaDto(Evento evento)
        {
            return evento == null ? null : new EventoDto
            {
                DataFim = evento.DataFim,
                DataInicio = evento.DataInicio,
                Descricao = evento.Descricao,
                DreId = evento.DreId,
                FeriadoId = evento.FeriadoId,
                Id = evento.Id,
                Letivo = evento.Letivo,
                Nome = evento.Nome,
                TipoCalendarioId = evento.TipoCalendarioId,
                TipoEventoId = evento.TipoEventoId,
                UeId = evento.UeId
            };
        }
    }
}