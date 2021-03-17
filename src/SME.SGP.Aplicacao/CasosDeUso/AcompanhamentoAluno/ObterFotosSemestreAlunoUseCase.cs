﻿using MediatR;
using SME.SGP.Aplicacao.Interfaces;
using SME.SGP.Dominio;
using SME.SGP.Infra;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SME.SGP.Aplicacao
{
    public class ObterFotosSemestreAlunoUseCase : AbstractUseCase, IObterFotosSemestreAlunoUseCase
    {
        public ObterFotosSemestreAlunoUseCase(IMediator mediator) : base(mediator)
        {
        }

        public async Task<IEnumerable<ArquivoDto>> Executar(long acompanhamentoSemestreId)
        {
            var miniaturas = await mediator.Send(new ObterMiniaturasFotosSemestreAlunoQuery(acompanhamentoSemestreId));

            return await DownloadMiniaturas(miniaturas);
        }

        private async Task<IEnumerable<ArquivoDto>> DownloadMiniaturas(IEnumerable<Arquivo> miniaturas)
        {
            var arquivos = new List<ArquivoDto>();

            foreach(var miniatura in miniaturas)
            {
                var arquivoFisico = await mediator.Send(new DownloadArquivoCommand(miniatura.Codigo, miniatura.Nome, miniatura.Tipo));

                arquivos.Add(new ArquivoDto()
                {
                    Codigo = miniatura.Codigo,
                    Download = (arquivoFisico, miniatura.TipoConteudo, miniatura.Nome)
                });
            }

            return arquivos;
        }
    }
}
