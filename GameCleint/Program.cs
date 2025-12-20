// See https://aka.ms/new-console-template for more information
using GameClient;

Console.WriteLine("Hello, World!");

var C = new Client();
await C.connect();  
await C.ping();
while (true)
{
    var key=Console.ReadKey().Key;
    if (key == ConsoleKey.Q)
    {
        break;
    }
    else if (key==ConsoleKey.F)
    {
       await C.FindMatch();
    }
    else if (key==ConsoleKey.M)
    {
        await C.PingGroup();
    }
}

Console.ReadKey();