using IceCoffee.AspNetCore;
using IceCoffee.AspNetCore.Authentication;
using IceCoffee.AspNetCore.Authorization;
using IceCoffee.AspNetCore.Extensions;
using IceCoffee.AspNetCore.Middlewares;
using IceCoffee.AspNetCore.Models.Primitives;
using IceCoffee.AspNetCore.Resources;
using IceCoffee.DbCore;
using IceCoffee.DbCore.Utils;
using LSTY.Sdtd.Data;
using LSTY.Sdtd.Services.Extensions;
using LSTY.Sdtd.Services.Models;
using LSTY.Sdtd.WebApi.Middlewares;
using LSTY.Sdtd.WebApi.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.Extensions.DependencyInjection.Extensions;
using NSwag;
using Serilog;
using System.Globalization;
using System.Text;

[assembly: ApiController]

namespace LSTY.Sdtd.WebApi
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            Log.Logger = new LoggerConfiguration()
                 .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss.fff} {Level:u3}] {Message:lj}{NewLine}{Exception}")
                 .CreateBootstrapLogger();

            Log.Information("Starting up!");

            try
            {
                var builder = WebApplication.CreateBuilder(args);
                builder.Host.UseSerilog((context, services, configuration) => configuration
                    .ReadFrom.Configuration(context.Configuration)
                    .ReadFrom.Services(services)
                    .Enrich.FromLogContext());

                builder.ConfigureServices();

                var app = builder.Build();
                app.Configure();

                app.Run();

                Log.Information("Stopped cleanly");
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "An unhandled exception occured during bootstrapping");
            }
            finally
            {
                Log.CloseAndFlush();
            }
        }

        private static void ConfigureServices(this WebApplicationBuilder builder)
        {
            var env = builder.Environment;
            var config = builder.Configuration;
            var services = builder.Services;

            services.Configure<AppSettings>(config);
            services.ConfigureWritable<FunctionSettings>(config.GetSection(nameof(FunctionSettings)));
            services.AddSdtd();

            #region ���ݿ�

            var dbConnectionInfo = config.GetSection("DbConnectionInfos").GetSection("DefaultDbConnectionInfo").Get<DefaultDbConnectionInfo>();

            InitializeDatabase(dbConnectionInfo, env);

            services.TryAddSingleton(dbConnectionInfo);

            foreach (var type in typeof(DefaultDbConnectionInfo).Assembly.GetExportedTypes())
            {
                if (type.Namespace != null && type.Namespace.StartsWith("LSTY.Sdtd.Data.Repositories"))
                {
                    var interfaceType = type.GetInterfaces().First(
                        p => p.Namespace != null && p.Namespace.StartsWith("LSTY.Sdtd.Data.IRepositories"));
                    services.TryAddSingleton(interfaceType, type);
                }
            }

            #endregion ���ݿ�

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

            #endregion ȫ��&���ػ�

            #region ��֤&��Ȩ

            services.AddAuthentication(options =>
            {
                options.DefaultScheme = AuthenticationSchemes.ApiKeyAuthenticationSchemeName;
            }).AddApiKeyAuthentication(options =>
            {
                options.AccessToken = config.GetSection("AccessToken").Value;
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
                    //.AddRequirements(new PermissionRequirement())
                    .Build();
            });

            #endregion ��֤&��Ȩ

            #region Swagger�ĵ�
            bool enableSwagger = config.GetSection("EnableSwagger").Get<bool>();
            if (enableSwagger)
            {
                // ���ݷ����ServiceType��ImplementationType�����жϣ�����Ѵ��ڶ�Ӧ�ķ�������ӣ�������Ϊͬһ��������Ӷ����ͬ��ʵ�ֵĳ���
                services.TryAddEnumerable(ServiceDescriptor.Singleton<IApplicationModelProvider, ResponseTypeModelProvider>());

                // Register the Swagger services
                services.AddOpenApiDocument(config =>
                {
                    config.GenerateEnumMappingDescription = true;
                    config.PostProcess = document =>
                    {
                        document.Info.Version = "v1";
                        document.Info.Title = "˳��AQI�ӿ�";
                        document.Info.Description = "A simple ASP.NET Core web API";
                        document.Info.TermsOfService = "https://hycx-gd.cn";
                        document.Info.Contact = new OpenApiContact()
                        {
                            Name = "���ƴ��ţ��㶫����̬�����Ƽ����޹�˾",
                            Email = "hycx2019@ciestgd.com",
                            Url = "https://hycx-gd.cn"
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
            }
            #endregion Swagger�ĵ�

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

            #endregion ���䷴�����

            #region ����
            bool enableCors = config.GetSection("EnableCors").Get<bool>();
            if (enableCors)
            {
                services.AddCors(options =>
                {
                    options.AddPolicy("Cors", builder =>
                    {
                        builder.AllowAnyOrigin();
                        builder.AllowAnyHeader();
                        builder.AllowAnyMethod();
                    });
                });
            }

            #endregion ����
        }

        private static void Configure(this WebApplication app)
        {
            var env = app.Environment;
            var config = app.Configuration;

            app.UseSerilogRequestLogging();
            app.UseMiddleware<GlobalExceptionHandleMiddleware>();
            app.UseForwardedHeaders();
            app.UsePathBase("/api");

            app.Use(async (context, next) =>
            {
                if (context.Request.Path == "/")
                {
                    context.Response.Redirect("/index.html");
                    return;
                }

                await next();
            });

            app.UseWebSockets();
            app.UseMiddleware<WebSocketMiddleware>();

            bool enableSwagger = config.GetSection("EnableSwagger").Get<bool>();
            if (enableSwagger)
            {
                // Register the Swagger generator and the Swagger UI middlewares
                app.UseOpenApi(config =>
                {
                    config.PostProcess = (document, httpRequest) =>
                    {
                        document.BasePath = httpRequest.PathBase;
                    };
                });
                app.UseSwaggerUi3();
            }

            app.UseStaticFiles();

            app.UseRouting();
            // UseCors ������ UseAuthorization ֮ǰ�� UseRouting ֮�����
            bool enableCors = config.GetSection("EnableCors").Get<bool>();
            if (enableCors)
            {
                app.UseCors("Cors");
            }

            app.UseAuthentication();
            app.UseAuthorization();
            app.UseRequestLocalization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }

        /// <summary>
        /// ��ʼ�����ݿ�
        /// </summary>
        private static void InitializeDatabase(DbConnectionInfo dbConnectionInfo, IWebHostEnvironment webHostEnvironment)
        {
            try
            {
                string sql = File.ReadAllText(Path.Combine(webHostEnvironment.ContentRootPath, "Data/patrons.sql"), Encoding.UTF8);

                DBHelper.ExecuteSql(dbConnectionInfo, sql);
            }
            catch (Exception ex)
            {
                throw new Exception("Initialize database error", ex);
            }
        }
    }
}