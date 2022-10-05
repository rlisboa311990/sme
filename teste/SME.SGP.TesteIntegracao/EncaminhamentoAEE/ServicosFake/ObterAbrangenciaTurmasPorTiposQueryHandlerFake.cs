﻿using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using SME.SGP.Aplicacao;
using SME.SGP.Dominio;
using SME.SGP.Dominio.Enumerados;
using SME.SGP.Dto;

namespace SME.SGP.TesteIntegracao.EncaminhamentoAEE.ServicosFake
{
    public class ObterAbrangenciaTurmasPorTiposQueryHandlerFake : IRequestHandler<ObterAbrangenciaTurmasPorTiposQuery, IEnumerable<AbrangenciaTurmaRetorno>>
    {
        public async Task<IEnumerable<AbrangenciaTurmaRetorno>> Handle(ObterAbrangenciaTurmasPorTiposQuery request, CancellationToken cancellationToken)
        {
            var lista = new List<AbrangenciaTurmaRetorno>()
            {
                new AbrangenciaTurmaRetorno
                {
                    Id = 1,
                    Semestre = 1,
                    TipoTurma = (int)TipoTurma.ItinerarioEnsMedio,
                    EnsinoEspecial = false,
                    Nome = "Nome teste",
                    Ano = "8",
                    AnoLetivo = request.AnoLetivo,
                    CodigoModalidade = (int)Modalidade.Medio,
                    Codigo = "1",
                }
            };

            return lista;
        }
    }
}