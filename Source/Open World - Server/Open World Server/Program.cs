using Microsoft.Extensions.DependencyInjection;
using OpenWorldServer.Models;
using System;

namespace OpenWorldServer
{
    internal class Program
    {
        static void Main(string[] args)
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
                .AddScoped<WorldUtils>()
                .BuildServiceProvider();

            var server = serviceProvider.GetRequiredService<Server>();
            server.Start();
        }
    }
}
