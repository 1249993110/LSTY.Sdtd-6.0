using IceCoffee.AspNetCore;
using IceCoffee.AspNetCore.Authentication;
using IceCoffee.AspNetCore.Authorization;
using IceCoffee.AspNetCore.Extensions;
using IceCoffee.AspNetCore.Middlewares;
using IceCoffee.AspNetCore.Models.Primitives;
using IceCoffee.AspNetCore.Options;
using IceCoffee.AspNetCore.Permission;
using IceCoffee.AspNetCore.Resources;
using IceCoffee.Common.Extensions;
using IceCoffee.DbCore;
using IceCoffee.DbCore.ExceptionCatch;
using IceCoffee.DbCore.Utils;
using LSTY.Sdtd.Data;
using LSTY.Sdtd.Services;
using LSTY.Sdtd.Services.HubReceivers;
using LSTY.Sdtd.Services.Models;
using LSTY.Sdtd.Services.Models.Configs;
using LSTY.Sdtd.Shared.Hubs;
using LSTY.Sdtd.WebApi.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using NSwag;
using Serilog;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

[assembly: ApiController]
namespace LSTY.Sdtd.WebApi
{
    public class Startup
    {
        private const string _corsPolicyName = "CorsPolicy";
        public Startup(IConfiguration configuration, IWebHostEnvironment env)
        {
            Configuration = configuration;
             WebHostEnvironment = env;
        }

        public IConfiguration Configuration { get; }

        public IWebHostEnvironment WebHostEnvironment { get; }

        /// <summary>
        /// ��ʼ�����ݿ�
        /// </summary>
        private void InitializeDatabase(DbConnectionInfo dbConnectionInfo)
        {
            try
            {
                string sql = File.ReadAllText(Path.Combine(WebHostEnvironment.ContentRootPath, "Data/patrons.sql"), Encoding.UTF8);

                DBHelper.ExecuteSql(dbConnectionInfo, sql);
            }
            catch (Exception ex)
            {
                throw new DbCoreException("Initialize database error", ex);
            }
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.Configure<AppSettings>(Configuration);
            services.ConfigureWritable<FunctionConfigs>(Configuration.GetSection(nameof(FunctionConfigs)));

            services.AddHostedService<Worker>();
            services.TryAddSingleton<SignalRManager>();
            services.TryAddSingleton<IOnlinePlayers, OnlinePlayers>();
            services.TryAddSingleton<ModEventHookHubReceiver>();
            services.TryAddSingleton<ServerManageHubReceiver>();
            services.TryAddSingleton<FunctionFactory>();

            services.AddResponseCompression();
            services.AddMemoryCache();

            #region ���ݿ�
            var dbConnectionInfo = new DefaultDbConnectionInfo("DefaultConnection", Configuration.GetConnectionString("DefaultConnection"));
            services.TryAddSingleton(dbConnectionInfo);
            InitializeDatabase(dbConnectionInfo);

            foreach (var type in typeof(DefaultDbConnectionInfo).Assembly.GetExportedTypes())
            {
                if (type.FullName.StartsWith("LSTY.Sdtd.Data.Repositories"))
                {
                    var interfaceType = type.GetInterfaces().First(p => p.FullName.StartsWith("LSTY.Sdtd.Data.IRepositories"));
                    services.TryAddSingleton(interfaceType, type);
                }
            }
            #endregion

            services.AddControllers().ConfigureApiBehaviorOptions(options =>
            {
                options.InvalidModelStateResponseFactory = context =>
                {
                    string messages = string.Join(Environment.NewLine,
                        context.ModelState.Values.SelectMany(s => s.Errors).Select(s => s.ErrorMessage));

                    var result = new Response()
                    {
                        Code = CustomStatusCode.BadRequest,
                        Title = "One or more model validation errors occurred",
                        Message = messages
                    };

                    return (result as IConvertToActionResult).Convert();
                };
                // ����Ҫ����Ӧ��Ҫ
                options.SuppressConsumesConstraintForFormFileParameters = true;
                // options.SuppressInferBindingSourcesForParameters = true;
                // options.SuppressModelStateInvalidFilter = true;
                // options.SuppressMapClientErrors = true;
            })
            #region ȫ��&���ػ�
            .AddDataAnnotationsLocalization(options =>
            {
                options.DataAnnotationLocalizerProvider = (type, factory) =>
                {
                    return factory.Create(typeof(DataAnnotationsResource));
                };
            });

            services.AddLocalization();
            services.Configure<RequestLocalizationOptions>(options =>
            {
                var supportedCultures = new CultureInfo[]
                {
                    new CultureInfo("en"),
                    new CultureInfo("zh")
                };

                // State what the default culture for your application is. This will be used if no specific culture
                // can be determined for a given request.
                options.DefaultRequestCulture = new RequestCulture("zh");

                // You must explicitly state which cultures your application supports.
                // These are the cultures the app supports for formatting numbers, dates, etc.
                options.SupportedCultures = supportedCultures;

                // These are the cultures the app supports for UI strings, i.e. we have localized resources for.
                options.SupportedUICultures = supportedCultures;
            });
            #endregion

            #region ��֤&��Ȩ
            services.AddAuthentication(options => 
            {
                options.DefaultScheme = AuthenticationSchemes.ApiKeyAuthenticationSchemeName;
            }).AddApiKeyAuthentication(options => 
            { 
                options.AccessToken = Configuration.GetSection("AccessToken").Value; 
            });

            // �����Ȩ�����������ﲻ��ʹ�� TryAdd������ֻ�����һ�� IAuthorizationHandler
            services.AddScoped<IAuthorizationHandler, ApiKeyAuthorizationHandler>();
            // �����Ȩ���Է���
            services.AddAuthorization(options =>
            {
                // https://blog.csdn.net/sD7O95O/article/details/105382881
                // InvokeHandlersAfterFailure Ϊ true ������£�Ĭ��Ϊ true ��������ע���˵� AuthorizationHandler ���ᱻִ��
                options.InvokeHandlersAfterFailure = false;

                // �����Դ�����κ� IAuthorizeData ʵ�����򽫶����ǽ��������������ǻ��˲���
                options.FallbackPolicy = new AuthorizationPolicyBuilder()
                    .RequireAuthenticatedUser()
                    .AddRequirements(new PermissionRequirement(PermissionType.All))
                    .Build();
            });
            #endregion

            #region Swagger�ĵ�
            services.TryAddEnumerable(ServiceDescriptor.Transient<IApplicationModelProvider, ResponseTypeModelProvider>());

            // Register the Swagger services
            services.AddOpenApiDocument(config =>
            {
                config.GenerateEnumMappingDescription = true;
                config.PostProcess = document =>
                {
                    document.Info.Version = "v1";
                    document.Info.Title = "LSTY 7daytodie API";
                    document.Info.Description = "A simple ASP.NET Core web API";
                    document.Info.TermsOfService = "https://7daystodei.top"; 
                    document.Info.Contact = new OpenApiContact()
                    {
                        Name = "IceCoffee",
                        Email = "1249993110@qq.com",
                        Url = "https://github.com/1249993110"
                    };
                    //document.Info.License = new OpenApiLicense
                    //{
                    //    Name = "Use under LICX",
                    //    Url = "https://github.com/1249993110"
                    //};
                };

                // �������ô�ע���ļ����أ����Ǽ��ص����ݿɱ� OpenApiTagAttribute ���Ը���
                config.UseControllerSummaryAsTagDescription = true;

                config.AddSecurity("ApiKey", new OpenApiSecurityScheme()
                {
                    Type = OpenApiSecuritySchemeType.ApiKey,
                    Scheme = AuthenticationSchemes.ApiKeyAuthenticationSchemeName,
                    Name = ApiKeyAuthenticationHandler.HttpRequestHeaderName,
                    In = OpenApiSecurityApiKeyLocation.Header,
                    Description = "Type into the textbox: {your access-token}."
                });

                config.OperationProcessors.Add(new AspNetCoreOperationFallbackPolicyProcessor("ApiKey"));
            });
            #endregion

            #region ���䷴�����
            services.Configure<ForwardedHeadersOptions>(options =>
            {
                options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;

                // �� ForwardedHeadersMiddleware �м�������268�� CheckKnownAddress ����
                // ������ʵ�IP�Ƿ��� ForwardedHeadersOptions.KnownProxies ���� ForwardedHeadersOptions.KnownNetworks ֮��
                // ͨ����� KnownNetworks �� KnownProxies ��Ĭ��ֵ����ִ���ϸ�ƥ�䣬�������п����ܵ� IP��ƭ ����
                options.KnownNetworks.Clear();
                options.KnownProxies.Clear();
            });
            #endregion

            #region ����
            services.AddCors(options =>
            {
                options.AddPolicy(_corsPolicyName, builder =>
                {
                    string[] origins = Configuration.GetSection("Cors").Get<string[]>();
                    if (origins == null || origins.Length == 0)
                    {
                        builder.AllowAnyOrigin();
                    }
                    else
                    {
                        builder.WithOrigins(origins);
                    }

                    builder.AllowAnyHeader();
                    builder.AllowAnyMethod();
                });
            });
            #endregion
        }

        public void Configure(IApplicationBuilder app)
        {
            if (WebHostEnvironment.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseMiddleware<GlobalExceptionHandleMiddleware>();
            }

            app.UseForwardedHeaders();
            app.UseSerilogRequestLogging();

            app.Use(async (context, next) =>
            {
                if (context.Request.Path == "/")
                {
                    context.Response.Redirect("/index.html");
                    return;
                }

                await next();
            });

            // Register the Swagger generator and the Swagger UI middlewares
            app.UseOpenApi(config => 
            {
                config.PostProcess = (document, httpRequest) =>
                {
                    document.BasePath = "/api";
                };
            });
            app.UseSwaggerUi3();

            app.UseStaticFiles();

            app.UsePathBase("/api");
            app.UseRouting();

            if(WebHostEnvironment.IsProduction() && Configuration.GetValue<bool>("UseResponseCompression"))
            {
                app.UseResponseCompression();
            }

            // UseCors ������ UseAuthorization ֮ǰ�� UseRouting ֮�����
            if (WebHostEnvironment.IsDevelopment())
            {
                app.UseCors(_corsPolicyName);
            }

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseRequestLocalization();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
