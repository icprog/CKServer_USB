using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;

namespace CKServer
{
    class ErrorLog
    {
        public static string path;                          //Log文件的目录

        public static LogDLL2 Log = new LogDLL2();

        public static void start()
        {
            Log.path = path;
        }

        public static void Error(string error)
        {
            Log.LogAppendMethod(error);
        }
    }

    class LogDLL2
    {
        public string path;
        private string sDaytime;
        private int count = 0;
        private string RecordFilename;

        public void LogAppendMethod(string text)
        {
            string LogPath = path;
            if (!Directory.Exists(LogPath))
                Directory.CreateDirectory(LogPath);
            //如果软件记录到第二天，重新生成操作日志文件
            if (sDaytime != string.Format("{0}-{1:D2}-{2:D2}", DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day))
            {
                count = 0;
            }

            sDaytime = string.Format("{0}-{1:D2}-{2:D2}", DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day);

            RecordFilename = sDaytime + count.ToString() + "Log.txt";

            LogPath = LogPath + RecordFilename;

            
            File.AppendAllText(LogPath, text + "\r\n");

            FileInfo fi = new FileInfo(LogPath);
            if (fi.Length >= 1024 * 1024 * 60)
            {
                count++;
                sDaytime = string.Format("{0}-{1:D2}-{2:D2} Log", DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day);
                RecordFilename = sDaytime + count.ToString() + ".txt";
            }

        }
    }

}
