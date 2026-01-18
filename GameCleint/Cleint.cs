using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using Microsoft.AspNetCore.SignalR.Client;
namespace GameClient
{
    public partial class Client
    {
        HubConnection connection;

        string? matchId;
        public bool myTurn = false;
        public bool Active=false;
        public string? MySymbol { get; private set; }
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

            connection.On<string, char[], string>("GameStarted", (id, board, nextplayerId) =>  // edited 
               {
                   matchId = id;
                 
               
                   myTurn = (nextplayerId == connection.ConnectionId);
                   Console.Clear();
                   if(myTurn)
                   Console.WriteLine($"You are Player: X ");
                   else
                   {
                        Console.WriteLine($"You are Player: O");
                   }
                   Tools.DisplayBoard(board);
               });

            connection.On<char[], string>("BoardUpdate", (board, nextplayerId) => // edited 
             {
                 Console.Clear();
                 Tools.DisplayBoard(board);
                 if(connection.ConnectionId==nextplayerId)
                 {
                     myTurn=true;
                 }
                

                 if (myTurn) Console.WriteLine("\n[YOUR TURN] Enter 1-9:");
                 else Console.WriteLine($"\n[WAITING] Opponent's turn...");
             });
            
            connection.On<string>("GameOver",WinnderId =>
            {
                System.Console.WriteLine($"Winner : {WinnderId}");
                if (WinnderId == connection.ConnectionId)
                {
                    System.Console.WriteLine("YOU WON");
                }
                else if (WinnderId=="DRAW")
                {
                    System.Console.WriteLine("ITs DRAW");
                }
                else
                {
                    System.Console.WriteLine("YOU LOST");
                }
                Active=false;
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
        public async Task MakeMove(int index)
        {
            await connection.InvokeAsync("MakeMove", index, matchId);
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