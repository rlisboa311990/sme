﻿using MediatR;
using SME.SGP.Dominio;
using SME.SGP.Dominio.Interfaces;
using SME.SGP.Infra;
using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using SME.SGP.Infra.Interface;

namespace SME.SGP.Aplicacao
{
    public class CopiarArquivoCommandHandler : IRequestHandler<CopiarArquivoCommand, string>
    {
        private readonly IRepositorioArquivo repositorioArquivo;

        private readonly IServicoArmazenamento servicoArmazenamento;
        
        public CopiarArquivoCommandHandler(IRepositorioArquivo repositorioArquivo,IServicoArmazenamento servicoArmazenamento)
        {
            this.repositorioArquivo = repositorioArquivo ?? throw new ArgumentNullException(nameof(repositorioArquivo));
            this.servicoArmazenamento = servicoArmazenamento ?? throw new ArgumentNullException(nameof(servicoArmazenamento));
        }

        public async Task<string> Handle(CopiarArquivoCommand request, CancellationToken cancellationToken)
        {
            await servicoArmazenamento.Copiar(request.Nome);
                
            await SalvarCopiaArquivo(request.TipoArquivoDestino, request.Nome);
            
            return $@"/{request.TipoArquivoDestino.Name()}/{DateTime.Now.Year}/{DateTime.Now.Month:00}/";
        }
        private async Task SalvarCopiaArquivo(TipoArquivo tipo, string nomeArquivo)
        {
            var arquivo = await repositorioArquivo.ObterPorCodigo(new Guid(nomeArquivo.Split('.').FirstOrDefault()));
            if (arquivo != null)
            {
                arquivo.Tipo = tipo;
                await repositorioArquivo.SalvarAsync(MapearParaEntidade(tipo, arquivo));
            }
        }
        private Arquivo MapearParaEntidade(TipoArquivo tipo, Arquivo arquivo)
        {
            return new Arquivo
            {
                Nome = arquivo.Nome,
                Codigo = arquivo.Codigo,
                Tipo = tipo,
                TipoConteudo = arquivo.TipoConteudo
            };
        }
    }
}
