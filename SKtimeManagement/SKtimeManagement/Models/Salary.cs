using Dapper;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace SKtimeManagement
{
    public class SalaryInfo
    {
        public SalaryInfo() { }
        public SalaryInfo(int id)
        {
            EmployeeID = id;
        }
        public int ID { get; set; }
        public int EmployeeID { get; set; }
        public string EmployeeName { get; set; }
        public decimal BaseSalary { get; set; }
        public decimal OffValue { get; set; }
        public int ValidOffDays { get; set; }
        public int AdditionalOffDays { get; set; }
        public int EmployeeCount { get; set; }
        public decimal SalePoint { get; set; }
        public decimal SaleAdd { get; set; }
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = Constants.DateTimeString)]
        public DateTime? Month { get; set; }
        public string MonthString { get { return Month.HasValue ? Month.Value.ToString(Constants.MonthString) : ""; } }
        public decimal Paid { get; set; }
        public decimal Other { get; set; }
        public string Note { get; set; }
        public decimal CalculatedTotal { get; set; }
    }
    public class SalaryCalculator : BaseModel
    {
        public SalaryCalculator(EmployeeFilter filter, SalaryInfo info, EmployeeInfo employeeInfo = null)
        {
            EmployeeFilter = filter;
            EmployeeInfo = employeeInfo;
            Info = info;
            Paid = info.Paid;
            Other = info.Other;
            var time = DateTime.Now.AddMonths(-1);
            if (info.Month.HasValue)
                time = info.Month.Value;
            From = new DateTime(time.Year, time.Month, 1);
            To = From.AddMonths(1);
            Note = info.Note;
        }
        public SalaryInfo Info { get; set; }
        public EmployeeFilter EmployeeFilter { get; set; }
        public List<SaleOffset> Offsets { get; set; }
        public List<EmployeeOffDay> OffDays { get; set; }
        public List<ExportRecord> Orders { get; set; }
        public List<EmployeeFine> Fines { get; set; }
        public DateTime From { get; set; }
        public DateTime To { get; set; }
        public EmployeeInfo EmployeeInfo { get; set; }
        public decimal BaseSalary { get { return Info != null && Info.BaseSalary > 0 ? Info.BaseSalary : EmployeeInfo != null ? EmployeeInfo.BaseSalary : 0; } }
        public decimal SaleRate { get; set; }
        public decimal StoreSale { get; set; }
        public decimal SaleTotal { get; set; }
        public decimal SaleValue
        {
            get
            {
                if (Info.SaleAdd > 0)
                {
                    var value = Math.Floor((SaleTotal - Info.SalePoint) / Info.SaleAdd);
                    return 0.007m + 0.001m * value;
                }
                return 0;
            }
        }
        public decimal SaleSalary
        {
            get
            {
                if (Info.EmployeeCount > 0 && Info.SalePoint > 0)
                {
                    if (SaleTotal > Info.SalePoint)
                    {
                        return SaleTotal * SaleValue + StoreSale / 2 * 0.003m / Info.EmployeeCount;
                    }
                    return SaleTotal * 0.005m;
                }
                return 0;
            }
        }
        public int OffCount { get; set; }
        public decimal OffValue
        {
            get
            {
                var totalOffs = OffCount;
                if (totalOffs > Info.ValidOffDays)
                    return (Info.ValidOffDays - totalOffs) * Info.OffValue * BaseSalary / 30;
                else
                    return (Info.ValidOffDays - totalOffs) * BaseSalary / 30;
            }
        }
        public decimal FineTotal
        {
            get
            {
                return Fines != null ? Fines.Where(i => i.SubmitDate >= From && i.SubmitDate <= To).Sum(i => i.Amount) : 0;
            }
        }
        public decimal Paid { get; set; }
        public decimal Other { get; set; }
        public decimal Total { get { return BaseSalary + OffValue + SaleSalary + Other - Paid - FineTotal; } }
        public decimal CalculatedTotal { get; set; }
        public string Note { get; set; }
        public static List<SalaryInfo> Find(int bussinessID, EmployeeFilter filter = null)
        {
            var conditions = new List<string>();
            if (filter != null)
            {
                if (!String.IsNullOrEmpty(filter.Name))
                    conditions.Add(String.Format(" and e.Name like N'%{0}%'", filter.Name));
                if (!String.IsNullOrEmpty(filter.Phone))
                    conditions.Add(String.Format(" and e.Phone like N'%{0}%'", filter.Phone));
                if (!String.IsNullOrEmpty(filter.Address))
                    conditions.Add(String.Format(" and e.Address like N'%{0}%'", filter.Address));
                if (filter.StoreID.HasValue)
                    conditions.Add(String.Format(" and e.StoreID = {0}", filter.StoreID.Value));
                if (filter.SalaryMonth.HasValue)
                    conditions.Add(String.Format(" and s.Month = '{0}-{1}-1'", filter.SalaryMonth.Value.Year, filter.SalaryMonth.Value.Month));
            }
            var query = String.Format(
                    @"select s.*, e.Name as [EmployeeName] from Salary s join Employee e on s.EmployeeID = e.ID where e.BussinessID = {0} {1} order by s.Month desc",
                    bussinessID, String.Join("", conditions));
            using (var con = Repo.DB.SKtimeManagement)
            {
                return con.Query<SalaryInfo>(query).ToList();
            }
        }
        public static List<SalaryInfo> Find(int id, DateTime? from, DateTime? to)
        {
            using (var con = Repo.DB.SKtimeManagement)
            {
                return con.Query<SalaryInfo>(String.Format(
                    @"select * from Salary where EmployeeID = {0} {1} {2} order by Month desc", 
                    id, from.HasValue ? String.Format("and Month >= '{0}-{1}-1'", from.Value.Year, from.Value.Month) : "",
                    to.HasValue ? String.Format(" and Month <= '{0}-{1}-1'", to.Value.Year, to.Value.Month) : "")).ToList();
            }
        }
        public static SalaryInfo Get(int id)
        {
            using (var con = Repo.DB.SKtimeManagement)
            {
                return con.Query<SalaryInfo>(String.Format(@"select * from Salary where ID = {0} order by Month desc", id)).FirstOrDefault();
            }
        }
        public static SalaryCalculator Get(int userID, int employeeID, SalaryInfo info, bool? find = null, EmployeeInfo employeeInfo = null)
        {
            var result = new SalaryCalculator(new EmployeeFilter(), info, employeeInfo);
            try
            {
                using (var con = Repo.DB.SKtimeManagement)
                {
                    if (find.HasValue && find.Value)
                    {
                        var dbInfo = con.Query<SalaryInfo>(String.Format("select top 1 * from Salary where EmployeeID = {0} order by Month desc", result.Info.EmployeeID)).FirstOrDefault();
                        if (dbInfo != null)
                            result = new SalaryCalculator(new EmployeeFilter(), dbInfo, employeeInfo);
                    }
                    if (employeeInfo == null)
                        result.EmployeeInfo = con.Query<EmployeeInfo>(
                            String.Format(@"select e.*, l.ID as [LoginID], l.Username, s.Name as [StoreName], isnull(sum(o.Total - o.Discount), 0) as [CurrentSale]
                                from Employee e left join Login l on l.EmployeeID = e.ID left join Store s on e.StoreID = s.ID left join [Order] o on o.EmployeeID  = e.ID and o.Removed = 0 and o.Status <> N'{1}'
                                where e.Status = 'active' and e.ID = {0}
                                group by e.AdditionalSalary, e.Address,  e.BaseSalary, e.BussinessID, e.ID, e.Image, e.MonthlySale, e.Name,  e.OffDays, e.Phone, e.Position, e.DOB,
	                                e.StartDate, e.Status, e.StoreID, e.Summary,  e.WorkDays, e.WorkTime, e.EndDate, e.BankNumber, e.BankName, e.BankBranch, e.WorkStatus, l.ID, l.Username, s.Name", info.EmployeeID, OrderStatus.Refunded)).FirstOrDefault();

                    //result.Offsets = SaleOffset.Get(userID, employeeID, info.ID);
                    result.OffDays = con.Query<EmployeeOffDay>(String.Format("select * from EmployeeOffDay where EmployeeID = {0} order by OffDate desc", info.EmployeeID)).ToList();
                    if (result.EmployeeInfo.StoreID.HasValue)
                        result.Orders = con.Query<ExportRecord>(GetStoreOrders(userID, employeeID, result.EmployeeInfo.StoreID.Value, result.From, result.To)).ToList();
                    else
                        result.Orders = new List<ExportRecord>();
                    result.Fines = con.Query<EmployeeFine>(EmployeeFine.Query(result.EmployeeInfo.BussinessID, new EmployeeFineFilter(result.EmployeeInfo.ID, result.From, result.To))).ToList();
                    result.SaleTotal = result.Orders.Where(i => i.EmployeeID == info.EmployeeID).Sum(i => i.Paid);
                    result.StoreSale = result.Orders.Sum(i => i.Paid);
                    //result.SaleRate = result.Offsets.Count == 0 ? 0 : result.Offsets.FirstOrDefault(i => result.SaleTotal >= i.Offset).Value;
                    result.OffCount = result.OffDays.Where(i => i.OffDate >= result.From && i.OffDate <= result.To).Count() + info.AdditionalOffDays;
                    if (find.HasValue && !find.Value)
                    {
                        con.Execute(String.Format(
                            @"delete Salary where EmployeeID = {0} and Month = '{1}'
                            insert into Salary(EmployeeID, Month, OffValue, ValidOffDays, AdditionalOffDays, EmployeeCount, SalePoint, SaleAdd, Paid, Other, Note, CalculatedTotal, BaseSalary) 
                            values ({0}, '{1}', {2}, {3}, {4}, {5}, {6}, {7}, {8}, {9}, N'{10}', {11}, {12})",
                            result.Info.EmployeeID, result.From.ToString(Constants.DatabaseDateString), result.Info.OffValue, result.Info.ValidOffDays, result.Info.AdditionalOffDays,
                            result.Info.EmployeeCount, result.Info.SalePoint, result.Info.SaleAdd, result.Info.Paid, result.Info.Other, result.Info.Note, result.Total, result.BaseSalary));
                    }
                }
            }
            catch { }
            return result;
        }
        public static string GetStoreOrders(int userID, int employeeID, int storeID, DateTime from, DateTime to)
        {
            var conditions = new List<string>();
            conditions.AddRange(new string[] {
                String.Format(" and w.ID = (select WarehouseID from Store where ID = {0})", storeID),
                String.Format(" and o.SubmitDate >= '{0}'", from.ToString(Constants.DatabaseDatetimeString)),
                String.Format(" and o.SubmitDate <= '{0}'", to.ToString(Constants.DatabaseDatetimeString))
            });
            var query = String.Format(@"select o.ID, o.EmployeeID, o.DeliveryID, o.WarehouseID, o.Code, o.SubmitDate, o.PayMethod, o.Receipt, o.Total, o.Discount, o.Paid, o.Note, o.Status, w.Name as [WarehouseName], c.ID as [ClientID], c.Name as [ClientName]
                                from SKtimeManagement..[Order] o
                                    join Warehouse w on o.WarehouseID = w.ID
                                    join ExportProduct p on o.ID = p.OrderID
                                    left join Export ex on ex.ID = p.ExportID
                                    join Employee em on o.EmployeeID = em.ID
                                    left join Delivery del on o.DeliveryID = del.ID
                                    left join Client c on o.ClientID = c.ID
                                where o.Removed = 0 {0}
                                group by o.ID, o.EmployeeID, o.DeliveryID, o.WarehouseID, o.Code, o.SubmitDate, o.PayMethod, o.Receipt, o.Total, o.Discount, o.Paid, o.Note, o.Status, w.Name, c.ID, c.Name, em.Name, del.Name, ex.ID, ex.Code
                                order by o.ID desc", String.Join(" ", conditions));
            return query;
        }
    }
    public class SaleOffset : BaseModel
    {
        public int ID { get; set; }
        public int StoreID { get; set; }
        public decimal Offset { get; set; }
        public string OffsetString { get { return Offset.GetCurrencyString(); } }
        public decimal Value { get; set; }
        public static List<SaleOffset> Get(int userID, int employeeID, int id)
        {
            QueryOutput result;
            return Query<SaleOffset>(new DbQuery(userID, employeeID, SalaryAction.Calculate, String.Format("select * from StoreSaleOffset where StoreID = (select StoreID from Employee where ID = {0}) order by Offset desc", id), true), out result).ToList();
        }
        public static List<SaleOffset> Add(int userID, int employeeID, int storeID, decimal offset, decimal value)
        {
            QueryOutput result;
            return Query<SaleOffset>(new DbQuery(userID, employeeID, SalaryAction.Calculate, 
                String.Format(
                    @"insert into StoreSaleOffset(StoreID, Offset, Value) values ({0}, {1}, {2})
                    select * from StoreSaleOffset where StoreID = {0} order by Offset desc", storeID, offset, value), true), out result).ToList();
        }
        public static List<SaleOffset> Remove(int userID, int employeeID, int id)
        {
            QueryOutput result;
            return Query<SaleOffset>(new DbQuery(userID, employeeID, SalaryAction.Calculate,
                String.Format(
                    @"declare @storeID int = (select StoreID from StoreSaleOffset where ID = {0})
                    delete StoreSaleOffset where ID = {0}
                    select * from StoreSaleOffset where StoreID = @storeID order by Offset desc", id), true), out result).ToList();
        }
    }
    public class EmployeeOffDay : BaseModel
    {
        public int ID { get; set; }
        public int EmployeeID { get; set; }
        public DateTime OffDate { get; set; }
        public string Note { get; set; }
        public static List<EmployeeOffDay> Get(int userID, int employeeID, int id)
        {
            using (var con = Repo.DB.SKtimeManagement)
            {
                return con.Query<EmployeeOffDay>(String.Format("select * from EmployeeOffDay where EmployeeID = {0} order by OffDate desc", id)).ToList();
            }
        }
        public static List<EmployeeOffDay> Add(int userID, int employeeID, int id, DateTime date, string note)
        {
            QueryOutput result;
            return Query<EmployeeOffDay>(new DbQuery(userID, employeeID, SalaryAction.Calculate, 
                String.Format(
                    @"insert into EmployeeOffDay(EmployeeID, OffDate, Note) values ({0}, '{1}', N'{2}')
                    select * from EmployeeOffDay where EmployeeID = {0} order by OffDate desc", id, date.ToString(Constants.DatabaseDatetimeString), note), true), out result).ToList();
        }
        public static List<EmployeeOffDay> Remove(int userID, int employeeID, int id)
        {
            QueryOutput result;
            return Query<EmployeeOffDay>(new DbQuery(userID, employeeID, SalaryAction.Calculate,
                String.Format(
                    @"declare @employeeID int = (select EmployeeID from EmployeeOffDay where ID = {0})
                    delete EmployeeOffDay where ID = {0}
                    select * from EmployeeOffDay where EmployeeID = @employeeID order by OffDate desc", id), true), out result).ToList();
        }
    }
}