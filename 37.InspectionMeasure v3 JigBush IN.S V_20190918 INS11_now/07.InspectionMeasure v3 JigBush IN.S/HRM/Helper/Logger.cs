using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace InspectionMeasure.Helper
{
    public static class Logger
    {
        public static void WriteLog(string methodName, Exception ex)
        {
            CoreWriteLog(methodName, ex.Message, ex.StackTrace);
        }

        public static void WriteLog(string methodName, string customMessage)
        {
            CoreWriteLog(methodName, customMessage, "N/A (Crash by operational issue)");
        }

        private static void CoreWriteLog(string methodName, string errorMessage, string details)
        {
            try
            {
                // Url directory "Logs" on bin/Debug of app
                string folderPath = Path.Combine(Application.StartupPath, "Logs");
                if (!Directory.Exists(folderPath))
                {
                    Directory.CreateDirectory(folderPath);
                }

                // Ex: Log_2026_06_15.txt
                string filePath = Path.Combine(folderPath, $"Log_{DateTime.Now:yyyy_MM_dd}.txt");

                // Content include: Datetime, method, error, detail
                string logContent = $"----------------------------------------\n" +
                                    $"time: {DateTime.Now:HH:mm:ss}\n" +
                                    $"method: {methodName}\n" +
                                    $"error: {errorMessage}\n" +
                                    $"details: {details}\n";

                // write continues into the file, Not override old log
                File.AppendAllText(filePath, logContent);
            }
            catch
            {
                // No crash
            }
        }
    }
}
