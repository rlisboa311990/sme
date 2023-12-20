﻿using SME.SGP.Infra;
using SME.SGP.Infra.Dtos;
using SME.SGP.Metrica.Worker.Repositorios.Interfaces;
using SME.SGP.Metrica.Worker.UseCases.Interfaces;
using System;
using System.Threading.Tasks;

namespace SME.SGP.Metrica.Worker.UseCases
{
    public class DiariosBordoDiarioUseCase : IDiariosBordoDiarioUseCase
    {
        private readonly IRepositorioSGPConsulta repositorioSGP;
        private readonly IRepositorioDiariosBordoDiario repositorioDiariosBordo;

        public DiariosBordoDiarioUseCase(IRepositorioSGPConsulta repositorioSGP, IRepositorioDiariosBordoDiario repositorioDiariosBordo)
        {
            this.repositorioSGP = repositorioSGP ?? throw new ArgumentNullException(nameof(repositorioSGP));
            this.repositorioDiariosBordo = repositorioDiariosBordo ?? throw new ArgumentNullException(nameof(repositorioDiariosBordo));
        }

        public async Task<bool> Executar(MensagemRabbit mensagem)
        {
            var parametro = mensagem.ObterObjetoMensagem<FiltroDataDto>();
            var quantidadeAcessos = await repositorioSGP.ObterQuantidadeDiariosBordoDia(parametro.Data);

            await repositorioDiariosBordo.InserirAsync(new Entidade.DiariosBordoDiario(parametro.Data, quantidadeAcessos));

            return true;
        }
    }
}
