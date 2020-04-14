﻿using SME.SGP.Dominio;
using SME.SGP.Dominio.Interfaces;
using SME.SGP.Infra;

namespace SME.SGP.Dados.Repositorios
{
    public class RepositorioConselhoClasse : RepositorioBase<ConselhoClasse>, IRepositorioConselhoClasse
    {
        public RepositorioConselhoClasse(ISgpContext database) : base(database)
        {
        }

        //public async Task<ConselhoClasse> ObterPorTurmaBimestreAsync(int bimestre, long fechamentoTurmaId)
        //{
        //    var query = @"
        //                select
        //                 *
        //                from
        //                 conselho_classe cc
        //                inner join periodo_escolar pe on
        //                 ft.periodo_escolar_id = pe.id
        //                where
        //                 cc.fechamento_turma_id = @fechamentoTurmaId
        //                 and pe.bimestre = @bimestre";

        //    return await database.Conexao.QueryFirstOrDefaultAsync<ConselhoClasse>(query, new { bimestre, fechamentoTurmaId });
        //}
    }
}