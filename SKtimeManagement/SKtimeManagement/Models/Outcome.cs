using Dapper;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace SKtimeManagement
{
    public class OutcomeFilter
    {
        public int? EmployeeID { get; set; }
        public int? WarehouseID { get; set; }
        public string Code { get; set; }
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = Constants.DateTimeString)]
        public DateTime? From { get; set; }
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = Constants.DateTimeString)]
        public DateTime? To { get; set; }
    }
    public class OutcomeList
    {
        public OutcomeList(string message = "", OutcomeFilter filter = null)
        {
            Data = new List<OutcomeInfo>();
            Filter = filter != null ? filter : new OutcomeFilter();
            Message = message;
        }
        public List<OutcomeInfo> Data { get; set; }
        public OutcomeFilter Filter { get; set; }
        public string Message { get; set; }
    }
    public class OutcomeInfo : BaseModel
    {
        public OutcomeInfo() : base() { }
        public OutcomeInfo(int bussinessID, int employeeID) : base()
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
            var now = DateTime.Now;
            var action = "";
            var id = ID.ToString();
            if (ID > 0)
            {
                query = String.Format(@"update Outcome 
                                                set Amount = {0}, Type = N'{1}', Reason = N'{2}', Note = N'{3}', WarehouseID = {5}
                                                where ID = {4}",
                                            new object[] {
                                                    Amount, Type, Reason, Note, ID, WarehouseID
                });
                action = DbAction.Outcome.Modify;
            }
            else
            {
                query = String.Format(@"declare @outcome table (ID int)
                                        insert Outcome(EmployeeID, BussinessID, SubmitDate, Amount, Type, Reason, Note, Code, Status, WarehouseID)
                                        output inserted.ID into @outcome
                                        values ({0}, {1}, '{2}', {3}, N'{4}', N'{5}', N'{6}', '{7}', 'active', {8})",
                                            new object[] {
                                                    employeeID, bussinessID, now.ToString(Constants.DatabaseDatetimeString), Amount, Type, Reason, Note, NewUniqueCode(userID, employeeID, bussinessID, "Outcome"), WarehouseID
                });
                action = DbAction.Outcome.Create;
                id = "(select top 1 ID from @outcome)";
            }
            Result = Execute(new DbQuery(userID, employeeID, action, query, true, id), out queryResult);
            if (Result)
                Messages = new List<string>() { "Lưu thông tin thành công" };
            return Result;
        }
        public static bool Remove(int userID, int employeeID, int OutcomeID)
        {
            QueryOutput queryResult;
            return Execute(new DbQuery(userID, employeeID, DbAction.Outcome.Remove, String.Format("update Outcome set Status = 'removed' where ID = {0}", OutcomeID), true, OutcomeID), out queryResult);
        }
        public static string FindQuery(int userID, int bussinessID, OutcomeFilter filter = null, int? max = 100)
        {
            var conditions = new List<string>();
            if (filter != null)
            {
                if (!String.IsNullOrEmpty(filter.Code))
                    conditions.Add(String.Format("and o.Code like N'%{0}%'", filter.Code));
                if (filter.EmployeeID.HasValue)
                    conditions.Add(String.Format("and o.EmployeeID = {0}", filter.EmployeeID.Value));
                if (filter.WarehouseID.HasValue)
                    conditions.Add(String.Format("and o.WarehouseID = {0}", filter.WarehouseID.Value));
                if (filter.From.HasValue)
                    conditions.Add(String.Format("and o.SubmitDate >= '{0}'", filter.From.Value.ToString(Constants.DatabaseDatetimeString)));
                if (filter.To.HasValue)
                    conditions.Add(String.Format("and o.SubmitDate <= '{0}'", filter.To.Value.ToString(Constants.DatabaseDatetimeString)));
            }
            return String.Format(
                    @"select {3} o.*, e.Name as [EmployeeName], w.Name as [WarehouseName]
                    from Outcome o join Employee e on o.EmployeeID = e.ID join Warehouse w on o.WarehouseID = w.ID and w.Status = 'active' and ((select Username from Login where ID = {2}) = 'admin' or w.ID in (select WarehouseID from LoginWarehouse where LoginID = {2}))
                    where o.Status = 'active' and o.BussinessID = {0} {1} order by o.ID desc",
                bussinessID, String.Join(" ", conditions), userID,
                max.HasValue ? String.Format("top {0}", max.Value) : "");
        }
        public static OutcomeList Find(int userID, int employeeID, int bussinessID, string message = "", OutcomeFilter filter = null, bool log = false, int? max = 100)
        {
            QueryOutput queryResult;
            var result = new OutcomeList(message, filter);
            result.Data = Query<OutcomeInfo>(new DbQuery(userID, employeeID, DbAction.Outcome.View, FindQuery(userID, bussinessID, filter, max), log), out queryResult);
            return result;
        }
        public static OutcomeList Find(int userID, int bussinessID, OutcomeFilter filter = null, int? max = 100)
        {
            var result = new OutcomeList("", filter);
            using (var con = Repo.DB.SKtimeManagement)
            {
                result.Data = con.Query<OutcomeInfo>(FindQuery(userID, bussinessID, filter, max)).ToList();
            }
            return result;
        }
        public static List<dynamic> KeyList(int userID, int employeeID, int bussinessID, int productID)
        {
            QueryOutput queryResult;
            return Query<dynamic>(new DbQuery(userID, employeeID, DbAction.Outcome.View, String.Format("select top 100 w.ID, w.Name, q.ID as [Tagged] from Outcome w left join ProductQuantity q on w.ID = q.OutcomeID and q.ProductID = {1} where w.Status = 'active' and w.BussinessID = {0} order by w.Name", bussinessID, productID)), out queryResult);
        }
        public static OutcomeInfo Get(int userID, int employeeID, int OutcomeID, bool log = false)
        {
            QueryOutput queryResult;
            return Query<OutcomeInfo>(new DbQuery(userID, employeeID, DbAction.Outcome.View, 
                String.Format(
                    @"select top 100 o.*, e.Name as [EmployeeName], w.Name as [WarehouseName]
                    from Outcome o join Employee e on o.EmployeeID = e.ID join Warehouse w on o.WarehouseID = w.ID and w.Status = 'active'
                    where o.Status = 'active' and o.ID = {0} order by o.ID desc", 
                OutcomeID), log), out queryResult).FirstOrDefault();
        }
    }
}