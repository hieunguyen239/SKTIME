using NPOI.HSSF.UserModel;
using NPOI.HSSF.Util;
using NPOI.SS.UserModel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace SKtimeManagement
{
    public class ReportController : BaseController
    {
        [LoginFilter]
        [HttpGet]
        public ActionResult Summary()
        {
            var model = new Report();
            if (Request.IsAjaxRequest())
                return Json(new
                {
                    html = RenderPartialViewToString(Views.SummaryPartial, model)
                }, JsonRequestBehavior.AllowGet);
            return View(Views.Summary, model);
        }
        [LoginFilter]
        [HttpPost]
        public ActionResult Summary(ReportFilter filter)
        {
            var model = Report.Get(UserID, Employee.ID, Employee.BussinessID, filter);
            if (Request.IsAjaxRequest())
                return Json(new
                {
                    html = RenderPartialViewToString(Views.SummaryPartial, model)
                }, JsonRequestBehavior.DenyGet);
            return View(Views.Summary, model);
        }
        [LoginFilter]
        [HttpGet]
        public ActionResult Detail()
        {
            var model = new Report();
            if (Request.IsAjaxRequest())
                return Json(new
                {
                    html = RenderPartialViewToString(Views.DetailPartial, model)
                }, JsonRequestBehavior.AllowGet);
            return View(Views.Detail, model);
        }
        [LoginFilter]
        [HttpPost]
        public ActionResult Detail(ReportFilter filter)
        {
            var model = Report.Get(UserID, Employee.ID, Employee.BussinessID, filter);
            if (Request.IsAjaxRequest())
                return Json(new
                {
                    html = RenderPartialViewToString(Views.DetailPartial, model)
                }, JsonRequestBehavior.DenyGet);
            return View(Views.Detail, model);
        }
        [LoginFilter]
        [HttpGet]
        public ActionResult Salary()
        {
            var model = new SalaryReport();
            if (Request.IsAjaxRequest())
                return Json(new
                {
                    html = RenderPartialViewToString(Views.SalaryPartial, model)
                }, JsonRequestBehavior.AllowGet);
            return View(Views.Salary, model);
        }
        [LoginFilter]
        [HttpPost]
        public ActionResult Salary(EmployeeFilter filter)
        {
            var model = SalaryReport.Get(UserID, Employee.ID, Employee.BussinessID, filter);
            if (Request.IsAjaxRequest())
                return Json(new
                {
                    html = RenderPartialViewToString(Views.SalaryPartial, model)
                }, JsonRequestBehavior.DenyGet);
            return View(Views.Salary, model);
        }
        [LoginFilter]
        [HttpPost]
        public ActionResult SalaryDownload(EmployeeFilter filter)
        {
            var result = false;
            try
            {
                var data = SalaryReport.Get(UserID, Employee.ID, Employee.BussinessID, filter);
                if (data != null)
                {
                    var fileName = String.Format("Salary_{0}.xls", DateTime.Now.ToString("ddMMyyyyHHmmss"));
                    var file = String.Format("{0}/Content/Download/{1}", SiteConfiguration.ApplicationPath, fileName);
                    Functions.CheckDirectory(String.Format("{0}/Content/Download/", SiteConfiguration.ApplicationPath));
                    SaveDownloadSalary(file, data);
                    Session[SessionKey.Download] = fileName;
                    result = true;
                }
            }
            catch { }
            return Json(new
            {
                result = result
            }, JsonRequestBehavior.DenyGet);
        }
        private void SaveDownloadSalary(string fileName, SalaryReport list)
        {
            var workbook = new HSSFWorkbook();
            var worksheet = workbook.CreateSheet("Report");
            ExcelWorker.CellStyles = new List<ICellStyle>();
            var cells = new List<ExcelCell>() { ExcelWorker.CreateCell(workbook, "Tên", HSSFColor.RoyalBlue.Index, HSSFColor.White.Index) };
            var totals = new Dictionary<DateTime, decimal>();
            foreach (var month in list.Months)
            {
                cells.Add(ExcelWorker.CreateCell(workbook, month.ToString("MM/yyyy"), HSSFColor.RoyalBlue.Index, HSSFColor.White.Index));
            }
            ExcelWorker.CreateRow(worksheet, 0, cells.ToArray());
            for (var i = 0; i < list.Records.Count; i++)
            {
                var record = list.Records[i];
                var dictionary = (IDictionary<string, object>)record;
                cells = new List<ExcelCell>() { ExcelWorker.CreateCell(workbook, dictionary["Name"]) };
                foreach (var month in list.Months)
                {
                    var value = 0m;
                    var key = month.ToString("_MMyyyy");
                    if (dictionary.ContainsKey(key))
                    {
                        value = (decimal)dictionary[key];
                    }
                    cells.Add(ExcelWorker.CreateCell(workbook, value.GetCurrencyString()));
                    if (!totals.ContainsKey(month))
                        totals.Add(month, value);
                    else
                        totals[month] = totals[month] + value;
                }
                ExcelWorker.CreateRow(worksheet, i + 1, cells.ToArray());
            }
            var index = list.Records.Count + 1;
            var totalCells = new List<ExcelCell>() { ExcelWorker.CreateCell(workbook, "Tổng cộng") };
            totalCells.AddRange(totals.Select(i => ExcelWorker.CreateCell(workbook, i.Value.GetCurrencyString())));
            ExcelWorker.CreateRow(worksheet, index, totalCells.ToArray());
            using (var fs = new FileStream(fileName, FileMode.OpenOrCreate, FileAccess.Write))
            {
                workbook.Write(fs);
            }
        }
        [LoginFilter]
        [HttpPost]
        public ActionResult RevenueDownload(ReportFilter filter)
        {
            var result = false;
            try
            {
                var report = Report.Get(UserID, Employee.ID, Employee.BussinessID, filter);
                var fileName = String.Format("Revenue_{0}.xls", DateTime.Now.ToString("ddMMyyyyHHmmss"));
                var file = String.Format("{0}/Content/Download/{1}", SiteConfiguration.ApplicationPath, fileName);
                Functions.CheckDirectory(String.Format("{0}/Content/Download/", SiteConfiguration.ApplicationPath));
                SaveRevenue(file, report);
                Session[SessionKey.Download] = fileName;
                result = true;
            }
            catch { }
            return Json(new
            {
                result = result
            }, JsonRequestBehavior.DenyGet);
        }
        private void SaveRevenue(string fileName, Report report)
        {
            var workbook = new HSSFWorkbook();
            var worksheet = workbook.CreateSheet("Report");
            ExcelWorker.CellStyles = new List<ICellStyle>();
            ExcelWorker.CreateRow(worksheet, 0, new ExcelCell[] {
                ExcelWorker.CreateCell(workbook, "Kho"),
                ExcelWorker.CreateCell(workbook, report.Filter.WarehouseID.HasValue ? report.PaidOrders.FirstOrDefault().WarehouseName : "Toàn bộ")
            });
            ExcelWorker.CreateRow(worksheet, 1, new ExcelCell[] {
                ExcelWorker.CreateCell(workbook, "Ngày", HSSFColor.RoyalBlue.Index, HSSFColor.White.Index),
                ExcelWorker.CreateCell(workbook, "Số hóa đơn", HSSFColor.RoyalBlue.Index, HSSFColor.White.Index),
                ExcelWorker.CreateCell(workbook, "Tổng hóa đơn", HSSFColor.RoyalBlue.Index, HSSFColor.White.Index),
                ExcelWorker.CreateCell(workbook, "Số hóa đơn trả", HSSFColor.RoyalBlue.Index, HSSFColor.White.Index),
                ExcelWorker.CreateCell(workbook, "Tổng hóa đơn trả", HSSFColor.RoyalBlue.Index, HSSFColor.White.Index),
                ExcelWorker.CreateCell(workbook, "Số phiếu thu", HSSFColor.RoyalBlue.Index, HSSFColor.White.Index),
                ExcelWorker.CreateCell(workbook, "Tổng thu", HSSFColor.RoyalBlue.Index, HSSFColor.White.Index),
                ExcelWorker.CreateCell(workbook, "Số phiếu chi", HSSFColor.RoyalBlue.Index, HSSFColor.White.Index),
                ExcelWorker.CreateCell(workbook, "Tổng chi", HSSFColor.RoyalBlue.Index, HSSFColor.White.Index)
            });
            var current = report.Filter.From.Value;
            var index = 2;
            while (current < report.Filter.To)
            {
                var start = new DateTime(current.Year, current.Month, current.Day, 0, 0, 0);
                var end = start.AddDays(1);
                ExcelWorker.CreateRow(worksheet, index, new ExcelCell[] {
                    ExcelWorker.CreateCell(workbook, current.ToString("dd/MM/yyyy")),
                    ExcelWorker.CreateCell(workbook, report.PaidOrders.Count(i => i.Status != OrderStatus.Refunded && i.SubmitDate >= start && i.SubmitDate <= end).GetCurrencyString()),
                    ExcelWorker.CreateCell(workbook, report.PaidOrders.Where(i => i.Status != OrderStatus.Refunded && i.SubmitDate >= start && i.SubmitDate <= end).Sum(i => i.Total - i.Discount).GetCurrencyString()),
                    ExcelWorker.CreateCell(workbook, report.PaidOrders.Count(i => i.Status == OrderStatus.Refunded && i.SubmitDate >= start && i.SubmitDate <= end).GetCurrencyString()),
                    ExcelWorker.CreateCell(workbook, report.PaidOrders.Where(i => i.Status == OrderStatus.Refunded && i.SubmitDate >= start && i.SubmitDate <= end).Sum(i => i.Total - i.Discount).GetCurrencyString()),
                    ExcelWorker.CreateCell(workbook, report.Incomes.Count(i => i.SubmitDate >= start && i.SubmitDate <= end).GetCurrencyString()),
                    ExcelWorker.CreateCell(workbook, report.Incomes.Where(i => i.SubmitDate >= start && i.SubmitDate <= end).Sum(i => i.Amount).GetCurrencyString()),
                    ExcelWorker.CreateCell(workbook, report.Outcomes.Count(i => i.SubmitDate >= start && i.SubmitDate <= end).GetCurrencyString()),
                    ExcelWorker.CreateCell(workbook, report.Outcomes.Where(i => i.SubmitDate >= start && i.SubmitDate <= end).Sum(i => i.Amount).GetCurrencyString())
                });
                index++;
                current = current.AddDays(1);
            }
            ExcelWorker.CreateRow(worksheet, index, new ExcelCell[] {
                    ExcelWorker.CreateCell(workbook, "Tổng cộng"),
                    ExcelWorker.CreateCell(workbook, report.PaidOrders.Count(i => i.Status != OrderStatus.Refunded).GetCurrencyString()),
                    ExcelWorker.CreateCell(workbook, report.PaidOrders.Where(i => i.Status != OrderStatus.Refunded).Sum(i => i.Total - i.Discount).GetCurrencyString()),
                    ExcelWorker.CreateCell(workbook, report.PaidOrders.Count(i => i.Status == OrderStatus.Refunded).GetCurrencyString()),
                    ExcelWorker.CreateCell(workbook, report.PaidOrders.Where(i => i.Status == OrderStatus.Refunded).Sum(i => i.Total - i.Discount).GetCurrencyString()),
                    ExcelWorker.CreateCell(workbook, report.Incomes.Count.GetCurrencyString()),
                    ExcelWorker.CreateCell(workbook, report.Incomes.Sum(i => i.Amount).GetCurrencyString()),
                    ExcelWorker.CreateCell(workbook, report.Outcomes.Count.GetCurrencyString()),
                    ExcelWorker.CreateCell(workbook, report.Outcomes.Sum(i => i.Amount).GetCurrencyString())
                });
            using (var fs = new FileStream(fileName, FileMode.OpenOrCreate, FileAccess.Write))
            {
                workbook.Write(fs);
            }
        }
        [LoginFilter]
        [HttpPost]
        public ActionResult SaleDownload(ReportFilter filter)
        {
            var result = false;
            try
            {
                var report = Report.Get(UserID, Employee.ID, Employee.BussinessID, filter, true);
                var fileName = String.Format("Revenue_{0}.xls", DateTime.Now.ToString("ddMMyyyyHHmmss"));
                var file = String.Format("{0}/Content/Download/{1}", SiteConfiguration.ApplicationPath, fileName);
                Functions.CheckDirectory(String.Format("{0}/Content/Download/", SiteConfiguration.ApplicationPath));
                SaveSale(file, report);
                Session[SessionKey.Download] = fileName;
                result = true;
            }
            catch { }
            return Json(new
            {
                result = result
            }, JsonRequestBehavior.DenyGet);
        }
        private void SaveSale(string fileName, Report report)
        {
            var workbook = new HSSFWorkbook();
            var worksheet = workbook.CreateSheet("Report");
            ExcelWorker.CellStyles = new List<ICellStyle>();
            var employees = report.PaidOrders.GroupBy(i => i.EmployeeID).Select(i => new { ID = i.Key, Name = i.FirstOrDefault().EmployeeName }).ToArray();
            var header = new List<ExcelCell>() { ExcelWorker.CreateCell(workbook, "Tháng", HSSFColor.RoyalBlue.Index, HSSFColor.White.Index) };
            foreach (var employee in employees)
            {
                header.Add(ExcelWorker.CreateCell(workbook, employee.Name, HSSFColor.RoyalBlue.Index, HSSFColor.White.Index));
            }
            ExcelWorker.CreateRow(worksheet, 0, header.ToArray());
            var current = report.Filter.From.Value;
            var index = 1;
            while (current < report.Filter.To)
            {
                var start = new DateTime(current.Year, current.Month, 1, 0, 0, 0);
                var end = start.AddMonths(1);
                var cells = new List<ExcelCell>() { ExcelWorker.CreateCell(workbook, current.ToString("MM/yyyy")) };
                foreach (var employee in employees)
                {
                    cells.Add(ExcelWorker.CreateCell(workbook, report.PaidOrders.Where(i => i.EmployeeID == employee.ID && i.Status != OrderStatus.Refunded && i.SubmitDate >= start && i.SubmitDate <= end).Sum(i => i.Total - i.Discount).GetCurrencyString()));
                }
                ExcelWorker.CreateRow(worksheet, index, cells.ToArray());
                index++;
                current = current.AddMonths(1);
            }
            using (var fs = new FileStream(fileName, FileMode.OpenOrCreate, FileAccess.Write))
            {
                workbook.Write(fs);
            }
        }
        [LoginFilter]
        [HttpGet]
        public ActionResult Product()
        {
            var model = new ProductReport();
            if (Request.IsAjaxRequest())
                return Json(new
                {
                    html = RenderPartialViewToString(Views.ProductPartial, model)
                }, JsonRequestBehavior.AllowGet);
            return View(Views.Product, model);
        }
        [LoginFilter]
        [HttpPost]
        public ActionResult Product(ReportFilter filter)
        {
            var model = ProductReport.Get(Employee.BussinessID, filter);
            if (Request.IsAjaxRequest())
                return Json(new
                {
                    html = RenderPartialViewToString(Views.ProductPartial, model)
                }, JsonRequestBehavior.DenyGet);
            return View(Views.Product, model);
        }
        [LoginFilter]
        [HttpPost]
        public ActionResult ProductDownload(ReportFilter filter)
        {
            var result = false;
            try
            {
                var report = ProductReport.Get(Employee.BussinessID, filter);
                var fileName = String.Format("Revenue_{0}.xls", DateTime.Now.ToString("ddMMyyyyHHmmss"));
                var file = String.Format("{0}/Content/Download/{1}", SiteConfiguration.ApplicationPath, fileName);
                Functions.CheckDirectory(String.Format("{0}/Content/Download/", SiteConfiguration.ApplicationPath));
                SaveProduct(file, report);
                Session[SessionKey.Download] = fileName;
                result = true;
            }
            catch { }
            return Json(new
            {
                result = result
            }, JsonRequestBehavior.DenyGet);
        }
        private void SaveProduct(string fileName, ProductReport report)
        {
            var workbook = new HSSFWorkbook();
            if (report.Filter.WarehouseID.HasValue)
                SaveProductWarehouse(workbook, report.Products, report.Warehouses.FirstOrDefault(w => w.ID == report.Filter.WarehouseID.Value));
            else
            {
                SaveProductWarehouse(workbook, report.Products);
                foreach (var warehouse in report.Warehouses)
                {
                    SaveProductWarehouse(workbook, report.Products.Where(i => i.WarehouseID == warehouse.ID), warehouse);
                }
            }
            using (var fs = new FileStream(fileName, FileMode.OpenOrCreate, FileAccess.Write))
            {
                workbook.Write(fs);
            }
        }
        private void SaveProductWarehouse(HSSFWorkbook workbook, IEnumerable<ProductSaleReport> products, WarehouseInfo warehouse = null)
        {
            var name = warehouse != null ? warehouse.Name : "Toàn bộ";
            var worksheet = workbook.CreateSheet(name);
            ExcelWorker.CellStyles = new List<ICellStyle>();
            ExcelWorker.CreateRow(worksheet, 0, new ExcelCell[] {
                ExcelWorker.CreateCell(workbook, "Kho"),
                ExcelWorker.CreateCell(workbook, name)
            });
            ExcelWorker.CreateRow(worksheet, 0, new ExcelCell[] {
                ExcelWorker.CreateCell(workbook, "Mã", HSSFColor.RoyalBlue.Index, HSSFColor.White.Index),
                ExcelWorker.CreateCell(workbook, "Tên", HSSFColor.RoyalBlue.Index, HSSFColor.White.Index),
                ExcelWorker.CreateCell(workbook, "Tỷ lệ (số lượng)", HSSFColor.RoyalBlue.Index, HSSFColor.White.Index),
                ExcelWorker.CreateCell(workbook, "Tỷ lệ (tổng tiền)", HSSFColor.RoyalBlue.Index, HSSFColor.White.Index),
                ExcelWorker.CreateCell(workbook, "Số lượng bán", HSSFColor.RoyalBlue.Index, HSSFColor.White.Index),
                ExcelWorker.CreateCell(workbook, "Tổng tiền bán", HSSFColor.RoyalBlue.Index, HSSFColor.White.Index),
                ExcelWorker.CreateCell(workbook, "Số lượng nhập", HSSFColor.RoyalBlue.Index, HSSFColor.White.Index),
                ExcelWorker.CreateCell(workbook, "Tổng tiền nhập", HSSFColor.RoyalBlue.Index, HSSFColor.White.Index)
            });
            var index = 1;
            foreach (var product in products)
            {
                ExcelWorker.CreateRow(worksheet, index, new ExcelCell[] {
                    ExcelWorker.CreateCell(workbook, product.Code),
                    ExcelWorker.CreateCell(workbook, product.Name),
                    ExcelWorker.CreateCell(workbook, Math.Round(product.QuantityPercentage, 2).ToString()),
                    ExcelWorker.CreateCell(workbook, Math.Round(product.RevenuePercentage, 2).ToString()),
                    ExcelWorker.CreateCell(workbook, product.OrderQuantityString),
                    ExcelWorker.CreateCell(workbook, product.OrderTotalString),
                    ExcelWorker.CreateCell(workbook, product.ImportQuantityString),
                    ExcelWorker.CreateCell(workbook, product.ImportTotalString)
                });
                index++;
            }
            ExcelWorker.CreateRow(worksheet, index, new ExcelCell[] {
                ExcelWorker.CreateCell(workbook, ""),
                ExcelWorker.CreateCell(workbook, ""),
                ExcelWorker.CreateCell(workbook, ""),
                ExcelWorker.CreateCell(workbook, "Tổng cộng"),
                ExcelWorker.CreateCell(workbook, products.Sum(i => i.OrderQuantity).GetCurrencyString()),
                ExcelWorker.CreateCell(workbook, products.Sum(i => i.OrderTotal).GetCurrencyString()),
                ExcelWorker.CreateCell(workbook, products.Sum(i => i.ImportQuantity).GetCurrencyString()),
                ExcelWorker.CreateCell(workbook, products.Sum(i => i.ImportTotal).GetCurrencyString())
            });
        }
        [LoginFilter]
        [HttpGet]
        public ActionResult Client()
        {
            var model = new ClientReport();
            if (Request.IsAjaxRequest())
                return Json(new
                {
                    html = RenderPartialViewToString(Views.ClientPartial, model)
                }, JsonRequestBehavior.AllowGet);
            return View(Views.Client, model);
        }
        [LoginFilter]
        [HttpPost]
        public ActionResult Client(ClientFilter filter)
        {
            var model = ClientReport.Get(Employee.BussinessID, filter);
            if (Request.IsAjaxRequest())
                return Json(new
                {
                    html = RenderPartialViewToString(Views.ClientPartial, model)
                }, JsonRequestBehavior.AllowGet);
            return View(Views.Client, model);
        }
        [LoginFilter]
        [HttpGet]
        public ActionResult ProductPart()
        {
            var model = new ProductPartReport();
            if (Request.IsAjaxRequest())
                return Json(new
                {
                    html = RenderPartialViewToString(Views.ProductPartPartial, model)
                }, JsonRequestBehavior.AllowGet);
            return View(Views.ProductPart, model);
        }
        [LoginFilter]
        [HttpPost]
        public ActionResult ProductPart(ProductFilter filter)
        {
            var model = ProductPartReport.Get(UserID, Employee.BussinessID, filter);
            if (Request.IsAjaxRequest())
                return Json(new
                {
                    html = RenderPartialViewToString(Views.ProductPartPartial, model)
                }, JsonRequestBehavior.AllowGet);
            return View(Views.ProductPart, model);
        }
    }
}