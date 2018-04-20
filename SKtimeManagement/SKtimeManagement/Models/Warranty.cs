using Dapper;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace SKtimeManagement
{
    public class WarrantyStatus
    {
        public const string New = "Tiếp nhận";
        public const string Transfer = "Chuyển đi TTBH";
        public const string Received = "TTBH tiếp nhận";
        public const string Processing = "Đang sửa chữa";
        public const string Processed = "Chuyển đến cửa hàng";
        public const string Returned = "Cửa hàng đã nhận";
        public const string Finish = "Đã giao khách hàng";
        public static string[] List
        {
            get
            {
                return new string[] 
                {
                    WarrantyStatus.New, WarrantyStatus.Transfer, WarrantyStatus.Received, WarrantyStatus.Processing,
                    WarrantyStatus.Processed, WarrantyStatus.Returned, WarrantyStatus.Finish
                };
            }
        }
    }
    public class WarrantyList
    {
        public WarrantyList(WarrantyFilter filter = null)
        {
            Filter = filter;
            if (Filter == null)
                Filter = new WarrantyFilter();
        }
        public WarrantyFilter Filter { get; set; }
        public List<Warranty> Data { get; set; }
        public WarrantyModel Current { get; set; }
    }
    public class WarrantyFilter
    {
        public WarrantyFilter() { }
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
    public class WarrantyModel
    {
        public WarrantyModel(List<ExportRecord> orders = null, ImexFilter filter = null, Warranty model = null)
        {
            Filter = filter;
            if (Filter == null)
                Filter = new ImexFilter();
            Record = model;
            if (Record == null)
                Record = new Warranty();
            Orders = orders;
            if (Orders == null)
                Orders = new List<ExportRecord>();
        }
        public WarrantyModel(List<ProductInfo> products, ImexFilter filter = null, Warranty model = null)
        {
            Filter = filter;
            if (Filter == null)
                Filter = new ImexFilter();
            Record = model;
            if (Record == null)
                Record = new Warranty();
            Products = products;
            if (Orders == null)
                Orders = new List<ExportRecord>();
        }
        public ImexFilter Filter { get; set; }
        public Warranty Record { get; set; }
        public List<ExportRecord> Orders { get; set; }
        public List<ProductInfo> Products { get; set; }
        public ExportRecord SelectedOrder { get; set; }
        public ProductInfo SelectedProduct { get; set; }
        public bool ViewInternalNote { get; set; }
        public bool Editable { get; set; }
        public bool Edit { get; set; }
        public string ReceiveWarehouseName { get; set; }
    }
    public class WarrantyNote
    {
        public int ID { get; set; }
        public int WarrantyID { get; set; }
        public int EmployeeID { get; set; }
        public string EmployeeName { get; set; }
        public DateTime SubmitDate { get; set; }
        public string Message { get; set; }
        public string Title { get { return String.Format("{0} ({1})", EmployeeName, SubmitDate.ToString(Constants.DateTimeString)); } }
    }
    public class WarrantyNoteList
    {
        public WarrantyNoteList()
        {
            ID = Guid.NewGuid().ToString();
        }
        public string ID { get; set; }
        public string RemoveUrl { get; set; }
        public List<WarrantyNote> Data { get; set; }
    }
    public class Warranty : BaseModel
    {
        public Warranty()
        {
            FinishDate = DateTime.Now;
        }
        public int ID { get; set; }
        public int BussinessID { get; set; }
        public int EmployeeID { get; set; }
        public int WarehouseID { get; set; }
        public int ReceiveWarehouseID { get; set; }
        public int ProductID { get; set; }
        public string EmployeeName { get; set; }
        public string WarehouseName { get; set; }
        public string ReceiveWarehouseName { get; set; }
        public int? ClientID { get; set; }
        public string ClientCode { get; set; }
        public string ClientName { get; set; }
        public string ClientPhone { get; set; }
        public string ClientAddress { get; set; }
        public int? OrderID { get; set; }
        public string OrderCode { get; set; }
        public string Code { get; set; }
        public string ContactName { get; set; }
        public string ContactPhone { get; set; }
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
        public List<WarrantyNote> MechanicNotes { get; set; }
        public List<WarrantyNote> InternalNotes { get; set; }
        public List<Warranty> History { get; set; }
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
        public Warranty Save(ModelStateDictionary modelState, int userID, int employeeID, int bussinessID, string employeeName = null)
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
                    @"update Warranty 
                    set TransferDate = {1}, ReceivedDate = {2}, ProcessedDate = {3}, ReturnedDate = {4}, FinishDate = {5}, Service = N'{13}', 
                        ProductState = N'{6}', Fee = {7}, Discount = {8}, Other = N'{9}', Note = N'{10}', ContactName = N'{11}', ContactPhone = N'{12}'
                    where ID = {0}",
                    new object[] {
                        ID, TransferDate.DbValue(), ReceivedDate.DbValue(), ProcessedDate.DbValue(), ReturnedDate.DbValue(),
                        FinishDate.DbValue(), ProductState, Fee, Discount, Other, Note, ContactName, ContactPhone, Service
                });
                action = DbAction.Warranty.Modify;
            }
            else
            {
                Code = NewUniqueCode(userID, employeeID, bussinessID, "Warranty");
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
                    insert Warranty(BussinessID, EmployeeID, WarehouseID, ProductID, Code, SubmitDate, TransferDate, ReceivedDate, ProcessedDate, ReturnedDate, FinishDate, 
                        ProductState, Fee, Discount, Other, Note, Status, ClientID, OrderID, OrderCode, ContactName, ContactPhone, ReceiveWarehouseID, Service) 
                    output inserted.ID into @output
                    values ({0}, {1}, {2}, {3}, N'{4}', {5}, {6}, {7}, {8}, {9}, {10}, N'{11}', {12}, {13}, N'{14}', N'{15}', 'active', {16}, {17}, '{18}', N'{19}', N'{20}', {21}, N'{22}')",
                    new object[] {
                        BussinessID, EmployeeID, WarehouseID, ProductID, Code, SubmitDate.DbValue(), TransferDate.DbValue(), ReceivedDate.DbValue(),
                        ProcessedDate.DbValue(), ReturnedDate.DbValue(), FinishDate.DbValue(), ProductState, Fee, Discount, Other, Note,
                        clientID, OrderID.DbValue(), OrderCode, ContactName, ContactPhone, ReceiveWarehouseID, Service
                });
                id = "(select top 1 ID from @output)";
                if (Transactions != null)
                {
                    foreach (var tran in Transactions)
                    {
                        query += tran.AddTransactionQuery(employeeID, TransactionClass.Warranty, ref id);
                    }
                }
                action = DbAction.Warranty.Create;
            }
            query += String.Format(
                @" select top 1 w.ID, w.BussinessID, w.EmployeeID, w.WarehouseID, w.ProductID, w.ClientID, w.OrderID, w.Code, w.Service, 
	                w.SubmitDate, w.TransferDate, w.ReceivedDate, w.ProcessedDate, w.ReturnedDate, w.FinishDate, w.ProductState, w.Fee, w.Discount, w.Other, w.Note, 
                    rwh.Name as [ReceiveWarehouseName], wh.Name as [WarehouseName], w.ContactName, w.ContactPhone, e.Name as [EmployeeName], 
                    c.Name as [ClientName], c.Code as [ClientCode], c.Address as [ClientAddress], c.Phone as [ClientPhone], 
	                case when w.OrderID is not null then o.Code else w.OrderCode end as [OrderCode], isnull(sum(t.Amount), 0) as [Paid]
                from Warranty w 
                    join Warehouse wh on w.WarehouseID = wh.ID and w.Status = 'active' and ((select Username from Login where ID = {1}) = 'admin' or wh.ID in (select WarehouseID from LoginWarehouse where LoginID = {1}))
                    join Warehouse rwh on w.ReceiveWarehouseID = rwh.ID
                    join Employee e on w.EmployeeID = e.ID
                    join Product p on p.ID = w.ProductID
                    left join Client c on w.ClientID = c.ID
                    left join [Order] o on w.OrderID = o.ID
                    left join Transactions t on w.ID = t.WarrantyID
                where w.ID = {0} and w.Status = 'active'
                group by w.ID, w.BussinessID, w.EmployeeID, w.WarehouseID, w.ProductID, w.ClientID, w.OrderID, w.Code, w.ContactName, w.ContactPhone, 
                    w.SubmitDate, w.TransferDate, w.ReceivedDate, w.ProcessedDate, w.ReturnedDate, w.FinishDate, w.ProductState, w.Service, 
	                w.Fee, w.Discount, w.Other, w.Note, wh.Name, rwh.Name, e.Name, c.Name, c.Code, c.Address, c.Phone, o.Code, w.OrderCode", id, userID);
            var record = Query<Warranty>(new DbQuery(userID, employeeID, action, query, true, id), out queryResult).FirstOrDefault();
            if (Result = (queryResult == QueryOutput.Success))
                Messages = new List<string>() { "Lưu thông tin thành công" };
            return record;
        }
        public bool SaveTransferDate(int userID, int employeeID)
        {
            QueryOutput queryResult;
            var query = String.Format("update Warranty set TransferDate = {0} where ID = {1}", TransferDate.DbValue(), ID);
            Execute(new DbQuery(userID, employeeID, DbAction.Warranty.Modify, query, true, ID), out queryResult);
            return queryResult == QueryOutput.Success;
        }
        public bool SaveReceivedDate(int userID, int employeeID)
        {
            QueryOutput queryResult;
            var query = String.Format("update Warranty set ReceivedDate = {0} where ID = {1}", ReceivedDate.DbValue(), ID);
            Execute(new DbQuery(userID, employeeID, DbAction.Warranty.Modify, query, true, ID), out queryResult);
            return queryResult == QueryOutput.Success;
        }
        public bool SaveProcessedDate(int userID, int employeeID)
        {
            QueryOutput queryResult;
            var query = String.Format("update Warranty set ProcessedDate = {0} where ID = {1}", ProcessedDate.DbValue(), ID);
            Execute(new DbQuery(userID, employeeID, DbAction.Warranty.Modify, query, true, ID), out queryResult);
            return queryResult == QueryOutput.Success;
        }
        public bool SaveReturnedDate(int userID, int employeeID)
        {
            QueryOutput queryResult;
            var query = String.Format("update Warranty set ReturnedDate = {0} where ID = {1}", ReturnedDate.DbValue(), ID);
            Execute(new DbQuery(userID, employeeID, DbAction.Warranty.Modify, query, true, ID), out queryResult);
            return queryResult == QueryOutput.Success;
        }
        public bool SaveFinishDate(int userID, int employeeID)
        {
            QueryOutput queryResult;
            var query = String.Format("update Warranty set FinishDate = {0} where ID = {1}", FinishDate.DbValue(), ID);
            Execute(new DbQuery(userID, employeeID, DbAction.Warranty.Modify, query, true, ID), out queryResult);
            return queryResult == QueryOutput.Success;
        }
        public static bool Remove(int userID, int employeeID, int id)
        {
            QueryOutput queryResult;
            Execute(new DbQuery(userID, employeeID, DbAction.Warranty.Remove, String.Format("update Warranty set Status = 'removed' where ID = {0}", id), true, id), out queryResult);
            return queryResult == QueryOutput.Success;
        }
        public static WarrantyList Find(int userID, int employeeID, int bussinessID, WarrantyFilter filter = null, bool log = false, int? top = 100)
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
                @"select {1} w.ID, w.BussinessID, w.EmployeeID, w.WarehouseID, w.ProductID, w.ClientID, w.OrderID, w.Code, w.Service, 
	                w.SubmitDate, w.TransferDate, w.ReceivedDate, w.ProcessedDate, w.ReturnedDate, w.FinishDate, w.ProductState, 
	                w.Fee, w.Discount, w.Other, w.Note, wh.Name as [WarehouseName], rwh.Name as [ReceiveWarehouseName], w.ContactName, w.ContactPhone, e.Name as [EmployeeName], 
                    c.Name as [ClientName], c.Code as [ClientCode], c.Address as [ClientAddress], c.Phone as [ClientPhone], 
	                case when w.OrderID is not null then o.Code else w.OrderCode end as [OrderCode], isnull(sum(t.Amount), 0) as [Paid]
                from Warranty w 
                    join Warehouse wh on w.WarehouseID = wh.ID and w.Status = 'active' and ((select Username from Login where ID = {3}) = 'admin' or wh.ID in (select WarehouseID from LoginWarehouse where LoginID = {3}))
                    join Warehouse rwh on w.ReceiveWarehouseID = rwh.ID
                    join Employee e on w.EmployeeID = e.ID
                    join Product p on p.ID = w.ProductID
                    left join Client c on w.ClientID = c.ID
                    left join [Order] o on w.OrderID = o.ID
                    left join Transactions t on w.ID = t.WarrantyID
                where w.BussinessID = {0} and w.Status = 'active' {2}
                group by w.ID, w.BussinessID, w.EmployeeID, w.WarehouseID, w.ProductID, w.ClientID, w.OrderID, w.Code, w.ContactName, w.ContactPhone, 
                    w.SubmitDate, w.TransferDate, w.ReceivedDate, w.ProcessedDate, w.ReturnedDate, w.FinishDate, w.ProductState, w.Service, 
	                w.Fee, w.Discount, w.Other, w.Note, wh.Name, rwh.Name, e.Name, c.Name, c.Code, c.Address, c.Phone, o.Code, w.OrderCode
                order by w.ID desc", 
                bussinessID, top.HasValue ? String.Format("top {0}", top.Value) : "", String.Join(" ", conditions), userID);
            var result = new WarrantyList(filter);
            result.Data = Query<Warranty>(new DbQuery(userID, employeeID, DbAction.Warranty.View, query, log), out queryResult).ToList();
            return result;
        }
        public static WarrantyModel Get(int userID, int employeeID, int id)
        {
            QueryOutput queryResult;
            var query = String.Format(
                @"select top 1 w.ID, w.BussinessID, w.EmployeeID, w.WarehouseID, w.ProductID, w.ClientID, w.OrderID, w.Code, w.Service, 
	                w.SubmitDate, w.TransferDate, w.ReceivedDate, w.ProcessedDate, w.ReturnedDate, w.FinishDate, w.ProductState, 
	                w.Fee, w.Discount, w.Other, w.Note, wh.Name as [WarehouseName], rwh.Name as [ReceiveWarehouseName], w.ContactName, w.ContactPhone, e.Name as [EmployeeName], 
                    c.Name as [ClientName], c.Code as [ClientCode], c.Address as [ClientAddress], c.Phone as [ClientPhone], 
	                case when w.OrderID is not null then o.Code else w.OrderCode end as [OrderCode], isnull(sum(t.Amount), 0) as [Paid]
                from Warranty w 
                    join Warehouse wh on w.WarehouseID = wh.ID and w.Status = 'active' and ((select Username from Login where ID = {1}) = 'admin' or wh.ID in (select WarehouseID from LoginWarehouse where LoginID = {1}))
                    join Warehouse rwh on w.ReceiveWarehouseID = rwh.ID
                    join Employee e on w.EmployeeID = e.ID
                    join Product p on p.ID = w.ProductID
                    left join Client c on w.ClientID = c.ID
                    left join [Order] o on w.OrderID = o.ID
                    left join Transactions t on w.ID = t.WarrantyID
                where w.ID = {0} and w.Status = 'active'
                group by w.ID, w.BussinessID, w.EmployeeID, w.WarehouseID, w.ProductID, w.ClientID, w.OrderID, w.Code, w.ContactName, w.ContactPhone, 
                    w.SubmitDate, w.TransferDate, w.ReceivedDate, w.ProcessedDate, w.ReturnedDate, w.FinishDate, w.ProductState, w.Service, 
	                w.Fee, w.Discount, w.Other, w.Note, wh.Name, rwh.Name, e.Name, c.Name, c.Code, c.Address, c.Phone, o.Code, w.OrderCode", id, userID);
            var result = new WarrantyModel();
            var action = DbAction.Warranty.View;
            result.Record = Query<Warranty>(new DbQuery(userID, employeeID, action, query), out queryResult).FirstOrDefault();
            if (result.Record != null)
            {
                result.SelectedProduct = ProductInfo.Get(userID, employeeID, result.Record.ProductID, false, action);
                if (result.Record.OrderID.HasValue)
                    result.SelectedOrder = Export.GetOrder(userID, employeeID, result.Record.OrderID.Value, false, action);
            }
            return result;
        }
        public static List<Warranty> GetHistory(int userID, int productID, string orderCode)
        {
            using (var con = Repo.DB.SKtimeManagement)
            {
                var query = String.Format(
                    @"select w.ID, w.BussinessID, w.EmployeeID, w.WarehouseID, w.ProductID, w.ClientID, w.OrderID, w.Code, w.Service, 
	                    w.SubmitDate, w.TransferDate, w.ReceivedDate, w.ProcessedDate, w.ReturnedDate, w.FinishDate, w.ProductState, 
	                    w.Fee, w.Discount, w.Other, w.Note, wh.Name as [WarehouseName], rwh.Name as [ReceiveWarehouseName], w.ContactName, w.ContactPhone, e.Name as [EmployeeName], 
                        c.Name as [ClientName], c.Code as [ClientCode], c.Address as [ClientAddress], c.Phone as [ClientPhone], 
	                    case when w.OrderID is not null then o.Code else w.OrderCode end as [OrderCode], isnull(sum(t.Amount), 0) as [Paid]
                    from Warranty w 
                        join Warehouse wh on w.WarehouseID = wh.ID and w.Status = 'active' and ((select Username from Login where ID = {2}) = 'admin' or wh.ID in (select WarehouseID from LoginWarehouse where LoginID = {2}))
                        join Warehouse rwh on w.ReceiveWarehouseID = rwh.ID
                        join Employee e on w.EmployeeID = e.ID
                        join Product p on p.ID = w.ProductID
                        left join Client c on w.ClientID = c.ID
                        left join [Order] o on w.OrderID = o.ID
                        left join Transactions t on w.ID = t.WarrantyID
                    where w.Status = 'active' and w.ProductID = {0} and (w.OrderCode = '{1}' or o.Code = '{1}')
                    group by w.ID, w.BussinessID, w.EmployeeID, w.WarehouseID, w.ProductID, w.ClientID, w.OrderID, w.Code, w.ContactName, w.ContactPhone, 
                        w.SubmitDate, w.TransferDate, w.ReceivedDate, w.ProcessedDate, w.ReturnedDate, w.FinishDate, w.ProductState, w.Service, 
	                    w.Fee, w.Discount, w.Other, w.Note, wh.Name, rwh.Name, e.Name, c.Name, c.Code, c.Address, c.Phone, o.Code, w.OrderCode
                    order by w.ID desc", productID, orderCode, userID);
                return con.Query<Warranty>(query).ToList();
            }
        }
        private static string MechanicNoteQuery(object recordID)
        {
            return String.Format(
                @"select n.*, e.Name as [EmployeeName] 
                from WarrantyMechanicNote n join Employee e on n.EmployeeID = e.ID 
                where n.WarrantyID = {0}
                order by n.ID desc", recordID);
        }
        public static WarrantyNoteList GetMechanicNotes(int userID, int employeeID, int recordID)
        {
            QueryOutput queryOutput;
            var list = new WarrantyNoteList();
            list.Data = Query<WarrantyNote>(new DbQuery(userID, employeeID, DbAction.Warranty.View, MechanicNoteQuery(recordID)), out queryOutput).ToList();
            return list;
        }
        public static WarrantyNoteList SaveMechanicNote(int userID, int employeeID, WarrantyNote note)
        {
            QueryOutput queryOutput;
            var query = String.Format(
                @"insert into WarrantyMechanicNote(WarrantyID, EmployeeID, SubmitDate, Message)
                values ({0}, {1}, {2}, N'{3}') {4}", note.WarrantyID, employeeID, DateTime.Now.DbValue(), note.Message, MechanicNoteQuery(note.WarrantyID));
            var list = new WarrantyNoteList();
            list.Data = Query<WarrantyNote>(new DbQuery(userID, employeeID, DbAction.Warranty.View, query), out queryOutput).ToList();
            return list;
        }
        public static WarrantyNoteList RemoveMechanicNote(int userID, int employeeID, int noteID)
        {
            QueryOutput queryOutput;
            var query = String.Format(
                @"declare @recordID int = (select top 1 WarrantyID from WarrantyMechanicNote where ID = {0}) 
                delete WarrantyMechanicNote where ID = {0} {1}", 
                noteID, MechanicNoteQuery("@recordID"));
            var list = new WarrantyNoteList();
            list.Data = Query<WarrantyNote>(new DbQuery(userID, employeeID, DbAction.Warranty.View, query), out queryOutput).ToList();
            return list;
        }
        private static string InternalNoteQuery(object recordID)
        {
            return String.Format(
                @"select n.*, e.Name as [EmployeeName] 
                from WarrantyInternalNote n join Employee e on n.EmployeeID = e.ID 
                where n.WarrantyID = {0}
                order by n.ID desc", recordID);
        }
        public static WarrantyNoteList GetInternalNotes(int userID, int employeeID, int recordID)
        {
            QueryOutput queryOutput;
            var list = new WarrantyNoteList();
            list.Data = Query<WarrantyNote>(new DbQuery(userID, employeeID, DbAction.Warranty.View, InternalNoteQuery(recordID)), out queryOutput).ToList();
            return list;
        }
        public static WarrantyNoteList SaveInternalNote(int userID, int employeeID, WarrantyNote note)
        {
            QueryOutput queryOutput;
            var query = String.Format(
                @"insert into WarrantyInternalNote(WarrantyID, EmployeeID, SubmitDate, Message)
                values ({0}, {1}, {2}, N'{3}') {4}", note.WarrantyID, employeeID, DateTime.Now.DbValue(), note.Message, InternalNoteQuery(note.WarrantyID));
            var list = new WarrantyNoteList();
            list.Data = Query<WarrantyNote>(new DbQuery(userID, employeeID, DbAction.Warranty.View, query), out queryOutput).ToList();
            return list;
        }
        public static WarrantyNoteList RemoveInternalNote(int userID, int employeeID, int noteID)
        {
            QueryOutput queryOutput;
            var query = String.Format(
                @"declare @recordID int = (select top 1 WarrantyID from WarrantyInternalNote where ID = {0})
                delete WarrantyInternalNote where ID = {0} {1}", 
                noteID, InternalNoteQuery("@recordID"));
            var list = new WarrantyNoteList();
            list.Data = Query<WarrantyNote>(new DbQuery(userID, employeeID, DbAction.Warranty.View, query), out queryOutput).ToList();
            return list;
        }
        public static List<ExportProduct> GetProducts(int userID, int employeeID, int recordID)
        {
            QueryOutput queryOutput;
            return Query<ExportProduct>(new DbQuery(userID, employeeID, DbAction.Export.View,
                String.Format("select e.*, p.Code, p.Name as [ProductName], p.Unit from ExportProduct e join Product p on e.ProductID = p.ID where e.Returned = 0 and e.WarrantyID = {0}",
                recordID)), out queryOutput);
        }
        public static bool AddProduct(int userID, int employeeID, int bussinessID, int productID, int recordID)
        {
            QueryOutput queryOutput;
            var query = String.Format(
                @"declare @warehouseID int = (select WarehouseID from Warranty where ID = {0})
                declare @exportID int = (select top 1 ExportID from ExportProduct where WarrantyID = {0})
                declare @IDs table (ID int)
                if (@exportID is null) begin
                    insert Export(EmployeeID, BussinessID, WarehouseID, Code, SubmitDate, Status, Removed)
                    output inserted.ID into @IDs
                    values ({3}, {4}, @warehouseID, '{5}', {6}, N'Đã xuất', 0)
                    set @exportID = (select top 1 ID from @IDs)
                end
                declare @productID int = (select ID from ExportProduct where ExportID = @exportID and ProductID = {1})
                if (@productID is null) begin
                    insert into ExportProduct(WarrantyID, ExportID, ProductID, Quantity, Price, Point, Returned)
                    values ({0}, @exportID, {1}, 1, 0, 0, 0)
                end else begin
                    update ExportProduct set Quantity = Quantity + 1 where ID = @productID
                end",
                recordID, productID, userID, employeeID, bussinessID, NewUniqueCode(userID, employeeID, bussinessID, "Export"), DateTime.Now.DbValue());
            return Execute(new DbQuery(userID, employeeID, DbAction.Warranty.View, query, true), out queryOutput);
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
            return Execute(new DbQuery(userID, employeeID, DbAction.Warranty.View, query, true), out queryOutput);
        }
    }
}