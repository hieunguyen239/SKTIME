using Dapper;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace SKtimeManagement
{
    public class RepairList
    {
        public RepairList(RepairFilter filter = null)
        {
            Filter = filter;
            if (Filter == null)
                Filter = new RepairFilter();
        }
        public RepairFilter Filter { get; set; }
        public List<Repair> Data { get; set; }
        public RepairModel Current { get; set; }
    }
    public class RepairFilter
    {
        public RepairFilter() { }
        public int? WarehouseID { get; set; }
        public string Code { get; set; }
        public string ClientName { get; set; }
        public string ClientPhone { get; set; }
        public string ClientEmail { get; set; }
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = Constants.DateString)]
        public DateTime? From { get; set; }
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = Constants.DateString)]
        public DateTime? To { get; set; }
    }
    public class RepairModel
    {
        public RepairModel(List<ProductInfo> products = null, ImexFilter filter = null, Repair model = null)
        {
            Filter = filter;
            if (Filter == null)
                Filter = new ImexFilter();
            Record = model;
            if (Record == null)
                Record = new Repair();
            Products = products;
            if (Products == null)
                Products = new List<ProductInfo>();
        }
        public ImexFilter Filter { get; set; }
        public Repair Record { get; set; }
        public List<ProductInfo> Products { get; set; }
        public ProductInfo SelectedProduct { get; set; }
        public bool ViewInternalNote { get; set; }
        public bool Editable { get; set; }
        public bool Edit { get; set; }
    }
    public class RepairNote
    {
        public int ID { get; set; }
        public int RepairID { get; set; }
        public int EmployeeID { get; set; }
        public string EmployeeName { get; set; }
        public DateTime SubmitDate { get; set; }
        public string Message { get; set; }
        public string Title { get { return String.Format("{0} ({1})", EmployeeName, SubmitDate.ToString(Constants.DateTimeString)); } }
    }
    public class RepairNoteList
    {
        public RepairNoteList()
        {
            ID = Guid.NewGuid().ToString();
        }
        public string ID { get; set; }
        public string RemoveUrl { get; set; }
        public List<RepairNote> Data { get; set; }
    }
    public class Repair : BaseModel
    {
        public Repair()
        {
            FinishDate = DateTime.Now;
        }
        public int ID { get; set; }
        public int BussinessID { get; set; }
        public int EmployeeID { get; set; }
        public int WarehouseID { get; set; }
        public int ReceiveWarehouseID { get; set; }
        public int? ProductID { get; set; }
        public string ProductName { get; set; }
        public string EmployeeName { get; set; }
        public string WarehouseName { get; set; }
        public string ReceiveWarehouseName { get; set; }
        public int? ClientID { get; set; }
        public string ClientCode { get; set; }
        public string ClientName { get; set; }
        public string ClientPhone { get; set; }
        public string ClientAddress { get; set; }
        public string Code { get; set; }
        public string Service { get; set; }
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = Constants.DateTimeString)]
        public DateTime SubmitDate { get; set; }
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = Constants.DateString)]
        public DateTime? TransferDate { get; set; }
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = Constants.DateString)]
        public DateTime? ReceivedDate { get; set; }
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = Constants.DateString)]
        public DateTime? ProcessedDate { get; set; }
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = Constants.DateString)]
        public DateTime? ReturnedDate { get; set; }
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = Constants.DateString)]
        public DateTime FinishDate { get; set; }
        public string ProductState { get; set; }
        public decimal Fee { get; set; }
        public string FeeString { get { return Fee.GetCurrencyString(); } }
        public decimal Discount { get; set; }
        public string DiscountString { get { return Discount.GetCurrencyString(); } }
        public decimal Paid { get; set; }
        public string PaidString { get { return Paid.GetCurrencyString(); } }
        public string Other { get; set; }
        public string Note { get; set; }
        public List<RepairNote> MechanicNotes { get; set; }
        public List<RepairNote> InternalNotes { get; set; }
        public List<Repair> History { get; set; }
        public int HistoryCount { get; set; }
        public List<Transaction> Transactions { get; set; }
        public string Status
        {
            get
            {
                if (ReturnedDate.HasValue && FinishDate <= DateTime.Now)
                    return WarrantyStatus.Finish;
                if (ReturnedDate.HasValue)
                    return WarrantyStatus.Returned;
                if (ProcessedDate.HasValue)
                    return WarrantyStatus.Processed;
                if (ReceivedDate.HasValue)
                    return WarrantyStatus.Processing;
                if (TransferDate.HasValue)
                    return WarrantyStatus.Transfer;
                return WarrantyStatus.New;
            }
        }
        public string Color
        {
            get
            {
                if (Status != WarrantyStatus.Finish && FinishDate <= DateTime.Now.Date.AddDays(2))
                    return "red";
                if (Status == WarrantyStatus.Processing)
                    return "green";
                if (Status == WarrantyStatus.Returned)
                    return "orange";
                if (Status == WarrantyStatus.Transfer)
                    return "blue";
                if (Status == WarrantyStatus.Processed)
                    return "yellow";
                return "";
            }
        }
        public Repair Save(ModelStateDictionary modelState, int userID, int employeeID, int bussinessID, string employeeName = null)
        {
            QueryOutput queryResult;
            if (!Validate(modelState))
            {
                Result = false;
                return null;
            }
            var query = "";
            var action = "";
            var id = ID.ToString();
            if (ID > 0)
            {
                query = String.Format(
                    @"update Repair 
                    set TransferDate = {1}, ReceivedDate = {2}, ProcessedDate = {3}, ReturnedDate = {4}, FinishDate = {5}, ProductState = N'{6}', 
                        Fee = {7}, Discount = {8}, Other = N'{9}', Note = N'{10}', ProductName = N'{11}', Service = N'{12}'
                    where ID = {0}",
                    new object[] {
                        ID, TransferDate.DbValue(), ReceivedDate.DbValue(), ProcessedDate.DbValue(), ReturnedDate.DbValue(), FinishDate.DbValue(),
                        ProductState, Fee, Discount, Other, Note, ProductName, Service
                });
                action = DbAction.Repair.Modify;
            }
            else
            {
                Code = NewUniqueCode(userID, employeeID, bussinessID, "Repair");
                EmployeeID = employeeID;
                BussinessID = bussinessID;
                EmployeeName = employeeName;
                SubmitDate = DateTime.Now;
                var clientID = "@ClientID";
                query = String.Format("declare {0} int = {1}", clientID, ClientID.DbValue());
                if ((!ClientID.HasValue || ClientID <= 0) && !String.IsNullOrEmpty(ClientName))
                    query += String.Format(
                        @" declare @client table (ID int)
                        insert Client(BussinessID, Code, Name, Phone, Address, Point, Status)
                        output inserted.ID into @client
                        values ({0}, '{1}', N'{2}', '{3}', N'{4}', 0, 'active')
                        set {5} = (select top 1 ID from @client)",
                    bussinessID, String.IsNullOrEmpty(ClientCode) ? NewUniqueCode(userID, employeeID, bussinessID, "Client", 3, null) : ClientCode, ClientName, ClientPhone, ClientAddress, clientID);
                query += String.Format(
                    @" declare @output table (ID int)
                    insert Repair(BussinessID, EmployeeID, WarehouseID, ProductID, Code, SubmitDate, TransferDate, ReceivedDate, 
                        ProcessedDate, ReturnedDate, FinishDate, ProductState, Fee, Discount, Other, Note, Status, ClientID, ProductName, ReceiveWarehouseID, Service) 
                    output inserted.ID into @output
                    values ({0}, {1}, {2}, {3}, N'{4}', {5}, {6}, {7}, {8}, {9}, {10}, N'{11}', {12}, {13}, N'{14}', N'{15}', 'active', {16}, N'{17}', {18}, N'{19}')",
                    new object[] {
                        BussinessID, EmployeeID, WarehouseID, ProductID.DbValue(), Code, SubmitDate.DbValue(), TransferDate.DbValue(), ReceivedDate.DbValue(),
                        ProcessedDate.DbValue(), ReturnedDate.DbValue(), FinishDate.DbValue(), ProductState, Fee, Discount, Other, Note, clientID, ProductName, ReceiveWarehouseID, Service
                });
                id = "(select top 1 ID from @output)";
                if (Transactions != null)
                {
                    foreach (var tran in Transactions)
                    {
                        query += tran.AddTransactionQuery(employeeID, TransactionClass.Repair, ref id);
                    }
                }
                action = DbAction.Repair.Create;
            }
            query += String.Format(
                @" select top 1 w.ID, w.BussinessID, w.EmployeeID, w.WarehouseID, w.ProductID, w.ClientID, w.Code, w.Service, 
                    case when w.ProductID is null then w.ProductName else p.Name end as [ProductName], 
	                w.SubmitDate, w.TransferDate, w.ReceivedDate, w.ProcessedDate, w.ReturnedDate, w.FinishDate, w.ProductState, 
	                w.Fee, w.Discount, w.Other, w.Note, wh.Name as [WarehouseName], rwh.Name as [ReceiveWarehouseName], e.Name as [EmployeeName], 
                    c.Name as [ClientName], c.Code as [ClientCode], c.Address as [ClientAddress], c.Phone as [ClientPhone], isnull(sum(t.Amount), 0) as [Paid]
                from Repair w 
                    join Warehouse wh on w.WarehouseID = wh.ID and w.Status = 'active' and ((select Username from Login where ID = {1}) = 'admin' or wh.ID in (select WarehouseID from LoginWarehouse where LoginID = {1}))
                    join Warehouse rwh on w.ReceiveWarehouseID = rwh.ID
                    join Employee e on w.EmployeeID = e.ID
                    left join Product p on p.ID = w.ProductID
                    left join Client c on w.ClientID = c.ID
                    left join Transactions t on w.ID = t.RepairID
                where w.ID = {0} and w.Status = 'active'
                group by w.ID, w.BussinessID, w.EmployeeID, w.WarehouseID, w.ProductID, w.ProductName, w.ClientID, w.Code, w.Service, 
                    w.SubmitDate, w.TransferDate, w.ReceivedDate, w.ProcessedDate, w.ReturnedDate, w.FinishDate, w.ProductState, 
	                w.Fee, w.Discount, w.Other, w.Note, wh.Name, rwh.Name, p.Name, e.Name, c.Name, c.Code, c.Address, c.Phone", id, userID);
            var record = Query<Repair>(new DbQuery(userID, employeeID, action, query, true, id), out queryResult).FirstOrDefault();
            if (Result = (queryResult == QueryOutput.Success))
                Messages = new List<string>() { "Lưu thông tin thành công" };
            return record;
        }
        public bool SaveTransferDate(int userID, int employeeID)
        {
            QueryOutput queryResult;
            var query = String.Format("update Repair set TransferDate = {0} where ID = {1}", TransferDate.DbValue(), ID);
            Execute(new DbQuery(userID, employeeID, DbAction.Repair.Modify, query, true, ID), out queryResult);
            return queryResult == QueryOutput.Success;
        }
        public bool SaveReceivedDate(int userID, int employeeID)
        {
            QueryOutput queryResult;
            var query = String.Format("update Repair set ReceivedDate = {0} where ID = {1}", ReceivedDate.DbValue(), ID);
            Execute(new DbQuery(userID, employeeID, DbAction.Repair.Modify, query, true, ID), out queryResult);
            return queryResult == QueryOutput.Success;
        }
        public bool SaveProcessedDate(int userID, int employeeID)
        {
            QueryOutput queryResult;
            var query = String.Format("update Repair set ProcessedDate = {0} where ID = {1}", ProcessedDate.DbValue(), ID);
            Execute(new DbQuery(userID, employeeID, DbAction.Repair.Modify, query, true, ID), out queryResult);
            return queryResult == QueryOutput.Success;
        }
        public bool SaveReturnedDate(int userID, int employeeID)
        {
            QueryOutput queryResult;
            var query = String.Format("update Repair set ReturnedDate = {0} where ID = {1}", ReturnedDate.DbValue(), ID);
            Execute(new DbQuery(userID, employeeID, DbAction.Repair.Modify, query, true, ID), out queryResult);
            return queryResult == QueryOutput.Success;
        }
        public bool SaveFinishDate(int userID, int employeeID)
        {
            QueryOutput queryResult;
            var query = String.Format("update Repair set FinishDate = {0} where ID = {1}", FinishDate.DbValue(), ID);
            Execute(new DbQuery(userID, employeeID, DbAction.Repair.Modify, query, true, ID), out queryResult);
            return queryResult == QueryOutput.Success;
        }
        public static bool Remove(int userID, int employeeID, int id)
        {
            QueryOutput queryResult;
            Execute(new DbQuery(userID, employeeID, DbAction.Repair.Remove, String.Format("update Repair set Status = 'removed' where ID = {0}", id), true, id), out queryResult);
            return queryResult == QueryOutput.Success;
        }
        public static RepairList Find(int userID, int employeeID, int bussinessID, RepairFilter filter = null, bool log = false, int? top = 100)
        {
            QueryOutput queryResult;
            var conditions = new List<string>();
            if (filter != null)
            {
                if (filter.WarehouseID.HasValue)
                    conditions.Add(String.Format("and w.WarehouseID = {0}", filter.WarehouseID.DbValue()));
                if (!String.IsNullOrEmpty(filter.Code))
                    conditions.Add(String.Format("and w.Code like N'%{0}%'", filter.Code));
                if (!String.IsNullOrEmpty(filter.ClientName))
                    conditions.Add(String.Format("and c.Name like N'%{0}%'", filter.ClientName));
                if (!String.IsNullOrEmpty(filter.ClientPhone))
                    conditions.Add(String.Format("and c.Phone like N'%{0}%'", filter.ClientPhone));
                if (!String.IsNullOrEmpty(filter.ClientEmail))
                    conditions.Add(String.Format("and c.Email like N'%{0}%'", filter.ClientEmail));
                if (filter.From.HasValue)
                    conditions.Add(String.Format("and w.SubmitDate >= '{0}'", filter.From.Value.ToString(Constants.DatabaseDatetimeString)));
                if (filter.To.HasValue)
                    conditions.Add(String.Format("and w.SubmitDate <= '{0}'", filter.To.Value.ToString(Constants.DatabaseDatetimeString)));
            }
            var query = String.Format(
                @"select {1} w.ID, w.BussinessID, w.EmployeeID, w.WarehouseID, w.ProductID, w.ClientID, w.Code, w.Service, 
                    case when w.ProductID is null then w.ProductName else p.Name end as [ProductName], 
	                w.SubmitDate, w.TransferDate, w.ReceivedDate, w.ProcessedDate, w.ReturnedDate, w.FinishDate, w.ProductState, 
	                w.Fee, w.Discount, w.Other, w.Note, wh.Name as [WarehouseName], rwh.Name as [ReceiveWarehouseName], e.Name as [EmployeeName],
                    c.Name as [ClientName], c.Code as [ClientCode], c.Address as [ClientAddress], c.Phone as [ClientPhone], isnull(sum(t.Amount), 0) as [Paid]
                from Repair w 
                    join Warehouse wh on w.WarehouseID = wh.ID and w.Status = 'active' and ((select Username from Login where ID = {3}) = 'admin' or wh.ID in (select WarehouseID from LoginWarehouse where LoginID = {3}))
                    join Warehouse rwh on w.ReceiveWarehouseID = rwh.ID
                    join Employee e on w.EmployeeID = e.ID
                    left join Product p on p.ID = w.ProductID
                    left join Client c on w.ClientID = c.ID
                    left join Transactions t on w.ID = t.RepairID
                where w.BussinessID = {0} and w.Status = 'active' {2}
                group by w.ID, w.BussinessID, w.EmployeeID, w.WarehouseID, w.ProductID, w.ProductName, w.ClientID, w.Code, w.Service, 
                    w.SubmitDate, w.TransferDate, w.ReceivedDate, w.ProcessedDate, w.ReturnedDate, w.FinishDate, w.ProductState, 
	                w.Fee, w.Discount, w.Other, w.Note, wh.Name, rwh.Name, p.Name, e.Name, c.Name, c.Code, c.Address, c.Phone
                order by w.ID desc", bussinessID, top.HasValue ? String.Format("top {0}", top.Value) : "", String.Join(" ", conditions), userID);
            var result = new RepairList(filter);
            result.Data = Query<Repair>(new DbQuery(userID, employeeID, DbAction.Repair.View, query, log), out queryResult).ToList();
            return result;
        }
        public static RepairModel Get(int userID, int employeeID, int id)
        {
            QueryOutput queryResult;
            var query = String.Format(
                @"select top 1 w.ID, w.BussinessID, w.EmployeeID, w.WarehouseID, w.ProductID, w.ClientID, w.Code, w.Service, 
                    case when w.ProductID is null then w.ProductName else p.Name end as [ProductName], 
	                w.SubmitDate, w.TransferDate, w.ReceivedDate, w.ProcessedDate, w.ReturnedDate, w.FinishDate, w.ProductState, 
	                w.Fee, w.Discount, w.Other, w.Note, wh.Name as [WarehouseName], rwh.Name as [ReceiveWarehouseName], e.Name as [EmployeeName],
                    c.Name as [ClientName], c.Code as [ClientCode], c.Address as [ClientAddress], c.Phone as [ClientPhone], isnull(sum(t.Amount), 0) as [Paid]
                from Repair w 
                    join Warehouse wh on w.WarehouseID = wh.ID and w.Status = 'active' and ((select Username from Login where ID = {1}) = 'admin' or wh.ID in (select WarehouseID from LoginWarehouse where LoginID = {1}))
                    join Warehouse rwh on w.ReceiveWarehouseID = rwh.ID
                    join Employee e on w.EmployeeID = e.ID
                    left join Product p on p.ID = w.ProductID
                    left join Client c on w.ClientID = c.ID
                    left join Transactions t on w.ID = t.RepairID
                where w.ID = {0} and w.Status = 'active'
                group by w.ID, w.BussinessID, w.EmployeeID, w.WarehouseID, w.ProductID, w.ProductName, w.ClientID, w.Code, w.Service, 
                    w.SubmitDate, w.TransferDate, w.ReceivedDate, w.ProcessedDate, w.ReturnedDate, w.FinishDate, w.ProductState, 
	                w.Fee, w.Discount, w.Other, w.Note, wh.Name, rwh.Name, p.Name, e.Name, c.Name, c.Code, c.Address, c.Phone", id, userID);
            var result = new RepairModel();
            var action = DbAction.Repair.View;
            result.Record = Query<Repair>(new DbQuery(userID, employeeID, action, query), out queryResult).FirstOrDefault();
            if (result.Record != null && result.Record.ProductID.HasValue)
            {
                result.SelectedProduct = ProductInfo.Get(userID, employeeID, result.Record.ProductID.Value, false, action);
            }
            return result;
        }
        public static List<Repair> GetHistory(int userID, int productID, int clientID)
        {
            using (var con = Repo.DB.SKtimeManagement)
            {
                var query = String.Format(
                    @"select w.ID, w.BussinessID, w.EmployeeID, w.WarehouseID, w.ProductID, w.ClientID, w.Code, w.Service, 
                    case when w.ProductID is null then w.ProductName else p.Name end as [ProductName], 
	                    w.SubmitDate, w.TransferDate, w.ReceivedDate, w.ProcessedDate, w.ReturnedDate, w.FinishDate, w.ProductState, 
	                    w.Fee, w.Discount, w.Other, w.Note, wh.Name as [WarehouseName], rwh.Name as [ReceiveWarehouseName], e.Name as [EmployeeName],
                        c.Name as [ClientName], c.Code as [ClientCode], c.Address as [ClientAddress], c.Phone as [ClientPhone], isnull(sum(t.Amount), 0) as [Paid]
                    from Repair w 
                        join Warehouse wh on w.WarehouseID = wh.ID and w.Status = 'active' and ((select Username from Login where ID = {2}) = 'admin' or wh.ID in (select WarehouseID from LoginWarehouse where LoginID = {2}))
                        join Warehouse rwh on w.ReceiveWarehouseID = rwh.ID
                        join Employee e on w.EmployeeID = e.ID
                        left join Product p on p.ID = w.ProductID
                        left join Client c on w.ClientID = c.ID
                        left join Transactions t on w.ID = t.RepairID
                    where w.Status = 'active' and w.ProductID = {0} and w.ClientID = {1}
                    group by w.ID, w.BussinessID, w.EmployeeID, w.WarehouseID, w.ProductID, w.ProductName, w.ClientID, w.Code, w.Service, 
                        w.SubmitDate, w.TransferDate, w.ReceivedDate, w.ProcessedDate, w.ReturnedDate, w.FinishDate, w.ProductState, 
	                    w.Fee, w.Discount, w.Other, w.Note, wh.Name, rwh.Name, p.Name, e.Name, c.Name, c.Code, c.Address, c.Phone", productID, clientID, userID);
                return con.Query<Repair>(query).ToList();
            }
        }
        private static string MechanicNoteQuery(object recordID)
        {
            return String.Format(
                @"select n.*, e.Name as [EmployeeName] 
                from RepairMechanicNote n join Employee e on n.EmployeeID = e.ID 
                where n.RepairID = {0}
                order by n.ID desc", recordID);
        }
        public static RepairNoteList GetMechanicNotes(int userID, int employeeID, int recordID)
        {
            QueryOutput queryOutput;
            var list = new RepairNoteList();
            list.Data = Query<RepairNote>(new DbQuery(userID, employeeID, DbAction.Repair.View, MechanicNoteQuery(recordID)), out queryOutput).ToList();
            return list;
        }
        public static RepairNoteList SaveMechanicNote(int userID, int employeeID, RepairNote note)
        {
            QueryOutput queryOutput;
            var query = String.Format(
                @"insert into RepairMechanicNote(RepairID, EmployeeID, SubmitDate, Message)
                values ({0}, {1}, {2}, N'{3}') {4}", note.RepairID, employeeID, DateTime.Now.DbValue(), note.Message, MechanicNoteQuery(note.RepairID));
            var list = new RepairNoteList();
            list.Data = Query<RepairNote>(new DbQuery(userID, employeeID, DbAction.Repair.View, query), out queryOutput).ToList();
            return list;
        }
        public static RepairNoteList RemoveMechanicNote(int userID, int employeeID, int noteID)
        {
            QueryOutput queryOutput;
            var query = String.Format(
                @"declare @recordID int = (select top 1 RepairID from RepairMechanicNote where ID = {0}) 
                delete RepairMechanicNote where ID = {0} {1}",
                noteID, MechanicNoteQuery("@recordID"));
            var list = new RepairNoteList();
            list.Data = Query<RepairNote>(new DbQuery(userID, employeeID, DbAction.Repair.View, query), out queryOutput).ToList();
            return list;
        }
        private static string InternalNoteQuery(object recordID)
        {
            return String.Format(
                @"select n.*, e.Name as [EmployeeName] 
                from RepairInternalNote n join Employee e on n.EmployeeID = e.ID 
                where n.RepairID = {0}
                order by n.ID desc", recordID);
        }
        public static RepairNoteList GetInternalNotes(int userID, int employeeID, int recordID)
        {
            QueryOutput queryOutput;
            var list = new RepairNoteList();
            list.Data = Query<RepairNote>(new DbQuery(userID, employeeID, DbAction.Repair.View, InternalNoteQuery(recordID)), out queryOutput).ToList();
            return list;
        }
        public static RepairNoteList SaveInternalNote(int userID, int employeeID, RepairNote note)
        {
            QueryOutput queryOutput;
            var query = String.Format(
                @"insert into RepairInternalNote(RepairID, EmployeeID, SubmitDate, Message)
                values ({0}, {1}, {2}, N'{3}') {4}", note.RepairID, employeeID, DateTime.Now.DbValue(), note.Message, InternalNoteQuery(note.RepairID));
            var list = new RepairNoteList();
            list.Data = Query<RepairNote>(new DbQuery(userID, employeeID, DbAction.Repair.View, query), out queryOutput).ToList();
            return list;
        }
        public static RepairNoteList RemoveInternalNote(int userID, int employeeID, int noteID)
        {
            QueryOutput queryOutput;
            var query = String.Format(
                @"declare @recordID int = (select top 1 RepairID from RepairInternalNote where ID = {0})
                delete RepairInternalNote where ID = {0} {1}",
                noteID, InternalNoteQuery("@recordID"));
            var list = new RepairNoteList();
            list.Data = Query<RepairNote>(new DbQuery(userID, employeeID, DbAction.Repair.View, query), out queryOutput).ToList();
            return list;
        }
        public static List<ExportProduct> GetProducts(int userID, int employeeID, int recordID)
        {
            QueryOutput queryOutput;
            return Query<ExportProduct>(new DbQuery(userID, employeeID, DbAction.Export.View,
                String.Format("select e.*, p.Code, p.Name as [ProductName], p.Unit from ExportProduct e join Product p on e.ProductID = p.ID where e.Returned = 0 and e.RepairID = {0}",
                recordID)), out queryOutput);
        }
        public static bool AddProduct(int userID, int employeeID, int bussinessID, int productID, int recordID)
        {
            QueryOutput queryOutput;
            var query = String.Format(
                @"declare @warehouseID int = (select WarehouseID from Repair where ID = {0})
                declare @exportID int = (select top 1 ExportID from ExportProduct where RepairID = {0})
                declare @IDs table (ID int)
                if (@exportID is null) begin
                    insert Export(EmployeeID, BussinessID, WarehouseID, Code, SubmitDate, Status, Removed)
                    output inserted.ID into @IDs
                    values ({3}, {4}, @warehouseID, '{5}', {6}, N'Đã xuất', 0)
                    set @exportID = (select top 1 ID from @IDs)
                end
                declare @productID int = (select ID from ExportProduct where ExportID = @exportID and ProductID = {1})
                if (@productID is null) begin
                    insert into ExportProduct(RepairID, ExportID, ProductID, Quantity, Price, Point, Returned)
                    values ({0}, @exportID, {1}, 1, 0, 0, 0)
                end else begin
                    update ExportProduct set Quantity = Quantity + 1 where ID = @productID
                end",
                recordID, productID, userID, employeeID, bussinessID, NewUniqueCode(userID, employeeID, bussinessID, "Export"), DateTime.Now.DbValue());
            return Execute(new DbQuery(userID, employeeID, DbAction.Repair.View, query, true), out queryOutput);
        }
        public static bool RemoveProduct(int userID, int employeeID, int productID)
        {
            QueryOutput queryOutput;
            var query = String.Format(
                @"declare @exportID int = (select ExportID from ExportProduct where ID = {0})
                delete ExportProduct where ID = {0}
                if ((select count(ID) from ExportProduct where ExportID = @exportID) = 0) begin
                    delete Export where ID = @exportID
                end", productID);
            return Execute(new DbQuery(userID, employeeID, DbAction.Repair.View, query, true), out queryOutput);
        }
    }
}