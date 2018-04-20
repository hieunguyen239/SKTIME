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
    public class EmployeeController : BaseController
    {
        [LoginFilter]
        [HttpPost]
        public ActionResult Download(EmployeeFilter filter)
        {
            var result = false;
            try
            {
                var data = EmployeeInfo.Find(UserID, Employee.ID, Employee.BussinessID, "", filter, true);
                if (data != null)
                {
                    var fileName = String.Format("Employees_{0}.xls", DateTime.Now.ToString("ddMMyyyyHHmmss"));
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
        private void SaveDownload(string fileName, List<EmployeeInfo> list)
        {
            var workbook = new HSSFWorkbook();
            var worksheet = workbook.CreateSheet("Report");
            ExcelWorker.CellStyles = new List<ICellStyle>();
            ExcelWorker.CreateRow(worksheet, 0, new ExcelCell[] {
                ExcelWorker.CreateCell(workbook, "Tên", HSSFColor.RoyalBlue.Index, HSSFColor.White.Index),
                ExcelWorker.CreateCell(workbook, "Chức vụ", HSSFColor.RoyalBlue.Index, HSSFColor.White.Index),
                ExcelWorker.CreateCell(workbook, "Ngày sinh", HSSFColor.RoyalBlue.Index, HSSFColor.White.Index),
                ExcelWorker.CreateCell(workbook, "Ngày vào làm", HSSFColor.RoyalBlue.Index, HSSFColor.White.Index),
                ExcelWorker.CreateCell(workbook, "Lương căn bản", HSSFColor.RoyalBlue.Index, HSSFColor.White.Index),
                ExcelWorker.CreateCell(workbook, "Tài khoản NH", HSSFColor.RoyalBlue.Index, HSSFColor.White.Index),
                ExcelWorker.CreateCell(workbook, "Tên NH", HSSFColor.RoyalBlue.Index, HSSFColor.White.Index),
                ExcelWorker.CreateCell(workbook, "Chi nhánh NH", HSSFColor.RoyalBlue.Index, HSSFColor.White.Index),
                ExcelWorker.CreateCell(workbook, "Trạng thái", HSSFColor.RoyalBlue.Index, HSSFColor.White.Index)
            });
            for (var i = 0; i < list.Count; i++)
            {
                var employee = list[i];
                ExcelWorker.CreateRow(worksheet, i + 1, new ExcelCell[] {
                    ExcelWorker.CreateCell(workbook, employee.Name),
                    ExcelWorker.CreateCell(workbook, employee.Position),
                    ExcelWorker.CreateCell(workbook, employee.DOB.HasValue ? employee.DOB.Value.ToString(Constants.DateString) : ""),
                    ExcelWorker.CreateCell(workbook, employee.StartDate.HasValue ? employee.StartDate.Value.ToString(Constants.DateString) : ""),
                    ExcelWorker.CreateCell(workbook, employee.BaseSalaryString),
                    ExcelWorker.CreateCell(workbook, employee.BankNumber),
                    ExcelWorker.CreateCell(workbook, employee.BankName),
                    ExcelWorker.CreateCell(workbook, employee.BankBranch),
                    ExcelWorker.CreateCell(workbook, employee.WorkStatus)
                });
            }
            using (var fs = new FileStream(fileName, FileMode.OpenOrCreate, FileAccess.Write))
            {
                workbook.Write(fs);
            }
        }
        [LoginFilter(NonAuthorized = true)]
        [HttpGet]
        public ActionResult Find(string id)
        {
            var data = EmployeeInfo.Find(UserID, Employee.ID, Employee.BussinessID, "", new EmployeeFilter() { Name = id });
            return Json(new {
                list = data.Data.Select(e => new {
                    ID = e.ID,
                    Name = e.Name
                }).ToArray()
            }, JsonRequestBehavior.AllowGet);
        }
        [LoginFilter]
        [HttpGet]
        public ActionResult Detail(int id)
        {
            var data = EmployeeInfo.Get(UserID, Employee.ID, id, true);
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
            var data = EmployeeInfo.Find(UserID, Employee.ID, Employee.BussinessID);
            if (Request.IsAjaxRequest())
                return Json(new
                {
                    html = RenderPartialViewToString(Views.ListPartial, data)
                }, JsonRequestBehavior.AllowGet);
            return View(Views.List, data);
        }
        [LoginFilter]
        [HttpPost]
        public ActionResult List(EmployeeFilter filter)
        {
            var data = EmployeeInfo.Find(UserID, Employee.ID, Employee.BussinessID, "", filter, true);
            if (Request.IsAjaxRequest())
                return Json(new
                {
                    result = true,
                    html = RenderPartialViewToString(Views.ListPartial, data)
                }, JsonRequestBehavior.AllowGet);
            return View(Views.List, data);
        }
        [LoginFilter(NonAuthorized = true)]
        [HttpPost]
        public ActionResult UnassignedList()
        {
            var list = EmployeeInfo.GetUnassigned(UserID, Employee.ID, Employee.BussinessID);
            return Json(new
            {
                id = list.Select(e => e.ID).ToArray(),
                name = list.Select(e => e.Name).ToArray()
            }, JsonRequestBehavior.DenyGet);
        }
        [LoginFilter(NonAuthorized = true)]
        [HttpPost]
        public ActionResult DataList()
        {
            var list = EmployeeInfo.Find(UserID, Employee.ID, Employee.BussinessID);
            return Json(new
            {
                id = list.Data.Select(e => e.ID).ToArray(),
                name = list.Data.Select(e => e.Name).ToArray()
            }, JsonRequestBehavior.DenyGet);
        }
        [LoginFilter]
        [HttpGet]
        public ActionResult Create()
        {
            if (Request.IsAjaxRequest())
                return Json(new
                {
                    html = RenderPartialViewToString(Views.SavePartial, new EmployeeInfo(Employee.BussinessID))
                }, JsonRequestBehavior.AllowGet);
            return View(Views.Save, new EmployeeInfo(Employee.BussinessID));
        }
        [LoginFilter]
        [HttpGet]
        public ActionResult Update(int id)
        {
            var model = EmployeeInfo.Get(UserID, Employee.ID, id);
            if (Request.IsAjaxRequest())
                return Json(new
                {
                    html = RenderPartialViewToString(Views.SavePartial, model)
                }, JsonRequestBehavior.AllowGet);
            return View(Views.Save, model);
        }
        [LoginFilter]
        [HttpPost]
        public ActionResult Save(EmployeeInfo info)
        {
            var result = info.Save(ModelState, UserID, Employee.ID);
            var isAjaxRequest = Request.IsAjaxRequest();
            string view; object model;
            if (result)
            {
                view = isAjaxRequest ? Views.ListPartial : Views.List;
                model = EmployeeInfo.Find(UserID, Employee.ID, Employee.BussinessID, "Luu thông tin thành công");
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
            var message = "Xóa thông tin thành công";
            if (EmployeeInfo.IsAdmin(UserID, Employee.ID, id, DbAction.Employee.Remove))
                message = "Không thể xóa nhân viên có quyền admin";
            else
                EmployeeInfo.Remove(UserID, Employee.ID, id);
            var model = EmployeeInfo.Find(UserID, Employee.ID, Employee.BussinessID, message);
            if (Request.IsAjaxRequest())
                return Json(new
                {
                    html = RenderPartialViewToString(Views.ListPartial, model)
                }, JsonRequestBehavior.AllowGet);
            return View(Views.List, model);
        }
        [LoginFilter]
        [HttpGet]
        public ActionResult GetFine(int id)
        {
            var list = EmployeeFine.Get(UserID, Employee.ID, Employee.BussinessID, DbAction.Employee.View, new EmployeeFineFilter(id));
            return Json(new
            {
                html = RenderPartialViewToString("FineList", list)
            }, JsonRequestBehavior.AllowGet);
        }
        [LoginFilter]
        [HttpPost]
        public ActionResult AddFine(EmployeeFine fine)
        {
            var list = fine.Save(UserID, Employee.ID, Employee.BussinessID, DbAction.Employee.Modify);
            return Json(new
            {
                html = RenderPartialViewToString("FineList", list)
            }, JsonRequestBehavior.DenyGet);
        }
        [LoginFilter]
        [HttpGet]
        public ActionResult RemoveFine(int id, int subID)
        {
            var list = EmployeeFine.Remove(UserID, Employee.ID, Employee.BussinessID, DbAction.Employee.Modify, id, subID);
            return Json(new
            {
                html = RenderPartialViewToString("FineList", list)
            }, JsonRequestBehavior.AllowGet);
        }
    }
}