using System;
using System.IO;
using System.Linq;
using System.Reflection;
using Abp.AspNetCore;
using Abp.AspNetCore.Configuration;
using Abp.AspNetCore.Mvc.Antiforgery;
using Abp.AspNetCore.Mvc.Extensions;
using Abp.AspNetCore.SignalR.Hubs;
using Abp.AspNetZeroCore.Web.Authentication.JwtBearer;
using Abp.Castle.Logging.Log4Net;
using Abp.Extensions;
using Abp.Hangfire;
using Abp.PlugIns;
using Castle.Facilities.Logging;
using Hangfire;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Chamran.Deed.Authorization;
using Chamran.Deed.Configuration;
using Chamran.Deed.EntityFrameworkCore;
using Chamran.Deed.Identity;
using Chamran.Deed.Web.Chat.SignalR;
using Chamran.Deed.Web.Common;
using Swashbuckle.AspNetCore.Swagger;
using Chamran.Deed.Web.IdentityServer;
using Chamran.Deed.Web.Swagger;
using Stripe;
using ILoggerFactory = Microsoft.Extensions.Logging.ILoggerFactory;
using GraphQL.Server;
using GraphQL.Server.Ui.Playground;
using HealthChecks.UI.Client;
using IdentityServer4.Configuration;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using Chamran.Deed.Configure;
using Chamran.Deed.Schemas;
using Chamran.Deed.Web.HealthCheck;
using Newtonsoft.Json.Serialization;
using Owl.reCAPTCHA;
using HealthChecksUISettings = HealthChecks.UI.Configuration.Settings;
using Microsoft.AspNetCore.Server.Kestrel.Https;
using Chamran.Deed.Web.MultiTenancy;
using Abp.AspNetCore.Localization;
using Chamran.Deed.Info;
using Chamran.Deed.Web.Helpers.StimulsoftHelpers;
using Chamran.Deed.Web.MiddleWares;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.AspNetCore.Http.Features;

namespace Chamran.Deed.Web.Startup
{
    public class Startup
    {
        private const string DefaultCorsPolicyName = "localhost";

        private readonly IConfigurationRoot _appConfiguration;
        private readonly IWebHostEnvironment _hostingEnvironment;

        public Startup(IWebHostEnvironment env)
        {
            _hostingEnvironment = env;
            _appConfiguration = env.GetAppConfiguration();
        }

        public IServiceProvider ConfigureServices(IServiceCollection services)
        {
            LicenseHelper.StimulsoftRegister();
            RemoveLimits(services);
            IsWorkaroundInit(services);

            services.AddControllersWithViews(options =>
            {
                options.Filters.Add(new AbpAutoValidateAntiforgeryTokenAttribute());
            })
#if DEBUG
            .AddRazorRuntimeCompilation()
#endif
            .AddNewtonsoftJson();

            services.AddSignalR();

            services.AddCors(options =>
            {
                options.AddPolicy(DefaultCorsPolicyName, builder =>
                {
                    builder
                        .WithOrigins(AppSettingProviders.Get(_appConfiguration, "App:CorsOrigins")
                            .Split(",", StringSplitOptions.RemoveEmptyEntries)
                            .Select(o => o.RemovePostFix("/"))
                            .ToArray())
                        .SetIsOriginAllowedToAllowWildcardSubdomains()
                        .AllowAnyHeader()
                        .AllowAnyMethod()
                        .AllowCredentials();
                });
            });

            if (AppSettingProvider.GetBool("KestrelServer__IsEnabled", _appConfiguration))
            {
                ConfigureKestrel(services);
            }

            IdentityRegistrar.Register(services);
            services.Configure<IdentityOptions>(options =>
            {
                options.Lockout.AllowedForNewUsers = false;
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.Zero;
                options.Lockout.MaxFailedAccessAttempts = int.MaxValue;
            });

            AuthConfigurer.Configure(services, _appConfiguration);

            if (bool.Parse(AppSettingProviders.Get(_appConfiguration, "IdentityServer:IsEnabled")))
            {
                IdentityServerRegistrar.Register(services, _appConfiguration, options =>
                    options.UserInteraction = new UserInteractionOptions()
                    {
                        LoginUrl = "/UI/Login",
                        LogoutUrl = "/UI/LogOut",
                        ErrorUrl = "/Error"
                    });
            }
            else
            {
                services.Configure<SecurityStampValidatorOptions>(opts =>
                {
                    opts.OnRefreshingPrincipal = SecurityStampValidatorCallback.UpdatePrincipal;
                });
            }

            if (WebConsts.SwaggerUiEnabled)
            {
                ConfigureSwagger(services);
            }

            services.AddreCAPTCHAV3(x =>
            {
                x.SiteKey = AppSettingProviders.Get(_appConfiguration, "Recaptcha:SiteKey");
                x.SiteSecret = AppSettingProviders.Get(_appConfiguration, "Recaptcha:SecretKey");
            });

            if (WebConsts.HangfireDashboardEnabled)
            {
                services.AddHangfire(config =>
                {
                    config.UseSqlServerStorage(AppSettingProviders.Get(_appConfiguration, "ConnectionStrings:Default"));
                });

                services.AddHangfireServer();
            }

            if (WebConsts.GraphQL.Enabled)
            {
                services.AddAndConfigureGraphQL();
            }

            if (bool.Parse(AppSettingProviders.Get(_appConfiguration, "HealthChecks:HealthChecksEnabled")))
            {
                ConfigureHealthChecks(services);
            }

            return services.AddAbp<DeedWebHostModule>(options =>
            {
                options.IocManager.IocContainer.AddFacility<LoggingFacility>(
                    f => f.UseAbpLog4Net().WithConfig(_hostingEnvironment.IsDevelopment()
                        ? "log4net.config"
                        : "log4net.Production.config")
                );

                options.PlugInSources.AddFolder(Path.Combine(_hostingEnvironment.WebRootPath, "Plugins"),
                    SearchOption.AllDirectories);
            });
        }

        private void IsWorkaroundInit(IServiceCollection services)
        {
            //services.AddSingleton<Autofac.IContainer, DummyAutofacContainer>();

            //// Register the custom service provider is service implementation
            //services.AddSingleton<IServiceProviderIsService, CustomServiceProviderIsService>();
        }

        private void RemoveLimits(IServiceCollection services)
        {

            //services.Configure<KestrelServerOptions>(options =>
            //{
            //    options.Limits.MaxRequestBodySize = null; // Set to null to remove the limit
            //    // or
            //    options.Limits.MaxRequestBodySize = new long?(); // An alternative way to set it to null
            //});

            //services.Configure<FormOptions>(options =>
            //{
            //    options.MultipartBodyLengthLimit = null; // Set to null to remove the limit
            //    // or
            //    options.MultipartBodyLengthLimit = new long?(); // An alternative way to set it to null
            //});

            services.Configure<KestrelServerOptions>(options =>
            {
                options.Limits.MaxRequestBodySize = null; // Set to the desired maximum size in bytes
            });

            services.Configure<FormOptions>(options =>
            {
                options.MultipartBodyLengthLimit = 524288000; // Set the maximum multipart body length (e.g., 100 MB)
            });
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, ILoggerFactory loggerFactory)
        {
            // اضافه کردن X-Frame-Options به هدر
            app.Use(async (context, next) =>
            {
                context.Response.Headers["X-Frame-Options"] = "SAMEORIGIN";
                await next();
            });

            app.UseMiddleware<ClickjackingMiddleware>();
            app.UseForwardedHeaders(new ForwardedHeadersOptions
            {
                ForwardedHeaders = ForwardedHeaders.All, //ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
                ForwardLimit = null

            });
            //Initializes ABP framework.
            app.UseAbp(options =>
            {
                options.UseAbpRequestLocalization = false; //used below: UseAbpRequestLocalization
            });

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseHsts();
            }
            else
            {
                app.UseStatusCodePagesWithRedirects("~/Error?statusCode={0}");
                app.UseExceptionHandler("/Error");
            }

            app.UseStaticFiles();

            //if (DeedConsts.PreventNotExistingTenantSubdomains)
            //{
            //    app.UseMiddleware<DomainTenantCheckMiddleware>();
            //}

            app.UseRouting();

            app.UseCors(DefaultCorsPolicyName); //Enable CORS!

            app.UseAuthentication();
            app.UseJwtTokenMiddleware();

            if (bool.Parse(_appConfiguration["IdentityServer:IsEnabled"]))
            {
                app.UseJwtTokenMiddleware("IdentityBearer");
                app.UseIdentityServer();
            }

            app.UseAuthorization();

            using (var scope = app.ApplicationServices.CreateScope())
            {
                if (scope.ServiceProvider.GetService<DatabaseCheckHelper>()
                    .Exist(_appConfiguration["ConnectionStrings:Default"]))
                {
                    var hostingEnv = scope.ServiceProvider.GetRequiredService<IWebHostEnvironment>();
                    EnsureDirectoryExists(Path.Combine(hostingEnv.WebRootPath, "thumbs"));
                    EnsureDirectoryExists(Path.Combine(hostingEnv.WebRootPath, "previews"));
                    EnsureDirectoryExists(Path.Combine(hostingEnv.WebRootPath, "videos"));
                    //app.UseAbpRequestLocalization();
                    app.UseAbpRequestLocalization(options =>
                    {
                        var headerProvider = options.RequestCultureProviders.OfType<AbpLocalizationHeaderRequestCultureProvider>().First();
                        headerProvider.HeaderName = "-AspNetCore-Culture";
                    });
                }
            }

            if (WebConsts.HangfireDashboardEnabled)
            {
                //Hangfire dashboard &server(Enable to use Hangfire instead of default job manager)
                app.UseHangfireDashboard(WebConsts.HangfireDashboardEndPoint, new DashboardOptions
                {
                    Authorization = new[]
                        {new AbpHangfireAuthorizationFilter(AppPermissions.Pages_Administration_HangfireDashboard)}
                });
            }

            if (bool.Parse(_appConfiguration["Payment:Stripe:IsActive"]))
            {
                StripeConfiguration.ApiKey = _appConfiguration["Payment:Stripe:SecretKey"];
            }

            if (WebConsts.GraphQL.Enabled)
            {
                app.UseGraphQL<MainSchema>();
                if (WebConsts.GraphQL.PlaygroundEnabled)
                {
                    app.UseGraphQLPlayground(
                        new GraphQLPlaygroundOptions()); //to explorer API navigate https://*DOMAIN*/ui/playground
                }
            }

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapHub<AbpCommonHub>("/signalr");
                endpoints.MapHub<ChatHub>("/signalr-chat");

                endpoints.MapControllerRoute("defaultWithArea", "{area}/{controller=Home}/{action=Index}/{id?}");
                endpoints.MapControllerRoute("default", "{controller=Home}/{action=Index}/{id?}");

                app.ApplicationServices.GetRequiredService<IAbpAspNetCoreConfiguration>().EndpointConfiguration.ConfigureAllEndpoints(endpoints);
            });

            if (bool.Parse(_appConfiguration["HealthChecks:HealthChecksEnabled"]))
            {
                if (bool.Parse(_appConfiguration["HealthChecks:HealthChecksUI:HealthChecksUIEnabled"]))
                {
                    app.UseHealthChecks("/health", new HealthCheckOptions()
                    {
                        Predicate = _ => true,
                        ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
                    });

                    app.UseHealthChecksUI();
                }
            }

            if (WebConsts.SwaggerUiEnabled)
            {
                // Enable middleware to serve generated Swagger as a JSON endpoint
                app.UseSwagger();
                // Enable middleware to serve swagger-ui assets (HTML, JS, CSS etc.)

                app.UseSwaggerUI(options =>
                {
                    options.SwaggerEndpoint(
                        AppSettingProviders.Get(_appConfiguration, "App:SwaggerEndPoint"),
                        "Deed API V1"
                    );
                    //options.IndexStream = () => Assembly.GetExecutingAssembly()
                    //    .GetManifestResourceStream("Chamran.Deed.Web.wwwroot.swagger.ui.index.html");
                    options.InjectBaseUrl(_appConfiguration["App:ServerRootAddress"]);
                }); //URL: /swagger
            }
        }

        private void ConfigureKestrel(IServiceCollection services)
        {
            services.Configure<Microsoft.AspNetCore.Server.Kestrel.Core.KestrelServerOptions>(options =>
            {
                options.Listen(new System.Net.IPEndPoint(System.Net.IPAddress.Any, 443),
                    listenOptions =>
                    {
                        var certPassword = _appConfiguration.GetValue<string>("Kestrel:Certificates:Default:Password");
                        var certPath = _appConfiguration.GetValue<string>("Kestrel:Certificates:Default:Path");
                        var cert = new System.Security.Cryptography.X509Certificates.X509Certificate2(certPath,
                            certPassword);
                        listenOptions.UseHttps(new HttpsConnectionAdapterOptions()
                        {
                            ServerCertificate = cert
                        });
                    });
            });
        }

        private void ConfigureSwagger(IServiceCollection services)
        {
            services.AddSwaggerGen(options =>
            {
                options.SwaggerDoc("v1", new OpenApiInfo() { Title = "Deed API", Version = "v1" });
                options.DocInclusionPredicate((docName, description) => true);
                options.ParameterFilter<SwaggerEnumParameterFilter>();
                options.SchemaFilter<SwaggerEnumSchemaFilter>();
                options.OperationFilter<SwaggerOperationIdFilter>();
                options.OperationFilter<SwaggerOperationFilter>();
                options.CustomDefaultSchemaIdSelector();

                //add summaries to swagger
                bool canShowSummaries = _appConfiguration.GetValue<bool>("Swagger:ShowSummaries");
                if (canShowSummaries)
                {
                    var hostXmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                    var hostXmlPath = Path.Combine(AppContext.BaseDirectory, hostXmlFile);
                    options.IncludeXmlComments(hostXmlPath);

                    var applicationXml = $"Chamran.Deed.Application.xml";
                    var applicationXmlPath = Path.Combine(AppContext.BaseDirectory, applicationXml);
                    options.IncludeXmlComments(applicationXmlPath);

                    var webCoreXmlFile = $"Chamran.Deed.Web.Core.xml";
                    var webCoreXmlPath = Path.Combine(AppContext.BaseDirectory, webCoreXmlFile);
                    options.IncludeXmlComments(webCoreXmlPath);
                }
            }).AddSwaggerGenNewtonsoftSupport();
        }

        private void ConfigureHealthChecks(IServiceCollection services)
        {
            services.AddAbpZeroHealthCheck();

            var healthCheckUISection = _appConfiguration.GetSection("HealthChecks")?.GetSection("HealthChecksUI");

            if (bool.Parse(healthCheckUISection["HealthChecksUIEnabled"]))
            {
                services.Configure<HealthChecksUISettings>(settings =>
                {
                    healthCheckUISection.Bind(settings, c => c.BindNonPublicProperties = true);
                });
                services.AddHealthChecksUI()
                    .AddInMemoryStorage();
            }
        }
        private void EnsureDirectoryExists(string path)
        {
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
        }

    }
}
