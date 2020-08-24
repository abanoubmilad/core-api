using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using AutoMapper;
using core_api.Data;
using core_api.Models;
using core_api.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.SpaServices.AngularCli;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Pomelo.EntityFrameworkCore.MySql.Infrastructure;

namespace core_api
{
    public class Startup
    {
        private readonly AppSettings _appSettings;

        public Startup(IConfiguration configuration)
        {
            _appSettings = new AppSettings(configuration);
        }

        public IConfiguration Configuration { get; }


        private void ConfigureAuth(IServiceCollection services)
        {
            services.AddIdentity<User, IdentityRole>(opt =>
            {
                opt.Password.RequiredLength = 6;
                opt.Password.RequireDigit = false;
                opt.Password.RequireNonAlphanumeric = false;
                opt.Password.RequireUppercase = false;
                opt.Password.RequireLowercase = false;

                opt.User.RequireUniqueEmail = true;

                opt.Lockout.AllowedForNewUsers = _appSettings.SecurityConfig.LockoutConfig.AllowedForNewUsers;
                opt.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(_appSettings.SecurityConfig.LockoutConfig.DefaultLockoutTimeSpanInMins);
                opt.Lockout.MaxFailedAccessAttempts = _appSettings.SecurityConfig.LockoutConfig.MaxFailedAccessAttempts;

            })
             .AddEntityFrameworkStores<DataContext>()
             .AddDefaultTokenProviders();

            services.Configure<DataProtectionTokenProviderOptions>(opt =>
            opt.TokenLifespan = TimeSpan.FromDays(1));

            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = _appSettings.SecurityConfig.JwtConfig.ValidateIssuer,
                    ValidateAudience = _appSettings.SecurityConfig.JwtConfig.ValidateAudience,
                    ValidateLifetime = _appSettings.SecurityConfig.JwtConfig.ValidateLifetime,
                    ValidateIssuerSigningKey = _appSettings.SecurityConfig.JwtConfig.ValidateIssuerSigningKey,
                    ValidIssuer = _appSettings.SecurityConfig.JwtConfig.Issuer,
                    ValidAudience = _appSettings.SecurityConfig.JwtConfig.Audience,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_appSettings.SecurityConfig.JwtConfig.Key))

                };
            })
            .AddFacebook(facebookOptions =>
            {
                facebookOptions.AppId = _appSettings.SecurityConfig.Facebook.Id;
                facebookOptions.AppSecret = _appSettings.SecurityConfig.Facebook.Secret;
            }); ;
        }

        private static void ConfigureServicesAndRepos(IServiceCollection services)
        {
            // repositories

            services.AddScoped<EfRepository>();

            // services

            services.AddSingleton<IEmailService, EmailService>();

            services.AddScoped<IAuthService, AuthService>();

            services.AddScoped<IFirmService, FirmService>();
            services.AddScoped<IBookingService, BookingService>();
            services.AddScoped<IUserService, UserService>();
            services.AddScoped<IMeetingService, MeetingService>();
        }

        private void ConfigureDocumentation(IServiceCollection services)
        {
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc(_appSettings.Documentation.Version,
                    new OpenApiInfo
                    {
                        Title = _appSettings.Documentation.Title
                    ,
                        Version = _appSettings.Documentation.Version
                    });
                c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Description = @"JWT Authorization header using the Bearer scheme. \r\n\r\n 
                      Enter 'Bearer' [space] and then your token in the text input below.
                      \r\n\r\nExample: 'Bearer 12345abcdef'",
                    Name = "Authorization",
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.ApiKey,
                    Scheme = "Bearer"
                });

                c.AddSecurityRequirement(new OpenApiSecurityRequirement(){
                        {
                          new OpenApiSecurityScheme
                          { Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme,    Id = "Bearer"   },
                              Scheme = "oauth2",
                              Name = "Bearer",
                              In = ParameterLocation.Header,

                            },
                            new List<string>()
                          }
                        });
                var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
                // todo include doc comments
                //  c.IncludeXmlComments(xmlPath);
            });
        }












        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            // db configuration
            services.AddDbContextPool<DataContext>(options => options
               .UseMySql(_appSettings.DatabaseConfig.ConnectionString,
               mySqlOptions => mySqlOptions
                    .ServerVersion(new Version(8, 0, 18), ServerType.MySql)
            ));

            ConfigureAuth(services);

            // cors
            services.AddCors();

            services.AddControllers();

            // app client
            // In production, the files will be served from this directory
            services.AddSpaStaticFiles(configuration =>
            {
                configuration.RootPath = "wwwroot";
            });

            services.AddAutoMapper(typeof(Startup));

            services.AddSingleton<AppSettings>(_appSettings);
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();

            ConfigureServicesAndRepos(services);

            // documentation
            if (_appSettings.Documentation.Enable)
            {
                ConfigureDocumentation(services);
            }
        }


        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env,
             UserManager<User> userManager, DataContext dataContext)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseDatabaseErrorPage();
            }
            else
            {
                // todo check both lines
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            // documentation
            if (_appSettings.Documentation.Enable)
            {
                app.UseSwagger();
                app.UseSwaggerUI(c =>
                {
                    c.SwaggerEndpoint("/swagger/v1/swagger.json", _appSettings.Documentation.Title);
                    c.RoutePrefix = _appSettings.Documentation.RoutePrefix;
                });
            }


            app.UseHttpsRedirection();

            // app client
            if (!env.IsDevelopment())
            {
                app.UseSpaStaticFiles();
            }

            // cors
            app.UseCors(options => options.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());


            app.UseRouting();

            app.UseAuthentication();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });

            // app client
            app.UseSpa(spa =>
            {
                spa.Options.SourcePath = "wwwroot";
            });



            if (_appSettings.DatabaseConfig.MigrateAtStart)
            {
                // run any required db migrations
                dataContext.Database.Migrate();
            }

            // todo
            if (_appSettings.HangfireConfig.UseDashboard)
            {
                // add the Dashboard UI to use all the Hangfire features
            }

            // todo optimize this
            // seeding admin user
            if (_appSettings.SecurityConfig.AdminConfig.SeedAdmin)
            {
                userManager.CreateAsync(new User()
                {
                    Email = _appSettings.SecurityConfig.AdminConfig.Email,
                    UserName = _appSettings.SecurityConfig.AdminConfig.Email,
                    Role = Role.Admin
                },
                  _appSettings.SecurityConfig.AdminConfig.Password
                  ).Wait();
            }
        }
    }
}
