using System;
using System.IO;
using System.Linq;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNet.OData.Extensions;
using Microsoft.AspNet.OData.Builder;
using Microsoft.IdentityModel.Tokens;
using GpEnerSaf.Models;
using GpEnerSaf.Authentication;
using GpEnerSaf.Services;
using GpEnerSaf.Repositories;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Infrastructure;

namespace GpEnerSaf
{
    public partial class Startup
    {
        public Startup(IConfiguration configuration, IWebHostEnvironment env)
        {
            Configuration = configuration;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            OnConfiguringServices(services);
            AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
            AppContext.SetSwitch("Npgsql.DisableDateTimeInfinityConversions", true);

            services.AddOptions();
            services.AddLogging(logging =>
            {
                logging.AddConsole();
                logging.AddDebug();
            });

            services.AddCors(options =>
            {
                options.AddPolicy(
                    "AllowAny",
                    x =>
                    {
                        x.AllowAnyHeader()
                        .AllowAnyMethod()
                        .SetIsOriginAllowed(isOriginAllowed: _ => true)
                        .AllowCredentials();
                    });
            });
            services.AddMvc(options =>
            {
                options.EnableEndpointRouting = false;
            }).AddNewtonsoftJson();

            services.AddAuthorization();
      
            services.AddOData();
            services.AddODataQueryFilter();

            services.AddHttpContextAccessor();
            var tokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = TokenProviderOptions.Key,
                ValidateIssuer = true,
                ValidIssuer = TokenProviderOptions.Issuer,
                ValidateAudience = true,
                ValidAudience = TokenProviderOptions.Audience,
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero
            };

            services.AddAuthentication(options =>
            {
                    options.DefaultScheme = Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerDefaults.AuthenticationScheme;
            }).AddJwtBearer(options =>
            {
                    options.Audience = TokenProviderOptions.Audience;
                    options.ClaimsIssuer = TokenProviderOptions.Issuer;
                    options.TokenValidationParameters = tokenValidationParameters;
                    options.SaveToken = true;
            });

            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>(); services.AddSingleton<IActionContextAccessor, ActionContextAccessor>();
            services.AddIdentity<ApplicationUser, IdentityRole>();
            services.Configure<ApplicationUserManagerOptions>(Configuration.GetSection("LDAPConfig"));
            services.AddScoped<IEnerSafService, EnersafServiceImpl>();
            services.AddScoped<IGPRepository, GPRepositoryImpl>();
            services.AddScoped<IEnersincRepository, EnersincRepositoryImpl>();
            services.AddScoped<ISAFIRORepository, SAFIRORepositoryImpl>();
            services.AddScoped<IUserClaimsPrincipalFactory<ApplicationUser>, ApplicationPrincipalFactory>();
            services.AddScoped<IUserStore<ApplicationUser>, ApplicationUserStore>();
            services.AddScoped<UserManager<ApplicationUser>, ApplicationUserManager>();
            services.AddScoped<IRoleStore<IdentityRole>, ApplicationRoleStore>();

            services.AddDbContext<GpEnerSaf.Data.DbGpContext>(options =>
            {
                options.UseNpgsql(Configuration.GetConnectionString("DBGPConnection"));
            });

            OnConfigureServices(services);
        }
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {

            OnConfiguring(app, env);

            IServiceProvider provider = app.ApplicationServices.GetRequiredService<IServiceProvider>();
            app.UseCors("AllowAny");
            app.Use(async (context, next) => {
                if (context.Request.Path.Value == "/__ssrsreport" || context.Request.Path.Value == "/ssrsproxy") {
                    await next();
                    return;
                }
                await next();
                if ((context.Response.StatusCode == 404 || context.Response.StatusCode == 401) && !Path.HasExtension(context.Request.Path.Value) && !context.Request.Path.Value.Contains("/odata")) {
                    context.Request.Path = "/index.html";
                    await next();
                }
            });

            app.UseDefaultFiles();
            app.UseStaticFiles();
            app.UseDeveloperExceptionPage();

            app.UseMvc(builder =>
            {
                if (env.EnvironmentName == "Development")
                {
                    builder.MapRoute(name: "default",
                        template: "{controller}/{action}/{id?}",
                        defaults: new { controller = "Home", action = "Index" }
                    );
                }

                builder.Count().Filter().OrderBy().Expand().Select().MaxTop(null).SetTimeZoneInfo(TimeZoneInfo.Utc);
            });

            OnConfigure(app);
            OnConfigure(app, env);
        }

        partial void OnConfigure(IApplicationBuilder app);
        partial void OnConfigure(IApplicationBuilder app, IWebHostEnvironment env);
        partial void OnConfiguring(IApplicationBuilder app, IWebHostEnvironment env);
        public IConfiguration Configuration { get; }
        partial void OnConfigureServices(IServiceCollection services);
        partial void OnConfiguringServices(IServiceCollection services);

    }
}
