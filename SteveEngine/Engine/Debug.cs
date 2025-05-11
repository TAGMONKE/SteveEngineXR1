using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SteveEngine.SteveEngine.Engine
{
    public class Debug
    {
        public long FrameCount { get; private set; }
        public long LastFrameTime { get; private set; }
        public long LastFrameTimeMs { get; private set; }

        public static void Log(string message)
        {
            Console.WriteLine($"{message}");
        }

        public static void LogError(string message)
        {
            Console.WriteLine($"\\x1b[31m {message}");
        }
    }
}
