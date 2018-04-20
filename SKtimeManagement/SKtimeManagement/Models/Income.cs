using Dapper;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace SKtimeManagement
{
    public class InOutType
    {
        public const string Cash = "Tiền mặt";
        public const string Transfer = "Chuyển khoản";
        public const string Card = "Thẻ";
        public static string[] List = new string[] { InOutType.Card, InOutType.Cash, InOutType.Transfer };
    }
    public class IncomeFilter
    {
        public int? EmployeeID { get; set; }
        public int? WarehouseID { get; set; }
        public string Code { get; set; }
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = Constants.DateTimeString)]
        public DateTime? From { get; set; }
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = Constants.DateTimeString)]
        public DateTime? To { get; set; }
    }
    public class IncomeList
    {
        public IncomeList(string message = "", IncomeFilter filter = null)
        {
            Data = new List<IncomeInfo>();
            Filter = filter != null ? filter : new IncomeFilter();
            Message = message;
        }
        public List<IncomeInfo> Data { get; set; }
        public IncomeFilter Filter { get; set; }
        public string Message { get; set; }
    }
    public class IncomeInfo : BaseModel
    {
        public IncomeInfo() : base() { }
        public IncomeInfo(int bussinessID, int employeeID) : base()
        {
            BussinessID = bussinessID;
            EmployeeID = employeeID;
        }
        public int ID { get; set; }
        public int WarehouseID { get; set; }
        public string WarehouseName { get; set; }
        public int EmployeeID { get; set; }
        public string EmployeeName { get; set; }
        public int BussinessID { get; set; }
        public DateTime SubmitDate { get; set; }
        public string SubmitDateString { get { return SubmitDate.ToString(Constants.DateTimeString); } }
        public string Code { get; set; }
        public decimal Amount { get; set; }
        public string AmountString { get { return Amount.GetCurrencyString(); } }
        public string Type { get; set; }
        public string Reason { get; set; }
        public string Note { get; set; }
        public bool Save(ModelStateDictionary modelState, int userID, int employeeID, int bussinessID)
        {
            QueryOutput queryResult;
            if (!Validate(modelState))
            {
                return Result = false;
            }
            var query = "";
            var action = "";
            var id = ID.ToString();
            if (ID > 0)
            {
                query = String.Format(@"update Income 
                                                set Amount = {0}, Type = N'{1}', Reason = N'{2}', Note = N'{3}', WarehouseID = {5}
                                                where ID = {4}",
                                            new object[] {
                                                    Amount, Type, Reason, Note, ID, WarehouseID
                });
                action = DbAction.Income.Modify;
            }
            else
            {
                query = String.Format(@"declare @income table (ID int)
                                        insert Income(EmployeeID, BussinessID, SubmitDate, Amount, Type, Reason, Note, Code, Status, WarehouseID) 
                                        output inserted.ID into @income
                                        values ({0}, {1}, '{2}', {3}, N'{4}', N'{5}', N'{6}', '{7}', 'active', {8})",
                                            new object[] {
                                                    employeeID, bussinessID, DateTime.Now.ToString(Constants.DatabaseDatetimeString), Amount, Type, Reason, Note, NewUniqueCode(userID, employeeID, bussinessID, "Income"), WarehouseID
                });
                id = "(select top 1 ID from @income)";
                action = DbAction.Income.Create;
            }
            Result = Execute(new DbQuery(userID, employeeID, action, query, true, id), out queryResult);
            if (Result)
                Messages = new List<string>() { "Lưu thông tin thành công" };
            return Result;
        }
        public static bool Remove(int userID, int employeeID, int incomeID)
        {
            QueryOutput queryResult;
            return Execute(new DbQuery(userID, employeeID, DbAction.Income.Remove, String.Format("update Income set Status = 'removed' where ID = {0}", incomeID), true, incomeID), out queryResult);
        }
        public static string FindQuery(int userID, int bussinessID, IncomeFilter filter = null, int? max = 100)
        {
            var conditions = new List<string>();
            if (filter != null)
            {
                if (!String.IsNullOrEmpty(filter.Code))
                    conditions.Add(String.Format("and i.Code like N'%{0}%'", filter.Code));
                if (filter.EmployeeID.HasValue)
                    conditions.Add(String.Format("and i.EmployeeID = {0}", filter.EmployeeID.Value));
                if (filter.WarehouseID.HasValue)
                    conditions.Add(String.Format("and i.WarehouseID = {0}", filter.WarehouseID.Value));
                if (filter.From.HasValue)
                    conditions.Add(String.Format("and i.SubmitDate >= '{0}'", filter.From.Value.ToString(Constants.DatabaseDatetimeString)));
                if (filter.To.HasValue)
                    conditions.Add(String.Format("and i.SubmitDate <= '{0}'", filter.To.Value.ToString(Constants.DatabaseDatetimeString)));

            }
            return String.Format(
                    @"select {3} i.*, e.Name as [EmployeeName], w.Name as [WarehouseName]
                    from Income i join Employee e on i.EmployeeID = e.ID  join Warehouse w on i.WarehouseID = w.ID and w.Status = 'active' and ((select Username from Login where ID = {2}) = 'admin' or w.ID in (select WarehouseID from LoginWarehouse where LoginID = {2}))
                    where i.Status = 'active' and i.BussinessID = {0} {1} order by i.ID desc",
                bussinessID, String.Join(" ", conditions), userID,
                max.HasValue ? String.Format("top {0}", max.Value) : "");
        }
        public static IncomeList Find(int userID, int employeeID, int bussinessID, string message = "", IncomeFilter filter = null, bool log = false, int? max = 100)
        {
            QueryOutput queryResult;
            var result = new IncomeList(message, filter);
            result.Data = Query<IncomeInfo>(new DbQuery(userID, employeeID, DbAction.Income.View, FindQuery(userID, bussinessID, filter, max), log), out queryResult);
            return result;
        }
        public static IncomeList Find(int userID, int bussinessID, IncomeFilter filter = null, int? max = 100)
        {
            var result = new IncomeList("", filter);
            using (var con = Repo.DB.SKtimeManagement)
            {
                result.Data = con.Query<IncomeInfo>(FindQuery(userID, bussinessID, filter, max)).ToList();
            }
            return result;
        }
        public static IncomeInfo Get(int userID, int employeeID, int incomeID, bool log = false)
        {
            QueryOutput queryResult;
            return Query<IncomeInfo>(new DbQuery(userID, employeeID, DbAction.Income.View, 
                String.Format(
                    @"select top 100 i.*, e.Name as [EmployeeName], w.Name as [WarehouseName]
                    from Income i join Employee e on i.EmployeeID = e.ID join Warehouse w on i.WarehouseID = w.ID and w.Status = 'active'
                    where i.Status = 'active' and i.ID = {0} order by i.ID desc", 
                incomeID), log), out queryResult).FirstOrDefault();
        }
    }
}