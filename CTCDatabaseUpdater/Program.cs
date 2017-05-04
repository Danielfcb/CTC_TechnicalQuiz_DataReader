
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;
using CTCDatabaseUpdater.Models;
using CTCDatabaseUpdater.DataAccessLayer;

namespace CTCDatabaseUpdater
{
    class Program
    {
        static void Main(string[] args)
        {
            string dataFilesPath;

            if (ConfigurationManager.AppSettings["AbsoluteDataFilesPath"] == "" && ConfigurationManager.AppSettings["RelativeDataFilesPath"] == "")
            {
                throw new Exception("There is no absolute or relateive data file paths defined in Config file!");
            }
            else if(ConfigurationManager.AppSettings["AbsoluteDataFilesPath"] == "")
            {
                dataFilesPath = AppDomain.CurrentDomain.BaseDirectory + "/../../" + ConfigurationManager.AppSettings["RelativeDataFilesPath"];
            }
            else
            {
                dataFilesPath = ConfigurationManager.AppSettings["AbsoluteDataFilesPath"];
            }

            DataReader reader = new DataReader(dataFilesPath);
            List<string> dataFiles = reader.GetValidFileNames(ConfigurationManager.AppSettings["ValidDataPrefixes"],
                                                              ConfigurationManager.AppSettings["DataFileExtension"]);


            List<DataFileModel> validRecords = new List<DataFileModel>();
            foreach (var file in dataFiles)
            {
                validRecords = reader.ReadFile(dataFilesPath + "/" + file);
            }

            List<DataFileModel> managerEmployees = new List<DataFileModel>();
            List<DataFileModel> supervisorEmployees = new List<DataFileModel>();
            List<DataFileModel> workerEmployees = new List<DataFileModel>();


            foreach(var employee in validRecords)
            {
                if(employee.Role == "Manager")
                {
                    managerEmployees.Add(employee);
                }
                else if (employee.Role == "Supervisor")
                {
                    supervisorEmployees.Add(employee);
                }
                else if(employee.Role == "Worker")
                {
                    workerEmployees.Add(employee);
                }
            }
            DAL dal = new DAL();

            dal.InsertIntoEmployeesTable(managerEmployees);
            dal.InsertIntoEmployeesTable(supervisorEmployees);
            dal.InsertIntoEmployeesTable(workerEmployees);

            //Setting the status of employees not mentioned in the data file to 0
            List<string> employeeNumbersInDatabase = dal.GetAllEmployeeNumbers();
            List<string> newEmployeeNumberList = validRecords.Select(r => r.Employee_num).ToList();

            List<string> InactiveEmployeeNumbers = employeeNumbersInDatabase.Where(dbEmployeeNumber => !newEmployeeNumberList.Contains(dbEmployeeNumber)).ToList();


        }
    }
}
