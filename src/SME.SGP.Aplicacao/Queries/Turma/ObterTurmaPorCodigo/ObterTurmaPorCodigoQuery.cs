﻿using MediatR;
using SME.SGP.Dominio;

namespace SME.SGP.Aplicacao
{
    public class ObterTurmaPorCodigoQuery : IRequest<Turma>
    {
        public ObterTurmaPorCodigoQuery() { }
        public ObterTurmaPorCodigoQuery(string turmaCodigo)
        {
            TurmaCodigo = turmaCodigo;
        }

        public string TurmaCodigo { get; set; }
    }
}
