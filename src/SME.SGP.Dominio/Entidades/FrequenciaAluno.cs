﻿using System;

namespace SME.SGP.Dominio
{
    public class FrequenciaAluno : EntidadeBase
    {
        public FrequenciaAluno(string codigoAluno,
                 string disciplinaId,
                 DateTime periodoInicio,
                 DateTime periodoFim,
                 int bimestre,
                 int totalAusencias,
                 int totalAulas,
                 TipoFrequenciaAluno tipo)
        {
            Bimestre = bimestre;
            CodigoAluno = codigoAluno;
            DisciplinaId = disciplinaId;
            PeriodoFim = periodoFim;
            PeriodoInicio = periodoInicio;
            TotalAulas = totalAulas;
            Tipo = tipo;
            TotalAusencias = totalAusencias;
        }

        protected FrequenciaAluno()
        {
        }

        public int Bimestre { get; set; }
        public string CodigoAluno { get; set; }
        public string DisciplinaId { get; set; }
        public double PercentualFrequencia => 100 - ((TotalAusencias / TotalAulas) * 100);
        public DateTime PeriodoFim { get; set; }
        public DateTime PeriodoInicio { get; set; }
        public TipoFrequenciaAluno Tipo { get; set; }
        public double TotalAulas { get; set; }
        public double TotalAusencias { get; set; }

        public double NumeroFaltasNaoCompensadas { get => TotalAusencias; }

        public FrequenciaAluno DefinirFrequencia(int totalAusencias, int totalAulas, TipoFrequenciaAluno tipoFrequencia)
        {
            Tipo = tipoFrequencia;
            TotalAusencias = totalAusencias;
            TotalAulas = totalAulas;
            return this;
        }
    }
}