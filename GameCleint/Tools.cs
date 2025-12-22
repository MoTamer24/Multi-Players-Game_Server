namespace GameClient
{
    public static class Tools
    {
        public static void DisplayBoard(char[] board)
{

    // Row 1
    Console.WriteLine($"  {board[0]}  |  {board[1]}  |  {board[2]}  ");
    Console.WriteLine("_____|_____|_____");
    Console.WriteLine("     |     |     ");

    // Row 2
    Console.WriteLine($"  {board[3]}  |  {board[4]}  |  {board[5]}  ");
    Console.WriteLine("_____|_____|_____");
    Console.WriteLine("     |     |     ");

    // Row 3
    Console.WriteLine($"  {board[6]}  |  {board[7]}  |  {board[8]}  ");
    Console.WriteLine("     |     |     ");
    
}
    }
    
}