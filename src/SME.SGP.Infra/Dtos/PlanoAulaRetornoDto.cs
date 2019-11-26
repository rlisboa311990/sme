﻿using SME.SGP.Dominio;
using System.Collections.Generic;

namespace SME.SGP.Infra
{
    public class PlanoAulaRetornoDto
    {
        public PlanoAulaRetornoDto()
        {
            ObjetivosAprendizagemAula = new List<long>();
        }

        public long Id { get; set; }
        public string Descricao { get; set; }
        public string DesenvolvimentoAula { get; set; }
        public string RecuperacaoAula { get; set; }
        public string LicaoCasa { get; set; }
        public long AulaId { get; set; }
        public int QtdAulas { get; set; }

        public List<long> ObjetivosAprendizagemAula { get; set; }
    }
}
