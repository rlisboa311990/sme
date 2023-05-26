﻿using System.Collections.Generic;
using System.Threading.Tasks;
using SME.SGP.Dominio;
using SME.SGP.Infra;
using SME.SGP.Infra.Interface;

namespace SME.SGP.Dados.Repositorios
{
    public class RepositorioConsolidadoEncaminhamentoNAAPA: RepositorioBase<ConsolidadoEncaminhamentoNAAPA>, IRepositorioConsolidadoEncaminhamentoNAAPA
    {
        public RepositorioConsolidadoEncaminhamentoNAAPA(ISgpContext database, IServicoAuditoria servicoAuditoria) : base(database, servicoAuditoria)
        {
        }

        public async Task<IEnumerable<ConsolidadoEncaminhamentoNAAPA>> ObterPorUeIdAnoLetivo(long ueId,int anoLetivo)
        {
            var query = " select * from consolidado_encaminhamento_naapa cen where cen.ue_id = @ueId and cen.ano_letivo = @anoLetivo ";
            return await database.Conexao.QueryAsync<ConsolidadoEncaminhamentoNAAPA>(query, new {  ueId,anoLetivo }, commandTimeout: 60);
        }
    }
}