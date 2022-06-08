﻿using Dapper;
using SME.SGP.Dominio.Interfaces;
using SME.SGP.Infra;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace SME.SGP.Dados.Repositorios
{
    public class RepositorioFrequenciaDiariaAluno : IRepositorioFrequenciaDiariaAluno
    {
        private readonly ISgpContext database;
        public RepositorioFrequenciaDiariaAluno(ISgpContext database)
        {
            this.database = database ?? throw new ArgumentNullException(nameof(database));
        }

        public async Task<PaginacaoResultadoDto<QuantidadeAulasDiasPorBimestreAlunoCodigoTurmaDisciplinaDto>> ObterQuantidadeAulasDiasTipoFrequenciaPorBimestreAlunoCodigoTurmaDisciplina(Paginacao paginacao, int bimestre, string codigoAluno, long turmaId, string aulaDisciplinaId)
        {
            var query = new StringBuilder(@"
                       SELECT DISTINCT 
	                        rfa.id AS RegistroFrequenciaAlunoId,
	                        a.data_aula AS DataAula,
	                        a.id AS AulasId,
	                        rfa.valor AS TipoFrequencia,
	                        rfa.codigo_aluno AS AlunoCodigo,
	                        an.id AS AnotacaoId,
	                        CASE
		                        WHEN ma.descricao IS NOT NULL THEN ma.descricao
		                        ELSE an.anotacao
	                        END AS MotivoAusencia
                        FROM registro_frequencia_aluno rfa 
                        INNER JOIN registro_frequencia rf ON rfa.registro_frequencia_id = rf.id
                        INNER JOIN aula a ON rf.aula_id = a.id
                        INNER JOIN turma t ON t.turma_id = a.turma_id
                        INNER JOIN periodo_escolar pe ON a.tipo_calendario_id = pe.tipo_calendario_id AND a.data_aula BETWEEN pe.periodo_inicio AND pe.periodo_fim AND pe.bimestre = @bimestre
                        LEFT JOIN anotacao_frequencia_aluno an ON a.id = an.aula_id AND an.codigo_aluno  = rfa.codigo_aluno
                        LEFT JOIN motivo_ausencia ma ON an.motivo_ausencia_id = ma.id
                        WHERE NOT rfa.excluido AND NOT rf.excluido AND NOT a.excluido
	                        AND rfa.codigo_aluno = @codigoAluno
	                        AND t.id = @turmaId AND a.disciplina_id = @aulaDisciplinaId 
	                        ORDER BY a.data_aula  ");

            if (paginacao.QuantidadeRegistros > 0)
                query.AppendLine($"OFFSET {paginacao.QuantidadeRegistrosIgnorados} ROWS FETCH NEXT {paginacao.QuantidadeRegistros} ROWS ONLY ");

            var parametros = new
            {
                bimestre,
                codigoAluno,
                turmaId,
                aulaDisciplinaId
            };
            var retorno = new PaginacaoResultadoDto<QuantidadeAulasDiasPorBimestreAlunoCodigoTurmaDisciplinaDto>();
            using (var multi = await database.Conexao.QueryMultipleAsync(query.ToString(), parametros))
            {
                retorno.Items = multi.Read<QuantidadeAulasDiasPorBimestreAlunoCodigoTurmaDisciplinaDto>();
                retorno.TotalRegistros = retorno.Items.AsList().Count;
            }

            retorno.TotalPaginas = (int)Math.Ceiling((double)retorno.TotalRegistros / paginacao.QuantidadeRegistros);

            return retorno;
        }
    }
}
