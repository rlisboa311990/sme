﻿using MediatR;
using SME.SGP.Dominio;
using System;
using System.Threading;
using System.Threading.Tasks;
using SME.SGP.Infra;

namespace SME.SGP.Aplicacao
{
    public class UploadArquivoCommandHandler : IRequestHandler<UploadArquivoCommand, ArquivoArmazenadoDto>
    {
        private readonly IMediator mediator;

        public UploadArquivoCommandHandler(IMediator mediator)
        {
            this.mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        }

        public async Task<ArquivoArmazenadoDto> Handle(UploadArquivoCommand request, CancellationToken cancellationToken)
        {
            if (request.TipoConteudo != TipoConteudoArquivo.Indefinido)
            {
                if (request.Arquivo.ContentType != request.TipoConteudo.Name())
                    throw new NegocioException("O formato de arquivo enviado não é aceito");
            }

            var nomeArquivo = request.Arquivo.FileName;

            var arquivo = await mediator.Send(new SalvarArquivoRepositorioCommand(nomeArquivo, request.Tipo, request.Arquivo.ContentType));
            await mediator.Send(new ArmazenarArquivoFisicoCommand(request.Arquivo, arquivo.Codigo.ToString(), request.Tipo));

            return arquivo;
        }
    }
}
