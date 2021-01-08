﻿using SME.SGP.Dominio;
using SME.SGP.Dominio.Interfaces;
using SME.SGP.Infra;

namespace SME.SGP.Dados.Repositorios
{
    public class RepositorioEncaminhamentoAEESecao : RepositorioBase<EncaminhamentoAEESecao>, IRepositorioEncaminhamentoAEESecao
    {
        public RepositorioEncaminhamentoAEESecao(ISgpContext database) : base(database)
        {
        }
    }
}
