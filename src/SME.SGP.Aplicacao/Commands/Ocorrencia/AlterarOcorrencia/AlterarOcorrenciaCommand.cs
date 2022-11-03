﻿using FluentValidation;
using MediatR;
using SME.SGP.Infra;
using System;
using System.Collections.Generic;

namespace SME.SGP.Aplicacao
{
    public class AlterarOcorrenciaCommand : IRequest<AuditoriaDto>
    {
        public long Id { get; set; }
        public DateTime DataOcorrencia { get; set; }
        public string HoraOcorrencia { get; set; }
        public string Titulo { get; set; }
        public string Descricao { get; set; }
        public long OcorrenciaTipoId { get; set; }
        public IEnumerable<long> CodigosAlunos { get; set; }
        public IEnumerable<string> CodigosServidores { get; set; }
        public bool ConsideraHistorico { get; set; }
        public int AnoLetivo { get; set; }
        public int DreId { get; set; }
        public int UeId { get; set; }
        public int Modalidade { get; set; }
        public int Semestre { get; set; }
        public long TurmaId { get; set; }



        public AlterarOcorrenciaCommand()
        {
            CodigosAlunos = new List<long>();
            CodigosServidores = new List<string>();
        }

        public AlterarOcorrenciaCommand(AlterarOcorrenciaDto dto)
        {
            Id = dto.Id;
            DataOcorrencia = dto.DataOcorrencia;
            HoraOcorrencia = dto.HoraOcorrencia;
            Titulo = dto.Titulo;
            Descricao = dto.Descricao;
            OcorrenciaTipoId = dto.OcorrenciaTipoId;
            CodigosAlunos = dto.CodigosAlunos;
            CodigosServidores = dto.CodigosServidores;
            ConsideraHistorico = dto.ConsideraHistorico;
            AnoLetivo = dto.AnoLetivo;
            DreId = dto.DreId;
            UeId = dto.UeId;
            Modalidade = dto.Modalidade;
            Semestre = dto.Semestre;
            TurmaId = dto.TurmaId;
        }
    }

    public class AlterarOcorrenciaCommandValidator : AbstractValidator<AlterarOcorrenciaCommand>
    {
        public AlterarOcorrenciaCommandValidator()
        {
            RuleFor(x => x.Id)
                .NotEmpty()
                .WithMessage("A ocorrência deve ser informada apra alteração.");

            RuleFor(x => x.DataOcorrencia)
                .NotEmpty()
                .WithMessage("A data da ocorrência deve ser informada.");

            RuleFor(x => x.Descricao)
                .NotEmpty()
                .WithMessage("A descrição da ocorrência deve ser informada.");

            RuleFor(x => x.HoraOcorrencia)
                .Matches("^([01][0-9]|2[0-3]):([0-5][0-9])$")
                .When(x => !string.IsNullOrWhiteSpace(x.HoraOcorrencia))
                .WithMessage("A hora da ocorrência informada é inválida.");

            RuleFor(x => x.OcorrenciaTipoId)
                .NotEmpty()
                .WithMessage("P tipo da ocorrência deve ser informada.");

            RuleFor(x => x.Titulo)
                .NotEmpty()
                .WithMessage("O título da ocorrência deve ser informado.");

            RuleFor(x => x.CodigosAlunos)
                .NotEmpty()
                .WithMessage("Os alunos envolvidos na ocorrência devem ser informados.");

            RuleForEach(x => x.CodigosAlunos)
                .NotEmpty()
                .WithMessage("Um ou mais alunos selecionados são inválidos.");
        }
    }
}
