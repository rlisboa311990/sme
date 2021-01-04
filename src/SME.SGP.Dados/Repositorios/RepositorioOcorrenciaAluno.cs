﻿using SME.SGP.Dados.Repositorios;
using SME.SGP.Dominio;
using SME.SGP.Infra;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SME.SGP.Dados
{
    public class RepositorioOcorrenciaAluno : IRepositorioOcorrenciaAluno
    {
        private readonly ISgpContext database;

        public RepositorioOcorrenciaAluno(ISgpContext database)
        {
            this.database = database;
        }

        public async Task<long> SalvarAsync(OcorrenciaAluno entidade)
        {
            if (entidade.Id > 0)
            {
                await database.Conexao.UpdateAsync(entidade);
                await AuditarAsync(entidade.Id, "A");
            }
            else
            {
                entidade.Id = (long)(await database.Conexao.InsertAsync(entidade));
                await AuditarAsync(entidade.Id, "I");
            }

            return entidade.Id;
        }

        protected async Task AuditarAsync(long identificador, string acao)
        {
            await database.Conexao.InsertAsync(new Auditoria()
            {
                Data = DateTime.Now,
                Entidade = nameof(OcorrenciaAluno).ToLower(),
                Chave = identificador,
                Usuario = database.UsuarioLogadoNomeCompleto,
                RF = database.UsuarioLogadoRF,
                Acao = acao
            });
        }
    public class RepositorioOcorrenciaAluno : RepositorioBase<OcorrenciaAluno>, IRepositorioOcorrenciaAluno
    {
        public RepositorioOcorrenciaAluno(ISgpContext conexao) : base(conexao) { }

        public async Task<IEnumerable<string>> ObterAlunosPorOcorrencia(long ocorrenciaId)
        {
            string query = @"select
							oa.codigo_aluno
						from
							ocorrencia o
						inner join ocorrencia_tipo ot on ot.id = o.ocorrencia_tipo_id 
						inner join ocorrencia_aluno oa on oa.ocorrencia_id = o.id
						where not excluido and o.id = @ocorrenciaId ";

            return await database.Conexao.QueryAsync<string>(query.ToString(), new { ocorrenciaId });
        }
    }
}
