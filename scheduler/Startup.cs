using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Serialization;
using NLog.Extensions.Logging;
using scheduler.DataStore;
using scheduler.Models;
using scheduler.Services;
using Swashbuckle.AspNetCore.Swagger;

namespace scheduler
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc()
            .AddJsonOptions(o =>
            {
                if (o.SerializerSettings.ContractResolver != null)
                {
                    var castedResolver = o.SerializerSettings.ContractResolver
                     as DefaultContractResolver;
                    castedResolver.NamingStrategy = null;
                }
            });

            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new Info
                {
                    Title = "TimeTable API",
                    Version = "v1",
                    Description = "A simple scheduler API",
                    TermsOfService = "None",
                    Contact = new Contact
                    {
                        Name = "Voltis Agolli",
                        Email = "v.agolli@live.com",
                        Url = string.Empty
                    }
                });
            });

            services.AddDbContext<ScheduleContext>(o =>
                o.UseSqlServer(Configuration.GetConnectionString("DefaultConnection")));

            services.AddScoped<ISchdulesRepository, SchedulesRepository>();
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env,
            ILoggerFactory loggerFactory, ScheduleContext scheduleContext)
        {
            loggerFactory.AddNLog();

            app.UseSwagger();

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler();
            }

            scheduleContext.EnsureSeedDataForContext();

            app.UseStatusCodePages();

            AutoMapper.Mapper.Initialize(config =>
            {
                config.CreateMap<Schedule, ScheduleViewModel>().ReverseMap();
                config.CreateMap<Schedule, Schedule>();
            });

            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "TimeTable API V1");
            });

            app.UseMvc();
        }
    }
}
