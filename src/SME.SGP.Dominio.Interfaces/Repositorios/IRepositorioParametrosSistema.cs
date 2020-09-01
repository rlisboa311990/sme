﻿using System.Collections.Generic;

using System.Threading.Tasks;

namespace SME.SGP.Dominio.Interfaces
{
    public interface IRepositorioParametrosSistema : IRepositorioBase<ParametrosSistema>
    {
        Task AtualizarValorPorTipoAsync(TipoParametroSistema tipo, string valor, int? ano = null);

        IEnumerable<KeyValuePair<string, string>> ObterChaveEValorPorTipo(TipoParametroSistema tipo);

        Task<string> ObterValorUnicoPorTipo(TipoParametroSistema tipo);

        string ObterValorPorTipoEAno(TipoParametroSistema tipo, int? ano = null);
        Task<T> ObterValorUnicoPorTipo<T>(TipoParametroSistema tipoParametroSistema);
    }
}