﻿using MediatR;
using SME.SGP.Dominio;
using SME.SGP.Infra;
using System.Collections.Generic;

namespace SME.SGP.Aplicacao
{
    public class ObterComponentesCurricularesEOLPorTurmasCodigoQuery : IRequest<IEnumerable<ComponenteCurricularEol>>
    {
        public ObterComponentesCurricularesEOLPorTurmasCodigoQuery(string[] codigosDeTurmas, bool? adicionarComponentesPlanejamento = null)
        {
            CodigosDeTurmas = codigosDeTurmas;
            AdicionarComponentesPlanejamento = adicionarComponentesPlanejamento;
        }

        public string[] CodigosDeTurmas { get; set; }
        public bool? AdicionarComponentesPlanejamento { get; private set; }
    }
}
