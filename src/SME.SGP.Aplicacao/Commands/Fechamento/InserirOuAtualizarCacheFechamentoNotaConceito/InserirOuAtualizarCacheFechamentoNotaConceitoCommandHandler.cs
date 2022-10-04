using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using SME.SGP.Dominio.Constantes;
using SME.SGP.Dominio.Interfaces;
using SME.SGP.Infra;

namespace SME.SGP.Aplicacao
{
    public class
        InserirOuAtualizarCacheFechamentoNotaConceitoCommandHandler : IRequestHandler<
            InserirOuAtualizarCacheFechamentoNotaConceitoCommand, bool>
    {
        private readonly IRepositorioCache repositorioCache;

        public InserirOuAtualizarCacheFechamentoNotaConceitoCommandHandler(IRepositorioCache repositorioCache)
        {
            this.repositorioCache = repositorioCache ?? throw new ArgumentNullException(nameof(repositorioCache));
        }

        public async Task<bool> Handle(InserirOuAtualizarCacheFechamentoNotaConceitoCommand request,
            CancellationToken cancellationToken)
        {
            var nomeChaveCache = string.Format(NomeChaveCache.CHAVE_FECHAMENTO_NOTA_FINAL_COMPONENTE_TURMA,
                request.ComponenteCurricularId, request.TurmaCodigo);

            var retornoCacheMapeado =
                await repositorioCache.ObterObjetoAsync<List<FechamentoNotaAlunoAprovacaoDto>>(nomeChaveCache,
                    "Obter fechamento nota final");

            if (retornoCacheMapeado == null)
                return false;

            foreach (var fechamentoNotaConceito in request.FechamentosNotasConceitos)
            {
                var cacheAluno = retornoCacheMapeado.FirstOrDefault(c =>
                    c.AlunoCodigo == fechamentoNotaConceito.CodigoAluno &&
                    c.ComponenteCurricularId == request.ComponenteCurricularId &&
                    c.Bimestre == request.Bimestre);

                if (cacheAluno == null)
                {
                    retornoCacheMapeado.Add(new FechamentoNotaAlunoAprovacaoDto
                    {
                        Bimestre = request.Bimestre,
                        Nota = fechamentoNotaConceito.Nota,
                        AlunoCodigo = fechamentoNotaConceito.CodigoAluno,
                        ConceitoId = fechamentoNotaConceito.ConceitoId,
                        EmAprovacao = request.EmAprovacao,
                        ComponenteCurricularId = request.ComponenteCurricularId
                    });

                    continue;
                }

                cacheAluno.Nota = fechamentoNotaConceito.Nota;
                cacheAluno.ConceitoId = fechamentoNotaConceito.ConceitoId;
                cacheAluno.EmAprovacao = request.EmAprovacao;
            }

            await repositorioCache.SalvarAsync(nomeChaveCache, retornoCacheMapeado);
            return true;
        }
    }
}