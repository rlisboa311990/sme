﻿using System.Collections.Generic;
using System.Threading.Tasks;
using Dapper;
using SME.SGP.Dominio;
using SME.SGP.Dominio.Interfaces;
using SME.SGP.Infra;

namespace SME.SGP.Dados.Repositorios
{
    public class RepositorioAcompanhamentoAlunoSemestre : RepositorioBase<AcompanhamentoAlunoSemestre>, IRepositorioAcompanhamentoAlunoSemestre
    {
        public RepositorioAcompanhamentoAlunoSemestre(ISgpContext conexao) : base(conexao)
        {
        }

        public async Task AtualizarLinkImagem(long id, string linkAnterior, string linkAtual)
        {
            await database.Conexao.ExecuteScalarAsync("update ACOMPANHAMENTO_ALUNO_SEMESTRE set PERCURSO_INDIVIDUAL = REPLACE(PERCURSO_INDIVIDUAL, @linkAnterior, @linkAtual) where id = @id"
                , new { id, linkAnterior, linkAtual });
        }

        public async Task<int> ObterAnoPorId(long acompanhamentoAlunoSemestreId)
        {
            var query = @"select ano_letivo
                          from acompanhamento_aluno_semestre aas 
                         inner join acompanhamento_aluno aa on aa.id = aas.acompanhamento_aluno_id 
                         inner join turma t on t.id = aa.turma_id 
                         where aas.id = @acompanhamentoAlunoSemestreId";

            return await database.Conexao.QueryFirstOrDefaultAsync<int>(query, new { acompanhamentoAlunoSemestreId });
        }

        public async Task<IEnumerable<AjusteRotaImagensAcompanhamentoAlunoDto>> ObterImagensParaAjusteRota()
        {
            var query = @"select id
                            , NomeCompleto
                            , NomeCompleto
                            , NomeCompletoAlternativo
                            , NomeCompletoAlternativo2
                            , NomeCompletoAlternativo3 
                         from tmp_acompanhamento_aluno";
            return await database.Conexao.QueryAsync<AjusteRotaImagensAcompanhamentoAlunoDto>(query);
        }
    }
}
