﻿using SME.SGP.Infra;
using System.Threading.Tasks;

namespace SME.SGP.Aplicacao.Interfaces
{
    public interface IComandosEvento
    {
        void Excluir(long[] idsEventos);

        Task Alterar(long id, EventoDto eventoDto);

        Task Criar(EventoDto eventoDto);
    }
}