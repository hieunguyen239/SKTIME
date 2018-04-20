using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Dapper;
using System.Web.Mvc;
using System.ComponentModel.DataAnnotations;
using System.IO;
using NPOI.HSSF.UserModel;

namespace SKtimeManagement
{
    public class ProductFilter
    {
        public int? ProductID { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        public int? SupplierID { get; set; }
        public int? WarehouseID { get; set; }
        public int? TagID { get; set; }
        public string Returned { get; set; }
        public string Action { get; set; }
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = Constants.DateTimeString)]
        public DateTime? From { get; set; }
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = Constants.DateTimeString)]
        public DateTime? To { get; set; }
        public bool? ForRepair { get; set; }
    }
    public class ProductList
    {
        public ProductList(string message = "", ProductFilter filter = null)
        {
            Data = new List<ProductInfo>();
            Tags = new List<TagInfo>();
            Filter = filter != null ? filter : new ProductFilter();
            Message = message;
        }
        public List<ProductInfo> Data { get; set; }
        public ProductFilter Filter { get; set; }
        public List<TagInfo> Tags { get; set; }
        public List<WarehouseInfo> Warehouses { get; set; }
        public string Message { get; set; }
    }
    public class FindList
    {
        public FindList(ProductFilter filter = null)
        {
            Data = new List<FindItem>();
            Tags = new List<TagInfo>();
            Filter = filter != null ? filter : new ProductFilter();
        }
        public List<FindItem> Data { get; set; }
        public ProductFilter Filter { get; set; }
        public List<TagInfo> Tags { get; set; }
        public List<WarehouseInfo> Warehouses { get; set; }
    }
    public class FindItem
    {
        public FindItem() { }
        public FindItem(string code, string warehouse, int quantity, ProductInfo product = null)
        {
            ID = product != null ? product.ID : 0;
            Code = code;
            WarehouseName = warehouse;
            Quantity = quantity;
            Exist = false;
        }
        public FindItem(ProductInfo info, int quantity = 0, bool exist = true)
        {
            if (info != null)
            {
                ID = info.ID;
                Code = info.Code;
                Name = info.Name;
                Unit = info.Unit;
                Image = info.Image;
                TagName = info.TagName;
                WarehouseName = info.WarehouseName;
                Max = info.Quantity.HasValue ? info.Quantity.Value : 0;
                Quantity = quantity;
            }
            Exist = exist;
        }
        public int ID { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        public string Unit { get; set; }
        public string Image { get; set; }
        public string TagName { get; set; }
        public string WarehouseName { get; set; }
        public int Max { get; set; }
        public int Quantity { get; set; }
        public bool Exist { get; set; }
        public string Json
        {
            get
            {
                return String.Format("ID: {0}, Code: '{1}', Unit: '{2}', Tag: '{3}', Warehouse: '{4}', Max: {5}, Quantity: {6}, Exist: {7}",
                    ID, Code, Unit, TagName, WarehouseName, Max, Quantity, Exist.JsonValue());
            }
        }
    }
    public class ProductInfo : BaseModel
    {
        public ProductInfo() : base() { }
        public ProductInfo(int bussinessID, string code = "") : base()
        {
            Code = code;
            BussinessID = bussinessID;
        }
        public int ID { get; set; }
        public int BussinessID { get; set; }
        [Required(ErrorMessage = "Mã vạch không được bỏ trống")]
        public string Code { get; set; }
        [Required(ErrorMessage = "Tên sản phẩm không được bỏ trống")]
        public string Name { get; set; }
        public decimal Price { get; set; }
        public string PriceString { get { return Price.GetCurrencyString(); } }
        public string Image { get; set; }
        public string Specs { get; set; }
        public string MadeIn { get; set; }
        public string Type { get; set; }
        public string Engine { get; set; }
        public string Gender { get; set; }
        public string MirrorType { get; set; }
        public string TrapMaterial { get; set; }
        public string CaseMaterial { get; set; }
        public string CaseType { get; set; }
        public string FrontColor { get; set; }
        public string CaseWidth { get; set; }
        public string TrapSize { get; set; }
        public string Diameter { get; set; }
        public string WaterResistance { get; set; }
        public string Functions { get; set; }
        public string Style { get; set; }
        public string Description { get; set; }
        public string OriginalWarranty { get; set; }
        public string BussinessWarranty { get; set; }
        public int? SupplierID { get; set; }
        public string SupplierName { get; set; }
        public string Unit { get; set; }
        public string WarehouseName { get; set; }
        public int StartImportQuantity { get; set; }
        public int StartExportQuantity { get; set; }
        public int StartQuantity { get { return StartImportQuantity - StartExportQuantity; } }
        public string StartQuantityString { get { return StartQuantity.GetCurrencyString(); } }
        public int EndQuantity { get { return StartQuantity + ImportQuantity - ExportQuantity; } }
        public string EndQuantityString { get { return EndQuantity.GetCurrencyString(); } }
        public int? Quantity { get; set; }
        public string QuantityString { get { return Quantity.HasValue ? Quantity.Value.GetCurrencyString() : "0"; } }
        public int Returned { get; set; }
        public string ReturnedString { get { return Returned.GetCurrencyString(); } }
        public int ImportQuantity { get; set; }
        public string ImportQuantityString { get { return ImportQuantity.GetCurrencyString(); } }
        public int ExportQuantity { get; set; }
        public string ExportQuantityString { get { return ExportQuantity.GetCurrencyString(); } }
        public int SaleQuantity { get; set; }
        public string SaleQuantityString { get { return SaleQuantity.GetCurrencyString(); } }
        public List<ProductQuantity> Quantities { get; set; }
        public int Point { get; set; }
        public string PointString { get { return Point.GetCurrencyString(); } }
        public int? TagID { get; set; }
        public string TagName { get; set; }
        public string Json
        {
            get
            {
                return String.Format(
                    @"TagName: '{0}', Code: '{1}', Unit: '{2}', WarehouseName: '{3}', StartQuantityString: '{4}', ImportQuantityString: '{5}', ExportQuantityString: '{6}', EndQuantityString: '{7}', SaleQuantityString: '{8}', ReturnedString: '{9}'",
                    TagName, Code, Unit, WarehouseName, StartQuantityString, ImportQuantityString, ExportQuantityString, EndQuantityString, SaleQuantityString, ReturnedString);
            }
        }
        public static bool NameExist(int userID, int employeeID, int bussinessID, string name)
        {
            QueryOutput queryResult;
            return Query<bool>(new DbQuery(userID, employeeID, DbAction.Product.Create, String.Format("select case when count(Code) > 0 then 1 else 0 end from Product where Status = 'active' and BussinessID = {0} and Name = N'{1}'", bussinessID, name)), out queryResult).FirstOrDefault();
        }
        public bool Save(ModelStateDictionary modelState, int userID, int employeeID, int bussinessID, IEnumerable<int> tags = null, IEnumerable<ProductQuantity> quantities = null)
        {
            if (!Validate(modelState))
            {
                return Result = false;
            }
            if (ID <= 0 && BaseModel.CodeExist(userID, employeeID, bussinessID, Code, "Product"))
                Messages = new List<string>() { "Mã sản phẩm đã được sử dụng" };
            else if (ID <= 0 && NameExist(userID, employeeID, bussinessID, Name))
                Messages = new List<string>() { "Tên sản phẩm đã được sử dụng" };
            else if (Save(userID, employeeID, bussinessID, tags, quantities))
                Messages = new List<string>() { "Lưu thông tin thành công" };
            return Result;
        }
        public bool Save(int userID, int employeeID, int bussinessID, IEnumerable<int> tags = null, IEnumerable<ProductQuantity> quantities = null)
        {
            QueryOutput queryResult;
            var query = "";
            Result = false;
            var id = ID.ToString();
            if (ID > 0)
            {
                query += String.Format(@"update Product 
                                            set Code = N'{0}', Name = N'{1}', Price = {2}, Image = N'{3}',
                                            MadeIn = N'{4}', Description = N'{5}', OriginalWarranty = N'{6}',
                                            BussinessWarranty = N'{7}', SupplierID = {8}, Unit = N'{9}', Point = {10}, Specs = N'{11}',
                                            Type = N'{13}', Engine = N'{14}', Gender = N'{15}', MirrorType = N'{16}', TrapMaterial = N'{17}', CaseMaterial = N'{18}', CaseType = N'{19}',
                                            FrontColor = N'{20}', CaseWidth = N'{21}', TrapSize = N'{22}', Diameter = N'{23}', WaterResistance = N'{24}', Functions = N'{25}', Style = N'{26}'
                                            where ID = {12}",
                                        new object[] {
                                                Code, Name.Replace("'", ""), Price, Image, MadeIn, Description, OriginalWarranty, BussinessWarranty, SupplierID.DbValue(), Unit, Point, Specs, ID,
                                                Type, Engine, Gender, MirrorType, TrapMaterial, CaseMaterial, CaseType, FrontColor, CaseWidth, TrapSize, Diameter, WaterResistance, Functions, Style
                });
                Result = Execute(new DbQuery(userID, employeeID, DbAction.Product.Modify, query, true, id, "Name"), out queryResult);
            }
            else
            {
                query += String.Format(@"declare @ID table (ID int) declare @productID int
                                            insert Product(Code, Name, Price, Image, MadeIn, Description, OriginalWarranty, BussinessWarranty, SupplierID, Unit, Point, BussinessID, Specs, Status,
                                                Type, Engine, Gender, MirrorType, TrapMaterial, CaseMaterial, CaseType, FrontColor, CaseWidth, TrapSize, Diameter, WaterResistance, Functions, Style) 
                                            output inserted.ID into @ID
                                            values (N'{0}', N'{1}', {2}, N'{3}', N'{4}', N'{5}', N'{6}', N'{7}', {8}, N'{9}', {10}, {11}, N'{12}', 'active',
                                                N'{13}', N'{14}', N'{15}', N'{16}', N'{17}', N'{18}', N'{19}', N'{20}', N'{21}', N'{22}', N'{23}', N'{24}', N'{25}', N'{26}')
                                            set @productID = (select top 1 ID from @ID)",
                                        new object[] {
                                                Code, Name.Replace("'", ""), Price, Image, MadeIn, Description, OriginalWarranty, BussinessWarranty, SupplierID.DbValue(), Unit, Point, bussinessID, Specs,
                                                Type, Engine, Gender, MirrorType, TrapMaterial, CaseMaterial, CaseType, FrontColor, CaseWidth, TrapSize, Diameter, WaterResistance, Functions, Style
                });
                if (tags != null)
                    query += String.Format(" insert into ProductTag(ProductID, TagID) values {0}", String.Join(",", tags.Select(i => String.Format("(@productID, {0})", i))));
                if (quantities != null)
                    query += " " + String.Join(" ", quantities.Select(q => String.Format(
                            @"if ((select top 1 ID from ProductQuantity where ProductID = @productID and WarehouseID = {0}) is null)
                                begin
                                insert into ProductQuantity(ProductID, WarehouseID, Quantity) values(@productID, {0}, {1})
                                end
                                else
                                begin
                                update ProductQuantity set Quantity = {1} where ProductID = @productID and WarehouseID = {0}
                                end", q.WarehouseID, q.Quantity)));
                query += " select @productID";
                id = "@productID";
                ID = Query<int>(new DbQuery(userID, employeeID, DbAction.Product.Create, query, true, id, "Name"), out queryResult).FirstOrDefault();
                Result = true;
            }            
            return Result;
        }
        public static bool Import(int userID, int bussinessID, int employeeID, IEnumerable<ProductInfo> products)
        {
            QueryOutput queryResult;
            var query = String.Format(
                @"declare @product table 
                    (Code nvarchar(50), Name nvarchar(100), Price decimal(18, 0), Image nvarchar(50), MadeIn nvarchar(50), Description nvarchar(MAX), OriginalWarranty nvarchar(50), 
                    BussinessWarranty nvarchar(50), SupplierID int, Unit nvarchar(10), Point int, BussinessID int, Specs nvarchar(200), Status nvarchar(10), TagID int,
                    Type nvarchar(50), Engine nvarchar(50), Gender nvarchar(50), MirrorType nvarchar(50), TrapMaterial nvarchar(50), CaseMaterial nvarchar(50),
                    CaseType nvarchar(50), FrontColor nvarchar(50), CaseWidth nvarchar(50), TrapSize nvarchar(50), Diameter nvarchar(50), WaterResistance nvarchar(50),
                    Functions nvarchar(200), Style nvarchar(200))
                insert into @product values {0}
                update p set p.Price = i.Price, p.MadeIn = i.MadeIn, p.Specs = i.Specs, p.Description = i.Description, p.OriginalWarranty = i.OriginalWarranty, p.BussinessWarranty = i.BussinessWarranty,
                    p.Type = i.Type, p.Engine = i.Engine, p.Gender = i.Gender, p.MirrorType = i.MirrorType, p.TrapMaterial = i.TrapMaterial, p.CaseMaterial = i.CaseMaterial, p.CaseType = i.CaseType,
                    p.FrontColor = i.FrontColor, p.CaseWidth = i.CaseWidth, p.TrapSize = i.TrapSize, p.Diameter = i.Diameter, p.WaterResistance = i.WaterResistance, p.Functions = i.Functions, p.Style = i.Style
                from Product p join @product i on p.Code = i.Code and p.Status = 'active' and p.BussinessID = {1}
                delete i from Product p join @product i on p.Code = i.Code and p.Status = 'active' and p.BussinessID = {1}
                insert into Product
                    (Code, Name, Price, Image, MadeIn, Description, OriginalWarranty, BussinessWarranty, SupplierID, Unit, Point, BussinessID, Specs, Status,
                    Type, Engine, Gender, MirrorType, TrapMaterial, CaseMaterial, CaseType, FrontColor, CaseWidth, TrapSize, Diameter, WaterResistance, Functions, Style) 
                select Code, Name, Price, Image, MadeIn, Description, OriginalWarranty, BussinessWarranty, SupplierID, Unit, Point, BussinessID, Specs, Status,
                    Type, Engine, Gender, MirrorType, TrapMaterial, CaseMaterial, CaseType, FrontColor, CaseWidth, TrapSize, Diameter, WaterResistance, Functions, Style
                from @product
                insert into ProductTag(ProductID, TagID) 
                select (select top 1 p.ID from Product p where p.Code = i.Code and p.Status = 'active' and p.BussinessID = 1 order by p.ID desc), i.TagID 
                from @product i where i.TagID is not null",
                String.Join(",", products.Select(product => 
                    String.Format(
                        @"(N'{0}', N'{1}', {2}, N'{3}', N'{4}', N'{5}', N'{6}', N'{7}', {8}, N'{9}', {10}, {11}, N'{12}', 'active', {13},
                        N'{14}', N'{15}', N'{16}', N'{17}', N'{18}', N'{19}', N'{20}', N'{21}', N'{22}', N'{23}', N'{24}', N'{25}', N'{26}', N'{27}')",
                    product.Code, product.Name.Replace("'", ""), product.Price, product.Image, product.MadeIn, product.Description, product.OriginalWarranty, 
                    product.BussinessWarranty, product.SupplierID.DbValue(), product.Unit, product.Point, bussinessID, product.Specs, product.TagID.DbValue(),
                    product.Type, product.Engine, product.Gender, product.MirrorType, product.TrapMaterial, product.CaseMaterial, product.CaseType, product.FrontColor,
                    product.CaseWidth, product.TrapSize, product.Diameter, product.WaterResistance, product.Functions, product.Style))),
                bussinessID);
            return Execute(new DbQuery(userID, employeeID, DbAction.Product.Create, query, true, String.Format("(select top 1 ID from Product where Code = '{0}' order by ID desc)", products.FirstOrDefault().Code), "Name"), out queryResult);
        }
        public static bool Remove(int userID, int employeeID, int productID)
        {
            QueryOutput queryResult;
            return Execute(new DbQuery(userID, employeeID, DbAction.Product.Remove, String.Format("update Product set Status = 'removed' where ID = {0}", productID), true, productID, "Name"), out queryResult);
        }
        public static List<ProductInfo> GetUnassigned(int userID, int employeeID, int bussinessID)
        {
            QueryOutput queryResult;
            return Query<ProductInfo>(new DbQuery(userID, employeeID, DbAction.Product.View, 
                String.Format("select top 100 * from Product where Status = 'active' and BussinessID = {0} and EmployeeID is null order by Name", bussinessID)), out queryResult);
        }
        public static ProductList Find(int userID, int employeeID, int bussinessID, string message = "", ProductFilter filter = null, bool log = false, int? max = 100)
        {
            
            var result = new ProductList(message, filter);
            result.Data = FindList(userID, employeeID, bussinessID, filter, log, max);
            result.Tags = TagInfo.Find(userID, employeeID, bussinessID).Data;
            return result;
        }
        public static List<ProductInfo> FindList(int userID, int employeeID, int bussinessID, ProductFilter filter = null, bool log = false, int? max = 100, string action = null)
        {
            QueryOutput queryResult;
            if (String.IsNullOrEmpty(action))
                action = DbAction.Product.View;
            var conditions = new List<string>();
            if (filter != null)
            {
                if (!String.IsNullOrEmpty(filter.Code))
                    conditions.Add(String.Format("and p.Code like N'%{0}%'", filter.Code));
                if (!String.IsNullOrEmpty(filter.Name))
                    conditions.Add(String.Format("and p.Name like N'%{0}%'", filter.Name));
                if (filter.SupplierID.HasValue)
                    conditions.Add(String.Format("and p.SupplierID = {0}", filter.SupplierID.DbValue()));
                if (filter.TagID.HasValue)
                    conditions.Add(String.Format("and t.ID = {0}", filter.TagID.DbValue()));
                if (filter.Returned == "on")
                    conditions.Add(String.Format("and p.Returned > 0"));
            }
            return Query<ProductInfo>(new DbQuery(userID, employeeID, action,
                String.Format(
                    @"select {2} p.*, t.ID as [TagID], t.Name as [TagName], s.Name as [SupplierName],
                        isnull((select sum(ep.Quantity) from ImportProduct ep left join Import e on e.ID = ep.ImportID and e.Status = 'active' where ep.ProductID = p.ID and e.BussinessID = {0}), 0) - 
                        isnull((select sum(ep.Quantity) from ExportProduct ep left join Export e on e.ID = ep.ExportID and e.Removed = 0 and e.Status in (N'Đã xuất', N'Đã chuyển') where ep.Returned = 0 and ep.ProductID = p.ID and e.BussinessID = {0}), 0) as [Quantity]
                    from Product p
                        left join Supplier s on p.SupplierID = s.ID
                        left join ProductTag pt on p.ID = pt.ProductID and pt.TagID = (select top 1 TagID from ProductTag where ProductID = p.ID {3} order by TagID desc)
                        left join Tag t on pt.TagID = t.ID and t.Status = 'active'
                    where p.Status = 'active' and p.BussinessID = {0} {1} order by Name",
                bussinessID, String.Join(" ", conditions),
                max.HasValue ? String.Format("top {0}", max.Value) : "",
                filter != null && filter.TagID.HasValue ? String.Format("and TagID = {0}", filter.TagID.DbValue()) : ""), log), out queryResult);
        }
        public static ProductList FindAll(int userID, int employeeID, int bussinessID, string message = "", ProductFilter filter = null, bool log = false, int? max = 100)
        {
            QueryOutput queryResult;
            var conditions = new List<string>();
            if (filter != null)
            {
                if (!String.IsNullOrEmpty(filter.Code))
                    conditions.Add(String.Format("and p.Code = '{0}'", filter.Code));
                if (!String.IsNullOrEmpty(filter.Name))
                    conditions.Add(String.Format("and p.Name like N'%{0}%'", filter.Name));
                if (filter.SupplierID.HasValue)
                    conditions.Add(String.Format("and p.SupplierID = {0}", filter.SupplierID.DbValue()));
                if (filter.WarehouseID.HasValue)
                    conditions.Add(String.Format("and w.ID = {0}", filter.WarehouseID.DbValue()));
                if (filter.TagID.HasValue)
                    conditions.Add(String.Format("and t.ID = {0}", filter.TagID.DbValue()));
                if (filter.Returned == "on")
                    conditions.Add(String.Format("and p.Returned > 0"));
            }
            var result = new ProductList(message, filter);
            result.Data = Query<ProductInfo>(new DbQuery(userID, employeeID, DbAction.Product.View,
                String.Format(
                    @"select {3} p.ID, p.Code, p.Name, p.Price, p.Image, p.MadeIn, p.Description, p.OriginalWarranty, p.BussinessWarranty, p.SupplierID, p.Unit, p.Specs, p.Point, p.BussinessID,
                        w.Name as [WarehouseName], t.ID as [TagID], t.Name as [TagName],
                        sum(isnull(ip.Quantity, 0)) - isnull((select sum(ep.Quantity) from ExportProduct ep left join Export e on e.ID = ep.ExportID and e.Removed = 0 and e.Status in (N'Đã xuất', N'Đã chuyển') where ep.Returned = 0 and ep.ProductID = p.ID and e.WarehouseID = w.ID), 0) as [Quantity],
                        isnull((select sum(ep.Quantity) from ExportProduct ep left join Export e on e.ID = ep.ExportID and e.Removed = 0 and e.Status in (N'Đã xuất', N'Đã chuyển') where ep.ProductID = p.ID and e.WarehouseID = w.ID and ep.Returned = 1), 0) as [Returned]
                    from Product p 
                        left join ProductTag pt on p.ID = pt.ProductID and pt.TagID = (select top 1 TagID from ProductTag where ProductID = p.ID {4} order by TagID desc)
                        left join Tag t on pt.TagID = t.ID and t.Status = 'active'
                        left join ImportProduct ip on p.ID = ip.ProductID
                        left join Import i on i.ID = ip.ImportID and i.Status = 'active' 
                        left join Warehouse w on w.Status = 'active' and w.ID = i.WarehouseID
                    where 
                        p.Status = 'active' and p.BussinessID = {0} 
                        and (w.ID is null or (select Username from Login where ID = {2}) = 'admin' or w.ID in (select WarehouseID from LoginWarehouse where LoginID = {2}))
                        {1} 
                    group by p.ID, p.Code, p.Name, p.Price, p.Image, p.MadeIn, p.Description, p.OriginalWarranty, p.BussinessWarranty, p.SupplierID, p.Unit, p.Specs, p.Point, p.BussinessID, w.Name, t.ID, t.Name, w.ID
                    order by p.Name", 
                    bussinessID, String.Join(" ", conditions), 
                    userID, max.HasValue ? String.Format("top {0}", max.Value) : "",
                    filter != null && filter.TagID.HasValue ? String.Format("and TagID = {0}", filter.TagID.DbValue()) : ""), log), out queryResult);
            return result;
        }
        public static ProductList Check(int userID, int employeeID, int bussinessID, ProductFilter filter, int? max = 100)
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
                if (filter.WarehouseID.HasValue)
                    conditions.Add(String.Format("and w.ID = {0}", filter.WarehouseID.DbValue()));
                if (filter.Returned == "on")
                    conditions.Add(String.Format("and p.Returned > 0"));
            }
            var startImport = "0";
            if (filter != null && filter.From.HasValue)
                startImport = String.Format("isnull((select sum(ip.Quantity) from ImportProduct ip join Import i on ip.ImportID = i.ID and i.Status = 'active' and i.WarehouseID = w.ID and i.SubmitDate < '{0}' where ip.ProductID = p.ID), 0)", filter.From.Value.ToString(Constants.DatabaseDatetimeString));
            var startExport = "0";
            if (filter != null && filter.From.HasValue)
                startExport = String.Format("isnull((select sum(ep.Quantity) from ExportProduct ep join Export e on ep.ExportID = e.ID and e.Removed = 0 and e.Status in (N'Đã xuất', N'Đã chuyển') and e.WarehouseID = w.ID and e.SubmitDate < '{0}' where ep.Returned = 0 and ep.ProductID = p.ID), 0)", filter.From.Value.ToString(Constants.DatabaseDatetimeString));
            var result = new ProductList("", filter);
            result.Data = Query<ProductInfo>(new DbQuery(userID, employeeID, DbAction.Product.View,
                String.Format(
                    @"declare @username nvarchar(50) = (select Username from Login where ID = {3})
                    declare @warehouse table (ID int) insert into @warehouse select WarehouseID from LoginWarehouse where LoginID = {3}
                    declare @products table (ID int, Code nvarchar(50), Name nvarchar(200), Unit nvarchar(50), Price decimal(18, 0), WarehouseName nvarchar(200), TagID int, 
                        TagName nvarchar(100), WarehouseID int, ImportQuantity int, ExportQuantity int, SaleQuantity int, StartImportQuantity int, StartExportQuantity int, Returned int)

                    insert into @products
                    select {4} p.ID, p.Code, p.Name, p.Unit, p.Price, w.Name as [WarehouseName], t.ID as [TagID], t.Name as [TagName], max(i.WarehouseID) as [WarehouseID],
                        isnull((select sum(ip.Quantity) from ImportProduct ip join Import i on ip.ImportID = i.ID and i.Status = 'active' and i.WarehouseID = w.ID {5} {6} where ip.ProductID = p.ID), 0) as [ImportQuantity], 
	                    isnull((select sum(ep.Quantity) from ExportProduct ep join Export e on ep.ExportID = e.ID and e.Removed = 0 and e.Status in (N'Đã xuất', N'Đã chuyển') and e.WarehouseID = w.ID {7} {8} where ep.Returned = 0 and ep.ProductID = p.ID), 0) as [ExportQuantity],
                        isnull((select sum(ep.Quantity) from ExportProduct ep join Export e on ep.ExportID = e.ID and e.Removed = 0 and e.Status in (N'Đã xuất', N'Đã chuyển') and e.WarehouseID = w.ID {7} {8} where ep.OrderID is not null and ep.Returned = 0 and ep.ProductID = p.ID), 0) as [SaleQuantity],
                        {9} as [StartImportQuantity], {10} as [StartExportQuantity],
                        isnull((select sum(ep.Quantity) from ExportProduct ep join Export e on ep.ExportID = e.ID and e.Removed = 0 and e.Status in (N'Đã xuất', N'Đã chuyển') and e.WarehouseID = w.ID {7} {8} where ep.ProductID = p.ID and ep.Returned = 1), 0) as [Returned]
                    from Product p 
                        join ImportProduct ip on p.ID = ip.ProductID
                        join Import i on i.ID = ip.ImportID and i.Status = 'active' 
                        join Warehouse w on w.Status = 'active' and w.ID = i.WarehouseID
                        left join ProductTag pt on p.ID = pt.ProductID
                        left join Tag t on pt.TagID = t.ID and t.Status = 'active'
                    where 
                        p.Status = 'active' and p.BussinessID = {0} 
                        and {2}
                        {1} 
                    group by p.ID, p.Code, p.Name, p.Unit, p.Price, w.Name, t.ID, t.Name, w.ID

                    insert into @products
                    select {4} p.ID, p.Code, p.Name, p.Unit, p.Price, w.Name as [WarehouseName], t.ID as [TagID], t.Name as [TagName], max(i.WarehouseID) as [WarehouseID],
                        isnull((select sum(ip.Quantity) from ImportProduct ip join Import i on ip.ImportID = i.ID and i.Status = 'active' and i.WarehouseID = w.ID {5} {6} where ip.ProductID = p.ID), 0) as [ImportQuantity], 
	                    isnull((select sum(ep.Quantity) from ExportProduct ep join Export e on ep.ExportID = e.ID and e.Removed = 0 and e.Status in (N'Đã xuất', N'Đã chuyển') and e.WarehouseID = w.ID {7} {8} where ep.Returned = 0 and ep.ProductID = p.ID), 0) as [ExportQuantity],
                        isnull((select sum(ep.Quantity) from ExportProduct ep join Export e on ep.ExportID = e.ID and e.Removed = 0 and e.Status in (N'Đã xuất', N'Đã chuyển') and e.WarehouseID = w.ID {7} {8} where ep.OrderID is not null and ep.Returned = 0 and ep.ProductID = p.ID), 0) as [SaleQuantity],
                        {9} as [StartImportQuantity], {10} as [StartExportQuantity],
                        isnull((select sum(ep.Quantity) from ExportProduct ep join Export e on ep.ExportID = e.ID and e.Removed = 0 and e.Status in (N'Đã xuất', N'Đã chuyển') and e.WarehouseID = w.ID {7} {8} where ep.ProductID = p.ID and ep.Returned = 1), 0) as [Returned]
                    from Product p 
                        left join ImportProduct ip on p.ID = ip.ProductID
                        left join Import i on i.ID = ip.ImportID and i.Status = 'active' 
                        left join Warehouse w on w.Status = 'active' and w.ID = i.WarehouseID
                        left join ProductTag pt on p.ID = pt.ProductID
                        left join Tag t on pt.TagID = t.ID and t.Status = 'active'
                    where 
                        p.Status = 'active' and p.BussinessID = {0} and w.ID is null and p.ID not in (select ID from @products)
                        and {2}
                        {1} 
                    group by p.ID, p.Code, p.Name, p.Unit, p.Price, w.Name, t.ID, t.Name, w.ID
                    select * from @products order by Name",
                    bussinessID, String.Join(" ", conditions),
                    filter != null && filter.TagID.HasValue ? 
                        String.Format("t.ID = {0}", filter.TagID.Value) :
                        "(t.ID is null or t.ID = (select top 1 TagID from ProductTag where ProductID = p.ID order by TagID desc))",
                    userID, max.HasValue ? String.Format("top {0}", max.Value) : "",
                    filter != null && filter.From.HasValue ? String.Format("and i.SubmitDate >= '{0}'", filter.From.Value.ToString(Constants.DatabaseDatetimeString)) : "",
                    filter != null && filter.To.HasValue ? String.Format("and i.SubmitDate <= '{0}'", filter.To.Value.ToString(Constants.DatabaseDatetimeString)) : "",
                    filter != null && filter.From.HasValue ? String.Format("and e.SubmitDate >= '{0}'", filter.From.Value.ToString(Constants.DatabaseDatetimeString)) : "",
                    filter != null && filter.To.HasValue ? String.Format("and e.SubmitDate <= '{0}'", filter.To.Value.ToString(Constants.DatabaseDatetimeString)) : "",
                    startImport, startExport), true), out queryResult);
            return result;
        }
        public static ProductInfo Get(int userID, int employeeID, int productID, bool log = false, string action = null)
        {
            QueryOutput queryResult;
            if (String.IsNullOrEmpty(action))
                action = DbAction.Product.View;
            return Query<ProductInfo>(new DbQuery(userID, employeeID, action, 
                String.Format("select top 100 p.*, s.Name as [SupplierName] from Product p left join Supplier s on p.SupplierID = s.ID where p.Status = 'active' and p.ID = {0} order by p.ID desc", 
                productID), log), out queryResult).FirstOrDefault();
        }
        public static bool AddTag(int userID, int employeeID, int productID, IEnumerable<int> tagIDs)
        {
            QueryOutput queryResult;
            return Execute(new DbQuery(userID, employeeID, DbAction.Product.Modify, 
                String.Format("insert into ProductTag(ProductID, TagID) values {0}", String.Join(",", tagIDs.Select(i => String.Format("({0}, {1})", productID, i)))), true, productID, "Name"), out queryResult);
        }
        public static bool RemoveTag(int userID, int employeeID, int productID, IEnumerable<int> tagIDs)
        {
            QueryOutput queryResult;
            return Execute(new DbQuery(userID, employeeID, DbAction.Product.Modify, String.Format("delete ProductTag where ProductID = {0} and TagID in ({1})", productID, String.Join(",", tagIDs)), true, productID, "Name"), out queryResult);
        }
        public static List<ProductInfo> Read(int userID, int employeeID, FileInfo fileInfo, int bussinessID)
        {
            var result = new List<ProductInfo>();
            HSSFWorkbook hssfwb;
            using (FileStream file = new FileStream(fileInfo.FullName, FileMode.Open, FileAccess.Read))
            {
                hssfwb = new HSSFWorkbook(file);
            }

            var sheet = hssfwb.GetSheetAt(0);
            if (sheet.LastRowNum > 0)
            {
                var supplierList = new List<SupplierInfo>();
                var tagList = new List<TagInfo>();
                for (var i = 1; i <= sheet.LastRowNum; i++)
                {
                    var row = sheet.GetRow(i);
                    try
                    {
                        var product = new ProductInfo(bussinessID);
                        SupplierInfo supplier = null;
                        TagInfo tag = null;
                        var supplierName = row.GetCellValue<string>(22);
                        if (!String.IsNullOrEmpty(supplierName))
                        {
                            supplier = supplierList.FirstOrDefault(it => it.Name == supplierName);
                            if (supplier == null)
                            {
                                supplier = SupplierInfo.Get(userID, employeeID, bussinessID, supplierName);
                                if (supplier != null)
                                    supplierList.Add(supplier);
                            }
                        }
                        var tagName = row.GetCellValue<string>(23);
                        if (!String.IsNullOrEmpty(tagName))
                        {
                            tag = tagList.FirstOrDefault(it => it.Name == tagName);
                            if (tag == null)
                            {
                                tag = TagInfo.Get(userID, employeeID, bussinessID, tagName);
                                if (tag != null)
                                    tagList.Add(tag);
                            }
                        }
                        product.Code = row.GetCellValue<string>(0);
                        product.Name = row.GetCellValue<string>(1);
                        if (!String.IsNullOrEmpty(product.Name))
                            product.Name = product.Name.Replace("'", "");
                        product.Price = row.GetCellValue<decimal>(2);
                        //product.Image = row.GetCellValue<string>(3);
                        product.MadeIn = row.GetCellValue<string>(3);
                        product.Type = row.GetCellValue<string>(4);
                        product.Engine = row.GetCellValue<string>(5);
                        product.Gender = row.GetCellValue<string>(6);
                        product.MirrorType = row.GetCellValue<string>(7);
                        product.TrapMaterial = row.GetCellValue<string>(8);
                        product.CaseMaterial = row.GetCellValue<string>(9);
                        product.CaseType = row.GetCellValue<string>(10);
                        product.FrontColor = row.GetCellValue<string>(11);
                        product.CaseWidth = row.GetCellValue<string>(12);
                        product.TrapSize = row.GetCellValue<string>(13);
                        product.Diameter = row.GetCellValue<string>(14);
                        product.WaterResistance = row.GetCellValue<string>(15);
                        product.Functions = row.GetCellValue<string>(16);
                        product.Style = row.GetCellValue<string>(17);

                        product.Specs = row.GetCellValue<string>(18);
                        product.Description = row.GetCellValue<string>(19);
                        product.OriginalWarranty = row.GetCellValue<string>(20);
                        product.BussinessWarranty = row.GetCellValue<string>(21);
                        if (supplier != null)
                        {
                            product.SupplierID = supplier.ID;
                            product.SupplierName = supplier.Name;
                        }
                        if (tag != null)
                        {
                            product.TagID = tag.ID;
                            product.TagName = tag.Name;
                        }
                        product.Unit = row.GetCellValue<string>(24);
                        product.Point = row.GetCellValue<int>(25);
                        if (!String.IsNullOrEmpty(product.Code))
                            result.Add(product);
                    }
                    catch { }
                }
            }

            return result;
        }
    }
    public class ProductPrice : BaseModel
    {
        public ProductPrice() : base() { }
        public int ID { get; set; }
        public int ProductID { get; set; }
        public string Name { get; set; }
        public int Quantity { get; set; }
        public decimal Price { get; set; }
    }
}