﻿using SME.SGP.Infra;
using System.Threading.Tasks;

namespace SME.SGP.Aplicacao.Interfaces
{
    public interface ITrataSincronizacaoInstitucionalTurmaUseCase
    {
        Task<bool> Executar(MensagemRabbit mensagemRabbit);
    }
}