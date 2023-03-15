﻿using MediatR;
using SME.SGP.Aplicacao.Interfaces;
using SME.SGP.Dominio;
using SME.SGP.Dominio.Enumerados;
using SME.SGP.Infra;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SME.SGP.Aplicacao
{
    public class ExcluirAulaUseCase : AbstractUseCase, IExcluirAulaUseCase
    {
        public ExcluirAulaUseCase(IMediator mediator) : base(mediator)
        {
        }

        public async Task<RetornoBaseDto> Executar(ExcluirAulaDto excluirDto)
        {            
            var aula = await mediator.Send(new ObterAulaPorIdQuery(excluirDto.AulaId));

            if (aula == null)
                throw new NegocioException($"Não foi possível localizar a aula de id : {excluirDto.AulaId}");

            var usuarioLogado = await mediator.Send(new ObterUsuarioLogadoQuery());
            IList<(string codigo, string codigoComponentePai, string codigoTerritorioSaber)> componentesCurricularesDoProfessorCj = new List<(string, string, string)>();
            IEnumerable<ComponenteCurricularEol> componentesCurricularesEolProfessor = Enumerable.Empty<ComponenteCurricularEol>();
            var componenteCurricularId = long.Parse(aula.DisciplinaId);
            var componenteCurricular = await mediator.Send(new ObterComponenteCurricularPorIdQuery(componenteCurricularId));

            if (!usuarioLogado.EhProfessorCj())
                componentesCurricularesEolProfessor = await mediator
                    .Send(new ObterComponentesCurricularesDoProfessorNaTurmaQuery(aula.TurmaId,
                                                                                  usuarioLogado.CodigoRf,
                                                                                  usuarioLogado.PerfilAtual,
                                                                                  usuarioLogado.EhProfessorInfantilOuCjInfantil()));

            if (usuarioLogado.EhProfessorCj())
            {
                var componentesCurricularesDoProfessorCJ = await mediator
                   .Send(new ObterComponentesCurricularesDoProfessorCJNaTurmaQuery(usuarioLogado.Login));

                if (componentesCurricularesDoProfessorCJ.Any())
                {
                    var dadosComponentes = await mediator.Send(new ObterDisciplinasPorIdsQuery(componentesCurricularesDoProfessorCJ.Select(c => c.DisciplinaId).ToArray()));
                    if (dadosComponentes.Any())
                    {
                        componentesCurricularesDoProfessorCj = dadosComponentes
                            .Select(d => (d.CodigoComponenteCurricular.ToString(), d.CdComponenteCurricularPai.ToString(), d.TerritorioSaber
                                ? d.CodigoComponenteCurricular.ToString() : "0")).ToArray();
                    }
                }
            }

            var componenteCorrespondente = !usuarioLogado.EhProfessorCj() && componentesCurricularesEolProfessor != null && (componentesCurricularesEolProfessor.Any(x => x.Regencia) || usuarioLogado.EhProfessor())
                    ? componentesCurricularesEolProfessor.FirstOrDefault(cp => cp.CodigoComponenteCurricularPai.ToString() == aula.DisciplinaId ||(componenteCurricular != null && cp.Codigo.ToString() == componenteCurricular.CdComponenteCurricularPai.ToString()) ||    cp.Codigo.ToString() == aula.DisciplinaId)
                    : new ComponenteCurricularEol
                    {
                        Codigo = long.TryParse(aula.DisciplinaId, out long codigo) ? codigo : 0,
                        CodigoComponenteCurricularPai = componentesCurricularesDoProfessorCj.Select(c => long.TryParse(c.codigoComponentePai, out long codigoPai) ? codigoPai : 0).FirstOrDefault(),
                        CodigoComponenteTerritorioSaber = componentesCurricularesDoProfessorCj.Select(c => long.TryParse(c.codigoTerritorioSaber, out long codigoTerritorio) ? codigoTerritorio : 0).FirstOrDefault()
                    };

            var codigoComponentes = new[] { componenteCorrespondente.Regencia ? componenteCorrespondente.CodigoComponenteCurricularPai.ToString() : componenteCorrespondente.Codigo.ToString() };
            if (componenteCorrespondente.CodigoComponenteTerritorioSaber > 0)
                codigoComponentes = codigoComponentes.Append(componenteCorrespondente.CodigoComponenteTerritorioSaber.ToString()).ToArray();

            var componenteCurricularNome = componenteCorrespondente != null && componenteCorrespondente.TerritorioSaber ? 
                componenteCorrespondente.Descricao : await mediator.Send(new ObterDescricaoComponenteCurricularPorIdQuery(long.Parse(aula.DisciplinaId)));

            if (excluirDto.RecorrenciaAula == RecorrenciaAula.AulaUnica)
            {
                return await mediator.Send(new ExcluirAulaUnicaCommand(usuarioLogado,
                                                                       excluirDto.AulaId,
                                                                       componenteCurricularNome));
            }
            else
            {
                try
                {
                    // TODO alterar para fila do RabbitMQ
                    await mediator.Send(new IncluirFilaExclusaoAulaRecorrenteCommand(excluirDto.AulaId,
                                                                                     excluirDto.RecorrenciaAula,
                                                                                     componenteCurricularNome,
                                                                                     usuarioLogado));

                    return new RetornoBaseDto("Serão excluidas aulas recorrentes, em breve você receberá uma notificação com o resultado do processamento.");
                }
                catch (Exception ex)
                {
                    await mediator.Send(new SalvarLogViaRabbitCommand("Exclusão de aulas recorrentes", LogNivel.Critico, LogContexto.Aula, ex.Message));
                }

                return new RetornoBaseDto("Ocorreu um erro ao solicitar a exclusão de aulas recorrentes, por favor tente novamente.");
            }
        }
    }
}
