using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace Betty
{
    public class CsgoLogger
    {
        private const string folderName = "Logs";
        private const string fileName = "CsgoLog.txt";

        public static void Log(string message)
        {
            FileInit();

            string filePath = Path.Combine(Directory.GetCurrentDirectory(), folderName, fileName);

            using (StreamWriter stream = File.AppendText(filePath))
            {
                stream.WriteLine(message);
            }
        }

        private static void FileInit()
        {
            string current = Directory.GetCurrentDirectory();
            string logPath = Path.Combine(current, folderName);

            if (!Directory.Exists(logPath))
            {
                Directory.CreateDirectory(logPath);
            }

            string filePath = Path.Combine(logPath, fileName);

            if (!File.Exists(filePath))
            {
                File.Create(filePath);
            }
        }
    }
}