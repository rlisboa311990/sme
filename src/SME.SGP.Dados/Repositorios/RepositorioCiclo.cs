﻿using Dapper;
using SME.SGP.Dados.Contexto;
using SME.SGP.Dominio;
using SME.SGP.Dominio.Interfaces;
using SME.SGP.Dto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SME.SGP.Dados.Repositorios
{
    public class RepositorioCiclo : RepositorioBase<Ciclo>, IRepositorioCiclo
    {
        public RepositorioCiclo(ISgpContext conexao) : base(conexao)
        {
        }

        public CicloDto ObterCicloPorAno(int ano)
        {
            StringBuilder query = new StringBuilder();
            query.AppendLine("select");
            query.AppendLine("	tc.id,");
            query.AppendLine("	tc.descricao");
            query.AppendLine("from");
            query.AppendLine("	tipo_ciclo tc");
            query.AppendLine("inner join tipo_ciclo_ano tca on");
            query.AppendLine("  tc.id = tca.tipo_ciclo_id");
            query.AppendLine("where");
            query.AppendLine("  tca.ano = @ano");
            return database.Conexao.Query<CicloDto>(query.ToString(), new { ano }).SingleOrDefault();
        }

        public IEnumerable<CicloDto> ObterCiclosPorTurma(IEnumerable<string> Ano, string AnoSelecionado, int Modalidade)
        {
            var anos = "'" + string.Join("','", Ano) + "'";

            StringBuilder query = new StringBuilder();
            query.AppendLine("SELECT");
            query.AppendLine("	tc.id,");
            query.AppendLine("	tc.descricao,");
            query.AppendLine("	1 AS Selecionado");
            query.AppendLine("from ");
            query.AppendLine("	tipo_ciclo tc inner join tipo_ciclo_ano tca on tc.id = tca.tipo_ciclo_id ");
            query.AppendLine("WHERE ");
            query.AppendLine($"  tca.Ano = '{AnoSelecionado}'");
            query.AppendLine($"  AND etapa_id = {Modalidade}");
            query.AppendLine(" UNION ");
            query.AppendLine("SELECT");
            query.AppendLine("	tc.id,");
            query.AppendLine("	tc.descricao,");
            query.AppendLine("	0 AS Selecionado");
            query.AppendLine("from ");
            query.AppendLine("	tipo_ciclo tc inner join tipo_ciclo_ano tca on tc.id = tca.tipo_ciclo_id ");
            query.AppendLine("WHERE ");
            query.AppendLine($"  tca.Ano IN ({anos})");
            query.AppendLine($"  AND etapa_id = {Modalidade} ");
            query.AppendLine($"ORDER BY Selecionado DESC");
            return database.Conexao.Query<CicloDto>(query.ToString()).ToList();
        }

        private object IEnumerable<T>(string v, object p)
        {
            throw new NotImplementedException();
        }
    }
}