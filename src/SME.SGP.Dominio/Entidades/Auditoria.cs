﻿using System;

namespace SME.SGP.Dominio.Entidades
{
    public class Auditoria
    {
        public string Acao { get; set; }
        public long Chave { get; set; }
        public DateTime Data { get; set; }
        public string Entidade { get; set; }
        public string Usuario { get; set; }
    }
}