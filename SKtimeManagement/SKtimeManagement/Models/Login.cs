using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Dapper;
using System.Web.Mvc;
using System.ComponentModel.DataAnnotations;

namespace SKtimeManagement
{
    public class LoginList
    {
        public LoginList(string message = "", LoginFilter filter = null)
        {
            Filter = filter != null ? filter : new LoginFilter();
            Message = message;
        }
        public List<Login> Data { get; set; }
        public LoginFilter Filter { get; set; }
        public string Message { get; set; }
    }
    public class LoginPermission
    {
        public string BussinessModify { get; set; }
        public string StoreView { get; set; }
        public string StoreCreate { get; set; }
        public string StoreModify { get; set; }
        public string StoreRemove { get; set; }
        public string EmployeeView { get; set; }
        public string EmployeeCreate { get; set; }
        public string EmployeeModify { get; set; }
        public string EmployeeRemove { get; set; }
        public string LoginView { get; set; }
        public string LoginCreate { get; set; }
        public string LoginModify { get; set; }
        public string LoginRemove { get; set; }
        public string SupplierView { get; set; }
        public string SupplierCreate { get; set; }
        public string SupplierModify { get; set; }
        public string SupplierRemove { get; set; }
        public string ClientView { get; set; }
        public string ClientCreate { get; set; }
        public string ClientModify { get; set; }
        public string ClientRemove { get; set; }
        public string ClientTypeView { get; set; }
        public string ClientTypeCreate { get; set; }
        public string ClientTypeModify { get; set; }
        public string ClientTypeRemove { get; set; }
        public string ProductView { get; set; }
        public string ProductCreate { get; set; }
        public string ProductModify { get; set; }
        public string ProductRemove { get; set; }
        public string TagView { get; set; }
        public string TagCreate { get; set; }
        public string TagModify { get; set; }
        public string TagRemove { get; set; }
        public string WarehouseView { get; set; }
        public string WarehouseCreate { get; set; }
        public string WarehouseModify { get; set; }
        public string WarehouseRemove { get; set; }
        public string ImportView { get; set; }
        public string ImportCreate { get; set; }
        public string ImportModify { get; set; }
        public string ImportRemove { get; set; }
        public string ExportView { get; set; }
        public string ExportCreate { get; set; }
        public string ExportModify { get; set; }
        public string ExportRemove { get; set; }
        public string OrderView { get; set; }
        public string OrderCreate { get; set; }
        public string OrderModify { get; set; }
        public string OrderRemove { get; set; }
        public string IncomeView { get; set; }
        public string IncomeCreate { get; set; }
        public string IncomeModify { get; set; }
        public string IncomeRemove { get; set; }
        public string OutcomeView { get; set; }
        public string OutcomeCreate { get; set; }
        public string OutcomeModify { get; set; }
        public string OutcomeRemove { get; set; }
        public string WarrantyView { get; set; }
        public string WarrantyCreate { get; set; }
        public string WarrantyModify { get; set; }
        public string WarrantyRemove { get; set; }
        public string RepairView { get; set; }
        public string RepairCreate { get; set; }
        public string RepairModify { get; set; }
        public string RepairRemove { get; set; }
        public string ReportSummary { get; set; }
        public string ReportDetail { get; set; }
        public string ReportProduct { get; set; }
        public string ReportSalary { get; set; }
        public string ReportClient { get; set; }
        public string ReportProductPart { get; set; }
        public string Salary { get; set; }
        public string TransactionModify { get; set; }
        public List<string> AuthorizedPermissions()
        {
            var result = new List<string>();
            if ("on" == this.BussinessModify)
            {
                result.Add(DbAction.Bussiness.Create);
                result.Add(DbAction.Bussiness.Modify);
            }

            if ("on" == this.ClientView)
                result.Add(DbAction.Client.View);
            if ("on" == this.ClientCreate)
                result.Add(DbAction.Client.Create);
            if ("on" == this.ClientModify)
                result.Add(DbAction.Client.Modify);
            if ("on" == this.ClientRemove)
                result.Add(DbAction.Client.Remove);

            if ("on" == this.ClientTypeView)
                result.Add(DbAction.ClientType.View);
            if ("on" == this.ClientTypeCreate)
                result.Add(DbAction.ClientType.Create);
            if ("on" == this.ClientTypeModify)
                result.Add(DbAction.ClientType.Modify);
            if ("on" == this.ClientTypeRemove)
                result.Add(DbAction.ClientType.Remove);

            if ("on" == this.EmployeeCreate)
                result.Add(DbAction.Employee.Create);
            if ("on" == this.EmployeeModify)
                result.Add(DbAction.Employee.Modify);
            if ("on" == this.EmployeeRemove)
                result.Add(DbAction.Employee.Remove);
            if ("on" == this.EmployeeView)
                result.Add(DbAction.Employee.View);

            if ("on" == this.ExportCreate)
                result.Add(DbAction.Export.Create);
            if ("on" == this.ExportModify)
                result.Add(DbAction.Export.Modify);
            if ("on" == this.ExportRemove)
                result.Add(DbAction.Export.Remove);
            if ("on" == this.ExportView)
                result.Add(DbAction.Export.View);

            if ("on" == this.ImportCreate)
                result.Add(DbAction.Import.Create);
            if ("on" == this.ImportModify)
                result.Add(DbAction.Import.Modify);
            if ("on" == this.ImportRemove)
                result.Add(DbAction.Import.Remove);
            if ("on" == this.ImportView)
                result.Add(DbAction.Import.View);

            if ("on" == this.LoginCreate)
                result.Add(DbAction.Login.Create);
            if ("on" == this.LoginModify)
                result.Add(DbAction.Login.Modify);
            if ("on" == this.LoginRemove)
                result.Add(DbAction.Login.Remove);
            if ("on" == this.LoginView)
                result.Add(DbAction.Login.View);

            if ("on" == this.IncomeCreate)
                result.Add(DbAction.Income.Create);
            if ("on" == this.IncomeModify)
                result.Add(DbAction.Income.Modify);
            if ("on" == this.IncomeRemove)
                result.Add(DbAction.Income.Remove);
            if ("on" == this.IncomeView)
                result.Add(DbAction.Income.View);

            if ("on" == this.OrderCreate)
                result.Add(DbAction.Order.Create);
            if ("on" == this.OrderModify)
                result.Add(DbAction.Order.Modify);
            if ("on" == this.OrderRemove)
                result.Add(DbAction.Order.Remove);
            if ("on" == this.OrderView)
                result.Add(DbAction.Order.View);

            if ("on" == this.OutcomeCreate)
                result.Add(DbAction.Outcome.Create);
            if ("on" == this.OutcomeModify)
                result.Add(DbAction.Outcome.Modify);
            if ("on" == this.OutcomeRemove)
                result.Add(DbAction.Outcome.Remove);
            if ("on" == this.OutcomeView)
                result.Add(DbAction.Outcome.View);

            if ("on" == this.ProductCreate)
                result.Add(DbAction.Product.Create);
            if ("on" == this.ProductModify)
                result.Add(DbAction.Product.Modify);
            if ("on" == this.ProductRemove)
                result.Add(DbAction.Product.Remove);
            if ("on" == this.ProductView)
                result.Add(DbAction.Product.View);

            if ("on" == this.StoreCreate)
                result.Add(DbAction.Store.Create);
            if ("on" == this.StoreModify)
                result.Add(DbAction.Store.Modify);
            if ("on" == this.StoreRemove)
                result.Add(DbAction.Store.Remove);
            if ("on" == this.StoreView)
                result.Add(DbAction.Store.View);

            if ("on" == this.SupplierCreate)
                result.Add(DbAction.Supplier.Create);
            if ("on" == this.SupplierModify)
                result.Add(DbAction.Supplier.Modify);
            if ("on" == this.SupplierRemove)
                result.Add(DbAction.Supplier.Remove);
            if ("on" == this.SupplierView)
                result.Add(DbAction.Supplier.View);

            if ("on" == this.TagCreate)
                result.Add(DbAction.Tag.Create);
            if ("on" == this.TagModify)
                result.Add(DbAction.Tag.Modify);
            if ("on" == this.TagRemove)
                result.Add(DbAction.Tag.Remove);
            if ("on" == this.TagView)
                result.Add(DbAction.Tag.View);

            if ("on" == this.WarehouseCreate)
                result.Add(DbAction.Warehouse.Create);
            if ("on" == this.WarehouseModify)
                result.Add(DbAction.Warehouse.Modify);
            if ("on" == this.WarehouseRemove)
                result.Add(DbAction.Warehouse.Remove);
            if ("on" == this.WarehouseView)
                result.Add(DbAction.Warehouse.View);

            if ("on" == this.WarrantyCreate)
                result.Add(DbAction.Warranty.Create);
            if ("on" == this.WarrantyModify)
                result.Add(DbAction.Warranty.Modify);
            if ("on" == this.WarrantyRemove)
                result.Add(DbAction.Warranty.Remove);
            if ("on" == this.WarrantyView)
                result.Add(DbAction.Warranty.View);

            if ("on" == this.RepairCreate)
                result.Add(DbAction.Repair.Create);
            if ("on" == this.RepairModify)
                result.Add(DbAction.Repair.Modify);
            if ("on" == this.RepairRemove)
                result.Add(DbAction.Repair.Remove);
            if ("on" == this.RepairView)
                result.Add(DbAction.Repair.View);

            if ("on" == this.ReportSummary)
                result.Add(ReportAction.Summary);
            if ("on" == this.ReportDetail)
                result.Add(ReportAction.Detail);
            if ("on" == this.ReportProduct)
                result.Add(ReportAction.Product);
            if ("on" == this.ReportSalary)
                result.Add(ReportAction.Salary);
            if ("on" == this.ReportClient)
                result.Add(ReportAction.Client);
            if ("on" == this.ReportProductPart)
                result.Add(ReportAction.ProductPart);

            if ("on" == this.Salary)
                result.Add(SalaryAction.Calculate);

            if ("on" == this.TransactionModify)
                result.Add(DbAction.Transaction.Modify);
            return result;
        }
    }
    public class LoginFilter
    {
        public int? EmployeeID { get; set; }
        public string Username { get; set; }
    }
    public class LoginType
    {
        public const string Sale = "Bán hàng";
        public const string Mechanic = "Kỹ thuật";
        public const string Office = "Quản lý";
    }
    public class Login : BaseModel 
    {
        public Login() : base() { }
        public Login(int bussinessID, int? employeeID = null) : base()
        {
            BussinessID = bussinessID;
            EmployeeID = employeeID;
        }
        public int ID { get; set; }
        public int BussinessID { get; set; }
        public int? EmployeeID { get; set; }
        public string EmployeeName { get; set; }
        [Required(ErrorMessage = "Tài khoản không được bỏ trống")]
        public string Username { get; set; }
        [Required(ErrorMessage = "Mật khẩu không được bỏ trống")]
        public string Password { get; set; }
        public string Type { get; set; }
        public string Name { get { return Username; } }
        public string Message { get; set; }
        public EmployeeInfo EmployeeInfo { get; set; }
        //[Required(ErrorMessage = "Tài khoản chưa được cấp quyền")]
        public LoginPermission Permission { get; set; }
        public bool IsValid()
        {
            Message = "Đăng nhập không thành công";
            try
            {
                using (var con = Repo.DB.SKtimeManagement)
                {
                    var login = con.Query<Login>(String.Format("select top 100 l.* from Login l join Employee e on l.EmployeeID = e.ID where e.Status = 'active' and l.Status = 'active' and l.EmployeeID is not null and l.Username = N'{0}' order by l.ID desc", Username)).FirstOrDefault();
                    if (login != null && login.ID > 0 && login.Password == Password)
                    {
                        ID = login.ID;
                        EmployeeID = login.EmployeeID;
                        Type = login.Type;
                        EmployeeInfo = con.Query<EmployeeInfo>(String.Format("select * from Employee where ID = {0} order by ID desc", EmployeeID)).FirstOrDefault();
                        return true;
                    }
                    if (login == null || login.ID <= 0)
                        Message = "Tài khoản không tồn tại";
                    else
                        Message = "Mật khẩu không đúng";
                }
            }
            catch { }
            return false;
        }
        public bool Save(ModelStateDictionary modelState, int userID, int employeeID, List<int> warehouseIDs)
        {
            QueryOutput queryResult;
            if (!Validate(modelState))
            {
                return Result = false;
            }
            if (ID == 0 && Query<bool>(new DbQuery(userID, employeeID, DbAction.Login.View, 
                String.Format("select case when (select top 1 ID from Login where BussinessID = {0} and Username = N'{1}' and Status = 'active') is null then 0 else 1 end", 
                BussinessID, Username)), out queryResult).FirstOrDefault())
            {
                Messages.Add("Tài khoản đã được sử dụng");
                ErrorFields.Add("Username");
                return Result = false;
            }
            if (Username != "admin" && Permission == null)
            {
                Messages.Add("Tài khoản chưa được cấp quyền");
                return Result = false;
            }
            var query = "";
            var id = ID.ToString();
            var action = "";
            if (ID > 0)
            {
                query = String.Format(@"update Login 
                                            set Username = (case when Username = 'admin' then 'admin' else N'{0}' end), Password = N'{1}', EmployeeID = {2}, Type = N'{4}'
                                            where ID = {3}", new object[] {
                                            Username, Password, EmployeeID.DbValue(), ID, Type
                    });
                action = DbAction.Login.Modify;
            }
            else
            {
                query = String.Format(@"declare @ID table (ID int)
                                        insert Login(Username, Password, BussinessID, EmployeeID, Status, Type) 
                                        output inserted.ID into @ID
                                        values ('{0}', N'{1}', {2}, {3}, 'active', N'{4}')",
                                        new object[] { Username, Password, BussinessID, EmployeeID.DbValue(), Type
                });
                id = "(select top 1 ID from @ID)";
                action = DbAction.Login.Create;
            }
            if (Username != "admin")
            {
                query += String.Format(
                    " if ((select count(Username) from Login where ID = {0} and Username = 'admin') = 0) begin delete LoginPermission where LoginID = {0}", id);
                var permissions = Permission.AuthorizedPermissions();
                if (permissions.Count > 0)
                {
                    query += String.Format(" insert into LoginPermission(LoginID, Action) values {0}",
                        String.Join(",", permissions.Select(p => String.Format("({0}, N'{1}')", id, p))));
                }
                if (warehouseIDs != null && warehouseIDs.Count > 0)
                {
                    query += String.Format(" insert into LoginWarehouse(LoginID, WarehouseID) values {0}", String.Join(",", warehouseIDs.Select(wh => String.Format("({0}, {1})", id, wh))));
                }
                query += " end";
            }
            Result = Execute(new DbQuery(userID, employeeID, action, query, true, id, "Username"), out queryResult);
            if (Result)
                Messages = new List<string>() { "Lưu thông tin thành công" };
            return Result;
        }
        public static bool Remove(int userID, int employeeID, int loginID)
        {
            QueryOutput queryResult;
            return Execute(new DbQuery(userID, employeeID, DbAction.Login.Remove, String.Format("update Login set Status = 'removed' where ID = {0} and Username <> 'admin'", loginID), true, loginID, "Username"), out queryResult);
        }
        public static List<Login> GetUnassigned(int userID, int employeeID, int bussinessID)
        {
            QueryOutput queryResult;
            return Query<Login>(new DbQuery(userID, employeeID, DbAction.Login.View, String.Format("select * from Login where Status = 'active' and BussinessID = {0} and EmployeeID is null order by Username", bussinessID)), out queryResult);
        }
        public static List<string> GetPermissions(int userID, int employeeID, int loginID)
        {
            QueryOutput queryResult;
            return Query<string>(new DbQuery(userID, employeeID, DbAction.Login.Modify, String.Format("select Action from LoginPermission where LoginID = {0} order by Action", loginID)), out queryResult).ToList();
        }
        public static LoginList Find(int userID, int employeeID, int bussinessID, string message = "", LoginFilter filter = null, bool log = false)
        {
            QueryOutput queryResult;
            var conditions = new List<string>();
            if (filter != null)
            {
                if (filter.EmployeeID.HasValue)
                    conditions.Add(String.Format("and e.ID = {0}", filter.EmployeeID.DbValue()));
                if (!String.IsNullOrEmpty(filter.Username))
                    conditions.Add(String.Format("and l.Username like N'%{0}%'", filter.Username));
            }
            var result = new LoginList(message, filter);
            result.Data = Query<Login>(new DbQuery(userID, employeeID, DbAction.Login.View, 
                String.Format(@"select top 100 l.*, e.Name as [EmployeeName] 
                                from Login l left join Employee e on l.EmployeeID = e.ID 
                                where l.Status = 'active' and l.BussinessID = {0} {1}
                                order by l.Username", bussinessID, String.Join(" ", conditions)), log), out queryResult);
            return result;
        }
        public static Login Get(int userID, int employeeID, int loginID, bool log = false)
        {
            QueryOutput queryResult;
            return Query<Login>(new DbQuery(userID, employeeID, DbAction.Login.View, 
                String.Format("select l.*, e.Name as [EmployeeName] from Login l left join Employee e on l.EmployeeID = e.ID where l.Status = 'active' and l.ID = {0}", loginID), log), out queryResult).FirstOrDefault();
        }
        public static Login Get(int loginID)
        {
            Login result = null;
            try
            {
                using (var con = Repo.DB.SKtimeManagement)
                {
                    result = con.Query<Login>(String.Format("select l.*, e.Name as [EmployeeName] from Login l left join Employee e on l.EmployeeID = e.ID where l.Status = 'active' and l.ID = {0}", loginID)).FirstOrDefault();
                }
            }
            catch { }
            return result;
        }
        public static bool Authorize(int loginID, string[] action)
        {
            try
            {
                using (var con = Repo.DB.SKtimeManagement)
                {
                    return con.Query<bool>(
                        String.Format(
                            "select case when count(ID) > 0 then 1 else 0 end from LoginPermission where LoginID = {0} and Action in ({1},'admin')", 
                            loginID, String.Join(",", action.Select(i => String.Format("N'{0}'", i))))).FirstOrDefault();
                }
            }
            catch
            {
                return false;
            }
        }
        public static bool AddWarehouse(int userID, int employeeID, int loginID, int warehouseID)
        {
            QueryOutput queryResult;
            return Execute(new DbQuery(userID, employeeID, DbAction.Login.Modify,
                String.Format("delete LoginWarehouse where LoginID = {0} and WarehouseID = {1} insert into LoginWarehouse(LoginID, WarehouseID) values ({0}, {1})", loginID, warehouseID)), out queryResult);
        }
        public static bool RemoveWarehouse(int userID, int employeeID, int loginID, int warehouseID)
        {
            QueryOutput queryResult;
            return Execute(new DbQuery(userID, employeeID, DbAction.Login.Modify,
                String.Format("delete LoginWarehouse where LoginID = {0} and WarehouseID = {1}", loginID, warehouseID)), out queryResult);
        }
    }
    public class ChangePasswordModel
    {
        public string OldPassword { get; set; }
        public string NewPassword { get; set; }
        public string RepeatNewPassword { get; set; }
        public string Message { get; set; }
        public bool Result { get; set; }
        public bool Validate(int userID)
        {
            Message = "Đổi mật khẩu không thành công";
            if (NewPassword != RepeatNewPassword)
            {
                Message = "Mật khẩu mới không trùng với mật khẩu nhập lại";
                return false;
            }
            if (OldPassword == NewPassword)
            {
                Message = "Mật khẩu mới trùng với mật khẩu cũ";
                return false;
            }
            try
            {
                using (var con = Repo.DB.SKtimeManagement)
                {
                    var password = con.Query<string>(String.Format("select Password from Login where ID = {0}", userID)).FirstOrDefault();
                    if (password == OldPassword)
                        return true;
                    if (String.IsNullOrEmpty(password))
                        Message = "Tài khoản không tồn tại";
                    else
                        Message = "Mật khẩu không đúng";
                }
            }
            catch { }
            return false;
        }
        public bool Submit(int userID)
        {
            if (Validate(userID))
            {
                try
                {
                    using (var con = Repo.DB.SKtimeManagement)
                    {
                        con.Execute(String.Format("update Login set Password = N'{0}' where ID = {1}", NewPassword, userID));
                    }
                }
                catch { }
                Result = true;
            }
            return Result;
        }
    }
}