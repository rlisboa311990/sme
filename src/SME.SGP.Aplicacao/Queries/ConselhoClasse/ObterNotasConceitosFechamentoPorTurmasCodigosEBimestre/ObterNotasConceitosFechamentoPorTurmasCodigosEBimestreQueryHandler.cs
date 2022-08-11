using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using SME.SGP.Dominio.Constantes;
using SME.SGP.Dominio.Interfaces;
using SME.SGP.Infra;

namespace SME.SGP.Aplicacao;

public class ObterNotasConceitosFechamentoPorTurmasCodigosEBimestreQueryHandler : IRequestHandler<ObterNotasConceitosFechamentoPorTurmasCodigosEBimestreQuery, IEnumerable<NotaConceitoBimestreComponenteDto>>
{
    private readonly IRepositorioConselhoClasseNotaConsulta repositorioConselhoClasseNota;
    private readonly IRepositorioCache repositorioCache;

    public ObterNotasConceitosFechamentoPorTurmasCodigosEBimestreQueryHandler(IRepositorioConselhoClasseNotaConsulta repositorioConselhoClasseNota, IRepositorioCache repositorioCache)
    {
        this.repositorioConselhoClasseNota = repositorioConselhoClasseNota ?? throw new ArgumentNullException(nameof(repositorioConselhoClasseNota));        
        this.repositorioCache = repositorioCache ?? throw new ArgumentNullException(nameof(repositorioCache));
    }

    public async Task<IEnumerable<NotaConceitoBimestreComponenteDto>> Handle(ObterNotasConceitosFechamentoPorTurmasCodigosEBimestreQuery request, CancellationToken cancellationToken)
    {
        var retorno = new List<NotaConceitoBimestreComponenteDto>();

        foreach (var turmaCodigo in request.TurmasCodigos)
        {
            var notasConceitosFechamento = (await repositorioCache.ObterAsync(string.Format(NomeChaveCache.CHAVE_NOTA_CONCEITO_FECHAMENTO_TURMA_BIMESTRE, turmaCodigo, request.Bimestre),
                async () => await repositorioConselhoClasseNota.ObterNotasConceitosFechamentoPorTurmaCodigoEBimestreAsync(turmaCodigo, request.Bimestre))).ToList();
        
            if (notasConceitosFechamento.Any())
                retorno.AddRange(notasConceitosFechamento);
        }

        return await Task.FromResult(retorno);
    }
}