﻿namespace SME.SGP.Infra
{
    public class RegistroItineranciaQuestaoDto
    {
        public long Id { get; set; }
        public long QuestaoId { get; set; }
        public string Descricao { get; set; }
        public string Resposta { get; set; }
        public long RegistroItineranciaId { get; set; }
        public bool Obrigatorio { get; set; }
    }
}