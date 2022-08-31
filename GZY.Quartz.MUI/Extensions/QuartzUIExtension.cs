using GZY.Quartz.MUI.BaseJobs;
using GZY.Quartz.MUI.BaseService;
using GZY.Quartz.MUI.EFContext;
using GZY.Quartz.MUI.Service;
using GZY.Quartz.MUI.Tools;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Quartz;
using Quartz.Impl;
using Quartz.Spi;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace GZY.Quartz.MUI.Extensions
{
    public static class QuartzUIExtension
    {
        public static IServiceCollection AddQuartzUI(this IServiceCollection services, DbContextOptions<QuarzEFContext> option = null)
        {
            services.AddRazorPages();
            services.AddHttpClient();
            services.AddHttpContextAccessor();
            if (option != null)
            {

                services.AddSingleton<DbContextOptions<QuarzEFContext>>(a => { return option; });
                services.AddDbContext<QuarzEFContext>();
                services.AddScoped<IQuartzLogService, EFQuartzLogService>();
                services.AddScoped<IQuartzService, EFQuartzService>();


            }
            else
            {
                services.AddScoped<QuartzFileHelper>();
                services.AddScoped<IQuartzLogService, FileQuartzLogService>();
                services.AddScoped<IQuartzService, FileQuartzService>();
            }

            services.AddScoped<HttpResultfulJob>();
            services.AddScoped<ClassLibraryJob>();
            services.AddSingleton<ISchedulerFactory, StdSchedulerFactory>();
            services.AddSingleton<IJobFactory, ASPDIJobFactory>();
            services.AddScoped<IQuartzHandle, QuartzHandle>();
            return services;

        }

        /// <summary>
        /// 自动注入定时任务类
        /// </summary>
        /// <param name="services"></param>
        /// <returns></returns>
        public static IServiceCollection AddQuartzClassJobs(this IServiceCollection services)
       {
            var baseType = typeof(IJobService);
            var path = AppDomain.CurrentDomain.RelativeSearchPath ?? AppDomain.CurrentDomain.BaseDirectory;
            var referencedAssemblies = Directory.GetFiles(path, "*.dll");
            List<Type> typelist = new List<Type>();
            foreach (var item in referencedAssemblies)
            {
                try
                {
                    var assembly = Assembly.LoadFrom(item);
                    Type[] ts = assembly.GetTypes();
                    typelist.AddRange(ts.ToList());
                }
                catch (Exception)
                {

                    continue;
                }
            }
            var types = typelist
            .Where(x => x != baseType && baseType.IsAssignableFrom(x)).ToArray();
            var implementTypes = types.Where(x => x.IsClass).ToArray();
            var interfaceTypes = types.Where(x => x.IsInterface).ToArray();
            foreach (var implementType in implementTypes)
            {
                var interfaceType = implementType.GetInterfaces().First();
                    services.AddScoped(interfaceType, implementType);

                ClassJobsFactory.ClassJobs.Add(implementType.Name);
            }
            return services;



        }

        public static IApplicationBuilder UseQuartz(this IApplicationBuilder builder)
        {
            builder.UseRouting();
            builder.UseStaticFiles();
            builder.UseEndpoints(endpoints =>
            {
                endpoints.MapRazorPages();
                
            });
            IServiceProvider services = builder.ApplicationServices;
            using (var serviceScope = services.CreateScope())
            {

                var dd = serviceScope.ServiceProvider.GetService<IQuartzHandle>();
                dd.InitJobs();
            }

            return builder;
        }
    }
}
