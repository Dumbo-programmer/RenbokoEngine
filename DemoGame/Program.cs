using RenyulEngine.Core;
using System;
namespace DemoGame
{
    public static class Program
    {
        [STAThread]
        static void Main()
        {
            using var game = new GameApp();
            game.Run();
        }
    }
}
