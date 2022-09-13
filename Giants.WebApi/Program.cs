namespace Giants.Web
{
    using Autofac;
    using Autofac.Extensions.DependencyInjection;
    using AutoMapper;
    using Giants.Services;
    using Microsoft.AspNetCore.Authentication.JwtBearer;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.Infrastructure;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.DependencyInjection.Extensions;
    using Microsoft.Extensions.Hosting;
    using Microsoft.Extensions.Logging;
    using Microsoft.Identity.Web;
    using Microsoft.IdentityModel.Logging;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net.Http.Headers;
    using System.Threading.Tasks;

    public class Program
    {
        private static readonly List<Module> AdditionalRegistrationModules = new();

        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Host.UseServiceProviderFactory(new AutofacServiceProviderFactory());
            builder.Host.ConfigureContainer<ContainerBuilder>((containerBuilder) => ConfigureAutofacServices(containerBuilder, builder.Configuration));

            ConfigureServices(builder);

            var app = builder
                .Build();
            ConfigureApplication(app, app.Environment);

            app.Run();
        }

        public static void AddAdditionalRegistrations(IList<Module> modules)
        {
            // Hack: ConfigureTestServices doesn't work with Autofac containers in .NET 6.
            // Add test registrations to a static list, to be registered last.
            AdditionalRegistrationModules.AddRange(modules);
        }

        private static void ConfigureServices(WebApplicationBuilder builder)
        {
            var services = builder.Services;

            services.AddControllers();
            services.AddApiVersioning(config =>
            {
                config.DefaultApiVersion = new ApiVersion(1, 0);
                config.AssumeDefaultVersionWhenUnspecified = true;
            });

            services.AddOpenApiDocument();

            services.AddApplicationInsightsTelemetry();

            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
              .AddMicrosoftIdentityWebApi(options =>
              {
                  builder.Configuration.Bind("AzureAd", options);
                  options.Events = new JwtBearerEvents();
                  options.Events.OnAuthenticationFailed = async context =>
                  {
                      await Task.CompletedTask;
                  };
                  options.Events.OnForbidden = async context =>
                  {
                      await Task.CompletedTask;
                  };
                  options.Events.OnChallenge = async context =>
                  {
                      await Task.CompletedTask;
                  };
                  options.Events.OnTokenValidated = async context =>
                  {
                      string[] allowedClientApps = builder.Configuration.GetValue<string>("AllowedClientIds").Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);

                      string clientAppId = context?.Principal?.Claims
                          .FirstOrDefault(x => x.Type == "azp" || x.Type == "appid")?.Value;

                      if (clientAppId == null || !allowedClientApps.Contains(clientAppId))
                      {
                          throw new UnauthorizedAccessException("The client app is not permitted to access this API");
                      }

                      await Task.CompletedTask;
                  };

              }, options =>
              {
                  builder.Configuration.Bind("AzureAd", options);
              });

            services.AddHttpContextAccessor();
            services.TryAddSingleton<IActionContextAccessor, ActionContextAccessor>();

            IMapper mapper = Services.Mapper.GetMapper();
            services.AddSingleton(mapper);

            services.AddHealthChecks();

            RegisterHttpClients(services, builder.Configuration);

            RegisterHostedServices(services);

            builder.Logging.AddEventSourceLogger();
            builder.Logging.AddApplicationInsights();
        }

        private static void ConfigureAutofacServices(ContainerBuilder containerBuilder, IConfiguration configuration)
        {
            containerBuilder.RegisterModule(new ServicesModule(configuration));

            foreach (var module in AdditionalRegistrationModules)
            {
                containerBuilder.RegisterModule(module);
            }
        }

        private static void RegisterHttpClients(IServiceCollection services, IConfiguration configuration)
        {
            services.AddHttpClient("Sentry", c =>
            {
                c.BaseAddress = new Uri(configuration["SentryBaseUri"]);

                string sentryAuthenticationToken = configuration["SentryAuthenticationToken"];
                c.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", sentryAuthenticationToken);
            });
        }

        private static void RegisterHostedServices(IServiceCollection services)
        {
            services.AddHostedService<ServerRegistryCleanupService>();
        }

        private static void ConfigureApplication(WebApplication app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                IdentityModelEventSource.ShowPII = true;
                app.UseDeveloperExceptionPage();
                app.UseOpenApi();
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });

            app.MapHealthChecks("/health");
        }
    }
}
