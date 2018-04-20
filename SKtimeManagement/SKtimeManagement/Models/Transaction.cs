using Dapper;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace SKtimeManagement
{
    public enum TransactionClass
    {
        Order,
        Repair,
        Warranty
    }
    public class TransactionList
    {
        public TransactionList(string employeeName, int recordID, List<Transaction> data = null)
        {
            EmployeeName = employeeName;
            RecordID = recordID;
            Data = data == null ? new List<Transaction>() : data;
        }
        public List<Transaction> Data { get; set; }
        public int RecordID { get; set; }
        public string EmployeeName { get; set; }
        public bool Editable { get; set; }
    }
    public class Transaction : BaseModel
    {
        public Transaction()
        {
            SubmitDate = DateTime.Now;
        }
        public Transaction(TransactionClass cls, int employeeID, int recordID, string method, decimal amount, DateTime? transferDate = null)
        {
            EmployeeID = employeeID;
            switch (cls)
            {
                case TransactionClass.Order: OrderID = recordID; break;
                case TransactionClass.Repair: RepairID = recordID; break;
                case TransactionClass.Warranty: WarrantyID = recordID; break;
                default: break;
            }
            Method = method;
            Amount = amount;
            TransferDate = transferDate.HasValue ? transferDate.Value : DateTime.Now;
            SubmitDate = DateTime.Now;
        }
        public int ID { get; set; }
        public int EmployeeID { get; set; }
        public string EmployeeName { get; set; }
        public int? OrderID { get; set; }
        public int? RepairID { get; set; }
        public int? WarrantyID { get; set; }
        public int RecordID
        {
            get
            {
                if (OrderID.HasValue)
                    return OrderID.Value;
                else if (RepairID.HasValue)
                    return RepairID.Value;
                else if (WarrantyID.HasValue)
                    return WarrantyID.Value;
                else
                    return 0;
            }
        }
        public string Method { get; set; }
        public decimal Amount { get; set; }
        public DateTime SubmitDate { get; set; }
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = Constants.DateTimeString)]
        public DateTime TransferDate { get; set; }
        private static string GetQuery(TransactionClass cls, object id)
        {
            var column = "";
            switch (cls)
            {
                case TransactionClass.Order: column = "OrderID"; break;
                case TransactionClass.Repair: column = "RepairID"; break;
                case TransactionClass.Warranty: column = "WarrantyID"; break;
                default: break;
            }
            var query = String.Format(" select t.*, e.Name as [EmployeeName] from Transactions t join Employee e on t.EmployeeID = e.ID where t.{0} = {1} order by t.ID desc", column, id);
            return query;
        }
        public static List<Transaction> Find(int loginID, TransactionClass[] lst, DateTime? from = null, DateTime? to = null)
        {
            var query = "declare @result table (ID int, EmployeeID int, OrderID int, RepairID int, WarrantyID int, Method nvarchar(50), Amount decimal(18, 0), SubmitDate datetime, TransferDate datetime, EmployeeName nvarchar(100))";
            foreach (var cls in lst)
            {
                var column = "";
                var table = "";
                var conditions = new List<string>();
                switch (cls)
                {
                    case TransactionClass.Order: column = "OrderID"; table = "[Order]"; conditions.Add(String.Format("and r.Removed = 0")); break;
                    case TransactionClass.Repair: column = "RepairID"; table = "Repair"; conditions.Add(String.Format("and r.Status = 'active'")); break;
                    case TransactionClass.Warranty: column = "WarrantyID"; table = "Warranty"; conditions.Add(String.Format("and r.Status = 'active'")); break;
                    default: break;
                }
                if (from.HasValue)
                    conditions.Add(String.Format(" and r.SubmitDate >= {0} ", from.DbValue()));
                if (to.HasValue)
                    conditions.Add(String.Format(" and r.SubmitDate <= {0} ", to.DbValue()));
                query += String.Format(
                    @" insert into @result
                    select t.*, e.Name as [EmployeeName] 
                    from Transactions t 
                        join Employee e on t.EmployeeID = e.ID 
                        join {2} r on r.ID = t.{0} and ((select Username from Login where ID = {3}) = 'admin' or r.WarehouseID in (select WarehouseID from LoginWarehouse where LoginID = {3}))
                    where t.{0} is not null {1} order by t.ID desc",
                    column, String.Join("", conditions), table, loginID);
            }
            query += String.Format(" select * from @result");
            using (var con = Repo.DB.SKtimeManagement)
            {
                return con.Query<Transaction>(query).ToList();
            }
        }
        public static TransactionList Get(int userID, int employeeID, string employeeName, TransactionClass cls, int recordID, string action)
        {
            QueryOutput queryOutput;
            return new TransactionList(employeeName, recordID, Query<Transaction>(new DbQuery(userID, employeeID, action, GetQuery(cls, recordID)), out queryOutput));
        }
        public string AddTransactionQuery(int employeeID, TransactionClass cls, ref string recordID)
        {
            var column = "";
            var updateRecord = "";
            switch (cls)
            {
                case TransactionClass.Order:
                    column = "OrderID"; recordID = String.IsNullOrEmpty(recordID) ? OrderID.DbValue() : recordID;
                    updateRecord = String.Format("update [Order] set Paid = (select isnull(sum(Amount), 0) from Transactions where OrderID = {0}) where ID = {0}", recordID);
                    break;
                case TransactionClass.Repair: column = "RepairID"; recordID = String.IsNullOrEmpty(recordID) ? RepairID.DbValue() : recordID; break;
                case TransactionClass.Warranty: column = "WarrantyID"; recordID = String.IsNullOrEmpty(recordID) ? WarrantyID.DbValue() : recordID; break;
                default: break;
            }
            var result = String.Format(
                @" insert into Transactions(EmployeeID, {0}, Method, Amount, SubmitDate, TransferDate)
                values ({1}, {2}, N'{3}', {4}, {5}, {6}) {7}",
                column, employeeID, recordID, Method, Amount, SubmitDate.DbValue(), TransferDate.DbValue(), updateRecord);
            return result;
        }
        public TransactionList Save(int userID, int employeeID, string employeeName, TransactionClass cls, string action)
        {
            QueryOutput queryOutput;
            string recordID = null;
            var query = String.Format("{0}{1}", AddTransactionQuery(employeeID, cls, ref recordID), GetQuery(cls, recordID));
            return new TransactionList(employeeName, RecordID, Query<Transaction>(new DbQuery(userID, employeeID, action, query, true, recordID), out queryOutput));
        }
        public static TransactionList Remove(int userID, int employeeID, string employeeName, int transactionID, int recordID, TransactionClass cls, string action)
        {
            QueryOutput queryOutput;
            var updateRecord = "";
            if (cls == TransactionClass.Order)
            {
                updateRecord = String.Format("update [Order] set Paid = (select isnull(sum(Amount), 0) from Transactions where OrderID = {0}) where ID = {0}", recordID);
            }
            var query = String.Format(
                @"delete Transactions where ID = {0} {1} {2}", 
                transactionID, updateRecord, GetQuery(cls, recordID));
            return new TransactionList(employeeName, recordID, Query<Transaction>(new DbQuery(userID, employeeID, action, query, true, recordID), out queryOutput));
        }
    }
}