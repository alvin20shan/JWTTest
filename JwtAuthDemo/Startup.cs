using JWTAuthDemo;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace JwtAuthDemo
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
            //services.AddControllers();
            services.AddControllersWithViews();
            //添加配置JWT参数
            services.Configure<JwtConfig>(Configuration.GetSection("JwtConfig"));
            var jwtConfig = Configuration.GetSection("JwtConfig").Get<JwtConfig>();

            services.AddAuthentication(o =>
            {
                o.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                o.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            }).AddJwtBearer(o =>
            {
                o.RequireHttpsMetadata = false;
                o.SaveToken = true;
                o.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters()
                {
                    ValidateIssuerSigningKey = true,//是否调用对签名securityToken的SecurityKey进行验证
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtConfig.SigningKey)),
                    ValidIssuer = jwtConfig.Issuer,//将用于检查令牌的发行者是否与此发行者相同
                    ValidAudience = jwtConfig.Audience,//检查令牌的受众群体是否与此受众群体相同
                    ValidateLifetime=jwtConfig.ValidateLifetime,
                    ValidateIssuer = false,//是否验证发行者
                    ValidateAudience = false//在令牌验证期间验证受众
                };  
            });
            //添加swagger服务
            services.AddSwaggerGen(s =>
            {
                //定义由Swagger生成器创建的一个或多个文档
                s.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo()
                {
                    Title = "Swagger测试",
                    Description = "这是一个swagger测试接口",
                    Version = "v1",
                    TermsOfService = new Uri("https://test.com"), // A URL to the Terms of Service for the API. MUST be in the format of a URL.   API服务条款的URL

                    Contact = new Microsoft.OpenApi.Models.OpenApiContact()
                    {
                        Email = "shanshanyouwen@126.com",
                        Name = "shanshanyouwen"
                    },
                    License = new Microsoft.OpenApi.Models.OpenApiLicense()
                    {

                        Name = "SwaggerLicense",
                        Url = new Uri("https://test.com")
                    }

                 
                });

               

                //将 Swagger 配置为使用按照上述说明生成的 XML 文件。 对于 Linux 或非 Windows 操作系统，文件名和路径区分大小写。 例如，TodoApi.XML 文件在 Windows 上有效，但在 CentOS 上无效。
                var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
                // AppContext.BaseDirectory属性用于构造 XML 文件的路径。 一些 Swagger 功能（例如，输入参数的架构，或各自属性中的 HTTP 方法和响应代码）无需使用 XML 文档文件即可起作用。 对于大多数功能（即方法摘要以及参数说明和响应代码说明），必须使用 XML 文件。
                s.IncludeXmlComments(xmlPath);

                //swagger 添加 JWT 验证
                s.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
                {
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.ApiKey,
                    Description = "需要在请求头中框中输入Jwt授权Token: Bearer Token",
                    Name = "Authorization",
                    BearerFormat = "JWT",
                    Scheme = "Bearer"
                });
                s.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                           Reference = new OpenApiReference
                           {
                                Type = ReferenceType.SecurityScheme,
                                Id = "Bearer"
                           }
                        }, new string[] { }
                    }
                });

            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
            }
            app.UseStaticFiles();

        

            // Enable middleware to serve generated Swagger as a JSON endpoint. 允许中间件将生成的Swagger用作JSON端点。
            // Register the Swagger middleware with optional setup action for DI-injected options 使用DI注入选项的可选设置操作注册Swagger中间件 
            app.UseSwagger();

            //Register the SwaggerUI middleware with optional setup action for DI-injected  为注入的DI注册带有可选设置操作的SwaggerUI中间件
            //Enable middleware to serve swagger-ui (HTML, JS, CSS, etc.) 使中间件能够为swagger ui（HTML、JS、CSS等）提供服务
            app.UseSwaggerUI();
            app.UseRouting();
            //jwt
            app.UseAuthentication();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            { 
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}
