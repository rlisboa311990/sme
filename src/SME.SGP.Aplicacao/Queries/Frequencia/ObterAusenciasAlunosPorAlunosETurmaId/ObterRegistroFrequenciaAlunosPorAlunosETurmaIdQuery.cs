﻿using FluentValidation;
using MediatR;
using SME.SGP.Infra;
using System;
using System.Collections.Generic;

namespace SME.SGP.Aplicacao
{
    public class ObterRegistroFrequenciaAlunosPorAlunosETurmaIdQuery : IRequest<IEnumerable<RegistroFrequenciaPorDisciplinaAlunoDto>>
    {
        public ObterRegistroFrequenciaAlunosPorAlunosETurmaIdQuery(DateTime dataAula, IEnumerable<(string codigo, DateTime dataMatricula, DateTime? dataSituacao)> alunos, params string[] turmasId)
        {
            DataAula = dataAula;
            Alunos = alunos;
            TurmasId = turmasId;
        }

        public DateTime DataAula { get; set; }
        public IEnumerable<(string codigo, DateTime dataMatricula, DateTime? dataSituacao)> Alunos { get; set; }
        public string[] TurmasId { get; set; }
    }

    public class ObterRegistroFrequenciaAlunosPorAlunosETurmaIdQueryValidator : AbstractValidator<ObterRegistroFrequenciaAlunosPorAlunosETurmaIdQuery>
    {
        public ObterRegistroFrequenciaAlunosPorAlunosETurmaIdQueryValidator()
        {
            RuleFor(a => a.DataAula)
                .NotEmpty()
                .WithMessage("A data da aula precisa ser informada");
            RuleFor(a => a.Alunos)
                .NotEmpty()
                .WithMessage("Os alunos precisam ser informados");
            RuleFor(a => a.TurmasId)
                .NotEmpty()
                .WithMessage("A turma precisa ser informada");
        }
    }
}
