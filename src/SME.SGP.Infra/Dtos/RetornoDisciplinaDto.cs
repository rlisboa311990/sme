﻿using Newtonsoft.Json;

namespace SME.SGP.Infra
{
    public class RetornoDisciplinaDto
    {
        public int CdComponenteCurricular { get; set; }
        [JsonProperty("codDisciplinaPai")]
        public int? CdComponenteCurricularPai { get; set; }
        public string Descricao { get; set; }
        public bool EhCompartilhada { get; set; }
        public bool EhRegencia { get; set; }
        public bool RegistraFrequencia { get; set; }
    }
}