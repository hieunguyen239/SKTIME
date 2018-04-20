using Dapper;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace SKtimeManagement
{
    public class WarehouseFilter
    {
        public string Name { get; set; }
        public string Address { get; set; }
        public string Phone { get; set; }
    }
    public class WarehouseList
    {
        public WarehouseList(string message = "", WarehouseFilter filter = null)
        {
            Data = new List<WarehouseInfo>();
            Filter = filter != null ? filter : new WarehouseFilter();
            Message = message;
        }
        public List<WarehouseInfo> Data { get; set; }
        public WarehouseFilter Filter { get; set; }
        public string Message { get; set; }
    }
    public class WarehouseInfo : BaseModel
    {
        public WarehouseInfo() : base() { }
        public WarehouseInfo(int bussinessID) : base()
        {
            BussinessID = bussinessID;
        }
        public int ID { get; set; }
        public int BussinessID { get; set; }
        [Required(ErrorMessage = "Tên kho không được bỏ trống")]
        public string Name { get; set; }
        public string Phone { get; set; }
        public string Address { get; set; }
        public bool NameExist(int userID, int employeeID, int bussinessID)
        {
            QueryOutput queryResult;
            return Query<bool>(new DbQuery(userID, employeeID, DbAction.Product.Create, String.Format("select case when count(Name) > 0 then 1 else 0 end from Warehouse where Status = 'active' and BussinessID = {0} and Name = N'{1}'", bussinessID, Name)), out queryResult).FirstOrDefault();
        }
        public bool Save(ModelStateDictionary modelState, int userID, int employeeID, int bussinessID)
        {
            QueryOutput queryResult;
            if (!Validate(modelState))
            {
                return Result = false;
            }
            if (ID <= 0 && NameExist(userID,  employeeID, bussinessID))
            {
                Messages = new List<string>() { "Tên kho đã được sử dụng" };
                return Result = false;
            }
            var query = "";
            var action = "";
            var id = ID.ToString();
            if (ID > 0)
            {
                query = String.Format(@"update Warehouse 
                                                set Name= N'{0}', Phone= N'{1}', Address = N'{2}'
                                                where ID = {3}",
                                            new object[] {
                                                    Name, Phone, Address, ID
                });
                action = DbAction.Warehouse.Modify;
            }
            else
            {
                query = String.Format(@"declare @warehouse table (ID int)
                                        insert Warehouse(Name, Phone, Address, BussinessID, Status) 
                                        output inserted.ID into @warehouse
                                        values (N'{0}', N'{1}', N'{2}', {3}, 'active')",
                                            new object[] {
                                                    Name, Phone, Address, bussinessID
                });
                action = DbAction.Warehouse.Create;
                id = "(select top 1 ID from @warehouse)";
            }
            Result = Execute(new DbQuery(userID, employeeID, action, query, true, id, "Name"), out queryResult);
            if (Result)
                Messages = new List<string>() { "Lưu thông tin thành công" };
            return Result;
        }
        public static bool Remove(int userID, int employeeID, int warehouseID)
        {
            QueryOutput queryResult;
            return Execute(new DbQuery(userID, employeeID, DbAction.Warehouse.Remove, String.Format("update Warehouse set Status = 'removed' where ID = {0}", warehouseID), true, warehouseID, "Name"), out queryResult);
        }
        public static WarehouseList Find(int userID, int employeeID, int bussinessID, string message = "", WarehouseFilter filter = null, bool log = false)
        {
            QueryOutput queryResult;
            var conditions = new List<string>();
            if (filter != null)
            {
                if (!String.IsNullOrEmpty(filter.Address))
                    conditions.Add(String.Format("and Address like N'%{0}%'", filter.Address));
                if (!String.IsNullOrEmpty(filter.Name))
                    conditions.Add(String.Format("and Name like N'%{0}%'", filter.Name));
                if (!String.IsNullOrEmpty(filter.Phone))
                    conditions.Add(String.Format("and Phone like N'%{0}%'", filter.Phone));
            }
            var result = new WarehouseList(message, filter);
            result.Data = Query<WarehouseInfo>(new DbQuery(userID, employeeID, DbAction.Warehouse.View, 
                String.Format("select * from Warehouse where Status = 'active' and BussinessID = {0} {1} order by Name", bussinessID, String.Join(" ", conditions)), log), out queryResult);
            return result;
        }
        public static List<dynamic> KeyList(int userID, int employeeID, int bussinessID, int productID)
        {
            QueryOutput queryResult;
            return Query<dynamic>(new DbQuery(userID, employeeID, DbAction.Warehouse.View, 
                String.Format(
                    @"select w.ID, w.Name, q.ID as [Tagged] 
                    from Warehouse w left join ProductQuantity q on w.ID = q.WarehouseID and q.ProductID = {1} 
                    where w.Status = 'active' and w.BussinessID = {0} 
                    and ((select Username from Login where ID = {2}) = 'admin' or w.ID in (select WarehouseID from LoginWarehouse where LoginID = {2}))
                    order by w.Name", bussinessID, productID, userID)), out queryResult);
        }
        public static List<dynamic> KeyListWithLogin(int userID, int employeeID, int bussinessID, int loginID)
        {
            QueryOutput queryResult;
            return Query<dynamic>(new DbQuery(userID, employeeID, DbAction.Warehouse.View, 
                String.Format(@"select w.ID, w.Name, l.LoginID as [Tagged] 
                    from Warehouse w left join LoginWarehouse l on w.ID = l.WarehouseID and l.loginID = {1} 
                    where w.Status = 'active' and w.BussinessID = {0} order by w.Name", 
                    bussinessID, loginID)), out queryResult);
        }
        public static List<WarehouseInfo> FindAuthorized(int userID, int employeeID, int bussinessID, int loginID, string action)
        {
            QueryOutput queryResult;
            return Query<WarehouseInfo>(new DbQuery(userID, employeeID, action,
                String.Format(@"select w.*
                    from Warehouse w
                    where w.Status = 'active' and w.BussinessID = {0} 
                    and ((select Username from Login where ID = {1}) = 'admin' or w.ID in (select WarehouseID from LoginWarehouse where LoginID = {1}))
                    order by w.Name",
                    bussinessID, loginID)), out queryResult);
        }
        public static WarehouseInfo Get(int userID, int employeeID, int warehouseID, bool log = false)
        {
            QueryOutput queryResult;
            return Query<WarehouseInfo>(new DbQuery(userID, employeeID, DbAction.Warehouse.View, 
                String.Format("select top 100 * from Warehouse where Status = 'active' and ID = {0} order by ID desc", warehouseID), log), out queryResult).FirstOrDefault();
        }
        public static List<WarehouseInfo> Find(int userID, int employeeID, List<string> names, bool log = false)
        {
            QueryOutput queryResult;
            return Query<WarehouseInfo>(new DbQuery(userID, employeeID, DbAction.Warehouse.View,
                String.Format("select * from Warehouse where Status = 'active' and ({0}) order by Name",
                String.Join(" or ", names.Select(i => String.Format("Name = N'{0}'", i)))), log), out queryResult).ToList();
        }
    }
    public class ProductQuantity : BaseModel
    {
        public ProductQuantity() : base() { }
        public ProductQuantity(int productID, int warehouseID, int quantity = 0) : base()
        {
            ProductID = productID;
            WarehouseID = warehouseID;
            Quantity = quantity;
        }
        public int ID { get; set; }
        public int ProductID { get; set; }
        public int WarehouseID { get; set; }
        public string WarehouseName { get; set; }
        public int Quantity { get; set; }
        public static List<ProductQuantity> Find(int userID, int employeeID, int productID, bool log = false)
        {
            QueryOutput queryResult;
            return Query<ProductQuantity>(new DbQuery(userID, employeeID, DbAction.Product.View, 
                String.Format("select q.*, w.Name as [WarehouseName] from ProductQuantity q join Warehouse w on q.WarehouseID = w.ID and w.Status = 'active' where q.ProductID = {0}", productID), log), out queryResult);
        }
        public static bool Save(int userID, int employeeID, IEnumerable<ProductQuantity> quantities, int productID = 0)
        {
            QueryOutput queryResult;
            return Execute(new DbQuery(userID, employeeID, DbAction.Product.Modify, String.Join(" ", quantities.Select(q => String.Format(
                    @"if ((select top 1 ID from ProductQuantity where ProductID = {0} and WarehouseID = {1}) is null)
                    begin
                    insert into ProductQuantity(ProductID, WarehouseID, Quantity) values({0}, {1}, {2})
                    end
                    else
                    begin
                    update ProductQuantity set Quantity = {2} where ProductID = {0} and WarehouseID = {1}
                    end",
                productID > 0 ? productID : q.ProductID, q.WarehouseID, q.Quantity))), true), out queryResult);
        }
        public static bool Update(int userID, int employeeID, IEnumerable<ProductQuantity> quantities)
        {
            QueryOutput queryResult;
            return Execute(new DbQuery(userID, employeeID, DbAction.Product.Modify, 
                String.Format(String.Join(" ", quantities.Select(i => String.Format("update ProductQuantity set Quantity = {0} where ID = {1}", i.Quantity, i.ID)))), true), out queryResult);
        }
        public static bool Remove(int userID, int employeeID, IEnumerable<ProductQuantity> quantities)
        {
            QueryOutput queryResult;
            return Execute(new DbQuery(userID, employeeID, DbAction.Product.Remove, 
                String.Format("delete ProductQuantity where ProductID = {0} and WarehouseID in ({1})", quantities.FirstOrDefault().ProductID, String.Join(",", quantities.Select(q => q.WarehouseID))), true), out queryResult);
        }
    }
}