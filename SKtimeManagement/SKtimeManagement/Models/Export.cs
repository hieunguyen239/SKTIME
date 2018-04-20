using Dapper;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace SKtimeManagement
{
    public class OrderStatus
    {
        public const string Normal = "Bình thường";
        public const string Refunded = "Trả hàng";
        public const string Unpaid = "Chưa thanh toán";
        public const string Paid = "Đã thanh toán";
    }
    public class PayMethod
    {
        public const string Cash = "Tiền mặt";
        public const string Card = "Thẻ";
        public const string BankTransfer = "Chuyển khoản";
        public static string[] List
        {
            get
            {
                return new string[] { PayMethod.Cash, PayMethod.Card, PayMethod.BankTransfer };
            }
        }
    }
    public class ImexFilter
    {
        public int? WarehouseID { get; set; }
        public int? EmployeeID { get; set; }
        public int? ClientID { get; set; }
        public string Code { get; set; }
        public string Receipt { get; set; }
        public string OrderCode { get; set; }
        public string ExportCode { get; set; }
        public string ClientPhone { get; set; }
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = Constants.DateTimeString)]
        public DateTime? From { get; set; }
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = Constants.DateTimeString)]
        public DateTime? To { get; set; }
        public string Returned { get; set; }
        public string FindUnpaid { get; set; }
    }
    public class ExportList
    {
        public ExportList(ImexFilter filter = null)
        {
            List = new List<ExportRecord>();
            Filter = filter != null ? filter : new ImexFilter();
        }
        public List<ExportRecord> List { get; set; }
        public ImexFilter Filter { get; set; }
        public ExportRecord Current { get; set; }
        public string Message { get; set; }
        public bool Result { get; set; }
    }
    public class OrderRecord
    {
        public OrderRecord()
        {
            Code = DateTime.Now.ToString(Constants.CodeValue);
        }
        public OrderRecord(ExportRecord record, bool returned = false)
        {
            ID = record.ID;
            Code = record.Code;
            WarehouseID = record.WarehouseID;
            EmployeeID = record.EmployeeID;
            EmployeeName = record.EmployeeName;
            ClientID = record.ClientID;
            ClientCode = record.ClientCode;
            ClientName = record.ClientName;
            ClientPhone = record.ClientPhone;
            ClientAddress = record.ClientAddress;
            ClientType = record.ClientType;
            DiscountType = record.DiscountType;
            DiscountValue = record.DiscountValue;
            Items = record.Products.Select(p => new ImexItem(p) { Return = returned }).ToArray();
            ReturnItems = record.ReturnProducts.Select(p => new ImexItem(p)).ToArray();
            PayMethod = record.PayMethod;
            Export = record.ExportID.HasValue ? "on" : "";
            DeliveryID = record.DeliveryID;
            Paid = record.Paid;
            Receipt = record.Receipt;
            Note = record.Note;
            Discount = record.Discount;
            Status = record.Status;
            Returned = record.Returned;
        }
        public int ID { get; set; }
        public string Code { get; set; }
        public int WarehouseID { get; set; }
        public string WarehouseName { get; set; }
        public int EmployeeID { get; set; }
        public int? NewEmployeeID { get; set; }
        public string EmployeeName { get; set; }
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = Constants.DateTimeString)]
        public DateTime SubmitDate { get; set; }
        public string SubmitDateString { get { return SubmitDate.ToString(Constants.DateTimeString); } }
        public int? ClientID { get; set; }
        public string ClientCode { get; set; }
        public string ClientName { get; set; }
        public string ClientPhone { get; set; }
        public string ClientAddress { get; set; }
        public string ClientType { get; set; }
        public string DiscountType { get; set; }
        public int? DiscountValue { get; set; }
        public ImexItem[] Items { get; set; }
        public ImexItem[] ReturnItems { get; set; }
        public string PayMethod { get; set; }
        public int ExportID { get; set; }
        public string Export { get; set; }
        public int? DeliveryID { get; set; }
        public bool ToExport { get { return Export == "on"; } }
        public decimal Total { get; set; }
        public string TotalString { get { return Total.GetCurrencyString(); } }
        public decimal Paid { get; set; }
        public string PaidString { get { return Paid.GetCurrencyString(); } }
        public string Receipt { get; set; }
        public string Note { get; set; }
        public decimal Discount { get; set; }
        public string DiscountString { get { return Discount.GetCurrencyString(); } }
        public string Status { get; set; }
        public bool Return { get; set; }
        public decimal ReturnValue { get; set; }
        public bool Returned { get; set; }
        public List<Transaction> Transactions { get; set; }
    }
    public class ExportRecord
    {
        public ExportRecord()
        {
            Code = DateTime.Now.ToString(Constants.CodeValue);
        }
        public int ID { get; set; }
        public int WarehouseID { get; set; }
        public string WarehouseName { get; set; }
        public int EmployeeID { get; set; }
        public string EmployeeName { get; set; }
        public string Code { get; set; }
        public int? OrderID { get; set; }
        public string OrderCode { get; set; }
        public int? ExportID { get; set; }
        public string ExportCode { get; set; }
        public int? ToWarehouseID { get; set; }
        public string ToWarehouseName { get; set; }
        public int? ClientID { get; set; }
        public string ClientCode { get; set; }
        public string ClientName { get; set; }
        public string ClientPhone { get; set; }
        public string ClientAddress { get; set; }
        public string ClientType { get; set; }
        public int ClientPoint { get; set; }
        public string ClientPointString { get { return ClientPoint.GetCurrencyString(); } }
        public string DiscountType { get; set; }
        public int? DiscountValue { get; set; }
        public int? DeliveryID { get; set; }
        public string DeliveryName { get; set; }
        public string Receipt { get; set; }
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = Constants.DateTimeString)]
        public DateTime SubmitDate { get; set; }
        public string SubmitDateString { get { return SubmitDate.ToString(Constants.DateTimeString); } }
        public string PayMethod { get; set; }
        public decimal Discount { get; set; }
        public string DiscountString { get { return Discount.GetCurrencyString(); } }
        public decimal Paid { get; set; }
        public string PaidString { get { return Paid.GetCurrencyString(); } }
        public string Note { get; set; }
        public bool Result { get; set; }
        public string Message { get; set; }
        public string CreateExport { get; set; }
        public string CreateOrder { get; set; }
        public decimal Total { get; set; }
        public string TotalString { get { return Total.GetCurrencyString(); } }
        public string Status { get; set; }
        public string PaidStatus { get { return Total - Discount > Paid ? OrderStatus.Unpaid : OrderStatus.Normal; } }
        public string StatusString { get { return String.IsNullOrEmpty(Status) ? OrderStatus.Normal : OrderStatus.Refunded; } }
        public bool Returned { get; set; }
        public List<ExportProduct> Products { get; set; }
        public List<ExportProduct> ReturnProducts { get; set; }
    }
    public class ExportProduct
    {
        public int ID { get; set; }
        public int? OrderID { get; set; }
        public int? RepairID { get; set; }
        public int? WarrantyID { get; set; }
        public int ExportID { get; set; }
        public int ProductID { get; set; }
        public string Code { get; set; }
        public string ProductName { get; set; }
        public string ProductUrl { get; set; }
        public string Image { get; set; }
        public int WarehouseID { get; set; }
        public string WarehouseName { get; set; }
        public string Unit { get; set; }
        public int Maximum { get; set; }
        public int Quantity { get; set; }
        public string QuantityString { get { return Quantity.GetCurrencyString(); } }
        public decimal Price { get; set; }
        public string PriceString
        {
            get
            {
                return Price.GetCurrencyString();
            }
        }
        public int Point { get; set; }
        public string PointString
        {
            get
            {
                return Point.GetCurrencyString();
            }
        }
        public string Total
        {
            get
            {
                return (Price * Quantity).GetCurrencyString();
            }
        }
        public string OriginalWarranty { get; set; }
        public int OriginalWarrantyValue
        {
            get
            {
                var value = 0;
                if (!String.IsNullOrEmpty(OriginalWarranty))
                    Int32.TryParse(OriginalWarranty.Split(' ')[0], out value);
                return value;
            }
        }
        public string BussinessWarranty { get; set; }
        public int BussinessWarrantyValue
        {
            get
            {
                var value = 0;
                if (!String.IsNullOrEmpty(BussinessWarranty))
                    Int32.TryParse(BussinessWarranty.Split(' ')[0], out value);
                return value;
            }
        }
        public List<Warranty> WarrantHistory { get; set; }
        public int WarrantCount { get; set; }
    }
    public class ExportStatus
    {
        public const string Pending = "Chờ";
        public const string Delivered = "Đã chuyển";
        public const string Exported = "Đã xuất";
    }
    public class Export : BaseModel
    {
        public Export(string employeeName, List<WarehouseInfo> warehouses, int? toWarehouseID = null, string status = ExportStatus.Pending, int warehouseID = 0, ImexItem[] selected = null, string note = "", string message = "", bool result = false) : base()
        {
            EmployeeName = employeeName;
            WarehouseID = warehouseID;
            if (warehouseID == 0 && warehouses.Count >= 1)
                WarehouseID = warehouses.FirstOrDefault().ID;
            ToWarehouseID = toWarehouseID;
            Data = new List<ImexItem>();
            Warehouses = warehouses;
            if (selected == null)
                Selected = new ImexItem[] { };
            else
                Selected = selected;
            Note = note;
            Message = message;
            Result = result;
            Filter = new ProductFilter();
            Status = status;
        }
        public Export(List<WarehouseInfo> warehouses, OrderRecord record, bool editable = false, string message = "", bool result = false)
        {
            ID = record.ID;
            Code = record.Code;
            EmployeeID = record.EmployeeID;
            EmployeeName = record.EmployeeName;
            WarehouseID = record.WarehouseID;
            if (record.WarehouseID == 0 && warehouses.Count >= 1)
                WarehouseID = warehouses.FirstOrDefault().ID;
            Warehouses = warehouses;
            ClientID = record.ClientID;
            ClientCode = record.ClientCode;
            ClientName = record.ClientName;
            ClientPhone = record.ClientPhone;
            ClientAddress = record.ClientAddress;
            ClientType = record.ClientType;
            DiscountType = record.DiscountType;
            DiscountValue = record.DiscountValue;
            DeliveryID = record.DeliveryID;
            OrderReceipt = record.Receipt;
            PayMethod = record.PayMethod;
            ToExport = record.ToExport;
            Discount = record.Discount;
            Paid = record.Paid;
            Data = new List<ImexItem>();
            if (record.Items == null)
                Selected = new ImexItem[] { };
            else
                Selected = record.Items;
            Note = record.Note;
            Status = record.Status;
            Return = record.Return;
            Message = message;
            Result = result;
            Editable = editable;
            Filter = new ProductFilter();
        }
        public int ID { get; set; }
        public string Code { get; set; }
        public int EmployeeID { get; set; }
        public string EmployeeName { get; set; }
        public int WarehouseID { get; set; }
        public int? ClientID { get; set; }
        public string ClientCode { get; set; }
        public string ClientName { get; set; }
        public string ClientPhone { get; set; }
        public string ClientAddress { get; set; }
        public string ClientType { get; set; }
        public string DiscountType { get; set; }
        public int? DiscountValue { get; set; }
        public int? DeliveryID { get; set; }
        public string DeliveryName { get; set; }
        public int? ToWarehouseID { get; set; }
        public string ToWarehouseName { get; set; }
        public string OrderReceipt { get; set; }
        public string PayMethod { get; set; }
        public bool ToExport { get; set; }
        public decimal Discount { get; set; }
        public decimal Paid { get; set; }
        public string Status { get; set; }
        public bool Editable { get; set; }
        public bool Return { get; set; }
        public bool AllowDept { get; set; }
        public ProductFilter Filter { get; set; }
        public List<ImexItem> Data { get; set; }
        public List<WarehouseInfo> Warehouses { get; set; }
        public ImexItem[] Selected { get; set; }
        public string Note { get; set; }
        public string Message { get; set; }
        public static ExportProduct GetProduct(int userID, int employeeID, int productID, string action = null)
        {
            if (String.IsNullOrEmpty(action))
                action = DbAction.Warranty.Create;
            QueryOutput queryResult;
            var query = String.Format(
                @"select e.ID, e.OrderID, e.ExportID, e.ProductID, e.Quantity, e.Price, e.Point, p.Code, p.Name as [ProductName], p.Unit, p.OriginalWarranty, p.BussinessWarranty, p.Image, count(wr.ID) as [WarrantCount]
                from ExportProduct e 
	                join Export ex on e.ExportID = ex.ID
                    join Product p on e.ProductID = p.ID 
                    left join Warranty wr on e.ID = wr.ProductID and ex.WarehouseID = wr.WarehouseID and wr.Status = 'active'
                where e.ID = {0}
                group by e.ID, e.OrderID, e.ExportID, e.ProductID, e.Quantity, e.Price, e.Point, p.ID, p.Code, p.Name, p.Unit, p.OriginalWarranty, p.BussinessWarranty, p.Image",
                productID);
            return Query<ExportProduct>(new DbQuery(userID, employeeID, action, query), out queryResult).FirstOrDefault();
        }
        public static ExportRecord GetOrder(int userID, int employeeID, int recordID, bool log = false, string action = null)
        {
            if (String.IsNullOrEmpty(action))
                action = DbAction.Order.View;
            QueryOutput queryResult;
            var result = Query<ExportRecord>(new DbQuery(userID, employeeID, action, 
                String.Format(@"select top 100 o.ID, o.EmployeeID, o.DeliveryID, o.WarehouseID, o.Code, o.SubmitDate, o.PayMethod, o.Receipt, o.Total, o.Discount, o.Paid, o.Note, o.Status, o.Returned,
                                    w.Name as [WarehouseName], c.ID as [ClientID], c.Code as [ClientCode], c.Name as [ClientName], c.Address as [ClientAddress], c.Phone as [ClientPhone], 
                                    em.Name as [EmployeeName], del.Name as [DeliveryName], ex.ID as [ExportID], ex.Code as [ExportCode], t.Name as [ClientType], t.DiscountType as [DiscountType], t.DiscountValue as [DiscountValue]
                                from SKtimeManagement..[Order] o
                                    join Warehouse w on o.WarehouseID = w.ID and w.Status = 'active' and ((select Username from Login where ID = {1}) = 'admin' or w.ID in (select WarehouseID from LoginWarehouse where LoginID = {1}))
                                    join ExportProduct p on o.ID = p.OrderID and p.Returned = 0
                                    left join Export ex on ex.ID = p.ExportID
                                    join Employee em on o.EmployeeID = em.ID
                                    left join Delivery del on o.DeliveryID = del.ID
                                    left join Client c on o.ClientID = c.ID
                                    left join ClientType ct on c.ID = ct.ClientID
                                    left join Type t on ct.TypeID = t.ID
                                where o.Removed = 0 and o.ID = {0}
                                group by o.ID, o.EmployeeID, o.DeliveryID, o.WarehouseID, o.Code, o.SubmitDate, o.PayMethod, o.Receipt, o.Total, o.Discount, o.Paid, o.Note, o.Status, o.Returned, 
                                    w.Name, c.ID, c.Code, c.Name, c.Address, c.Phone, em.Name, del.Name, ex.ID, ex.Code, t.Name, t.DiscountType, t.DiscountValue
                                order by o.ID desc", recordID, userID)), out queryResult).FirstOrDefault();
            result.Products = Query<ExportProduct>(new DbQuery(userID, employeeID, action, 
                String.Format(
                    @"select e.ID, e.OrderID, e.ExportID, e.ProductID, e.Quantity, e.Price, e.Point, p.Code, p.Name as [ProductName], p.Unit, p.OriginalWarranty, p.BussinessWarranty, 
                        sum(isnull(ip.Quantity, 0)) - isnull((select sum(ep.Quantity) from ExportProduct ep left join Export e on e.ID = ep.ExportID and e.Removed = 0 and e.Status in (N'Đã xuất', N'Đã chuyển') where ep.ProductID = p.ID and e.WarehouseID = max(w.ID)), 0) as [Maximum],
                        count(wr.ID) as [WarrantCount]
                    from ExportProduct e 
	                    join Export ex on e.ExportID = ex.ID
                        join Product p on e.ProductID = p.ID 
                        left join ImportProduct ip on p.ID = ip.ProductID
                        left join Import i on i.ID = ip.ImportID and i.Status = 'active' 
                        left join Warehouse w on w.Status = 'active' and w.ID = i.WarehouseID
                        left join Warranty wr on e.ID = wr.ProductID and ex.WarehouseID = wr.WarehouseID and wr.Status = 'active'
                    where e.Returned = 0 and e.OrderID = {0} and w.ID = {1}
                    group by e.ID, e.OrderID, e.ExportID, e.ProductID, e.Quantity, e.Price, e.Point, p.ID, p.Code, p.Name, p.Unit, p.OriginalWarranty, p.BussinessWarranty", 
                recordID, result.WarehouseID), log), out queryResult).ToList();
            result.ReturnProducts = Query<ExportProduct>(new DbQuery(userID, employeeID, action,
                String.Format(
                    @"select e.ID, e.OrderID, e.ExportID, e.ProductID, e.Quantity, e.Price, e.Point, p.Code, p.Name as [ProductName], p.Unit, p.OriginalWarranty, p.BussinessWarranty, 
                        sum(isnull(ip.Quantity, 0)) - isnull((select sum(ep.Quantity) from ExportProduct ep left join Export e on e.ID = ep.ExportID and e.Removed = 0 and e.Status in (N'Đã xuất', N'Đã chuyển') where ep.ProductID = p.ID and e.WarehouseID = max(w.ID)), 0) as [Maximum]
                    from ExportProduct e 
                        join Product p on e.ProductID = p.ID 
                        left join ImportProduct ip on p.ID = ip.ProductID
                        left join Import i on i.ID = ip.ImportID and i.Status = 'active' 
                        left join Warehouse w on w.Status = 'active' and w.ID = i.WarehouseID
                    where e.Returned = 1 and e.OrderID = {0} and w.ID = {1}
                    group by e.ID, e.OrderID, e.ExportID, e.ProductID, e.Quantity, e.Price, e.Point, p.ID, p.Code, p.Name, p.Unit, p.OriginalWarranty, p.BussinessWarranty",
                recordID, result.WarehouseID), log), out queryResult).ToList();
            return result;
        }
        public static ExportRecord GetExport(int userID, int employeeID, int recordID, bool log = false)
        {
            QueryOutput queryResult;
            var result = Query<ExportRecord>(new DbQuery(userID, employeeID, DbAction.Export.View, 
                String.Format(@"select top 1 e.ID, e.EmployeeID, e.BussinessID, e.WarehouseID, e.Code, e.SubmitDate, e.Note, e.Status, e.ToWarehouseID, tw.Name as [ToWarehouseName], w.Name as [WarehouseName], c.ID as [ClientID], c.Name as [ClientName], o.ID as [OrderID], o.Code as [OrderCode], em.Name as [EmployeeName]
                                from Export e 
                                    join Warehouse w on e.WarehouseID = w.ID and w.Status = 'active'
                                    join Employee em on e.EmployeeID = em.ID
                                    join ExportProduct p on e.ID = p.ExportID and p.Returned = 0
                                    left join SKtimeManagement..[Order] o on p.OrderID = o.ID
                                    left join Client c on o.ClientID = c.ID
                                    left join Warehouse tw on e.ToWarehouseID = tw.ID and tw.Status = 'active'
                                where e.Removed  = 0 and p.OrderID is null and p.ExportID is not null and e.ID = {0} 
                                    and ((select Username from Login where ID = {1}) = 'admin' or e.WarehouseID in (select WarehouseID from LoginWarehouse where LoginID = {1}) or e.ToWarehouseID in (select WarehouseID from LoginWarehouse where LoginID = {1}))
                                group by e.ID, e.EmployeeID, e.BussinessID, e.WarehouseID, e.Code, e.SubmitDate, e.Note, e.Status, e.ToWarehouseID, tw.Name, w.Name, c.ID, c.Name, o.ID, o.Code, em.Name
                                order by e.ID desc", recordID, userID)), out queryResult).FirstOrDefault();
            result.Products = Query<ExportProduct>(new DbQuery(userID, employeeID, DbAction.Export.View, 
                String.Format("select e.*, p.Code, p.Name as [ProductName], p.Unit from ExportProduct e join Product p on e.ProductID = p.ID where e.Returned = 0 and e.ExportID = {0}", 
                recordID), log), out queryResult).ToList();
            return result;
        }
        public static List<ExportRecord> History(int userID, int employeeID, int bussinessID, ImexFilter filter = null, bool log = false)
        {
            QueryOutput queryResult;
            var conditions = new List<string>();
            if (filter != null)
            {
                if (filter.WarehouseID.HasValue)
                    conditions.Add(String.Format("and e.WarehouseID = {0}", filter.WarehouseID.DbValue()));
                if (filter.EmployeeID.HasValue)
                    conditions.Add(String.Format("and e.EmployeeID = {0}", filter.EmployeeID.DbValue()));
                if (filter.ClientID.HasValue)
                    conditions.Add(String.Format("and c.ID = {0}", filter.ClientID.DbValue()));
                if (!String.IsNullOrEmpty(filter.Code))
                    conditions.Add(String.Format("and e.Code like N'%{0}%'", filter.Code));
                if (!String.IsNullOrEmpty(filter.OrderCode))
                    conditions.Add(String.Format("and o.Code like N'%{0}%'", filter.OrderCode));
                if (filter.From.HasValue)
                    conditions.Add(String.Format("and e.SubmitDate >= '{0}'", filter.From.Value.ToString(Constants.DatabaseDatetimeString)));
                if (filter.To.HasValue)
                    conditions.Add(String.Format("and e.SubmitDate <= '{0}'", filter.To.Value.ToString(Constants.DatabaseDatetimeString)));
            }
            return Query<ExportRecord>(new DbQuery(userID, employeeID, DbAction.Export.View, 
                String.Format(@"select top 100 e.ID, e.EmployeeID, e.BussinessID, e.WarehouseID, e.Code, e.SubmitDate, e.Note, e.Status, e.ToWarehouseID, tw.Name as [ToWarehouseName], w.Name as [WarehouseName], c.ID as [ClientID], c.Name as [ClientName], o.ID as [OrderID], o.Code as [OrderCode], em.Name as [EmployeeName]
                                from Export e 
                                    join Warehouse w on e.WarehouseID = w.ID and w.Status = 'active' and ((select Username from Login where ID = {2}) = 'admin' or w.ID in (select WarehouseID from LoginWarehouse where LoginID = {2}))
                                    join Employee em on e.EmployeeID = em.ID
                                    join ExportProduct p on e.ID = p.ExportID and p.Returned = 0
                                    left join SKtimeManagement..[Order] o on p.OrderID = o.ID
                                    left join Client c on o.ClientID = c.ID
                                    left join Warehouse tw on e.ToWarehouseID = tw.ID and tw.Status = 'active'
                                where e.Removed  = 0 and p.OrderID is null and p.ExportID is not null and e.BussinessID = {0} {1}
                                group by e.ID, e.EmployeeID, e.BussinessID, e.WarehouseID, e.Code, e.SubmitDate, e.Note, e.Status, e.ToWarehouseID, tw.Name, w.Name, c.ID, c.Name, o.ID, o.Code, em.Name
                                order by e.ID desc", bussinessID, String.Join(" ", conditions), userID), log), out queryResult);
        }
        public static List<ExportRecord> OrderHistory(int userID, int employeeID, int bussinessID, ImexFilter filter = null, bool log = false, string action = null)
        {
            if (String.IsNullOrEmpty(action))
                action = DbAction.Order.View;
            QueryOutput queryResult;
            var conditions = new List<string>();
            if (filter != null)
            {
                if (filter.FindUnpaid == "on")
                    conditions.Add(String.Format("and o.Total - o.Discount > o.Paid"));
                if (filter.WarehouseID.HasValue)
                    conditions.Add(String.Format("and o.WarehouseID = {0}", filter.WarehouseID.DbValue()));
                if (filter.EmployeeID.HasValue)
                    conditions.Add(String.Format("and o.EmployeeID = {0}", filter.EmployeeID.DbValue()));
                if (filter.ClientID.HasValue)
                    conditions.Add(String.Format("and c.ID = {0}", filter.ClientID.DbValue()));
                if (!String.IsNullOrEmpty(filter.ClientPhone))
                    conditions.Add(String.Format("and c.Phone like N'%{0}%'", filter.ClientPhone));
                if (!String.IsNullOrEmpty(filter.Code))
                    conditions.Add(String.Format("and o.Code like N'%{0}%'", filter.Code));
                if (!String.IsNullOrEmpty(filter.ExportCode))
                    conditions.Add(String.Format("and ex.Code like N'%{0}%'", filter.ExportCode));
                if (filter.From.HasValue)
                    conditions.Add(String.Format("and o.SubmitDate >= '{0}'", filter.From.Value.ToString(Constants.DatabaseDatetimeString)));
                if (filter.To.HasValue)
                    conditions.Add(String.Format("and o.SubmitDate <= '{0}'", filter.To.Value.ToString(Constants.DatabaseDatetimeString)));
                if (filter.Returned == "on")
                    conditions.Add(String.Format("and o.Returned = 1"));
            }
            return Query<ExportRecord>(new DbQuery(userID, employeeID, action, 
                String.Format(@"select top 100 o.ID, o.EmployeeID, o.DeliveryID, o.WarehouseID, o.Code, o.SubmitDate, o.PayMethod, o.Receipt, o.Total, o.Discount, o.Paid, o.Note, o.Status, o.Returned,
                                    w.Name as [WarehouseName], c.ID as [ClientID], c.Name as [ClientName], em.Name as [EmployeeName], del.Name as [DeliveryName], ex.ID as [ExportID], ex.Code as [ExportCode]
                                from SKtimeManagement..[Order] o
                                    join Warehouse w on o.WarehouseID = w.ID and w.Status = 'active' and ((select Username from Login where ID = {2}) = 'admin' or w.ID in (select WarehouseID from LoginWarehouse where LoginID = {2}))
                                    join ExportProduct p on o.ID = p.OrderID and p.Returned = 0
                                    left join Export ex on ex.ID = p.ExportID
                                    join Employee em on o.EmployeeID = em.ID
                                    left join Delivery del on o.DeliveryID = del.ID
                                    left join Client c on o.ClientID = c.ID
                                where o.Removed = 0 and o.BussinessID = {0} {1} 
                                group by o.ID, o.EmployeeID, o.DeliveryID, o.WarehouseID, o.Code, o.SubmitDate, o.PayMethod, o.Receipt, o.Total, o.Discount, o.Paid, o.Note, o.Status, o.Returned, 
                                    w.Name, c.ID, c.Name, em.Name, del.Name, ex.ID, ex.Code
                                order by o.SubmitDate desc", bussinessID, String.Join(" ", conditions), userID), log), out queryResult);
        }
        public static bool ExportOrder(int userID, int employeeID, int orderID)
        {
            var now = DateTime.Now;
            QueryOutput queryResult;
            return Execute(new DbQuery(userID, employeeID, DbAction.Export.Create,
                String.Format(
                    @"declare @ID table (ID int)
                    insert Export(EmployeeID, BussinessID, WarehouseID, Code, SubmitDate, Status) 
                    output inserted.ID into @ID
                    select EmployeeID, BussinessID, WarehouseID, Code, '{0}', N'{1}' from SKtimeManagement..[Order] where ID = {2}
                    update ExportProduct set ExportID = (select top 1 ID from @ID) where OrderID = {2}",
                now.ToString(Constants.DatabaseDatetimeString), ExportStatus.Exported, orderID), true, "(select top 1 ID from @ID)"), out queryResult);
        }
        public static bool Exported(int userID, int employeeID, int bussinessID, int recordID)
        {
            QueryOutput queryResult;
            var now = DateTime.Now;
            var query = String.Format(
                @"if ((select Status from Export where ID = {0}) = N'{1}') begin
                    update Export set Status = N'{2}' where ID = {0}
                    declare @toWarehouseID int = (select ToWarehouseID from Export where ID = {0})
                    if (@toWarehouseID is not null) begin
                        declare @import table (ID int)
                        insert Import(EmployeeID, BussinessID, Code, SubmitDate, Note, WarehouseID, ExportID, Status) output inserted.ID into @import
                        values ({3}, {4}, '{5}', '{6}', N'Nhập từ phiếu xuất ' + (select Code from Export where ID = {0}), @toWarehouseID, {0}, 'active')
                        insert into ImportProduct(ImportID, ProductID, Quantity, Price) select (select top 1 ID from @import), ProductID, Quantity, Price from ExportProduct where ExportID = {0}
                    end
                end
                ", recordID, ExportStatus.Delivered, ExportStatus.Exported,
                employeeID, bussinessID, NewUniqueCode(userID, employeeID, bussinessID, "Import"), now.ToString(Constants.DatabaseDatetimeString));
            return Execute(new DbQuery(userID, employeeID, DbAction.Export.View, query, true, recordID), out queryResult);
        }
        public static bool Delivered(int userID, int employeeID, int bussinessID, int recordID)
        {
            QueryOutput queryResult;
            var now = DateTime.Now;
            var query = String.Format(
                @"if ((select Status from Export where ID = {0}) = N'{1}') begin
                    update Export set Status = N'{2}' where ID = {0}
                end
                ", recordID, ExportStatus.Pending, ExportStatus.Delivered,
                employeeID, bussinessID, NewUniqueCode(userID, employeeID, bussinessID, "Import"), now.ToString(Constants.DatabaseDatetimeString));
            return Execute(new DbQuery(userID, employeeID, DbAction.Export.View, query, true, recordID), out queryResult);
        }
        public static int Submit(int recordID, int userID, int employeeID, int bussinessID, int warehouseID, int? toWarehouseID, string status, ImexItem[] list, string note = "")
        {
            QueryOutput queryResult;
            var exportID = "@ExportID";
            var now = DateTime.Now;
            var sql = "declare @status nvarchar(10) = ''";
            var action = DbAction.Export.Create;
            if (recordID > 0)
            {
                action = DbAction.Export.Modify;
                sql += String.Format(
                    @" declare {0} int = {1}
                    delete ExportProduct where ExportID = {0}
                    update Export set EmployeeID = {2}, Note = N'{3}', WarehouseID = {4}, ToWarehouseID = {5}, Status = N'{6}' where ID = {0}",
                    exportID, recordID, employeeID, note, warehouseID, toWarehouseID.DbValue(), status);
            }
            else
            {
                if (!toWarehouseID.HasValue)
                    status = ExportStatus.Exported;
                sql += String.Format(
                    @" declare @ID table (ID int) declare {5} int = null
                    insert Export(EmployeeID, BussinessID, Code, SubmitDate, Note, WarehouseID, ToWarehouseID, Status)
                    output inserted.ID into @ID
                    values ({0}, {1}, '{2}', '{3}', N'{4}', {6}, {7}, N'{8}')
                    set {5} = (select top 1 ID from @ID)",
                    employeeID, bussinessID, NewUniqueCode(userID, employeeID, bussinessID, "Export"), now.ToString(Constants.DatabaseDatetimeString), note, exportID, warehouseID, toWarehouseID.DbValue(), status
                );
            }
            sql += String.Format(
                " insert into ExportProduct(ExportID, ProductID, Quantity, Price, Point) values {0}",
                String.Join(",", list.Select(i => String.Format("({0}, {1}, {2}, {3}, 0)", exportID, i.ID, i.Quantity, i.Price)))
            );
            sql += String.Format(
                @"select {0}", exportID);
            return Query<int>(new DbQuery(userID, employeeID, action, sql, true, exportID), out queryResult).FirstOrDefault();
        }
        public static int SaveOrder(int userID, int employeeID, int bussinessID, OrderRecord record)
        {
            QueryOutput queryResult;
            var exportID = "@ExportID";
            var orderID = "@OrderID";
            var clientID = "@ClientID";
            var now = DateTime.Now;
            var discount = record.Discount;
            var paid = record.Paid + record.ReturnValue;
            var action = DbAction.Order.Create;
            if (record.ID > 0)
                action = DbAction.Order.Modify;
            var sql = String.Format(
                @"declare {0} int = null declare {1} int = null declare {2} int = null
                declare @status nvarchar(10) = null",
                orderID, exportID, clientID);
            var code = record.Code;
            if (record.ID <= 0)
                code = NewUniqueCode(userID, employeeID, bussinessID, "Order");
            code = String.Format("'{0}'", code);
            if ((!record.ClientID.HasValue || record.ClientID <= 0) && !String.IsNullOrEmpty(record.ClientName))
            {
                var clientCode = String.IsNullOrEmpty(record.ClientCode) ? NewUniqueCode(userID, employeeID, bussinessID, "Client", 3, null) : record.ClientCode;
                sql += String.Format(
                    @" set {5} = (select top 1 ID from Client where BussinessID = {0} and Code = '{1}' and Status = 'active')
                    if ({5} is null) begin
                    declare @client table (ID int)
                    insert Client(BussinessID, Code, Name, Phone, Address, Point, Status)
                    output inserted.ID into @client
                    values ({0}, '{1}', N'{2}', '{3}', N'{4}', 0, 'active')
                    set {5} = (select top 1 ID from @client) end",
                    bussinessID, clientCode, record.ClientName, record.ClientPhone, record.ClientAddress, clientID);
            }
            else
            {
                sql += String.Format(" set {0} = {1}", clientID, record.ClientID.DbValue());
            }
            if (record.ID > 0)
            {
                sql += String.Format(
                    @" set {0} = {1}
                    set @status = (select Status from SKtimeManagement..[Order] where ID = {0})
                    declare @point int = (select sum(Point) from ExportProduct where OrderID = {0} and Returned = 0)
                    update Client set Point = (case when Point - @point < 0 then 0 else Point - @point end) where ID = {2}",
                    orderID, record.ID, clientID);
                if (!record.Return)
                    sql += String.Format(" delete ExportProduct where OrderID = {0} and Returned = 0", orderID);
                else
                    sql += String.Format(" update ExportProduct set Returned = 1 where OrderID = {0}", orderID);
                //var isRefund = record.Status == OrderStatus.Refunded;
                //sql += String.Format(@" if (isnull((select top 1 Status from SKtimeManagement..[Order] where ID = {0}), '') != N'{1}')
                //begin
                //    update q set q.Quantity = {2} from ProductQuantity q join ExportProduct o on q.ProductID = o.ProductID where o.OrderID = {0}
                //    {3}
                //end",
                //record.ID, record.Status,
                //isRefund ? "q.Quantity + o.Quantity" : "case when q.Quantity - o.Quantity < 0 then 0 else q.Quantity - o.Quantity end",
                //record.ClientID.HasValue ?
                //    String.Format(
                //        @"declare @point int = (select isnull(sum(Point), 0) from ExportProduct where OrderID = {0})
                //        update c set c.Point = {2} from Client c where c.ID = {1}",
                //        record.ID, record.ClientID, isRefund ? "case when c.Point - @point < 0 then 0 else c.Point - @point end" : "c.Point + @point") : "");
                sql += String.Format(
                    @" update SKtimeManagement..[Order] 
                    set Receipt = '{0}', Note = N'{2}', Status = N'{3}', DeliveryID = {5},
                    PayMethod = N'{6}', ClientID = {7}, Discount = {8}, Total = {9}, Returned = {10} {11}
                    where ID = {4}",
                    record.Receipt, paid, record.Note, record.Status, record.ID, record.DeliveryID.DbValue(), record.PayMethod, 
                    clientID, discount, Math.Round(record.Items.Sum(i => i.Price * i.Quantity), 2), record.Return ? 1 : 0, 
                    record.NewEmployeeID.HasValue ? String.Format(", EmployeeID = {0}", record.NewEmployeeID.Value) : "");
            }
            else
            {
                paid = record.Transactions != null ? record.Transactions.Sum(i => i.Amount) : 0;
                sql += String.Format(
                    @" declare @ID table (ID int)
                    insert into SKtimeManagement..[Order](EmployeeID, BussinessID, ClientID, Code, SubmitDate, Receipt, Total, Discount, Paid, Note, PayMethod, DeliveryID, WarehouseID, Status) 
                    output inserted.ID into @ID
                    values ({0}, {1}, {2}, {3}, {4}, '{5}', {6}, {7}, {8}, N'{9}', N'{11}', {12}, {13}, '')
                    set {10} = (select top 1 ID from @ID) delete @ID",
                    employeeID, bussinessID, clientID,
                    code, record.SubmitDate.DbValue(), record.Receipt,
                    Math.Round(record.Items.Sum(i => i.Price * i.Quantity), 2), discount,
                    paid, record.Note, orderID, record.PayMethod, record.DeliveryID.DbValue(), record.WarehouseID
                );
                if (record.Transactions != null)
                {
                    foreach (var tran in record.Transactions)
                    {
                        sql += tran.AddTransactionQuery(employeeID, TransactionClass.Order, ref orderID);
                    }
                }
            }
            if (record.ToExport)
            {
                if (record.ID > 0)
                    code = String.Format("(select top 1 Code from SKtimeManagement..[Order] where ID = {0})", orderID);
                sql += String.Format(
                    @" set {4} = (select top 1 ID from Export where Code = {2})
                    if ({4} is null)
                    begin
                        declare @export table (ID int)
                        insert Export(EmployeeID, BussinessID, Code, SubmitDate, WarehouseID, Status)
                        output inserted.ID into @export
                        values ({0}, {1}, {2}, '{3}', {5}, N'{6}')
                        set {4} = (select top 1 ID from @export) delete @export
                    end",
                    employeeID, bussinessID, code, now.ToString(Constants.DatabaseDatetimeString), exportID, record.WarehouseID, ExportStatus.Exported);
            }
            sql += String.Format(
                @" insert into ExportProduct(OrderID, ExportID, ProductID, Quantity, Price, Point) values {0}",
                String.Join(",", record.Items.Select(i => String.Format("({0}, {1}, {2}, {3}, {4}, {5})", orderID, exportID, i.ID, i.Quantity, i.Price, i.Point)))
            );
            sql += String.Format(
                @" update Client set Point = Point + {1} where ID = {0}
                delete ClientType where ClientID = {0}
                declare @type int = (select top 1 ID from Type where BussinessID = {7} and Status = 'active' and Revenue <= (select sum(Total - Discount) from [Order] where ClientID = {0} and Status <> N'{8}') order by Revenue desc, DiscountValue)
                if (@type is not null) begin
                    insert into ClientType(ClientID, TypeID, CreateDate, Status) 
                    values({0}, @type, getdate(), 'active')
                end
                select {4}",
                clientID, record.Items.Sum(i => i.Point), record.Status, OrderStatus.Refunded, orderID, record.WarehouseID,
                record.Status == OrderStatus.Refunded ? "q.Returned + p.Quantity" : "(case when q.Returned - p.Quantity < 0 then 0 else q.Returned - p.Quantity end)",
                bussinessID, OrderStatus.Refunded);
            var result = Query<int>(new DbQuery(userID, employeeID, action, sql, true, orderID, String.Format("(select top 1 Code + ' (Kho ' + (select Name from Warehouse where ID = WarehouseID) + '-' + format(Total, 'c0', 'vi-vn') + ')' from [Order] where ID = {0})", orderID)), out queryResult).FirstOrDefault();
            return result;
        }
        public static bool Remove(int userID, int employeeID, int recordID, bool log = false)
        {
            QueryOutput queryResult;
            var query = String.Format(
                @"update Export set Removed = 1 where ID = {0} update Import set Status = 'remove' where ExportID = {0}", recordID, ExportStatus.Exported);
            return Execute(new DbQuery(userID, employeeID, DbAction.Export.Remove, query, log, recordID), out queryResult);
        }
        public static bool RemoveOrder(int userID, int employeeID, int recordID, bool log = false)
        {
            QueryOutput queryResult;
            var query = String.Format(
                @"update [Order] set Removed = 1 where ID = {0} update Export set Removed = 1 where ID = (select top 1 ExportID from ExportProduct where OrderID = {0})
                declare @point int = (select sum(Point) from ExportProduct where OrderID = {0})
                update Client set Point = (case when Point - @point < 0 then 0 else Point - @point end) where ID = (select ClientID from [Order] where ID = {0})", recordID, OrderStatus.Refunded);
            return Execute(new DbQuery(userID, employeeID, DbAction.Order.Remove, query, log, recordID), out queryResult);
        }
    }
}