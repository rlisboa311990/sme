﻿using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using SME.SGP.Aplicacao;
using SME.SGP.Infra;

namespace SME.SGP.TesteIntegracao.PlanoAula.ServicosFakes
{
    public class ObterAlunosPorCodigoEolNomeQueryHandlerFake : IRequestHandler<ObterAlunosPorCodigoEolNomeQuery, IEnumerable<AlunoSimplesDto>>
    {
        public async Task<IEnumerable<AlunoSimplesDto>> Handle(ObterAlunosPorCodigoEolNomeQuery request, CancellationToken cancellationToken)
        {
            var listaRetorno = new List<AlunoSimplesDto>
            {
               new AlunoSimplesDto()
               {

               }   
            };
            return listaRetorno;
        }
    }
}