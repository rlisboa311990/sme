﻿using SME.SGP.Infra;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SME.SGP.Dominio.Interfaces
{
    public interface IRepositorioPendencia : IRepositorioBase<Pendencia>
    {
        void AtualizarPendencias(long fechamentoId, SituacaoPendencia situacaoPendencia, TipoPendencia tipoPendencia);
        
        void ExcluirPendenciasFechamento(long fechamentoId, TipoPendencia tipoPendencia);
        
        Task<PaginacaoResultadoDto<Pendencia>> ListarPendenciasUsuario(long usuarioId, string turmaId, int? tipoPendencia, string tituloPendencia, Paginacao paginacao);
        
        Task<long[]> ObterIdsPendenciasPorPlanoAEEId(long planoAeeId);
        
        Task AtualizarStatusPendenciasPorIds(long[] ids, SituacaoPendencia situacaoPendencia);
        
        Task<IEnumerable<PendenciaPendenteDto>> ObterPendenciasPendentes();
        
        Task<IEnumerable<PendenciaPendenteDto>> ObterPendenciasSemPendenciaPerfilUsuario();
    }
}