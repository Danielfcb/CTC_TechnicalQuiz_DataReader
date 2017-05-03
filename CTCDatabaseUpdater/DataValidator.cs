
using CTCDatabaseUpdater.DataAccessLayer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

            //validate the department name
            if(string.IsNullOrEmpty(lineElements[0]))
            {
                result = false;
            }
            if(result && _dal.GetAllDistinctDepartments().Where(d => d.department_name == lineElements[0]).Count() == 0)
            {
                result = false;
            }

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
    }
}
