﻿using System;

namespace SME.SGP.Dominio
{
    public class Documento : EntidadeBase
    {
        public long ClassificacaoDocumentoId { get; set; }
        public ClassificacaoDocumento ClassificacaoDocumento { get; set; }
        public string UsuarioRf { get; set; }
        public long ArquivoId { get; set; }
        public Arquivo Arquivo { get; set; }

    }
}