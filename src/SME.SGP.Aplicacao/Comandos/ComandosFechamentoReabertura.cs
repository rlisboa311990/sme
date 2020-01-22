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
        private readonly IServicoFechamentoReabertura servicoFechamentoReabertura;

        public ComandosFechamentoReabertura(IRepositorioDre repositorioDre, IRepositorioUe repositorioUe,
                                            IRepositorioTipoCalendario repositorioTipoCalendario, IServicoFechamentoReabertura servicoFechamentoReabertura)
        {
            this.repositorioDre = repositorioDre ?? throw new ArgumentNullException(nameof(repositorioDre));
            this.repositorioUe = repositorioUe ?? throw new ArgumentNullException(nameof(repositorioUe));
            this.repositorioTipoCalendario = repositorioTipoCalendario ?? throw new ArgumentNullException(nameof(repositorioTipoCalendario));
            this.servicoFechamentoReabertura = servicoFechamentoReabertura ?? throw new ArgumentNullException(nameof(servicoFechamentoReabertura));
        }

        public async Task Salvar(FechamentoReaberturaPersistenciaDto fechamentoReaberturaPersistenciaDto)
        {
            FechamentoReabertura entidade = TransformarDtoEmEntidadeParaPersistencia(fechamentoReaberturaPersistenciaDto);
            await servicoFechamentoReabertura.Salvar(entidade);
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