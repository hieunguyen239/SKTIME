using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Dapper;
using System.Web.Mvc;
using System.ComponentModel.DataAnnotations;

namespace SKtimeManagement
{
    public class SupplierFilter
    {
        public string Name { get; set; }
        public string Address { get; set; }
        public string Phone { get; set; }
    }
    public class SupplierList
    {
        public SupplierList(string message = "", SupplierFilter filter = null)
        {
            Data = new List<SupplierInfo>();
            Filter = filter != null ? filter : new SupplierFilter();
            Message = message;
        }
        public List<SupplierInfo> Data { get; set; }
        public SupplierFilter Filter { get; set; }
        public string Message { get; set; }
    }
    public class SupplierInfo : BaseModel
    {
        public SupplierInfo() : base() { }
        public SupplierInfo(int bussinessID) : base()
        {
            BussinessID = bussinessID;
        }
        public int ID { get; set; }
        public int BussinessID { get; set; }
        [Required(ErrorMessage = "Tên nhà cung cấp không được bỏ trống")]
        public string Name { get; set; }
        public string Phone { get; set; }
        public string Address { get; set; }
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
                query = String.Format(@"update Supplier 
                                                set Name = N'{0}', Phone = N'{1}', Address = N'{2}'
                                                where ID = {3}",
                                            new object[] {
                                                    Name, Phone, Address, ID
                });
                action = DbAction.Supplier.Modify;
            }
            else
            {
                query = String.Format(@"declare @supplier table (ID int)
                                        insert Supplier(Name, Phone, Address, BussinessID, Status) 
                                        output inserted.ID into @supplier
                                        values (N'{0}', N'{1}', N'{2}', {3}, 'active')",
                                            new object[] {
                                                    Name, Phone, Address, bussinessID
                });
                action = DbAction.Supplier.Create;
                id = "(select top 1 ID from @supplier)";
            }
            Result = Execute(new DbQuery(userID, employeeID, action, query, true, id, "Name"), out queryResult);
            if (Result)
                Messages = new List<string>() { "Lưu thông tin thành công" };
            return Result;
        }
        public static bool Remove(int userID, int employeeID, int supplierID)
        {
            QueryOutput queryResult;
            return Execute(new DbQuery(userID, employeeID, DbAction.Supplier.Remove, String.Format("update Supplier set Status = 'removed' where ID = {0}", supplierID), true, supplierID, "Name"), out queryResult);
        }
        public static SupplierList Find(int userID, int employeeID, int bussinessID, string message = "", SupplierFilter filter = null, bool log = false)
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
            var result = new SupplierList(message, filter);
            result.Data = Query<SupplierInfo>(new DbQuery(userID, employeeID, DbAction.Supplier.View, 
                String.Format("select * from Supplier where Status = 'active' and BussinessID = {0} {1} order by Name", bussinessID, String.Join(" ", conditions)), log), out queryResult);
            return result;
        }
        public static SupplierInfo Get(int userID, int employeeID, int bussinessID, string name, bool log = false)
        {
            QueryOutput queryResult;
            return Query<SupplierInfo>(new DbQuery(userID, employeeID, DbAction.Supplier.View, 
                String.Format("select top 100 * from Supplier where Status = 'active' and BussinessID = {0} and Name = N'{1}' order by ID desc", bussinessID, name), log), out queryResult).FirstOrDefault();
        }
        public static SupplierInfo Get(int userID, int employeeID, int supplierID, bool log = false)
        {
            QueryOutput queryResult;
            return Query<SupplierInfo>(new DbQuery(userID, employeeID, DbAction.Supplier.View, 
                String.Format("select top 100 * from Supplier where Status = 'active' and ID = {0} order by Name", supplierID), log), out queryResult).FirstOrDefault();
        }
    }
}