﻿namespace SME.SGP.Infra
{
    public class DisciplinaDto
    {
        public long CodigoComponenteCurricular { get; set; }
        public bool Compartilhada { get; set; }
        public string Nome { get; set; }
        public bool PossuiObjetivos { get; set; }
        public bool Regencia { get; set; }
        public bool RegistraFrequencia { get; set; }
        public bool TerritorioSaber { get; set; }
    }
}