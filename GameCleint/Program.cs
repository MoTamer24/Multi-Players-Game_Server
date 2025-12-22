using GameClient;

Console.WriteLine("--- TIC TAC TOE CLIENT ---");
var client = new Client();
await client.connect();

// Phase 1: The Menu
Console.WriteLine("\nPress 'F' to Find Match, or 'Q' to Quit.");
while (true)
{
    // If we haven't found a match yet, show menu logic
    if (string.IsNullOrEmpty(client.MySymbol)) 
    {
        var key = Console.ReadKey(true).Key; // 'true' hides the key press
        if (key == ConsoleKey.F)
        {
            Console.WriteLine("Searching for opponent...");
            await client.FindMatch();
            client.Active=true;
            break; // Exit menu loop, start game loop
        }
        else if (key == ConsoleKey.Q) return;
    }
}

while (client.Active)
{
    if (client.myTurn)
    {
        string input = Console.ReadLine();
        if (int.TryParse(input, out int moveIndex))
        {
            await client.MakeMove(moveIndex);
            client.myTurn = false; // Lock input immediately after sending
        }
        else
        {
            Console.WriteLine("Invalid input. Enter 1-9:");
        }
    }
    else
    {
        // IMPORTANT: Wait a bit so we don't crash the CPU
        await Task.Delay(500); 
    }
}