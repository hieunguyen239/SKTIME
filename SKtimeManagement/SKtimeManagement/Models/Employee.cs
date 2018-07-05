using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using Dapper;
using System.Web.Mvc;

namespace SKtimeManagement
{
    public class EmployeePosition
    {
        public const string Officer = "Nhân viên văn phòng";
        public const string Sale = "Nhân viên bán hàng";
        public static string[] List
        {
            get
            {
                return new string[] { EmployeePosition.Officer, EmployeePosition.Sale };
            }
        }
    }
    public class EmployeeWorkStatus
    {
        public const string Working = "Đang làm";
        public const string Retired = "Đã nghỉ";
        public static string[] List
        {
            get
            {
                return new string[] { EmployeeWorkStatus.Working, EmployeeWorkStatus.Retired };
            }
        }
    }
    public class EmployeeFilter
    {
        public int? ID { get; set; }
        public string Name { get; set; }
        public int? StoreID { get; set; }
        public string Phone { get; set; }
        public string Address { get; set; }
        public string WorkStatus { get; set; }
        public int? LoginID { get; set; }
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = Constants.DateTimeString)]
        public DateTime? DOB { get; set; }
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = Constants.DateTimeString)]
        public DateTime? From { get; set; }
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = Constants.DateTimeString)]
        public DateTime? To { get; set; }
        public string BaseSalary { get; set; }
        public bool ViewBaseSalary { get { return BaseSalary == "on"; } }
        public int? Month { get; set; }
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = Constants.MonthString)]
        public DateTime? SalaryMonth { get; set; }
    }
    public class EmployeeList
    {
        public EmployeeList(string message = "", EmployeeFilter filter = null)
        {
            Data = new List<EmployeeInfo>();
            Filter = filter != null ? filter : new EmployeeFilter();
            Message = message;
        }
        public List<EmployeeInfo> Data { get; set; }
        public EmployeeFilter Filter { get; set; }
        public string Message { get; set; }
    }
    public class EmployeeInfo : BaseModel
    {
        public EmployeeInfo() : base() { }
        public EmployeeInfo(int bussinessID, int? storeID = null) : base()
        {
            BussinessID = bussinessID;
            StoreID = storeID;
            StartDate = DateTime.Now;
        }
        public static EmployeeList Find(int userID, int employeeID, int bussinessID, string message = "", EmployeeFilter filter = null, bool log = false)
        {
            QueryOutput queryResult;
            var conditions = new List<string>();
            if (filter != null)
            {
                if (!String.IsNullOrEmpty(filter.Address))
                    conditions.Add(String.Format("and e.Address like N'%{0}%'", filter.Address));
                if (filter.LoginID.HasValue)
                    conditions.Add(String.Format("and l.ID = {0}", filter.LoginID.DbValue()));
                if (!String.IsNullOrEmpty(filter.Name))
                    conditions.Add(String.Format("and e.Name like N'%{0}%'", filter.Name));
                if (!String.IsNullOrEmpty(filter.Phone))
                    conditions.Add(String.Format("and e.Phone like N'%{0}%'", filter.Phone));
                if (!String.IsNullOrEmpty(filter.WorkStatus))
                    conditions.Add(String.Format("and e.WorkStatus = N'{0}'", filter.WorkStatus));
                if (filter.StoreID.HasValue)
                    conditions.Add(String.Format("and e.StoreID = {0}", filter.StoreID.DbValue()));
                if (filter.DOB.HasValue)
                    conditions.Add(String.Format("and e.DOB >= '{0}' and e.DOB <= '{1}'", filter.DOB.Value.ToString("yyyy-MM-01"), filter.DOB.Value.AddMonths(1).ToString("yyyy-MM-01")));
                if (filter.Month.HasValue)
                    conditions.Add(String.Format("and month(e.DOB) = {0}", filter.Month.Value));
            }
            var result = new EmployeeList(message, filter);
            result.Data = Query<EmployeeInfo>(new DbQuery(userID, employeeID, DbAction.Employee.View, 
                String.Format(@"select e.*, l.ID as [LoginID], l.Username, isnull(sum(o.Total - o.Discount), 0) as [CurrentSale], s.Name as [StoreName]
                    from Employee e left join Login l on l.EmployeeID = e.ID left join Store s on e.StoreID = s.ID
	                    left join [Order] o on o.EmployeeID  = e.ID and o.Removed = 0 and o.Status <> N'{2}'
                    where e.Status = 'active' and e.BussinessID = {0} {1}
                    group by e.AdditionalSalary, e.Address,  e.BaseSalary, e.BussinessID, e.ID, e.Image, e.MonthlySale, e.Name,  e.OffDays, e.Phone, e.Position, e.DOB, s.Name,
	                    e.StartDate, e.Status, e.StoreID, e.Summary,  e.WorkDays, e.WorkTime, e.EndDate, e.BankNumber, e.BankName, e.BankBranch, e.WorkStatus, l.ID, l.Username
                    order by e.Name", 
                bussinessID, String.Join(" ", conditions), OrderStatus.Refunded), log), out queryResult);
            return result;
        }
        public static EmployeeInfo Get(int userID, int employeeID, int id, bool log = false)
        {
            QueryOutput queryResult;
            return Query<EmployeeInfo>(new DbQuery(userID, employeeID, DbAction.Employee.View, 
                String.Format(@"select e.*, l.ID as [LoginID], l.Username, s.Name as [StoreName], isnull(sum(o.Total - o.Discount), 0) as [CurrentSale]
                                from Employee e left join Login l on l.EmployeeID = e.ID left join Store s on e.StoreID = s.ID left join [Order] o on o.EmployeeID  = e.ID and o.Removed = 0 and o.Status <> N'{1}'
                                where e.Status = 'active' and e.ID = {0}
                                group by e.AdditionalSalary, e.Address,  e.BaseSalary, e.BussinessID, e.ID, e.Image, e.MonthlySale, e.Name,  e.OffDays, e.Phone, e.Position, e.DOB,
	                                e.StartDate, e.Status, e.StoreID, e.Summary,  e.WorkDays, e.WorkTime, e.EndDate, e.BankNumber, e.BankName, e.BankBranch, e.WorkStatus, l.ID, l.Username, s.Name", id, OrderStatus.Refunded), log), out queryResult).FirstOrDefault();
        }
        public static List<EmployeeInfo> GetUnassigned(int userID, int employeeID, int bussinessID)
        {
            QueryOutput queryResult;
            return Query<EmployeeInfo>(new DbQuery(userID, employeeID, DbAction.Employee.View, 
                String.Format(
                    @"select e.* from Employee e
                    where e.Status = 'active' and e.BussinessID = {0} and 
                    e.ID not in (select EmployeeID from Login where Status = 'active' and EmployeeID is not null and BussinessID = {0})
                    order by e.Name", 
                    bussinessID)), out queryResult);
        }
        public bool Save(ModelStateDictionary modelState, int userID, int employeeID)
        {
            if (!Validate(modelState))
            {
                return Result = false;
            }
            QueryOutput queryResult;
            var query = "";
            var action = "";
            var id = ID.ToString();
            if (!StartDate.HasValue)
                StartDate = DateTime.Now;
            if (ID > 0)
            {
                query += String.Format(
                    @"update Employee 
                    set Name = N'{0}', Address = N'{1}', Phone = N'{2}', Position = N'{3}',
                    Image = N'{4}', BussinessID = {5}, StoreID = {6}, StartDate = {7},
                    WorkTime = N'{8}', MonthlySale = {9}, BaseSalary = {10}, AdditionalSalary = {11},
                    WorkDays = {12}, OffDays = {13}, Summary = N'{14}', EndDate = {16},
                    BankNumber = N'{17}', BankName = N'{18}', BankBranch = N'{19}', WorkStatus = N'{20}', DOB = {21}
                    where ID = {15}", new object[] {
                        Name, Address, Phone, Position, Image, BussinessID, StoreID.DbValue(), StartDate.DbValue(Constants.DatabaseDateString),
                        WorkTime, MonthlySale, BaseSalary, AdditionalSalary, WorkDays, OffDays, Summary, ID,
                        EndDate.DbValue(Constants.DatabaseDateString), BankNumber, BankName, BankBranch, WorkStatus, DOB.DbValue(Constants.DatabaseDateString)
                    });
                if (LoginID > 0)
                    query += String.Format(" update Login set EmployeeID = {0} where ID = {1}", ID, LoginID);
                else
                    query += String.Format(" update Login set EmployeeID = null where EmployeeID = {0}", ID);
                action = DbAction.Employee.Modify;
            }
            else
            {
                query += String.Format(@"declare @ID table (ID int)
                                            insert Employee(Name, Address, Phone, BussinessID, StoreID, Position, Image, StartDate, WorkTime, MonthlySale, BaseSalary, AdditionalSalary, WorkDays, OffDays, Summary, Status, EndDate, BankNumber, BankName, BankBranch, WorkStatus, DOB) 
                                            output inserted.ID into @ID
                                            values (N'{0}', N'{1}', N'{2}', {3}, {4}, N'{5}', N'{6}', {7}, N'{8}', {9}, {10}, {11}, {12}, {13}, N'{14}', 'active', {15}, N'{16}', N'{17}', N'{18}', N'{19}', {20})",
                                        new object[] { Name, Address, Phone, BussinessID, StoreID.DbValue(), Position, Image, StartDate.DbValue(Constants.DatabaseDateString),
                                            WorkTime, MonthlySale, BaseSalary, AdditionalSalary, WorkDays, OffDays, Summary,
                                            EndDate.DbValue(Constants.DatabaseDateString), BankNumber, BankName, BankBranch, WorkStatus, DOB.DbValue(Constants.DatabaseDateString)
                });
                id = "(select top 1 ID from @ID)";
                if (LoginID > 0)
                    query += String.Format(" update Login set EmployeeID = (select top 1 ID from @ID) where ID = {0}", LoginID);
                action = DbAction.Employee.Create;
            }
            Result = Execute(new DbQuery(userID, employeeID, action, query, true, id, "Name"), out queryResult);
            if (Result)
                Messages = new List<string>() { "Lưu thông tin thành công" };
            return Result;
        }
        public static bool Remove(int userID, int employeeID, int removeID)
        {
            QueryOutput queryResult;
            return Execute(new DbQuery(userID, employeeID, DbAction.Employee.Remove, String.Format("update Employee set Status = 'removed' where ID = {0}", removeID), true, removeID, "Name"), out queryResult);
        }
        public static bool IsAdmin(int userID, int employeeID, int id, string action)
        {
            QueryOutput queryResult;
            return Query<bool>(new DbQuery(userID, employeeID, action, String.Format("select case when l.ID is null then 0 else 1 end from Employee e left join Login l on e.ID = l.EmployeeID and l.ID = 1 where e.ID = {0}", id)), out queryResult).FirstOrDefault();
        }
        public int ID { get; set; }
        [Required(ErrorMessage = "Tên nhân viên không được bỏ trống")]
        public string Name { get; set; }
        [Required(ErrorMessage = "Vị trí không được bỏ trống")]
        public string Position { get; set; }
        public string Image { get; set; }
        public int BussinessID { get; set; }
        public int? StoreID { get; set; }
        public string StoreName { get; set; }
        [Phone(ErrorMessage = "Số điện thoại không hợp lệ")]
        public string Phone { get; set; }
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = Constants.DateTimeString)]
        public DateTime? DOB { get; set; }
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = Constants.DateTimeString)]
        public DateTime? StartDate { get; set; }
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = Constants.DateTimeString)]
        public DateTime? EndDate { get; set; }
        public string BankNumber { get; set; }
        public string BankName { get; set; }
        public string BankBranch { get; set; }
        public string WorkStatus { get; set; }
        public string WorkTime { get; set; }
        public decimal MonthlySale { get; set; }
        public string MonthlySaleString { get { return MonthlySale.GetCurrencyString(); } }
        public string Address { get; set; }
        public decimal CurrentSale { get; set; }
        public string CurrentSaleString { get { return CurrentSale.GetCurrencyString(); } }
        public decimal BaseSalary { get; set; }
        public string BaseSalaryString { get { return BaseSalary.GetCurrencyString(); } }
        public decimal AdditionalSalary { get; set; }
        public string AdditionalSalaryString { get { return AdditionalSalary.GetCurrencyString(); } }
        public int WorkDays { get; set; }
        public int OffDays { get; set; }
        public int? LoginID { get; set; }
        public string Username { get; set; }
        public string Summary { get; set; }
    }
}