﻿using FluentValidation;
using MediatR;
using SME.SGP.Dominio;
using System;
using System.Collections.Generic;
using System.Text;

namespace SME.SGP.Aplicacao
{
    public class SalvarPendenciaPerfilCommand : IRequest<List<long>>
    {
        public SalvarPendenciaPerfilCommand(long pendenciaId, List<int> perfisCodigo)
        {
            PendenciaId = pendenciaId;
            PerfisCodigo = perfisCodigo;
        }

        public long PendenciaId { get; set; }
        public List<int> PerfisCodigo { get; set; }
    }

    public class SalvarPendenciaPerfilCommandValidator : AbstractValidator<SalvarPendenciaPerfilCommand>
    {
        public SalvarPendenciaPerfilCommandValidator()
        {
            RuleFor(c => c.PendenciaId)
            .NotEmpty()
            .WithMessage("O id de pendência deve ser informado para geração de pendência.");

            RuleFor(c => c.PerfisCodigo)
           .NotEmpty()
           .WithMessage("Os códigos dos perfis deve ser informado para geração de pendência.");

        }
    }

}
