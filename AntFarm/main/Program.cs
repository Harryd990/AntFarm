using AntFarm.main;

internal class Program
{
    private static void Main(string[] args)
    {
        Console.WriteLine("Neah Ant Colony Simulation");
        Console.WriteLine("--------------------------");
        Console.WriteLine("enter width then height of sim");
        int width = 0;
        int height = 0;
        bool validInput = false;
        while (!(validInput))
        {
            Console.WriteLine("Please enter a valid integer for width and height (greater then 4x4):");
            if ((int.TryParse(Console.ReadLine(), out int w) && int.TryParse(Console.ReadLine(), out int h)&&(w >= 4) && (h>= 4)))
            {
                validInput = true;
                Console.WriteLine($"Width: {w}, Height: {h}");
                width = w;
                height = h;
            }
            else
            {
                Console.WriteLine("Invalid input. Please enter integers for width and height.");
            }
        }
        
        Console.ReadLine();
        Game game = new Game(width, height);
        game.Initialise_Game();
        game.Run();
        Console.ReadLine();
       
    }
    

}