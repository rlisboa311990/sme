﻿using SME.SGP.Infra;
using SME.SGP.Infra.Dtos;
using SME.SGP.Metrica.Worker.Repositorios.Interfaces;
using SME.SGP.Metrica.Worker.UseCases.Interfaces;
using System;
using System.Threading.Tasks;

namespace SME.SGP.Metrica.Worker.UseCases
{
    public class DevolutivasDiarioBordoMensalUseCase : IDevolutivasDiarioBordoMensalUseCase
    {
        private readonly IRepositorioSGPConsulta repositorioSGP;
        private readonly IRepositorioDevolutivasDiarioBordoMensal repositorioDevolutivas;

        public DevolutivasDiarioBordoMensalUseCase(IRepositorioSGPConsulta repositorioSGP, IRepositorioDevolutivasDiarioBordoMensal repositorioDevolutivas)
        {
            this.repositorioSGP = repositorioSGP ?? throw new ArgumentNullException(nameof(repositorioSGP));
            this.repositorioDevolutivas = repositorioDevolutivas ?? throw new ArgumentNullException(nameof(repositorioDevolutivas));
        }

        public async Task<bool> Executar(MensagemRabbit mensagem)
        {
            var parametro = mensagem.ObterObjetoMensagem<FiltroDataDto>();
            var quantidadeRegistros = await repositorioSGP.ObterQuantidadeDevolutivasDiarioBordoMes(parametro.Data);

            await repositorioDevolutivas.InserirAsync(new Entidade.DevolutivasDiarioBordoMensal(parametro.Data, quantidadeRegistros));

            return true;
        }
    }
}
