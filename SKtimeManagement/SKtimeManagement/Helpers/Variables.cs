using Dapper;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace SKtimeManagement
{
    public class SessionKey
    {
        public const string Account = "Account";
        public const string AccountInfo = "AccountInfo";
        public const string Employee = "Employee";
        public const string Token = "Token";
        public const string Download = "Download";
        public const string Route = "Route";
        public const string Tag = "Tag";
        public const string Warehouse = "Warehouse";
        public const string Quantity = "Quantity";
        public const string Import = "Import";
        public const string ImportList = "ImportList";
        public const string ExportList = "ExportList";
    }
    public class Constants
    {
        public const string MonthString = "MM-yyyy";
        public const string DateTimeString = "dd-MM-yyyy HH:mm:ss";
        public const string DateTimeWebString = "DD-MM-YYYY HH:mm:ss";
        public const string ShortDateString = "dd/MM";
        public const string DateString = "dd-MM-yyyy";
        public const string DateWebString = "DD-MM-YYYY";
        public const string DatabaseDateString = "yyyy-MM-dd";
        public const string DatabaseDatetimeString = "yyyy-MM-dd HH:mm:ss";
        public const string CodeValue = "ddMMyyyyHHmmss";
    }
    public class Views
    {
        public const string ForEmployee = "ForEmployee";
        public const string ForEmployeePartial = "ForEmployeePartial";
        public const string Calculate = "Calculate";
        public const string CalculatePartial = "CalculatePartial";
        public const string Find = "Find";
        public const string FindPartial = "FindPartial";
        public const string Check = "Check";
        public const string CheckPartial = "CheckPartial";
        public const string Summary = "Summary";
        public const string SummaryPartial = "SummaryPartial";
        public const string SummaryAdmin = "SummaryAdmin";
        public const string SummaryAdminPartial = "SummaryAdminPartial";
        public const string Unauthorized = "Unauthorized";
        public const string UnauthorizedPartial = "UnauthorizedPartial";
        public const string Index = "Index";
        public const string IndexPartial = "IndexPartial";
        public const string List = "List";
        public const string ListPartial = "ListPartial";
        public const string Save = "Save";
        public const string SavePartial = "SavePartial";
        public const string Bussiness = "Bussiness";
        public const string BussinessPartial = "BussinessPartial";
        public const string ChangePassword = "ChangePassword";
        public const string ChangePasswordPartial = "ChangePasswordPartial";
        public const string Store = "Store";
        public const string StorePartial = "StorePartial";
        public const string StoreSave = "StoreSave";
        public const string StoreSavePartial = "StoreSavePartial";
        public const string Import = "Import";
        public const string ImportPartial = "ImportPartial";
        public const string Export = "Export";
        public const string ExportPartial = "ExportPartial";
        public const string Table = "Table";
        public const string History = "History";
        public const string HistoryPartial = "HistoryPartial";
        public const string Products = "Products";
        public const string Detail = "Detail";
        public const string DetailPartial = "DetailPartial";
        public const string Update = "Update";
        public const string UpdatePartial = "UpdatePartial";
        public const string Filter = "Filter";
        public const string Selector = "Selector";
        public const string NotFoundPartial = "NotFoundPartial";
        public const string AccessDeniedPartial = "AccessDeniedPartial";
        public const string Product = "Product";
        public const string ProductPartial = "ProductPartial";
        public const string SalaryPartial = "SalaryPartial";
        public const string Salary = "Salary";
        public const string Client = "Client";
        public const string ClientPartial = "ClientPartial";
        public const string ProductPart = "ProductPart";
        public const string ProductPartPartial = "ProductPartPartial";
    }
    public enum QueryOutput
    {
        PermissionDenied = 0,
        Success = 1,
        Error = 2
    }
    public class DbQuery
    {
        public DbQuery(int loginID, int employeeID, string action, string query, bool log = false, object recordID = null, string code = "Code")
        {
            LoginID = loginID;
            EmployeeID = employeeID;
            Action = action;
            Query = query;
            Log = log;
            if (Log)
            {
                var table = SKtimeManagement.Log.TableFromAction(Action);
                Query += String.Format(
                    " insert into Log(LoginID, EmployeeID, Action, Query, SubmitDate, Result, RecordID, RecordCode) values ({0}, {1}, N'{2}', N'{3}', '{4}', 1, {5}, {6})",
                    LoginID, EmployeeID, Action, Query.Replace("'", "''"), DateTime.Now.ToString(Constants.DatabaseDatetimeString),
                    (Action.Contains("Create") || Action.Contains("Modify") || Action.Contains("Remove")) ? recordID : "null",
                    (Action.Contains("Create") || Action.Contains("Modify") || Action.Contains("Remove")) ? String.Format("(select top 1 {1} from {2} where ID = {0})", recordID, code, table) : "null");
                //Query += String.Format(" update Log set RecordID = {0}, RecordCode = (select top 1 {1} from {2} where ID = {0}) where ID = (select top 1 ID from @logID)", recordID, code,  table);
            }
        }
        public int LoginID { get; set; }
        public int EmployeeID { get; set; }
        public string Action { get; set; }
        public string Query { get; set; }
        public bool Log { get; set; }
    }
    public class BaseModel
    {
        public BaseModel()
        {
            Messages = new List<string>();
            ErrorFields = new List<string>();
        }
        public List<string> Messages { get; set; }
        public List<string> ErrorFields { get; set; }
        public bool Result { get; set; }
        public bool Valid(string fieldName)
        {
            return String.IsNullOrEmpty(ErrorFields.FirstOrDefault(f => f == fieldName));
        }
        public bool Validate(ModelStateDictionary modelState)
        {
            var result = true;
            foreach (var field in modelState)
            {
                if (field.Value.Errors.Count > 0)
                {
                    Messages.AddRange(field.Value.Errors.Select(e => e.ErrorMessage));
                    ErrorFields.Add(field.Key);
                    result = false;
                }
            }
            return result;
        }
        public static List<T> Query<T>(DbQuery query, out QueryOutput result)
        {
            var data = new List<T>();
            try
            {
                result = QueryOutput.PermissionDenied;
                using (var con = Repo.DB.SKtimeManagement)
                {
                    if (query.Log)
                    {
                        var param = new DynamicParameters();
                        param.Add("loginID", query.LoginID, DbType.Int32);
                        param.Add("employeeID", query.EmployeeID, DbType.Int32);
                        param.Add("action", query.Action, DbType.String);
                        param.Add("query", query.Query, DbType.String);
                        param.Add("log", query.Log, DbType.Boolean);
                        param.Add("result", dbType: DbType.Int32, direction: ParameterDirection.Output);
                        data = con.Query<T>("sp_ExecuteQuery", param, null, true, 300, System.Data.CommandType.StoredProcedure).ToList();
                        result = param.Get<QueryOutput>("result");
                    }
                    else
                    {
                        data = con.Query<T>(query.Query).ToList();
                        result = QueryOutput.Success;
                    }
                }
                return data;
            }
            catch (Exception e)
            {
                result = QueryOutput.Error;
                return data;
            }
        }
        public static bool Execute(DbQuery query, out QueryOutput result)
        {
            try
            {
                result = QueryOutput.PermissionDenied;
                using (var con = Repo.DB.SKtimeManagement)
                {
                    var param = new DynamicParameters();
                    param.Add("loginID", query.LoginID, DbType.Int32);
                    param.Add("employeeID", query.EmployeeID, DbType.Int32);
                    param.Add("action", query.Action, DbType.String);
                    param.Add("query", query.Query, DbType.String);
                    param.Add("log", query.Log, DbType.Boolean);
                    param.Add("result", dbType: DbType.Int32, direction: ParameterDirection.Output);
                    con.Execute("sp_ExecuteQuery", param, null, 300, System.Data.CommandType.StoredProcedure);
                    result = param.Get<QueryOutput>("result");
                }
                return result == QueryOutput.Success;
            }
            catch (Exception e)
            {
                result = QueryOutput.Error;
                return false;
            }
        }
        public static string NewUniqueCode(int userID, int employeeID, int bussinessID, string table, int length = 9, string prefix = "450")
        {
            var result = "";
            while (true)
            {
                var ean13 = new Ean13();
                ean13.CountryCode = String.IsNullOrEmpty(prefix) ? SKtimeManagement.Functions.RandomNumberString(2) : prefix;
                ean13.ManufacturerCode = SKtimeManagement.Functions.RandomNumberString(Convert.ToInt32(Math.Floor(length / 2.0)));
                ean13.ProductCode = SKtimeManagement.Functions.RandomNumberString(Convert.ToInt32(Math.Ceiling(length / 2.0)));
                ean13.CalculateChecksumDigit();
                result = String.Format("{0}{1}{2}{3}", ean13.CountryCode, ean13.ManufacturerCode, ean13.ProductCode, ean13.ChecksumDigit);
                if (!CodeExist(userID, employeeID, bussinessID, result, table))
                    break;
            }
            return result;
        }
        public static bool CodeExist(int userID, int employeeID, int bussinessID, string code, string table)
        {
            try
            {
                using (var con = Repo.DB.SKtimeManagement)
                {
                    return con.Query<bool>(String.Format("select case when count(Code) > 0 then 1 else 0 end from [{2}] where {3} and BussinessID = {0} and Code = '{1}'",
                        bussinessID, code, table, table == "Export" || table == "Order" ? "Removed = 0" : "Status = 'active'")).FirstOrDefault();
                }
            }
            catch { }
            return false;
        }
    }
}