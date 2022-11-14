using Microsoft.Extensions.DependencyInjection;
using OpenWorldServer.Models;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace OpenWorldServer
{
    internal class Program
    {
        private static async Task Main(string[] args)
        {
            IServiceProvider serviceProvider = new ServiceCollection()
                .AddScoped<Server>()
                .AddScoped<PlayerUtils>()
                .AddScoped<SaveSystem>()
                .AddScoped(_ => new SaveFileProvider("Bans.json"))
                .AddScoped<Networking>()
                .AddScoped<NetworkingHandler>()
                .AddScoped<FactionHandler>()
                .AddScoped<JoiningsUtils>()
                .AddScoped<FactionBuildingHandler>()
                .AddScoped<FactionSiloHandler>()
                .AddScoped<FactionBankHandler>()
                .AddScoped<SimpleCommands>()
                .AddScoped<AdvancedCommands>()
                .AddScoped<HttpClient>()
                .AddScoped<WorldUtils>()
                .AddScoped<ServerUtils>()
                .BuildServiceProvider();

            var server = serviceProvider.GetRequiredService<Server>();
            await server.StartAsync();
        }
    }
}
