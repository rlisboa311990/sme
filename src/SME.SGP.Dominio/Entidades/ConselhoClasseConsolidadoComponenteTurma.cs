﻿using System;

namespace SME.SGP.Dominio
{
    public class ConselhoClasseConsolidadoComponenteTurma : EntidadeBase
    {
        public DateTime DataAtualizacao { get; set; }

        public StatusFechamento Status { get; set; }

        public string AlunoCodigo { get; set; }

        public long ParecerConclusivoId { get; set; }

        public long TurmaId { get; set; }

        public int Bimestre { get; set; }
    }
}
