
using CTCDatabaseUpdater.DataAccessLayer;
using CTCDatabaseUpdater.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static CTCDatabaseUpdater.Models.DataFileRecordModel;

namespace CTCDatabaseUpdater
{
    public class DataReader 
    {
        private string _dataFilesFolder;
        private DataValidator _dataValidator;


        public DataReader()
        {
            _dataValidator = new DataValidator();
        }

        public string GetDataFilesFolder()
        {

            if (ConfigurationManager.AppSettings["AbsoluteDataFilesPath"] == "" && ConfigurationManager.AppSettings["RelativeDataFilesPath"] == "")
            {
                throw new Exception("There is no absolute or relateive data file paths defined in Config file!");
            }
            else if (ConfigurationManager.AppSettings["AbsoluteDataFilesPath"] == "")
            {
                _dataFilesFolder = AppDomain.CurrentDomain.BaseDirectory + "/../../" + ConfigurationManager.AppSettings["RelativeDataFilesPath"];
            }
            else
            {
                _dataFilesFolder = ConfigurationManager.AppSettings["AbsoluteDataFilesPath"];
            }

            return _dataFilesFolder;
        }
        public List<string> GetValidFileNames(string filePrefix, string fileExtension)
        {
            List<string> allFiles = new List<string>();
            DirectoryInfo d = new DirectoryInfo(_dataFilesFolder);
            FileInfo[] Files = d.GetFiles( filePrefix + "*." + fileExtension); 

            foreach (FileInfo file in Files)
            {
                allFiles.Add(file.Name);
            }

            return allFiles;
        }

        public string ReadFile(string fullFileName)
        {

            StreamReader file = new StreamReader(fullFileName);

            return file.ReadToEnd();
        }

    }
}
