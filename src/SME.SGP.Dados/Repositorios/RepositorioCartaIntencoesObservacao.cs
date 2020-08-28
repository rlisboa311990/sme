﻿using SME.SGP.Dominio;
using SME.SGP.Dominio.Interfaces;
using SME.SGP.Infra;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SME.SGP.Dados.Repositorios
{
    public class RepositorioCartaIntencoesObservacao : RepositorioBase<CartaIntencoesObservacao>, IRepositorioCartaIntencoesObservacao
    {
        public RepositorioCartaIntencoesObservacao(ISgpContext conexao) : base(conexao) { }

        public async Task<IEnumerable<ListarObservacaoCartaIntencoesDto>> ListarPorCartaIntencoesAsync(long cartaIntencoesId, long usuarioLogadoId)
        {
			var sql = @"select
							id,
							observacao,
							(usuario_id = @usuarioLogadoId) as Proprietario,
							criado_em as CriadoEm,
							criado_por as CriadoPor,
							criado_rf as CriadoRf,
							alterado_em as AlteradoEm,
							alterado_por as AlteradoPor,
							alterado_rf as AlteradoRf
						from
							carta_intencoes_observacao
						where
							carta_intencoes_id = @cartaIntencoesId
							and not excluido 
                        order by criado_em desc";

			return await database.Conexao.QueryAsync<ListarObservacaoCartaIntencoesDto>(sql, new { cartaIntencoesId, usuarioLogadoId });
		}

      //  public async Task<IEnumerable<ListarObservacaoDiarioBordoDto>> ListarPorDiarioBordoAsync(long diarioBordoId, long usuarioLogadoId)
      //  {
      //      var sql = @"select
						//	id,
						//	observacao,
						//	(usuario_id = @usuarioLogadoId) as Proprietario,
						//	criado_em as CriadoEm,
						//	criado_por as CriadoPor,
						//	criado_rf as CriadoRf,
						//	alterado_em as AlteradoEm,
						//	alterado_por as AlteradoPor,
						//	alterado_rf as AlteradoRf
						//from
						//	diario_bordo_observacao
						//where
						//	diario_bordo_id = @diarioBordoId
						//	and not excluido 
      //                  order by criado_em desc";

      //      return await database.Conexao.QueryAsync<ListarObservacaoDiarioBordoDto>(sql, new { diarioBordoId, usuarioLogadoId });
      //  }
    }
}
