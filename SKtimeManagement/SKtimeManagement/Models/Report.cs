using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Dapper;
using System.Web;
using System.Dynamic;

namespace SKtimeManagement
{
    public class ReportFilter
    {
        public int? WarehouseID { get; set; }
        public int? EmployeeID { get; set; }
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = Constants.DateTimeString)]
        public DateTime? From { get; set; }
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = Constants.DateTimeString)]
        public DateTime? To { get; set; }
        public int? Max { get; set; }
    }
    public enum ReportGraphType
    {
        Sale,
        Revenue,
        SaleRelative
    }
    public class ReportGraph
    {
        public ReportGraph(ReportGraphType type, DateTime from, DateTime to)
        {
            Type = type;
            From = new DateTime(from.Year, from.Month, from.Day, 0, 0, 0);
            var dif = to - from;
            if (dif.TotalHours <= 24)
            {
                _Step = 1;
                TimeFormat = "HH:mm";
            }
            else if (dif.TotalDays <= 31)
            {
                _Step = 24;
                TimeFormat = "dd/MM";
            }
            else if (dif.TotalDays <= 7 * 24)
            {
                _Step = 7 * 24;
                TimeFormat = "dd/MM";
            }
            else
            {
                _Step = 0;
                TimeFormat = "MM/yyyy";
            }
            Revenues = new List<GraphSalePoint>();
        }
        public ReportGraphType Type { get; set; }
        public string Name { get; set; }
        public List<WarehouseInfo> Warehouses { get; set; }
        public decimal Max { get; set; }
        public int Count { get; set; }
        public DateTime From { get; set; }
        public DateTime Current { get; set; }
        public int _Step { get; set; }
        public int Step
        {
            get
            {
                if (_Step > 0)
                    return _Step;
                else
                {
                    var next = Current.AddMonths(1);
                    return Convert.ToInt32((new DateTime(next.Year, next.Month, 1) - Current).TotalHours);
                }
            }
        }
        public string TimeFormat { get; set; }
        public List<GraphSalePoint> Revenues { get; set; }
        public static ReportGraph Create(Report report, ReportGraphType type = ReportGraphType.Sale)
        {
            var graph = new ReportGraph(type, report.Filter.From.Value, report.Filter.To.Value);
            graph.Current = graph.From;
            graph.Warehouses = report.Warehouses;
            var count = 0;
            while (true)
            {
                var to = graph.Current.AddHours(graph.Step);
                var total = 0m;
                foreach (var wh in report.Warehouses)
                {
                    var paidOrders = report.PaidOrders.Where(i => i.WarehouseID == wh.ID && i.SubmitDate >= graph.Current && i.SubmitDate <= to).Sum(i => i.Paid);
                    var incomes = report.Incomes.Where(i => i.WarehouseID == wh.ID && i.SubmitDate >= graph.Current && i.SubmitDate <= to).Sum(i => i.Amount);
                    var outcomes = report.Outcomes.Where(i => i.WarehouseID == wh.ID && i.SubmitDate >= graph.Current && i.SubmitDate <= to).Sum(i => i.Amount);
                    var employees = report.Employees.Where(i => i.WarehouseID == wh.ID);
                    var salaries = report.Salaries.Where(i => employees.FirstOrDefault(e => e.ID == i.EmployeeID) != null && i.Month.HasValue && i.Month.Value >= graph.Current && i.Month.Value < to).Sum(i => i.CalculatedTotal);
                    var revenue = 0m;
                    switch (type)
                    {
                        case ReportGraphType.Revenue: revenue = (paidOrders + incomes) * 0.65m - outcomes - salaries; break;
                        case ReportGraphType.Sale: revenue = paidOrders; break;
                        case ReportGraphType.SaleRelative:
                            var store = report.Stores.FirstOrDefault(i => i.WarehouseID == wh.ID);
                            revenue = store != null && store.SalePoint > 0 ? Math.Round(paidOrders / store.SalePoint * 100, 2) : 0m; break;
                        default: break;
                    }
                    graph.Revenues.Add(new GraphSalePoint(graph.Current, revenue, wh.ID));
                    total += revenue;
                }
                if (!report.Filter.WarehouseID.HasValue)
                {
                    graph.Revenues.Add(new GraphSalePoint(graph.Current, total));
                }
                graph.Current = graph.Current.AddHours(graph.Step);
                count++;
                if (graph.Current >= report.Filter.To.Value || count == 31)
                    break;
            }
            graph.Count = count;
            graph.Max = graph.Revenues.Max(i => i.Value);
            return graph;
        }
    }
    public class GraphSalePoint
    {
        public GraphSalePoint(DateTime time, decimal value, int? warehouse = null)
        {
            Warehouse = warehouse;
            Time = time;
            Value = value;
        }
        public DateTime Time { get; set; }
        public decimal Value { get; set; }
        public int? Warehouse { get; set; }
    }
    public class EmployeeSale
    {
        public int ID { get; set; }
        public string Name { get; set; }
        public int WarehouseID { get; set; }
        public string WarehouseName { get; set; }
        public int OrderCount { get; set; }
        public string OrderCountString { get { return OrderCount.GetCurrencyString(); } }
        public int ReturnedCount { get; set; }
        public string ReturnedCountString { get { return ReturnedCount.GetCurrencyString(); } }
        public decimal Total { get; set; }
        public string TotalString { get { return Total.GetCurrencyString(); } }
        public decimal Returned { get; set; }
        public string ReturnedString { get { return Returned.GetCurrencyString(); } }
    }
    public class ProductSaleReport
    {
        public int ID { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        public int WarehouseID { get; set; }
        public string WarehouseName { get; set; }
        public decimal QuantityPercentage { get; set; }
        public string QuantityPercentageString
        {
            get
            {
                var type = "";
                if (QuantityPercentage >= 80)
                    type = "text-info";
                else if (QuantityPercentage >= 50)
                    type = "";
                else if (QuantityPercentage >= 20)
                    type = "text-warning";
                else
                    type = "text-danger";
                return String.Format("<strong class=\\\"{1}\\\">{0}</strong>", Math.Round(QuantityPercentage, 2), type);
            }
        }
        public decimal RevenuePercentage { get; set; }
        public string RevenuePercentageString
        {
            get
            {
                var type = "";
                if (RevenuePercentage >= 80)
                    type = "text-info";
                else if (RevenuePercentage >= 50)
                    type = "";
                else if (RevenuePercentage >= 20)
                    type = "text-warning";
                else
                    type = "text-danger";
                return String.Format("<strong class=\\\"{1}\\\">{0}</strong>", Math.Round(RevenuePercentage, 2), type);
            }
        }
        public int OrderQuantity { get; set; }
        public string OrderQuantityString { get { return OrderQuantity.GetCurrencyString(); } }
        public decimal OrderTotal { get; set; }
        public string OrderTotalString { get { return OrderTotal.GetCurrencyString(); } }
        public int ImportQuantity { get; set; }
        public string ImportQuantityString { get { return ImportQuantity.GetCurrencyString(); } }
        public decimal ImportTotal { get; set; }
        public string ImportTotalString { get { return ImportTotal.GetCurrencyString(); } }
    }
    public class ProductReport
    {
        public ProductReport()
        {
            Filter = Report.RenderFilter();
            Warehouses = new List<WarehouseInfo>();
            Products = new List<ProductSaleReport>();
        }
        public ReportFilter Filter { get; set; }
        public List<WarehouseInfo> Warehouses { get; set; }
        public List<ProductSaleReport> Products { get; set; }
        public static ProductReport Get(int bussinessID, ReportFilter filter = null)
        {
            var result = new ProductReport();
            result.Filter = Report.RenderFilter(filter);
            try
            {
                using (var con = Repo.DB.SKtimeManagement)
                {
                    result.Products = con.Query<ProductSaleReport>(Report.ProductSaleQuery(bussinessID, result.Filter)).ToList();
                    result.Warehouses.AddRange(result.Products.GroupBy(i => i.WarehouseID).Select(i => new WarehouseInfo() {
                        ID = i.Key,
                        Name = i.FirstOrDefault().WarehouseName
                    }));
                }
            }
            catch (Exception e) { }
            return result;
        }
    }
    public class Report
    {
        public Report()
        {
            Filter = RenderFilter();
            PaidOrders = new List<OrderRecord>();
            Incomes = new List<IncomeInfo>();
            Outcomes = new List<OutcomeInfo>();
            Warehouses = new List<WarehouseInfo>();
            Employees = new List<EmployeeSale>();
            Salaries = new List<SalaryInfo>();
            ProductsByQuantity = new List<ProductSaleReport>();
        }
        public ReportFilter Filter { get; set; }
        public List<OrderRecord> PaidOrders { get; set; }
        public List<Transaction> Transactions { get; set; }
        public List<IncomeInfo> Incomes { get; set; }
        public List<OutcomeInfo> Outcomes { get; set; }
        public List<WarehouseInfo> Warehouses { get; set; }
        public List<StoreInfo> Stores { get; set; }
        public List<EmployeeSale> Employees { get; set; }
        public List<SalaryInfo> Salaries { get; set; }
        public ReportGraph Graph { get; set; }
        public ReportGraph RevenueGraph { get; set; }
        public ReportGraph RelativeGraph { get; set; }
        public List<ProductSaleReport> ProductsByQuantity { get; set; }
        public List<ProductSaleReport> ProductsByRevenue { get; set; }
        public int? MainEmployeeID { get; set; }
        public decimal TotalRevenue
        {
            get
            {
                return PaidOrders.Sum(i => i.Paid) + Incomes.Sum(i => i.Amount) - Outcomes.Sum(i => i.Amount);
            }
        }
        public decimal LastYearRevenue { get; set; }
        public DateTime LastYearFrom { get; set; }
        public DateTime LastYearTo { get; set; }
        public decimal LastPeriodRevenue { get; set; }
        public DateTime LastPeriodFrom { get; set; }
        public DateTime LastPeriodTo { get; set; }
        public static Report Get(int userID, int employeeID, int bussinessID, ReportFilter filter = null, bool justOrders = false)
        {
            var result = new Report();
            result.Filter = RenderFilter(filter);
            result.Filter.Max = 10;
            try
            {
                result.MainEmployeeID = null;
                using (var con = Repo.DB.SKtimeManagement)
                {
                    var login = Login.Get(userID);
                    if (login.Username != "admin" && login.Type == LoginType.Sale)
                        result.MainEmployeeID = employeeID;
                    result.PaidOrders = con.Query<OrderRecord>(PaidOrdersQuery(userID, employeeID, bussinessID, result.Filter)).ToList();
                    if (!justOrders)
                    {
                        result.Transactions = con.Query<Transaction>(OrderTransactions(userID, employeeID, bussinessID, result.Filter)).ToList();
                        result.Warehouses.AddRange(result.PaidOrders.GroupBy(i => i.WarehouseID).Select(i => new WarehouseInfo() { ID = i.Key, Name = i.FirstOrDefault().WarehouseName }));
                        result.Incomes = con.Query<IncomeInfo>(IncomesQuery(userID, employeeID, bussinessID, result.Filter)).ToList();
                        result.Warehouses.AddRange(result.Incomes.Where(i => result.Warehouses.FirstOrDefault(w => w.ID == i.WarehouseID) == null).GroupBy(i => i.WarehouseID).Select(i => new WarehouseInfo() { ID = i.Key, Name = i.FirstOrDefault().WarehouseName }));
                        result.Outcomes = con.Query<OutcomeInfo>(OutcomesQuery(userID, employeeID, bussinessID, result.Filter)).ToList();
                        result.Warehouses.AddRange(result.Outcomes.Where(i => result.Warehouses.FirstOrDefault(w => w.ID == i.WarehouseID) == null).GroupBy(i => i.WarehouseID).Select(i => new WarehouseInfo() { ID = i.Key, Name = i.FirstOrDefault().WarehouseName }));
                        if (result.Warehouses.Count > 0)
                            result.Stores = con.Query<StoreInfo>(StoreQuery(bussinessID, result.Warehouses)).ToList();
                        result.ProductsByQuantity = con.Query<ProductSaleReport>(PopularProductQuery(bussinessID, result.Filter, ProductSaleOrder.ByQuantityTotal)).ToList();
                        result.ProductsByRevenue = con.Query<ProductSaleReport>(PopularProductQuery(bussinessID, result.Filter, ProductSaleOrder.ByRevenueTotal)).ToList();
                        result.Salaries = con.Query<SalaryInfo>(SalariesQuery(bussinessID, result.Filter)).ToList();
                        if (result.Filter.To.Value.Year == result.Filter.From.Value.Year)
                        {
                            result.LastYearFrom = filter.From.Value.AddYears(-1);
                            result.LastYearTo = filter.To.Value.AddYears(-1);
                            result.LastYearRevenue = con.Query<decimal>(RevenueQuery(userID, employeeID, bussinessID, result.Filter, result.LastYearFrom, result.LastYearTo)).FirstOrDefault();
                        }
                        if ((result.Filter.To.Value - result.Filter.From.Value).TotalHours <= 24)
                        {
                            result.LastPeriodFrom = filter.From.Value.AddDays(-1);
                            result.LastPeriodTo = filter.To.Value.AddDays(-1);
                        }
                        else if ((result.Filter.To.Value - result.Filter.From.Value).TotalHours <= 24 * 31)
                        {
                            result.LastPeriodFrom = filter.From.Value.AddMonths(-1);
                            result.LastPeriodTo = filter.To.Value.AddMonths(-1);
                        }
                        else
                        {
                            result.LastPeriodFrom = filter.From.Value.AddHours(-(filter.To.Value - filter.From.Value).TotalHours);
                            result.LastPeriodTo = filter.From.Value;
                        }
                        result.LastPeriodRevenue = con.Query<decimal>(RevenueQuery(userID, employeeID, bussinessID, result.Filter, result.LastPeriodFrom, result.LastPeriodTo)).FirstOrDefault();
                    }
                }
                result.Employees.AddRange(result.PaidOrders.Where(i => !result.MainEmployeeID.HasValue || (result.MainEmployeeID.HasValue && i.EmployeeID == result.MainEmployeeID)).GroupBy(i => i.EmployeeID).Select(i => new EmployeeSale() {
                    ID = i.FirstOrDefault().EmployeeID,
                    Name = i.FirstOrDefault().EmployeeName,
                    WarehouseID = i.FirstOrDefault().WarehouseID,
                    WarehouseName = i.FirstOrDefault().WarehouseName,
                    OrderCount = i.Count(),
                    ReturnedCount = i.Where(o => o.Status == OrderStatus.Refunded).Count(),
                    Total = i.Sum(o => o.Paid),
                    Returned = i.Where(o => o.Status == OrderStatus.Refunded).Sum(o => o.Total)
                }));
                result.Graph = ReportGraph.Create(result);
                result.RevenueGraph = ReportGraph.Create(result, ReportGraphType.Revenue);
                result.RelativeGraph = ReportGraph.Create(result, ReportGraphType.SaleRelative);
            }
            catch (Exception e) { }
            return result;
        }
        public static ReportFilter RenderFilter(ReportFilter filter = null)
        {
            filter = filter == null ? new ReportFilter() : filter;
            var now = DateTime.Now;
            if (!filter.From.HasValue && !filter.To.HasValue)
            {
                filter.To = now.AddDays(1);
                filter.From = new DateTime(now.Year, now.Month, 1, 0, 0, 0);
            }
            else if (!filter.From.HasValue)
            {
                filter.From = new DateTime(filter.To.Value.Year, filter.To.Value.Month, 1, 0, 0, 0);
            }
            else if (!filter.To.HasValue)
            {
                var to = filter.From.Value.AddMonths(1);
                filter.To = new DateTime(to.Year, to.Month, 1, 0, 0, 0);
            }
            filter.From = new DateTime(filter.From.Value.Year, filter.From.Value.Month, filter.From.Value.Day);
            filter.To = new DateTime(filter.To.Value.Year, filter.To.Value.Month, filter.To.Value.Day, 23, 59, 59);
            if (filter.From == filter.To)
                filter.To = filter.To.Value.AddDays(1);
            return filter;
        }
        public static string StoreQuery(int bussinessID, List<WarehouseInfo> warehouses)
        {
            var query = String.Format(
                @"select s.*, w.Name as [WarehouseName] 
                from Store s join Warehouse w on s.WarehouseID = w.ID and w.ID in ({1})
                where s.Status = 'active' and s.BussinessID = {0} order by s.Name",
                bussinessID, String.Join(",", warehouses.Select(i => i.ID)));
            return query;
        }
        public static string OutcomesQuery(int userID, int employeeID, int bussinessID, ReportFilter filter)
        {
            var conditions = new List<string>();
            if (filter != null)
            {
                if (filter.WarehouseID.HasValue)
                    conditions.Add(String.Format("and o.WarehouseID = {0}", filter.WarehouseID.Value));
                if (filter.From.HasValue)
                    conditions.Add(String.Format("and o.SubmitDate >= '{0}'", filter.From.Value.ToString(Constants.DatabaseDatetimeString)));
                if (filter.To.HasValue)
                    conditions.Add(String.Format("and o.SubmitDate <= '{0}'", filter.To.Value.ToString(Constants.DatabaseDatetimeString)));
            }
            var query = String.Format(
                @"select o.*, e.Name as [EmployeeName], w.Name as [WarehouseName]
                    from Outcome o join Employee e on o.EmployeeID = e.ID join Warehouse w on o.WarehouseID = w.ID and ((select Username from Login where ID = {2}) = 'admin' or w.ID in (select WarehouseID from LoginWarehouse where LoginID = {2}))
                    where o.Status = 'active' and o.BussinessID = {0} {1} order by o.ID desc",
                //((select case when Username = 'admin' or Type = N'{4}' then 1 else 0 end from Login where ID = {2}) = 1 or o.EmployeeID = {3}) and 
                bussinessID, String.Join(" ", conditions), userID, employeeID, LoginType.Office);
            return query;
        }
        public static string IncomesQuery(int userID, int employeeID, int bussinessID, ReportFilter filter)
        {
            var conditions = new List<string>();
            if (filter != null)
            {
                if (filter.WarehouseID.HasValue)
                    conditions.Add(String.Format("and i.WarehouseID = {0}", filter.WarehouseID.Value));
                if (filter.From.HasValue)
                    conditions.Add(String.Format("and i.SubmitDate >= '{0}'", filter.From.Value.ToString(Constants.DatabaseDatetimeString)));
                if (filter.To.HasValue)
                    conditions.Add(String.Format("and i.SubmitDate <= '{0}'", filter.To.Value.ToString(Constants.DatabaseDatetimeString)));
            }
            var query = String.Format(
                @"select i.*, e.Name as [EmployeeName], w.Name as [WarehouseName]
                    from Income i join Employee e on i.EmployeeID = e.ID  join Warehouse w on i.WarehouseID = w.ID and ((select Username from Login where ID = {2}) = 'admin' or w.ID in (select WarehouseID from LoginWarehouse where LoginID = {2}))
                    where i.Status = 'active' and i.BussinessID = {0} {1} order by i.ID desc", 
                bussinessID, String.Join(" ", conditions), userID, employeeID, LoginType.Office);
            return query;
        }
        public static string PaidOrdersQuery(int userID, int employeeID, int bussinessID, ReportFilter filter)
        {
            var conditions = new List<string>();
            if (filter != null)
            {
                if (filter.WarehouseID.HasValue)
                    conditions.Add(String.Format("and o.WarehouseID = {0}", filter.WarehouseID.Value));
                if (filter.EmployeeID.HasValue)
                    conditions.Add(String.Format("and o.EmployeeID = {0}", filter.EmployeeID.Value));
                if (filter.From.HasValue)
                    conditions.Add(String.Format("and o.SubmitDate >= '{0}'", filter.From.Value.ToString(Constants.DatabaseDatetimeString)));
                if (filter.To.HasValue)
                    conditions.Add(String.Format("and o.SubmitDate <= '{0}'", filter.To.Value.ToString(Constants.DatabaseDatetimeString)));
            }
            var query = String.Format(
                @"select o.ID, o.EmployeeID, o.DeliveryID, o.WarehouseID, o.Code, o.SubmitDate, o.PayMethod, o.Receipt, o.Total, o.Discount, o.Paid, o.Note, o.Status, 
                            w.Name as [WarehouseName], c.ID as [ClientID], c.Code as [ClientCode], c.Name as [ClientName], c.Address as [ClientAddress], c.Phone as [ClientPhone], 
                            em.Name as [EmployeeName], del.Name as [DeliveryName], ex.ID as [ExportID], ex.Code as [ExportCode], t.Name as [ClientType], t.DiscountType as [DiscountType], t.DiscountValue as [DiscountValue]
                        from SKtimeManagement..[Order] o
                            join Warehouse w on o.WarehouseID = w.ID and ((select Username from Login where ID = {2}) = 'admin' or w.ID in (select WarehouseID from LoginWarehouse where LoginID = {2}))
                            join ExportProduct p on o.ID = p.OrderID
                            left join Export ex on ex.ID = p.ExportID
                            join Employee em on o.EmployeeID = em.ID
                            left join Employee del on o.DeliveryID = del.ID
                            left join Client c on o.ClientID = c.ID
                            left join ClientType ct on c.ID = ct.ClientID
                            left join Type t on ct.TypeID = t.ID
                        where o.Removed = 0 and o.BussinessID = {0} {1}
                        group by o.ID, o.EmployeeID, o.DeliveryID, o.WarehouseID, o.Code, o.SubmitDate, o.PayMethod, o.Receipt, o.Total, o.Discount, o.Paid, o.Note, o.Status, 
                            w.Name, c.ID, c.Code, c.Name, c.Address, c.Phone, em.Name, del.Name, ex.ID, ex.Code, t.Name, t.DiscountType, t.DiscountValue
                        order by o.ID desc", bussinessID, String.Join(" ", conditions), userID, employeeID, LoginType.Office);
            return query;
        }
        public static string OrderTransactions(int userID, int employeeID, int bussinessID, ReportFilter filter)
        {
            var conditions = new List<string>();
            if (filter != null)
            {
                if (filter.WarehouseID.HasValue)
                    conditions.Add(String.Format("and o.WarehouseID = {0}", filter.WarehouseID.Value));
                if (filter.EmployeeID.HasValue)
                    conditions.Add(String.Format("and o.EmployeeID = {0}", filter.EmployeeID.Value));
                if (filter.From.HasValue)
                    conditions.Add(String.Format("and o.SubmitDate >= '{0}'", filter.From.Value.ToString(Constants.DatabaseDatetimeString)));
                if (filter.To.HasValue)
                    conditions.Add(String.Format("and o.SubmitDate <= '{0}'", filter.To.Value.ToString(Constants.DatabaseDatetimeString)));
            }
            var query = String.Format(
                @"select t.*, e.Name as [EmployeeName] 
                from Transactions t join Employee e on t.EmployeeID = e.ID 
                    join [Order] o on t.OrderID = o.ID
                    join Warehouse w on o.WarehouseID = w.ID and ((select Username from Login where ID = {2}) = 'admin' or w.ID in (select WarehouseID from LoginWarehouse where LoginID = {2}))
                where 
                    o.Removed = 0 and t.OrderID is not null and o.BussinessID = {0} {1} order by t.ID desc", 
                bussinessID, String.Join(" ", conditions), userID, employeeID, LoginType.Office);
            return query;
        }
        public static string RevenueQuery(int userID, int employeeID, int bussinessID, ReportFilter filter, DateTime from, DateTime to)
        {
            var conditions = new List<string>();
            if (filter != null)
            {
                if (filter.WarehouseID.HasValue)
                    conditions.Add(String.Format("and o.WarehouseID = {0}", filter.WarehouseID.Value));
                if (filter.EmployeeID.HasValue)
                    conditions.Add(String.Format("and o.EmployeeID = {0}", filter.EmployeeID.Value));
                conditions.Add(String.Format("and o.SubmitDate >= '{0}'", from.ToString(Constants.DatabaseDatetimeString)));
                conditions.Add(String.Format("and o.SubmitDate <= '{0}'", to.ToString(Constants.DatabaseDatetimeString)));
            }
            var query = String.Format(
                @"select isnull(sum(o.Total - o.Discount), 0)
                    + (select isnull(sum(Amount), 0) from Income where ((select Username from Login where ID = {2}) = 'admin' or WarehouseID in (select WarehouseID from LoginWarehouse where LoginID = {2})) and Status = 'active' and SubmitDate between '{5}' and '{6}') 
                    - (select isnull(sum(Amount), 0) from Outcome where ((select Username from Login where ID = {2}) = 'admin' or WarehouseID in (select WarehouseID from LoginWarehouse where LoginID = {2})) and Status = 'active' and SubmitDate between '{5}' and '{6}')
                from SKtimeManagement..[Order] o
                    join Warehouse w on o.WarehouseID = w.ID and ((select Username from Login where ID = {2}) = 'admin' or w.ID in (select WarehouseID from LoginWarehouse where LoginID = {2}))
                where ((select case when Username = 'admin' or Type = N'{4}' then 1 else 0 end from Login where ID = {2}) = 1 or o.EmployeeID = {3}) and o.Removed = 0 and o.BussinessID = {0} {1}", 
                bussinessID, String.Join(" ", conditions), userID, employeeID, LoginType.Office,
                from.ToString(Constants.DatabaseDatetimeString), to.ToString(Constants.DatabaseDatetimeString));
            return query;
        }
        public static string ProductSaleQuery(int bussinessID, ReportFilter filter, string orderby = ProductSaleOrder.ByQuantityRate)
        {
            var conditions = new List<string>();
            if (filter != null)
            {
                if (filter.WarehouseID.HasValue)
                    conditions.Add(String.Format("and i.WarehouseID = {0}", filter.WarehouseID.DbValue()));
                if (filter.From.HasValue)
                    conditions.Add(String.Format("and i.SubmitDate >= '{0}'", filter.From.Value.ToString(Constants.DatabaseDatetimeString)));
                if (filter.To.HasValue)
                    conditions.Add(String.Format("and i.SubmitDate <= '{0}'", filter.To.Value.ToString(Constants.DatabaseDatetimeString)));
            }
            var query = String.Format(
                @"select {4} p.ID, p.Code, p.Name, p.Unit,w.ID as [WarehouseID], w.Name as [WarehouseName],
	                sum(ip.Quantity) as [ImportQuantity], sum(ip.Quantity * ip.Price) as [ImportTotal],
	                isnull(sum(ep.Quantity), 0) as [OrderQuantity], isnull(sum(ep.Quantity * ep.Price), 0) as [OrderTotal],
	                case when sum(ip.Quantity) = 0 then 0 else isnull(sum(ep.Quantity), 0) / convert(decimal, sum(ip.Quantity)) * 100 end as [QuantityPercentage],
	                case when sum(ip.Quantity * ip.Price) = 0 then 0 else isnull(sum(ep.Quantity * ep.Price), 0) / sum(ip.Quantity * ip.Price) * 100 end as [RevenuePercentage]
                from ImportProduct ip
	                join Product p on ip.ProductID = p.ID
	                join Import i on ip.ImportID = i.ID
	                join Warehouse w on i.WarehouseID = w.ID
	                left join ExportProduct ep on ep.ProductID = ip.ProductID and ep.Returned = 0
	                    and ep.OrderID in (select ID from [Order] where Removed = 0 and WarehouseID = w.ID and SubmitDate >= '{2}' and SubmitDate <= '{3}')
                where i.Status = 'active' and i.BussinessID = {0} {1}
                group by p.Name, p.ID, p.Code, p.Unit, w.ID, w.Name
                order by {5} desc", 
                bussinessID, String.Join(" ", conditions), filter.From.Value.ToString(Constants.DatabaseDatetimeString), filter.To.Value.ToString(Constants.DatabaseDatetimeString),
                filter.Max.HasValue ? String.Format("top {0}", filter.Max.Value) : "",
                orderby);
            return query;
        }
        public static string PopularProductQuery(int bussinessID, ReportFilter filter, string orderby = ProductSaleOrder.ByQuantityRate)
        {
            var conditions = new List<string>();
            //if (filter != null)
            //{
            //    if (filter.WarehouseID.HasValue)
            //        conditions.Add(String.Format("and i.WarehouseID = {0}", filter.WarehouseID.DbValue()));
            //    if (filter.From.HasValue)
            //        conditions.Add(String.Format("and i.SubmitDate >= '{0}'", filter.From.Value.ToString(Constants.DatabaseDatetimeString)));
            //    if (filter.To.HasValue)
            //        conditions.Add(String.Format("and i.SubmitDate <= '{0}'", filter.To.Value.ToString(Constants.DatabaseDatetimeString)));
            //}
            var query = String.Format(
                @"select {4} p.ID, p.Code, p.Name, p.Unit,
	                isnull(sum(ep.Quantity), 0) as [OrderQuantity], isnull(sum(ep.Quantity * ep.Price), 0) as [OrderTotal]
                from Product p
	                left join ExportProduct ep on ep.ProductID = p.ID and ep.Returned = 0
	                    and ep.OrderID in (select ID from [Order] where Removed = 0 and SubmitDate >= '{2}' and SubmitDate <= '{3}' {6})
                where p.BussinessID = {0} {1}
                group by p.Name, p.ID, p.Code, p.Unit
                order by {5} desc",
                bussinessID, String.Join(" ", conditions), filter.From.Value.ToString(Constants.DatabaseDatetimeString), filter.To.Value.ToString(Constants.DatabaseDatetimeString),
                filter.Max.HasValue ? String.Format("top {0}", filter.Max.Value) : "",
                orderby, filter.WarehouseID.HasValue ? String.Format("and WarehouseID = {0}", filter.WarehouseID.Value) : "");
            return query;
        }
        public static string SalariesQuery(int bussinessID, ReportFilter filter)
        {
            var query = String.Format(
                    @"select s.* from Salary s join Employee e on s.EmployeeID = e.ID where e.BussinessID = {0} {1} {2} order by s.Month desc",
                    bussinessID, filter.From.HasValue ? String.Format("and s.Month >= '{0}-{1}-1'", filter.From.Value.Year, filter.From.Value.Month) : "",
                    filter.To.HasValue ? String.Format(" and s.Month <= '{0}-{1}-1'", filter.To.Value.Year, filter.To.Value.Month) : "");
            return query;
        }
    }
    public class ProductSaleOrder
    {
        public const string ByQuantityRate = "case when sum(ip.Quantity) = 0 then 0 else isnull(sum(ep.Quantity), 0) / convert(decimal, sum(ip.Quantity)) * 100 end";
        public const string ByRevenueRate = "case when sum(ip.Quantity * ip.Price) = 0 then 0 else isnull(sum(ep.Quantity * ep.Price), 0) / sum(ip.Quantity * ip.Price) * 100 end";
        public const string ByQuantityTotal = "isnull(sum(ep.Quantity), 0)";
        public const string ByRevenueTotal = "isnull(sum(ep.Quantity * ep.Price), 0)";
    }
    public class SalaryReport
    {
        public SalaryReport(EmployeeFilter filter = null)
        {
            Filter = filter;
            if (filter == null)
                Filter = new EmployeeFilter();
            Employees = new List<SalaryInfo>();
            Months = new List<DateTime>();
            Records = new List<dynamic>();
        }
        public EmployeeFilter Filter { get; set; }
        public List<SalaryInfo> Employees { get; set; }
        public List<DateTime> Months { get; set; }
        public List<dynamic> Records { get; set; }
        public static List<EmployeeInfo> FindEmployees(int bussinessID, EmployeeFilter filter = null)
        {
            var conditions = new List<string>();
            if (filter != null)
            {
                if (filter.ID.HasValue)
                    conditions.Add(String.Format("and e.ID = {0}", filter.ID.DbValue()));
                if (!String.IsNullOrEmpty(filter.Address))
                    conditions.Add(String.Format("and e.Address like N'%{0}%'", filter.Address));
                if (filter.LoginID.HasValue)
                    conditions.Add(String.Format("and l.ID = {0}", filter.LoginID.DbValue()));
                if (!String.IsNullOrEmpty(filter.Name))
                    conditions.Add(String.Format("and e.Name like N'%{0}%'", filter.Name));
                if (!String.IsNullOrEmpty(filter.Phone))
                    conditions.Add(String.Format("and e.Phone like N'%{0}%'", filter.Phone));
                if (filter.StoreID.HasValue)
                    conditions.Add(String.Format("and e.StoreID = {0}", filter.StoreID.DbValue()));
            }
            using (var con = Repo.DB.SKtimeManagement)
            {
                return con.Query<EmployeeInfo>(String.Format(@"select e.*, l.ID as [LoginID], l.Username, isnull(sum(o.Total - o.Discount), 0) as [CurrentSale]
                    from Employee e left join Login l on l.EmployeeID = e.ID 
	                    left join [Order] o on o.EmployeeID  = e.ID and o.Removed = 0 and o.Status <> N'{2}'
                    where e.Status = 'active' and e.BussinessID = {0} {1}
                    group by e.AdditionalSalary, e.Address,  e.BaseSalary, e.BussinessID, e.ID, e.Image, e.MonthlySale, e.Name,  e.OffDays, e.Phone, e.Position, e.DOB,
	                    e.StartDate, e.Status, e.StoreID, e.Summary,  e.WorkDays, e.WorkTime, e.EndDate, e.BankNumber, e.BankName, e.BankBranch, e.WorkStatus, l.ID, l.Username
                    order by e.Name",
                    bussinessID, String.Join(" ", conditions), OrderStatus.Refunded)).ToList();
            }
        }
        public static SalaryReport Get(int userID, int employeeID, int bussinessID, EmployeeFilter filter)
        {
            var result = new SalaryReport(filter);
            var employees = SalaryReport.FindEmployees(bussinessID, filter);
            foreach (var employee in employees)
            {
                var infos = SalaryCalculator.Find(employee.ID, filter.From, filter.To);
                dynamic record = new ExpandoObject();
                var dictionary = (IDictionary<string, object>)record;
                dictionary.Add("Name", employee.Name);
                foreach (var info in infos)
                {
                    result.Employees.Add(info);
                    var month = info.Month.Value;
                    if (!result.Months.Contains(month))
                        result.Months.Add(month);
                    dictionary.Add(info.Month.Value.ToString("_MMyyyy"), filter.ViewBaseSalary ? info.BaseSalary > 0 ? info.BaseSalary : employee.BaseSalary : info.CalculatedTotal.Round());
                }
                result.Records.Add(record);
            }
            result.Months = result.Months.OrderByDescending(i => i).ToList();
            return result;
        }
    }
    public class ServiceReport
    {
        public ServiceReport()
        {
            Warranties = new List<Warranty>();
            Repaires = new List<Repair>();
        }
        public List<Warranty> Warranties { get; set; }
        public List<Repair> Repaires { get; set; }
        public static ServiceReport ProcessingServices(int userID, int bussinessID, bool includeFinished = false)
        {
            var result = new ServiceReport();
            try
            {
                using (var con = Repo.DB.SKtimeManagement)
                {
                    result.Warranties = con.Query<Warranty>(String.Format(
                        @"select w.ID, w.BussinessID, w.EmployeeID, w.WarehouseID, w.ProductID, w.ClientID, w.OrderID, w.Code, 
	                        w.SubmitDate, w.TransferDate, w.ReceivedDate, w.ProcessedDate, w.ReturnedDate, w.FinishDate, w.ProductState, 
	                        w.Fee, w.Discount, w.Other, w.Note, wh.Name as [WarehouseName], e.Name as [EmployeeName], 
                            c.Name as [ClientName], c.Code as [ClientCode], c.Address as [ClientAddress], c.Phone as [ClientPhone], 
	                        case when w.OrderID is not null then o.Code else w.OrderCode end as [OrderCode], isnull(sum(t.Amount), 0) as [Paid]
                        from Warranty w 
                            join Warehouse wh on w.WarehouseID = wh.ID and w.Status = 'active' and ((select Username from Login where ID = {1}) = 'admin' or wh.ID in (select WarehouseID from LoginWarehouse where LoginID = {1}))
                            join Employee e on w.EmployeeID = e.ID
                            join Product p on p.ID = w.ProductID
                            left join Client c on w.ClientID = c.ID
                            left join [Order] o on w.OrderID = o.ID
                            left join Transactions t on w.ID = t.WarrantyID
                        where w.BussinessID = {0} and w.Status = 'active' {2}
                        group by w.ID, w.BussinessID, w.EmployeeID, w.WarehouseID, w.ProductID, w.ClientID, w.OrderID, w.Code, 
                            w.SubmitDate, w.TransferDate, w.ReceivedDate, w.ProcessedDate, w.ReturnedDate, w.FinishDate, w.ProductState, 
	                        w.Fee, w.Discount, w.Other, w.Note, wh.Name, e.Name, c.Name, c.Code, c.Address, c.Phone, o.Code, w.OrderCode
                        order by w.FinishDate",
                        bussinessID, userID, includeFinished ? "" : String.Format("and (w.ReturnedDate is null or w.FinishDate > {0})", DateTime.Now.DbValue()))).ToList();
                    result.Repaires = con.Query<Repair>(String.Format(
                        @"select w.ID, w.BussinessID, w.EmployeeID, w.WarehouseID, w.ProductID, w.ClientID, w.Code, 
	                        w.SubmitDate, w.TransferDate, w.ReceivedDate, w.ProcessedDate, w.ReturnedDate, w.FinishDate, w.ProductState, 
	                        w.Fee, w.Discount, w.Other, w.Note, wh.Name as [WarehouseName], e.Name as [EmployeeName],
                            c.Name as [ClientName], c.Code as [ClientCode], c.Address as [ClientAddress], c.Phone as [ClientPhone], isnull(sum(t.Amount), 0) as [Paid]
                        from Repair w 
                            join Warehouse wh on w.WarehouseID = wh.ID and w.Status = 'active' and ((select Username from Login where ID = {1}) = 'admin' or wh.ID in (select WarehouseID from LoginWarehouse where LoginID = {1}))
                            join Employee e on w.EmployeeID = e.ID
                            join Product p on p.ID = w.ProductID
                            left join Client c on w.ClientID = c.ID
                            left join Transactions t on w.ID = t.RepairID
                        where w.BussinessID = {0} and w.Status = 'active' {2}
                        group by w.ID, w.BussinessID, w.EmployeeID, w.WarehouseID, w.ProductID, w.ClientID, w.Code, 
                            w.SubmitDate, w.TransferDate, w.ReceivedDate, w.ProcessedDate, w.ReturnedDate, w.FinishDate, w.ProductState, 
	                        w.Fee, w.Discount, w.Other, w.Note, wh.Name, e.Name, c.Name, c.Code, c.Address, c.Phone
                        order by w.FinishDate",
                        bussinessID, userID, includeFinished ? "" : String.Format("and (w.ReturnedDate is null or w.FinishDate > {0})", DateTime.Now.DbValue()))).ToList();
                }
            }
            catch { }
            return result;
        }
    }
    public class ClientReport
    {
        public ClientReport(ClientFilter filter = null)
        {
            Filter = filter != null ? filter : new ClientFilter();
        }
        public ClientFilter Filter { get; set; }
        public List<ClientInfo> Data { get; set; }
        public static ClientReport Get(int bussinessID, ClientFilter filter = null)
        {
            var result = new ClientReport(filter);
            try
            {
                var conditions = new List<string>();
                if (filter != null)
                {
                    if (!String.IsNullOrEmpty(filter.Code))
                        conditions.Add(String.Format("and c.Code like N'%{0}%'", filter.Code));
                    if (!String.IsNullOrEmpty(filter.Name))
                        conditions.Add(String.Format("and c.Name like N'%{0}%'", filter.Name));
                }
                var query = String.Format(
                    @"select c.ID, c.BussinessID, c.Code, c.Name, c.Phone, c.Address, c.Email, c.City, c.District, MAX(t.ID) as [TypeID], t.Name as [TypeName], c.Point,
                        isnull(sum(ts.Amount), 0) as [SaleTotal]
                    from Client c
                        left join ClientType ct on c.ID = ct.ClientID and ct.Status = 'active'
                        left join Type t on ct.TypeID = t.ID and t.Status = 'active'
                        left join [Order] o on o.ClientID = c.ID
                        left join Transactions ts on o.ID = ts.OrderID {2} {3}
                    where o.Removed = 0 and o.BussinessID = {0} {1} 
                    group by c.ID, c.BussinessID, c.Code, c.Name, c.Phone, c.Address, c.Email, c.City, c.District, t.Name, c.Point
                    order by isnull(sum(ts.Amount), 0) desc",
                    bussinessID, String.Join(" ", conditions),
                    filter != null && filter.From.HasValue ? String.Format("and o.SubmitDate >= {0}", filter.From.DbValue()) : "",
                    filter != null && filter.To.HasValue ? String.Format("and o.SubmitDate <= {0}", filter.To.DbValue()) : "");
                using (var con = Repo.DB.SKtimeManagement)
                {
                    result.Data = con.Query<ClientInfo>(query).ToList();
                }
            }
            catch { }
            return result;
        }
    }
    public class ProductPartReport
    {
        public ProductPartReport(ProductFilter filter = null)
        {
            Filter = filter != null ? filter : new ProductFilter();
        }
        public ProductFilter Filter { get; set; }
        public List<ProductInfo> Data { get; set; }
        public static ProductPartReport Get(int userID, int bussinessID, ProductFilter filter = null)
        {
            var result = new ProductPartReport(filter);
            try
            {
                var conditions = new List<string>();
                if (filter != null)
                {
                    if (!String.IsNullOrEmpty(filter.Code))
                        conditions.Add(String.Format("and p.Code like N'%{0}%'", filter.Code));
                    if (!String.IsNullOrEmpty(filter.Name))
                        conditions.Add(String.Format("and p.Name like N'%{0}%'", filter.Name));
                }
                var query = String.Format(
                    @"select p.*, t.ID as [TagID], t.Name as [TagName], s.Name as [SupplierName],
                        (select isnull(sum(ep.Quantity), 0) from ExportProduct ep join Export e on ep.ExportID = e.ID 
                        join Warranty r on ep.WarrantyID = r.ID and r.Status = 'active' {3} {4}
                        where ep.ProductID = p.ID and ep.WarrantyID is not null and 
                        ((select Username from Login where ID = {2}) = 'admin' or e.WarehouseID in (select WarehouseID from LoginWarehouse where LoginID = {2}))) +
                        (select isnull(sum(ep.Quantity), 0) from ExportProduct ep join Export e on ep.ExportID = e.ID 
                        join Repair r on ep.RepairID = r.ID and r.Status = 'active' {3} {4}
                        where ep.ProductID = p.ID and ep.RepairID is not null and 
                        ((select Username from Login where ID = {2}) = 'admin' or e.WarehouseID in (select WarehouseID from LoginWarehouse where LoginID = {2})))
                        as [Quantity]
                    from Product p
                        left join Supplier s on p.SupplierID = s.ID
                        join ProductTag pt on p.ID = pt.ProductID and pt.TagID = (select top 1 TagID from ProductTag where ProductID = p.ID order by TagID desc)
                        join Tag t on pt.TagID = t.ID and t.Status = 'active' and t.ForRepair = 1
                    where p.Status = 'active' and p.BussinessID = {0} {1} order by Name",
                    bussinessID, String.Join(" ", conditions), userID,
                    filter != null && filter.From.HasValue ? String.Format(" and r.SubmitDate >= {0}", filter.From.DbValue()) : "",
                    filter != null && filter.To.HasValue ? String.Format(" and r.SubmitDate <= {0}", filter.To.DbValue()) : "");
                using (var con = Repo.DB.SKtimeManagement)
                {
                    result.Data = con.Query<ProductInfo>(query).ToList();
                }
            }
            catch { }
            return result;
        }
    }
}