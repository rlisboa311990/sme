﻿namespace SME.SGP.Infra
    public class ObterFrequenciaAlunosPorBimestreDto
    {
        public long ComponenteCurricularId { get; set; }
        public long TurmaId { get; set; }
        public short? Bimestre { get; set; }
    }
}