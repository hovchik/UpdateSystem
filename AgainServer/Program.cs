using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AgainServer
{
    public class Program
    {
        public static void Main(string[] args)
        {
            BuildWebHost(args).Run();
        }

        public static IWebHost BuildWebHost(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
            .ConfigureAppConfiguration((hostingContext, config) =>
            {
                var env = hostingContext.HostingEnvironment;

                config.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                      .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true, reloadOnChange: true);

            })
            .ConfigureServices(serv =>
            {
                serv.AddSingleton<HubConnection>(
                    new HubConnectionBuilder()
                        .WithUrl("http://localhost:16879/messageHub").Build()); //https://againserver.azurewebsites.net    http://localhost:16879
            })
            .UseStartup<Startup>()
            .UseIISIntegration()
            .Build();
    }
}