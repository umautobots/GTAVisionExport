using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BitMiracle.LibTiff.Classic;

namespace GTAVisionUtils {
    public class Logger {
        
        //most code taken from https://github.com/dbaaron/log-writer
        private static Queue<string> LogQueue = new Queue<string>();
        public static string logFilePath { private get; set; }
        public static int FlushAfterSeconds = 5;
        public static int FlushAtQty = 10;
        private static DateTime FlushedAt = DateTime.Now;
        
        public static void ForceFlush()
        {
            FlushLogToFile();
        }

        private static bool CheckTimeToFlush()
        {
            var time = DateTime.Now - FlushedAt;
            if (time.TotalSeconds >= FlushAfterSeconds)
            {
                FlushedAt = DateTime.Now;
                return true;
            }
            return false;
        }

        private static void FlushLogToFile()
        {
            while (LogQueue.Count > 0)
            {

                // Get entry to log
                var entry = LogQueue.Dequeue();

                // Crete filestream
                var stream = new FileStream(logFilePath, FileMode.Append, FileAccess.Write);
                using (var writer = new StreamWriter(stream))
                {
                    // Log to file
                    writer.Write(entry);
                }
            }
        }

        private static string WrapLogMessage(string message) {
            const string dateTimeFormat = @"yyyy-MM-dd--HH-mm-ss";
            return $"{DateTime.UtcNow.ToString(dateTimeFormat)}:  {message}\r\n";
        }
        
        public static void WriteLine(string line) {
            lock (LogQueue)
            {

                // Create log
                LogQueue.Enqueue(WrapLogMessage(line));

                // Check if should flush
                if (LogQueue.Count >= FlushAtQty || CheckTimeToFlush())
                {
                    FlushLogToFile();
                }

            }
        }

        public static void WriteLine(Exception e) {
            WriteLine(e.Message);
            WriteLine(e.Source);
            WriteLine(e.StackTrace);
        }

        public static void WriteLine(object value) {
            if (value == null) {
                return;
            }

            WriteLine(value.ToString());
        }
    }
}