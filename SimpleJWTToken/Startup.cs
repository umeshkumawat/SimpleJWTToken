using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using SimpleJWTToken.Data;

namespace SimpleJWTToken
{
    public class Startup
    {
        public IConfiguration _configuration { get; private set; }

        public Startup(IConfiguration configuration)
        {
            _configuration = configuration;
        }
        public void ConfigureServices(IServiceCollection services)
        {
            services.Configure<JWTSettings>(_configuration.GetSection("JWTSettings"));

            services.AddDbContext<GroceryListContext>(config => config.UseInMemoryDatabase("Memory"));

            services.AddDbContext<UserDbContext>(config => config.UseInMemoryDatabase("ASPUSER"));

            services.AddIdentity<IdentityUser, IdentityRole>(config => 
            {
                config.Password.RequireDigit = false;
                config.Password.RequireLowercase = false;
                config.Password.RequireNonAlphanumeric = false;
                config.Password.RequireUppercase = false;
            })
            .AddEntityFrameworkStores<UserDbContext>();

            // जब client अपने साथ JWT token लाता है तो ये वाली service JWT Bearer token को validate karegi.
            services.AddAuthentication("OAuth")
                .AddJwtBearer("OAuth", config =>
                {
                    var secretKey = _configuration.GetSection("JWTSettings:SecretKey").Value;
                    config.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidIssuer = _configuration.GetSection("JWTSettings:Issuer").Value,
                        ValidAudience = _configuration.GetSection("JWTSettings:Audience").Value,
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey))
                    };
                });

            services.AddControllers();

        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseRouting();

            app.UseAuthentication();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapDefaultControllerRoute();
            });
        }
    }
}
