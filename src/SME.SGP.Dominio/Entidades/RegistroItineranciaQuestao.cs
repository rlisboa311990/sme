﻿namespace SME.SGP.Dominio
{
    public class RegistroItineranciaQuestao : EntidadeBase
    {
        public long QuestaoId { get; set; }
        public string Resposta { get; set; }
        public bool Excluido { get; set; }
        public long RegistroItineranciaAlunoId { get; set; }
    }
}