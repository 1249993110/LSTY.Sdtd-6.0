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
        /// 初始化数据库
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

            #region 数据库
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
            #endregion

            #region 认证&授权
            services.AddAuthentication(options => 
            {
                options.DefaultScheme = AuthenticationSchemes.ApiKeyAuthenticationSchemeName;
            }).AddApiKeyAuthentication(options => 
            { 
                options.AccessToken = Configuration.GetSection("AccessToken").Value; 
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
                    .AddRequirements(new PermissionRequirement(PermissionType.All))
                    .Build();
            });
            #endregion

            #region Swagger文档
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
            #endregion

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
            #endregion

            #region 跨域
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

            // UseCors 必须在 UseAuthorization 之前在 UseRouting 之后调用
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
