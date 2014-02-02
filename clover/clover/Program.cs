using System;

namespace clover
{
#if WINDOWS || XBOX
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main(string[] args)
        {
            using (Clover game = new Clover())
            {
                game.Run();
            }
        }
    }
#endif
}

