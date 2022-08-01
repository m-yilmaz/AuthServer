using AuthServer.Core.Configuration;
using AuthServer.Core.Models;
using AuthServer.Core.Repositories;
using AuthServer.Core.Services;
using AuthServer.Core.UnitOfWork;
using AuthServer.Data;
using AuthServer.Data.Repositories;
using AuthServer.Service.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using SharedLibrary.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AuthServer.API
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
            //DI Register
            services.AddScoped<IAuthenticationService, AuthenticationService>();
            services.AddScoped<IUserService, UserService>();
            services.AddScoped<ITokenService, TokenService>();
            services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
            services.AddScoped(typeof(IServiceGeneric<,>), typeof(ServiceGeneric<,>));

            services.AddScoped<IUnitOfWork, UnitOfWork>();

            services.AddDbContext<AppDbContext>(options =>
            {
                //DBContext, Core katman�nda old�
                options.UseSqlServer(Configuration.GetConnectionString("SqlServer"), sqlOptions =>
                {
                    //Migration dosyalar� Data katman�nda olacak.
                    sqlOptions.MigrationsAssembly("AuthServer.Data");
                });
            });

            services.AddIdentity<UserApp, IdentityRole>(options =>
            {
                options.User.RequireUniqueEmail = true;
                options.Password.RequireNonAlphanumeric = false;
            }).AddEntityFrameworkStores<AppDbContext>().AddDefaultTokenProviders();


            // options pattern
            //DI ile appsettings i�indeki datalara eri�me.
            services.Configure<CustomTokenOptions>(Configuration.GetSection("TokenOption"));
            services.Configure<List<Client>>(Configuration.GetSection("Clients"));

            // Appsettings den TokenOption okuduk, CustomTokenOption's a mapledik.
            var tokenOptions = Configuration.GetSection("TokenOption").Get<CustomTokenOptions>();

            // Auth mekanizmas�
            services.AddAuthentication(options =>
            {
                // Schema vermek gerekiyor. 
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                //DefaultChallange�le Auth mekanizmas�n�n schemas� ile JWT nin mekanizmas�n� ba�l�yorum. Birbiri ile konu�mas� i�in.
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;


                // JWT Bazl� auth yapt���m�z i�in AddJWTBearer ekledik.
            }).AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, opts => // i�ine schema verdik.
            {
                opts.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters()
                {
                    // gelen token' a g�re do�rulama i�lemleri yap�yor.
                    // token geldikten sonra, issue, audienceslerini, s�relerini kontrol etmek istiyorum.
                    ValidIssuer = tokenOptions.Issuer,//
                    ValidAudience = tokenOptions.Audience[0],
                    IssuerSigningKey = SignService.GetSymmetricSecurityKey(tokenOptions.SecurityKey),

                    // �mzas�n� do�ruluyoruz.
                    ValidateIssuerSigningKey = true,
                    ValidateAudience = true,
                    ValidateIssuer = true,
                    ValidateLifetime = true,
                    // default olarak 5dk �m�r ekliyor. Farkl� serverlardan gelen api'den tokenleri
                    // do�rulama esnas�nda ge�en zaman i�in.
                    ClockSkew = TimeSpan.Zero
                };
            });

            services.AddControllers();
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "AuthServer.API", Version = "v1" });
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "AuthServer.API v1"));
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
