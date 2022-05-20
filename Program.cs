using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Routing;
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
                logger.Info("Application starting...");  
                CreateHostBuilder(args).Build().Run();
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
                .Configure(app =>
                {
                    // An example ASP.NET Core middleware that throws an
                    // exception when serving a request to path: /throw
                    app.UseRouting();
                    
                    app.UseEndpoints(endpoints =>
                    {
                        // Reported events will be grouped by route pattern
                        endpoints.MapGet("/handle_order", context =>
                        {
                            var log = context.RequestServices
                                .GetRequiredService<ILoggerFactory>()
                                .CreateLogger<Program>();

                            log.LogInformation("Handling order request...");
                            
                            return Task.CompletedTask;
                        });
                    });
                });
    }
}