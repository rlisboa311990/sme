﻿namespace SME.SGP.Infra
{
    /// <summary>
    /// Rotas que não são consumidas em tempo real e sim via agendamento
    /// </summary>
    public static class RotasRabbitSgpAgendamento
    {
        public const string RotaMuralAvisosSync = "sgp.mural.avisos.sync.agendado";
    }
}
