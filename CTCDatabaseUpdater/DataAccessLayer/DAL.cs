using CTCDatabaseUpdater.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static CTCDatabaseUpdater.Models.DataFileModel;

namespace CTCDatabaseUpdater.DataAccessLayer
{
    public class DAL
    {
        private MasterRosterEntities _db;

        public DAL()
        {
            _db = new MasterRosterEntities();
        }
        public List<Crew> GetAllDisctinctCrews()
        {
            return _db.Crews.Distinct().ToList();
        }

        public List<Department> GetAllDistinctDepartments()
        {
            return _db.Departments.Distinct().ToList();
        }

        public int? GetDepartmentIDByName(string departmentName)
        {
            return _db.Departments.Where(d => d.department_name == departmentName).SingleOrDefault().department_id;
        }

        public bool InsertIntoEmployeesTable(List<DataFileModel> records)
        {
            List<Employee> managerEmployees = new List<Employee>();
            List<Employee> nonManagerEmployees = new List<Employee>();
            int managerRoleTypeId = GetRoleTypeIdByName("Manager");

            bool result = false;
            try
            {
                foreach (var record in records)
                {
                    Employee employee = new Employee()
                    {
                        name = record.employee_name,
                        department_id = GetAllDistinctDepartments().Where(d => d.department_name == record.DepartmentName).SingleOrDefault().department_id,
                        employee_num = record.Employee_num,
                        status = (record.Status == Statuses.Active) ? (true) : (false),
                        seniority_date = Convert.ToDateTime(record.SeniorityDate),
                        roletype_id = GetAllDistinctRollTypes().Where(r => r.roletype_name == record.Role).SingleOrDefault().roletype_id,
                        crew_id = GetAllDisctinctCrews().Where(c => c.crew_code == record.Crew_Code).SingleOrDefault().crew_id,
                        supervisor_id = record.Supervisor_num
                    };

                    if (employee.roletype_id == managerRoleTypeId)
                    {
                        managerEmployees.Add(employee);
                    }
                    else
                    {
                        nonManagerEmployees.Add(employee);
                    }
                }

                result = true;
            }
            catch
            {
                result = false;
            }

            // For the manger we need to disable FK_Employees_Supervisors foreign key constraint 
            // This constraint checks to make sure supervisor_id exists in database which in case of Managers
            // there is no supervisor_id assigned
            if (managerEmployees.Count > 0)
            {
                
                _db.Database.ExecuteSqlCommand("IF (OBJECT_ID('FK_Employees_Supervisors', 'F') IS NOT NULL) BEGIN ALTER TABLE Employees DROP CONSTRAINT FK_Employees_Supervisors END");

                foreach(var manager in managerEmployees)
                {
                    _db.Employees.Add(manager);
                }
                _db.SaveChanges();

                _db.Database.ExecuteSqlCommand("IF (OBJECT_ID('FK_Employees_Supervisors', 'F') IS NULL) BEGIN ALTER TABLE Employees With Nocheck Add CONSTRAINT [FK_Employees_Supervisors] foreign key(supervisor_id) references[Employees](employee_num) END");
                
            }
            if(nonManagerEmployees.Count > 0 )
            {

                foreach(var employee in nonManagerEmployees)
                {
                    _db.Employees.Add(employee);
                    
                }

                _db.SaveChanges();
            }
            return result;
        }

        public List<RoleType> GetAllDistinctRollTypes()
        {
            return _db.RoleTypes.Distinct().ToList();
        }

        public int GetRoleTypeIdByName(string roleTypeName)
        {
            return _db.RoleTypes.Where(r => r.roletype_name == roleTypeName).SingleOrDefault().roletype_id;
        }
    }
}
