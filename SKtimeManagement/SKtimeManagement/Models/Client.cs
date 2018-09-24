using Dapper;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace SKtimeManagement
{
    public class ClientFilter
    {
        public string Code { get; set; }
        public string Name { get; set; }
        public string Phone { get; set; }
        public string Address { get; set; }
        public string Email { get; set; }
        public string City { get; set; }
        public string District { get; set; }
        public int? TypeID { get; set; }
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = Constants.DateTimeString)]
        public DateTime? From { get; set; }
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = Constants.DateTimeString)]
        public DateTime? To { get; set; }
    }
    public class ClientList
    {
        public ClientList(string message = "", ClientFilter filter = null)
        {
            Data = new List<ClientInfo>();
            Filter = filter != null ? filter : new ClientFilter();
            Message = message;
        }
        public List<ClientInfo> Data { get; set; }
        public ClientFilter Filter { get; set; }
        public string Message { get; set; }
    }
    public class ClientInfo : BaseModel
    {
        public ClientInfo() : base()
        {
            Code = DateTime.Now.ToString(Constants.CodeValue);
        }
        public ClientInfo(int bussinessID) : base()
        {
            BussinessID = bussinessID;
            Code = DateTime.Now.ToString(Constants.CodeValue);
            OrderHistory = new List<ExportRecord>();
        }
        public int ID { get; set; }
        public int BussinessID { get; set; }
        [Required(ErrorMessage = "Mã khách hàng không được bỏ trống")]
        public string Code { get; set; }
        [Required(ErrorMessage = "Tên khách hàng không được bỏ trống")]
        public string Name { get; set; }
        [Phone(ErrorMessage = "Số điện thoại không hợp lệ")]
        public string Phone { get; set; }
        public string Address { get; set; }
        [EmailAddress(ErrorMessage = "Email không hợp lệ")]
        public string Email { get; set; }
        public string City { get; set; }
        public string District { get; set; }
        public int? TypeID { get; set; }
        public string TypeName { get; set; }
        public int Point { get; set; }
        public string PointString { get { return Point.GetCurrencyString(); } }
        public decimal SaleTotal { get; set; }
        public string SaleTotalString { get { return SaleTotal.GetCurrencyString(); } }
        public List<ExportRecord> OrderHistory { get; set; }
        public bool Save(ModelStateDictionary modelState, int userID, int employeeID)
        {
            if (!Validate(modelState))
            {
                return Result = false;
            }
            QueryOutput existResult;
            var exist = Query<bool>(new DbQuery(userID, employeeID, DbAction.Client.View, String.Format("select count(ID) from Client where Code = '{0}' {1}", Code, ID > 0 ? String.Format("and ID <> {0}", ID) : "")), out existResult).FirstOrDefault();
            if (exist)
            {
                ErrorFields.Add("Code");
                Messages = new List<string>() { "Mã khách hàng đã được sử dụng" };
                return Result = false;
            }
            QueryOutput queryResult;
            var query = "";
            var id = ID.ToString();
            var action = "";
            if (ID > 0)
            {
                query += String.Format(@"update Client 
                                            set Name= N'{0}', Phone= N'{1}', Address = N'{2}', Point = {8},
                                            Email = N'{3}', City = N'{4}', District = N'{5}', Code = N'{6}'
                                            where ID = {7}",
                                        new object[] {
                                                Name, Phone, Address, Email, City, District, Code, ID, Point
                });
                action = DbAction.Client.Modify;
            }
            else
            {
                query += String.Format(@"declare @ID table(ID int)
                                            insert Client(Name, Phone, Address, Email, City, District, BussinessID, Code, Point, Status) 
                                            output inserted.ID
                                            into @ID
                                            values (N'{0}', N'{1}', N'{2}', N'{3}', N'{4}', N'{5}', {6}, N'{7}', {8}, 'active')",
                                        new object[] {
                                                Name, Phone, Address, Email, City, District, BussinessID, Code, Point
                });
                id = "(select top 1 ID from @ID)";
                action = DbAction.Client.Create;
            }
            query += String.Format(" delete ClientType where ClientID = {0}", id);
            if (TypeID.HasValue && TypeID > 0)
            {
                query += String.Format(" insert into ClientType(ClientID, TypeID, CreateDate, Status) values({0}, {1}, '{2}', 'active')", id, TypeID, DateTime.Now.ToString(Constants.DatabaseDatetimeString));
            }
            Result = Execute(new DbQuery(userID, employeeID, action, query, true, id, "Name"), out queryResult);
            if (Result)
                Messages = new List<string>() { "Lưu thông tin thành công" };
            return Result;
        }
        public static bool Remove(int userID, int employeeID, int clientID)
        {
            var result = false;
            QueryOutput queryResult;
            result = Execute(new DbQuery(userID, employeeID, DbAction.Client.Remove, String.Format("update Client set Status = 'removed' where ID = {0}", clientID), true, clientID, "Name"), out queryResult);
            return result;
        }
        public static ClientList Find(int userID, int employeeID, int bussinessID, string message = "", ClientFilter filter = null, bool log = false, int? max = 100)
        {
            var result = new ClientList(message, filter);
            var conditions = new List<string>();
            if (filter != null)
            {
                if (!String.IsNullOrEmpty(filter.Code))
                    conditions.Add(String.Format("and c.Code like N'%{0}%'", filter.Code));
                if (!String.IsNullOrEmpty(filter.Name))
                    conditions.Add(String.Format("and c.Name like N'%{0}%'", filter.Name));
                if (!String.IsNullOrEmpty(filter.Phone))
                    conditions.Add(String.Format("and c.Phone like N'%{0}%'", filter.Phone));
                if (!String.IsNullOrEmpty(filter.Address))
                    conditions.Add(String.Format("and c.Address like N'%{0}%'", filter.Address));
                if (!String.IsNullOrEmpty(filter.Email))
                    conditions.Add(String.Format("and c.Email like N'%{0}%'", filter.Email));
                if (!String.IsNullOrEmpty(filter.City))
                    conditions.Add(String.Format("and c.City like N'%{0}%'", filter.City));
                if (!String.IsNullOrEmpty(filter.District))
                    conditions.Add(String.Format("and c.District like N'%{0}%'", filter.District));
                if (filter.TypeID.HasValue)
                    conditions.Add(String.Format("and t.ID = {0}", filter.TypeID.DbValue()));
            }
            QueryOutput queryResult; // coding here - change the way of list paging to improve performance
            var strSql = String.Format(@"select {2} c.ID, c.BussinessID, c.Code, c.Name, c.Phone, c.Address, c.Email, c.City, c.District, MAX(t.ID) as [TypeID], t.Name as [TypeName], c.Point
                from Client c 
                    left join ClientType ct on c.ID = ct.ClientID and ct.Status = 'active'
                    left join Type t on ct.TypeID = t.ID and t.Status = 'active'
                where c.BussinessID = {0} and c.Status = 'active' {1} 
                group by c.ID, c.BussinessID, c.Code, c.Name, c.Phone, c.Address, c.Email, c.City, c.District, t.Name, c.Point
                order by c.Name",
                bussinessID, String.Join("", conditions), max.HasValue ? String.Format("top {0}", max.Value) : String.Format("top {0}", 100));
            result.Data = Query<ClientInfo>(
                new DbQuery(userID, employeeID, DbAction.Client.View, 
                strSql, log), out queryResult);
            return result;
        }
        public static List<dynamic> KeyList(int userID, int employeeID, int bussinessID)
        {
            QueryOutput queryResult;
            return Query<dynamic>(new DbQuery(userID, employeeID, DbAction.Client.View, String.Format("select top 100 * from Client where Status = 'active' and BussinessID = {0} order by Name", bussinessID)), out queryResult);
        }
        public static ClientInfo Get(int userID, int employeeID, int clientID, bool log = false)
        {
            QueryOutput queryResult;
            return Query<ClientInfo>(new DbQuery(userID, employeeID, DbAction.Client.View,
                String.Format(@"select c.ID, c.BussinessID, c.Code, c.Name, c.Phone, c.Address, c.Email, c.City, c.District, MAX(t.ID) as [TypeID], t.Name as [TypeName], c.Point
                                from Client c 
                                    left join ClientType ct on c.ID = ct.ClientID and ct.Status = 'active'
                                    left join Type t on ct.TypeID = t.ID and t.Status = 'active'
                                where c.ID = {0} and c.Status = 'active'
                                group by c.ID, c.BussinessID, c.Code, c.Name, c.Phone, c.Address, c.Email, c.City, c.District, t.Name, c.Point", clientID), log), out queryResult).FirstOrDefault();
        }
        public static string GetClientOrderQuery(int bussinessID, int clientID, DateTime? from = null, DateTime? to = null)
        {
            var conditions = new List<string>();
            conditions.Add(String.Format("and c.ID = {0}", clientID));
            if (from.HasValue)
                conditions.Add(String.Format(" and o.SubmitDate >= {0}", from.DbValue()));
            if (to.HasValue)
                conditions.Add(String.Format(" and o.SubmitDate <= {0}", to.DbValue()));
            return String.Format(
                @"select o.ID, o.EmployeeID, o.DeliveryID, o.WarehouseID, o.Code, o.SubmitDate, o.PayMethod, o.Receipt, o.Total, o.Discount, o.Paid, o.Note, o.Status, w.Name as [WarehouseName], c.ID as [ClientID], c.Name as [ClientName]
                from SKtimeManagement..[Order] o
                    join Warehouse w on o.WarehouseID = w.ID and w.Status = 'active'
                    join ExportProduct p on o.ID = p.OrderID
                    left join Export ex on ex.ID = p.ExportID
                    join Employee em on o.EmployeeID = em.ID
                    left join Delivery del on o.DeliveryID = del.ID
                    left join Client c on o.ClientID = c.ID
                where o.Removed = 0 and o.BussinessID = {0} {1} 
                group by o.ID, o.EmployeeID, o.DeliveryID, o.WarehouseID, o.Code, o.SubmitDate, o.PayMethod, o.Receipt, o.Total, o.Discount, o.Paid, o.Note, o.Status, w.Name, c.ID, c.Name, em.Name, del.Name, ex.ID, ex.Code
                order by o.ID desc",
                bussinessID, String.Join(" ", conditions));
        }
        public static List<ExportRecord> GetClientOrders(int userID, int employeeID, int bussinessID, int clientID)
        {
            QueryOutput queryResult;
            var conditions = new List<string>();
            conditions.Add(String.Format("and c.ID = {0}", clientID));
            return Query<ExportRecord>(new DbQuery(userID, employeeID, DbAction.Client.View, GetClientOrderQuery(bussinessID, clientID), false), out queryResult);
        }
        public static ClientTypeInfo GetClientType(int userID, int employeeID, int clientID)
        {
            QueryOutput queryResult;
            return Query<ClientTypeInfo>(new DbQuery(userID, employeeID, DbAction.Client.View,
                String.Format(@"select t.*
                                from Client c 
                                    left join ClientType ct on c.ID = ct.ClientID and ct.Status = 'active'
                                    left join Type t on ct.TypeID = t.ID and t.Status = 'active'
                                where c.ID = {0} and c.Status = 'active'", clientID)), out queryResult).FirstOrDefault();
        }
    }
}