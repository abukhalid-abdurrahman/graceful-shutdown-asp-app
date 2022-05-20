using System;
using System.Threading.Tasks;
using graceful_shutdown_asp_app.Hosts;
using graceful_shutdown_asp_app.Services;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NLog.Web;

namespace graceful_shutdown_asp_app
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var logger = NLogBuilder
                .ConfigureNLog("nlog.config")
                .GetCurrentClassLogger();

            try  
            {  
                CreateHostBuilder(args)
                    .Build()
                    .Run();
            }  
            catch (Exception ex)  
            {  
                logger.Error(ex, "Fatal error was expected!");  
                throw;
            }  
            finally  
            {  
                NLog.LogManager.Shutdown();  
            }  
        }

        private static IWebHostBuilder CreateHostBuilder(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .ConfigureLogging(logging =>  
                {  
                    logging.ClearProviders();  
                    logging.SetMinimumLevel(LogLevel.Information);  
                })
                .UseNLog()
                .ConfigureServices(services =>
                {
                    services.AddSingleton<ServerStateService>();
                    services.AddHostedService<HostApplicationLifeTime>();
                })
                .Configure(app =>
                {
                    // An example ASP.NET Core middleware that throws an
                    // exception when serving a request to path: /throw
                    app.UseRouting();
                    
                    app.Use(async (context, next) =>
                    {
                        var serviceState = context.RequestServices
                            .GetRequiredService<ServerStateService>();
                        
                        // if(serviceState.ApplicationState != ApplicationStateEnum.Started)
                        // {
                        //     context.Response.ContentType = "application/json";
                        //     context.Response.StatusCode = 503;
                        //     await context.Response.WriteAsync("{\"status\": \"service not available\"}");
                        // }
                        await next();
                    });

                    app.UseEndpoints(endpoints =>
                    {
                        // Reported events will be grouped by route pattern
                        endpoints.MapGet("/handle_order", context =>
                        {
                            var log = context.RequestServices
                                .GetRequiredService<ILoggerFactory>()
                                .CreateLogger<Program>();
                            
                            var serviceState = context.RequestServices
                                .GetRequiredService<ServerStateService>();

                            log.LogInformation("Handling order request...");
                            
                            if(serviceState.ApplicationState == ApplicationStateEnum.Stopping)
                            {
                                Task.Delay(1000).Wait();
                                log.LogInformation("five sec");
                            }
                            
                            return Task.CompletedTask;
                        });
                    });
                });
    }
}