﻿using SME.SGP.Infra;
using SME.SGP.Infra.Dtos;
using System.Threading.Tasks;

namespace SME.SGP.Aplicacao.Interfaces
{
    public interface INotificarNovaCartaIntencoesObservacaoUseCase
    {
        Task<bool> Executar(MensagemRabbit mensagemRabbit);
    }
}