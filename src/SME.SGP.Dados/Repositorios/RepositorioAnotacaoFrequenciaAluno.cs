﻿using System.Threading.Tasks;
using SME.SGP.Dominio;
using SME.SGP.Dominio.Interfaces;
using SME.SGP.Infra;

namespace SME.SGP.Dados.Repositorios
{
    public class RepositorioAnotacaoFrequenciaAluno : RepositorioBase<AnotacaoFrequenciaAluno>, IRepositorioAnotacaoFrequenciaAluno
    {
        public RepositorioAnotacaoFrequenciaAluno(ISgpContext conexao) : base(conexao)
        {
        }

        public async Task<AnotacaoFrequenciaAluno> ObterPorAlunoAula(string codigoAluno, long aulaId)
        {
            var query = "select * from anotacao_frequencia_aluno where not excluido and codigo_aluno = @codigoAluno and aula_id = @aulaId";

            return await database.Conexao.QueryFirstOrDefaultAsync<AnotacaoFrequenciaAluno>(query, new { codigoAluno, aulaId });
        }
    }
}