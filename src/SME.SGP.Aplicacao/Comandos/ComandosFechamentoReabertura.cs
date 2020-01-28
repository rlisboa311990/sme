﻿using SME.SGP.Dominio;
using SME.SGP.Dominio.Interfaces;
using SME.SGP.Infra;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace SME.SGP.Aplicacao
{
    public class ComandosFechamentoReabertura : IComandosFechamentoReabertura
    {
        public readonly IRepositorioDre repositorioDre;
        public readonly IRepositorioTipoCalendario repositorioTipoCalendario;
        public readonly IRepositorioUe repositorioUe;
        private readonly IRepositorioFechamentoReabertura repositorioFechamentoReabertura;
        private readonly IServicoFechamentoReabertura servicoFechamentoReabertura;

        public ComandosFechamentoReabertura(IRepositorioDre repositorioDre, IRepositorioUe repositorioUe,
                                            IRepositorioTipoCalendario repositorioTipoCalendario, IServicoFechamentoReabertura servicoFechamentoReabertura,
                                            IRepositorioFechamentoReabertura repositorioFechamentoReabertura)
        {
            this.repositorioDre = repositorioDre ?? throw new ArgumentNullException(nameof(repositorioDre));
            this.repositorioUe = repositorioUe ?? throw new ArgumentNullException(nameof(repositorioUe));
            this.repositorioTipoCalendario = repositorioTipoCalendario ?? throw new ArgumentNullException(nameof(repositorioTipoCalendario));
            this.servicoFechamentoReabertura = servicoFechamentoReabertura ?? throw new ArgumentNullException(nameof(servicoFechamentoReabertura));
            this.repositorioFechamentoReabertura = repositorioFechamentoReabertura ?? throw new ArgumentNullException(nameof(repositorioFechamentoReabertura));
        }

        public async Task<string> Alterar(FechamentoReaberturaAlteracaoDto fechamentoReaberturaPersistenciaDto, long id)
        {
            var fechamentoReabertura = repositorioFechamentoReabertura.ObterCompleto(id, 0);
            if (fechamentoReabertura == null)
                throw new NegocioException("Não foi possível localizar esta Reabertura de Fechamento.");

            var dataInicioAnterior = fechamentoReabertura.Inicio;
            var dataFimAnterior = fechamentoReabertura.Fim;

            AtualizarEntidadeComDto(fechamentoReabertura, fechamentoReaberturaPersistenciaDto);

            return await servicoFechamentoReabertura.Alterar(fechamentoReabertura, dataInicioAnterior, dataFimAnterior);
        }

        public async Task<string> Excluir(long[] ids)
        {
            var fechamentos = await repositorioFechamentoReabertura.Listar(0, 0, 0, ids);

            foreach (var fechamento in fechamentos)
            {
            }

            return "ok";
        }

        public async Task<string> Salvar(FechamentoReaberturaPersistenciaDto fechamentoReaberturaPersistenciaDto)
        {
            FechamentoReabertura entidade = TransformarDtoEmEntidadeParaPersistencia(fechamentoReaberturaPersistenciaDto);
            return await servicoFechamentoReabertura.Salvar(entidade);
        }

        private void AtualizarEntidadeComDto(FechamentoReabertura fechamentoReabertura, FechamentoReaberturaAlteracaoDto fechamentoReaberturaPersistenciaDto)
        {
            fechamentoReabertura.Inicio = fechamentoReaberturaPersistenciaDto.Inicio;
            fechamentoReabertura.Fim = fechamentoReaberturaPersistenciaDto.Fim;
        }

        private FechamentoReabertura TransformarDtoEmEntidadeParaPersistencia(FechamentoReaberturaPersistenciaDto fechamentoReaberturaPersistenciaDto)
        {
            Dre dre = null;
            Ue ue = null;

            if (!string.IsNullOrEmpty(fechamentoReaberturaPersistenciaDto.DreCodigo))
            {
                dre = repositorioDre.ObterPorCodigo(fechamentoReaberturaPersistenciaDto.DreCodigo);
                if (dre == null)
                    throw new NegocioException("Não foi possível localizar a Dre.");
            }

            if (!string.IsNullOrEmpty(fechamentoReaberturaPersistenciaDto.UeCodigo))
            {
                ue = repositorioUe.ObterPorCodigo(fechamentoReaberturaPersistenciaDto.UeCodigo);
                if (ue == null)
                    throw new NegocioException("Não foi possível localizar a UE.");
            }

            var tipoCalendario = repositorioTipoCalendario.ObterPorId(fechamentoReaberturaPersistenciaDto.TipoCalendarioId);
            if (tipoCalendario == null)
                throw new NegocioException("Não foi possível localizar o Tipo de Calendário.");

            var fechamentoReabertura = new FechamentoReabertura()
            {
                Descricao = fechamentoReaberturaPersistenciaDto.Descricao,
                Fim = fechamentoReaberturaPersistenciaDto.Fim,
                Inicio = fechamentoReaberturaPersistenciaDto.Inicio
            };

            fechamentoReabertura.AtualizarDre(dre);
            fechamentoReabertura.AtualizarUe(ue);
            fechamentoReabertura.AtualizarTipoCalendario(tipoCalendario);

            fechamentoReaberturaPersistenciaDto.Bimestres.ToList().ForEach(bimestre =>
            {
                fechamentoReabertura.Adicionar(new FechamentoReaberturaBimestre()
                {
                    Bimestre = bimestre
                });
            });

            return fechamentoReabertura;
        }
    }
}