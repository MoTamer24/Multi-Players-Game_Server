using System;
using System.Threading.Tasks;
using System.Windows;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.VisualBasic;

namespace GameClient
{
    public partial class Client
    {
        HubConnection connection;
        string? matchId;
        public Client()
        {
            connection = new HubConnectionBuilder()
                .WithUrl("http://localhost:5024/Hub")
                .Build();

            connection.Closed += async (error) =>
            {
                await Task.Delay(new Random().Next(0, 5) * 1000);
                await connection.StartAsync();
            };
        }

        public async Task connect()
        {
            connection.On<string>("Ping", (message) =>
            {
                Console.WriteLine($"[SERVER SAYS]: {message}");
            });

            connection.On<string>("ReceiveMsg", message =>
            {
                Console.WriteLine($"[SERVER SAYS]: {message}");
            });

            connection.On<string>("GameStarted", message =>
           {
               matchId=message;
               Console.WriteLine($"[SERVER SAYS]: {message}");
           });


            try
            {
                await connection.StartAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
        public async Task FindMatch()
        {
            try
            {
                await connection.InvokeAsync("FindMatch");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
        public async Task PingGroup()
        {
            await connection.InvokeAsync("MakeMove","consider this a move",matchId);
        }
        public async Task ping()
        {
            try
            {
                await connection.InvokeAsync("Ping");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
    }
}