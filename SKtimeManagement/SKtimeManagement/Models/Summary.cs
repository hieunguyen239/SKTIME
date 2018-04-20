using Dapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SKtimeManagement
{
    public class SummaryAdmin
    {
        public SummaryAdmin() { }
        public List<Log> Logs { get; set; }
        public List<Popular> PopularProductsByQuantity { get; set; }
        public List<Popular> PopularTagsByQuantity { get; set; }
        public List<ExportRecord> PendingExports { get; set; }
        public static SummaryAdmin Get(int bussinessID)
        {
            try
            {
                using (var con = Repo.DB.SKtimeManagement)
                {
                    var now = DateTime.Now;
                    var result = new SummaryAdmin();
                    result.Logs = con.Query<Log>(
                        String.Format(
                            @"select top 100 l.*, e.Name as [EmployeeName] 
                            from Log l join Employee e on l.EmployeeID = e.ID 
                            where e.BussinessID = {0} and (Action like N'%create%' or Action like N'%modify%' or Action like N'%remove%') 
                            order by l.ID desc", bussinessID)).ToList();
                    result.PopularProductsByQuantity = con.Query<Popular>(
                        String.Format(
                            @"select top 100 e.ProductID as [ID], p.Name, sum(e.Quantity) as [TotalQuantity], sum(e.Quantity * e.Price) as [Revenue]
                            from ExportProduct e join Product p on e.ProductID = p.ID 
                            where p.BussinessID = {0} and e.OrderID is not null group by e.ProductID, p.Name 
                            order by sum(e.Quantity) desc", 
                            bussinessID)).ToList();
                    result.PopularTagsByQuantity = con.Query<Popular>(
                        String.Format(
                            @"select top 100 t.Name, sum(e.Quantity) as [TotalQuantity], sum(e.Quantity * e.Price)  as [Revenue]
                            from Tag t join ProductTag pt on t.ID = pt.TagID join ExportProduct e on pt.ProductID = e.ProductID join Product p on e.ProductID = p.ID 
                            where p.BussinessID = 1 and e.OrderID is not null 
                            group by t.Name order by sum(e.Quantity) desc", 
                            bussinessID)).ToList();
                    result.PendingExports = con.Query<ExportRecord>(
                        String.Format(
                            @"select e.ID, e.EmployeeID, e.BussinessID, e.WarehouseID, e.Code, e.SubmitDate, e.Note, e.Status, e.ToWarehouseID, tw.Name as [ToWarehouseName], w.Name as [WarehouseName], em.Name as [EmployeeName]
                            from Export e 
                                join Warehouse w on e.WarehouseID = w.ID and w.Status = 'active'
                                join Employee em on e.EmployeeID = em.ID
                                join ExportProduct p on e.ID = p.ExportID
                                join Warehouse tw on e.ToWarehouseID = tw.ID and tw.Status = 'active'
                            where e.BussinessID = {0} and e.Removed = 0 and e.Status <> N'{1}'
                            group by e.ID, e.EmployeeID, e.BussinessID, e.WarehouseID, e.Code, e.SubmitDate, e.Note, e.Status, e.ToWarehouseID, tw.Name, w.Name, em.Name
                            order by e.ID desc", bussinessID, ExportStatus.Exported)).ToList();
                    return result;
                }
            }
            catch
            {
                return new SummaryAdmin();
            }
        }
    }
    public class Summary
    {
        public List<OrderRecord> CreatedOrders { get; set; }
        public List<OrderRecord> NewOrders { get; set; }
        public List<ExportRecord> PendingExports { get; set; }
        public static Summary Get(int employeeID, int bussinessID, int loginID)
        {
            var result = new Summary();
            try
            {
                using (var con = Repo.DB.SKtimeManagement)
                {
                    var now = DateTime.Now;
                    var startMonth = new DateTime(now.Year, now.Month, 1, 0, 0, 0);
                    var endMonth = startMonth.AddMonths(1);
                    var startDay = new DateTime(now.Year, now.Month, now.Day, 0, 0, 0);
                    var endDay = startDay.AddDays(1);
                    result.CreatedOrders = con.Query<OrderRecord>(
                        String.Format(
                            @"select o.ID, o.EmployeeID, o.DeliveryID, o.WarehouseID, o.Code, o.SubmitDate, o.PayMethod, o.Receipt, o.Total, o.Discount, o.Paid, o.Note, o.Status, 
                                w.Name as [WarehouseName], c.ID as [ClientID], c.Code as [ClientCode], c.Name as [ClientName], c.Address as [ClientAddress], c.Phone as [ClientPhone], 
                                em.Name as [EmployeeName], del.Name as [DeliveryName], ex.ID as [ExportID], ex.Code as [ExportCode], t.Name as [ClientType], t.DiscountType as [DiscountType], t.DiscountValue as [DiscountValue]
                            from SKtimeManagement..[Order] o
                                join Warehouse w on o.WarehouseID = w.ID and w.Status = 'active'
                                join LoginWarehouse lw on w.ID = lw.WarehouseID and lw.LoginID = {3}
                                join ExportProduct p on o.ID = p.OrderID
                                left join Export ex on ex.ID = p.ExportID
                                join Employee em on o.EmployeeID = em.ID
                                left join Employee del on o.DeliveryID = del.ID
                                left join Client c on o.ClientID = c.ID
                                left join ClientType ct on c.ID = ct.ClientID
                                left join Type t on ct.TypeID = t.ID
                            where o.EmployeeID = {0} and o.Removed = 0 and o.SubmitDate between '{1}' and '{2}'
                            group by o.ID, o.EmployeeID, o.DeliveryID, o.WarehouseID, o.Code, o.SubmitDate, o.PayMethod, o.Receipt, o.Total, o.Discount, o.Paid, o.Note, o.Status, 
                                w.Name, c.ID, c.Code, c.Name, c.Address, c.Phone, em.Name, del.Name, ex.ID, ex.Code, t.Name, t.DiscountType, t.DiscountValue
                            order by o.ID desc", employeeID, startDay.ToString(Constants.DatabaseDatetimeString), endDay.ToString(Constants.DatabaseDatetimeString), loginID)).ToList();
                    result.NewOrders = con.Query<OrderRecord>(
                        String.Format(
                            @"select top 100 o.ID, o.EmployeeID, o.DeliveryID, o.WarehouseID, o.Code, o.SubmitDate, o.PayMethod, o.Receipt, o.Total, o.Discount, o.Paid, o.Note, o.Status, 
                                w.Name as [WarehouseName], c.ID as [ClientID], c.Code as [ClientCode], c.Name as [ClientName], c.Address as [ClientAddress], c.Phone as [ClientPhone], 
                                em.Name as [EmployeeName], del.Name as [DeliveryName], ex.ID as [ExportID], ex.Code as [ExportCode], t.Name as [ClientType], t.DiscountType as [DiscountType], t.DiscountValue as [DiscountValue]
                            from SKtimeManagement..[Order] o
                                join Warehouse w on o.WarehouseID = w.ID and w.Status = 'active'
                                join LoginWarehouse lw on w.ID = lw.WarehouseID and lw.LoginID = {1}
                                join ExportProduct p on o.ID = p.OrderID
                                left join Export ex on ex.ID = p.ExportID
                                join Employee em on o.EmployeeID = em.ID
                                left join Employee del on o.DeliveryID = del.ID
                                left join Client c on o.ClientID = c.ID
                                left join ClientType ct on c.ID = ct.ClientID
                                left join Type t on ct.TypeID = t.ID
                            where o.BussinessID = {0} and o.Removed = 0 and o.SubmitDate between '{2}' and '{3}'
                            group by o.ID, o.EmployeeID, o.DeliveryID, o.WarehouseID, o.Code, o.SubmitDate, o.PayMethod, o.Receipt, o.Total, o.Discount, o.Paid, o.Note, o.Status, 
                                w.Name, c.ID, c.Code, c.Name, c.Address, c.Phone, em.Name, del.Name, ex.ID, ex.Code, t.Name, t.DiscountType, t.DiscountValue
                            order by o.ID desc", bussinessID, loginID, startDay.ToString(Constants.DatabaseDatetimeString), endDay.ToString(Constants.DatabaseDatetimeString))).ToList();
                    result.PendingExports = con.Query<ExportRecord>(
                        String.Format(
                            @"select e.ID, e.EmployeeID, e.BussinessID, e.WarehouseID, e.Code, e.SubmitDate, e.Note, e.Status, e.ToWarehouseID, tw.Name as [ToWarehouseName], w.Name as [WarehouseName], em.Name as [EmployeeName]
                            from Export e 
                                join Warehouse w on e.WarehouseID = w.ID and w.Status = 'active'
                                join Employee em on e.EmployeeID = em.ID
                                join ExportProduct p on e.ID = p.ExportID
                                join Warehouse tw on e.ToWarehouseID = tw.ID and tw.Status = 'active'
                            where e.BussinessID = {0} and e.Removed = 0 and e.Status <> N'{1}' and e.ToWarehouseID in (select WarehouseID from LoginWarehouse where LoginID = {2})
                            group by e.ID, e.EmployeeID, e.BussinessID, e.WarehouseID, e.Code, e.SubmitDate, e.Note, e.Status, e.ToWarehouseID, tw.Name, w.Name, em.Name
                            order by e.ID desc", bussinessID, ExportStatus.Exported, loginID)).ToList();
                }
            }
            catch (Exception e) { }
            return result;
        }
    }
    public class Popular
    {
        public string Name { get; set; }
        public int TotalQuantity { get; set; }
        public decimal Revenue { get; set; }
        public string RevenueString
        {
            get
            {
                return Revenue.GetCurrencyString();
            }
        }
    }
    public class TransactionSummary
    {
        public List<Transaction> Transactions { get; set; }
        public IncomeList Incomes { get; set; }
        public OutcomeList Outcomes { get; set; }
    }
}