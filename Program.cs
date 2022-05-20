using System;
using System.Linq;
using System.Threading.Tasks;
using graceful_shutdown_asp_app.Hosts;
using graceful_shutdown_asp_app.Models;
using graceful_shutdown_asp_app.Services;
using graceful_shutdown_asp_app.Storage;
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
                    services.AddSingleton<OrderStorage>();
                    
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
                        
                        if(serviceState.ApplicationState != ApplicationStateEnum.Started)
                        {
                            context.Response.ContentType = "application/json";
                            context.Response.StatusCode = 503;
                            await context.Response.WriteAsync("{\"status\": \"service not available\"}");
                        }
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

                            var orderStorage = context.RequestServices
                                .GetRequiredService<OrderStorage>();

                            log.LogInformation("Handling order request...");
                            
                            // Create new order
                            var newOrderId = Guid.NewGuid();
                            // Add pending order into concurrent bag
                            orderStorage.Orders.Add(new Order()
                            {
                                Id = newOrderId,
                                State = "pending"
                            });
                            
                            // TODO: send request into external API
                            
                            var existOrder = orderStorage.Orders.FirstOrDefault(x => x.Id == newOrderId);
                            if (existOrder == null) return Task.CompletedTask;
                            
                            existOrder.State = "approved";
                            // TODO: Update in database
                            
                            // Remove from concurrent storage
                            orderStorage.Orders.TryTake(out existOrder);
                            
                            return Task.CompletedTask;
                        });
                    });
                });
    }
}