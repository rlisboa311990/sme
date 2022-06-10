﻿using Microsoft.Extensions.DependencyInjection;
using SME.SGP.Dominio;
using SME.SGP.Infra.Contexto;
using SME.SGP.Infra.Interfaces;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SME.SGP.TesteIntegracao.Setup
{
    public class ItensBasicosBuilder
    {
        private readonly TesteBase _teste;
        public ItensBasicosBuilder(TesteBase teste)
        {
            _teste = teste;
        }

        public async Task CriaItensComuns()
        {
            await _teste.InserirNaBase(new Dre
            {
                Id = 1,
                CodigoDre = "1",
                Abreviacao = "DRE AB",
                Nome = "DRE AB"
            });
            await _teste.InserirNaBase(new Ue
            {
                Id = 1,
                CodigoUe = "1",
                DreId = 1,
                Nome = "Nome da UE",
            });

            await _teste.InserirNaBase(new PrioridadePerfil
            {
                Id = 1,
                CodigoPerfil = new Guid("40e1e074-37d6-e911-abd6-f81654fe895d"),
                NomePerfil = "Professor",
                Ordem = 290,
                Tipo = TipoPerfil.UE,
                CriadoPor = "Sistema",
                CriadoRF = "1"
            });

            await _teste.InserirNaBase(new PrioridadePerfil
            {
                Id = 2,
                CodigoPerfil = new Guid("41e1e074-37d6-e911-abd6-f81654fe895d"),
                NomePerfil = "Professor CJ",
                Ordem = 320,
                Tipo = TipoPerfil.UE,
                CriadoPor = "Sistema",
                CriadoRF = "1"
            });
        }

        public async Task CriaItensComunsEja()
        {
            await CriaItensComunsEja(false);
        }

        public async Task CriarUsuarioLogadoCP()
        {
            await _teste.InserirNaBase(new Usuario
            {
                Id = 1,
                Login = "8888888",
                CodigoRf = "8888888",
                Nome = "Usuario CP",
                CriadoPor = "Sistema",
                CriadoRF = "0",
                AlteradoRF = "0"
            });

            var contextoAplicacao = _teste.ServiceProvider.GetService<IContextoAplicacao>();

            var variaveis = new Dictionary<string, object>
            {
                { "NomeUsuario", "Usuario CP" },
                { "UsuarioLogado", "8888888" },
                { "RF", "8888888" },
                { "login", "8888888" },
                {
                    "Claims", new List<InternalClaim> {
                        new InternalClaim { Value = "8888888", Type = "rf" },
                        new InternalClaim { Value = "44E1E074-37D6-E911-ABD6-F81654FE895D", Type = "perfil" }
                    }
                }
            };

            contextoAplicacao.AdicionarVariaveis(variaveis);
        }

        public async Task CriaItensComunsEja(bool incluirAdm)
        {
            CriarClaimRegenciaEja(incluirAdm);

            await CriaItensComuns();
            await CriarPeriodoEscolar();
            await CriaAtribuicaoCJ();

            await _teste.InserirNaBase(new Usuario
            {
                Id = 29,
                Login = "6926886",
                CodigoRf = "6926886",
                Nome = "ESTER CUSTODIA DOS SANTOS",
                CriadoPor = "Sistema",
                CriadoRF = "1"
            });

            await _teste.InserirNaBase(new Turma
            {
                UeId = 1,
                Ano = "2",
                CodigoTurma = "1",
                Historica = true,
                ModalidadeCodigo = Modalidade.EJA,
                AnoLetivo = 2022,
                Semestre = 2,
                Nome = "Turma Nome 1"
            });

            await _teste.InserirNaBase(new TipoCalendario
            {
                AnoLetivo = 2022,
                Nome = "Ano Letivo 202",
                Periodo = Periodo.Semestral,
                Modalidade = ModalidadeTipoCalendario.EJA,
                Situacao = true,
                CriadoEm = DateTime.Now,
                CriadoPor = "Sistema",
                CriadoRF = "1",
                Excluido = false,
                Migrado = false
            });

            await _teste.InserirNaBase(new Aula
            {
                UeId = "1",
                DisciplinaId = "1114",
                TurmaId = "1",
                TipoCalendarioId = 1,
                ProfessorRf = "6926886",
                Quantidade = 1,
                DataAula = new DateTime(2022, 02, 10),
                RecorrenciaAula = 0,
                TipoAula = TipoAula.Normal,
                CriadoEm = new DateTime(2022, 02, 10),
                CriadoPor = "Sistema",
                CriadoRF = "1",
                Excluido = false,
                Migrado = false,
                AulaCJ = false
            });

            await _teste.InserirNaBase(new Abrangencia
            {
                UsuarioId = 1,
                DreId = 1,
                UeId = 1,
                TurmaId = 1,
                Historico = true,
                Perfil = new Guid("41e1e074-37d6-e911-abd6-f81654fe895d")
            });
        }

        public async Task CriaAulaSemFrequencia()
        {
            await _teste.InserirNaBase(new Aula
            {
                UeId = "1",
                DisciplinaId = "1106",
                TurmaId = "1",
                TipoCalendarioId = 1,
                ProfessorRf = "6926886",
                Quantidade = 1,
                DataAula = new DateTime(2022, 02, 10),
                RecorrenciaAula = 0,
                TipoAula = TipoAula.Normal,
                CriadoEm = new DateTime(2022, 02, 10),
                CriadoPor = "Sistema",
                CriadoRF = "1",
                Excluido = false,
                Migrado = false,
                AulaCJ = false
            });
        }

        public async Task CriaComponenteCurricularSemFrequencia()
        {
            await _teste.InserirNaBase("componente_curricular_area_conhecimento", "1", "'Área de conhecimento 1'");

            await _teste.InserirNaBase("componente_curricular_grupo_matriz", "1", "'Grupo matriz 1'");

            await _teste.InserirNaBase("componente_curricular", "1106", "1106", "1", "1", "'ED.INF. EMEI 4 HS'", "false", "false", "true", "false", "false", "true", "'Regência de Classe Infantil'", "'REGÊNCIA INFANTIL EMEI 4H'");
        }

        private void CriarClaimRegenciaEja(bool incluirAdm)
        {
            var contextoAplicacao = _teste.ServiceProvider.GetService<IContextoAplicacao>();
            var variaveis = new Dictionary<string, object>();
            variaveis.Add("NomeUsuario", "ESTER CUSTODIA DOS SANTOS");
            variaveis.Add("UsuarioLogado", "6926886");
            variaveis.Add("RF", "6926886");
            variaveis.Add("login", "6926886");

            if (incluirAdm)
            {
                variaveis.Add("Administrador", "7924488");
            }

            variaveis.Add("Claims", new List<InternalClaim> {
                new InternalClaim { Value = "6926886", Type = "rf" },
                new InternalClaim { Value = "41e1e074-37d6-e911-abd6-f81654fe895d", Type = "perfil" }
            });
            contextoAplicacao.AdicionarVariaveis(variaveis);
        }

        private async Task CriarPeriodoEscolar()
        {
            await _teste.InserirNaBase(new PeriodoEscolar
            {
                Id = 1,
                TipoCalendarioId = 1,
                Bimestre = 2,
                PeriodoInicio = new DateTime(2022, 01, 10),
                PeriodoFim = DateTime.Now.AddYears(1),
                CriadoPor = "Sistema",
                CriadoRF = "1",
                CriadoEm = DateTime.Now,
                Migrado = false
            });
        }

        private async Task CriaAtribuicaoCJ()
        {
            await _teste.InserirNaBase(new AtribuicaoCJ
            {
                TurmaId = "1",
                DreId = "1",
                UeId = "1",
                ProfessorRf = "6926886",
                DisciplinaId = 1106,
                Modalidade = Modalidade.CIEJA,
                Substituir = true,
                CriadoPor = "Sistema",
                CriadoRF = "1",
                CriadoEm = DateTime.Now,
                Migrado = false
            });
        }
    }
}
