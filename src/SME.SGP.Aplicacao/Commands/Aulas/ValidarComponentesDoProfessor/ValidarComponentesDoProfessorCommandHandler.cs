﻿using MediatR;
using SME.SGP.Dominio;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using SME.SGP.Dominio.Constantes.MensagensNegocio;

namespace SME.SGP.Aplicacao
{
    public class ValidarComponentesDoProfessorCommandHandler : IRequestHandler<ValidarComponentesDoProfessorCommand, (bool resultado, string mensagem)>
    {
        private readonly IMediator mediator;

        public ValidarComponentesDoProfessorCommandHandler(IMediator mediator)
        {
            this.mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        }

        public async Task<(bool resultado, string mensagem)> Handle(ValidarComponentesDoProfessorCommand request, CancellationToken cancellationToken)
        {
            var podeCriarAulasParaTurma = false;
            if (request.Usuario.EhProfessorCj())
            {
                var componentesCurricularesDoProfessorCJ = await mediator
                    .Send(new ObterComponentesCurricularesDoProfessorCJNaTurmaQuery(request.Usuario.Login));

                podeCriarAulasParaTurma = componentesCurricularesDoProfessorCJ == null ||
                                          !componentesCurricularesDoProfessorCJ.Any(c => c.TurmaId == request.TurmaCodigo && c.DisciplinaId == request.ComponenteCurricularCodigo);

                if (!podeCriarAulasParaTurma)
                    return (false, MensagemNegocioComuns.Voce_nao_pode_criar_aulas_para_essa_turma);
            }
            else
            {
                var componentesCurricularesDoProfessor = await mediator
                    .Send(new ObterComponentesCurricularesDoProfessorNaTurmaQuery(request.TurmaCodigo, request.Usuario.Login, request.Usuario.PerfilAtual, request.Usuario.EhProfessorInfantilOuCjInfantil()));

                if (componentesCurricularesDoProfessor == null)
                    componentesCurricularesDoProfessor = await VerificaPossibilidadeDeTurmaComMotivoErroDeCadastroNoUsuario(request.TurmaCodigo, request.Usuario.Login, request.Usuario.PerfilAtual, request.Usuario.EhProfessorInfantilOuCjInfantil());

                podeCriarAulasParaTurma = componentesCurricularesDoProfessor == null ||
                                          !componentesCurricularesDoProfessor.Any(c => (c.Codigo == request.ComponenteCurricularCodigo && !c.TerritorioSaber || c.CodigoComponenteTerritorioSaber == request.ComponenteCurricularCodigo && c.TerritorioSaber)) &&
                                          !componentesCurricularesDoProfessor.Any(r => r.Regencia && r.CodigoComponenteCurricularPai == request.ComponenteCurricularCodigo);

                if (!podeCriarAulasParaTurma)
                    return (false, MensagemNegocioComuns.Voce_nao_pode_criar_aulas_para_essa_turma);

                if (!request.Usuario.EhGestorEscolar())
                {
                    
                    var usuarioPodePersistirTurmaNaData = await mediator
                        .Send(new ObterUsuarioPossuiPermissaoNaTurmaEDisciplinaQuery(request.ComponenteCurricularCodigo, request.TurmaCodigo, request.Data, request.Usuario));

                    if (!usuarioPodePersistirTurmaNaData)
                        return (false, MensagemNegocioComuns.Voce_nao_pode_fazer_alteracoes_ou_inclusoes_nesta_turma_componente_e_data);
                }
            }

            return (true, string.Empty);
        }

        public async Task<IEnumerable<ComponenteCurricularEol>> VerificaPossibilidadeDeTurmaComMotivoErroDeCadastroNoUsuario(string turmaCodigo, string login, Guid perfilAtual, bool realizaAgrupamento)
         => await mediator.Send(new ObterComponentesCurricularesDoProfessorNaTurmaQuery(turmaCodigo, login, perfilAtual, realizaAgrupamento, false));
    }
}