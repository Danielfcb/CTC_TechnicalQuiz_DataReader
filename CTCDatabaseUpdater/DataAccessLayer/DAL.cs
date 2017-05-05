using CTCDatabaseUpdater.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static CTCDatabaseUpdater.Models.DataFileRecordModel;

namespace CTCDatabaseUpdater.DataAccessLayer
{
    public class DAL
    {
        private MasterRosterContext _db;

        public DAL()
        {
            _db = new MasterRosterContext();
        }
        public List<Crew> GetAllDisctinctCrews()
        {
            return _db.Crews.Distinct().ToList();
        }

        public List<Department> GetAllDistinctDepartments()
        {
            return _db.Departments.Distinct().ToList();
        }

        public bool DoesEmployeeNumberExist(string employeeNumber)
        {
            return (_db.Employees.Where(e => e.employee_num == employeeNumber).Count() > 0);
        }

        public int? GetDepartmentIDByName(string departmentName)
        {
            return _db.Departments.Where(d => d.department_name == departmentName).SingleOrDefault().department_id;
        }

        public bool InsertIntoEmployeesTable(List<Employee> employees, bool disableForeignKey = false)
        {
            bool result = true;
            try
            {
                // For the manger we need to disable FK_Employees_Supervisors foreign key constraint 
                // This constraint checks to make sure supervisor_id exists in database which in case of Managers
                // there is no supervisor_id assigned
                if (disableForeignKey)
                {
                    _db.Database.ExecuteSqlCommand("IF (OBJECT_ID('FK_Employees_Supervisors', 'F') IS NOT NULL) BEGIN ALTER TABLE Employees DROP CONSTRAINT FK_Employees_Supervisors END");
                }

                foreach (var employee in employees)
                {
                    _db.Employees.Add(employee);
                }

                _db.SaveChanges();

                if (disableForeignKey)
                {
                    _db.Database.ExecuteSqlCommand("IF (OBJECT_ID('FK_Employees_Supervisors', 'F') IS NULL) BEGIN ALTER TABLE Employees With Nocheck Add CONSTRAINT [FK_Employees_Supervisors] foreign key(supervisor_id) references[Employees](employee_id) END");
                }
            }
            catch
            {
                result = false;
                // log the details
            }

            return result;
        }


        public List<string> GetAllEmployeeNumbers()
        {
            return _db.Employees.Select(e => e.employee_num).ToList();
        }

        public void SetStatusToInactive(string employeeNumber)
        {
            _db.Database.ExecuteSqlCommand("Update Employees set Status = 0 where employee_num='" + employeeNumber + "'");

        }

        public void RemoveEmployeeFromDatabase(string employeeNumber)
        {
            _db.Database.ExecuteSqlCommand("delete from Employees where employee_num='" + employeeNumber + "'");
        }

        public int? GetLastEmployeeIdForEmployeeNumber(string employeeNumber)
        {
            if (!string.IsNullOrEmpty(employeeNumber))
            {
                return _db.Employees.Where(e => e.employee_num == employeeNumber).SingleOrDefault().employee_id;
            }
            else
            {
                return null;
            }
        }
        public List<RoleType> GetAllDistinctRollTypes()
        {
            return _db.RoleTypes.Distinct().ToList();
        }

        public int GetRoleTypeIdByName(string roleTypeName)
        {
            return _db.RoleTypes.Where(r => r.roletype_name == roleTypeName).SingleOrDefault().roletype_id;
        }

        public bool UpdateEmployeeRecord(Employee employee)
        {
            bool result = true;
            try
            {
                _db.Employees.Attach(employee);
                _db.Entry(employee).State = System.Data.Entity.EntityState.Modified;
                _db.SaveChanges();
            }
            catch
            {
                result = false;
                //log the details
            }

            return result;
        }
    }
}

