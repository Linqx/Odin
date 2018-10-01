using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Odin.Tools
{
    public class Logger
    {
        private static DateTime CurrentTime { get => DateTime.Now; }
        private static BlockingCollection<LoggingItem> LoggingQueue;

        static Logger()
        {

            LoggingQueue = new BlockingCollection<LoggingItem>();
            Task.Factory.StartNew(Consume, TaskCreationOptions.LongRunning);
        }

        public static void Log(string sender, object value, ConsoleColor color = ConsoleColor.Gray)
            => LoggingQueue.Add(new LoggingItem(sender, value.ToString(), color));

        private static void Consume()
        {
            foreach (var i in LoggingQueue.GetConsumingEnumerable())
            {
                var color = Console.ForegroundColor;

                if (i.Color != color)
                {
                    Console.ForegroundColor = i.Color;
                    Console.WriteLine($"[{i.Prefix}] {i.Message}");
                    Console.ForegroundColor = color;
                }
                else
                {
                    Console.WriteLine($"[{i.Prefix}] {i.Message}");
                }
            }
        }

        private class LoggingItem
        {
            public string Prefix;
            public string Message;
            public ConsoleColor Color;

            public LoggingItem(string prefix, string message, ConsoleColor color = ConsoleColor.Gray)
            {
                Prefix = prefix;
                Message = message;
                Color = color;
            }
        }
    }
}
