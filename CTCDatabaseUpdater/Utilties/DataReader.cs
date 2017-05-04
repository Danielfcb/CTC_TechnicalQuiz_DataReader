
using CTCDatabaseUpdater.DataAccessLayer;
using CTCDatabaseUpdater.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static CTCDatabaseUpdater.Models.DataFileModel;

namespace CTCDatabaseUpdater
{
    public class DataReader : iDataReader<DataFileModel>
    {
        private string _dataFilePath;
        private DAL _dal;
        private DataValidator _dataValidator;


        public DataReader(string dataFilePath)
        {
            _dataFilePath = dataFilePath;
            _dal = new DAL();
            _dataValidator = new DataValidator();
        }

        

        public List<string> GetValidFileNames(string filePrefix, string fileExtension)
        {
            List<string> allFiles = new List<string>();
            DirectoryInfo d = new DirectoryInfo(_dataFilePath);
            FileInfo[] Files = d.GetFiles( filePrefix + "*." + fileExtension); 

            foreach (FileInfo file in Files)
            {
                allFiles.Add(file.Name);
            }

            return allFiles;
        }

        public List<DataFileModel> ReadFile(string fullFilePath)
        {

            // Go through the files one by one and read the fields
            // Validates each line and if good add to the list of good records
            // if bad create a new err file and put the records with appropriate description of the error
            // Export the valid data into the database (this should be done in another class in Main method)
            List<DataFileModel> validRecords = new List<DataFileModel>();
            StreamReader file = new StreamReader(fullFilePath);
            string line;

            // reading the header line
            line = file.ReadLine();

            while((line = file.ReadLine()) != null)
            {
                bool isDuplicated;            
                if (_dataValidator.IsRecordValid(line, out isDuplicated))
                {
                    
                    string[] lineElements = line.Split(',');

                    if (isDuplicated)
                    {
                        // Removing is not a good idea
                        //_dal.RemoveEmployeeFromDatabase(lineElements[4]);
                    }

                    DataFileModel dataModel = new DataFileModel()
                    {
                        DepartmentName = lineElements[0],
                        Crew_Code = lineElements[1],
                        employee_name = lineElements[2],
                        Status = (Statuses)Enum.Parse(typeof(Statuses), lineElements[3]),
                        Employee_num = lineElements[4],
                        Role = lineElements[5],
                        SeniorityDate = lineElements[6],
                        Supervisor_name = lineElements[7],
                        Supervisor_num = lineElements[8]
                    };

                    validRecords.Add(dataModel);
                }
                else
                {
                    // Log the currpted reocrd to the log file
                }
            }

            return validRecords;
        }
    }
}
