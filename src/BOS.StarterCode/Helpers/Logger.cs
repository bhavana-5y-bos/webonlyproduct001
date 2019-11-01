using System;
using System.IO;

namespace BOS.StarterCode.Helpers
{
    public class Logger
    {

        public void Log(string logMessage)
        {
            string filePath = Directory.GetCurrentDirectory() + "/Logs/Log_" + DateTime.Now.ToString("MM-dd-yyyy") + ".txt";
            using (StreamWriter outputFile = new StreamWriter(filePath, true))
            {
                outputFile.WriteLine("------------------------------------------");
                outputFile.WriteLine("DATE: " + DateTime.Now);
                outputFile.WriteLine("MESSAGE: " + logMessage);
                outputFile.WriteLine("------------------------------------------");
                outputFile.WriteLine(" ");
            }
        }

        public void LogException(string controller, string methodName, Exception exception)
        {
            string filePath = Directory.GetCurrentDirectory() + "/Logs/ErrorLog_" + DateTime.Now.ToString("MM-dd-yyyy") + ".txt";
            using (StreamWriter outputFile = new StreamWriter(filePath, true))
            {
                outputFile.WriteLine("------------------------------------------");
                outputFile.WriteLine("DATE: " +  DateTime.Now);
                outputFile.WriteLine("CONTROLLER: " + controller);
                outputFile.WriteLine("ENDPOINT: "  + methodName);
                outputFile.WriteLine("EXCEPTION MESSAGE: " + exception.Message);
                outputFile.WriteLine("EXCEPTION STACK TRACE: " + exception.StackTrace);
                outputFile.WriteLine("EXCEPTION INNER MESSAGE: " + exception.InnerException);
                outputFile.WriteLine("------------------------------------------");
                outputFile.WriteLine(" ");
            }
        }

    }
}
