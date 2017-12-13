using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BitMiracle.LibTiff.Classic;

namespace GTAVisionUtils
{
    public class Logger
    {
        private static string logFilePath;

        public static void setLogFilePath(string path)
        {
            logFilePath = path;
        }

        
        public static void writeLine(string line)
        {
            var dateTimeFormat = @"yyyy-MM-dd--HH-mm-ss";
            try
            {
                System.IO.File.AppendAllText(logFilePath, DateTime.UtcNow.ToString(dateTimeFormat) + ":  " + line + "\n");            
            }
            catch (System.IO.IOException e)
            {
//             just silently fail, better than throwing   
            }
        }

        public static void writeLine(Exception e)
        {
            writeLine(e.StackTrace);
        }

        public static void writeLine(object value)
        {
            if (value == null)
            {
                return;
            }
            writeLine(value.ToString());
        }
    }
}