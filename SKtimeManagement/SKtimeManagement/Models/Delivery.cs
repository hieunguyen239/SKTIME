using Dapper;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace SKtimeManagement
{
    public class DeliveryFilter
    {
        public string Name { get; set; }
        public string Address { get; set; }
        public string Phone { get; set; }
    }
    public class DeliveryList
    {
        public DeliveryList(string message = "", DeliveryFilter filter = null)
        {
            Data = new List<DeliveryInfo>();
            Filter = filter != null ? filter : new DeliveryFilter();
            Message = message;
        }
        public List<DeliveryInfo> Data { get; set; }
        public DeliveryFilter Filter { get; set; }
        public string Message { get; set; }
    }
    public class DeliveryInfo : BaseModel
    {
        public DeliveryInfo() : base() { }
        public DeliveryInfo(int bussinessID) : base()
        {
            BussinessID = bussinessID;
        }
        public int ID { get; set; }
        public int BussinessID { get; set; }
        [Required(ErrorMessage = "Tên kho không được bỏ trống")]
        public string Name { get; set; }
        public string Phone { get; set; }
        public string Address { get; set; }
        public bool Save(ModelStateDictionary modelState, int userID, int employeeID)
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
                query = String.Format(@"update Delivery 
                                                set Name= N'{0}', Phone= N'{1}', Address = N'{2}'
                                                where ID = {3}",
                                            new object[] {
                                                    Name, Phone, Address, ID
                });
                action = DbAction.Delivery.Modify;
            }
            else
            {
                query = String.Format(@"declare @Delivery table (ID int)
                                        insert Delivery(Name, Phone, Address, BussinessID, Status) 
                                        output inserted.ID into @Delivery
                                        values (N'{0}', N'{1}', N'{2}', {3}, 'active')",
                                            new object[] {
                                                    Name, Phone, Address, BussinessID
                });
                action = DbAction.Delivery.Create;
                id = "(select top 1 ID from @Delivery)";
            }
            Result = Execute(new DbQuery(userID, employeeID, action, query, true, id, "Name"), out queryResult);
            if (Result)
                Messages = new List<string>() { "Lưu thông tin thành công" };
            return Result;
        }
        public static bool Remove(int userID, int employeeID, int DeliveryID)
        {
            QueryOutput queryResult;
            return Execute(new DbQuery(userID, employeeID, DbAction.Delivery.Remove, String.Format("update Delivery set Status = 'removed' where ID = {0}", DeliveryID), true, DeliveryID, "Name"), out queryResult);
        }
        public static DeliveryList Find(int userID, int employeeID, int bussinessID, string message = "", DeliveryFilter filter = null, bool log = false)
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
            var result = new DeliveryList(message, filter);
            result.Data = Query<DeliveryInfo>(new DbQuery(userID, employeeID, DbAction.Delivery.View, 
                String.Format("select * from Delivery where Status = 'active' and BussinessID = {0} {1} order by Name", bussinessID, String.Join(" ", conditions)), log), out queryResult);
            return result;
        }
        public static List<dynamic> KeyList(int userID, int employeeID, int bussinessID)
        {
            QueryOutput queryResult;
            return Query<dynamic>(new DbQuery(userID, employeeID, DbAction.Delivery.View,
                String.Format("select * from Delivery where Status = 'active' and BussinessID = {0} order by Name", bussinessID)), out queryResult);
        }
        public static DeliveryInfo Get(int userID, int employeeID, int DeliveryID, bool log = false)
        {
            QueryOutput queryResult;
            return Query<DeliveryInfo>(new DbQuery(userID, employeeID, DbAction.Delivery.View, 
                String.Format("select top 1 * from Delivery where Status = 'active' and ID = {0}", DeliveryID), log), out queryResult).FirstOrDefault();
        }
    }
}