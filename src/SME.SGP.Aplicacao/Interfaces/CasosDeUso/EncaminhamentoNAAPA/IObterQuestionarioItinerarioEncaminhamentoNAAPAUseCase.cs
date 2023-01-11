﻿using SME.SGP.Infra;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SME.SGP.Aplicacao.Interfaces.CasosDeUso
{
    public interface IObterQuestionarioItinerarioEncaminhamentoNAAPAUseCase
    {
        Task<EncaminhamentoNAAPASecaoItineranciaQuestoesDto> Executar(long questionarioId, long? encaminhamentoSecaoId);
    }
}
