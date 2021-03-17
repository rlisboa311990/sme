﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace SME.SGP.Dominio.Interfaces
{
    public interface IRepositorioAcompanhamentoAlunoFoto : IRepositorioBase<AcompanhamentoAlunoFoto>
    {
        Task<IEnumerable<Arquivo>> ObterFotosPorSemestreId(long acompanhamentoSemestreId);
        Task<AcompanhamentoAlunoFoto> ObterFotoPorCodigo(Guid codigoFoto);
        Task<AcompanhamentoAlunoFoto> ObterFotoPorMiniaturaId(long miniaturaId);
    }
}
