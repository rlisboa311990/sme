﻿using SME.SGP.Infra;
using System.Collections.Generic;

namespace SME.SGP.Aplicacao
{
    public interface IConsultasTipoCalendario
    {
        TipoCalendarioCompletoDto BuscarPorId(long id);

        IEnumerable<TipoCalendarioDto> Listar();
    }
}