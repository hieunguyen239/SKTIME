using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SKtimeManagement
{
    public class SalaryAction
    {
        public const string Calculate = "Calculate Salary";
    }
    public class ReportAction
    {
        public const string Summary = "Summary";
        public const string Detail = "Detail";
        public const string Product = "Product";
        public const string Salary = "Salary";
        public const string Client = "Client";
        public const string ProductPart = "ProductPart";
    }
    public class DbAction
    {
        public DbAction(string name)
        {
            Name = name;
        }
        public string Name { get; set; }
        public string View { get { return String.Format("View {0}", Name); } }
        public string Create { get { return String.Format("Create {0}", Name); } }
        public string Modify { get { return String.Format("Modify {0}", Name); } }
        public string Remove { get { return String.Format("Remove {0}", Name); } }
        public string Report { get { return String.Format("Report {0}", Name); } }
        public static DbAction Bussiness { get { return new DbAction("Bussiness"); } }
        public static DbAction Client { get { return new DbAction("Client"); } }
        public static DbAction ClientType { get { return new DbAction("Client Type"); } }
        public static DbAction Delivery { get { return new DbAction("Delivery"); } }
        public static DbAction Employee { get { return new DbAction("Employee"); } }
        public static DbAction Import { get { return new DbAction("Import"); } }
        public static DbAction Export { get { return new DbAction("Export"); } }
        public static DbAction Login { get { return new DbAction("Login"); } }
        public static DbAction Order { get { return new DbAction("Order"); } }
        public static DbAction Product { get { return new DbAction("Product"); } }
        public static DbAction Store { get { return new DbAction("Store"); } }
        public static DbAction Supplier { get { return new DbAction("Supplier"); } }
        public static DbAction Tag { get { return new DbAction("Tag"); } }
        public static DbAction Warehouse { get { return new DbAction("Warehouse"); } }
        public static DbAction Income { get { return new DbAction("Income"); } }
        public static DbAction Outcome { get { return new DbAction("Outcome"); } }
        public static DbAction Warranty { get { return new DbAction("Warranty"); } }
        public static DbAction Repair { get { return new DbAction("Repair"); } }
        public static DbAction Transaction { get { return new DbAction("Transaction"); } }
        public static string[] GetPermission(string controller, string action)
        {
            var permission = new string[] { };
            switch (controller)
            {
                case "Client":
                    switch (action)
                    {
                        case "Download": permission = new string[] { DbAction.Client.View }; break;
                        case "Find": permission = new string[] { DbAction.Client.View }; break;
                        case "Discount": permission = new string[] { DbAction.Client.View }; break;
                        case "List": permission = new string[] { DbAction.Client.View }; break;
                        case "KeyList": permission = new string[] { DbAction.Client.View }; break;
                        case "Detail": permission = new string[] { DbAction.Client.View }; break;
                        case "Create": permission = new string[] { DbAction.Client.Create }; break;
                        case "Save": permission = new string[] { DbAction.Client.Create, DbAction.Client.Modify }; break;
                        case "Update": permission = new string[] { DbAction.Client.Modify }; break;
                        case "Remove": permission = new string[] { DbAction.Client.Remove }; break;
                        default: break;
                    }
                    break;
                case "ClientType":
                    switch (action)
                    {
                        case "List": permission = new string[] { DbAction.ClientType.View }; break;
                        case "KeyList": permission = new string[] { DbAction.ClientType.View }; break;
                        case "Create": permission = new string[] { DbAction.ClientType.Create }; break;
                        case "Save": permission = new string[] { DbAction.ClientType.Create, DbAction.ClientType.Modify }; break;
                        case "Update": permission = new string[] { DbAction.ClientType.Modify }; break;
                        case "Remove": permission = new string[] { DbAction.ClientType.Remove }; break;
                        default: break;
                    }
                    break;
                case "Delivery":
                    switch (action)
                    {
                        case "Find": permission = new string[] { DbAction.Delivery.View }; break;
                        case "List": permission = new string[] { DbAction.Delivery.View }; break;
                        case "KeyList": permission = new string[] { DbAction.Delivery.View }; break;
                        case "Create": permission = new string[] { DbAction.Delivery.Create }; break;
                        case "Save": permission = new string[] { DbAction.Delivery.Create, DbAction.Delivery.Modify }; break;
                        case "Update": permission = new string[] { DbAction.Delivery.Modify }; break;
                        case "Remove": permission = new string[] { DbAction.Delivery.Remove }; break;
                        default: break;
                    }
                    break;
                case "Employee":
                    switch (action)
                    {
                        case "Download": permission = new string[] { DbAction.Employee.View }; break;
                        case "Find": permission = new string[] { DbAction.Employee.View }; break;
                        case "List": permission = new string[] { DbAction.Employee.View }; break;
                        case "DataList": permission = new string[] { DbAction.Employee.View }; break;
                        case "UnassignedList": permission = new string[] { DbAction.Employee.View }; break;
                        case "Detail": permission = new string[] { DbAction.Employee.View }; break;
                        case "Create": permission = new string[] { DbAction.Employee.Create }; break;
                        case "Save": permission = new string[] { DbAction.Employee.Create, DbAction.Employee.Modify }; break;
                        case "Update": permission = new string[] { DbAction.Employee.Modify }; break;
                        case "Remove": permission = new string[] { DbAction.Employee.Remove }; break;
                        case "GetFine": permission = new string[] { DbAction.Employee.View }; break;
                        case "AddFine": permission = new string[] { DbAction.Employee.View }; break;
                        case "RemoveFine": permission = new string[] { DbAction.Employee.View }; break;
                        default: break;
                    }
                    break;
                case "Export":
                    switch (action)
                    {
                        case "AuthorizedWarehouse": permission = new string[] { DbAction.Export.View }; break;
                        case "Exported": permission = new string[] { DbAction.Export.View }; break;
                        case "Delivered": permission = new string[] { DbAction.Export.View }; break;
                        case "ProductList": permission = new string[] { DbAction.Export.Create, DbAction.Export.Modify }; break;
                        case "Delete": permission = new string[] { DbAction.Export.Remove }; break;
                        case "Create": permission = new string[] { DbAction.Export.Create }; break;
                        case "Add": permission = new string[] { DbAction.Export.Create, DbAction.Export.Modify }; break;
                        case "Remove": permission = new string[] { DbAction.Export.Create, DbAction.Export.Modify }; break;
                        case "Submit": permission = new string[] { DbAction.Export.Create, DbAction.Export.Modify }; break;
                        case "Detail": permission = new string[] { DbAction.Export.View }; break;
                        case "History": permission = new string[] { DbAction.Export.View }; break;
                        case "UploadFile": permission = new string[] { DbAction.Export.Create, DbAction.Export.Modify }; break;
                        case "UploadTemplate": permission = new string[] { DbAction.Export.Create, DbAction.Export.Modify }; break;
                        case "Update": permission = new string[] { DbAction.Export.Modify }; break;
                        case "Copy": permission = new string[] { DbAction.Export.Create }; break;
                        default: break;
                    }
                    break;
                case "Import":
                    switch (action)
                    {
                        case "AuthorizedWarehouse": permission = new string[] { DbAction.Import.View }; break;
                        case "ProductList": permission = new string[] { DbAction.Import.Create, DbAction.Import.Modify }; break;
                        case "Delete": permission = new string[] { DbAction.Import.Remove }; break;
                        case "Create": permission = new string[] { DbAction.Import.Create }; break;
                        case "CreateFromExport": permission = new string[] { DbAction.Import.Create }; break;
                        case "Add": permission = new string[] { DbAction.Import.Create, DbAction.Import.Modify }; break;
                        case "Remove": permission = new string[] { DbAction.Import.Create, DbAction.Import.Modify }; break;
                        case "Submit": permission = new string[] { DbAction.Import.Create, DbAction.Import.Modify }; break;
                        case "Detail": permission = new string[] { DbAction.Import.View }; break;
                        case "History": permission = new string[] { DbAction.Import.View }; break;
                        case "UploadFile": permission = new string[] { DbAction.Import.Create, DbAction.Import.Modify }; break;
                        case "UploadTemplate": permission = new string[] { DbAction.Import.Create, DbAction.Import.Modify }; break;
                        case "Update": permission = new string[] { DbAction.Import.Modify }; break;
                        case "Copy": permission = new string[] { DbAction.Import.Create }; break;
                        default: break;
                    }
                    break;
                case "Login":
                    switch (action)
                    {
                        case "Find": permission = new string[] { DbAction.Login.View }; break;
                        case "List": permission = new string[] { DbAction.Login.View }; break;
                        case "UnassignedList": permission = new string[] { DbAction.Login.View }; break;
                        case "DataList": permission = new string[] { DbAction.Login.View }; break;
                        case "Permission": permission = new string[] { DbAction.Login.View }; break;
                        case "Create": permission = new string[] { DbAction.Login.Create }; break;
                        case "Save": permission = new string[] { DbAction.Login.Create, DbAction.Login.Modify }; break;
                        case "Update": permission = new string[] { DbAction.Login.Modify }; break;
                        case "Remove": permission = new string[] { DbAction.Login.Remove }; break;
                        case "AddWarehouse": permission = new string[] { DbAction.Login.Create, DbAction.Login.Modify }; break;
                        case "RemoveWarehouse": permission = new string[] { DbAction.Login.Create, DbAction.Login.Modify }; break;
                        case "WarehouseList": permission = new string[] { DbAction.Login.View }; break;
                        case "AuthorizedWarehouse": permission = new string[] { DbAction.Login.View }; break;
                        default: break;
                    }
                    break;
                case "Order":
                    switch (action)
                    {
                        case "AuthorizedWarehouse": permission = new string[] { DbAction.Order.View }; break;
                        case "ProductList": permission = new string[] { DbAction.Order.Create, DbAction.Order.Modify }; break;
                        case "Create": permission = new string[] { DbAction.Order.Create }; break;
                        case "Add": permission = new string[] { DbAction.Order.Create, DbAction.Order.Modify }; break;
                        case "Remove": permission = new string[] { DbAction.Order.Create, DbAction.Order.Modify }; break;
                        case "Submit": permission = new string[] { DbAction.Order.Create, DbAction.Order.Modify }; break;
                        case "Detail": permission = new string[] { DbAction.Order.View }; break;
                        case "History": permission = new string[] { DbAction.Order.View }; break;
                        case "UploadFile": permission = new string[] { DbAction.Order.Create, DbAction.Order.Modify }; break;
                        case "UploadTemplate": permission = new string[] { DbAction.Order.Create, DbAction.Order.Modify }; break;
                        case "Update": permission = new string[] { DbAction.Order.Modify }; break;
                        case "Delete": permission = new string[] { DbAction.Order.Remove }; break;
                        case "Exchange": permission = new string[] { DbAction.Order.Modify }; break;
                        case "Copy": permission = new string[] { DbAction.Order.Create }; break;
                        case "ExportOrder": permission = new string[] { DbAction.Order.Modify }; break;
                        case "GetTransactions": permission = new string[] { DbAction.Order.View }; break;
                        case "AddTransaction": permission = new string[] { DbAction.Transaction.Modify }; break;
                        case "RemoveTransaction": permission = new string[] { DbAction.Transaction.Modify }; break;
                        default: break;
                    }
                    break;
                case "Product":
                    switch (action)
                    {
                        case "FindDownload": permission = new string[] { DbAction.Product.View }; break;
                        case "DownloadAll": permission = new string[] { DbAction.Product.View }; break;
                        case "AuthorizedWarehouse": permission = new string[] { DbAction.Product.View }; break;
                        case "Check": permission = new string[] { DbAction.Product.View }; break;
                        case "Find": permission = new string[] { DbAction.Product.View }; break;
                        case "FindFinish": permission = new string[] { DbAction.Product.View }; break;
                        case "DownloadCheck": permission = new string[] { DbAction.Product.View }; break;
                        case "NewUniqueCode": permission = new string[] { DbAction.Product.Create, DbAction.Product.Modify }; break;
                        case "Detail": permission = new string[] { DbAction.Product.View }; break;
                        case "Download": permission = new string[] { DbAction.Product.View }; break;
                        case "List": permission = new string[] { DbAction.Product.View }; break;
                        case "ListByTag": permission = new string[] { DbAction.Product.View }; break;
                        case "Create": permission = new string[] { DbAction.Product.Create }; break;
                        case "Save": permission = new string[] { DbAction.Product.Create, DbAction.Product.Modify }; break;
                        case "Update": permission = new string[] { DbAction.Product.Modify }; break;
                        case "Remove": permission = new string[] { DbAction.Product.Remove }; break;
                        case "AddTag": permission = new string[] { DbAction.Product.Create, DbAction.Product.Modify }; break;
                        case "RemoveTag": permission = new string[] { DbAction.Product.Create, DbAction.Product.Modify }; break;
                        case "Quantity": permission = new string[] { DbAction.Product.Create, DbAction.Product.Modify }; break;
                        case "Quantities": permission = new string[] { DbAction.Product.Create, DbAction.Product.Modify }; break;
                        case "EditQuantity": permission = new string[] { DbAction.Product.Create, DbAction.Product.Modify }; break;
                        case "SaveQuantity": permission = new string[] { DbAction.Product.Create, DbAction.Product.Modify }; break;
                        case "RemoveQuantity": permission = new string[] { DbAction.Product.Create, DbAction.Product.Modify }; break;
                        case "Import": permission = new string[] { DbAction.Product.Create, DbAction.Product.Modify }; break;
                        case "UploadImportData": permission = new string[] { DbAction.Product.Create, DbAction.Product.Modify }; break;
                        case "SubmitImportData": permission = new string[] { DbAction.Product.Create, DbAction.Product.Modify }; break;
                        case "ImportTemplate": permission = new string[] { DbAction.Product.Create, DbAction.Product.Modify }; break;
                        default: break;
                    }
                    break;
                case "Setting":
                    switch (action)
                    {
                        case "Bussiness": permission = new string[] { DbAction.Bussiness.Modify }; break;
                        case "BussinessUpate": permission = new string[] { DbAction.Bussiness.Modify }; break;
                        case "FindStore": permission = new string[] { DbAction.Store.View }; break;
                        case "Store": permission = new string[] { DbAction.Store.View }; break;
                        case "StoreList": permission = new string[] { DbAction.Store.View }; break;
                        case "StoreCreate": permission = new string[] { DbAction.Store.Create }; break;
                        case "StoreUpdate": permission = new string[] { DbAction.Store.Modify }; break;
                        case "StoreSave": permission = new string[] { DbAction.Store.Create, DbAction.Store.Modify }; break;
                        case "StoreRemove": permission = new string[] { DbAction.Store.Remove }; break;
                        default: break;
                    }
                    break;
                case "Supplier":
                    switch (action)
                    {
                        case "Find": permission = new string[] { DbAction.Supplier.View }; break;
                        case "List": permission = new string[] { DbAction.Supplier.View }; break;
                        case "KeyList": permission = new string[] { DbAction.Supplier.View }; break;
                        case "Create": permission = new string[] { DbAction.Supplier.Create }; break;
                        case "Save": permission = new string[] { DbAction.Supplier.Create, DbAction.Supplier.Modify }; break;
                        case "Update": permission = new string[] { DbAction.Supplier.Modify }; break;
                        case "Remove": permission = new string[] { DbAction.Supplier.Remove }; break;
                        default: break;
                    }
                    break;
                case "Tag":
                    switch (action)
                    {
                        case "Find": permission = new string[] { DbAction.Tag.View }; break;
                        case "List": permission = new string[] { DbAction.Tag.View }; break;
                        case "KeyList": permission = new string[] { DbAction.Tag.View }; break;
                        case "Create": permission = new string[] { DbAction.Tag.Create }; break;
                        case "Save": permission = new string[] { DbAction.Tag.Create, DbAction.Tag.Modify }; break;
                        case "Update": permission = new string[] { DbAction.Tag.Modify }; break;
                        case "Remove": permission = new string[] { DbAction.Tag.Remove }; break;
                        default: break;
                    }
                    break;
                case "Warehouse":
                    switch (action)
                    {
                        case "List": permission = new string[] { DbAction.Warehouse.View }; break;
                        case "KeyList": permission = new string[] { DbAction.Warehouse.View }; break;
                        case "Create": permission = new string[] { DbAction.Warehouse.Create }; break;
                        case "Save": permission = new string[] { DbAction.Warehouse.Create, DbAction.Warehouse.Modify }; break;
                        case "Update": permission = new string[] { DbAction.Warehouse.Modify }; break;
                        case "Remove": permission = new string[] { DbAction.Warehouse.Remove }; break;
                        default: break;
                    }
                    break;
                case "Income":
                    switch (action)
                    {
                        case "Download": permission = new string[] { DbAction.Income.View }; break;
                        case "AuthorizedWarehouse": permission = new string[] { DbAction.Income.View }; break;
                        case "Detail": permission = new string[] { DbAction.Income.View }; break;
                        case "List": permission = new string[] { DbAction.Income.View }; break;
                        case "Create": permission = new string[] { DbAction.Income.Create }; break;
                        case "Save": permission = new string[] { DbAction.Income.Create, DbAction.Income.Modify }; break;
                        case "Update": permission = new string[] { DbAction.Income.Modify }; break;
                        case "Remove": permission = new string[] { DbAction.Income.Remove }; break;
                        default: break;
                    }
                    break;
                case "Outcome":
                    switch (action)
                    {
                        case "Download": permission = new string[] { DbAction.Outcome.View }; break;
                        case "AuthorizedWarehouse": permission = new string[] { DbAction.Outcome.View }; break;
                        case "Detail": permission = new string[] { DbAction.Outcome.View }; break;
                        case "List": permission = new string[] { DbAction.Outcome.View }; break;
                        case "Create": permission = new string[] { DbAction.Outcome.Create }; break;
                        case "Save": permission = new string[] { DbAction.Outcome.Create, DbAction.Outcome.Modify }; break;
                        case "Update": permission = new string[] { DbAction.Outcome.Modify }; break;
                        case "Remove": permission = new string[] { DbAction.Outcome.Remove }; break;
                        default: break;
                    }
                    break;
                case "Report":
                    switch (action)
                    {
                        case "Summary": permission = new string[] { ReportAction.Summary }; break;
                        case "Detail": permission = new string[] { ReportAction.Detail }; break;
                        case "SaleDownload": permission = new string[] { ReportAction.Detail }; break;
                        case "RevenueDownload": permission = new string[] { ReportAction.Detail }; break;
                        case "Download": permission = new string[] { ReportAction.Detail }; break;
                        case "Product": permission = new string[] { ReportAction.Product }; break;
                        case "ProductDownload": permission = new string[] { ReportAction.Product }; break;
                        case "Salary": permission = new string[] { ReportAction.Salary }; break;
                        case "SalaryDownload": permission = new string[] { ReportAction.Salary }; break;
                        case "Client": permission = new string[] { ReportAction.Client }; break;
                        case "ProductPart": permission = new string[] { ReportAction.ProductPart }; break;
                        default: break;
                    }
                    break;
                case "Salary":
                    switch (action)
                    {
                        case "Detail": permission = new string[] { SalaryAction.Calculate }; break;
                        case "History": permission = new string[] { SalaryAction.Calculate }; break;
                        case "FindSalary": permission = new string[] { SalaryAction.Calculate }; break;
                        case "Calculate": permission = new string[] { SalaryAction.Calculate }; break;
                        case "ForEmployee": permission = new string[] { SalaryAction.Calculate }; break;
                        case "FindEmployee": permission = new string[] { SalaryAction.Calculate }; break;
                        case "AddOffset": permission = new string[] { SalaryAction.Calculate }; break;
                        case "RemoveOffset": permission = new string[] { SalaryAction.Calculate }; break;
                        case "AddDayOff": permission = new string[] { SalaryAction.Calculate }; break;
                        case "RemoveDayOff": permission = new string[] { SalaryAction.Calculate }; break;
                        default: break;
                    }
                    break;
                case "Warranty":
                    switch (action)
                    {
                        case "SearchProduct": permission = new string[] { DbAction.Warranty.View }; break;
                        case "GetProducts": permission = new string[] { DbAction.Warranty.View }; break;
                        case "AddProduct": permission = new string[] { DbAction.Warranty.View }; break;
                        case "RemoveProduct": permission = new string[] { DbAction.Warranty.View }; break;
                        case "Create": permission = new string[] { DbAction.Warranty.Create }; break;
                        case "OrderList": permission = new string[] { DbAction.Warranty.Create }; break;
                        case "SelectOrder": permission = new string[] { DbAction.Warranty.Create }; break;
                        case "SelectProduct": permission = new string[] { DbAction.Warranty.Create }; break;
                        case "ProductList": permission = new string[] { DbAction.Warranty.Create }; break;
                        case "OldOrder": permission = new string[] { DbAction.Warranty.Create }; break;
                        case "Submit": permission = new string[] { DbAction.Warranty.Create, DbAction.Warranty.Modify }; break;
                        case "Detail": permission = new string[] { DbAction.Warranty.View }; break;
                        case "History": permission = new string[] { DbAction.Warranty.View }; break;
                        case "WarrantHistory": permission = new string[] { DbAction.Warranty.View }; break;
                        case "Update": permission = new string[] { DbAction.Warranty.Modify }; break;
                        case "Remove": permission = new string[] { DbAction.Warranty.Remove }; break;
                        case "GetMechanicNote": permission = new string[] { DbAction.Warranty.View }; break;
                        case "SaveMechanicNote": permission = new string[] { DbAction.Warranty.View }; break;
                        case "RemoveMechanicNote": permission = new string[] { DbAction.Warranty.View }; break;
                        case "GetInternalNote": permission = new string[] { DbAction.Warranty.View }; break;
                        case "SaveInternalNote": permission = new string[] { DbAction.Warranty.View }; break;
                        case "RemoveInternalNote": permission = new string[] { DbAction.Warranty.View }; break;
                        case "TransferDate": permission = new string[] { DbAction.Warranty.View }; break;
                        case "ReceivedDate": permission = new string[] { DbAction.Warranty.View }; break;
                        case "ProcessedDate": permission = new string[] { DbAction.Warranty.View }; break;
                        case "ReturnedDate": permission = new string[] { DbAction.Warranty.View }; break;
                        case "FinishDate": permission = new string[] { DbAction.Warranty.View }; break;
                        case "Download": permission = new string[] { DbAction.Warranty.View }; break;
                        case "GetTransactions": permission = new string[] { DbAction.Warranty.View }; break;
                        case "AddTransaction": permission = new string[] { DbAction.Transaction.Modify }; break;
                        case "RemoveTransaction": permission = new string[] { DbAction.Transaction.Modify }; break;
                        default: break;
                    }
                    break;
                case "Repair":
                    switch (action)
                    {
                        case "SearchProduct": permission = new string[] { DbAction.Repair.View }; break;
                        case "Create": permission = new string[] { DbAction.Repair.Create }; break;
                        case "GetProducts": permission = new string[] { DbAction.Repair.View }; break;
                        case "AddProduct": permission = new string[] { DbAction.Repair.View }; break;
                        case "RemoveProduct": permission = new string[] { DbAction.Repair.View }; break;
                        case "OtherProduct": permission = new string[] { DbAction.Repair.Create }; break;
                        case "SelectProduct": permission = new string[] { DbAction.Repair.Create }; break;
                        case "ProductList": permission = new string[] { DbAction.Repair.Create }; break;
                        case "Client": permission = new string[] { DbAction.Repair.Create }; break;
                        case "Submit": permission = new string[] { DbAction.Repair.Create, DbAction.Repair.Modify }; break;
                        case "Detail": permission = new string[] { DbAction.Repair.View }; break;
                        case "History": permission = new string[] { DbAction.Repair.View }; break;
                        case "RepairHistory": permission = new string[] { DbAction.Repair.View }; break;
                        case "Update": permission = new string[] { DbAction.Repair.Modify }; break;
                        case "Remove": permission = new string[] { DbAction.Repair.Remove }; break;
                        case "GetMechanicNote": permission = new string[] { DbAction.Repair.View }; break;
                        case "SaveMechanicNote": permission = new string[] { DbAction.Repair.View }; break;
                        case "RemoveMechanicNote": permission = new string[] { DbAction.Repair.View }; break;
                        case "GetInternalNote": permission = new string[] { DbAction.Repair.View }; break;
                        case "SaveInternalNote": permission = new string[] { DbAction.Repair.View }; break;
                        case "RemoveInternalNote": permission = new string[] { DbAction.Repair.View }; break;
                        case "TransferDate": permission = new string[] { DbAction.Repair.View }; break;
                        case "ReceivedDate": permission = new string[] { DbAction.Repair.View }; break;
                        case "ProcessedDate": permission = new string[] { DbAction.Repair.View }; break;
                        case "ReturnedDate": permission = new string[] { DbAction.Repair.View }; break;
                        case "FinishDate": permission = new string[] { DbAction.Repair.View }; break;
                        case "Download": permission = new string[] { DbAction.Repair.View }; break;
                        case "GetTransactions": permission = new string[] { DbAction.Repair.View }; break;
                        case "AddTransaction": permission = new string[] { DbAction.Transaction.Modify }; break;
                        case "RemoveTransaction": permission = new string[] { DbAction.Transaction.Modify }; break;
                        default: break;
                    }
                    break;
                default: break;
            }
            return permission;
        }
    }
}