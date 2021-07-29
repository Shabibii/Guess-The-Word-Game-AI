using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Text.RegularExpressions;

namespace WordFamilies
{
    class Program
    {
        public static void Main(string[] args)
        {
            Console.WriteLine("Game Loading..."); // Stay interactive
            Thread.Sleep(1200);
            Dictionary.LoadWords();

            // Start game after dictionary is load
            var game = new Game();
            game.StartGame();
        }
    }
}
