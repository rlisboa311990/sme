﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Dommel.Bulk;
using Npgsql;
using NpgsqlTypes;
using SME.SGP.Dominio;
using SME.SGP.Dominio.Interfaces;
using SME.SGP.Infra;
using SME.SGP.Infra.Interface;

namespace SME.SGP.Dados.Repositorios
{
    public class RepositorioCompensacaoAusenciaAluno : RepositorioBase<CompensacaoAusenciaAluno>, IRepositorioCompensacaoAusenciaAluno
    {
        public RepositorioCompensacaoAusenciaAluno(ISgpContext database, IServicoAuditoria servicoAuditoria) : base(database, servicoAuditoria)
        {
        }

        public async Task<bool> InserirVarios(IEnumerable<CompensacaoAusenciaAluno> registros, Usuario usuarioLogado)
        {
            //var sql = @"copy compensacao_ausencia_aluno (                                         
            //                            compensacao_ausencia_id, 
            //                            codigo_aluno,
            //                            qtd_faltas_compensadas, 
            //                            notificado,
            //                            criado_por,                                        
            //                            criado_rf,
            //                            criado_em)
            //                from
            //                stdin (FORMAT binary)";

            //using (var writer = ((NpgsqlConnection) database.Conexao).BeginBinaryImport(sql))
            //{
            //    foreach (var compensacao in registros)
            //    {
            //        writer.StartRow();
            //        writer.Write(compensacao.CompensacaoAusenciaId, NpgsqlDbType.Bigint);
            //        writer.Write(compensacao.CodigoAluno, NpgsqlDbType.Varchar);
            //        writer.Write(compensacao.QuantidadeFaltasCompensadas, NpgsqlDbType.Integer);
            //        writer.Write(compensacao.Notificado);
            //        writer.Write(compensacao.CriadoPor ?? usuarioLogado.Nome);
            //        writer.Write(compensacao.CriadoRF ?? usuarioLogado.Login);
            //        writer.Write(compensacao.CriadoEm);
            //    }

            //    writer.Complete();
            //}

            //var add = registros.ToList().FirstOrDefault();
            //var registroInserir = registros.ToList();
            //for (int i = 0; i < 5000; i++)
            //{
            //    registroInserir.Add(add);
            //}
            //var dataInicio = DateTime.Now;
            await database.Conexao.InserirVariosRegistrosAsync(registros);
            //var dataFim = DateTime.Now;
            return await Task.FromResult(true);
        }
    }
}