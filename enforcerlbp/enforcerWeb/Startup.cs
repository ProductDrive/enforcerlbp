using AutoMapper;
using Data;
using Data.Context;
using DataAccess.UnitOfWork;
using enforcerWeb.Helper;
using MediatR;
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
using Microsoft.IdentityModel.Tokens;
using Services.Implementations;
using Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace enforcerWeb
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
            services.AddControllers();

            services.AddMediatR(typeof(Startup));

            services.AddDbContext<EnforcerContext>(options =>
               options.UseSqlServer(
               Configuration.GetConnectionString("DefaultConnection")));

            //DataAccess
            services.AddScoped(typeof(IUnitOfWork<>), typeof(UnitOfWork<>));

            //Application Service
            services.AddScoped<IUserService, UserService>();


            services.AddIdentity<EnforcerUser, IdentityRole>(options =>
            {
                options.User.RequireUniqueEmail = true;
                options.Password.RequireDigit = false;
                options.Password.RequiredLength = 6;
                options.Password.RequiredUniqueChars = 0;
                options.Password.RequireUppercase = false;
                options.Password.RequireLowercase = false;
                options.Password.RequireNonAlphanumeric = false;
            }).AddEntityFrameworkStores<EnforcerContext>().AddDefaultTokenProviders();

            var mappingConfig = new MapperConfiguration(mc => { mc.AddProfile(new MappingProfile()); });

            var mapper = mappingConfig.CreateMapper();

            services.AddSingleton(mapper);

            services.AddAutoMapper(typeof(Startup));

            services.Configure<DataProtectionTokenProviderOptions>(d => d.TokenLifespan = TimeSpan.FromMinutes(10));

            var key = Encoding.ASCII.GetBytes(Configuration["JwtSettings:Secret"].ToString());

            services.AddAuthentication(x =>
            {
                x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                x.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
            }).AddJwtBearer(x =>
            {
                x.RequireHttpsMetadata = false;
                x.SaveToken = false;
                x.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    ClockSkew = TimeSpan.Zero
                };
            });

            services.AddAuthorization(options =>
            {

                options.AddPolicy("RequireLoggedIn",
                    policy => policy.RequireAuthenticatedUser());
                options.AddPolicy("SuperUser", policy=>policy.RequireRole( "Owner","Developer").RequireAuthenticatedUser());
                options.AddPolicy("Admin", policy=>policy.RequireRole("Admin", "Developer").RequireAuthenticatedUser());
                options.AddPolicy("Therapist", policy=>policy.RequireRole("Admin","Physiotherapist","Developer").RequireAuthenticatedUser());
                options.AddPolicy("Patient", policy=>policy.RequireRole("Admin","Patient","Developer").RequireAuthenticatedUser());

            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
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
