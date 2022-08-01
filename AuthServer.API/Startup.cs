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
                //DBContext, Core katmanýnda oldý
                options.UseSqlServer(Configuration.GetConnectionString("SqlServer"), sqlOptions =>
                {
                    //Migration dosyalarý Data katmanýnda olacak.
                    sqlOptions.MigrationsAssembly("AuthServer.Data");
                });
            });

            services.AddIdentity<UserApp, IdentityRole>(options =>
            {
                options.User.RequireUniqueEmail = true;
                options.Password.RequireNonAlphanumeric = false;
            }).AddEntityFrameworkStores<AppDbContext>().AddDefaultTokenProviders();


            // options pattern
            //DI ile appsettings içindeki datalara eriþme.
            services.Configure<CustomTokenOptions>(Configuration.GetSection("TokenOption"));
            services.Configure<List<Client>>(Configuration.GetSection("Clients"));

            // Appsettings den TokenOption okuduk, CustomTokenOption's a mapledik.
            var tokenOptions = Configuration.GetSection("TokenOption").Get<CustomTokenOptions>();

            // Auth mekanizmasý
            services.AddAuthentication(options =>
            {
                // Schema vermek gerekiyor. 
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                //DefaultChallangeÝle Auth mekanizmasýnýn schemasý ile JWT nin mekanizmasýný baðlýyorum. Birbiri ile konuþmasý için.
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;


                // JWT Bazlý auth yaptýðýmýz için AddJWTBearer ekledik.
            }).AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, opts => // içine schema verdik.
            {
                opts.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters()
                {
                    // gelen token' a göre doðrulama iþlemleri yapýyor.
                    // token geldikten sonra, issue, audienceslerini, sürelerini kontrol etmek istiyorum.
                    ValidIssuer = tokenOptions.Issuer,//
                    ValidAudience = tokenOptions.Audience[0],
                    IssuerSigningKey = SignService.GetSymmetricSecurityKey(tokenOptions.SecurityKey),

                    // Ýmzasýný doðruluyoruz.
                    ValidateIssuerSigningKey = true,
                    ValidateAudience = true,
                    ValidateIssuer = true,
                    ValidateLifetime = true,
                    // default olarak 5dk ömür ekliyor. Farklý serverlardan gelen api'den tokenleri
                    // doðrulama esnasýnda geçen zaman için.
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
