﻿using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using SME.SGP.Aplicacao;
using SME.SGP.Dados;
using SME.SGP.Dados.Contexto;
using SME.SGP.Dados.Repositorios;
using SME.SGP.Dominio;
using SME.SGP.Dominio.Interfaces;

namespace SME.SGP.IoC
{
    public static class RegistraDependencias
    {
        public static void Registrar(IServiceCollection services)
        {
            RegistrarRepositorios(services);
            RegistrarContextos(services);
            RegistrarComandos(services);
            RegistrarConsultas(services);
        }

        private static void RegistrarComandos(IServiceCollection services)
        {
            services.TryAddScoped<IComandosPlanoCiclo, ComandosPlanoCiclo>();
            services.TryAddScoped<IComandosPlanoAnual, ComandosPlanoAnual>();
        }

        private static void RegistrarConsultas(IServiceCollection services)
        {
            services.TryAddScoped<IConsultasPlanoCiclo, ConsultasPlanoCiclo>();
            services.TryAddScoped<IConsultasMatrizSaber, ConsultasMatrizSaber>();
            services.TryAddScoped<IConsultasObjetivoDesenvolvimento, ConsultasObjetivoDesenvolvimento>();
            services.TryAddScoped<IConsultasCiclo, ConsultasCiclo>();
            services.TryAddScoped<IConsultasObjetivoAprendizagem, ConsultasObjetivoAprendizagem>();
        }

        private static void RegistrarContextos(IServiceCollection services)
        {
            services.TryAddScoped<ISgpContext, SgpContext>();
            services.TryAddScoped<IUnitOfWork, UnitOfWork>();
        }

        private static void RegistrarRepositorios(IServiceCollection services)
        {
            services.TryAddScoped<IRepositorioPlanoCiclo, RepositorioPlanoCiclo>();
            services.TryAddScoped<IRepositorioMatrizSaberPlano, RepositorioMatrizSaberPlano>();
            services.TryAddScoped<IRepositorioObjetivoDesenvolvimentoPlano, RepositorioObjetivoDesenvolvimentoPlano>();
            services.TryAddScoped<IRepositorioMatrizSaber, RepositorioMatrizSaber>();
            services.TryAddScoped<IRepositorioObjetivoDesenvolvimento, RepositorioObjetivoDesenvolvimento>();
            services.TryAddScoped<IRepositorioCiclo, RepositorioCiclo>();
            services.TryAddScoped<IRepositorioPlanoAnual, RepositorioPlanoAnual>();
            services.TryAddScoped<IRepositorioObjetivoAprendizagemPlano, RepositorioObjetivoAprendizagemPlano>();
            services.TryAddScoped<IRepositorioDisciplinaPlano, RepositorioDisciplinaPlano>();
        }
    }
}