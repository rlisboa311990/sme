﻿using Dapper;
using SME.SGP.Dominio;
using SME.SGP.Dominio.Enumerados;
using SME.SGP.Dominio.Interfaces;
using SME.SGP.Infra;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SME.SGP.Dados.Repositorios
{
    public class RepositorioEncaminhamentoAEE : RepositorioBase<EncaminhamentoAEE>, IRepositorioEncaminhamentoAEE
    {
        public RepositorioEncaminhamentoAEE(ISgpContext database) : base(database)
        {
        }

        public async Task<PaginacaoResultadoDto<EncaminhamentoAEEAlunoTurmaDto>> ListarPaginado(long dreId, long ueId, long turmaId, string alunoCodigo, int? situacao, Paginacao paginacao)
        {
            var query = MontaQueryCompleta(paginacao, dreId, ueId, turmaId, alunoCodigo, situacao);

            var parametros = new { dreId, ueId, turmaId, alunoCodigo, situacao };
            var retorno = new PaginacaoResultadoDto<EncaminhamentoAEEAlunoTurmaDto>();

            using (var multi = await database.Conexao.QueryMultipleAsync(query, parametros))
            {
                retorno.Items = multi.Read<EncaminhamentoAEEAlunoTurmaDto>();
                retorno.TotalRegistros = multi.ReadFirst<int>();
            }

            retorno.TotalPaginas = (int)Math.Ceiling((double)retorno.TotalRegistros / paginacao.QuantidadeRegistros);

            return retorno;
        }

        private static string MontaQueryCompleta(Paginacao paginacao, long dreId, long ueId, long turmaId, string alunoCodigo, int? situacao)
        {
            StringBuilder sql = new StringBuilder();

            MontaQueryConsulta(paginacao, sql, contador: false, ueId, turmaId, alunoCodigo, situacao);

            sql.AppendLine(";");

            MontaQueryConsulta(paginacao, sql, contador: true, ueId, turmaId, alunoCodigo, situacao);

            return sql.ToString();
        }

        private static void MontaQueryConsulta(Paginacao paginacao, StringBuilder sql, bool contador, long ueId, long turmaId, string alunoCodigo, int? situacao)
        {
            ObtenhaCabecalho(sql, contador);

            ObtenhaFiltro(sql, ueId, turmaId, alunoCodigo, situacao);

            if (!contador)
                sql.AppendLine("order by ea.aluno_nome");

            if (paginacao.QuantidadeRegistros > 0 && !contador)
                sql.AppendLine($"OFFSET {paginacao.QuantidadeRegistrosIgnorados} ROWS FETCH NEXT {paginacao.QuantidadeRegistros} ROWS ONLY");
        }

        private static void ObtenhaCabecalho(StringBuilder sql, bool contador)
        {
            sql.AppendLine("select ");
            if (contador)
                sql.AppendLine("count(ea.id)");
            else
            {
                sql.AppendLine(" ea.id ");
                sql.AppendLine(", ea.aluno_codigo as AlunoCodigo ");
                sql.AppendLine(", ea.aluno_nome as AlunoNome ");
                sql.AppendLine(", t.turma_id as TurmaCodigo");
                sql.AppendLine(", t.nome as TurmaNome");
                sql.AppendLine(", t.modalidade_codigo as TurmaModalidade");
                sql.AppendLine(", t.ano_letivo as TurmaAno");
                sql.AppendLine(", ea.situacao");
            }

            sql.AppendLine("from encaminhamento_aee ea ");
            sql.AppendLine("inner join turma t on t.id = ea.turma_id");
            sql.AppendLine("inner join ue on t.ue_id = ue.id");
        }

        private static void ObtenhaFiltro(StringBuilder sql, long ueId, long turmaId, string alunoCodigo, int? situacao)
        {
            sql.AppendLine("where ue.dre_id = @dreId and not ea.excluido");

            if (ueId > 0)
                sql.AppendLine("and ue.id = @ueId");
            if (turmaId > 0)
                sql.AppendLine("and t.id = @turmaId");
            if (!string.IsNullOrEmpty(alunoCodigo))
                sql.AppendLine("and ea.aluno_codigo = @alunoCodigo");
            if (situacao.HasValue && situacao > 0)
                sql.AppendLine("and ea.situacao = @situacao");
        }

        public async Task<SituacaoAEE> ObterSituacaoEncaminhamentoAEE(long encaminhamentoAEEId)
        {
            var query = "select situacao from encaminhamento_aee ea where id = @encaminhamentoAEEId";

            return await database.Conexao.QueryFirstOrDefaultAsync<SituacaoAEE>(query, new { encaminhamentoAEEId });
        }

        public async Task<EncaminhamentoAEE> ObterEncaminhamentoPorId(long id)
        {
            var query = @"select ea.*, eas.*, qea.*, rea.*
                        from encaminhamento_aee ea
                        inner join encaminhamento_aee_secao eas on eas.encaminhamento_aee_id = ea.id
                        inner join questao_encaminhamento_aee qea on qea.encaminhamento_aee_secao_id = eas.id
                        inner join resposta_encaminhamento_aee rea on rea.questao_encaminhamento_id = qea.id
                        where ea.id = @id";

            var encaminhamento = new EncaminhamentoAEE();

            await database.Conexao.QueryAsync<EncaminhamentoAEE, EncaminhamentoAEESecao, QuestaoEncaminhamentoAEE, RespostaEncaminhamentoAEE, EncaminhamentoAEE>(query.ToString()
                , (encaminhamentoAEE, secaoEncaminhamento, questaoEncaminhamentoAEE, respostaEncaminhamento) =>
            {
                if (encaminhamento.Id == 0)
                    encaminhamento = encaminhamentoAEE;

                var secao = encaminhamento.Secoes.FirstOrDefault(c => c.Id == secaoEncaminhamento.Id);
                if (secao == null)
                {
                    secao = secaoEncaminhamento;
                    encaminhamento.Secoes.Add(secao);
                }

                var questao = secao.Questoes.FirstOrDefault(c => c.Id == questaoEncaminhamentoAEE.Id);
                if (questao == null)
                {
                    questao = questaoEncaminhamentoAEE;
                    secao.Questoes.Add(questao);
                }

                var resposta = questao.Respostas.FirstOrDefault(c => c.Id == respostaEncaminhamento.Id);
                if (resposta == null)
                {
                    resposta = respostaEncaminhamento;
                    questao.Respostas.Add(resposta);
                }

                return encaminhamento;
            }, new { id });

            return encaminhamento;
        }

        public async Task<EncaminhamentoAEE> ObterEncaminhamentoComTurmaPorId(long encaminhamentoId)
        {
            var query = @"select ea.*, t.*
                            from encaminhamento_aee ea
                           inner join turma t on t.id = ea.turma_id     
                           where ea.id = @encaminhamentoId";

            return (await database.Conexao.QueryAsync<EncaminhamentoAEE, Turma, EncaminhamentoAEE>(query,
                (encaminhamentoAEE, turma) =>
                {
                    encaminhamentoAEE.Turma = turma;
                    return encaminhamentoAEE;
                }, new { encaminhamentoId })).FirstOrDefault();
        }
    }
}
