using System.ComponentModel.DataAnnotations;
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

            connection.On<string, char[], string>("GameStarted", (id, board, symbol) =>
               {
                   matchId = id;
                   MySymbol = symbol;
               
                   myTurn = (symbol == "X"); // X goes first
                   Console.Clear();
                   Console.WriteLine($"You are Player: {MySymbol}");
                   Tools.DisplayBoard(board);
               });

            connection.On<char[], string>("BoardUpdate", (board, nextTurnSymbol) =>
             {
                 Console.Clear();
                 Tools.DisplayBoard(board);

                 // Only enable input if the server says it's MY turn
                 myTurn = (nextTurnSymbol == MySymbol);

                 if (myTurn) Console.WriteLine("\n[YOUR TURN] Enter 1-9:");
                 else Console.WriteLine($"\n[WAITING] Opponent's turn...");
             });
            
            connection.On<string>("GameOver",WinnderId =>
            {
                Active=false;
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