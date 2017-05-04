
using CTCDatabaseUpdater.DataAccessLayer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static CTCDatabaseUpdater.Models.DataFileModel;

namespace CTCDatabaseUpdater
{
    public class DataValidator
    {
        private DAL _dal;

        public DataValidator()
        {
            _dal = new DAL();
        }
        public bool IsRecordValid(string record)
        {
            bool result = true;
            string[] lineElements = record.Split(',');



            //validate crew_code
            


            //DepartmentName = lineElements[0],
            //Crew_Code = _allCrews.Where(c => c.crew_code == lineElements[0]).SingleOrDefault().crew_code, 
            //employee_name = lineElements[2],
            //Status = (Statuses)Enum.Parse(typeof(Statuses), lineElements[3]),
            //Employee_num = lineElements[4],
            //Role = (Roles)Enum.Parse(typeof(Roles), lineElements[5]),
            //SeniorityDate = lineElements[6],
            //Supervisor_name = lineElements[7],
            //Supervisor_num = lineElements[8]

            return result;
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
        /// supervisor Id mu
        /// 
        /// </summary>
        /// <param name="supervisorId"></param>
        /// <param name="roleType"></param>
        /// <returns></returns>
        private bool isSupervisorIdValid(string supervisorId, string roleType)
        {
            bool result = true;

            // Other than managers, rest of the employees must have a supervisor assigned
            if (roleType != "Manager" && string.IsNullOrEmpty(supervisorId))
            {
                result = false;
            }

            return result;
        }
    }
}
