﻿using MediatR;
using SME.SGP.Dominio;
using SME.SGP.Infra;
using System.Collections.Generic;

namespace SME.SGP.Aplicacao
{
    public class ObterTiposCalendarioPorAnoLetivoDescricaoEModalidadesQuery : IRequest<IEnumerable<TipoCalendarioRetornoDto>>
    {
        public ObterTiposCalendarioPorAnoLetivoDescricaoEModalidadesQuery(int anoLetivo, Modalidade[] modalidades, string descricao)
        {
            AnoLetivo = anoLetivo;
            Modalidades = modalidades;
            Descricao = descricao;
        }

        public int AnoLetivo { get; set; }
        public Modalidade[] Modalidades { get; set; }
        public string Descricao { get; set; }
    }
}
