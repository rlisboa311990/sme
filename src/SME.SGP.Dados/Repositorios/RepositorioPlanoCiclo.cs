﻿using Dapper;
using SME.SGP.Dados.Contexto;
using SME.SGP.Dominio;
using SME.SGP.Dominio.Interfaces;
using SME.SGP.Dto;
using System.Linq;
using System.Text;

namespace SME.SGP.Dados.Repositorios
{
    public class RepositorioPlanoCiclo : RepositorioBase<PlanoCiclo>, IRepositorioPlanoCiclo
    {
        public RepositorioPlanoCiclo(ISgpContext conexao) : base(conexao)
        {
        }

        public PlanoCicloCompletoDto ObterPlanoCicloComMatrizesEObjetivos(int ano, long cicloId, long escolaId)
        {
            StringBuilder query = new StringBuilder();
            query.AppendLine("select");
            query.AppendLine("	pc.id,");
            query.AppendLine("	pc.descricao,");
            query.AppendLine("	pc.ciclo_id as CicloId,");
            query.AppendLine("	string_agg(distinct cast(msp.matriz_id as text), ',') as MatrizesSaber,");
            query.AppendLine("	string_agg(distinct cast(odp.objetivo_desenvolvimento_id as text), ',') as ObjetivosDesenvolvimento");
            query.AppendLine("from");
            query.AppendLine("	plano_ciclo pc");
            query.AppendLine("inner join matriz_saber_plano msp on");
            query.AppendLine("  pc.id = msp.plano_id");
            query.AppendLine("inner join objetivo_desenvolvimento_plano odp on");
            query.AppendLine("  odp.plano_id = pc.id");
            query.AppendLine("where");
            query.AppendLine("  pc.ciclo_id = @cicloId and pc.ano = @ano and pc.escola_id = @escolaId");
            query.AppendLine("group by");
            query.AppendLine("  pc.id");

            return database.Conexao.Query<PlanoCicloCompletoDto>(query.ToString(), new { cicloId, ano, escolaId }).SingleOrDefault();
        }

        public bool ObterPlanoCicloPorAnoCicloEEscola(int ano, long cicloId, long escolaId)
        {
            return database.Conexao.Query<bool>("select 1 from plano_ciclo where ano = @ano and ciclo_id = @cicloId and escola_id = @escolaId", new { ano, cicloId, escolaId }).SingleOrDefault();
        }
    }
}