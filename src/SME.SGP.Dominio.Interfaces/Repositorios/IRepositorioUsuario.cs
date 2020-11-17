﻿using SME.SGP.Infra;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SME.SGP.Dominio.Interfaces
{
    public interface IRepositorioUsuario : IRepositorioBase<Usuario>
    {
        Usuario ObterPorCodigoRfLogin(string codigoRf, string login);
        Usuario ObterPorTokenRecuperacaoSenha(Guid token);
        Task<Usuario> ObterUsuarioPorCodigoRfAsync(string codigoRf);
        Task<ProfessorDto> ObterProfessorDaTurmaPorAulaId(long aulaId);
        Task<IEnumerable<long>> ObterUsuariosIdPorCodigoRf(IList<string> codigoRf);
    }
}