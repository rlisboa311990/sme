﻿using MediatR;
using SME.SGP.Dominio;
using SME.SGP.Dominio.Interfaces;
using SME.SGP.Infra;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SME.SGP.Aplicacao
{
    public class ObterOcorrenciaPorIdQueryHandler : IRequestHandler<ObterOcorrenciaPorIdQuery, OcorrenciaDto>
    {
        private readonly IRepositorioOcorrencia repositorioOcorrencia;
        private readonly IRepositorioOcorrenciaAluno repositorioOcorrenciaAluno;
        private readonly IRepositorioOcorrenciaTipo repositorioOcorrenciaTipo;
        private readonly IMediator mediator;

        public ObterOcorrenciaPorIdQueryHandler(IRepositorioOcorrencia repositorioOcorrencia, IMediator mediator,
                                                IRepositorioOcorrenciaAluno repositorioOcorrenciaAluno,
                                                IRepositorioOcorrenciaTipo repositorioOcorrenciaTipo)
        {
            this.repositorioOcorrencia = repositorioOcorrencia ?? throw new ArgumentNullException(nameof(repositorioOcorrencia));
            this.mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            this.repositorioOcorrenciaAluno = repositorioOcorrenciaAluno ?? throw new ArgumentNullException(nameof(repositorioOcorrenciaAluno));
            this.repositorioOcorrenciaTipo = repositorioOcorrenciaTipo ?? throw new ArgumentNullException(nameof(repositorioOcorrenciaTipo));
        }

        public async Task<OcorrenciaDto> Handle(ObterOcorrenciaPorIdQuery request, CancellationToken cancellationToken)
        {
            var ocorrencia = await repositorioOcorrencia.ObterPorIdAsync(request.Id);
            if (ocorrencia is null)
                throw new NegocioException($"Não foi possível localizar a ocorrência {request.Id}.");

            var alunos = await ObterAlunos(ocorrencia);
            var servidores = await ObterServidores(ocorrencia);

            return MapearParaDto(ocorrencia, alunos, servidores);
        }

        private async Task<IEnumerable<UsuarioEolRetornoDto>> ObterServidores(Ocorrencia ocorrencia)
        {
            var codigosServidor = ocorrencia.Servidores.Select(servidor => servidor.CodigoServidor).ToArray();

            if (codigosServidor.Any())
            {
                var dtoUe = await mediator.Send(new ObterCodigoUEDREPorIdQuery(ocorrencia.UeId));
                return await mediator.Send(new ObterFuncionariosPorUeQuery(dtoUe.UeCodigo, codigosServidor));
            }

            return Enumerable.Empty<UsuarioEolRetornoDto>();
        }

        private async Task<IEnumerable<TurmasDoAlunoDto>> ObterAlunos(Ocorrencia ocorrencia)
        {
            var codigosAlunos = ocorrencia.Alunos.Select(aluno => aluno.CodigoAluno).ToArray();

            if (codigosAlunos.Any())
            {
                return await mediator.Send(new ObterAlunosEolPorCodigosQuery(codigosAlunos));
            }

            return Enumerable.Empty<TurmasDoAlunoDto>();
        }

        private OcorrenciaDto MapearParaDto(Ocorrencia ocorrencia, IEnumerable<TurmasDoAlunoDto> alunos, IEnumerable<UsuarioEolRetornoDto> servidores) 
            => new OcorrenciaDto()
            {
                Auditoria = (AuditoriaDto)ocorrencia,
                DataOcorrencia = ocorrencia.DataOcorrencia,
                Descricao = ocorrencia.Descricao,
                HoraOcorrencia = ocorrencia.HoraOcorrencia?.ToString(@"hh\:mm") ?? string.Empty,
                OcorrenciaTipoId = ocorrencia.OcorrenciaTipoId.ToString(),
                TurmaId = ocorrencia.TurmaId,
                Titulo = ocorrencia.Titulo,
                DreId = ocorrencia.Ue.DreId,
                AnoLetivo = ocorrencia.Turma!.AnoLetivo,
                Alunos = ocorrencia.Alunos.Select(ao => new OcorrenciaAlunoDto()
                {
                    Id = ao.Id,
                    CodigoAluno = ao.CodigoAluno,
                    Nome = alunos.FirstOrDefault(a => a.CodigoAluno == ao.CodigoAluno)?.NomeAluno
                }),
                Servidores = ocorrencia.Servidores.Select(ao => new OcorrenciaServidorDto()
                {
                    Id = ao.Id,
                    CodigoServidor = ao.CodigoServidor,
                    Nome = servidores.FirstOrDefault(servidor => servidor.CodigoRf == ao.CodigoServidor)?.NomeServidor
                })
            };
    }
}

