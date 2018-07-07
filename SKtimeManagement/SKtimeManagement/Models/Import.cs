using Dapper;
using NPOI.HSSF.UserModel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;

namespace SKtimeManagement
{
    public class ImexItemList
    {
        public ImexItemList(int recordID, int warehouseID, List<ImexItem> data)
        {
            RecordID = recordID;
            WarehouseID = warehouseID;
            Data = data;
            if (Data == null)
                Data = new List<ImexItem>();
        }
        public int RecordID { get; set; }
        public int WarehouseID { get; set; }
        public List<ImexItem> Data { get; set; }
    }
    public class ImexItem : BaseModel
    {
        public ImexItem() : base() { }
        public ImexItem(ProductInfo product, int whid) : base()
        {
            ID = product.ID;
            Code = product.Code;
            Name = product.Name;
            Price = product.Price;
            Quantity = 1;
            WarehouseID = whid;
        }
        public ImexItem(ExportProduct product, int warehouseID = 0,  string warehouseName = "")
        {
            ID = product.ProductID;
            WarehouseID = warehouseID;
            WarehouseName = warehouseName;
            Code = product.Code;
            Name = product.ProductName;
            Price = product.Price;
            Point = product.Point;
            Quantity = product.Quantity;
            Maximum = product.Maximum > 0 ? product.Maximum : product.Quantity;
        }
        public int ID { get; set; }
        public int WarehouseID { get; set; }
        public string WarehouseName { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        public string Url { get; set; }
        public string Image { get; set; }
        public decimal Price { get; set; }
        public string Unit { get; set; }
        public string PriceString
        {
            get
            {
                return Price.GetCurrencyString();
            }
        }
        public int Maximum { get; set; }
        public string MaximumString { get { return Maximum.GetCurrencyString(); } }
        public int Quantity { get; set; }
        public string QuantityString { get { return Quantity.GetCurrencyString(); } }
        public int Point { get; set; }
        public bool Return { get; set; }
        public string PointString { get { return Point.GetCurrencyString(); } }
        public string Total
        {
            get
            {
                return (Price * Quantity).GetCurrencyString();
            }
        }
        public string JsObject
        {
            get
            {
                return String.Format(
                    "{{ ID:{0}, Name: '{1}', WarehouseID:{2}, WarehouseName:'{3}', Quantity:{4}, Price:{5}, PriceString:'{6}', Total:'{7}', Code:'{8}', Maximum:{9}, MaximumString:'{10}', Point:{11}, PointString:'{12}', Image: '{13}', Returned: {14}, QuantityString: '{15}' }}",
                    ID, Name, WarehouseID, WarehouseName, Quantity, Price, PriceString, Total, Code, Maximum, MaximumString, Point, PointString, Image, Return ? "true" : "false", QuantityString);
            }
        }
        public ImexItem SetMaximum(int? qty = null)
        {
            Maximum = qty.HasValue ? qty.Value : Quantity;
            return this;
        }
        public static object[] ToJson(IEnumerable<ImexItem> list)
        {
            return list.Select(i => new {
                ID = i.ID,
                Code = i.Code,
                Name = i.Name,
                Image = i.Image,
                WarehouseID = i.WarehouseID,
                WarehouseName = i.WarehouseName,
                Maximum = i.Maximum,
                MaximumString = i.MaximumString,
                Quantity = i.Quantity,
                QuantityString = i.QuantityString,
                Price = i.Price,
                PriceString = i.PriceString,
                Point = i.Point,
                PointString = i.PointString,
                Total = i.Total
            }).ToArray();
        }
        public static int Available(int productID, int recordID, TransactionClass cls)
        {
            try
            {
                using (var con = Repo.DB.SKtimeManagement)
                {
                    var table = "";
                    switch (cls)
                    {
                        case TransactionClass.Order: table = "[Order]"; break;
                        case TransactionClass.Repair: table = "Repair"; break;
                        case TransactionClass.Warranty: table = "Warranty"; break;
                        default: break;
                    }
                    var query = String.Format(
                        @"declare @warehouseID int = (select WarehouseID from {1} where ID = {2})
                        select sum(isnull(ip.Quantity, 0)) - isnull((
                            select sum(ep.Quantity) 
                            from ExportProduct ep left join Export e on e.ID = ep.ExportID and e.Removed = 0 and e.Status in (N'Đã xuất', N'Đã chuyển') 
                            where ep.Returned = 0 and ep.ProductID = p.ID and e.WarehouseID = w.ID), 0) as [Quantity]
                        from Product p
                            join ImportProduct ip on p.ID = ip.ProductID
                            join Import i on i.ID = ip.ImportID and i.Status = 'active' 
                            join Warehouse w on w.Status = 'active' and w.ID = i.WarehouseID
                        where p.Status = 'active' and p.ID = {0} and w.ID = @warehouseID
                        group by p.ID, p.Code, p.Name, p.Image, p.Price, p.Point, w.Name, w.ID
                        order by p.Name", productID, table, recordID);
                    return con.Query<int>(query).FirstOrDefault();
                }
            }
            catch { }
            return 0;
        }
        public static List<ImexItem> Unavailable(int userID, int employeeID, int warehouseID, string action, ImexItem[] items, int orderID = 0, int exportID = 0)
        {
            QueryOutput queryResult;
            var sql = String.Format(
                @"select p.ID, p.Code, p.Name, p.Image, p.Price, p.Point, w.Name as [WarehouseName], w.ID as [WarehouseID],
                    sum(isnull(ip.Quantity, 0)) + {1} + {2}
                        - isnull((select sum(ep.Quantity) from ExportProduct ep left join Export e on e.ID = ep.ExportID and e.Removed = 0 and e.Status in (N'Đã xuất', N'Đã chuyển') where ep.Returned = 0 and ep.ProductID = p.ID and e.WarehouseID = w.ID), 0) as [Quantity]
                from Product p
                    join ImportProduct ip on p.ID = ip.ProductID
                    join Import i on i.ID = ip.ImportID and i.Status = 'active' 
                    join Warehouse w on w.Status = 'active' and w.ID = i.WarehouseID
                where p.Status = 'active' and ({0})
                group by p.ID, p.Code, p.Name, p.Image, p.Price, p.Point, w.Name, w.ID
                order by p.Name",
                String.Join(" or ", items.GroupBy(i => i.ID).Select(g => String.Format("p.ID = {0} and w.ID = {1}", g.FirstOrDefault().ID, warehouseID))),
                orderID > 0 ? String.Format("isnull((select Quantity from ExportProduct where Returned = 0 and OrderID = {0} and ProductID = p.ID), 0)", orderID) : "0",
                exportID > 0 ? String.Format("isnull((select Quantity from ExportProduct where ExportID = {0} and ProductID = p.ID), 0)", exportID) : "0");
            var products = Query<ImexItem>(new DbQuery(userID, employeeID, action, sql), out queryResult);
            return products.Where(p => items.FirstOrDefault(i => i.ID == p.ID && warehouseID == p.WarehouseID) == null || items.FirstOrDefault(i => i.ID == p.ID && warehouseID == p.WarehouseID).Quantity > p.Quantity).ToList();
        }
        public static List<ImexItem> Unremovable(int userID, int employeeID, string action, int importID)
        {
            QueryOutput queryResult;
            var sql = String.Format(
                @"select p.ID, p.Code, p.Name, p.Image, p.Price, p.Point, w.Name as [WarehouseName], w.ID as [WarehouseID],
                    sum(isnull(ip.Quantity, 0)) - isnull((select sum(Quantity) from ImportProduct where ImportID = {0} and ProductID = p.ID), 0)
                    - isnull((select sum(ep.Quantity) from ExportProduct ep left join Export e on e.ID = ep.ExportID and e.Removed = 0 and e.Status in (N'Đã xuất', N'Đã chuyển') where ep.Returned = 0 and ep.ProductID = p.ID and e.WarehouseID = w.ID), 0) as [Quantity]
                from Product p
                    join ImportProduct ip on p.ID = ip.ProductID
                    join Import i on i.ID = ip.ImportID and i.Status = 'active' 
                    join Warehouse w on w.Status = 'active' and w.ID = i.WarehouseID
                where p.Status = 'active' and w.ID = (select WarehouseID from Import where ID = {0}) and p.ID in (select ProductID from ImportProduct where ImportID = {0})
                group by p.ID, p.Code, p.Name, p.Image, p.Price, p.Point, w.Name, w.ID
                order by p.Name", importID);
            var products = Query<ImexItem>(new DbQuery(userID, employeeID, action, sql), out queryResult);
            return products.Where(i => i.Quantity < 0).ToList();
        }
        public static List<ImexItem> FindAll(int userID, int employeeID, string action, int bussinessID, ProductFilter filter = null, bool log = false)
        {
            QueryOutput queryResult;
            var conditions = new List<string>();
            if (filter != null)
            {
                if (!String.IsNullOrEmpty(filter.Code))
                    conditions.Add(String.Format("and p.Code like N'%{0}%'", filter.Code));
                if (!String.IsNullOrEmpty(filter.Name))
                    conditions.Add(String.Format("and p.Name like N'%{0}%'", filter.Name));
                if (filter.SupplierID.HasValue)
                    conditions.Add(String.Format("and p.SupplierID = {0}", filter.SupplierID.DbValue()));
            }
            return Query<ImexItem>(new DbQuery(userID, employeeID, action, 
                String.Format(@"select top 100 p.ID, p.Code, p.Name, p.Image, p.Price, p.Point, {3} as [WarehouseID]
                                from Product p {2}
                                where p.Status = 'active' and p.BussinessID = {0} {1} order by p.Name", 
                                bussinessID, String.Join(" ", conditions),
                                filter != null && filter.TagID.HasValue ? String.Format("join ProductTag pt on p.ID = pt.ProductID and pt.TagID = {0}", filter.TagID.Value) : "",
                                filter != null && filter.WarehouseID.HasValue ? filter.WarehouseID.Value : 0), log), out queryResult);
        }
        public static List<ImexItem> Find(int userID, int employeeID, string action, int bussinessID, ProductFilter filter = null, bool log = false)
        {
            QueryOutput queryResult;
            var conditions = new List<string>();
            if (filter != null)
            {
                if (filter.ProductID.HasValue)
                    conditions.Add(String.Format("and p.ID = {0}", filter.ProductID.Value));
                if (!String.IsNullOrEmpty(filter.Code))
                    conditions.Add(String.Format("and p.Code like N'%{0}%'", filter.Code));
                if (!String.IsNullOrEmpty(filter.Name))
                    conditions.Add(String.Format("and p.Name like N'%{0}%'", filter.Name));
                if (filter.SupplierID.HasValue)
                    conditions.Add(String.Format("and p.SupplierID = {0}", filter.SupplierID.DbValue()));
                if (filter.WarehouseID.HasValue && filter.WarehouseID > 0)
                    conditions.Add(String.Format("and w.ID = {0}", filter.WarehouseID.DbValue()));
            }
            var strSql = String.Format(
                @"select top 100 p.ID, p.Code, p.Name, p.Image, p.Price, p.Point, w.Name as [WarehouseName], w.ID as [WarehouseID],
                    sum(isnull(ip.Quantity, 0)) - isnull((select sum(ep.Quantity) from ExportProduct ep left join Export e on e.ID = ep.ExportID and e.Removed = 0 and e.Status in (N'Đã xuất', N'Đã chuyển') where ep.Returned = 0 and ep.ProductID = p.ID and e.WarehouseID = w.ID), 0) as [Quantity]
                from Product p
                    join ImportProduct ip on p.ID = ip.ProductID
                    join Import i on i.ID = ip.ImportID and i.Status = 'active' 
                    join Warehouse w on w.Status = 'active' and w.ID = i.WarehouseID {2} {3}
                where p.Status = 'active' and p.BussinessID = {0} {1} 
                group by p.ID, p.Code, p.Name, p.Image, p.Price, p.Point, w.Name, w.ID
                having sum(isnull(ip.Quantity, 0)) - isnull((select sum(ep.Quantity) from ExportProduct ep left join Export e on e.ID = ep.ExportID and e.Removed = 0 and e.Status in (N'Đã xuất', N'Đã chuyển') where ep.Returned = 0 and ep.ProductID = p.ID and e.WarehouseID = w.ID), 0) > 0
                order by p.Name",
                bussinessID, String.Join(" ", conditions),
                filter != null && filter.TagID.HasValue ? String.Format("join ProductTag pt on p.ID = pt.ProductID and pt.TagID = {0}", filter.TagID.Value) : "",
                filter != null && filter.ForRepair.HasValue ? String.Format("join ProductTag pt on p.ID = pt.ProductID join Tag t on t.ID = pt.TagID and t.ForRepair = {0}", filter.ForRepair.DbValue()) : "");
            var products = Query<ImexItem>(new DbQuery(userID, employeeID, action,
                strSql, log), out queryResult);
            return products.Select(i => i.SetMaximum()).ToList();
        }
        public static ImexItem Get(int userID, int employeeID, string action, int productID, int whid, bool log = false)
        {
            QueryOutput queryResult;
            var product = Query<ImexItem>(new DbQuery(userID, employeeID, action,
                String.Format(
                    @"select top 1 p.ID, p.Code, p.Name, p.Image, p.Price, p.Point, w.Name as [WarehouseName], w.ID as [WarehouseID],
                        sum(isnull(ip.Quantity, 0)) - isnull((select sum(ep.Quantity) from ExportProduct ep left join Export e on e.ID = ep.ExportID and e.Removed = 0 and e.Status in (N'Đã xuất', N'Đã chuyển') where ep.Returned = 0 and ep.ProductID = p.ID and e.WarehouseID = w.ID), 0) as [Quantity]
                    from Product p
                        join ImportProduct ip on p.ID = ip.ProductID
                        join Import i on i.ID = ip.ImportID and i.Status = 'active' 
                        join Warehouse w on w.Status = 'active' and w.ID = i.WarehouseID
                    where p.Status = 'active' and p.ID = {0} and w.ID = {1}
                    group by p.ID, p.Code, p.Name, p.Image, p.Price, p.Point, w.Name, w.ID
                    having sum(isnull(ip.Quantity, 0)) - isnull((select sum(ep.Quantity) from ExportProduct ep left join Export e on e.ID = ep.ExportID and e.Removed = 0 and e.Status in (N'Đã xuất', N'Đã chuyển') where ep.Returned = 0 and ep.ProductID = p.ID and e.WarehouseID = w.ID), 0) > 0
                    order by p.Name", productID, whid), log), out queryResult).FirstOrDefault();
            return product.SetMaximum();
        }
        public static ImexItem[] Get(int userID, int employeeID, int warehouseID, IEnumerable<ImexItem> items, bool log = false)
        {
            QueryOutput queryResult;
            var products = Query<ImexItem>(new DbQuery(userID, employeeID, DbAction.Product.View,
                String.Format(
                    @"select p.ID, p.Code, p.Name, p.Image, p.Price, p.Point, w.Name as [WarehouseName], w.ID as [WarehouseID],
                        sum(isnull(ip.Quantity, 0)) - isnull((select sum(ep.Quantity) from ExportProduct ep left join Export e on e.ID = ep.ExportID and e.Removed = 0 and e.Status in (N'Đã xuất', N'Đã chuyển') where ep.Returned = 0 and ep.ProductID = p.ID and e.WarehouseID = w.ID), 0) as [Quantity]
                    from Product p
                        join ImportProduct ip on p.ID = ip.ProductID
                        join Import i on i.ID = ip.ImportID and i.Status = 'active' 
                        join Warehouse w on w.Status = 'active' and w.ID = i.WarehouseID
                    where p.Status = 'active' and ({0})
                    group by p.ID, p.Code, p.Name, p.Image, p.Price, p.Point, w.Name, w.ID
                    having sum(isnull(ip.Quantity, 0)) - isnull((select sum(ep.Quantity) from ExportProduct ep left join Export e on e.ID = ep.ExportID and e.Removed = 0 and e.Status in (N'Đã xuất', N'Đã chuyển') where ep.Returned = 0 and ep.ProductID = p.ID and e.WarehouseID = w.ID), 0) > 0
                    order by p.Name", String.Join(" or ", items.Select(i => String.Format("(p.Code = '{0}' and w.ID = {1})", i.Code, warehouseID)))), log), out queryResult).ToArray();
            return products;
        }
        public static ImexItem[] Get(int userID, int employeeID, IEnumerable<ImexItem> items, bool log = false)
        {
            QueryOutput queryResult;
            var sql = "declare @items table (ID int, Code nvarchar(50), Name nvarchar(100), Price decimal(18,0), Point int, Quantity int, WarehouseID int, WarehouseName nvarchar(50))";
            sql += String.Format(
                @" insert into @items(ID, Code, Name, Price, Point, Quantity, WarehouseName, WarehouseID)
                select p.ID, p.Code, p.Name, p.Price, p.Point, 0, '', 0
                from Product p
                where p.Status = 'active' and ({0})", String.Join(" or ", items.Select(i => String.Format("p.Code = '{0}'", i.Code))));
            sql += " select * from @items";
            return Query<ImexItem>(new DbQuery(userID, employeeID, DbAction.Product.View, sql, log), out queryResult).ToArray();
        }
        public static List<ImexItem> Read(int userID, int employeeID, FileInfo fileInfo, int bussinessID, int warehouseID, bool getDbPrice = false)
        {
            var result = new List<ImexItem>();
            HSSFWorkbook hssfwb;
            using (FileStream file = new FileStream(fileInfo.FullName, FileMode.Open, FileAccess.Read))
            {
                hssfwb = new HSSFWorkbook(file);
            }

            var sheet = hssfwb.GetSheetAt(0);
            if (sheet.LastRowNum > 0)
            {
                for (var i = 1; i <= sheet.LastRowNum; i++)
                {
                    var row = sheet.GetRow(i);
                    try
                    {
                        var item = new ImexItem();
                        item.Code = row.GetCellValue<string>(0);
                        item.WarehouseID = warehouseID;
                        //item.WarehouseName = row.GetCellValue<string>(1);
                        item.Price = row.GetCellValue<decimal>(2);
                        item.Quantity = row.GetCellValue<int>(3);
                        result.Add(item);
                    }
                    catch { }
                }
            }

            if (result.Count > 0)
            {
                var dbItems = ImexItem.Get(userID, employeeID, warehouseID, result);
                foreach (var dbItem in dbItems)
                {
                    var item = result.FirstOrDefault(i => i.Code == dbItem.Code && i.WarehouseID == dbItem.WarehouseID);
                    item.ID = dbItem.ID;
                    item.Code = dbItem.Code;
                    item.Name = dbItem.Name;
                    item.Image = dbItem.Image;
                    item.WarehouseID = dbItem.WarehouseID;
                    item.Maximum = dbItem.Quantity;
                    item.Point = dbItem.Point;
                    if (getDbPrice)
                        item.Price = dbItem.Price;
                }
            }
            return result;
        }
        public static List<ImexItem> Read(int userID, int employeeID, FileInfo fileInfo, int bussinessID)
        {
            var result = new List<ImexItem>();
            HSSFWorkbook hssfwb;
            using (FileStream file = new FileStream(fileInfo.FullName, FileMode.Open, FileAccess.Read))
            {
                hssfwb = new HSSFWorkbook(file);
            }

            var sheet = hssfwb.GetSheetAt(0);
            if (sheet.LastRowNum > 0)
            {
                var whs = new List<string>();
                for (var i = 1; i <= sheet.LastRowNum; i++)
                {
                    var row = sheet.GetRow(i);
                    try
                    {
                        var wh = row.GetCellValue<string>(2);
                        if (!String.IsNullOrEmpty(wh) && !whs.Contains(wh))
                            whs.Add(wh);
                        var item = new ImexItem();
                        item.Code = row.GetCellValue<string>(0);
                        //item.WarehouseID = warehouseID;
                        item.WarehouseName = wh;
                        item.Price = row.GetCellValue<decimal>(3);
                        item.Quantity = row.GetCellValue<int>(4);
                        result.Add(item);
                    }
                    catch { }
                }
                var warehouses = WarehouseInfo.Find(userID, employeeID, whs);
                foreach (var item in result)
                {
                    if (warehouses.FirstOrDefault(i => i.Name == item.WarehouseName) != null)
                        item.WarehouseID = warehouses.FirstOrDefault(i => i.Name == item.WarehouseName).ID;
                }
                if (result.Count > 0)
                {
                    var dbItems = ImexItem.Get(userID, employeeID, result);
                    foreach (var dbItem in dbItems)
                    {
                        foreach (var item in result.Where(i => i.Code == dbItem.Code))
                        {
                            item.ID = dbItem.ID;
                            item.Code = dbItem.Code;
                            item.Name = dbItem.Name;
                            item.Price = dbItem.Price;
                            //item.Quantity = 1;
                        }
                    }
                }
                result = result.Where(i => i.ID > 0 && i.WarehouseID > 0).ToList();
            }
            return result;
        }
    }
    public class ImportList
    {
        public ImportList(ImexFilter filter = null, string message = "")
        {
            List = new List<ImportRecord>();
            Filter = filter != null ? filter : new ImexFilter();
            Message = message;
        }
        public List<ImportRecord> List { get; set; }
        public ImexFilter Filter { get; set; }
        public ImportRecord Current { get; set; }
        public bool Result { get; set; }
        public string Message { get; set; }
    }
    public class ImportRecord
    {
        public int ID { get; set; }
        public int WarehouseID { get; set; }
        public string WarehouseName { get; set; }
        public string WarehouseAddress { get; set; }
        public int EmployeeID { get; set; }
        public string EmployeeName { get; set; }
        public string Code { get; set; }
        public DateTime SubmitDate { get; set; }
        public string SubmitDateString { get { return SubmitDate.ToString(Constants.DateTimeString); } }
        public string Note { get; set; }
        public string Receipt { get; set; }
        public List<ImexItem> Products { get; set; }
    }
    public class Import : BaseModel
    {
        public Import(string employeeName, List<WarehouseInfo> warehouses, int? warehouseID = null, ImexItem[] selected = null, string note = "", string message = "") : base()
        {
            EmployeeName = employeeName;
            Warehouses = warehouses;
            SelectedWarehouse = warehouseID;
            if (warehouseID == 0 && warehouses.Count >= 1)
                SelectedWarehouse = warehouses.FirstOrDefault().ID;
            Data = new List<ImexItem>();
            if (selected == null)
                Selected = new ImexItem[] { };
            else
                Selected = selected;
            Note = note;
            Message = message;
            Filter = new ProductFilter();
        }
        public int ID { get; set; }
        public string Code { get; set; }
        public string EmployeeName { get; set; }
        public int? SelectedWarehouse { get; set; }
        public List<WarehouseInfo> Warehouses { get; set; }
        public ProductFilter Filter { get; set; }
        public List<ImexItem> Data { get; set; }
        public ImexItem[] Selected { get; set; }
        public string Receipt { get; set; }
        public string Note { get; set; }
        public string Message { get; set; }
        public static ImportRecord Get(int userID, int employeeID, int recordID, bool log = false)
        {
            QueryOutput queryResult;
            var result = Query<ImportRecord>(new DbQuery(userID, employeeID, DbAction.Import.View, 
                String.Format(
                    @"select top 100 i.*, e.Name as [EmployeeName], w.Name as [WarehouseName], w.Address as [WarehouseAddress] 
                    from Import i 
                        join Warehouse w on i.WarehouseID = w.ID and w.Status = 'active' and ((select Username from Login where ID = {1}) = 'admin' or w.ID in (select WarehouseID from LoginWarehouse where LoginID = {1}))
                        join Employee e on i.EmployeeID = e.ID 
                    where i.Status = 'active' and i.ID = {0} order by i.ID desc", 
                recordID, userID), log), out queryResult).FirstOrDefault();
            result.Products = Query<ImexItem>(new DbQuery(userID, employeeID, DbAction.Import.View, 
                String.Format("select (select WarehouseID from Import where ID = {0}) as [WarehouseID], i.Price, i.Quantity, p.ID, p.Code, p.Name, p.Unit from ImportProduct i join Product p on i.ProductID = p.ID where i.ImportID = {0}", 
                recordID)), out queryResult).ToList();
            return result;
        }
        public static List<ImportRecord> History(int userID, int employeeID, int bussinessID, ImexFilter filter = null, bool log = false)
        {
            QueryOutput queryResult;
            var conditions = new List<string>();
            if (filter != null)
            {
                if (filter.WarehouseID.HasValue)
                    conditions.Add(String.Format("and i.WarehouseID = {0}", filter.WarehouseID.DbValue()));
                if (filter.EmployeeID.HasValue)
                    conditions.Add(String.Format("and i.EmployeeID = {0}", filter.EmployeeID.DbValue()));
                if (!String.IsNullOrEmpty(filter.Code))
                    conditions.Add(String.Format("and i.Code like N'%{0}%'", filter.Code));
                if (!String.IsNullOrEmpty(filter.Receipt))
                    conditions.Add(String.Format("and i.Receipt like N'%{0}%'", filter.Receipt));
                if (filter.From.HasValue)
                    conditions.Add(String.Format("and i.SubmitDate >= '{0}'", filter.From.Value.ToString(Constants.DatabaseDatetimeString)));
                if (filter.To.HasValue)
                    conditions.Add(String.Format("and i.SubmitDate <= '{0}'", filter.To.Value.ToString(Constants.DatabaseDatetimeString)));
            }
            return Query<ImportRecord>(new DbQuery(userID, employeeID, DbAction.Import.View, 
                String.Format(
                    @"select top 100 i.*, e.Name as [EmployeeName], w.Name as [WarehouseName] 
                    from Import i 
                        join Warehouse w on i.WarehouseID = w.ID and w.Status = 'active' and ((select Username from Login where ID = {2}) = 'admin' or w.ID in (select WarehouseID from LoginWarehouse where LoginID = {2}))
                        join Employee e on i.EmployeeID = e.ID 
                    where i.Status = 'active' and i.BussinessID = {0} {1} order by i.ID desc", 
                bussinessID, String.Join(" ", conditions), userID), log), out queryResult);
        }
        public static int? Submit(int recordID, int userID, int employeeID, int bussinessID, int warehouseID, ImexItem[] list, string note = "", string receipt = "")
        {
            QueryOutput queryResult;
            foreach (var item in list)
            {
                if (item.ID <= 0)
                    return null;
                else if (item.Quantity <= 0)
                    return null;
            }
            var now = DateTime.Now;
            var sql = "";
            var action = DbAction.Import.Create;
            var id = recordID.ToString();
            if (recordID > 0)
            {
                action = DbAction.Import.Modify;
                sql = String.Format(
                    @"declare @id int = {3}
                    delete ImportProduct where ImportID = {3}
                    update Import set EmployeeID = {0}, Note = N'{1}', WarehouseID = {2}, Receipt = N'{4}' where ID = {3}", 
                    employeeID, note, warehouseID, recordID, receipt);
            }
            else
            {
                sql = String.Format(
                    @"declare @import table (ID int)
                    insert Import(EmployeeID, BussinessID, Code, SubmitDate, Note, WarehouseID, Status, Receipt) output inserted.ID into @import values ({0}, {1}, '{2}', '{3}', N'{4}', {5}, 'active', N'{6}')
                    declare @id int = (select top 1 ID from @import)",
                    employeeID, bussinessID, NewUniqueCode(userID, employeeID, bussinessID, "Import"), now.ToString(Constants.DatabaseDatetimeString), note, warehouseID, receipt
                );
                id = "@id";
            }
            sql += String.Format(" insert into ImportProduct(ImportID, ProductID, Quantity, Price) values {0}", String.Join(",", list.Select(i => String.Format("(@id, {0}, {1}, {2})", i.ID, i.Quantity, i.Price))));
            sql += " select @id";
            return Query<int?>(new DbQuery(userID, employeeID, action, sql, true, id), out queryResult).FirstOrDefault();
        }
        public static bool Remove(int userID, int employeeID, int recordID, bool log = false)
        {
            QueryOutput queryResult;
            var query = String.Format(@"update Import set Status = 'remove' where ID = {0}", recordID);
            return Execute(new DbQuery(userID, employeeID, DbAction.Import.Remove, query, log, recordID), out queryResult);
        }
    }
}