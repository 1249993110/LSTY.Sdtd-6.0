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

            #region 数据库

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

            #endregion 数据库

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
                // 不需要的理应不要
                options.SuppressConsumesConstraintForFormFileParameters = true;
                // options.SuppressInferBindingSourcesForParameters = true;
                // options.SuppressModelStateInvalidFilter = true;
                // options.SuppressMapClientErrors = true;
            })

            #region 全球化&本地化

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

            #endregion 全球化&本地化

            #region 认证&授权

            services.AddAuthentication(options =>
            {
                options.DefaultScheme = AuthenticationSchemes.ApiKeyAuthenticationSchemeName;
            }).AddApiKeyAuthentication(options =>
            {
                options.AccessToken = config.GetSection("AccessToken").Value;
            });

            // 添加授权处理器，这里不能使用 TryAdd，否则只会添加一个 IAuthorizationHandler
            services.AddScoped<IAuthorizationHandler, ApiKeyAuthorizationHandler>();
            // 添加授权策略服务
            services.AddAuthorization(options =>
            {
                // https://blog.csdn.net/sD7O95O/article/details/105382881
                // InvokeHandlersAfterFailure 为 true 的情况下（默认为 true ），所有注册了的 AuthorizationHandler 都会被执行
                options.InvokeHandlersAfterFailure = false;

                // 如果资源具有任何 IAuthorizeData 实例，则将对它们进行评估，而不是回退策略
                options.FallbackPolicy = new AuthorizationPolicyBuilder()
                    .RequireAuthenticatedUser()
                    //.AddRequirements(new PermissionRequirement())
                    .Build();
            });

            #endregion 认证&授权

            #region Swagger文档
            bool enableSwagger = config.GetSection("EnableSwagger").Get<bool>();
            if (enableSwagger)
            {
                // 根据服务的ServiceType和ImplementationType进行判断，如果已存在对应的服务则不添加，适用于为同一个服务添加多个不同的实现的场景
                services.TryAddEnumerable(ServiceDescriptor.Singleton<IApplicationModelProvider, ResponseTypeModelProvider>());

                // Register the Swagger services
                services.AddOpenApiDocument(config =>
                {
                    config.GenerateEnumMappingDescription = true;
                    config.PostProcess = document =>
                    {
                        document.Info.Version = "v1";
                        document.Info.Title = "顺德AQI接口";
                        document.Info.Description = "A simple ASP.NET Core web API";
                        document.Info.TermsOfService = "https://hycx-gd.cn";
                        document.Info.Contact = new OpenApiContact()
                        {
                            Name = "华云创信（广东）生态环境科技有限公司",
                            Email = "hycx2019@ciestgd.com",
                            Url = "https://hycx-gd.cn"
                        };
                    //document.Info.License = new OpenApiLicense
                    //{
                    //    Name = "Use under LICX",
                    //    Url = "https://github.com/1249993110"
                    //};
                };

                // 可以设置从注释文件加载，但是加载的内容可被 OpenApiTagAttribute 特性覆盖
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
            #endregion Swagger文档

            #region 适配反向代理

            services.Configure<ForwardedHeadersOptions>(options =>
            {
                options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;

                // 在 ForwardedHeadersMiddleware 中间件代码第268行 CheckKnownAddress 方法
                // 会检查访问的IP是否在 ForwardedHeadersOptions.KnownProxies 或者 ForwardedHeadersOptions.KnownNetworks 之中
                // 通过清空 KnownNetworks 和 KnownProxies 的默认值来不执行严格匹配，这样做有可能受到 IP欺骗 攻击
                options.KnownNetworks.Clear();
                options.KnownProxies.Clear();
            });

            #endregion 适配反向代理

            #region 跨域
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

            #endregion 跨域
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
            // UseCors 必须在 UseAuthorization 之前在 UseRouting 之后调用
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
        /// 初始化数据库
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