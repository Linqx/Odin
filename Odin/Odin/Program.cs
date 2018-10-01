using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Odin
{
    class Program
    {
        #region Public

        public static Config Config;

        #endregion

        #region Private

        private const string EXIT = "exit";

        private static bool Running = true;
        private static ClientManager Manager;

        #endregion

        static void Main(string[] args)
        {
            Config = Config.FromResource("config.json");
            Manager = new ClientManager();

            while (Running)
            {
                string readLine = Console.ReadLine();
                string[] split = readLine.Split(' ');
                string command = split[0].ToLower();
                string[] parameters = split.Where((s, i) => i != 0).ToArray();

                switch (command)
                {
                    case EXIT:
                        Running = false;
                        break;
                }
            }
        }
    }
}
