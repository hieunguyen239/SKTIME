using Dapper;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace SKtimeManagement
{
    public class Discount
    {
        public Discount(string value, string name)
        {
            Value = value;
            Name = name;
        }
        public string Name { get; set; }
        public string Value { get; set; }
        public static Discount[] List
        {
            get
            {
                return new Discount[] {
                    new Discount("Percentage", "Phần trăm"),
                    new Discount("Value", "Giá trị")
                };
            }
        }
    }
    public class ClientTypeFilter
    {
        public string Name { get; set; }
        public string DiscountType { get; set; }
    }
    public class ClientTypeList
    {
        public ClientTypeList(string message = "", ClientTypeFilter filter = null)
        {
            Data = new List<ClientTypeInfo>();
            Filter = filter != null ? filter : new ClientTypeFilter();
            Message = message;
        }
        public List<ClientTypeInfo> Data { get; set; }
        public ClientTypeFilter Filter { get; set; }
        public string Message { get; set; }
    }
    public class ClientTypeInfo : BaseModel
    {
        public ClientTypeInfo() : base() { }
        public ClientTypeInfo(int bussinessID) : base()
        {
            BussinessID = bussinessID;
        }
        public int ID { get; set; }
        public int BussinessID { get; set; }
        [Required]
        public string Name { get; set; }
        public string DiscountName
        {
            get
            {
                var type = Discount.List.FirstOrDefault(d => d.Value == DiscountType);
                if (type == null)
                    return null;
                else
                    return type.Name;
            }
        }
        public string DiscountType { get; set; }
        public decimal DiscountValue { get; set; }
        public string DiscountValueString { get { return DiscountValue.GetCurrencyString(); } }
        public decimal Revenue { get; set; }
        public string RevenueString { get { return Revenue.GetCurrencyString(); } }
        public bool Save(ModelStateDictionary modelState, int userID, int employeeID)
        {
            if (!Validate(modelState))
            {
                return Result = false;
            }
            QueryOutput queryResult;
            var query = "";
            var action = "";
            var recordID = ID.ToString();
            if (ID > 0)
            {
                query = String.Format(@"update Type 
                                                set Name= N'{0}', DiscountType = N'{1}', DiscountValue = {2}, Revenue = {4}
                                                where ID = {3}",
                                            new object[] {
                                                    Name, DiscountType, DiscountValue, ID, Revenue
                });
                action = DbAction.ClientType.Modify;
            }
            else
            {
                query = String.Format(@"declare @id table (ID int) 
                                        insert Type(Name, DiscountType, DiscountValue, BussinessID, Status, Revenue) 
                                        output inserted.ID into @id
                                        values (N'{0}', N'{1}', {2}, {3}, 'active', {4})",
                                            new object[] {
                                                    Name, DiscountType, DiscountValue, BussinessID, Revenue
                });
                action = DbAction.ClientType.Create;
                recordID = "(select top 1 ID from @id)";
            }
            Result = Execute(new DbQuery(userID, employeeID, action, query, true, recordID, "Name"), out queryResult);
            if (Result)
                Messages = new List<string>() { "Lưu thông tin thành công" };
            return Result;
        }
        public static bool Remove(int userID, int employeeID, int typeID)
        {
            QueryOutput queryResult;
            return Execute(new DbQuery(userID, employeeID, DbAction.ClientType.Remove, String.Format("update Type set Status = 'removed' where ID = {0}", typeID), true, typeID, "Name"), out queryResult);
        }
        public static ClientTypeList Find(int userID, int employeeID, int bussinessID, string message = "", ClientTypeFilter filter = null, bool log = false)
        {
            QueryOutput queryResult;
            var conditions = new List<string>();
            if (filter != null)
            {
                if (!String.IsNullOrEmpty(filter.Name))
                    conditions.Add(String.Format("and Name like N'%{0}%'", filter.Name));
                if (!String.IsNullOrEmpty(filter.DiscountType))
                    conditions.Add(String.Format("and DiscountType like N'%{0}%'", filter.DiscountType));
            }
            var result = new ClientTypeList(message, filter);
            result.Data = Query<ClientTypeInfo>(new DbQuery(userID, employeeID, DbAction.ClientType.View, 
                String.Format("select * from Type where Status = 'active' and BussinessID = {0} {1} order by Name", bussinessID, String.Join(" ", conditions)), log), out queryResult);
            return result;
        }
        public static List<dynamic> KeyList(int userID, int employeeID, int bussinessID)
        {
            QueryOutput queryResult;
            return Query<dynamic>(new DbQuery(userID, employeeID, DbAction.ClientType.View, String.Format("select * from Type where Status = 'active' and BussinessID = {0} order by Name", bussinessID)), out queryResult);
        }
        public static ClientTypeInfo Get(int userID, int employeeID, int clientID, bool log = false)
        {
            QueryOutput queryResult;
            return Query<ClientTypeInfo>(new DbQuery(userID, employeeID, DbAction.ClientType.View, String.Format("select * from Type where Status = 'active' and ID = {0}", clientID), log), out queryResult).FirstOrDefault();
        }
    }
}