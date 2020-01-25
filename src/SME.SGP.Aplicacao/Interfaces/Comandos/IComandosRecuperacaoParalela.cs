﻿using SME.SGP.Dto;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SME.SGP.Aplicacao
{
    public interface IComandosRecuperacaoParalela
    {
        Task<IEnumerable<RecuperacaoParalelaListagemDto>> Salvar(RecuperacaoParalelaDto recuperacaoParalelaDto);
    }
}