﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Threading.Tasks;
using AutoMapper;
using Menu.API.Abstraction.Repositories;
using Menu.API.Data;
using Menu.API.Models;
using Menu.API.Repositories;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Pivotal.Discovery.Client;
namespace Menu.API
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
            services.AddMvc();
            services.AddAuthorization();

            services.AddAuthentication("Bearer")
                .AddIdentityServerAuthentication(options =>
                {
                    options.Authority = Configuration["IdentityUrl"];
                    options.RequireHttpsMetadata = false;
                    options.ApiName = "api1";
                });

            var connectionString = Configuration.GetConnectionString("MenuDatabaseConnectionString");
            services.AddDbContext<ApplicationDbContext>(options =>
            {
                options.UseNpgsql(connectionString);
            });

            services.AddSwaggerGen(options =>
            {
                options.SwaggerDoc("v1", new Swashbuckle.AspNetCore.Swagger.Info
                {
                    Title = "Restaurant - Menu HTTP API",
                    Version = "v1",
                    TermsOfService = "Terms Of Service"
                });
            });

            services.AddScoped<IRepository<Category>, CategoryRepository>();
            services.AddScoped<IRepository<Food>, FoodRepository>();
            services.AddAutoMapper(typeof(Startup).GetTypeInfo().Assembly);
            services.AddDiscoveryClient(Configuration);
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            app.UseAuthentication();

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseHsts();
            }

            app.UseSwagger().UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "Menu.API V1");
            });

            app.UseMvcWithDefaultRoute();
            app.UseHttpsRedirection();
            app.UseMvc();
            app.UseDiscoveryClient();
        }
    }
}
