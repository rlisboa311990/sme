﻿using System.Collections.Generic;

namespace SME.SGP.Dominio.Interfaces
{
    public interface IRepositorioNotificacao : IRepositorioBase<Notificacao>
    {
        IEnumerable<Notificacao> ObterPorDreOuEscolaOuStatusOuTurmoOuUsuarioOuTipoOuCategoriaOuTitulo(string dreId, string escolaId, int statusId, string turmaId,
            string usuarioId, int tipoId, int categoriaId, string titulo, long codigo, int anoLetivo);

        IEnumerable<Notificacao> ObterNotificacoesPorAnoLetivoERf(int anoLetivo, string usuarioRf, int limite);

        int ObterQuantidadeNotificacoesNaoLidasPorAnoLetivoERf(int anoLetivo, string usuarioRf);

        long ObterUltimoCodigoPorAno(int ano);
    }
}