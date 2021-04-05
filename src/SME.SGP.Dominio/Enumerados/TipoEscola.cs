﻿using System.ComponentModel.DataAnnotations;

namespace SME.SGP.Dominio
{
    public enum TipoEscola
    {
        [Display(Name = "Não Informado", ShortName = "NA")]
        Nenhum = 0,

        [Display(Name = "Escola Municipal de Ensino Fundamental", ShortName = "EMEF")]
        EMEF = 1,

        [Display(Name = "Escola Municipal de Educação Infantil", ShortName = "EMEI")]
        EMEI = 2,

        [Display(Name = "Escola Municipal de Ensino Fundamental e Médio", ShortName = "EMEFM")]
        EMEFM = 3,

        [Display(Name = "Escola Municipal de Ensino Bilíngue para Surdos", ShortName = "EMEBS")]
        EMEBS = 4,

        [Display(Name = "Centro de Educação Infantil Direto ", ShortName = "CEIDIRET")]
        CEIDIRET = 10,

        [Display(Name = "Centro de Educação Infantil Indireto", ShortName = "CEIINDIR")]
        CEIINDIR = 11,

        [Display(Name = "Creche Particular COnveniada", ShortName = "CRPCONV")]
        CRPCONV = 12,

        [Display(Name = "CENTRO INTEGRADO DE EDUCACAO DE JOVENS E ADULTOS", ShortName = "CIEJA")]
        CIEJA = 13,

        [Display(Name = "CENTRO DE CONVIVENCIA INFANTIL /CENTRO INFANTIL DE PROTECAO A SAUDE", ShortName = "CCICIPS")]
        CCICIPS = 14,

        [Display(Name = "ESCOLA PARTICULAR", ShortName = "ESCPART")]
        ESCPART = 15,

        [Display(Name = "Centro Unificado de Educação - Escola Municipal de Ensino Fundamental", ShortName = "CEU EMEF")]
        CEUEMEF = 16,

        [Display(Name = "Centro Unificado de Educação - Escola Municipal de Educação Infantil", ShortName = "CEU EMEI")]
        CEUEMEI = 17,

        [Display(Name = "CENTRO EDUCACIONAL UNIFICADO - CEI", ShortName = "CEUCEI")]
        CEUCEI = 18,

        [Display(Name = "CENTRO EDUCACIONAL UNIFICADO", ShortName = "CEU")]
        CEU = 19,

        [Display(Name = "MOVIMENTO DE ALFABETIZACAO", ShortName = "MOVA")]
        MOVA = 22,

        [Display(Name = "CENTRO MUNICIPAL DE CAPACITACAO E TREIN.", ShortName = "CMCT")]
        CMCT = 23,

        [Display(Name = "ESCOLA TECNICA", ShortName = "ETEC")]
        ETEC = 25,

        [Display(Name = "ESPECIAL CONVENIADA", ShortName = "ESPCONV")]
        ESPCONV = 26,

        [Display(Name = "CEU EXCLUSIVO ATIVIDADE COMPLEMENTAR", ShortName = "CEUATCOMPL")]
        CEUATCOMPL = 27,

        [Display(Name = "CENTRO PARA CRIANCAS E ADOLESCENTES", ShortName = "CCA")]
        CCA = 29,

        [Display(Name = "Centro Municipal de Educação Infantil", ShortName = "CEMEI")]
        CEMEI = 28,

        [Display(Name = "Centro de Educação e Cultura Indígena", ShortName = "CECI")]
        CECI = 30,

        [Display(Name = "Centro Unificado de Educação - Centro Municipal de Educação Infantil", ShortName = "CEU CEMEI")]
        CEUCEMEI = 31
    }
}
