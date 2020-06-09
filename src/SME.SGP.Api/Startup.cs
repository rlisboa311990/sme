﻿using Dapper;
using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Prometheus;
using SME.Background.Core;
using SME.Background.Hangfire;
using SME.SGP.Api.HealthCheck;
using SME.SGP.Background;
using SME.SGP.Dados.Mapeamentos;
using SME.SGP.IoC;
using Swashbuckle.AspNetCore.Swagger;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using MediatR;
using SME.SGP.Aplicacao.Integracoes;
using System.Threading.Tasks;
using RabbitMQ.Client;

namespace SME.SGP.Api
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }
            app.UseRequestLocalization();

            app.UseHttpsRedirection();
            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "SGP Api");
            });

            //TODO: Ajustar para as os origins que irão consumir
            app.UseCors(builder => builder
                .AllowAnyOrigin()
                .AllowAnyMethod()
                .AllowAnyHeader()
                .AllowCredentials());

            app.UseMetricServer();

            app.UseHttpMetrics();

            app.UseAuthentication();

            app.UseMvc();

            app.UseStaticFiles();

            app.UseHealthChecks("/healthz", new HealthCheckOptions()
            {
                Predicate = _ => true,
                ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
            });
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton(Configuration);
            services.AddHttpContextAccessor();

            RegistraDependencias.Registrar(services);
            RegistrarMapeamentos.Registrar();
            RegistraClientesHttp.Registrar(services, Configuration);
            RegistraAutenticacao.Registrar(services, Configuration);
            RegistrarMvc.Registrar(services, Configuration);
            RegistraDocumentacaoSwagger.Registrar(services);
            

            DefaultTypeMap.MatchNamesWithUnderscores = true;

            services.AddDistributedRedisCache(options =>
            {
                options.Configuration = Configuration.GetConnectionString("SGP-Redis");
                options.InstanceName = Configuration.GetValue<string>("Nome-Instancia-Redis");
            });

            services.AddApplicationInsightsTelemetry(Configuration);

            services.AdicionarMediatr();
            services.AdicionarValidadoresFluentValidation();

            Orquestrador.Inicializar(services.BuildServiceProvider());

            if (Configuration.GetValue<bool>("FF_BackgroundEnabled", false))
            {
                Orquestrador.Registrar(new Processor(Configuration, "SGP-Postgres"));
                RegistraServicosRecorrentes.Registrar();
            }
            else
                Orquestrador.Desativar();

            services.AddHealthChecks()
                    .AddRedis(
                        Configuration.GetConnectionString("SGP-Redis"),
                        "Redis Cache",
                        null,
                        tags: new string[] { "db", "redis" })
                    .AddNpgSql(
                        Configuration.GetConnectionString("SGP-Postgres"),
                        name: "Postgres")
                    .AddCheck<ApiJuremaCheck>("API Jurema")
                    .AddCheck<ApiEolCheck>("API EOL");

            services.Configure<RequestLocalizationOptions>(options =>
            {
                options.DefaultRequestCulture = new Microsoft.AspNetCore.Localization.RequestCulture("pt-BR");
                options.SupportedCultures = new List<CultureInfo> { new CultureInfo("pt-BR"), new CultureInfo("pt-BR") };
            });

            //TODO: RETIRAR DAQUI!
            var factory = new ConnectionFactory
            {
                HostName = Environment.GetEnvironmentVariable("ConfiguracaoRabbit__Hostname"),
                UserName = Environment.GetEnvironmentVariable("ConfiguracaoRabbit__Username"),
                Password = Environment.GetEnvironmentVariable("ConfiguracaoRabbit__Password")
            };

            var conexaoRabbit = factory.CreateConnection();
            IModel _channel = conexaoRabbit.CreateModel();

            //TODO: VARIAVEIS PARA CONFIGURACOES!
            _channel.ExchangeDeclare("sme.sr.workers", ExchangeType.Topic);
            _channel.QueueDeclare("sme.sr.workers.sgp", false, false, false, null);
            _channel.QueueBind("sme.sr.workers.sgp", "sme.sr.workers", "relatorios");

            services.AddSingleton(conexaoRabbit);
            services.AddSingleton(_channel);


        }
    }
}