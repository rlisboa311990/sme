﻿using System;

namespace SME.SGP.Infra
{
    public class AulasPorTurmaDisciplinaDto
    {
        public DateTime DataAula { get; set; }
        public long Id { get; set; }
        public int ProfessorId { get; set; }
        public int Quantidade { get; set; }
    }
}