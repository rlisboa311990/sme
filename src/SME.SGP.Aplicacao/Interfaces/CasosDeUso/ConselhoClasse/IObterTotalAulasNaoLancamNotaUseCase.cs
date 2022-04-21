﻿using SME.SGP.Infra.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SME.SGP.Aplicacao.Interfaces.CasosDeUso.ConselhoClasse
{
    public interface IObterTotalAulasNaoLancamNotaUseCase
    {
        Task<IEnumerable<TotalAulasNaoLancamNotaDto>> Executar(string codigoTurma, int bimestre);
    }
}
