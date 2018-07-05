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
    public class IncomeController : BaseController
    {
        [LoginFilter]
        [HttpGet]
        public ActionResult Detail(int id)
        {
            var data = IncomeInfo.Get(UserID, Employee.ID, id, true);
            if (Request.IsAjaxRequest())
                return Json(new
                {
                    html = RenderPartialViewToString(Views.DetailPartial, data)
                }, JsonRequestBehavior.AllowGet);
            return View(Views.Detail, data);
        }
        [LoginFilter]
        [HttpGet]
        public ActionResult List()
        {
            var data = IncomeInfo.Find(UserID, Employee.ID, Employee.BussinessID);
            if (Request.IsAjaxRequest())
                return Json(new
                {
                    html = RenderPartialViewToString(Views.ListPartial, data)
                }, JsonRequestBehavior.AllowGet);
            return View(Views.List, data);
        }
        [LoginFilter]
        [HttpPost]
        public ActionResult List(IncomeFilter filter)
        {
            var data = IncomeInfo.Find(UserID, Employee.ID, Employee.BussinessID, "", filter, true);
            if (Request.IsAjaxRequest())
                return Json(new
                {
                    result = true,
                    html = RenderPartialViewToString(Views.ListPartial, data)
                }, JsonRequestBehavior.AllowGet);
            return View(Views.List, data);
        }
        [LoginFilter]
        [HttpGet]
        public ActionResult Create()
        {
            if (Request.IsAjaxRequest())
                return Json(new
                {
                    html = RenderPartialViewToString(Views.SavePartial, new IncomeInfo(Employee.BussinessID, Employee.ID))
                }, JsonRequestBehavior.AllowGet);
            return View(Views.Save, new IncomeInfo(Employee.BussinessID, Employee.ID));
        }
        [LoginFilter]
        [HttpGet]
        public ActionResult Update(int id)
        {
            var model = IncomeInfo.Get(UserID, Employee.ID, id);
            if (Request.IsAjaxRequest())
                return Json(new
                {
                    html = RenderPartialViewToString(Views.SavePartial, model)
                }, JsonRequestBehavior.AllowGet);
            return View(Views.Save, model);
        }
        [LoginFilter]
        [HttpPost]
        public ActionResult Save(IncomeInfo info)
        {
            var result = info.Save(ModelState, UserID, Employee.ID, Employee.BussinessID);
            var isAjaxRequest = Request.IsAjaxRequest();
            string view; object model;
            if (result)
            {
                view = isAjaxRequest ? Views.ListPartial : Views.List;
                model = IncomeInfo.Find(UserID, Employee.ID, Employee.BussinessID, "Luu thông tin thành công");
            }
            else
            {
                view = isAjaxRequest ? Views.SavePartial : Views.Save;
                model = info;
            }
            if (isAjaxRequest)
            {
                return Json(new
                {
                    result = result,
                    html = RenderPartialViewToString(view, model),
                },
                JsonRequestBehavior.DenyGet);
            }
            else
            {
                return View(view, model);
            }
        }
        [LoginFilter]
        [HttpGet]
        public ActionResult Remove(int id)
        {
            IncomeInfo.Remove(UserID, Employee.ID, id);
            var model = IncomeInfo.Find(UserID, Employee.ID, Employee.BussinessID, "Xóa thông tin thành công");
            if (Request.IsAjaxRequest())
                return Json(new
                {
                    html = RenderPartialViewToString(Views.ListPartial, model)
                }, JsonRequestBehavior.AllowGet);
            return View(Views.List, model);
        }
        [LoginFilter]
        [HttpPost]
        public ActionResult Download(IncomeFilter filter)
        {
            var result = false;
            try
            {
                var data = IncomeInfo.Find(UserID, Employee.ID, Employee.BussinessID, "", filter, true, null);
                if (data != null)
                {
                    var fileName = String.Format("Incomes_{0}.xls", DateTime.Now.ToString("ddMMyyyyHHmmss"));
                    var file = String.Format("{0}/Content/Download/{1}", SiteConfiguration.ApplicationPath, fileName);
                    Functions.CheckDirectory(String.Format("{0}/Content/Download/", SiteConfiguration.ApplicationPath));
                    SaveDownload(file, data.Data);
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
        private void SaveDownload(string fileName, List<IncomeInfo> products)
        {
            var workbook = new HSSFWorkbook();
            var worksheet = workbook.CreateSheet("Report");
            ExcelWorker.CellStyles = new List<ICellStyle>();
            ExcelWorker.CreateRow(worksheet, 0, new ExcelCell[] {
                ExcelWorker.CreateCell(workbook, "Mã", HSSFColor.RoyalBlue.Index, HSSFColor.White.Index),
                ExcelWorker.CreateCell(workbook, "Kho", HSSFColor.RoyalBlue.Index, HSSFColor.White.Index),
                ExcelWorker.CreateCell(workbook, "Nhân viên", HSSFColor.RoyalBlue.Index, HSSFColor.White.Index),
                ExcelWorker.CreateCell(workbook, "Ngày tạo", HSSFColor.RoyalBlue.Index, HSSFColor.White.Index),
                ExcelWorker.CreateCell(workbook, "Số tiền", HSSFColor.RoyalBlue.Index, HSSFColor.White.Index),
                ExcelWorker.CreateCell(workbook, "Phương thức", HSSFColor.RoyalBlue.Index, HSSFColor.White.Index),
                ExcelWorker.CreateCell(workbook, "Lý do", HSSFColor.RoyalBlue.Index, HSSFColor.White.Index),
                ExcelWorker.CreateCell(workbook, "Ghi chú", HSSFColor.RoyalBlue.Index, HSSFColor.White.Index)
            });
            for (var i = 0; i < products.Count; i++)
            {
                var product = products[i];
                ExcelWorker.CreateRow(worksheet, i + 1, new ExcelCell[] {
                    ExcelWorker.CreateCell(workbook, product.Code),
                    ExcelWorker.CreateCell(workbook, product.WarehouseName),
                    ExcelWorker.CreateCell(workbook, product.EmployeeName),
                    ExcelWorker.CreateCell(workbook, product.SubmitDate.ToString(Constants.DateTimeString)),
                    ExcelWorker.CreateCell(workbook, product.Amount.GetCurrencyString()),
                    ExcelWorker.CreateCell(workbook, product.Type),
                    ExcelWorker.CreateCell(workbook, product.Reason),
                    ExcelWorker.CreateCell(workbook, product.Note)
                });
            }
            ExcelWorker.CreateRow(worksheet, products.Count + 1, new ExcelCell[] {
                ExcelWorker.CreateCell(workbook, ""),
                ExcelWorker.CreateCell(workbook, ""),
                ExcelWorker.CreateCell(workbook, ""),
                ExcelWorker.CreateCell(workbook, "Tổng tiền"),
                ExcelWorker.CreateCell(workbook, products.Sum(i => i.Amount).GetCurrencyString())
            });
            using (var fs = new FileStream(fileName, FileMode.OpenOrCreate, FileAccess.Write))
            {
                workbook.Write(fs);
            }
        }
    }
}