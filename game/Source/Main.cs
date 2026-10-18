using Microsoft.Xna.Framework;

public static class Program
{
    [STAThread]
    static void Main(string[] args)
    {
        if (args.Contains("--soloud-selftest"))
            Environment.Exit(GameAudio.SelfTest());

        using var game = new Game1();
        game.Run();
    }
}
