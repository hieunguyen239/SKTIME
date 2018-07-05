using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using Dapper;
using System.Web.Mvc;

namespace SKtimeManagement
{
    public class StoreFilter
    {
        public string Name { get; set; }
        public string Address { get; set; }
        public string Phone { get; set; }
    }
    public class StoreList
    {
        public StoreList(string message = "", StoreFilter filter = null)
        {
            Data = new List<StoreInfo>();
            Filter = filter != null ? filter : new StoreFilter();
            Message = message;
        }
        public List<StoreInfo> Data { get; set; }
        public StoreFilter Filter { get; set; }
        public string Message { get; set; }
    }
    public class StoreInfo : BaseModel
    {
        public StoreInfo() : base() { }
        public static StoreInfo Get(int userID, int employeeID, int storeID, bool log = false)
        {
            QueryOutput queryResult;
            return Query<StoreInfo>(new DbQuery(userID, employeeID, DbAction.Store.View, String.Format("select top 100 * from Store where Status = 'active' and ID = {0} order by ID desc", storeID), log), out queryResult).FirstOrDefault();
        }
        public static StoreList Find(int userID, int employeeID, int bussinessID, string message = "", StoreFilter filter = null, bool log = false)
        {
            QueryOutput queryResult;
            var conditions = new List<string>();
            if (filter != null)
            {
                if (!String.IsNullOrEmpty(filter.Address))
                    conditions.Add(String.Format("and s.Address like N'%{0}%'", filter.Address));
                if (!String.IsNullOrEmpty(filter.Name))
                    conditions.Add(String.Format("and s.Name like N'%{0}%'", filter.Name));
                if (!String.IsNullOrEmpty(filter.Phone))
                    conditions.Add(String.Format("and s.Phone like N'%{0}%'", filter.Phone));
            }
            var result = new StoreList(message, filter);
            result.Data = Query<StoreInfo>(new DbQuery(userID, employeeID, DbAction.Store.View, 
                String.Format("select s.* from Store s join Bussiness b on s.BussinessID = b.ID where s.Status = 'active' and b.ID = {0} {1} order by s.Name", bussinessID, String.Join(" ", conditions)), log), out queryResult);
            return result;
        }
        public static bool Remove(int userID, int employeeID, int storeID)
        {
            QueryOutput queryResult;
            return Execute(new DbQuery(userID, employeeID, DbAction.Store.Remove, String.Format("update Store set Status = 'removed' where ID = {0}", storeID), true, storeID, "Name"), out queryResult);
        }
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
                query = String.Format(@"update Store 
                                                set Name = N'{0}', Address = N'{1}', Phone = N'{2}', SalePoint = {4}
                                                where ID = {3}", new object[] {
                                                Name, Address, Phone, ID, SalePoint
                    });
                action = DbAction.Store.Modify;
            }
            else
            {
                query += "declare @warehouseID int = null";
                if (CreateWarehouse == "on")
                {
                    query += String.Format(
                        @" declare @warehouse table (ID int)
                        insert Warehouse(Name, Phone, Address, BussinessID, Status) 
                        output inserted.ID into @warehouse
                        values (N'{0}', N'{1}', N'{2}', {3}, 'active')
                        set @warehouseID = (select top 1 ID from @warehouse)", Name, Phone, Address, bussinessID);
                }
                query += String.Format(
                    @" declare @store table (ID int)
                    insert Store(Name, Address, Phone, BussinessID, Status, SalePoint, WarehouseID) 
                    output inserted.ID into @store
                    values (N'{0}', N'{1}', N'{2}', {3}, 'active', {4}, @warehouseID)",
                    new object[] { Name, Address, Phone, bussinessID, SalePoint
                });
                action = DbAction.Store.Create;
                id = "(select top 1 ID from @store)";
            }
            Result = Execute(new DbQuery(userID, employeeID, action, query, true, id, "Name"), out queryResult);
            if (Result)
                Messages = new List<string>() { "Lưu thông tin thành công" };
            return Result;
        }
        public int ID { get; set; }
        [Required(ErrorMessage = "Tên cửa hàng không được bỏ trống")]
        public string Name { get; set; }
        public string Address { get; set; }
        public string Phone { get; set; }
        public string CreateWarehouse { get; set; }
        public decimal SalePoint { get; set; }
        public string SalePointString { get { return SalePoint.GetCurrencyString(); } }
        public int? WarehouseID { get; set; }
        public string WarehouseName { get; set; }
    }
}