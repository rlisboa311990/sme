﻿using Dapper;
using SME.SGP.Dominio;
using SME.SGP.Dominio.Interfaces;
using SME.SGP.Infra;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SME.SGP.Dados.Repositorios
{
    public class RepositorioFechamento : RepositorioBase<Fechamento>, IRepositorioFechamento
    {
        public RepositorioFechamento(ISgpContext conexao) : base(conexao)
        {
        }

        public Fechamento ObterPorTipoCalendarioDreEUE(long tipoCalendarioId, long? dreId, long? ueId)
        {
            var query = new StringBuilder("select f.*,fb.*,p.*, t.*");
            query.AppendLine("from");
            query.AppendLine("fechamento f");
            query.AppendLine("inner join fechamento_bimestre fb on");
            query.AppendLine("f.id = fb.fechamento_id");
            query.AppendLine("inner join periodo_escolar p on");
            query.AppendLine("fb.periodo_escolar_id = p.id");
            query.AppendLine("inner join tipo_calendario t on");
            query.AppendLine("p.tipo_calendario_id = t.id");
            query.AppendLine("where 1=1");
            query.AppendLine("and p.tipo_calendario_id = @tipoCalendarioId");

            if (dreId.HasValue)
                query.AppendLine("and f.dre_id = @dreId");
            else query.AppendLine("and f.dre_id is null");

            if (ueId.HasValue)
                query.AppendLine("and f.ue_id = @ueId");
            else query.AppendLine("and f.ue_id is null");

            var lookup = new Dictionary<long, Fechamento>();

            var lista = database.Conexao.Query<Fechamento, FechamentoBimestre, PeriodoEscolar, TipoCalendario, Fechamento>(query.ToString(), (fechamento, fechamentoBimestre, periodoEscolar, tipoCalendario) =>
             {
                 Fechamento periodoFechamento;
                 if (!lookup.TryGetValue(fechamento.Id, out periodoFechamento))
                 {
                     periodoFechamento = fechamento;
                     lookup.Add(fechamento.Id, periodoFechamento);
                 }

                 periodoEscolar.AdicionarTipoCalendario(tipoCalendario);
                 periodoFechamento.AdicionarFechamentoBimestre(periodoEscolar, fechamentoBimestre);
                 return periodoFechamento;
             }, new
             {
                 tipoCalendarioId,
                 dreId,
                 ueId
             });
            return lookup.Values.FirstOrDefault();
        }
    }
}