using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CTCDatabaseUpdater.Utilties
{
    public class LogWriter
    {
        private static string _logFileFullName;
        private static string _logFileFolder;
        public LogWriter()
        {
            if (ConfigurationManager.AppSettings["AbsoluteLogFilePath"] == "" && ConfigurationManager.AppSettings["RelativeLogFilePath"] == "")
            {
                throw new Exception("There is no absolute or relateive data file paths defined in Config file!");
            }
            else if (ConfigurationManager.AppSettings["AbsoluteLogFilePath"] == "")
            {
                _logFileFolder = AppDomain.CurrentDomain.BaseDirectory + "/../../" + ConfigurationManager.AppSettings["RelativeLogFilePath"];
            }
            else
            {
                _logFileFolder = ConfigurationManager.AppSettings["AbsoluteDataFilePath"];
            }

            _logFileFullName = _logFileFolder + "/Log_" + DateTime.Today.ToString("yyyyMMdd") + ".txt";
        }

        public static void WriteLog(string log)
        {
            if(!Directory.Exists(_logFileFolder))
            {
                Directory.CreateDirectory(_logFileFolder);
            }
            if(!File.Exists(_logFileFullName))
            {
                var file = File.Create(_logFileFullName);
                file.Close();
            }

            using (StreamWriter sw = File.AppendText(_logFileFullName))
            {
                sw.WriteLine(DateTime.Now + "    " + log);
            }
            
        }
    }
}
