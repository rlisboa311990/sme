﻿using System;
using System.Globalization;

namespace SME.SGP.Infra
{
    public class FrequenciaAlunoDto
    {
        public const int PERCENTUAL_FREQUENCIA_PRECISAO = 2;

        public string AlunoCodigo { get; set; }
        public int TotalAulas { get; set; }
        public int TotalAusencias { get; set; }
        public int TotalCompensacoes { get; set; }
        public int TotalPresencas { get; set; }
        public int TotalRemotos { get; set; }
        public DateTime PeriodoFim { get; set; }
        public int NumeroFaltasNaoCompensadas { get => TotalAusencias - TotalCompensacoes; }
        public string PercentualFrequenciaFormatado => FormatarPercentual(PercentualFrequencia);
        public int TotalPresencaRemoto { get => TotalPresencas - TotalRemotos; }
        public double PercentualFrequencia 
        {
            get
            {
                if (TotalAulas == 0)
                    return 0;

                var porcentagem = 100 - ((double)NumeroFaltasNaoCompensadas / TotalAulas) * 100;

                return Math.Round(porcentagem > 100 ? 100 : porcentagem, 2);
            }
        }
        public double TotalFrequencias 
        {
            get
            {
                return TotalPresencas + TotalAusencias + TotalRemotos;
            }
        }

        public string FormatarPercentual(double percentual)
        {
            return percentual.ToString($"N{PERCENTUAL_FREQUENCIA_PRECISAO}", CultureInfo.CurrentCulture);
        }
    }
}
