using Dapper;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace SKtimeManagement
{
    public class TagFilter
    {
        public string Name { get; set; }
        public string ForRepairString { get; set; }
        public bool ForRepairValue { get { return ForRepairString == "on"; } }
    }
    public class TagList
    {
        public TagList(string message = "", TagFilter filter = null)
        {
            Data = new List<TagInfo>();
            Filter = filter != null ? filter : new TagFilter();
            Message = message;
        }
        public List<TagInfo> Data { get; set; }
        public TagFilter Filter { get; set; }
        public string Message { get; set; }
    }
    public class TagInfo : BaseModel
    {
        public TagInfo() : base() { }
        public TagInfo(int bussinessID) : base()
        {
            BussinessID = bussinessID;
        }
        public int ID { get; set; }
        public int BussinessID { get; set; }
        [Required(ErrorMessage = "Tên nhóm sản phẩm không được bỏ trống")]
        public string Name { get; set; }
        public string ForRepairCheck { get; set; }
        public bool ForRepairValue { get { return ForRepairCheck == "on"; } }
        public bool ForRepair { get; set; }
        public bool NameExist(int userID, int employeeID, int bussinessID)
        {
            QueryOutput queryResult;
            return Query<bool>(new DbQuery(userID, employeeID, DbAction.Product.Create, String.Format("select case when count(Name) > 0 then 1 else 0 end from Tag where Status = 'active' and BussinessID = {0} and Name = N'{1}'", bussinessID, Name)), out queryResult).FirstOrDefault();
        }
        public bool Save(ModelStateDictionary modelState, int userID, int employeeID, int bussinessID)
        {
            QueryOutput queryResult;
            if (!Validate(modelState))
            {
                return Result = false;
            }
            if (ID <= 0 && NameExist(userID, employeeID, bussinessID))
            {
                Messages = new List<string>() { "Tên nhóm sản phẩm đã được sử dụng" };
                return Result = false;
            }
            var query = "";
            var action = "";
            var id = ID.ToString();
            if (ID > 0)
            {
                query = String.Format(@"update Tag 
                                                set Name = N'{0}', ForRepair = {2}
                                                where ID = {1}",
                                            new object[] {
                                                    Name, ID, ForRepairValue.DbValue()
                });
                action = DbAction.Tag.Modify;
            }
            else
            {
                query = String.Format(@"declare @tag table (ID int)
                                        insert Tag(Name, BussinessID, Status, ForRepair) 
                                        output inserted.ID into @tag
                                        values (N'{0}', {1}, 'active', {2})",
                                            new object[] {
                                                    Name, bussinessID, ForRepairValue.DbValue()
                });
                action = DbAction.Tag.Create;
                id = "(select top 1 ID from @tag)";
            }
            Result = Execute(new DbQuery(userID, employeeID, action, query, true, id, "Name"), out queryResult);
            if (Result)
                Messages = new List<string>() { "Lưu thông tin thành công" };
            return Result;
        }
        public static bool Remove(int userID, int employeeID, int tagID)
        {
            QueryOutput queryResult;
            return Execute(new DbQuery(userID, employeeID, DbAction.Tag.Remove, String.Format("update Tag set Status = 'removed' where ID = {0}", tagID), true, tagID, "Name"), out queryResult);
        }
        public static TagList Find(int userID, int employeeID, int bussinessID, string message = "", TagFilter filter = null, bool log = false)
        {
            QueryOutput queryResult;
            var conditions = new List<string>();
            if (filter != null)
            {
                if (!String.IsNullOrEmpty(filter.Name))
                    conditions.Add(String.Format("and Name like N'%{0}%'", filter.Name));
                if (filter.ForRepairValue)
                    conditions.Add(String.Format("and ForRepair = 1"));
            }
            var result = new TagList(message, filter);
            result.Data = Query<TagInfo>(new DbQuery(userID, employeeID, DbAction.Tag.View, 
                String.Format("select * from Tag where Status = 'active' and BussinessID = {0} {1} order by Name", bussinessID, String.Join(" ", conditions)), log), out queryResult);
            return result;
        }
        public static List<dynamic> KeyList(int userID, int employeeID, int bussinessID, int productID)
        {
            QueryOutput queryResult;
            return Query<dynamic>(new DbQuery(userID, employeeID, DbAction.Tag.View, String.Format("select t.ID, t.Name, p.ID as [Tagged] from Tag t left join ProductTag p on t.ID = p.TagID and p.ProductID = {1} where t.Status = 'active' and t.BussinessID = {0} order by t.Name", bussinessID, productID)), out queryResult);
        }
        public static TagInfo Get(int userID, int employeeID, int tagID, bool log = false)
        {
            QueryOutput queryResult;
            return Query<TagInfo>(new DbQuery(userID, employeeID, DbAction.Tag.View, String.Format("select top 100 * from Tag where Status = 'active' and ID = {0} order by ID desc", tagID), log), out queryResult).FirstOrDefault();
        }
        public static TagInfo Get(int userID, int employeeID, int bussinessID, string name, bool log = false)
        {
            QueryOutput queryResult;
            return Query<TagInfo>(new DbQuery(userID, employeeID, DbAction.Supplier.View,
                String.Format("select top 100 * from Tag where Status = 'active' and BussinessID = {0} and Name = N'{1}' order by Name", bussinessID, name), log), out queryResult).FirstOrDefault();
        }
    }
    public class ProductTag
    {
        public int ID { get; set; }
        public int ProductID { get; set; }
        public int TagID { get; set; }
    }
}