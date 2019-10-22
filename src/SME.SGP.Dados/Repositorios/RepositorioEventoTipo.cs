﻿using Dapper;
using SME.SGP.Dados.Contexto;
using SME.SGP.Dominio.Entidades;
using SME.SGP.Dominio.Enumerados;
using SME.SGP.Dominio.Interfaces.Repositorios;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SME.SGP.Dados.Repositorios
{
    public class RepositorioEventoTipo : RepositorioBase<EventoTipo>, IRepositorioEventoTipo
    {
        public RepositorioEventoTipo(ISgpContext conexao) : base(conexao)
        {
        }

        public IList<EventoTipo> ListarTipos(EventoLocalOcorrencia eventoLocalOcorrencia, EventoLetivo eventoLetivo, string descricao)
        {
            StringBuilder sql = new StringBuilder();

            sql.AppendLine("select * from evento_tipo where excluido = false");

            if (eventoLocalOcorrencia != 0)
                sql.AppendLine("and local_ocorrencia = @local_ocorrencia");

            if (eventoLetivo != 0)
                sql.AppendLine("and letivo = @letivo");

            if (!string.IsNullOrWhiteSpace(descricao))
                sql.AppendLine("and descricao like @descricao");

            return database.Conexao.Query<EventoTipo>(sql.ToString(), new { local_ocorrencia = eventoLocalOcorrencia, letivo = eventoLetivo, descricao = $"%{descricao}%" }).ToList();
        }
    }
}