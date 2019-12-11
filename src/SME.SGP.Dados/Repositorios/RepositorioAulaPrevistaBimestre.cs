﻿using Dapper;
using SME.SGP.Dominio;
using SME.SGP.Dominio.Interfaces;
using SME.SGP.Infra;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SME.SGP.Dados.Repositorios
{
    public class RepositorioAulaPrevistaBimestre : RepositorioBase<AulaPrevistaBimestre>, IRepositorioAulaPrevistaBimestre
    {
        public RepositorioAulaPrevistaBimestre(ISgpContext conexao) : base(conexao)
        {
        }

        public async Task<IEnumerable<AulaPrevistaBimestreQuantidade>> ObterBimestresAulasPrevistasPorId(long aulaPrevistaId)
        {
            try
            {
                var query = @"select ap.id, ap.criado_em as CriadoEm, ap.criado_por as CriadoPor, ap.alterado_em as AlteradoEm, ap.alterado_por as AlteradoPor,
                        p.bimestre, p.periodo_inicio as inicio, p.periodo_fim as fim, ap.Id as PD, apb.aulas_previstas as Previstas,
                         COUNT(a.id) filter (where a.tipo_aula = 1 and a.aula_cj = false) as CriadasTitular,
                         COUNT(a.id) filter (where a.tipo_aula = 1 and a.aula_cj = true) as CriadasCJ,
                         COUNT(a.id) filter (where a.tipo_aula = 1 and rf.id is not null) as Cumpridas,
                         COUNT(a.id) filter (where a.tipo_aula = 2 and rf.id is not null) as Reposicoes                         
                         from periodo_escolar p
                         left join aula_prevista ap on ap.tipo_calendario_id = p.tipo_calendario_id 
                         left join aula_prevista_bimestre apb on ap.id = apb.aula_prevista_id and p.bimestre = apb.bimestre
                         left join aula a on a.turma_id = ap.turma_id and
                         				a.disciplina_id = ap.disciplina_id and
			                            a.tipo_calendario_id = p.tipo_calendario_id and 
				                        a.data_aula BETWEEN p.periodo_inicio AND p.periodo_fim
                         left join registro_frequencia rf on a.id = rf.aula_id
                         where ap.id = @aulaPrevistaId
                         group by p.bimestre, p.periodo_inicio, p.periodo_fim, apb.aulas_previstas, ap.Id
                         order by p.periodo_inicio;";

                return (await database.Conexao.QueryAsync<AulaPrevistaBimestreQuantidade>(query, new { aulaPrevistaId }));
            }
            catch (Exception e)
            {
                throw e;
            }
           
        }
    }
}
