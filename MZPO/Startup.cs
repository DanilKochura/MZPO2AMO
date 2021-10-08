using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MZPO.Services;
using MZPO.DBRepository;
using Microsoft.EntityFrameworkCore;
using System;

namespace MZPO
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
            services.AddCors(options =>
            {
                options.AddDefaultPolicy(
                    builder =>
                    {
                        builder.WithOrigins("https://www.mzpo-s.ru",
                                            "https://mzpo-s.ru")
                               //.AllowAnyOrigin()
                               .AllowCredentials()
                               .AllowAnyHeader()
                               .AllowAnyMethod();
                    });
            });

            services.AddControllers();

            services.AddDbContext<MySQLContext>(opt => opt.UseMySql(
                Configuration.GetConnectionString("MySQLConnection"), 
                new MariaDbServerVersion(new Version(10, 5, 8)),
                builder =>
                {
                    builder.EnableRetryOnFailure(6, TimeSpan.FromSeconds(20), null);
                }
                ));

            services.AddScoped<IAccountRepo, AccountRepo>();
            services.AddScoped<ICityRepo, CityRepo>();
            services.AddScoped<ITagRepo, TagRepo>();
            services.AddScoped<ICFRepo, CFRepo>();

            services.AddSingleton(x =>
                ActivatorUtilities.CreateInstance<Amo>(x, 6));
            services.AddSingleton<ProcessQueue>();
            services.AddSingleton<LeadsSorter>();
            services.AddSingleton<Cred1C>();
            services.AddSingleton<Log>();
            services.AddSingleton<Uber>();
            services.AddSingleton<RecentlyUpdatedEntityFilter>();
            services.AddTransient<GSheets>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseCors();

            app.UseWebSockets();

            app.UseStaticFiles();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}