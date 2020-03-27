﻿using System.Collections.Generic;

namespace SME.SGP.Infra
{
    public class NotaConceitoAlunoBimestreDto
    {
        public bool Ativo { get; set; }
        public string Informacao { get; set; }
        public string Nome { get; set; }
        public IEnumerable<NotaConceitoBimestreRetornoDto> Notas { get; set; }
        public string CodigoAluno { get; set; }
        public int NumeroChamada { get; set; }
        public double PercentualFrequencia { get; set; }
        public double QuantidadeCompensacoes { get; set; }
        public double QuantidadeFaltas { get; set; }
        public int SinteseId { get; set; }
        public string Sintese { get; set; }
    }
}