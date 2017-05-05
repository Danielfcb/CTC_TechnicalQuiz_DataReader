
using CTCDatabaseUpdater.DataAccessLayer;
using CTCDatabaseUpdater.Models;
using CTCDatabaseUpdater.Utilties;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static CTCDatabaseUpdater.Models.DataFileRecordModel;

namespace CTCDatabaseUpdater
{
    public class DataValidator : iDataValidator<DataFileRecordModel>
    {
        private DAL _dal;
        public List<DataFileRecordModel> ValidRecords { get; private set; }
        public List<string> InvalidRecord { get; private set; }
        public List<DataFileRecordModel> DuplicatedRecords { get; private set; }
        public string HeaderLine { get; private set; }

        public DataValidator()
        {
            _dal = new DAL();
        }

        public void ValidateFileContent(string fileContent)
        {
            StringReader strReader = new StringReader(fileContent);
            HeaderLine = strReader.ReadLine();

            bool read = true;

            while(read)
            {
                string record = strReader.ReadLine();
                if(record != null)
                {
                    ValidateRecord(record);

                }
                else
                {
                    read = false;
                }
            }

        }

        public void ValidateRecord(string record)
        {
            bool result = true;

            string[] recordFields = record.Split(',');

            result = isDepartmentNameValid(recordFields[0]);
            if (result)
                result = isCrewCodeValid(recordFields[1]);
            if (result)
                result = isEmployeeNameValid(recordFields[2]);
            if (result)
                result = isStatusValid(recordFields[3]);
            if (result)
                result = isEmployeeNumberValid(recordFields[4]);
            if (result)
                result = isRoleTypeValid(recordFields[5]);
            if (result)
                result = isSeniorityDateValid(recordFields[6]);
            if (result)
                result = isSupervisorNameValid(recordFields[7], recordFields[5]);
            if (result)
                result = isSupervisorNumberValid(recordFields[8], recordFields[5]);
            // check that someone shoudn't report to themselves   

            if(result)
            {
                DataFileRecordModel dataFileModel = CreateDataFileRecordModel(record);
                // check if the data is duplicated
                if (isDuplicatedRecord(record))
                {
                    DuplicatedRecords.Add(dataFileModel);
                }
                else
                {
                    ValidRecords.Add(dataFileModel);
                }
            }
            else
            {
                InvalidRecord.Add(record);
            }


        }

        public DataFileRecordModel CreateDataFileRecordModel(string record)
        {
            string[] recordFields = record.Split(',');

            DataFileRecordModel dataFileRecordModel = new DataFileRecordModel()
            {
                DepartmentName = recordFields[0],
                Crew_Code = recordFields[1],
                employee_name = recordFields[2],
                Status = (Statuses)Enum.Parse(typeof(Statuses), recordFields[3]),
                Employee_num = recordFields[4],
                Role = recordFields[5],
                SeniorityDate = recordFields[6],
                Supervisor_name = recordFields[7],
                Supervisor_num = recordFields[8]
            };

            return dataFileRecordModel;
        }

        private bool isDepartmentNameValid(string departmentName)
        {
            bool result = true;

            if (!string.IsNullOrEmpty(departmentName) && _dal.GetAllDistinctDepartments().Where(d => d.department_name == departmentName).Count() == 0)
            {
                result = false;
            }

            return result;
        }

        private bool isCrewCodeValid(string crewCode)
        {
            bool result = true;

            if(!string.IsNullOrEmpty(crewCode) && _dal.GetAllDisctinctCrews().Where(c => c.crew_code == crewCode).Count() ==0)
            {
                result = false;
            }
            return result;
        }

        private bool isEmployeeNameValid(string employeeName)
        {
            bool result = true;

            if(string.IsNullOrEmpty(employeeName))
            {
                result = false;
            }

            return result;
        }

        private bool isStatusValid(string status)
        {
            bool result = true;

            try
            {
                Statuses theStats = (Statuses)Enum.Parse(typeof(Statuses), status);
            }
            catch
            {
                result = false;
            }

            return result;
        }

        private bool isEmployeeNumberValid(string employeeNumber)
        {
            bool result = true;

            if(string.IsNullOrEmpty(employeeNumber))
            {
                result = false;
            }

            return result;
        }

        private bool isRoleTypeValid(string role)
        {
            bool result = true;

            if (!string.IsNullOrEmpty(role) && _dal.GetAllDistinctRollTypes().Where(r => r.roletype_name == role).Count() == 0)
            {
                result = false;
            }

            return result;
        }

        private bool isSeniorityDateValid(string seniorityDate)
        {
            bool result = true;

            if(string.IsNullOrEmpty(seniorityDate))
            {
                result = false;
            }
            DateTime temp;

            if(result && !DateTime.TryParse(seniorityDate, out temp))
            {
                result = false;
            }

            return result;
        }

        private bool isSupervisorNameValid(string supervisorName, string roleType)
        {
            bool result = true;

            // Other than managers, rest of the employees must have a supervisor assigned
            if(roleType != "Manager" && string.IsNullOrEmpty(supervisorName))
            {
                result = false;
            }

            return result;
        }

        /// <summary>
        /// Other than Managers, rest of the employees must have a supervisor assigned
        /// supervisor Id (employee_id) must be avialble in database already or it should be 
        /// in the data file ready to inserted into the database
        /// </summary>
        /// <param name="supervisorNumber"></param>
        /// <param name="roleType"></param>
        /// <returns></returns>
        private bool isSupervisorNumberValid(string supervisorNumber, string roleType)
        {
            bool result = true;

            // Other than managers, rest of the employees must have a supervisor assigned
            if (roleType != "Manager" && string.IsNullOrEmpty(supervisorNumber))
            {
                result = false;
            }

            return result;
        }
        
        private bool isDuplicatedRecord(string employeeNumner)
        {
            return _dal.DoesEmployeeNumberExist(employeeNumner);
        }


        #region GettingFinalRecords
        public List<DataFileRecordModel> GetValidatedManagerRecords()
        {
            return (ValidRecords.Where(vr => vr.Role == "Manager").ToList());

        }

        public List<DataFileRecordModel> GetDuplicatedManagerRecords()
        {
            return DuplicatedRecords.Where(vr => vr.Role == "Manager").ToList();
        }

        public List<DataFileRecordModel> GetValidatedSupervisorRecords()
        {
            return ValidRecords.Where(vr => vr.Role == "Supervisor").ToList();
        }

        public List<DataFileRecordModel> GetDuplicatedSupervisorRecords()
        {
            return DuplicatedRecords.Where(vr => vr.Role == "Supervisor").ToList();
        }

        public List<DataFileRecordModel> GetValidatedWorkerRecords()
        {
            return ValidRecords.Where(vr => vr.Role == "Worker").ToList();
        }

        public List<DataFileRecordModel> GetDuplicatedWorkerRecords()
        {
            return DuplicatedRecords.Where(vr => vr.Role == "Worker").ToList();
        }
        #endregion




        #region GettingEmployees
        public List<Employee> GetValidatedManagerEmployees()
        {
            return CreateEmployeesFromRecords(GetValidatedManagerRecords());

        }

        public List<Employee> GetDuplicatedManagerEmployees()
        {
            return CreateEmployeesFromRecords(GetDuplicatedManagerRecords());
        }

        public List<Employee> GetValidatedSupervisorEmployees()
        {
            return CreateEmployeesFromRecords(GetValidatedSupervisorRecords());
        }

        public List<Employee> GetDuplicatedSupervisorEmployees()
        {
            return CreateEmployeesFromRecords(GetDuplicatedSupervisorRecords());
        }

        public List<Employee> GetValidatedWorkerEmployees()
        {
            return CreateEmployeesFromRecords(GetValidatedWorkerRecords());
        }

        public List<Employee> GetDuplicatedWorkerEmployees()
        {
            return CreateEmployeesFromRecords(GetDuplicatedWorkerRecords());
        }

        #endregion



        public List<Employee> CreateEmployeesFromRecords(List<DataFileRecordModel> records)
        {
            List<Employee> employees = new List<Employee>();
            List<Department> allDistinctDepartments = _dal.GetAllDistinctDepartments();
            List<RoleType> allDistinctRoleTypes = _dal.GetAllDistinctRollTypes();
            List<Crew> allDistinctCrews = _dal.GetAllDisctinctCrews();

            foreach (var record in records)
            {
                Employee employee = new Employee()
                {
                    name = record.employee_name,
                    department_id = allDistinctDepartments.Where(d => d.department_name == record.DepartmentName).SingleOrDefault().department_id,
                    employee_num = record.Employee_num,
                    status = (record.Status == Statuses.Active) ? (true) : (false),
                    seniority_date = Convert.ToDateTime(record.SeniorityDate),
                    roletype_id = allDistinctRoleTypes.Where(r => r.roletype_name == record.Role).SingleOrDefault().roletype_id,
                    crew_id = allDistinctCrews.Where(c => c.crew_code == record.Crew_Code).SingleOrDefault().crew_id,
                    supervisor_id = _dal.GetLastEmployeeIdForEmployeeNumber(record.Supervisor_num)
                };

                employees.Add(employee);
            }

            return employees;
        }

    }
}
