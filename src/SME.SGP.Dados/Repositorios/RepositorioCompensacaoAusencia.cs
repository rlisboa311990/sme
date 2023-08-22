﻿using SME.SGP.Dados.Repositorios;
using SME.SGP.Dominio;
using SME.SGP.Dominio.Interfaces;
using SME.SGP.Infra;
using SME.SGP.Infra.Interface;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace SME.SGP.Dados
{
    public class RepositorioCompensacaoAusencia : RepositorioBase<CompensacaoAusencia>, IRepositorioCompensacaoAusencia
    {
        public RepositorioCompensacaoAusencia(ISgpContext database, IServicoAuditoria servicoAuditoria) : base(database, servicoAuditoria)
        {
        }

        public async Task<IEnumerable<RegistroFaltasNaoCompensadaDto>> ObterAusenciaParaCompensacao(long compensacaoId, string turmaId, string[] disciplinasId, string[] codigoAlunos, int bimestre, string professor = null)
        {
            var tipoFrequenciaFalta = (int)TipoFrequencia.F;

            var query = @$" select
								rfa.codigo_aluno as CodigoAluno,
								a.id AulaId,
								a.data_aula as DataAula,
								rfa.numero_aula as NumeroAula,
								'Aula ' || rfa.numero_aula as Descricao,
								rfa.id as RegistroFrequenciaAlunoId,
								case 
									when caaa.id is null then false else true 
								end Sugestao
							from
								registro_frequencia_aluno rfa
							inner join aula a on rfa.aula_id = a.id
							inner join periodo_escolar p on a.tipo_calendario_id = p.tipo_calendario_id
							left join compensacao_ausencia_aluno_aula caaa on caaa.registro_frequencia_aluno_id = rfa.id and not caaa.excluido 
							left join compensacao_ausencia_aluno caa on caa.id = caaa.compensacao_ausencia_aluno_id and not caa.excluido
							left join compensacao_ausencia ca on ca.id = caa.compensacao_ausencia_id and not ca.excluido {(!string.IsNullOrWhiteSpace(professor) ? "and ca.professor_rf = @professor" : string.Empty)}
							where not rfa.excluido
							  and not a.excluido
							  and rfa.numero_aula <= a.quantidade
							  and a.data_aula >= p.periodo_inicio
							  and a.data_aula <= p.periodo_fim
							  and rfa.codigo_aluno = ANY(@codigoAlunos)
							  and a.turma_id = @turmaId
							  and rfa.valor = @tipoFrequenciaFalta
							  and p.bimestre = @bimestre
							  and a.disciplina_id = any(@disciplinasId)
							  and (ca.id = @compensacaoId or ca.id is null)
							  {(!string.IsNullOrWhiteSpace(professor) ? " and a.professor_rf = @professor " : string.Empty)}
							order by a.data_aula, rfa.numero_aula";

            var parametros = new
            {
                compensacaoId,
                turmaId,
                disciplinasId,
                codigoAlunos,
                bimestre,
                tipoFrequenciaFalta,
                professor
            };

            return await database.Conexao.QueryAsync<RegistroFaltasNaoCompensadaDto>(query, parametros);
        }      

        public async Task<IEnumerable<long>> ObterSemAlunoPorIds(long[] ids)
        {
            // TODO fazer o update for select para atualizar os dados utilizando a transação em aberto.
            var query = new StringBuilder(@"select ca.id
                                            from compensacao_ausencia ca
                                            left join compensacao_ausencia_aluno caa on ca.id = caa.compensacao_ausencia_id
                                            where not ca.excluido 
                                                  and ca.id = any(@ids)    
                                            group by ca.id
                                            having coalesce(sum(caa.qtd_faltas_compensadas) filter (where not caa.excluido),0)  = 0 ");

            return await database.Conexao.QueryAsync<long>(query.ToString(), new { ids });
        }       
    }
}
