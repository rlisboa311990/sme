﻿namespace SME.SGP.Infra
{
    public class RetornoDisciplinaDto
    {
        public long CdComponenteCurricular { get; set; }
        public string Descricao { get; set; }
        public bool EhCompartilhada { get; set; }
        public bool EhRegencia { get; set; }
        public bool RegistraFrequencia { get; set; }
        public bool Territorio { get; set; }
    }
}