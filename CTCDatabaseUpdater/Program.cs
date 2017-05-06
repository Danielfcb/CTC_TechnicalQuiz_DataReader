﻿
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;
using CTCDatabaseUpdater.Models;
using CTCDatabaseUpdater.DataAccessLayer;
using CTCDatabaseUpdater.Utilties;

namespace CTCDatabaseUpdater
{
    class Program
    {
        static void Main(string[] args)
        {
            
            LogWriter.WriteLog("Starting a new job!");

            // Finding the data files folder
            DataReader reader = new DataReader();
            string dataFilesFolder = reader.GetDataFilesFolder();

            // Finding all valid data files
            List<string> dataFiles = reader.GetValidFileNames(ConfigurationManager.AppSettings["ValidDataPrefixes"],
                                                              ConfigurationManager.AppSettings["DataFileExtension"]);


            // Reading data files and loding them into the memory
            DAL dataAccessLayer = new DAL();
            
            foreach (var file in dataFiles)
            {               
                try
                {
                    LogWriter.WriteLog("Starting to read file: " + file);

                    DataValidator dataValidator = new DataValidator();

                    string fileContent = reader.ReadFile(dataFilesFolder + "/" + file);

                    LogWriter.WriteLog("Validating the file: " + file);
                    dataValidator.ValidateFileContent(fileContent);

                    List<DataFileRecordModel> validRecords = dataValidator.ValidRecords;
                    List<DataFileRecordModel> duplicatedRecords = dataValidator.DuplicatedRecords;
                    List<string> invalidRecords = dataValidator.InvalidRecords;

                    LogWriter.WriteLog("Inserting valid data form " + file + " into the database");
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
                    if(invalidRecords.Count() > 0)
                        LogWriter.WriteLog(dataValidator.InvalidRecords.Count() + " Invalid Records found!\n\n" + String.Join("\n", dataValidator.InvalidRecords.ToArray()) + "\n");

                    //Setting the status of employees not mentioned in the data file to 0
                    List<string> employeeNumbersInDatabase = dataAccessLayer.GetAllEmployeeNumbers();
                    List<DataFileRecordModel> validAndDuplicatedRecords = new List<DataFileRecordModel>();
                    validAndDuplicatedRecords.AddRange(validRecords);
                    validAndDuplicatedRecords.AddRange(duplicatedRecords);
                    List<string> newEmployeeNumberList = validAndDuplicatedRecords.Select(r => r.Employee_num).ToList();

                    List<string> InactiveEmployeeNumbers = employeeNumbersInDatabase.Where(dbEmployeeNumber => !newEmployeeNumberList.Contains(dbEmployeeNumber)).ToList();

                    LogWriter.WriteLog("Setting the status of inactive employees from the file" + file );
                    dataAccessLayer.SetStatusToInactive(InactiveEmployeeNumbers);

                    LogWriter.WriteLog("Finished reading the file: " + file);
                }
                catch (Exception ex)
                {
                    LogWriter.WriteLog("Error in data file " + file);
                    LogWriter.WriteLog("Exception Stack Trace: \n\n" + ex.ToString() + "\n\n");
                }
            }

            // Some destructor tasks here (i.e. make sure if foreign ket is enables again in case something crashed)
            LogWriter.WriteLog("Job Finished!\n\n");
        }
    }
}
