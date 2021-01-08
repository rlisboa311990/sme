﻿using SME.SGP.Infra;
using System;
using System.Collections.Generic;
using System.Text;

namespace SME.SGP.Aplicacao.Interfaces
{
    public interface IObterEncaminhamentosAEEUseCase : IUseCase<FiltroPesquisaEncaminhamentosAEEDto, PaginacaoResultadoDto<EncaminhamentosAEEResumoDto>>
    {
    }
}
