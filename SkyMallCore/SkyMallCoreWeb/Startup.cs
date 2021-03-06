﻿using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SkyMallCore.Core;
using SkyMallCore.Services;
using SkyMallCoreWeb.Middlewares;

namespace SkyMallCoreWeb
{
    public class Startup
    {
        public Startup(IHostingEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json",optional:true,reloadOnChange:true)
                .AddEnvironmentVariables();

            if (env.IsDevelopment())
            {
                // This will push telemetry data through Application Insights pipeline faster, allowing you to view results immediately.
                builder.AddApplicationInsightsSettings(developerMode: true);
            }
            
            Configuration = builder.Build();
        }

        public IConfiguration Configuration { get; }
        
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddLogging(logging=> {
                logging.AddConfiguration(Configuration.GetSection("Logging"));
                logging.AddDebug();
            });

            //注册业务服务
            ServiceFactory.Initialize(services, Configuration);
            //用户认证注册
            AuthenticationFactory.Initialize(services);
            services.AddMvc(options=> {
                //add filters
                options.Filters.Add(typeof(ErrorAttribute));
            });

            services.AddMemoryCache();
            services.AddScoped<IMemCache, MemCache>();

            //services.AddDistributedMemoryCache(); 区别
            services.AddSession();
            CoreProviderContext.ServiceCollection = services;
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            CoreProviderContext.HttpContextAccessor = app.ApplicationServices.GetService<IHttpContextAccessor>();
            CoreProviderContext.Configuration = Configuration;
            CoreProviderContext.HostingEnvironment = env;
            if (env.IsDevelopment())
            {
                app.UseBrowserLink();
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
            }
            //使用授权
            app.UseAuthentication();

            app.UseStaticFiles(new StaticFileOptions {
                //不能识别的文件，作为图片处理
                ServeUnknownFileTypes = true,
                DefaultContentType = "image/png"
            });

            //demo
            //app.UseHelloWorld();

            //SEssion
            app.UseSession();
            //mvc路由
            app.UseMvc(routes =>
            {
                //针对Area
                routes.MapAreaRoute(
                  name: "SystemManage", areaName: "SystemManage",
                     template: "SystemManage/{controller=Home}/{action=Index}"
                );

                routes.MapAreaRoute(
                name: "SystemSecurity", areaName: "SystemSecurity",
                   template: "SystemSecurity/{controller=Home}/{action=Index}"
              );

                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}"
                    , defaults: new string[] { "SkyMallCoreWeb.Controllers" }
                    );

            });
        }
    }


    


}
