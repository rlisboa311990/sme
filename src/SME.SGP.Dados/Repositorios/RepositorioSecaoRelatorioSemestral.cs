﻿using SME.SGP.Dominio;
using SME.SGP.Dominio.Interfaces;
using SME.SGP.Infra;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace SME.SGP.Dados.Repositorios
{
    public class RepositorioSecaoRelatorioSemestral : IRepositorioSecaoRelatorioSemestral
    {
        private readonly ISgpContext database;

        public RepositorioSecaoRelatorioSemestral(ISgpContext database)
        {
            this.database = database ?? throw new ArgumentNullException(nameof(database));
        }

        public Task<IEnumerable<SecaoRelatorioSemestral>> ObterSecoesVigentes(DateTime dataReferencia)
        {
            throw new NotImplementedException();
        }
    }
}
