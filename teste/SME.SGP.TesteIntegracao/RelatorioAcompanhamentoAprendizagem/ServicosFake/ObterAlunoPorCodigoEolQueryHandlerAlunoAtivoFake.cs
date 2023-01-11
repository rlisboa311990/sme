using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using SME.SGP.Aplicacao;
using SME.SGP.Dominio;
using SME.SGP.Infra;

namespace SME.SGP.TesteIntegracao.RelatorioAcompanhamentoAprendizagem.ServicosFake
{
    public class ObterAlunoPorCodigoEolQueryHandlerAlunoAtivoFake  : IRequestHandler<ObterAlunoPorCodigoEolQuery, AlunoPorTurmaResposta>
    {
        private const string ALUNO_DESISTENTE_CODIGO_77777 = "77777";
        private const string ALUNO_DESISTENTE_NOME_77777 = "ALUNO INATIVO 77777";
        public async Task<AlunoPorTurmaResposta> Handle(ObterAlunoPorCodigoEolQuery request, CancellationToken cancellationToken)
        {
            if (string.IsNullOrEmpty(request.CodigoTurma))
                return Obter(request.CodigoTurma).OrderByDescending(a => a.DataSituacao)?.FirstOrDefault();

            return Obter(request.CodigoTurma).Where(da => da.CodigoTurma.ToString().Equals(request.CodigoTurma)).FirstOrDefault(); ; 
        }
        private List<AlunoPorTurmaResposta> Obter(string turmaId)
        {
            var dataReferencia = DateTimeExtension.HorarioBrasilia().Date;

            return new List<AlunoPorTurmaResposta> {
                new()
                {
                    CodigoAluno = "LUNO ATIVO 77777",
                    CodigoSituacaoMatricula= SituacaoMatriculaAluno.Ativo,
                    CodigoTurma=int.Parse(turmaId),
                    DataNascimento=new DateTime(dataReferencia.AddYears(-15).Year,01,16,00,00,00),
                    DataSituacao= new DateTime(dataReferencia.AddYears(-1).Year,11,09,17,25,31),
                    DataMatricula= new DateTime(dataReferencia.AddYears(-1).Year,11,09,17,25,31),
                    NomeAluno= "77777",
                    NumeroAlunoChamada=1,
                    SituacaoMatricula="Ativo",
                    DataAtualizacaoContato= new DateTime(dataReferencia.AddYears(-1).Year,06,22,19,02,35),
                }
            };
        }
    }
}