﻿using System;
using FluentValidation;
using MediatR;
using SME.SGP.Dominio;
using System.Collections.Generic;
using SME.SGP.Infra;

namespace SME.SGP.Aplicacao
{
    public class ObterFrequenciasAlunoComponentePorTurmasQuery : IRequest<IEnumerable<FrequenciaAluno>>
    {
        public ObterFrequenciasAlunoComponentePorTurmasQuery(string codigoAluno, string[] codigosTurmas, long tipoCalendarioId, IEnumerable<AlunoPorTurmaResposta> informacoesAluno, int bimestre = 0)
        {
            CodigoAluno = codigoAluno;
            CodigosTurmas = codigosTurmas;
            TipoCalendarioId = tipoCalendarioId;
            Bimestre = bimestre;
            InformacoesAluno = informacoesAluno;
        }

        public string CodigoAluno { get; }
        public string[] CodigosTurmas { get; }
        public long TipoCalendarioId { get; }
        public int Bimestre { get; }
        public IEnumerable<AlunoPorTurmaResposta> InformacoesAluno { get; }
    }

    public class ObterFrequenciasAlunoComponentePorTurmasQueryValidator : AbstractValidator<ObterFrequenciasAlunoComponentePorTurmasQuery>
    {
        public ObterFrequenciasAlunoComponentePorTurmasQueryValidator()
        {
            RuleFor(a => a.CodigoAluno)
                .NotEmpty()
                .WithMessage("O código do aluno deve ser informado para consulta de sua frequêncial anual");

            RuleFor(a => a.CodigosTurmas)
                .NotEmpty()
                .WithMessage("Os códigos de turmas devem ser informados para consulta da frequêncial anual do aluno");

            RuleFor(a => a.TipoCalendarioId)
                .NotEmpty()
                .WithMessage("O tipo de calendario devem ser informados para consulta da frequêncial anual do aluno");
        }
    }
}
