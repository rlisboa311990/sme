﻿using FluentValidation;
using MediatR;

namespace SME.SGP.Aplicacao
{
    public class SalvarPendenciaUsuarioCommand : IRequest<bool>
    {
        public SalvarPendenciaUsuarioCommand(long pendenciaId, long usuarioId)
        {
            PendenciaId = pendenciaId;
            UsuarioId = usuarioId;
        }

        public long PendenciaId { get; set; }
        public long UsuarioId { get; set; }
    }
    public class SalvarPendenciaUsuarioCommandValidator : AbstractValidator<SalvarPendenciaUsuarioCommand>
    {
        public SalvarPendenciaUsuarioCommandValidator()
        {
            RuleFor(c => c.PendenciaId)
            .NotEmpty()
            .WithMessage("O id da pendencia deve ser informado.");

            RuleFor(c => c.UsuarioId)
            .NotEmpty()
            .WithMessage("O id do usuario deve ser informado.");
        }

    }

}
