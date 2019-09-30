﻿using SME.SGP.Dominio;
using SME.SGP.Dto;
using System;
using System.Collections.Generic;

namespace SME.SGP.Aplicacao.Integracoes
{
    public interface IServicoPerfil
    {
        PerfisPorPrioridadeDto DefinirPerfilPrioritario(IEnumerable<Guid> perfis, Usuario usuario);
    }
}