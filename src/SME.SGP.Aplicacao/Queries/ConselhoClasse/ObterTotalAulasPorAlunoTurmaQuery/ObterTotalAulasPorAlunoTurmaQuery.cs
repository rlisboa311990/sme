﻿using FluentValidation;
using MediatR;
using SME.SGP.Infra.Dtos.ConselhoClasse;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SME.SGP.Aplicacao.Queries.ConselhoClasse.ObterTotalAulasPorAlunoTurmaQuery
{
    public class ObterTotalAulasPorAlunoTurmaQuery : IRequest<IEnumerable<TotalAulasPorAlunoTurmaDto>>
    {
        public ObterTotalAulasPorAlunoTurmaQuery(string codigoAluno, string codigoTurma)
        {
            CodigoAluno = codigoAluno;
            CodigoTurma = codigoTurma;
        }
        public string CodigoAluno { get; set; }
        public string CodigoTurma { get; set; }
    }

    public class ObterTotalAulasPorAlunoTurmaQueryValidator : AbstractValidator<ObterTotalAulasPorAlunoTurmaQuery>
    {
        public ObterTotalAulasPorAlunoTurmaQueryValidator()
        {
            RuleFor(x => x.CodigoAluno).NotEmpty().WithMessage("É necessário informar o código do aluno para calcular o seu total de aulas.");
            RuleFor(x => x.CodigoTurma).NotEmpty().WithMessage("É necessário informar o código da turma para calcular o seu total de aulas.");
        }
    }
}
