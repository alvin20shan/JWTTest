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
            //�������JWT����
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
                    ValidateIssuerSigningKey = true,//�Ƿ���ö�ǩ��securityToken��SecurityKey������֤
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtConfig.SigningKey)),
                    ValidIssuer = jwtConfig.Issuer,//�����ڼ�����Ƶķ������Ƿ���˷�������ͬ
                    ValidAudience = jwtConfig.Audience,//������Ƶ�����Ⱥ���Ƿ��������Ⱥ����ͬ
                    ValidateLifetime=jwtConfig.ValidateLifetime,
                    ValidateIssuer = false,//�Ƿ���֤������
                    ValidateAudience = false//��������֤�ڼ���֤����
                };  
            });
            //���swagger����
            services.AddSwaggerGen(s =>
            {
                //������Swagger������������һ�������ĵ�
                s.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo()
                {
                    Title = "Swagger����",
                    Description = "����һ��swagger���Խӿ�",
                    Version = "v1",
                    TermsOfService = new Uri("https://test.com"), // A URL to the Terms of Service for the API. MUST be in the format of a URL.   API���������URL

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

               

                //�� Swagger ����Ϊʹ�ð�������˵�����ɵ� XML �ļ��� ���� Linux ��� Windows ����ϵͳ���ļ�����·�����ִ�Сд�� ���磬TodoApi.XML �ļ��� Windows ����Ч������ CentOS ����Ч��
                var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
                // AppContext.BaseDirectory�������ڹ��� XML �ļ���·���� һЩ Swagger ���ܣ����磬��������ļܹ�������������е� HTTP ��������Ӧ���룩����ʹ�� XML �ĵ��ļ����������á� ���ڴ�������ܣ�������ժҪ�Լ�����˵������Ӧ����˵����������ʹ�� XML �ļ���
                s.IncludeXmlComments(xmlPath);

                //swagger ��� JWT ��֤
                s.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
                {
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.ApiKey,
                    Description = "��Ҫ������ͷ�п�������Jwt��ȨToken: Bearer Token",
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

        

            // Enable middleware to serve generated Swagger as a JSON endpoint. �����м�������ɵ�Swagger����JSON�˵㡣
            // Register the Swagger middleware with optional setup action for DI-injected options ʹ��DIע��ѡ��Ŀ�ѡ���ò���ע��Swagger�м�� 
            app.UseSwagger();

            //Register the SwaggerUI middleware with optional setup action for DI-injected  Ϊע���DIע����п�ѡ���ò�����SwaggerUI�м��
            //Enable middleware to serve swagger-ui (HTML, JS, CSS, etc.) ʹ�м���ܹ�Ϊswagger ui��HTML��JS��CSS�ȣ��ṩ����
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
