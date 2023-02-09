﻿using MediatR;
using Newtonsoft.Json;
using SME.SGP.Aplicacao.Integracoes;
using SME.SGP.Aplicacao.Integracoes.Respostas;
using SME.SGP.Dominio;
using SME.SGP.Dominio.Interfaces;
using SME.SGP.Infra;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SME.SGP.Aplicacao
{
    public class ConsultasDisciplina : AbstractUseCase, IConsultasDisciplina
    {
        private static readonly long[] IDS_COMPONENTES_REGENCIA = { 2, 7, 8, 89, 138 };
        private readonly IConsultasObjetivoAprendizagem consultasObjetivoAprendizagem;
        private readonly IRepositorioAtribuicaoCJ repositorioAtribuicaoCJ;
        private readonly IRepositorioCache repositorioCache;
        private readonly IRepositorioComponenteCurricularJurema repositorioComponenteCurricularJurema;
        private readonly IRepositorioComponenteCurricularConsulta repositorioComponenteCurricular;
        private readonly IServicoEol servicoEOL;
        private readonly IServicoUsuario servicoUsuario;

        public ConsultasDisciplina(IServicoEol servicoEOL,
            IRepositorioCache repositorioCache,
            IConsultasObjetivoAprendizagem consultasObjetivoAprendizagem,
            IServicoUsuario servicoUsuario,
            IRepositorioComponenteCurricularJurema repositorioComponenteCurricularJurema,
            IRepositorioAtribuicaoCJ repositorioAtribuicaoCJ,
            IRepositorioComponenteCurricularConsulta repositorioComponenteCurricular,
            IMediator mediator) : base(mediator)
        {
            this.servicoEOL = servicoEOL ??
                throw new System.ArgumentNullException(nameof(servicoEOL));
            this.repositorioCache = repositorioCache ??
                throw new System.ArgumentNullException(nameof(repositorioCache));
            this.consultasObjetivoAprendizagem = consultasObjetivoAprendizagem ??
                throw new System.ArgumentNullException(nameof(consultasObjetivoAprendizagem));
            this.servicoUsuario = servicoUsuario ??
                throw new System.ArgumentNullException(nameof(servicoUsuario));
            this.repositorioAtribuicaoCJ = repositorioAtribuicaoCJ ??
                throw new System.ArgumentNullException(nameof(repositorioAtribuicaoCJ));
            this.repositorioComponenteCurricular = repositorioComponenteCurricular ??
                throw new System.ArgumentNullException(nameof(repositorioComponenteCurricular));
            this.repositorioComponenteCurricularJurema = repositorioComponenteCurricularJurema ??
                throw new System.ArgumentNullException(nameof(repositorioComponenteCurricularJurema));
        }

        public IEnumerable<DisciplinaDto> MapearParaDto(IEnumerable<DisciplinaResposta> disciplinas)
        {
            foreach (var disciplina in disciplinas)
                yield return MapearParaDto(disciplina);
        }

        public async Task<IEnumerable<DisciplinaResposta>> ObterComponentesCJ(Modalidade? modalidade, string codigoTurma, string ueId, long codigoDisciplina, string rf, bool ignorarDeParaRegencia = false)
        {
            IEnumerable<DisciplinaResposta> componentes = null;
            var atribuicoes = await repositorioAtribuicaoCJ.ObterPorFiltros(modalidade,
                codigoTurma,
                ueId,
                codigoDisciplina,
                rf,
                string.Empty,
                true);

            if (atribuicoes == null || !atribuicoes.Any())
                return null;

            var disciplinasEol = await repositorioComponenteCurricular.ObterDisciplinasPorIds(atribuicoes.Select(a => a.DisciplinaId).Distinct().ToArray());

            var componenteRegencia = disciplinasEol?.FirstOrDefault(c => c.Regencia);
            if (componenteRegencia == null || ignorarDeParaRegencia)
                return TransformarListaDisciplinaEolParaRetornoDto(disciplinasEol);

            var componentesRegencia = await repositorioComponenteCurricular.ObterDisciplinasPorIds(IDS_COMPONENTES_REGENCIA);
            if (componentesRegencia != null)
                return TransformarListaDisciplinaEolParaRetornoDto(componentesRegencia);

            return componentes;
        }

        public async Task<List<DisciplinaDto>> ObterComponentesCurricularesPorProfessorETurma(string codigoTurma, bool turmaPrograma, bool realizarAgrupamentoComponente = false)
        {
            List<DisciplinaDto> disciplinasDto;
            var disciplinasEol = new List<DisciplinaResposta>();

            var usuarioLogado = await servicoUsuario.ObterUsuarioLogado();

            var dataInicioNovoSGP = await mediator.Send(new ObterParametroSistemaPorTipoQuery(TipoParametroSistema.DataInicioSGP));            

            var turma = await mediator.Send(new ObterTurmaPorCodigoQuery(codigoTurma));

            if (turma == null)
                throw new NegocioException("Não foi possível encontrar a turma");

            if (usuarioLogado.EhProfessorCj())
            {
                var disciplinas = await ObterDisciplinasPerfilCJ(codigoTurma, usuarioLogado.Login);
                var componenteTerritorio = disciplinas.Any(a => a.TerritorioSaber);
                var componentesEol = await mediator.Send(new ObterComponentesCurricularesPorIdsQuery(disciplinas.Select(x => x.CodigoComponenteCurricular).ToArray(), componenteTerritorio, turma.CodigoTurma));

                if (disciplinas.Any() || disciplinas != null)
                {
                    foreach(var componente in componentesEol)
                    {
                        var descricao = componentesEol.SingleOrDefault(c => c.CodigoComponenteCurricular == componente.Id)?.Nome;

                        disciplinasEol.Add(new DisciplinaResposta
                        {
                            CodigoComponenteCurricular = componente.CodigoComponenteCurricular,
                            Id = componente.Id,
                            Compartilhada = componente.Compartilhada,
                            CodigoComponenteCurricularPai = componente.CdComponenteCurricularPai,
                            CodigoComponenteTerritorioSaber = componente.TerritorioSaber ? componente.CodigoComponenteCurricular : null,
                            Nome = componente.Nome,
                            Regencia = componente.Regencia,
                            RegistroFrequencia = componente.RegistraFrequencia,
                            TerritorioSaber = componente.TerritorioSaber,
                            LancaNota = componente.LancaNota,
                            TurmaCodigo = componente.TurmaCodigo,
                            NomeComponenteInfantil = componente.NomeComponenteInfantil
                        });
                    }

                    disciplinas = disciplinasEol.OrderBy(x => x.Nome);
                }

                var disciplinasEolTratadas = realizarAgrupamentoComponente ? disciplinas?.DistinctBy(s => s.Nome) : disciplinas;

                disciplinasDto = MapearParaDto(disciplinasEolTratadas, turmaPrograma, turma.EnsinoEspecial)?.OrderBy(c => c.Nome)?.ToList();
            }
            else
            {
                var componentesCurriculares = await mediator.Send(new ObterComponentesCurricularesEolPorCodigoTurmaLoginEPerfilQuery(codigoTurma, usuarioLogado.Login, usuarioLogado.PerfilAtual, realizarAgrupamentoComponente));

                if(componentesCurriculares == null)
                    componentesCurriculares = await mediator.Send(new ObterComponentesCurricularesEolPorCodigoTurmaLoginEPerfilQuery(codigoTurma, usuarioLogado.Login, usuarioLogado.PerfilAtual, realizarAgrupamentoComponente, false));

                disciplinasDto = (await repositorioComponenteCurricular.ObterDisciplinasPorIds(
                    componentesCurriculares?
                    .Select(a => a.TerritorioSaber ? (a.CodigoComponenteTerritorioSaber == 0 ? a.Codigo : a.CodigoComponenteTerritorioSaber) : a.Codigo).ToArray()))?.OrderBy(c => c.Nome)?.ToList();

                var componentesCurricularesJurema = await repositorioCache.ObterAsync("ComponentesJurema", () => Task.FromResult(repositorioComponenteCurricularJurema.Listar()));

                if (componentesCurricularesJurema == null)
                    throw new NegocioException("Não foi possível recuperar a lista de componentes curriculares.");

                disciplinasDto.ForEach(d =>
                {
                    var componenteEOL = componentesCurriculares.FirstOrDefault(a => (a.TerritorioSaber && a.CodigoComponenteTerritorioSaber > 0) ? a.CodigoComponenteTerritorioSaber == d.CodigoComponenteCurricular : a.Codigo == d.CodigoComponenteCurricular);
                    d.PossuiObjetivos = turma.AnoLetivo >= Convert.ToInt32(dataInicioNovoSGP) && componenteEOL.PossuiObjetivosDeAprendizagem(componentesCurricularesJurema, turmaPrograma, turma.ModalidadeCodigo, turma.Ano);
                    d.CodigoComponenteCurricular = componenteEOL.Codigo;
                    d.Regencia = componenteEOL.Regencia;

                    if (d.TerritorioSaber)
                        d.Nome = componenteEOL.Descricao;

                    d.ObjetivosAprendizagemOpcionais = componenteEOL.PossuiObjetivosDeAprendizagemOpcionais(componentesCurricularesJurema, turma.EnsinoEspecial);
                    d.CdComponenteCurricularPai = componenteEOL.CodigoComponenteCurricularPai;
                    d.NomeComponenteInfantil = componenteEOL.ExibirComponenteEOL && !string.IsNullOrEmpty(d.NomeComponenteInfantil) ? d.NomeComponenteInfantil : d.Nome;
                });
            }

            //Exceção para disciplinas 1060 e 1061 que são compartilhadas entre EF e EJA
            if (turma.ModalidadeCodigo == Modalidade.EJA && disciplinasDto.Any())
            {
                var idComponenteInformaticaOie = 1060;
                
                var idComponenteLeituraOsl = 1061;
                
                foreach(var disciplina in disciplinasDto)
                {
                    disciplina.PossuiObjetivos = false;
                    if (disciplina.CodigoComponenteCurricular == idComponenteInformaticaOie || disciplina.CodigoComponenteCurricular == idComponenteLeituraOsl)
                        disciplina.RegistraFrequencia = false;                    
                }
            }

            if (turma.ModalidadeCodigo == Modalidade.Medio && turma.TipoTurno == (int)TipoTurnoEOL.Noite && disciplinasDto.Any())
            {
                var idComponenteSalaLeituraEM = 1347;
                var idComponenteTecAprendizagem = 1359;

                foreach (var disciplina in disciplinasDto)
                {
                    if (disciplina.CodigoComponenteCurricular == idComponenteSalaLeituraEM || disciplina.CodigoComponenteCurricular == idComponenteTecAprendizagem)
                        disciplina.RegistraFrequencia = false;
                }
            }

            return disciplinasDto;
        }

        public async Task<IEnumerable<DisciplinaDto>> ObterComponentesCurricularesPorProfessorETurmaParaPlanejamento(long codigoDisciplina, string codigoTurma, bool turmaPrograma, bool regencia)
        {
            List<DisciplinaDto> disciplinasDto;
            var usuario = await servicoUsuario.ObterUsuarioLogado();
            var dataInicioNovoSGP = await mediator.Send(new ObterParametroSistemaPorTipoQuery(TipoParametroSistema.DataInicioSGP));

            var chaveCache = $"Disciplinas-planejamento-{codigoTurma}-{codigoDisciplina}-{usuario.PerfilAtual}";
            if (!usuario.EhProfessor() && !usuario.EhProfessorCj() && !usuario.EhProfessorPoa())
            {
                var disciplinasCacheString = await repositorioCache.ObterAsync(chaveCache);

                if (!string.IsNullOrWhiteSpace(disciplinasCacheString))
                {
                    disciplinasDto = JsonConvert.DeserializeObject<List<DisciplinaDto>>(disciplinasCacheString);
                    var disciplinas = await TratarRetornoDisciplinasPlanejamento(disciplinasDto, codigoDisciplina, regencia, codigoTurma);
                    return disciplinas?.OrderBy(c => c.Nome)?.ToList();
                }
            }

            var componentesCurricularesJurema = await repositorioCache.ObterAsync("ComponentesJurema", () => Task.FromResult(repositorioComponenteCurricularJurema.Listar()));
            if (componentesCurricularesJurema == null)
            {
                throw new NegocioException("Não foi possível recuperar a lista de componentes curriculares.");
            }

            var turma = await mediator.Send(new ObterTurmaPorCodigoQuery(codigoTurma));
            if (turma == null)
                throw new NegocioException("Não foi possível encontrar a turma");

            if (usuario.EhProfessorCj())
            {
                var componentesCJ = await ObterComponentesCJ(null, codigoTurma,
                    string.Empty,
                    codigoDisciplina,
                    usuario.Login);
                disciplinasDto = MapearParaDto(componentesCJ, turmaPrograma)?.OrderBy(c => c.Nome)?.ToList();
            }
            else
            {
                var componentesCurriculares = await mediator.Send(new ObterComponentesCurricularesPorCodigoTurmaLoginEPerfilParaPlanejamentoQuery(codigoTurma, usuario.Login, usuario.PerfilAtual));

                if (turma.ModalidadeCodigo == Modalidade.EJA)
                    componentesCurriculares = RemoverEdFisicaEJA(componentesCurriculares);

                disciplinasDto = (await repositorioComponenteCurricular.ObterDisciplinasPorIds(componentesCurriculares.Select(a => a.Codigo).ToArray()))?.OrderBy(c => c.Nome)?.ToList();

                disciplinasDto.ForEach(d =>
                {
                    var componenteEOL = componentesCurriculares.FirstOrDefault(a => a.Codigo == d.CodigoComponenteCurricular);
                    d.PossuiObjetivos = turma.AnoLetivo < Convert.ToInt32(dataInicioNovoSGP) ? false : componenteEOL.PossuiObjetivosDeAprendizagem(componentesCurricularesJurema, turmaPrograma, turma.ModalidadeCodigo, turma.Ano);
                    d.Regencia = componenteEOL.Regencia;
                    d.ObjetivosAprendizagemOpcionais = componenteEOL.PossuiObjetivosDeAprendizagemOpcionais(componentesCurricularesJurema, turma.EnsinoEspecial);
                });
            }

            if (!usuario.EhProfessor() && !usuario.EhProfessorCj() && !usuario.EhProfessorPoa())
                await repositorioCache.SalvarAsync(chaveCache, JsonConvert.SerializeObject(disciplinasDto));

            return await TratarRetornoDisciplinasPlanejamento(disciplinasDto, codigoDisciplina, regencia, codigoTurma, usuario.EhProfessorCj());
        }

        private IEnumerable<ComponenteCurricularEol> RemoverEdFisicaEJA(IEnumerable<ComponenteCurricularEol> componentesCurriculares)
            => componentesCurriculares.Where(c => c.Codigo != 6);

        public async Task<IEnumerable<DisciplinaResposta>> ObterComponentesRegencia(Turma turma)
        {
            var componentesCurriculares = await mediator.Send(new ObterComponentesRegenciaPorAnoQuery(
                                                                    turma.TipoTurno == 4 || turma.TipoTurno == 5 ? turma.AnoTurmaInteiro : 0));

            return MapearComponentes(componentesCurriculares.OrderBy(c => c.Descricao));            
        }

        public async Task<DisciplinaDto> ObterDisciplina(long disciplinaId)
        {
            var disciplinas = await repositorioComponenteCurricular.ObterDisciplinasPorIds(new long[] { disciplinaId });
            if (disciplinas == null || !disciplinas.Any())
                throw new NegocioException($"Componente curricular não localizado no SGP [{disciplinaId}]");

            return disciplinas.FirstOrDefault();
        }

        public async Task<List<DisciplinaDto>> ObterDisciplinasAgrupadasPorProfessorETurma(string codigoTurma, bool turmaPrograma)
        {
            var disciplinasDto = new List<DisciplinaDto>();

            var login = servicoUsuario.ObterLoginAtual();
            var perfilAtual = servicoUsuario.ObterPerfilAtual();

            var chaveCache = $"Disciplinas-Agrupadas-{codigoTurma}-{login}--{perfilAtual}";
            var disciplinasCacheString = repositorioCache.Obter(chaveCache);

            if (!string.IsNullOrWhiteSpace(disciplinasCacheString))
            {
                disciplinasDto = JsonConvert.DeserializeObject<List<DisciplinaDto>>(disciplinasCacheString);
            }
            else
            {
                if (perfilAtual == Perfis.PERFIL_CJ || perfilAtual == Perfis.PERFIL_CJ_INFANTIL)
                {
                    // Carrega Disciplinas da Atribuição do CJ
                    var atribuicoes = await repositorioAtribuicaoCJ.ObterPorFiltros(null, codigoTurma, string.Empty, 0, login, string.Empty, true);
                    if (atribuicoes != null && atribuicoes.Any())
                    {
                        var disciplinasEol = await repositorioComponenteCurricular.ObterDisciplinasPorIds(atribuicoes.Select(a => a.DisciplinaId).Distinct().ToArray());

                        foreach (var disciplinaEOL in disciplinasEol)
                        {
                            if (disciplinaEOL.CdComponenteCurricularPai > 0)
                            {
                                // TODO Consulta por disciplina pai não esta funcionando no EOL. Refatorar na proxima sprint
                                disciplinaEOL.CdComponenteCurricularPai = 11211124;
                                disciplinaEOL.Nome = "REG CLASSE INTEGRAL";

                                //var consultaDisciplinaPai = servicoEOL.ObterDisciplinasPorIds(new long[] { disciplinaEOL.CodigoComponenteCurricularId });
                                //if (consultaDisciplinaPai == null)
                                //    throw new NegocioException($"Disciplina Pai de codigo [{disciplinaEOL.CodigoComponenteCurricularId}] não localizada no EOL.");

                                //disciplinasDto.Add(consultaDisciplinaPai.First());
                            }
                            else
                                disciplinasDto.Add(disciplinaEOL);
                        }
                    }
                }
                else
                {
                    // Carrega disciplinas do professor
                    IEnumerable<DisciplinaResposta> disciplinas = await servicoEOL.ObterDisciplinasPorCodigoTurmaLoginEPerfil(codigoTurma, login, perfilAtual);
                    foreach (var disciplina in disciplinas)
                    {
                        if (disciplina.CodigoComponenteCurricularPai.HasValue)
                        {
                            // TODO Consulta por disciplina pai não esta funcionando no EOL. Refatorar na proxima sprint
                            disciplina.CodigoComponenteCurricular = 11211124;
                            disciplina.Nome = "REG CLASSE INTEGRAL";

                            //var consultaDisciplinaPai = servicoEOL.ObterDisciplinasPorIds(new long[] { disciplina.CodigoComponenteCurricularPai.Value });
                            //if (consultaDisciplinaPai == null)
                            //    throw new NegocioException($"Disciplina Pai de codigo [{disciplina.CodigoComponenteCurricularPai}] não localizada no EOL.");

                            //disciplinasDto.Add(consultaDisciplinaPai.First());
                        }
                        var turma = await mediator.Send(new ObterTurmaPorCodigoQuery(codigoTurma));
                        disciplinasDto.Add(MapearParaDto(disciplina, true, turma.EnsinoEspecial));
                    }
                }

                if (disciplinasDto.Any())
                    await repositorioCache.SalvarAsync(chaveCache, JsonConvert.SerializeObject(disciplinasDto));
            }

            return disciplinasDto;
        }

        public async Task<IEnumerable<DisciplinaResposta>> ObterDisciplinasPerfilCJ(string codigoTurma, string login)
        {
            var atribuicoes = await repositorioAtribuicaoCJ.ObterPorFiltros(null, codigoTurma, string.Empty, 0, login, string.Empty, true);

            if (atribuicoes == null || !atribuicoes.Any())
                return null;

            var disciplinasEol = await repositorioComponenteCurricular.ObterDisciplinasPorIds(atribuicoes.Select(a => a.DisciplinaId).Distinct().ToArray());

            return TransformarListaDisciplinaEolParaRetornoDto(disciplinasEol);
        }

        public async Task<List<DisciplinaDto>> ObterDisciplinasPorProfessorETurma(string codigoTurma, bool turmaPrograma)
        {
            var disciplinasDto = new List<DisciplinaDto>();

            var login = servicoUsuario.ObterLoginAtual();
            var perfilAtual = servicoUsuario.ObterPerfilAtual();
            var ehPefilCJ = perfilAtual == Perfis.PERFIL_CJ || perfilAtual == Perfis.PERFIL_CJ_INFANTIL;

            var chaveCache = $"Disciplinas-{codigoTurma}-{login}--{perfilAtual}";

            var disciplinasCacheString = await repositorioCache.ObterAsync(chaveCache);

            if (!string.IsNullOrWhiteSpace(disciplinasCacheString))
                return JsonConvert.DeserializeObject<List<DisciplinaDto>>(disciplinasCacheString);

            var disciplinas = ehPefilCJ ? await ObterDisciplinasPerfilCJ(codigoTurma, login) :
                await servicoEOL.ObterDisciplinasPorCodigoTurmaLoginEPerfil(codigoTurma, login, perfilAtual);

            if (disciplinas == null || !disciplinas.Any())
                return disciplinasDto;

            disciplinasDto = MapearParaDto(disciplinas, turmaPrograma);

            await repositorioCache.SalvarAsync(chaveCache, JsonConvert.SerializeObject(disciplinasDto));

            return disciplinasDto;
        }

        public async Task<List<DisciplinaDto>> ObterDisciplinasPorTurma(string codigoTurma, bool turmaPrograma)
        {
            var disciplinasDto = new List<DisciplinaDto>();

            var login = servicoUsuario.ObterLoginAtual();
            var perfilAtual = servicoUsuario.ObterPerfilAtual();

            var chaveCache = $"Disciplinas-{codigoTurma}";
            var disciplinasCacheString = repositorioCache.Obter(chaveCache);

            if (!string.IsNullOrWhiteSpace(disciplinasCacheString))
            {
                disciplinasDto = JsonConvert.DeserializeObject<List<DisciplinaDto>>(disciplinasCacheString);
            }
            else
            {
                IEnumerable<DisciplinaResposta> disciplinas;

                if (perfilAtual == Perfis.PERFIL_CJ || perfilAtual == Perfis.PERFIL_CJ_INFANTIL)
                {
                    var atribuicoes = await repositorioAtribuicaoCJ.ObterPorFiltros(null, codigoTurma, string.Empty, 0, login, string.Empty, true);
                    if (atribuicoes != null && atribuicoes.Any())
                    {
                        var disciplinasEol = await repositorioComponenteCurricular.ObterDisciplinasPorIds(atribuicoes.Select(a => a.DisciplinaId).Distinct().ToArray());

                        disciplinas = TransformarListaDisciplinaEolParaRetornoDto(disciplinasEol);
                    }
                    else disciplinas = null;
                }
                else
                    disciplinas = await servicoEOL.ObterDisciplinasPorCodigoTurma(codigoTurma);

                if (disciplinas != null && disciplinas.Any())
                {
                    disciplinasDto = MapearParaDto(disciplinas, turmaPrograma);

                    await repositorioCache.SalvarAsync(chaveCache, JsonConvert.SerializeObject(disciplinasDto));
                }
            }
            return disciplinasDto;
        }

        public IEnumerable<DisciplinaResposta> MapearComponentes(IEnumerable<ComponenteCurricularEol> componentesCurriculares)
        {
            foreach (var componenteCurricular in componentesCurriculares)
                yield return new DisciplinaResposta()
                {
                    CodigoComponenteCurricularPai = componenteCurricular.CodigoComponenteCurricularPai,
                    CodigoComponenteCurricular = componenteCurricular.Codigo,
                    Nome = componenteCurricular.Descricao,
                    Regencia = componenteCurricular.Regencia,
                    TerritorioSaber = componenteCurricular.TerritorioSaber,
                    Compartilhada = componenteCurricular.Compartilhada,
                    LancaNota = componenteCurricular.LancaNota,
                    
                };
        }

        private IEnumerable<DisciplinaResposta> MapearComponentesComComponentesSgp(IEnumerable<ComponenteCurricularEol> componentesCurriculares, IEnumerable<DisciplinaDto> componentesSgp)
        {
            foreach (var componenteCurricular in componentesCurriculares)
            {
                var componenteSgp = componentesSgp.FirstOrDefault(c => c.CodigoComponenteCurricular == componenteCurricular.Codigo);
                yield return new DisciplinaResposta()
                {
                    CodigoComponenteCurricularPai = componenteCurricular.CodigoComponenteCurricularPai,
                    CodigoComponenteCurricular = componenteCurricular.Codigo,
                    Nome = componenteSgp != null ? componenteSgp.Nome : componenteCurricular.Descricao,
                    Regencia = componenteCurricular.Regencia,
                    TerritorioSaber = componenteCurricular.TerritorioSaber,
                    Compartilhada = componenteCurricular.Compartilhada,
                    LancaNota = componenteCurricular.LancaNota,
                };
            }
        }

        private DisciplinaResposta MapearDisciplinaResposta(DisciplinaDto disciplinaEol) => new DisciplinaResposta()
        {
            CodigoComponenteCurricular = disciplinaEol.CodigoComponenteCurricular,
            CodigoComponenteCurricularPai = disciplinaEol.CdComponenteCurricularPai,
            Nome = disciplinaEol.Nome,
            Regencia = disciplinaEol.Regencia,
            Compartilhada = disciplinaEol.Compartilhada,
            RegistroFrequencia = disciplinaEol.RegistraFrequencia,
            LancaNota = disciplinaEol.LancaNota,
            NomeComponenteInfantil = disciplinaEol.NomeComponenteInfantil,
            Id = disciplinaEol.Id,
            TerritorioSaber = disciplinaEol.TerritorioSaber
        };

        private List<DisciplinaDto> MapearParaDto(IEnumerable<DisciplinaResposta> disciplinas, bool turmaPrograma = false, bool ensinoEspecial = false)
        {
            var retorno = new List<DisciplinaDto>();

            if (disciplinas != null)
            {
                foreach (var disciplina in disciplinas)
                {
                    retorno.Add(MapearParaDto(disciplina, turmaPrograma, ensinoEspecial));
                }
            }
            return retorno;
        }

        private DisciplinaDto MapearParaDto(DisciplinaResposta disciplina, bool turmaPrograma = false, bool ensinoEspecial = false) => new DisciplinaDto()
        {
            Id = disciplina.Id,
            CdComponenteCurricularPai = disciplina.CodigoComponenteCurricularPai,
            CodigoComponenteCurricular = disciplina.CodigoComponenteCurricular,
            Nome = disciplina.Nome,
            NomeComponenteInfantil = disciplina.NomeComponenteInfantil,
            Regencia = disciplina.Regencia,
            TerritorioSaber = disciplina.TerritorioSaber,
            Compartilhada = disciplina.Compartilhada,
            RegistraFrequencia = disciplina.RegistroFrequencia,
            LancaNota = disciplina.LancaNota,
            PossuiObjetivos = !turmaPrograma && consultasObjetivoAprendizagem.DisciplinaPossuiObjetivosDeAprendizagem(disciplina.CodigoComponenteCurricular),
            ObjetivosAprendizagemOpcionais = consultasObjetivoAprendizagem.ComponentePossuiObjetivosOpcionais(disciplina.CodigoComponenteCurricular, disciplina.Regencia, ensinoEspecial).Result
        };

        private IEnumerable<DisciplinaResposta> TransformarListaDisciplinaEolParaRetornoDto(IEnumerable<DisciplinaDto> disciplinasEol)
        {
            foreach (var disciplinaEol in disciplinasEol)
            {
                yield return MapearDisciplinaResposta(disciplinaEol);
            }
        }

        private async Task<IEnumerable<DisciplinaDto>> TratarRetornoDisciplinasPlanejamento(IEnumerable<DisciplinaDto> disciplinas, long codigoDisciplina, bool regencia, string codigoTurma = "", bool CJ = false)
        {
            if (codigoDisciplina == 0 && !regencia)
                return disciplinas;

            if (regencia)
            {
                if(!codigoTurma.Equals(""))
                {
                    var regencias = await mediator.Send(new ObterComponentesCurricularesRegenciaPorTurmaCodigoQuery(codigoTurma));
                    return CJ ? disciplinas.Where(x => regencias.Any(c => c.CodigoComponenteCurricular == x.CodigoComponenteCurricular))
                        : disciplinas.Where(x => x.Regencia && regencias.Any(c => c.CodigoComponenteCurricular == x.CodigoComponenteCurricular));
                }                
                return CJ ? disciplinas : disciplinas.Where(x => x.Regencia);
            }                

            return disciplinas.Where(x => x.CodigoComponenteCurricular == codigoDisciplina);
        }
    }
}