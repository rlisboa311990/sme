﻿using FluentValidation;
using MediatR;
using SME.SGP.Dominio;

namespace SME.SGP.Aplicacao
{
    public class NotificarExclusaoNotificacaoCommand : IRequest
    {
        public NotificarExclusaoNotificacaoCommand(long codigo, string usuarioRf)
        {
            Codigo = codigo;
            UsuarioRf = usuarioRf;
        }

        public long Codigo { get; }
        public string UsuarioRf { get; }
    }

    public class NotificarExclusaoNotificacaoCommandValidator : AbstractValidator<NotificarExclusaoNotificacaoCommand>
    {
        public NotificarExclusaoNotificacaoCommandValidator()
        {
            RuleFor(x => x.Codigo)
                .NotEmpty()
                .WithMessage("O código da notificação deve ser informado para notificar sua exclusão");

            RuleFor(x => x.UsuarioRf)
                .NotEmpty()
                .WithMessage("O usuário da notificação deve ser informado para notificar sua exclusão");
        }
    }
}
