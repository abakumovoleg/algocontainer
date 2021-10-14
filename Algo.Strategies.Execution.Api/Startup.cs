using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Algo.Abstracts.Interfaces;
using Algo.Container;
using Algo.Container.Connectors.Bcs.FIX;
using Algo.Strategies.Execution.Api.Controllers;
using Algo.Strategies.Execution.Api.Services;
using Micro.Api;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

namespace Algo.Strategies.Execution.Api
{
    public class Startup
    {
        public Startup(IConfiguration configuration, IHostEnvironment environment)
        {
            Configuration = configuration;
            Environment = environment;
        }

        public IConfiguration Configuration { get; }
        public IHostEnvironment Environment { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddCors(options =>
            {
                options.AddPolicy(name: "cors",
                    builder =>
                    {
                        builder.WithOrigins("*");
                        builder.AllowAnyMethod();
                        builder.AllowAnyHeader();
                    });
            });
             
            services.AddAuthentication(x =>
                {
                    x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                    x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                })
                .AddJwtBearer(x =>
                {
                    x.Authority = Configuration["Auth:Authority"];
                    x.Audience = Configuration["Auth:Audience"];
                    x.RequireHttpsMetadata = false;
                    x.SaveToken = true;
                    x.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuerSigningKey = false,
                        ValidateAudience = false
                    };
                }); 

            services.AddControllers()
                .AddJsonOptions(opts =>
                {
                    opts.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
                    opts.JsonSerializerOptions.Converters.Add(new StrategyParametersJsonConverter());
                });

            services.AddSwaggerGen(c =>
            {
                c.CustomSchemaIds(x => x.GetCustomAttributes<DisplayNameAttribute>().SingleOrDefault()?.DisplayName ?? x.Name);
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "Fixing algo API", Version = "v1" });
                c.CustomSchemaIds(x => x.FullName);
                c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    In = ParameterLocation.Header,
                    Description = "Please insert JWT with Bearer into field",
                    Name = "Authorization",
                    Type = SecuritySchemeType.ApiKey
                });
                c.AddSecurityRequirement(new OpenApiSecurityRequirement {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "Bearer"
                            }
                        },
                        new string[] { }
                    }
                });
            });
            services.AddSingleton<StrategyManager>();
            services.AddTransient<ILogger>(x=>x.GetRequiredService<ILogger<Startup>>());
            services.AddTransient<StrategyController>();
            services.AddSingleton<IMarketDepthProvider, MarketDepthProvider>();
            services.AddDirectoryBrowser();
            services.AddInfrastructureServices(Configuration);
            services.AddSingleton<ISecurityProvider, SecurityProvider>();
            services.AddKeyCloakClaimsTransformer();

            services.AddAutomapper(x =>
            {
                x.AddProfile<Model2ApiProfile>();
                x.AddProfile<Api2ModelProfile>();
            });
            
            services.AddSingleton<IConnector>(sp =>
            {
                var logger = sp.GetRequiredService<ILogger<Connector>>();

                var path = Path.GetFullPath(Path.Combine(
                    Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location),
                    $"FixConfig.{Environment.EnvironmentName}.txt"));
                
                var connector = new Connector(path, sp.GetRequiredService<ILoggerFactory>());

                connector.ConnectionChanged.Subscribe(x => logger.LogInformation(x.ToString()));
                 
                return connector;
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, IServiceProvider sp)
        {
             
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
 
            }

            app.UseMiddleware<ExceptionToResponseConverterMiddleware>();

            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "Fixing Algo API V1");
            });

           

            app.UseRouting();

            app.UseCors("cors");

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });


            _ = sp.GetRequiredService<ISecurityProvider>();

            var connector = sp.GetRequiredService<IConnector>();
            
            connector.Connect();
            connector.WaitForConnect();

            
        }
    }
}
