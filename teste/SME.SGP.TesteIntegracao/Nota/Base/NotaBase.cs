﻿using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using SME.SGP.Aplicacao;
using SME.SGP.Dominio;
using SME.SGP.Infra;
using SME.SGP.TesteIntegracao.Setup;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SME.SGP.TesteIntegracao.Nota
{
    public abstract class NotaBase : TesteBaseComuns
    {
        protected const long AULA_ID_1 = 1;

        protected const string TIPO_FREQUENCIA_COMPARECEU = "C";
        protected const string TIPO_FREQUENCIA_FALTOU = "F";
        protected const string TIPO_FREQUENCIA_REMOTO = "R";

        protected const string CODIGO_ALUNO_99999 = "99999";

        private const string REABERTURA_GERAL = "Reabrir Geral";

        private readonly DateTime DATA_01_01 = new(DateTimeExtension.HorarioBrasilia().Year, 01, 01);
        private readonly DateTime DATA_31_12 = new(DateTimeExtension.HorarioBrasilia().Year, 12, 31);

        protected readonly string ALUNO_CODIGO_1 = "1";
        protected readonly string ALUNO_CODIGO_2 = "2";
        protected readonly string ALUNO_CODIGO_3 = "3";
        protected readonly string ALUNO_CODIGO_4 = "4";
        protected readonly string ALUNO_CODIGO_5 = "5";
        protected readonly string ALUNO_CODIGO_6 = "6";
        protected readonly string ALUNO_CODIGO_7 = "7";
        protected readonly string ALUNO_CODIGO_8 = "8";
        protected readonly string ALUNO_CODIGO_9 = "9";
        protected readonly string ALUNO_CODIGO_10 = "10";

        protected readonly long ATIVIDADE_AVALIATIVA_1 = 1;
        protected readonly long ATIVIDADE_AVALIATIVA_2 = 2;

        protected readonly double NOTA_1 = 1;
        protected readonly double NOTA_2 = 2;
        protected readonly double NOTA_3 = 3;
        protected readonly double NOTA_4 = 4;
        protected readonly double NOTA_5 = 5;
        protected readonly double NOTA_6 = 6;
        protected readonly double NOTA_7 = 7;
        protected readonly double NOTA_8 = 8;
        protected readonly double NOTA_9 = 9;
        protected readonly double NOTA_10 = 10;

        protected readonly string AVALIACAO_NOME_1 = "Avaliação 1";
        protected readonly string AVALIACAO_NOME_2 = "Avaliação 2";

        protected readonly long TIPO_AVALIACAO_CODIGO_1 = 1;
        protected readonly long TIPO_AVALIACAO_CODIGO_2 = 2;

        protected readonly long PERIODO_ESCOLAR_CODIGO_1 = 1;



        protected NotaBase(CollectionFixture collectionFixture) : base(collectionFixture)
        {
        }

        protected override void RegistrarFakes(IServiceCollection services)
        {
            base.RegistrarFakes(services);

            services.Replace(new ServiceDescriptor(typeof(IRequestHandler<ObterFuncionarioCoreSSOPorPerfilDreQuery, IEnumerable<UsuarioEolRetornoDto>>), typeof(ObterFuncionarioCoreSSOPorPerfilDreQueryHandlerFake), ServiceLifetime.Scoped));
        }

        private (IComandosNotasConceitos, IObterNotasParaAvaliacoesUseCase) RetornarServicosBasicos()
        {
            var comandosNotasConceitos = ServiceProvider.GetService<IComandosNotasConceitos>();

            var obterNotasParaAvaliacoesUseCase = ServiceProvider.GetService<IObterNotasParaAvaliacoesUseCase>();

            return (comandosNotasConceitos, obterNotasParaAvaliacoesUseCase);
        }

        protected async Task ExecutarNotasConceito(NotaConceitoListaDto notaconceito, ListaNotasConceitosDto listaNotaConceito)
        {
            var (comandosNotasConceitos, obterNotasParaAvaliacoesUseCase) = RetornarServicosBasicos();

            await comandosNotasConceitos.Salvar(notaconceito);

            var retorno = await obterNotasParaAvaliacoesUseCase.Executar(listaNotaConceito);
        }

        protected async Task<NotasConceitosRetornoDto> ExecutarNotasConceito(ListaNotasConceitosDto consultaListaNotasConceitosDto, NotaConceitoListaDto notaConceitoLista)
        {
            var (comandosNotasConceitos, obterNotasParaAvaliacoesUseCase) = RetornarServicosBasicos();

            await comandosNotasConceitos.Salvar(notaConceitoLista);

            return await obterNotasParaAvaliacoesUseCase.Executar(consultaListaNotasConceitosDto);
        }

        protected async Task CriarDadosBase(FiltroNota filtroNota)
        {
            await CriarTipoCalendario(filtroNota.TipoCalendario);

            await CriarDreUePerfilComponenteCurricular();

            CriarClaimUsuario(filtroNota.Perfil);

            await CriarUsuarios();

            await CriarTurma(filtroNota.Modalidade);

            if (filtroNota.CriarPeriodoEscolar)
                await CriarPeriodoEscolar();

            if (filtroNota.CriarPeriodoAbertura)
                await CriarPeriodoAbertura(filtroNota.TipoCalendarioId);

            await CriarParametrosNotas();

            await CriarAbrangencia(filtroNota.Perfil);

            await CriarCiclo();
        }

        private async Task CriarCiclo()
        {
            await InserirNaBase(new Ciclo()
            {
                Descricao = ALFABETIZACAO,
                CriadoEm = DateTime.Now,
                CriadoPor = SISTEMA_NOME,
                CriadoRF = SISTEMA_CODIGO_RF
            });

            await InserirNaBase(new CicloAno()
            {
                CicloId = 1,
                Ano = ANO_1,
                Modalidade = Modalidade.Fundamental
            });

            await InserirNaBase(new CicloAno()
            {
                CicloId = 1,
                Ano = ANO_2,
                Modalidade = Modalidade.Fundamental
            });

            await InserirNaBase(new CicloAno()
            {
                CicloId = 1,
                Ano = ANO_3,
                Modalidade = Modalidade.Fundamental
            });

            await InserirNaBase(new Ciclo()
            {
                Descricao = INTERDISCIPLINAR,
                CriadoEm = DateTime.Now,
                CriadoPor = SISTEMA_NOME,
                CriadoRF = SISTEMA_CODIGO_RF
            });

            await InserirNaBase(new CicloAno()
            {
                CicloId = 2,
                Ano = ANO_4,
                Modalidade = Modalidade.Fundamental
            });

            await InserirNaBase(new CicloAno()
            {
                CicloId = 2,
                Ano = ANO_5,
                Modalidade = Modalidade.Fundamental
            });

            await InserirNaBase(new CicloAno()
            {
                CicloId = 2,
                Ano = ANO_6,
                Modalidade = Modalidade.Fundamental
            });

            await InserirNaBase(new Ciclo()
            {
                Descricao = AUTORAL,
                CriadoEm = DateTime.Now,
                CriadoPor = SISTEMA_NOME,
                CriadoRF = SISTEMA_CODIGO_RF
            });

            await InserirNaBase(new CicloAno()
            {
                CicloId = 3,
                Ano = ANO_7,
                Modalidade = Modalidade.Fundamental
            });

            await InserirNaBase(new CicloAno()
            {
                CicloId = 3,
                Ano = ANO_8,
                Modalidade = Modalidade.Fundamental
            });
            
            await InserirNaBase(new CicloAno()
            {
                CicloId = 3,
                Ano = ANO_9,
                Modalidade = Modalidade.Fundamental
            });

            await InserirNaBase(new Ciclo()
            {
                Descricao = MEDIO,
                CriadoEm = DateTime.Now,
                CriadoPor = SISTEMA_NOME,
                CriadoRF = SISTEMA_CODIGO_RF
            });

            await InserirNaBase(new Ciclo()
            {
                Descricao = EJA_ALFABETIZACAO,
                CriadoEm = DateTime.Now,
                CriadoPor = SISTEMA_NOME,
                CriadoRF = SISTEMA_CODIGO_RF
            });

            await InserirNaBase(new Ciclo()
            {
                Descricao = EJA_BASICA,
                CriadoEm = DateTime.Now,
                CriadoPor = SISTEMA_NOME,
                CriadoRF = SISTEMA_CODIGO_RF
            });

            await InserirNaBase(new Ciclo()
            {
                Descricao = EJA_COMPLEMENTAR,
                CriadoEm = DateTime.Now,
                CriadoPor = SISTEMA_NOME,
                CriadoRF = SISTEMA_CODIGO_RF
            });

            await InserirNaBase(new Ciclo()
            {
                Descricao = EJA_FINAL,
                CriadoEm = DateTime.Now,
                CriadoPor = SISTEMA_NOME,
                CriadoRF = SISTEMA_CODIGO_RF
            });
        }

        private async Task CriarAbrangencia(string perfil)
        {
            await InserirNaBase(new Abrangencia()
            {
                DreId = DRE_ID_1,
                Historico = false,
                Perfil = new Guid(perfil),
                TurmaId = TURMA_ID_1,
                UeId = UE_ID_1,
                UsuarioId = USUARIO_ID_1
            });
        }

        protected async Task CriarDadosBase(string perfil, Modalidade modalidade, ModalidadeTipoCalendario tipoCalendario, DateTime dataInicio, DateTime dataFim, int bimestre, long tipoCalendarioId = 1, bool criarPeriodo = true)
        {
            await CriarTipoCalendario(tipoCalendario);

            await CriarItensComuns(criarPeriodo, dataInicio, dataFim, bimestre, tipoCalendarioId);

            CriarClaimUsuario(perfil);

            await CriarUsuarios();
        }

        protected async Task CriarDadosBasicos(string perfil, Modalidade modalidade, ModalidadeTipoCalendario tipoCalendario, DateTime dataInicio, DateTime dataFim, int bimestre, DateTime dataAula, string componenteCurricular, int quantidadeAula, bool criarPeriodo = true, long tipoCalendarioId = 1, bool criarPeriodoEscolarEAbertura = true)
        {
            await CriarTipoCalendario(tipoCalendario);
            
            await CriarItensComuns(criarPeriodo, dataInicio, dataFim, bimestre, tipoCalendarioId);
            
            CriarClaimUsuario(perfil);
            
            await CriarUsuarios();
            
            await CriarTurma(modalidade);
            
            await CriarAula(componenteCurricular, dataAula, RecorrenciaAula.AulaUnica, quantidadeAula);
            
            if (criarPeriodoEscolarEAbertura)
                await CriarPeriodoEscolarEAbertura();
            
            await CriarParametrosNotas();
        }

        private async Task CriarParametrosNotas()
        {
            var dataAtual = DateTimeExtension.HorarioBrasilia();

            await InserirNaBase(new ParametrosSistema() 
            { 
                Nome = DATA_INICIO_SGP, 
                Descricao = DATA_INICIO_SGP,
                Tipo = TipoParametroSistema.DataInicioSGP, 
                Valor = dataAtual.Year.ToString(), 
                Ano = dataAtual.Year, 
                Ativo = true, 
                CriadoEm = dataAtual, 
                CriadoRF = SISTEMA_CODIGO_RF, 
                CriadoPor = SISTEMA_NOME
            });

            await InserirNaBase(new ParametrosSistema()
            {
                Nome = PERCENTUAL_ALUNOS_INSUFICIENTES,
                Descricao = PERCENTUAL_ALUNOS_INSUFICIENTES,
                Tipo = TipoParametroSistema.PercentualAlunosInsuficientes,
                Valor = NUMERO_50,
                Ano = dataAtual.Year,
                Ativo = true,
                CriadoEm = dataAtual,
                CriadoRF = SISTEMA_CODIGO_RF,
                CriadoPor = SISTEMA_NOME
            });

            await InserirNaBase(new ParametrosSistema()
            {
                Nome = MEDIA_BIMESTRAL,
                Descricao = MEDIA_BIMESTRAL,
                Tipo = TipoParametroSistema.MediaBimestre,
                Valor = NUMERO_5,
                Ano = dataAtual.Year,
                Ativo = true,
                CriadoEm = dataAtual,
                CriadoRF = SISTEMA_CODIGO_RF,
                CriadoPor = SISTEMA_NOME
            });
        }

        protected async Task CriarPeriodoEscolarEAbertura()
        {
            await CriarPeriodoEscolar(DATA_INICIO_BIMESTRE_1, DATA_FIM_BIMESTRE_1, BIMESTRE_1);

            await CriarPeriodoEscolar(DATA_INICIO_BIMESTRE_2, DATA_FIM_BIMESTRE_2, BIMESTRE_2);

            await CriarPeriodoEscolar(DATA_INICIO_BIMESTRE_3, DATA_FIM_BIMESTRE_3, BIMESTRE_3);

            await CriarPeriodoEscolar(DATA_INICIO_BIMESTRE_4, DATA_FIM_BIMESTRE_4, BIMESTRE_4);

            await CriarPeriodoReabertura(TIPO_CALENDARIO_1);
        }

        protected async Task CriarPeriodoEscolar()
        {
            await CriarPeriodoEscolar(DATA_INICIO_BIMESTRE_1, DATA_FIM_BIMESTRE_1, BIMESTRE_1);

            await CriarPeriodoEscolar(DATA_INICIO_BIMESTRE_2, DATA_FIM_BIMESTRE_2, BIMESTRE_2);

            await CriarPeriodoEscolar(DATA_INICIO_BIMESTRE_3, DATA_FIM_BIMESTRE_3, BIMESTRE_3);

            await CriarPeriodoEscolar(DATA_INICIO_BIMESTRE_4, DATA_FIM_BIMESTRE_4, BIMESTRE_4);
        }

        protected async Task CriarPeriodoAbertura(long tipoCalendario)
        {
            await CriarPeriodoReabertura(tipoCalendario);
        }

        private ComponenteCurricularDto ObterComponenteCurricular(long componenteCurricularId)
        {
            if (componenteCurricularId == COMPONENTE_CURRICULAR_PORTUGUES_ID_138)
                return new ComponenteCurricularDto()
                {
                    Codigo = COMPONENTE_CURRICULAR_PORTUGUES_ID_138.ToString(),
                    Descricao = COMPONENTE_CURRICULAR_PORTUGUES_NOME
                };
            else if (componenteCurricularId == COMPONENTE_CURRICULAR_DESCONHECIDO_ID_999999)
                return new ComponenteCurricularDto()
                {
                    Codigo = COMPONENTE_CURRICULAR_DESCONHECIDO_ID_999999.ToString(),
                    Descricao = COMPONENTE_CURRICULAR_DESCONHECIDO_NOME
                };
            else if (componenteCurricularId == COMPONENTE_REG_CLASSE_SP_INTEGRAL_1A5_ANOS_ID_1213)
                return new ComponenteCurricularDto()
                {
                    Codigo = COMPONENTE_REG_CLASSE_SP_INTEGRAL_1A5_ANOS_ID_1213.ToString(),
                    Descricao = COMPONENTE_REG_CLASSE_SP_INTEGRAL_1A5_ANOS_NOME
                };
            else if (componenteCurricularId == COMPONENTE_REG_CLASSE_EJA_ETAPA_ALFAB_ID_1113)
                return new ComponenteCurricularDto()
                {
                    Codigo = COMPONENTE_REG_CLASSE_EJA_ETAPA_ALFAB_ID_1113.ToString(),
                    Descricao = COMPONENTE_REG_CLASSE_EJA_ETAPA_ALFAB_NOME
                };

            return null;
        }

        protected async Task CriarAula(string componenteCurricularCodigo, DateTime dataAula, RecorrenciaAula recorrencia, int quantidadeAula, string rf = USUARIO_PROFESSOR_LOGIN_2222222)
        {
            await InserirNaBase(ObterAula(componenteCurricularCodigo, dataAula, recorrencia, quantidadeAula, rf));
        }

        private Aula ObterAula(string componenteCurricularCodigo, DateTime dataAula, RecorrenciaAula recorrencia, int quantidadeAula, string rf = USUARIO_PROFESSOR_LOGIN_2222222)
        {
            return new Aula
            {
                UeId = UE_CODIGO_1,
                DisciplinaId = componenteCurricularCodigo,
                TurmaId = TURMA_CODIGO_1,
                TipoCalendarioId = 1,
                ProfessorRf = rf,
                Quantidade = quantidadeAula,
                DataAula = dataAula,
                RecorrenciaAula = recorrencia,
                TipoAula = TipoAula.Normal,
                CriadoEm = DateTime.Now,
                CriadoPor = SISTEMA_NOME,
                CriadoRF = SISTEMA_CODIGO_RF,
                Excluido = false,
                Migrado = false,
                AulaCJ = false
            };
        }

        protected async Task CriarPeriodoEscolarEAberturaPadrao()
        {
            await CriarPeriodoEscolar(DATA_INICIO_BIMESTRE_1, DATA_FIM_BIMESTRE_1, BIMESTRE_1);

            await CriarPeriodoEscolar(DATA_INICIO_BIMESTRE_2, DATA_FIM_BIMESTRE_2, BIMESTRE_2);

            await CriarPeriodoEscolar(DATA_INICIO_BIMESTRE_3, DATA_FIM_BIMESTRE_3, BIMESTRE_3);

            await CriarPeriodoEscolar(DATA_INICIO_BIMESTRE_4, DATA_FIM_BIMESTRE_4, BIMESTRE_4);

            await CriarPeriodoReabertura(TIPO_CALENDARIO_1);
        }
        protected async Task CriarMotivosAusencias(string descricao)
        {
            await InserirNaBase(new MotivoAusencia() { Descricao = descricao });
        }

        protected async Task CriarPeriodoReabertura(long tipoCalendarioId)
        {
            await InserirNaBase(new FechamentoReabertura()
            {
                Descricao = REABERTURA_GERAL,
                Inicio = DATA_01_01,
                Fim = DATA_31_12,
                TipoCalendarioId = tipoCalendarioId,
                CriadoEm = DateTime.Now,
                CriadoPor = SISTEMA_NOME,
                CriadoRF = SISTEMA_CODIGO_RF,
            });

            await InserirNaBase(new FechamentoReaberturaBimestre()
            {
                FechamentoAberturaId = 1,
                Bimestre = BIMESTRE_1,
                CriadoEm = DateTime.Now,
                CriadoPor = SISTEMA_NOME,
                CriadoRF = SISTEMA_CODIGO_RF,
            });

            await InserirNaBase(new FechamentoReaberturaBimestre()
            {
                FechamentoAberturaId = 1,
                Bimestre = BIMESTRE_2,
                CriadoEm = DateTime.Now,
                CriadoPor = SISTEMA_NOME,
                CriadoRF = SISTEMA_CODIGO_RF,
            });

            await InserirNaBase(new FechamentoReaberturaBimestre()
            {
                FechamentoAberturaId = 1,
                Bimestre = BIMESTRE_3,
                CriadoEm = DateTime.Now,
                CriadoPor = SISTEMA_NOME,
                CriadoRF = SISTEMA_CODIGO_RF,
            });

            await InserirNaBase(new FechamentoReaberturaBimestre()
            {
                FechamentoAberturaId = 1,
                Bimestre = BIMESTRE_4,
                CriadoEm = DateTime.Now,
                CriadoPor = SISTEMA_NOME,
                CriadoRF = SISTEMA_CODIGO_RF,
            });
        }

        protected async Task CriarTipoAvaliacao(TipoAvaliacaoCodigo tipoAvalicao, string descricaoAvaliacao)
        {
            await InserirNaBase(new TipoAvaliacao
            {
                Nome = descricaoAvaliacao,
                Descricao = descricaoAvaliacao,
                Situacao = true,
                AvaliacoesNecessariasPorBimestre = 1,
                Codigo = tipoAvalicao,
                CriadoPor = SISTEMA_NOME,
                CriadoRF = SISTEMA_CODIGO_RF,
                CriadoEm = DateTime.Now
            });
        }

        protected async Task CriarAtividadeAvaliativa(DateTime dataAvaliacao, long TipoAvaliacaoId,string nomeAvaliacao, bool ehRegencia = false, bool ehCj = false, string rf = USUARIO_PROFESSOR_CODIGO_RF_2222222)
        {
            await InserirNaBase(new AtividadeAvaliativa
            {
                DreId = DRE_CODIGO_1,
                UeId = UE_CODIGO_1,
                ProfessorRf = rf,
                TurmaId = TURMA_CODIGO_1,
                Categoria = CategoriaAtividadeAvaliativa.Normal,
                TipoAvaliacaoId = TipoAvaliacaoId,
                NomeAvaliacao = nomeAvaliacao,
                DescricaoAvaliacao = nomeAvaliacao,
                DataAvaliacao = dataAvaliacao,
                EhRegencia = ehRegencia,
                EhCj = ehCj,
                CriadoPor = SISTEMA_NOME,
                CriadoRF = SISTEMA_CODIGO_RF,
                CriadoEm = DateTime.Now
            });
        }

        protected async Task CriarAtividadeAvaliativaDisciplina(long atividadeAvaliativaId, string componenteCurricular)
        {
            await InserirNaBase(new AtividadeAvaliativaDisciplina
            {
                AtividadeAvaliativaId = atividadeAvaliativaId,
                DisciplinaId = componenteCurricular,
                CriadoPor = SISTEMA_NOME,
                CriadoRF = SISTEMA_CODIGO_RF,
                CriadoEm = DateTime.Now
            });
        }
    }
}