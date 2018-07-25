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
        private static Queue<string> LogQueue;
        public static string logFilePath { private get; set; }
        public static int FlushAfterSeconds = 5;
        public static int FlushAtQty = 10;
        private static DateTime FlushedAt;
        private Logger() { }

        public static void ForceFlush()
        {
            FlushLogToFile();
        }

        private static bool CheckTimeToFlush()
        {
            TimeSpan time = DateTime.Now - FlushedAt;
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
                string entry = LogQueue.Dequeue();

                // Crete filestream
                FileStream stream = new FileStream(logFilePath, FileMode.Append, FileAccess.Write);
                using (var writer = new StreamWriter(stream))
                {
                    // Log to file
                    writer.WriteLine(entry);
                    stream.Close();
                }
            }
        }

        private static string wrapLogMessage(string message) {
            var dateTimeFormat = @"yyyy-MM-dd--HH-mm-ss";
            return $"{DateTime.UtcNow.ToString(dateTimeFormat)}:  {message}\r\n";            
        }
        
        public static void writeLine(string line) {
            lock (LogQueue)
            {

                // Create log
                LogQueue.Enqueue(wrapLogMessage(line));

                // Check if should flush
                if (LogQueue.Count >= FlushAtQty || CheckTimeToFlush())
                {
                    FlushLogToFile();
                }

            }
        }

        public static void writeLine(Exception e) {
            writeLine(e.Message);
            writeLine(e.Source);
            writeLine(e.StackTrace);
        }

        public static void writeLine(object value) {
            if (value == null) {
                return;
            }

            writeLine(value.ToString());
        }
    }
}