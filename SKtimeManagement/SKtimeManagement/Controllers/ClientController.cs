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
    public class ClientController : BaseController
    {
        [LoginFilter(NonAuthorized = true)]
        [HttpPost]
        public ActionResult Find(string code, string name, string phone, string address)
        {
            var data = ClientInfo.Find(UserID, Employee.ID, Employee.BussinessID, "", new ClientFilter() { Code = code, Name = name, Phone = phone, Address = address });
            return Json(new
            {
                list = data.Data.Take(3).Select(c => new {
                    ID = c.ID,
                    Code = c.Code,
                    Name = c.Name,
                    Phone = c.Phone,
                    Address = c.Address,
                    Type = String.IsNullOrEmpty(c.TypeName) ? "Khách thường" : c.TypeName
                }).ToArray()
            }, JsonRequestBehavior.AllowGet);
        }
        [LoginFilter(NonAuthorized = true)]
        [HttpGet]
        public ActionResult Find(string id)
        {
            var data = ClientInfo.Find(UserID, Employee.ID, Employee.BussinessID, "", new ClientFilter() { Name = id });
            return Json(new
            {
                list = data.Data.Select(c => new {
                    ID = c.ID,
                    Name = c.Name
                }).ToArray()
            }, JsonRequestBehavior.AllowGet);
        }
        [LoginFilter]
        [HttpGet]
        public ActionResult Discount(int id)
        {
            var type = ClientInfo.GetClientType(UserID, Employee.ID, id);
            return Json(new
            {
                type = type != null ? type.DiscountType : "",
                value = type != null ? type.DiscountValue : 0
            }, JsonRequestBehavior.AllowGet);
        }
        [LoginFilter]
        [HttpGet]
        public ActionResult Detail(int id)
        {
            var data = ClientInfo.Get(UserID, Employee.ID, id, true);
            data.OrderHistory = ClientInfo.GetClientOrders(UserID, Employee.ID, Employee.BussinessID, id);
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
            var data = ClientInfo.Find(UserID, Employee.ID, Employee.BussinessID, "", null, false, null);
            if (Request.IsAjaxRequest())
                return Json(new
                {
                    html = RenderPartialViewToString(Views.ListPartial, data)
                }, JsonRequestBehavior.AllowGet);
            return View(Views.List, data);
        }
        [LoginFilter]
        [HttpPost]
        public ActionResult List(ClientFilter filter)
        {
            var data = ClientInfo.Find(UserID, Employee.ID, Employee.BussinessID, "", filter, true, null);
            if (Request.IsAjaxRequest())
                return Json(new
                {
                    result = true,
                    html = RenderPartialViewToString(Views.ListPartial, data)
                }, JsonRequestBehavior.AllowGet);
            return View(Views.List, data);
        }
        [LoginFilter]
        [HttpPost]
        public ActionResult Download(ClientFilter filter)
        {
            var result = false;
            try
            {
                var data = ClientInfo.Find(UserID, Employee.ID, Employee.BussinessID, "", filter, true, null);
                var fileName = String.Format("Client_{0}.xls", DateTime.Now.ToString("ddMMyyyyHHmmss"));
                var file = String.Format("{0}/Content/Download/{1}", SiteConfiguration.ApplicationPath, fileName);
                Functions.CheckDirectory(String.Format("{0}/Content/Download/", SiteConfiguration.ApplicationPath));
                SaveDownload(file, data);
                Session[SessionKey.Download] = fileName;
                result = true;
            }
            catch { }
            return Json(new
            {
                result = result
            }, JsonRequestBehavior.DenyGet);
        }
        private void SaveDownload(string fileName, ClientList list)
        {
            var workbook = new HSSFWorkbook();
            var worksheet = workbook.CreateSheet("Report");
            ExcelWorker.CellStyles = new List<ICellStyle>();
            ExcelWorker.CreateRow(worksheet, 0, new ExcelCell[] {
                ExcelWorker.CreateCell(workbook, "Mã", HSSFColor.RoyalBlue.Index, HSSFColor.White.Index),
                ExcelWorker.CreateCell(workbook, "Tên", HSSFColor.RoyalBlue.Index, HSSFColor.White.Index),
                ExcelWorker.CreateCell(workbook, "SĐT", HSSFColor.RoyalBlue.Index, HSSFColor.White.Index),
                ExcelWorker.CreateCell(workbook, "Loại", HSSFColor.RoyalBlue.Index, HSSFColor.White.Index),
                ExcelWorker.CreateCell(workbook, "Điểm", HSSFColor.RoyalBlue.Index, HSSFColor.White.Index),
                ExcelWorker.CreateCell(workbook, "Doanh số", HSSFColor.RoyalBlue.Index, HSSFColor.White.Index)
            });
            for (var i = 0; i < list.Data.Count; i++)
            {
                var client = list.Data[i];
                client.OrderHistory = ClientInfo.GetClientOrders(UserID, Employee.ID, Employee.BussinessID, client.ID);
                ExcelWorker.CreateRow(worksheet, i + 1, new ExcelCell[] {
                    ExcelWorker.CreateCell(workbook, client.Code),
                    ExcelWorker.CreateCell(workbook, client.Name),
                    ExcelWorker.CreateCell(workbook, client.Phone),
                    ExcelWorker.CreateCell(workbook, client.TypeName),
                    ExcelWorker.CreateCell(workbook, client.Point.GetCurrencyString()),
                    ExcelWorker.CreateCell(workbook, client.OrderHistory.Sum(o => o.Paid).GetCurrencyString())
                });
            }
            using (var fs = new FileStream(fileName, FileMode.OpenOrCreate, FileAccess.Write))
            {
                workbook.Write(fs);
            }
        }
        [LoginFilter(NonAuthorized = true)]
        [HttpPost]
        public ActionResult KeyList()
        {
            var list = ClientInfo.KeyList(UserID, Employee.ID, Employee.BussinessID);
            return Json(new
            {
                id = list.Select(l => l.ID).ToArray(),
                name = list.Select(l => l.Name).ToArray()
            }, JsonRequestBehavior.AllowGet);
        }
        [LoginFilter]
        [HttpGet]
        public ActionResult Create()
        {
            var model = new ClientInfo(Employee.BussinessID);
            model.Code = BaseModel.NewUniqueCode(UserID, Employee.ID, Employee.BussinessID, "Client", 3, null);
            if (Request.IsAjaxRequest())
                return Json(new
                {
                    html = RenderPartialViewToString(Views.SavePartial, model)
                }, JsonRequestBehavior.AllowGet);
            return View(Views.Save, model);
        }
        [LoginFilter]
        [HttpGet]
        public ActionResult Update(int id)
        {
            var model = ClientInfo.Get(UserID, Employee.ID, id);
            if (Request.IsAjaxRequest())
                return Json(new
                {
                    html = RenderPartialViewToString(Views.SavePartial, model)
                }, JsonRequestBehavior.AllowGet);
            return View(Views.Save, model);
        }
        [LoginFilter]
        [HttpPost]
        public ActionResult Save(ClientInfo info)
        {
            var result = info.Save(ModelState, UserID, Employee.ID);
            var isAjaxRequest = Request.IsAjaxRequest();
            string view; object model;
            if (result)
            {
                view = isAjaxRequest ? Views.ListPartial : Views.List;
                model = ClientInfo.Find(UserID, Employee.ID, Employee.BussinessID, "Luu thông tin thành công", null, false, null);
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
            ClientInfo.Remove(UserID, Employee.ID, id);
            var model = ClientInfo.Find(UserID, Employee.ID, Employee.BussinessID, "Xóa thông tin thành công", null, false, null);
            if (Request.IsAjaxRequest())
                return Json(new
                {
                    html = RenderPartialViewToString(Views.ListPartial, model)
                }, JsonRequestBehavior.AllowGet);
            return View(Views.List, model);
        }
    }
}