using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Dapper;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace SKtimeManagement
{
    public class BussinessInfo : BaseModel
    {
        public BussinessInfo() : base() { }
        public static BussinessInfo Find(int userID, int employeeID)
        {
            QueryOutput queryResult;
            return Query<BussinessInfo>(new DbQuery(userID, employeeID, DbAction.Bussiness.Modify, String.Format("select top 100 b.* from Bussiness b join Login l on b.ID = l.BussinessID where l.ID = {0} order by b.ID desc", userID)), out queryResult).FirstOrDefault();
        }
        public int ID { get; set; }
        [Required(ErrorMessage = "Tên công ty/ đơn vị không được bỏ trống")]
        public string Name { get; set; }
        public string Logo { get; set; }
        public string Address { get; set; }
        [Phone(ErrorMessage = "Số điện thoại không hợp lệ")]
        public string Phone { get; set; }
        public string Fax { get; set; }
        public string TaxNumber { get; set; }
        public string BankNumber { get; set; }
        [EmailAddress(ErrorMessage = "Email không hợp lệ")]
        public string Email { get; set; }
        public string Website { get; set; }
        public string Director { get; set; }
        public string Accountant { get; set; }
        public int DefaultDiscount { get; set; }
        public bool AllowDept { get; set; }
        public string AllowDeptCheck { get; set; }
        public List<StoreInfo> Stores { get; set; }
        public bool Save(ModelStateDictionary modelState, int userID, int employeeID)
        {
            if (!Validate(modelState))
            {
                return Result = false;
            }
            QueryOutput queryResult;
            var query = String.Format(@"update Bussiness 
                                        set Name = N'{0}', Logo = N'{1}', Address = N'{2}', Phone = N'{3}',
                                        Fax = N'{4}', TaxNumber = N'{5}', BankNumber = N'{6}', Email = N'{7}',
                                        Website = N'{8}', Director = N'{9}', Accountant = N'{10}', DefaultDiscount = {12}, AllowDept = {13}
                                        where ID = {11}", new object[] {
                                        Name, Logo, Address, Phone, Fax, TaxNumber, BankNumber, Email, Website, Director, Accountant, ID, DefaultDiscount, (AllowDeptCheck == "on").DbValue()
            });
            Result = Execute(new DbQuery(userID, employeeID, DbAction.Bussiness.Modify, query), out queryResult);
            if (Result)
            {
                Messages = new List<string>() { "Cập nhật thông tin thành công" };
                SiteConfiguration.BussinessInfo = Find(userID, employeeID);
                AllowDept = AllowDeptCheck == "on";
            }
            return Result;
        }
    }
}