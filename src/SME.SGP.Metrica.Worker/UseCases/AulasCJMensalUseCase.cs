﻿using SME.SGP.Infra;
using SME.SGP.Infra.Dtos;
using SME.SGP.Metrica.Worker.Repositorios.Interfaces;
using SME.SGP.Metrica.Worker.UseCases.Interfaces;
using System;
using System.Threading.Tasks;

namespace SME.SGP.Metrica.Worker.UseCases
{
    public class AulasCJMensalUseCase : IAulasCJMensalUseCase
    {
        private readonly IRepositorioSGPConsulta repositorioSGP;
        private readonly IRepositorioAulasCJMensal repositorioAulasCJ;

        public AulasCJMensalUseCase(IRepositorioSGPConsulta repositorioSGP, IRepositorioAulasCJMensal repositorioAulasCJ)
        {
            this.repositorioSGP = repositorioSGP ?? throw new ArgumentNullException(nameof(repositorioSGP));
            this.repositorioAulasCJ = repositorioAulasCJ ?? throw new ArgumentNullException(nameof(repositorioAulasCJ));
        }

        public async Task<bool> Executar(MensagemRabbit mensagem)
        {
            var parametro = mensagem.ObterObjetoMensagem<FiltroDataDto>();
            var quantidadeRegistros = await repositorioSGP.ObterQuantidadeAulasCJMes(parametro.Data);

            await repositorioAulasCJ.InserirAsync(new Entidade.AulasCJMensal(parametro.Data, quantidadeRegistros));
            
            return true;
        }
    }
}
