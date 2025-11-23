using RenbokoEngine.Core;
using System;
namespace DemoGame
{
    public static class Program
    {
        [STAThread]
        static void Main()
        {
            try
            {
                using var game = new GameApp();
                game.Run();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Unhandled exception: " + ex);
                Console.WriteLine("Press any key to exit...");
                Console.ReadKey();
            }
        }
    }
}
