﻿using SME.SGP.Aplicacao.Integracoes;
using SME.SGP.Dominio;
using SME.SGP.Dominio.Interfaces;
using SME.SGP.Dto;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SME.SGP.Aplicacao
{
    public class ComandoComunicado : IComandoComunicado
    {
        private readonly IRepositorioComunicado repositorio;
        private readonly IRepositorioComunicadoGrupo repositorioComunicadoGrupo;
        private readonly IServicoAcompanhamentoEscolar servicoAcompanhamentoEscolar;
        private readonly IUnitOfWork unitOfWork;

        public ComandoComunicado(IRepositorioComunicado repositorio,
            IServicoAcompanhamentoEscolar servicoAcompanhamentoEscolar,
            IRepositorioComunicadoGrupo repositorioComunicadoGrupo,
            IUnitOfWork unitOfWork)
        {
            this.repositorio = repositorio ?? throw new System.ArgumentNullException(nameof(repositorio));
            this.repositorioComunicadoGrupo = repositorioComunicadoGrupo ?? throw new System.ArgumentNullException(nameof(repositorioComunicadoGrupo));
            this.servicoAcompanhamentoEscolar = servicoAcompanhamentoEscolar ?? throw new System.ArgumentNullException(nameof(servicoAcompanhamentoEscolar));
            this.unitOfWork = unitOfWork ?? throw new System.ArgumentNullException(nameof(unitOfWork));
        }

        public async Task<string> Alterar(long id, ComunicadoInserirDto comunicadoDto)
        {
            Comunicado comunicado = BuscarComunicado(id);
            ComunicadoInserirAeDto comunicadoServico = new ComunicadoInserirAeDto();
            MapearParaEntidade(comunicadoDto, comunicado);

            try
            {
                unitOfWork.IniciarTransacao();
                await repositorioComunicadoGrupo.ExcluirPorIdComunicado(id);
                await SalvarGrupos(id, comunicadoDto);
                await repositorio.SalvarAsync(comunicado);

                MapearParaEntidadeServico(comunicadoServico, comunicado);
                await servicoAcompanhamentoEscolar.AlterarComunicado(comunicadoServico, id);

                unitOfWork.PersistirTransacao();
            }
            catch
            {
                unitOfWork.Rollback();
                throw;
            }

            return "Comunicado alterado com sucesso";
        }

        public async Task Excluir(long[] ids)
        {
            var erros = new StringBuilder();
            await servicoAcompanhamentoEscolar.ExcluirComunicado(ids);
            foreach (var id in ids)
            {
                var comunicado = repositorio.ObterPorId(id);
                if (comunicado == null)
                    erros.Append($"<li>{id} - comunicado não encontrado</li>");
                else
                {
                    try
                    {
                        await repositorioComunicadoGrupo.ExcluirPorIdComunicado(id);
                        comunicado.MarcarExcluido();
                        await repositorio.SalvarAsync(comunicado);
                    }
                    catch
                    {
                        erros.Append($"<li>{id} - {comunicado.Titulo}</li>");
                    }
                }
            }
            if (!string.IsNullOrEmpty(erros.ToString()))
                throw new NegocioException($"<p>Os seguintes comunicados não puderam ser excluídos:</p><br/>{erros.ToString()} por favor, tente novamente");
        }

        public async Task<string> Inserir(ComunicadoInserirDto comunicadoDto)
        {
            Comunicado comunicado = new Comunicado();
            ComunicadoInserirAeDto comunicadoServico = new ComunicadoInserirAeDto();
            MapearParaEntidade(comunicadoDto, comunicado);

            try
            {
                unitOfWork.IniciarTransacao();
                var id = await repositorio.SalvarAsync(comunicado);
                await SalvarGrupos(id, comunicadoDto);

                MapearParaEntidadeServico(comunicadoServico, comunicado);

                await servicoAcompanhamentoEscolar.CriarComunicado(comunicadoServico);

                unitOfWork.PersistirTransacao();
            }
            catch
            {
                unitOfWork.Rollback();
                throw;
            }

            return "Comunicado criado com sucesso";
        }

        private static void MapearParaEntidade(ComunicadoInserirDto comunicadoDto, Comunicado comunicado)
        {
            comunicado.DataEnvio = comunicadoDto.DataEnvio;
            comunicado.DataExpiracao = comunicadoDto.DataExpiracao;
            comunicado.Descricao = comunicadoDto.Descricao;
            comunicado.Titulo = comunicadoDto.Titulo;
            comunicado.Grupos = comunicadoDto.GruposId.Select(s => new GrupoComunicacao { Id = s }).ToList();
        }

        private Comunicado BuscarComunicado(long id)
        {
            var comunicado = repositorio.ObterPorId(id);
            if (comunicado is null)
                throw new NegocioException("Comunicado não encontrado");
            return comunicado;
        }

        private void MapearParaEntidadeServico(ComunicadoInserirAeDto comunicadoServico, Comunicado comunicado)
        {
            comunicadoServico.Id = comunicado.Id;
            comunicadoServico.AlteradoEm = comunicado.AlteradoEm;
            comunicadoServico.AlteradoPor = comunicado.AlteradoPor;
            comunicadoServico.AlteradoRF = comunicado.AlteradoRF;
            comunicadoServico.DataEnvio = comunicado.DataEnvio;
            comunicadoServico.DataExpiracao = comunicado.DataExpiracao;
            comunicadoServico.Mensagem = comunicado.Descricao;
            comunicadoServico.Titulo = comunicado.Titulo;
            comunicadoServico.Grupo = string.Join(",", comunicado.Grupos.Select(x => x.Id.ToString()).ToArray());
            comunicadoServico.CriadoEm = comunicado.CriadoEm;
            comunicadoServico.CriadoPor = comunicado.CriadoPor;
            comunicadoServico.CriadoRF = comunicado.CriadoRF;
        }

        private async Task SalvarGrupos(long id, ComunicadoInserirDto comunicadoDto)
        {
            foreach (var grupoId in comunicadoDto.GruposId)
            {
                await repositorioComunicadoGrupo.SalvarAsync(new ComunicadoGrupo { ComunicadoId = id, GrupoComunicadoId = grupoId });
            }
        }
    }
}