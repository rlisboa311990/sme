﻿using SME.SGP.Dominio;
using SME.SGP.Dominio.Interfaces;
using SME.SGP.Infra;
using SME.SGP.Infra.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace SME.SGP.Dados.Repositorios
{
    public class RepositorioRegistroFrequenciaAlunoConsulta : IRepositorioRegistroFrequenciaAlunoConsulta
    {
        private readonly ISgpContextConsultas database;

        public RepositorioRegistroFrequenciaAlunoConsulta(ISgpContextConsultas database)
        {
            this.database = database ?? throw new ArgumentNullException(nameof(database));
        }

        public async Task<IEnumerable<FrequenciaAlunoSimplificadoDto>> ObterFrequenciasPorAulaId(long aulaId)
        {
            var query = @"SELECT rfa.codigo_aluno AS CodigoAluno,
                               rfa.numero_aula AS NumeroAula,
                               rfa.valor AS TipoFrequencia
                        FROM registro_frequencia_aluno rfa
                        WHERE NOT rfa.excluido
                          AND rfa.aula_id = @aulaId";

            return await database.Conexao.QueryAsync<FrequenciaAlunoSimplificadoDto>(query, new { aulaId });
        }

        public Task<IEnumerable<RegistroFrequenciaAluno>> ObterRegistrosAusenciaPorAula(long aulaId)
        {
            var query = @"SELECT *
                            FROM registro_frequencia_aluno rfa
                            WHERE NOT rfa.excluido
                              AND rfa.valor = @tipoFrequencia
                              AND rfa.aula_id = @aulaId";

            return database.Conexao.QueryAsync<RegistroFrequenciaAluno>(query, new { aulaId, tipoFrequencia = (int)TipoFrequencia.F });
        }

        public async Task<IEnumerable<RegistroFrequenciaGeralPorDisciplinaAlunoTurmaDataDto>> ObterFrequenciaAlunosGeralPorAnoQuery(int ano)
        {
            var query = @"           
                        select distinct   
                            a.disciplina_id as DisciplinaId,
                            a.turma_id as TurmaId,
                            p.periodo_fim as DataAula,
                            rfa.codigo_aluno as AlunoCodigo
                        from
                            registro_frequencia_aluno rfa
                            join aula a on rfa.aula_id = a.id
                            join periodo_escolar p on a.tipo_calendario_id = p.tipo_calendario_id
                        where
                            not rfa.excluido
                            and not a.excluido
                            and extract(year from p.periodo_inicio) = @ano    
                            and a.data_aula >= p.periodo_inicio
                            and a.data_aula <= p.periodo_fim                    
                            and rfa.numero_aula <= a.quantidade                            
                        order by a.disciplina_id,
		                         a.turma_id,
		                         p.periodo_fim,
		                         rfa.codigo_aluno";

            return await database.Conexao.QueryAsync<RegistroFrequenciaGeralPorDisciplinaAlunoTurmaDataDto>(query, new { ano });
        }

        public async Task<IEnumerable<RegistroFrequenciaPorDisciplinaAlunoDto>> ObterRegistroFrequenciaAlunosPorAlunosETurmaIdEDataAula(DateTime dataAula, string[] turmasId, IEnumerable<string> codigoAlunos, bool somenteAusencias = false)
        {
            var query = $@"           
                    drop table if exists tmp_lista_dados_frequencia;
                    create temporary table tmp_lista_dados_frequencia as
                    select a.id aula_id,
                           pe.id periodo_id,
                      	   pe.periodo_inicio,
                      	   pe.periodo_fim,
                      	   pe.bimestre,
                      	   rfa.codigo_aluno,
                      	   a.disciplina_id, 
                      	   rfa.valor,  	
                      	   rfa.criado_em,
                      	   row_number() over (partition by a.id, rfa.codigo_aluno, rfa.numero_aula order by rfa.id) sequencia
                      	from registro_frequencia_aluno rfa 
                      		inner join aula a 
                      			on rfa.aula_id = a.id
                      		inner join periodo_escolar pe 
                      			on a.tipo_calendario_id = pe.tipo_calendario_id
                    where not rfa.excluido and
                      	  not a.excluido and
                      	  rfa.codigo_aluno = any(@codigoAlunos) and
                    	  a.turma_id = any(@turmasId) and
                          @dataAula::date between pe.periodo_inicio and pe.periodo_fim;
                    
                    select {(somenteAusencias ? string.Empty : "count(0) filter (where tmp.valor = 1) TotalPresencas,")}
                    	   count(0) filter (where tmp.valor = 2) TotalAusencias,
                    	   {(somenteAusencias ? string.Empty : "count(0) filter (where tmp.valor = 3) TotalRemotos,")}
                    	   tmp.periodo_id as PeriodoEscolarId,
                    	   tmp.periodo_inicio as PeriodoInicio,
                    	   tmp.periodo_fim as PeriodoFim,
                    	   tmp.bimestre,
                           tmp.codigo_aluno as AlunoCodigo,
                           tmp.disciplina_id as ComponenteCurricularId
                    	from tmp_lista_dados_frequencia tmp
                    where tmp.sequencia = 1	                               
                    group by tmp.periodo_id,
                        	 tmp.periodo_inicio,
                        	 tmp.periodo_fim,
                        	 tmp.bimestre,
                        	 tmp.codigo_aluno,
                        	 tmp.disciplina_id;";

            return await database.Conexao.QueryAsync<RegistroFrequenciaPorDisciplinaAlunoDto>(query, new { dataAula, codigoAlunos, turmasId });
        }

        public Task<IEnumerable<RegistroFrequenciaAluno>> ObterRegistrosAusenciaPorAulaAsync(long aulaId)
        {
            var query = @"select * 
                          from registro_frequencia_aluno rfa
                          where not rfa.excluido and rfa.valor = @tipo
                            and rfa.aula_id = @aulaId ";

            return database.Conexao.QueryAsync<RegistroFrequenciaAluno>(query, new { aulaId, tipo = (int)TipoFrequencia.F });
        }

        public Task<IEnumerable<FrequenciaAlunoAulaDto>> ObterFrequenciasDoAlunoNaAula(string codigoAluno, long aulaId)
        {
            var query = @"select
	                        rfa.id as FrequenciaAlunoCodigo,
	                        rfa.valor TipoFrequencia,
	                        rfa.numero_aula as NumeroAula,
	                        rfa.codigo_aluno as AlunoCodigo 
                        from registro_frequencia_aluno rfa
	                        join aula a on rfa.aula_id = a.id
                        where not rfa.excluido and not a.excluido
	                        and rfa.codigo_aluno = @codigoAluno and a.id = @aulaId
	                        order by rfa.id desc";

            return database.Conexao.QueryAsync<FrequenciaAlunoAulaDto>(query, new { aulaId, codigoAluno });
        }


        public async Task<int> ObterTotalAulasPorDisciplinaETurma(DateTime dataAula, string disciplinaId, params string[] turmasId)
        {
            String query = BuildQueryObterTotalAulasPorDisciplinaETurma(disciplinaId);
            return await database.Conexao.QueryFirstOrDefaultAsync<int>(query.ToString(), new { dataAula, disciplinaId, turmasId });
        }

        private String BuildQueryObterTotalAulasPorDisciplinaETurma(string disciplinaId)
        {
            StringBuilder query = new StringBuilder();
            query.AppendLine("select COALESCE(SUM(a.quantidade),0) AS total");
            query.AppendLine("  from aula a ");
            query.AppendLine("      inner join periodo_escolar p on");
            query.AppendLine("          a.tipo_calendario_id = p.tipo_calendario_id");
            query.AppendLine("where not a.excluido");
            query.AppendLine("and p.periodo_inicio <= @dataAula");
            query.AppendLine("and p.periodo_fim >= @dataAula ");
            query.AppendLine("and a.data_aula >= p.periodo_inicio");
            query.AppendLine("and a.data_aula <= p.periodo_fim");

            if (!string.IsNullOrWhiteSpace(disciplinaId))
                query.AppendLine("and a.disciplina_id = @disciplinaId");

            query.AppendLine("and a.turma_id = any(@turmasId)");
            query.AppendLine("and exists (select 1");
            query.AppendLine("				from registro_frequencia rf");
            query.AppendLine("			  where a.id = rf.aula_id and");
            query.AppendLine("				  not a.excluido);");

            return query.ToString();
        }

        public async Task<IEnumerable<RegistroFrequenciaAlunoPorTurmaEMesDto>> ObterRegistroFrequenciaAlunosPorTurmaEMes(string turmaCodigo, int mes)
        {
            const string query = @"SELECT t.id AS TurmaId,
									       rfa.codigo_aluno AS AlunoCodigo,
									       sum(a.quantidade) AS QuantidadeAulas,
									       count(distinct(rfa.registro_frequencia_id * rfa.numero_aula)) filter (
									        WHERE rfa.valor = 2) AS QuantidadeAusencias,
									       sum(0) AS QuantidadeCompensacoes,
									       t.ano_letivo
									FROM registro_frequencia_aluno rfa
									INNER JOIN aula a ON a.id = rfa.aula_id
									AND NOT a.excluido
									INNER JOIN turma t ON t.turma_id = a.turma_id
									WHERE NOT rfa.excluido
									  AND a.turma_id = @turmaCodigo
									  AND extract(MONTH
									              FROM a.data_aula) = @mes
									GROUP BY rfa.codigo_aluno,
									         t.id,
									         t.ano_letivo
									ORDER BY t.ano_letivo,
									         rfa.codigo_aluno";

            var parametros = new { turmaCodigo, mes };

            return await database.Conexao.QueryAsync<RegistroFrequenciaAlunoPorTurmaEMesDto>(query, parametros);
        }

        public Task<IEnumerable<RegistroFrequenciaAluno>> ObterRegistrosAusenciaPorIdRegistro(long registroFrequenciaId)
        {
            var query = @"SELECT
							id,
							valor,
							codigo_aluno,
							numero_aula,
							registro_frequencia_id,
							criado_em,
							criado_por,
							criado_rf,
							alterado_em,
							alterado_por,
							alterado_rf,
							excluido,
							migrado,
							aula_id
						FROM
							registro_frequencia_aluno
                        where not excluido 
                            and registro_frequencia_id = @registroFrequenciaId";
            return database.Conexao.QueryAsync<RegistroFrequenciaAluno>(query, new { registroFrequenciaId });
        }
    }
}
