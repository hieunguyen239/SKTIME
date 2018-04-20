using Dapper;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace SKtimeManagement
{
    public class EmployeeFineFilter
    {
        public EmployeeFineFilter(int? employeeID, DateTime? from = null, DateTime? to = null)
        {
            EmployeeID = employeeID;
            From = from;
            To = to;
        }
        public int? EmployeeID { get; set; }
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = Constants.DateString)]
        public DateTime? From { get; set; }
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = Constants.DateString)]
        public DateTime? To { get; set; }
    }
    public class EmployeeFineList
    {
        public EmployeeFineList(int employeeID, List<EmployeeFine> data = null)
        {
            EmployeeID = employeeID;
            Data = data == null ? new List<EmployeeFine>() : data;
        }
        public int EmployeeID { get; set; }
        public List<EmployeeFine> Data { get; set; }
    }
    public class EmployeeFine : BaseModel
    {
        public int ID { get; set; }
        public int EmployeeID { get; set; }
        public string EmployeeName { get; set; }
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = Constants.DateTimeString)]
        public DateTime SubmitDate { get; set; }
        public decimal Amount { get; set; }
        public string AmountString { get { return Amount.GetCurrencyString(); } }
        public string Reason { get; set; }
        public EmployeeFineList Save(int userID, int employeeID, int bussinessID, string action, string code = "Name")
        {
            QueryOutput queryOutput;
            var query = String.Format(
                @"insert into EmployeeFine(EmployeeID, SubmitDate, Amount, Reason) 
                values ({0}, {1}, {2}, N'{3}') {4}",
                EmployeeID, SubmitDate.DbValue(), Amount, Reason, Query(bussinessID, new EmployeeFineFilter(EmployeeID)));
            return new EmployeeFineList(EmployeeID, Query<EmployeeFine>(new DbQuery(userID, employeeID, action, query, true, EmployeeID, code), out queryOutput));
        }
        public static string Query(int bussinessID, EmployeeFineFilter filter = null)
        {
            var conditions = new List<string>();
            if (filter != null)
            {
                if (filter.EmployeeID.HasValue)
                    conditions.Add(String.Format("and f.EmployeeID = {0}", filter.EmployeeID.Value));
                if (filter.From.HasValue)
                    conditions.Add(String.Format("and f.SubmitDate >= {0}", filter.From.DbValue()));
                if (filter.To.HasValue)
                    conditions.Add(String.Format("and f.SubmitDate <= {0}", filter.To.DbValue()));
            }
            var query = String.Format(
                @" select f.*, e.Name as [EmployeeName] from EmployeeFine f join Employee e on f.EmployeeID = e.ID where e.BussinessID = {0} {1} order by f.ID desc", 
                bussinessID, String.Join(" ", conditions));
            return query;
        }
        public static List<EmployeeFine> Get(int bussinessID, int employeeID)
        {
            using (var con = Repo.DB.SKtimeManagement)
            {
                return con.Query<EmployeeFine>(Query(bussinessID, new EmployeeFineFilter(employeeID))).ToList();
            }
        }
        public static EmployeeFineList Get(int userID, int employeeID, int bussinessID, string action, EmployeeFineFilter filter = null)
        {
            QueryOutput queryOutput;
            return new EmployeeFineList(
                filter.EmployeeID.HasValue ? filter.EmployeeID.Value : 0, 
                Query<EmployeeFine>(new DbQuery(userID, employeeID, action, Query(bussinessID, filter)), out queryOutput));
        }
        public static EmployeeFineList Remove(int userID, int employeeID, int bussinessID, string action, int id, int employee, string code = "Name")
        {
            QueryOutput queryOutput;
            var query = String.Format(
                @"delete EmployeeFine where ID = {0} {1}", id, Query(bussinessID, new EmployeeFineFilter(employee)));
            return new EmployeeFineList(employee, Query<EmployeeFine>(new DbQuery(userID, employeeID, action, query, true, employee, code), out queryOutput));
        }
    }
}