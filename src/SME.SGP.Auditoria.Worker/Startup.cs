using Dapper.FluentMap;
using Dapper.FluentMap.Dommel;
using Elastic.Apm.AspNetCore;
using Elastic.Apm.DiagnosticSource;
using Elastic.Apm.SqlClient;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using SME.SGP.Auditoria.Worker.Interfaces;
using System;
using System.Collections.Generic;
using Elasticsearch.Net;
using Nest;
using SME.SGP.Auditoria.Worker.Repositorio;
using SME.SGP.Auditoria.Worker.Repositorio.Interfaces;
using SME.SGP.Dados.ElasticSearch;
using SME.SGP.IoC;

namespace SME.SGP.Auditoria.Worker
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            RegistrarElasticSearch(services);
            RegistrarDependencias(services);
            RegistrarMapeamentos();
            RegistrarRabbitMQ(services);
            RegistrarTelemetria(services);

            services.AddHostedService<WorkerRabbitAuditoria>();
        }

        private void RegistrarTelemetria(IServiceCollection services)
        {
            services.AddOptions<TelemetriaOptions>()
                .Bind(Configuration.GetSection(TelemetriaOptions.Secao), c => c.BindNonPublicProperties = true);
            services.AddSingleton<TelemetriaOptions>();
        }

        private void RegistrarRabbitMQ(IServiceCollection services)
        {
            services.AddOptions<ConfiguracaoRabbitOptions>()
                .Bind(Configuration.GetSection(ConfiguracaoRabbitOptions.Secao), c => c.BindNonPublicProperties = true);

            var serviceProvider = services.BuildServiceProvider();
            var options = serviceProvider.GetService<IOptions<ConfiguracaoRabbitOptions>>().Value;

            services.AddSingleton<IConnectionFactory>(serviceProvider =>
            {
                var factory = new ConnectionFactory
                {
                    HostName = options.HostName,
                    UserName = options.UserName,
                    Password = options.Password,
                    VirtualHost = options.VirtualHost,
                    RequestedHeartbeat = System.TimeSpan.FromSeconds(options.TempoHeartBeat),
                };

                return factory;
            });
            services.AddSingleton<ConfiguracaoRabbitOptions>();
        }

        private void RegistrarMapeamentos()
        {
            FluentMapper.Initialize(config =>
            {
                config.AddMap(new AuditoriaMap());

                config.ForDommel();
            });
        }

        private void RegistrarElasticSearch(IServiceCollection services)
        {
            var elasticOptions = new ElasticOptions();
            Configuration.GetSection("ElasticSearchAuditoria").Bind(elasticOptions, c => c.BindNonPublicProperties = true);
            services.AddSingleton(elasticOptions);

            var nodes = new List<Uri>();

            if (elasticOptions.Urls.Contains(','))
            {
                string[] urls = elasticOptions.Urls.Split(',');
                foreach (string url in urls)
                    nodes.Add(new Uri(url));
            }
            else
            {
                nodes.Add(new Uri(elasticOptions.Urls));
            }
            var connectionPool = new StaticConnectionPool(nodes);
            var connectionSettings = new ConnectionSettings(connectionPool);
            connectionSettings.ServerCertificateValidationCallback((sender, cert, chain, errors) => true);
            connectionSettings.DefaultIndex(elasticOptions.DefaultIndex);

            if (!string.IsNullOrEmpty(elasticOptions.CertificateFingerprint))
                connectionSettings.CertificateFingerprint(elasticOptions.CertificateFingerprint);

            if (!string.IsNullOrEmpty(elasticOptions.Username) && !string.IsNullOrEmpty(elasticOptions.Password))
                connectionSettings.BasicAuthentication(elasticOptions.Username, elasticOptions.Password);
            
            var elasticClient = new ElasticClient(connectionSettings);
            services.AddSingleton<IElasticClient>(elasticClient);
        }

        private void RegistrarDependencias(IServiceCollection services)
        {
            services.ConfigurarTelemetria(Configuration);
            services.TryAddScoped<IRepositorioAuditoria, RepositorioAuditoria>();

            services.TryAddScoped<IRegistrarAuditoriaUseCase, RegistrarAuditoriaUseCase>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseElasticApm(Configuration,
                new SqlClientDiagnosticSubscriber(),
                new HttpDiagnosticsSubscriber());

            if (env.IsDevelopment())
                app.UseDeveloperExceptionPage();

            app.Run(async (context) =>
            {
                await context.Response.WriteAsync("WorkerRabbitAuditoria!");
            });
        }
    }
}
