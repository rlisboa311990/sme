﻿using System;
using System.Collections.Generic;
using System.Text;

namespace SME.SGP.Infra
{
    public class ConselhoDeClasseComponenteSinteseDto
    {
        public string Codigo { get; set; }
        public string Nome { get; set; }
        public int TotalFaltas { get; set; }
        public double PercentualFrequencia { get; set; }
        public string ParecerFinal { get; set; }
    }
}
