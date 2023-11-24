﻿using MediatR;
using SME.SGP.Aplicacao;
using SME.SGP.Dominio;
using SME.SGP.Dominio.Enumerados;
using SME.SGP.Infra;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SME.SGP.TesteIntegracao.PlanoAEE.ServicosFake
{
    public class ObterAlunosEolPorCodigosQueryHandlerFake_AnoPosterior : IRequestHandler<ObterAlunosEolPorCodigosQuery, IEnumerable<TurmasDoAlunoDto>>
    {
        private const int ALUNO_1 = 1;
        private const int ALUNO_2 = 2;
        public Task<IEnumerable<TurmasDoAlunoDto>> Handle(ObterAlunosEolPorCodigosQuery request, CancellationToken cancellationToken)
        {
            var dataAnoPosterior = DateTimeExtension.HorarioBrasilia().AddYears(1);
            var dataAnoAnterior = DateTimeExtension.HorarioBrasilia().AddYears(-1);
            var lista = new List<TurmasDoAlunoDto>()
            {
                new TurmasDoAlunoDto
                {
                    CodigoAluno = ALUNO_1,
                    NomeAluno = $"Nome do Aluno {ALUNO_1} ",
                    NomeSocialAluno = $"Nome Social do Aluno {ALUNO_1}",
                    CodigoSituacaoMatricula = (int)SituacaoMatriculaAluno.Ativo,
                    DataSituacao = DateTimeExtension.HorarioBrasilia(),
                    NumeroAlunoChamada = 1,
                    CodigoTurma = 1,
                    AnoLetivo= DateTimeExtension.HorarioBrasilia().Year,
                    CodigoTipoTurma= (int)TipoTurma.Regular,
                },
                new TurmasDoAlunoDto
                {
                    CodigoAluno = ALUNO_1,
                    NomeAluno = $"Nome do Aluno {ALUNO_1} ",
                    NomeSocialAluno = $"Nome Social do Aluno {ALUNO_1}",
                    CodigoSituacaoMatricula = (int)SituacaoMatriculaAluno.Ativo,
                    DataSituacao = dataAnoPosterior,
                    NumeroAlunoChamada = 1,
                    CodigoTurma = 2,
                    AnoLetivo= dataAnoPosterior.Year,
                    CodigoTipoTurma= (int)TipoTurma.Regular,
                },

                new TurmasDoAlunoDto
                {
                    CodigoAluno = ALUNO_2,
                    NomeAluno = $"Nome do Aluno {ALUNO_2} ",
                    NomeSocialAluno = $"Nome Social do Aluno {ALUNO_2}",
                    CodigoSituacaoMatricula = (int)SituacaoMatriculaAluno.Ativo,
                    DataSituacao = dataAnoAnterior,
                    NumeroAlunoChamada = 1,
                    CodigoTurma = 1,
                    AnoLetivo= dataAnoAnterior.Year,
                    CodigoTipoTurma= (int)TipoTurma.Regular,
                },
                new TurmasDoAlunoDto
                {
                    CodigoAluno = ALUNO_2,
                    NomeAluno = $"Nome do Aluno {ALUNO_2} ",
                    NomeSocialAluno = $"Nome Social do Aluno {ALUNO_2}",
                    CodigoSituacaoMatricula = (int)SituacaoMatriculaAluno.Ativo,
                    DataSituacao = DateTimeExtension.HorarioBrasilia(),
                    NumeroAlunoChamada = 1,
                    CodigoTurma = 2,
                    AnoLetivo= DateTimeExtension.HorarioBrasilia().Year,
                    CodigoTipoTurma= (int)TipoTurma.Regular,
                }
            };

            var resultado = lista.FindAll(aluno => aluno.CodigoAluno == request.CodigosAluno.FirstOrDefault());

            return Task.FromResult<IEnumerable<TurmasDoAlunoDto>>(resultado);
        }
    }
}
