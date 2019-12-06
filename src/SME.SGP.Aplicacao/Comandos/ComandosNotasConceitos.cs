﻿using SME.SGP.Aplicacao.Interfaces;
using SME.SGP.Dominio;
using SME.SGP.Dominio.Interfaces;
using SME.SGP.Infra;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SME.SGP.Aplicacao
{
    public class ComandosNotasConceitos : IComandosNotasConceitos
    {
        private readonly IRepositorioNotasConceitos repositorioNotasConceitos;
        private readonly IServicoDeNotasConceitos servicosDeNotasConceitos;
        private readonly IServicoUsuario servicoUsuario;

        public ComandosNotasConceitos(IServicoDeNotasConceitos servicosDeNotasConceitos, IRepositorioNotasConceitos repositorioNotasConceitos, IServicoUsuario servicoUsuario)
        {
            this.servicosDeNotasConceitos = servicosDeNotasConceitos ?? throw new System.ArgumentNullException(nameof(servicosDeNotasConceitos));
            this.repositorioNotasConceitos = repositorioNotasConceitos ?? throw new System.ArgumentNullException(nameof(repositorioNotasConceitos));
            this.servicoUsuario = servicoUsuario ?? throw new System.ArgumentNullException(nameof(servicoUsuario));
        }

        public async Task Salvar(NotaConceitoListaDto notaConceitoLista)
        {
            var notasConceitosDto = notaConceitoLista.NotasConceitos;

            var alunos = notasConceitosDto.Select(x => x.AlunoId).ToList();
            var avaliacoes = notasConceitosDto.Select(x => x.AtividadeAvaliativaID).ToList();

            var notasBanco = repositorioNotasConceitos.ObterNotasPorAlunosAtividadesAvaliativas(avaliacoes, alunos);

            var professorRf = servicoUsuario.ObterRf();

            var notasSalvar = new List<NotaConceito>();

            if (notasBanco == null || !notasBanco.Any())
                await IncluirTodasNotas(notasConceitosDto, notasSalvar, professorRf, notaConceitoLista.TurmaId);
            else
                await TratarInclusaoEdicaoNotas(notasConceitosDto, notasBanco, notasSalvar, professorRf, notaConceitoLista.TurmaId);
        }

        private async Task IncluirTodasNotas(IEnumerable<NotaConceitoDto> notasConceitosDto, List<NotaConceito> notasSalvar, string professorRf, string turmaId)
        {
            notasSalvar = notasConceitosDto.Select(x => ObterEntidadeInclusao(x)).ToList();
            await servicosDeNotasConceitos.Salvar(notasSalvar, professorRf, turmaId);
        }

        private NotaConceito ObterEntidadeEdicao(NotaConceitoDto dto, NotaConceito entidade)
        {
            entidade.Nota = dto.Nota;
            entidade.Conceito = dto.Conceito;

            return entidade;
        }

        private NotaConceito ObterEntidadeInclusao(NotaConceitoDto Dto)
        {
            return new NotaConceito
            {
                AtividadeAvaliativaID = Dto.AtividadeAvaliativaID,
                AlunoId = Dto.AlunoId,
                Nota = Dto.Nota,
                Conceito = Dto.Conceito,
            };
        }

        private async Task TratarInclusaoEdicaoNotas(IEnumerable<NotaConceitoDto> notasConceitosDto, IEnumerable<NotaConceito> notasBanco, List<NotaConceito> notasSalvar, string professorRf, string turmaId)
        {
            var notasEdicao = notasConceitosDto.Where(dto => notasBanco.Any(banco => banco.AlunoId == dto.AlunoId && banco.AtividadeAvaliativaID == dto.AtividadeAvaliativaID))
                .Select(dto => ObterEntidadeEdicao(dto, notasBanco.FirstOrDefault(banco => banco.AtividadeAvaliativaID == dto.AtividadeAvaliativaID && banco.AlunoId == dto.AlunoId)));

            var notasInclusao = notasConceitosDto.Where(dto => !notasBanco.Any(banco => banco.AlunoId == dto.AlunoId && banco.AtividadeAvaliativaID == dto.AtividadeAvaliativaID)).Select(dto => ObterEntidadeInclusao(dto));

            notasSalvar.AddRange(notasEdicao);
            notasSalvar.AddRange(notasInclusao);

            await servicosDeNotasConceitos.Salvar(notasSalvar, professorRf, turmaId);
        }
    }
}