﻿using SME.SGP.Dominio;
using SME.SGP.Dominio.Interfaces;
using SME.SGP.Infra;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace SME.SGP.Dados.Repositorios
{
    public class RepositorioPrioridadePerfil : RepositorioBase<PrioridadePerfil>, IRepositorioPrioridadePerfil
    {
        public RepositorioPrioridadePerfil(ISgpContext conexao) : base(conexao)
        {
        }

        public IEnumerable<PrioridadePerfil> ObterPerfisPorIds(IEnumerable<Guid> idsPerfis)
        {
            var query = new StringBuilder();
            query.AppendLine("select * from prioridade_perfil where codigo_perfil = Any(@Ids) order by ordem");

            return database.Conexao.Query<PrioridadePerfil>(query.ToString(), new { Ids = idsPerfis });
        }

        public async Task<IEnumerable<PrioridadePerfil>> ObterPerfisPorTipo(int tipo)
        {
            var query = "select * from prioridade_perfil where tipo >= @tipo";

            return await database.Conexao.QueryAsync<PrioridadePerfil>(query, new { tipo });
        }

        public async Task<int> ObterTipoPerfil(Guid perfil)
        {
            var query = @"select tipo from prioridade_perfil where codigo_perfil = @perfil";

            return await database.Conexao.QueryFirstOrDefaultAsync<int>(query, new { perfil });
        }
    }
}