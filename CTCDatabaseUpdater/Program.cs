
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
            // Finding the data files folder
            DataReader reader = new DataReader();
            string dataFilesFolder = reader.GetDataFilesFolder();

            // Finding all valid data files
            List<string> dataFiles = reader.GetValidFileNames(ConfigurationManager.AppSettings["ValidDataPrefixes"],
                                                              ConfigurationManager.AppSettings["DataFileExtension"]);


            // Reading data files and loding them into the memory
            DAL dataAccessLayer = new DAL();
            DataValidator dataValidator = new DataValidator();
            foreach (var file in dataFiles)
            {
                string fileContent = reader.ReadFile(dataFilesFolder + "/" + file);
                dataValidator.ValidateFileContent(fileContent);

                List<DataFileRecordModel> validRecords = dataValidator.ValidRecords;
                List<DataFileRecordModel> duplicatedRecords = dataValidator.DuplicatedRecords;
                List<string> invalidRecords = dataValidator.InvalidRecords;

                // Employees with "Manager" role need to be inserted (if they don't exist) or updated (if they are already in database)
                dataAccessLayer.InsertIntoEmployeesTable(dataValidator.GetValidatedManagerEmployees(), true);
                dataAccessLayer.UpdateEmployeeRecords(dataValidator.GetDuplicatedManagerEmployees(), true);

                // Employees with "Supervisor" role need to be inserted (if they don't exist) or updated (if they are already in database)
                dataAccessLayer.InsertIntoEmployeesTable(dataValidator.GetValidatedSupervisorEmployees());
                dataAccessLayer.UpdateEmployeeRecords(dataValidator.GetDuplicatedSupervisorEmployees());

                // Employee with "Worker" role need to be inserted (if they don't exist) or updated (if they are already in database)
                dataAccessLayer.InsertIntoEmployeesTable(dataValidator.GetValidatedWorkerEmployees());
                dataAccessLayer.UpdateEmployeeRecords(dataValidator.GetDuplicatedWorkerEmployees());

                // Invalid records are logged in a log file - The log file address should be read from the Config file



                //Setting the status of employees not mentioned in the data file to 0
                List<string> employeeNumbersInDatabase = dataAccessLayer.GetAllEmployeeNumbers();
                List<string> newEmployeeNumberList = validRecords.Select(r => r.Employee_num).ToList();

                List<string> InactiveEmployeeNumbers = employeeNumbersInDatabase.Where(dbEmployeeNumber => !newEmployeeNumberList.Contains(dbEmployeeNumber)).ToList();
            }

        }
    }
}
