﻿using Dapper;
using SME.SGP.Dominio;
using SME.SGP.Dominio.Interfaces;
using SME.SGP.Infra;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SME.SGP.Dados.Repositorios
{
    public class RepositorioSupervisorEscolaDre : RepositorioBase<SupervisorEscolaDre>, IRepositorioSupervisorEscolaDre
    {
        public RepositorioSupervisorEscolaDre(ISgpContext conexao) : base(conexao)
        {
        }

        public IEnumerable<SupervisorEscolasDreDto> ObtemPorDreESupervisor(string dreId, string supervisorId, bool excluidos = false)
        {
            StringBuilder query = new();

            query.AppendLine("select id, dre_id, escola_id, supervisor_id, criado_em, criado_por, alterado_em, alterado_por, criado_rf, alterado_rf, excluido");
            query.AppendLine("from supervisor_escola_dre sed");
            query.AppendLine("where 1 = 1 and sed.Tipo = @tipoResponsavelAtribuicao");

            if (!excluidos)
                query.AppendLine("and excluido = false");

            if (!string.IsNullOrEmpty(supervisorId))
                query.AppendLine("and sed.supervisor_id = @supervisorId");

            if (!string.IsNullOrEmpty(dreId))
                query.AppendLine("and sed.dre_id = @dreId");

            var tipoResponsavelAtribuicao = (int)TipoResponsavelAtribuicao.SupervisorEscolar;
            return database.Conexao.Query<SupervisorEscolasDreDto>(query.ToString(), new { supervisorId, dreId, tipoResponsavelAtribuicao }).AsList();
        }

        public IEnumerable<SupervisorEscolasDreDto> ObtemPorDreESupervisores(string dreId, string[] supervisoresId)
        {
            StringBuilder query = new();

            query.AppendLine("select id, dre_id, escola_id, supervisor_id, criado_em, criado_por, alterado_em, alterado_por, criado_rf, alterado_rf, excluido");
            query.AppendLine("from supervisor_escola_dre sed");
            query.AppendLine("where excluido = false and sed.Tipo = @tipoResponsavelAtribuicao");

            if (supervisoresId.Length > 0)
            {
                var idsSupervisores = from a in supervisoresId
                                      select $"'{a}'";

                query.AppendLine($"and sed.supervisor_id in ({string.Join(",", idsSupervisores)})");
            }

            if (!string.IsNullOrEmpty(dreId))
                query.AppendLine("and sed.dre_id = @dreId");

            var tipoResponsavelAtribuicao = (int)TipoResponsavelAtribuicao.SupervisorEscolar;
            return database.Conexao.Query<SupervisorEscolasDreDto>(query.ToString(), new { dreId, tipoResponsavelAtribuicao }).AsList();
        }

        public SupervisorEscolasDreDto ObtemPorUe(FiltroObterSupervisorEscolasDto filtro)
        {
            StringBuilder query = new();

            query.AppendLine("select id, tipo, dre_id as DreId, escola_id as EscolaId, supervisor_id as SupervisorId");
            query.AppendLine(", criado_em as CriadoEm, criado_por as CriadoPor, alterado_em as AlteradoEm");
            query.AppendLine(", alterado_por as AlteradoPor, criado_rf as CriadoRf, alterado_rf as AlteradoRf, excluido");
            query.AppendLine("from supervisor_escola_dre sed");
            query.AppendLine("where excluido = false and dre_id = @dre");

            if (!string.IsNullOrEmpty(filtro.UeCodigo))
                query.AppendLine(" and escola_id = @ue ");

            if (filtro.TipoCodigo > 0)
                query.AppendLine(" and tipo = @tipo ");

            var parametros = new
            {
                dre = filtro.DreCodigo,
                ue = filtro.UeCodigo,
                tipo = filtro.TipoCodigo
            };

            return database.Conexao.Query<SupervisorEscolasDreDto>(query.ToString(), parametros)
                .AsList()
                .FirstOrDefault();
        }

        public async Task<IEnumerable<SupervisorEscolasDreDto>> ObtemSupervisoresPorDreAsync(string codigoDre,
            TipoResponsavelAtribuicao tipoResponsavelAtribuicao)
        {
            StringBuilder query = new();

            query.AppendLine("select id, dre_id as DreId, escola_id as UeId, supervisor_id as SupervisorId, criado_em as CriadoEm," +
                "criado_por as CriadoPor, alterado_em as AlteradoEm, alterado_por as AlteradoPor, criado_rf as CriadoRF, " +
                "alterado_rf as AlteradoRF, excluido as Excluido");

            query.AppendLine("from supervisor_escola_dre sed");

            query.AppendLine("where dre_id = @codigoDre " +
                "and excluido = false " +
                "and tipo = @tipoResponsavelAtribuicao");

            return await database.Conexao.QueryAsync<SupervisorEscolasDreDto>(query.ToString(), new { codigoDre, tipoResponsavelAtribuicao });
        }

        public IEnumerable<SupervisorEscolasDreDto> ObtemSupervisoresPorUe(string ueId)
        {
            StringBuilder query = new();

            query.AppendLine("select id, dre_id as DreId, escola_id as UeId, supervisor_id as SupervisorId, criado_em as CriadoEm, criado_por as CriadoPor, alterado_em as AlteradoEm, alterado_por as AlteradoPor, criado_rf as CriadoRF, alterado_rf as AlteradoRF, excluido as Excluido");
            query.AppendLine("from supervisor_escola_dre sed");
            query.AppendLine("where escola_id = @ueId and excluido = false and sed.Tipo = @tipoResponsavelAtribuicao");

            var tipoResponsavelAtribuicao = (int)TipoResponsavelAtribuicao.SupervisorEscolar;
            return database.Conexao.Query<SupervisorEscolasDreDto>(query.ToString(), new { ueId, tipoResponsavelAtribuicao })
                .AsList();
        }

        public Task<IEnumerable<SupervisorEscolasDreDto>> ObtemSupervisoresPorUeAsync(string ueId)
        {
            StringBuilder query = new();

            query.AppendLine("select id, dre_id as DreId, escola_id as UeId, supervisor_id as SupervisorId, criado_em as CriadoEm, criado_por as CriadoPor, alterado_em as AlteradoEm, alterado_por as AlteradoPor, criado_rf as CriadoRF, alterado_rf as AlteradoRF, excluido as Excluido");
            query.AppendLine("from supervisor_escola_dre sed");
            query.AppendLine("where escola_id = @ueId and excluido = false and sed.Tipo = @tipoResponsavelAtribuicao");

            var tipoResponsavelAtribuicao = (int)TipoResponsavelAtribuicao.SupervisorEscolar;
            return database.Conexao.QueryAsync<SupervisorEscolasDreDto>(query.ToString(), new { ueId, tipoResponsavelAtribuicao });
        }

        public Task<IEnumerable<DadosAbrangenciaSupervisorDto>> ObterDadosAbrangenciaSupervisor(string rfSupervisor, bool consideraHistorico, int anoLetivo)
        {
            var sqlQuery = new StringBuilder();

            sqlQuery.AppendLine("select distinct vact.modalidade_codigo Modalidade,");
            sqlQuery.AppendLine("                vact.dre_abreviacao AbreviacaoDre,");
            sqlQuery.AppendLine("                vact.dre_codigo CodigoDre,");
            sqlQuery.AppendLine("                vact.dre_nome DreNome,");
            sqlQuery.AppendLine("                vact.dre_id DreId,");
            sqlQuery.AppendLine("				 vact.ue_codigo CodigoUe,");
            sqlQuery.AppendLine("				 vact.ue_nome UeNome,");
            sqlQuery.AppendLine("				 ue.tipo_escola TipoEscola,");
            sqlQuery.AppendLine("				 vact.ue_id UeId,");
            sqlQuery.AppendLine("				 vact.turma_semestre Semestre,");
            sqlQuery.AppendLine("				 vact.turma_codigo CodigoTurma,");
            sqlQuery.AppendLine("                vact.turma_ano TurmaAno,");
            sqlQuery.AppendLine("                vact.turma_ano_letivo TurmaAnoLetivo,");
            sqlQuery.AppendLine("                vact.turma_nome TurmaNome,");
            sqlQuery.AppendLine("                vact.qt_duracao_aula QuantidadeDuracaoAula,");
            sqlQuery.AppendLine("                vact.tipo_turno TipoTurno,");
            sqlQuery.AppendLine("                vact.ensino_especial EnsinoEspecial,");
            sqlQuery.AppendLine("                vact.turma_id TurmaId,");
            sqlQuery.AppendLine("                vact.tipo_turma TipoTurma,");
            sqlQuery.AppendLine("                vact.nome_filtro NomeFiltro");
            sqlQuery.AppendLine("	from supervisor_escola_dre sed");
            sqlQuery.AppendLine("		inner join ue");
            sqlQuery.AppendLine("			on sed.escola_id = ue.ue_id");
            sqlQuery.AppendLine("		inner join v_abrangencia_cadeia_turmas vact");
            sqlQuery.AppendLine("			on ue.id = vact.ue_id");
            sqlQuery.AppendLine("where sed.supervisor_id = @rfSupervisor and");
            sqlQuery.AppendLine("	 not sed.excluido and");
            sqlQuery.AppendLine("    vact.turma_historica = @consideraHistorico and");
            sqlQuery.AppendLine("    vact.turma_ano_letivo = @anoLetivo and");
            sqlQuery.AppendLine("    sed.Tipo = @tipoResponsavelAtribuicao");

            var tipoResponsavelAtribuicao = (int)TipoResponsavelAtribuicao.SupervisorEscolar;

            return database.Conexao
                .QueryAsync<DadosAbrangenciaSupervisorDto>(sqlQuery.ToString(),
                new
                {
                    rfSupervisor,
                    consideraHistorico,
                    anoLetivo,
                    tipoResponsavelAtribuicao
                });
        }
    }
}