﻿using FluentValidation;
using MediatR;
using SME.SGP.Infra;
using System;
using System.Collections.Generic;
using System.Text;

namespace SME.SGP.Aplicacao
{
    public class AlterarPlanoAEEObservacaoCommand : IRequest<AuditoriaDto>
    {
        public AlterarPlanoAEEObservacaoCommand(long id, string observacao, IEnumerable<long> usuarios)
        {
            Id = id;
            Observacao = observacao;
            Usuarios = usuarios;
        }

        public long Id { get; }
        public string Observacao { get; }
        public IEnumerable<long> Usuarios { get; }
    }

    public class AlterarPlanoAEEObservacaoCommandValidator : AbstractValidator<AlterarPlanoAEEObservacaoCommand>
    {
        public AlterarPlanoAEEObservacaoCommandValidator()
        {
            RuleFor(a => a.Id)
                .NotEmpty()
                .WithMessage("O Id da observação deve ser informado para alteração");

            RuleFor(a => a.Observacao)
                .NotEmpty()
                .WithMessage("A observação deve ser informada para alteração");
        }
    }
}
