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
using Microsoft.Identity.Web;
using Microsoft.IdentityModel.Logging;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Threading.Tasks;

namespace Giants.Web
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            this.Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers();
            services.AddApiVersioning(config => 
            {
                config.DefaultApiVersion = new ApiVersion(1, 0);
                config.AssumeDefaultVersionWhenUnspecified = true;
            });

            services.AddOpenApiDocument();

            services.AddApplicationInsightsTelemetry();

            IdentityModelEventSource.ShowPII = true;

            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
              .AddMicrosoftIdentityWebApi(options =>
                  {
                      Configuration.Bind("AzureAd", options);
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
                          string[] allowedClientApps = this.Configuration.GetValue<string>("AllowedClientIds").Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);

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
                      Configuration.Bind("AzureAd", options);
                  });

            services.AddHttpContextAccessor();
            services.TryAddSingleton<IActionContextAccessor, ActionContextAccessor>();
            
            ServicesModule.RegisterServices(services, this.Configuration);

            IMapper mapper = Services.Mapper.GetMapper();
            services.AddSingleton(mapper);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                Microsoft.IdentityModel.Logging.IdentityModelEventSource.ShowPII = true;
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
        }
    }
}
