﻿using System;
using MediatR;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.Formats.Gif;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.PixelFormats;
using SME.SGP.Aplicacao;
using SME.SGP.Dominio.Enumerados;
using SME.SGP.Infra;

namespace SME.SGP.ComprimirArquivos.Worker
{
    public class ComprimirImagemCommandHandler : IRequestHandler<ComprimirImagemCommand, bool>
    {
        private readonly IMediator mediator;
        
        public ComprimirImagemCommandHandler(IMediator mediator)
        {
            this.mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        }
        
        public async Task<bool> Handle(ComprimirImagemCommand request, CancellationToken cancellationToken)
        {
            try
            {
                if (!request.NomeArquivo.EhArquivoImagemParaOtimizar())
                    return false;
            
                var input = Path.Combine(UtilArquivo.ObterDiretorioCompletoArquivos(), request.NomeArquivo);

                if (!File.Exists(input))
                    await mediator.Send(new SalvarLogViaRabbitCommand($"O arquivo '{request.NomeArquivo}' não foi localizado no endereço '{input}'", LogNivel.Critico, LogContexto.ComprimirArquivos)); 

                var output = Path.Combine(UtilArquivo.ObterDiretorioCompletoTemporario(), request.NomeArquivo);

                using (Image image = Image.Load(input))
                {
                    IImageEncoder imageEncoder;
                    switch (image)
                    {
                        case Image<Rgba32> _:
                        case Image<Bgra32> _:
                            imageEncoder = new PngEncoder { CompressionLevel = PngCompressionLevel.BestCompression };
                            break;
                        case Image<Argb32> _:
                            imageEncoder = new GifEncoder();
                            break;
                        default: 
                            imageEncoder = new JpegEncoder { Quality = 50 };
                            break;
                    }
                    image.Save(output, imageEncoder);
                }

                await mediator.Send(new MoverExcluirArquivoFisicoCommand(input, output));

                return true;
            }
            catch (Exception ex)
            {
                await mediator.Send(new SalvarLogViaRabbitCommand($"Erro ao comprimir arquivo imagem", LogNivel.Critico, LogContexto.ComprimirArquivos, ex.Message,rastreamento:ex.StackTrace,excecaoInterna:ex.InnerException?.ToString()));
                return false;
            }
        }
    }
}
