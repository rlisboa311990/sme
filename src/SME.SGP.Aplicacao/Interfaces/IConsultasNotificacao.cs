﻿using SME.SGP.Dto;
using System.Collections.Generic;

namespace SME.SGP.Aplicacao
{
    public interface IConsultasNotificacao
    {
        IEnumerable<NotificacaoBasicaDto> Listar(NotificacaoFiltroDto filtroNotificacaoDto);

        NotificacaoDetalheDto Obter(long notificacaoId);

        IEnumerable<EnumeradoRetornoDto> ObterCategorias();

        IEnumerable<EnumeradoRetornoDto> ObterStatus();

        IEnumerable<EnumeradoRetornoDto> ObterTipos();
    }
}