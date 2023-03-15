﻿using MediatR;
using SME.SGP.Aplicacao.Integracoes;
using SME.SGP.Dominio;
using SME.SGP.Dominio.Enumerados;
using SME.SGP.Dominio.Interfaces;
using SME.SGP.Infra;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SME.SGP.Aplicacao
{
    public class ObterComponentesCurricularesPorIdsQueryHandler : IRequestHandler<ObterComponentesCurricularesPorIdsQuery, IEnumerable<DisciplinaDto>>
    {
        private readonly IRepositorioComponenteCurricularConsulta repositorioComponenteCurricular;
        private readonly IServicoEol servicoEol;
        private readonly IMediator mediator;

        public ObterComponentesCurricularesPorIdsQueryHandler(IRepositorioComponenteCurricularConsulta repositorioComponenteCurricular, IServicoEol servicoEol, IMediator mediator)
        {
            this.repositorioComponenteCurricular = repositorioComponenteCurricular ?? throw new System.ArgumentNullException(nameof(repositorioComponenteCurricular));
            this.servicoEol = servicoEol ?? throw new System.ArgumentNullException(nameof(servicoEol));
            this.mediator = mediator ?? throw new System.ArgumentNullException(nameof(mediator));
        }

        public async Task<IEnumerable<DisciplinaDto>> Handle(ObterComponentesCurricularesPorIdsQuery request, CancellationToken cancellationToken)
        {
            var usuarioLogado = await RetornarUsuario();
            var disciplinasRetorno = new List<DisciplinaDto>();

            var disciplinasUsuario = await mediator
                .Send(new ObterComponentesCurricularesDoProfessorNaTurmaQuery(request.CodigoTurma, usuarioLogado.Login, usuarioLogado.PerfilAtual));

            var disciplinasAgrupadas = await servicoEol
                    .ObterDisciplinasPorIdsAgrupadas(request.Ids, request.CodigoTurma);            

            if (request.PossuiTerritorio.HasValue && request.PossuiTerritorio.Value && !usuarioLogado.EhProfessorCj())
            {
                var listaDisciplinas = new List<DisciplinaDto>();

                foreach (var disciplina in disciplinasAgrupadas)
                {
                    var disciplinaCorrespondente = disciplinasUsuario
                        .FirstOrDefault(du => du.Codigo.Equals(disciplina.CodigoComponenteCurricular) || du.CodigoComponenteTerritorioSaber.Equals(disciplina.CodigoComponenteCurricular));

                    disciplina.RegistraFrequencia = await mediator.Send(new ObterComponenteRegistraFrequenciaQuery(disciplinaCorrespondente?.CodigoComponenteTerritorioSaber > 0 ? disciplinaCorrespondente.CodigoComponenteTerritorioSaber : disciplina.CodigoComponenteCurricular));
                    listaDisciplinas.Add(disciplina);
                }
            }
            else
            {
                foreach (var id in request.Ids)
                {
                    var disciplinaCorreposdente = disciplinasUsuario
                        .FirstOrDefault(d => d.Codigo.Equals(id) || d.CodigoComponenteTerritorioSaber.Equals(id));

                    if (disciplinaCorreposdente != null)
                    {
                        var registraFrequencia = await mediator
                            .Send(new ObterComponenteRegistraFrequenciaQuery(disciplinaCorreposdente != null && disciplinaCorreposdente.CodigoComponenteTerritorioSaber > 0 ? disciplinaCorreposdente.CodigoComponenteTerritorioSaber : disciplinaCorreposdente.Codigo));

                        disciplinasRetorno.Add(new DisciplinaDto()
                        {
                            Id = disciplinaCorreposdente.CodigoComponenteTerritorioSaber,
                            CodigoComponenteCurricular = disciplinaCorreposdente.Codigo,
                            CdComponenteCurricularPai = disciplinaCorreposdente.CodigoComponenteCurricularPai,
                            Compartilhada = disciplinaCorreposdente.Compartilhada,
                            Nome = disciplinaCorreposdente.Descricao,
                            NomeComponenteInfantil = disciplinaCorreposdente.Descricao,
                            PossuiObjetivos = disciplinaCorreposdente.PossuiObjetivos,
                            Regencia = disciplinaCorreposdente.Regencia,
                            RegistraFrequencia = registraFrequencia,
                            TerritorioSaber = disciplinaCorreposdente.TerritorioSaber,
                            LancaNota = disciplinaCorreposdente.LancaNota,
                            TurmaCodigo = disciplinaCorreposdente.TurmaCodigo
                        });
                    }
                    else
                    {
                        var disciplina = disciplinasAgrupadas.FirstOrDefault(da => da.CodigoComponenteCurricular.Equals(id)) ?? (await repositorioComponenteCurricular
                            .ObterDisciplinasPorIds(new long[] { id })).FirstOrDefault();

                        if (disciplina != null)
                        {
                            disciplina.RegistraFrequencia = await mediator
                                .Send(new ObterComponenteRegistraFrequenciaQuery(disciplina.CodigoComponenteCurricular));

                            disciplinasRetorno.Add(disciplina);
                        }
                    }
                }
            }

            return disciplinasRetorno;
        }

        private async Task<Usuario> RetornarUsuario()
        {
            try
            {
                return await mediator.Send(new ObterUsuarioLogadoQuery());
            }
            catch (Exception ex)
            {
                await mediator.Send(new SalvarLogViaRabbitCommand($"Erro ao obter usuario obter componentes curriculares por ids Motivo: {ex.Message}", LogNivel.Critico, LogContexto.Usuario, ex.Message));
                throw;
            }
        }
    }
}
