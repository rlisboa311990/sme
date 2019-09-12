﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace SME.SGP.Dominio
{
    public enum Cargo
    {
        [Display(Name = "Coordenador Pedagógico")]
        CP = 3379,
        [Display(Name = "Assistente Diretor")]
        AD = 3085,
        [Display(Name = "Diretor de Escola")]
        Diretor = 3360,
        [Display(Name = "Supervisor Escolar")]
        Supervisor = 3352

    }
}
