using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SKtimeManagement
{
    public class Log
    {
        public int ID { get; set; }
        public int EmployeeID { get; set; }
        public string EmployeeName { get; set; }
        public DateTime SubmitDate { get; set; }
        public string Action { get; set; }
        public string TableName { get; set; }
        public int? RecordID { get; set; }
        public string RecordCode { get; set; }
        public string Color
        {
            get
            {
                if (Action.Contains(DbAction.Import.Name))
                    return "green";
                if (Action.Contains(DbAction.Export.Name))
                    return "red";
                if (Action.Contains(DbAction.Order.Modify))
                    return "orange";
                if (Action.Contains(DbAction.Order.Name))
                    return "blue";
                return "";
            }
        }
        public string Message
        {
            get
            {
                return String.Format("<strong>{0}</strong> {1}{2} lúc <strong>{3}</strong>", 
                    EmployeeName, ActionName, 
                    !String.IsNullOrEmpty(RecordCode) ? String.Format(" <strong>{0}</strong>", RecordCode) : "", 
                    SubmitDate.ToString(Constants.DateTimeString));
            }
        }
        public bool UrlAvailable
        {
            get
            {
                return RecordID.HasValue && (Action.Contains("Create") || Action.Contains("Modify"));
            }
        }
        public string UrlClass
        {
            get
            {
                if (UrlAvailable)
                    return "";
                return "hidden";
            }
        }
        public string UrlController
        {
            get
            {
                if (UrlAvailable)
                {
                    if (Action.Contains(DbAction.Bussiness.Name))
                        return "Bussiness";
                    if (Action.Contains(DbAction.Delivery.Name))
                        return "Delivery";
                    else if (Action.Contains(DbAction.ClientType.Name))
                        return "ClientType";
                    else if (Action.Contains(DbAction.Client.Name))
                        return "Client";
                    else if (Action.Contains(DbAction.Employee.Name))
                        return "Employee";
                    else if (Action.Contains(DbAction.Import.Name))
                        return "Import";
                    else if (Action.Contains(DbAction.Export.Name))
                        return "Export";
                    else if (Action.Contains(DbAction.Login.Name))
                        return "Login";
                    else if (Action.Contains(DbAction.Order.Name))
                        return "Order";
                    else if (Action.Contains(DbAction.Product.Name))
                        return "Product";
                    else if (Action.Contains(DbAction.Store.Name))
                        return "Setting";
                    else if (Action.Contains(DbAction.Supplier.Name))
                        return "Supplier";
                    else if (Action.Contains(DbAction.Tag.Name))
                        return "Tag";
                    else if (Action.Contains(DbAction.Warehouse.Name))
                        return "Warehouse";
                    else if (Action.Contains(DbAction.Income.Name))
                        return "Income";
                    else if (Action.Contains(DbAction.Outcome.Name))
                        return "Outcome";
                    else if (Action.Contains(DbAction.Warranty.Name))
                        return "Warranty";
                    else if (Action.Contains(DbAction.Repair.Name))
                        return "Repair";
                }
                return "";
            }
        }
        public string UrlAction
        {
            get
            {
                if (UrlAvailable)
                {
                    if (Action.Contains(DbAction.ClientType.Name))
                        return "List";
                    if (Action.Contains(DbAction.Delivery.Name))
                        return "List";
                    else if (Action.Contains(DbAction.Login.Name))
                        return "List";
                    else if (Action.Contains(DbAction.Store.Name))
                        return "Store";
                    else if (Action.Contains(DbAction.Supplier.Name))
                        return "List";
                    else if (Action.Contains(DbAction.Tag.Name))
                        return "List";
                    else if (Action.Contains(DbAction.Warehouse.Name))
                        return "List";
                    return "Detail";
                }
                return "";
            }
        }
        public string UrlFor
        {
            get
            {
                if (UrlAvailable)
                {
                    if (Action.Contains(DbAction.ClientType.Name))
                        return @"for=\""main-content\""";
                    if (Action.Contains(DbAction.Delivery.Name))
                        return @"for=\""main-content\""";
                    else if (Action.Contains(DbAction.Login.Name))
                        return @"for=\""main-content\""";
                    else if (Action.Contains(DbAction.Store.Name))
                        return @"for=\""main-content\""";
                    else if (Action.Contains(DbAction.Supplier.Name))
                        return @"for=\""main-content\""";
                    else if (Action.Contains(DbAction.Tag.Name))
                        return @"for=\""main-content\""";
                    else if (Action.Contains(DbAction.Warehouse.Name))
                        return @"for=\""main-content\""";
                    else if (Action.Contains(DbAction.Import.Name))
                        return "";
                    else if (Action.Contains(DbAction.Export.Name))
                        return "";
                    else if (Action.Contains(DbAction.Order.Name))
                        return "";
                    else if (Action.Contains(DbAction.Warranty.Name))
                        return "";
                    else if (Action.Contains(DbAction.Repair.Name))
                        return "";
                    return @"for=\""pop-up-content\""";
                }
                return "";
            }
        }
        public string ActionName
        {
            get
            {
                var result = "";
                if (Action.Contains("Create"))
                    result += "Tạo";
                if (Action.Contains("Modify"))
                    result += "Cập nhật";
                else if (Action.Contains("Remove"))
                    result += "Xóa";

                if (Action.Contains(DbAction.Bussiness.Name))
                    result += " thông tin công ty/đơn vị";
                else if (Action.Contains(DbAction.ClientType.Name))
                    result += " loại khác hàng";
                else if (Action.Contains(DbAction.Client.Name))
                    result += " khách hàng";
                else if (Action.Contains(DbAction.Delivery.Name))
                    result += " đơn vị giao hàng";
                else if (Action.Contains(DbAction.Employee.Name))
                    result += " nhân viên";
                else if (Action.Contains(DbAction.Import.Name))
                    result += " phiếu nhập kho";
                else if (Action.Contains(DbAction.Export.Name))
                    result += " phiếu xuất kho";
                else if (Action.Contains(DbAction.Login.Name))
                    result += " tài khoản đăng nhập";
                else if (Action.Contains(DbAction.Order.Name))
                    result += " hóa đơn";
                else if (Action.Contains(DbAction.Product.Name))
                    result += " sản phẩm";
                else if (Action.Contains(DbAction.Store.Name))
                    result += " cửa hàng";
                else if (Action.Contains(DbAction.Supplier.Name))
                    result += " nhà cung cấp";
                else if (Action.Contains(DbAction.Tag.Name))
                    result += " loại sản phẩm";
                else if (Action.Contains(DbAction.Warehouse.Name))
                    result += " kho";
                else if (Action.Contains(DbAction.Income.Name))
                    result += " phiếu thu";
                else if (Action.Contains(DbAction.Outcome.Name))
                    result += " phiếu chi";
                else if (Action.Contains(DbAction.Warranty.Name))
                    result += " phiếu bảo hành";
                else if (Action.Contains(DbAction.Repair.Name))
                    result += " phiếu sửa chữa";
                return result;
            }
        }
        public static string TableFromAction(string action)
        {
            if (action.Contains(DbAction.Bussiness.Name))
                return "Bussiness";
            else if (action.Contains(DbAction.ClientType.Name))
                return "Type";
            else if (action.Contains(DbAction.Client.Name))
                return "Client";
            else if (action.Contains(DbAction.Delivery.Name))
                return "Delivery";
            else if (action.Contains(DbAction.Employee.Name))
                return "Employee";
            else if (action.Contains(DbAction.Import.Name))
                return "Import";
            else if (action.Contains(DbAction.Export.Name))
                return "Export";
            else if (action.Contains(DbAction.Login.Name))
                return "Login";
            else if (action.Contains(DbAction.Order.Name))
                return "[Order]";
            else if (action.Contains(DbAction.Product.Name))
                return "Product";
            else if (action.Contains(DbAction.Store.Name))
                return "Store";
            else if (action.Contains(DbAction.Supplier.Name))
                return "Supplier";
            else if (action.Contains(DbAction.Tag.Name))
                return "Tag";
            else if (action.Contains(DbAction.Warehouse.Name))
                return "Warehouse";
            else if (action.Contains(DbAction.Income.Name))
                return "Income";
            else if (action.Contains(DbAction.Outcome.Name))
                return "Outcome";
            else if (action.Contains(DbAction.Warranty.Name))
                return "Warranty";
            else if (action.Contains(DbAction.Repair.Name))
                return "Repair";
            return "";
        }
    }
}