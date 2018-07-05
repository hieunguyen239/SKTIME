using NPOI.HSSF.UserModel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace SKtimeManagement
{
    public class ExportController : BaseController
    {
        [LoginFilter]
        [HttpPost]
        public ActionResult ProductList(ProductFilter filter)
        {
            var data = ImexItem.Find(UserID, Employee.ID, filter.Action, Employee.BussinessID, filter);
            return Json(new
            {
                result = true,
                html = RenderPartialViewToString(Views.Selector, data)
            }, JsonRequestBehavior.AllowGet);
        }
        [LoginFilter]
        [HttpGet]
        public ActionResult Create()
        {
            var login = Login.Get(UserID);
            var model = new Export(Employee.Name,
                WarehouseInfo.FindAuthorized(UserID, Employee.ID, Employee.BussinessID, UserID, DbAction.Export.View));
            model.Editable = login.Username == "admin";
            if (Request.IsAjaxRequest())
                return Json(new
                {
                    html = RenderPartialViewToString(Views.ExportPartial, model)
                }, JsonRequestBehavior.AllowGet);
            return View(Views.Export, model);
        }
        [LoginFilter]
        [HttpPost]
        public ActionResult UploadFile(int id)
        {
            var list = new List<ImexItem>();
            var result = false;
            var message = "";
            foreach (string f in Request.Files)
            {
                var fileContent = Request.Files[f];
                if (fileContent != null && fileContent.ContentLength > 0)
                {
                    // get a stream
                    var stream = fileContent.InputStream;
                    var fileName = String.Format("{1}_{0}", Path.GetFileName(fileContent.FileName), DateTime.Now.ToString("yyyy-MM-dd HH-mm-ss"));
                    FileInfo fileInfo;
                    FileManagement.SaveFile("Import/Export", fileName, stream, out fileInfo);
                    list = ImexItem.Read(UserID, Employee.ID, fileInfo, Employee.BussinessID, id, true);
                    var unavailable = list.Count(i => i.ID <= 0 || i.WarehouseID <= 0);
                    if (unavailable > 0)
                    {
                        message = String.Format("{0} sản phẩm không tìm thấy", unavailable);
                    }
                    else
                    {
                        message = "Nhập sản phẩn từ file thành công";
                        result = true;
                    }
                }
                break;
            }
            return Json(new
            {
                result = result,
                list = ImexItem.ToJson(list.Where(i => i.ID > 0 && i.WarehouseID > 0)),
                message = message
            }, JsonRequestBehavior.DenyGet);
        }
        [LoginFilter]
        [HttpGet]
        public ActionResult UploadTemplate()
        {
            string fileName = "ExportTemplate.xls";
            var file = String.Format("{0}/Content/Template/{1}", SiteConfiguration.ApplicationPath, fileName);
            //UpdateTemplate(file);
            byte[] fileBytes = System.IO.File.ReadAllBytes(file);
            return File(fileBytes, System.Net.Mime.MediaTypeNames.Application.Octet, fileName);
        }
        private void UpdateTemplate(string fileName)
        {
            var warehouses = WarehouseInfo.Find(UserID, Employee.ID, Employee.BussinessID);
            if (warehouses.Data.Count > 0)
            {
                HSSFWorkbook hssfwb;
                using (FileStream file = new FileStream(fileName, FileMode.Open, FileAccess.Read))
                {
                    hssfwb = new HSSFWorkbook(file);
                }

                var sheet = hssfwb.GetSheetAt(0);
                var rowCount = sheet.LastRowNum;
                for (var i = 0; i < warehouses.Data.Count; i++)
                {
                    if (i + 1 > rowCount)
                        sheet.CreateRow(i + 1);
                    var row = sheet.GetRow(i + 1);
                    if (row.Cells.FirstOrDefault(c => c.ColumnIndex == 10) == null)
                        row.CreateCell(10);
                    row.Cells.FirstOrDefault(c => c.ColumnIndex == 10).SetCellValue(warehouses.Data[i].Name);
                }
                using (var fs = new FileStream(fileName, FileMode.OpenOrCreate, FileAccess.Write))
                {
                    hssfwb.Write(fs);
                }
            }
        }
        [LoginFilter]
        [HttpGet]
        public ActionResult Add(int id, int subID)
        {
            var product = ImexItem.Get(UserID, Employee.ID, DbAction.Export.View, id, subID);
            product.Quantity = 1;
            if (Request.IsAjaxRequest())
                return Json(new
                {
                    result = true,
                    html = RenderPartialViewToString(Views.List, product)
                }, JsonRequestBehavior.AllowGet);
            return RedirectToAction("Create");
        }
        [LoginFilter]
        [HttpPost]
        public ActionResult Remove(int id, int whid)
        {
            var list = SessionValue<List<ImexItem>>(SessionKey.ExportList);
            if (list == null)
                list = new List<ImexItem>();
            list.Remove(list.FirstOrDefault(i => i.ID == id && i.WarehouseID == whid));
            Session[SessionKey.ExportList] = list;
            return Json(new
            {
                data = ImexItem.ToJson(list),
                total = list.Sum(i => i.Quantity * i.Price).GetCurrencyString()
            }, JsonRequestBehavior.DenyGet);
        }
        [LoginFilter]
        [HttpPost]
        public ActionResult Submit(int id, int warehouseID, int? toWarehouseID, ImexItem[] items, string note, string status)
        {
            var action = id > 0 ? DbAction.Export.Modify : DbAction.Export.Create;
            var unavailable = ImexItem.Unavailable(UserID, Employee.ID, warehouseID, action, items, 0, id);
            var exportID = 0;
            var message = "";
            var result = false;
            var login = Login.Get(UserID);
            if (id > 0 && login.Username != "admin" && login.Type != LoginType.Office)
                message = "Bạn không có quyền cập nhật phiếu xuất";
            else if (unavailable.Count > 0)
            {
                message = String.Format("Số lượng vượt mức tồn kho:<br/>{0}", String.Join("<br/>", unavailable.Select(i => i.Name)));
            }
            else if (toWarehouseID.HasValue && toWarehouseID.Value == warehouseID)
            {
                message = "Kho nhận trùng kho xuất";
            }
            else
            {
                message = "Lưu thông tin không thành công";
                if (items != null && items.Length > 0)
                {
                    if ((exportID = SKtimeManagement.Export.Submit(id, UserID, Employee.ID, Employee.BussinessID, warehouseID, toWarehouseID, status, items, note)) > 0)
                    {
                        message = "Lưu thông tin thành công";
                        result = true;
                    }
                }
                else
                    message = "Không có sản phẩm được chọn";
            }
            if (result)
            {
                var model = new ExportList();
                model.List = Export.History(UserID, Employee.ID, Employee.BussinessID);
                model.Current = Export.GetExport(UserID, Employee.ID, exportID);
                foreach (var product in model.Current.Products)
                {
                    product.ProductUrl = RenderPartialViewToString("ProductUrl", product);
                }
                if (Request.IsAjaxRequest())
                    return Json(new
                    {
                        html = RenderPartialViewToString(Views.HistoryPartial, model)
                    }, JsonRequestBehavior.DenyGet);
                return RedirectToAction(Views.History);
            }
            else
            {
                var model = new Export(Employee.Name,
                    WarehouseInfo.FindAuthorized(UserID, Employee.ID, Employee.BussinessID, UserID, DbAction.Export.View), 
                    toWarehouseID, status, warehouseID, items, note, message, result);
                model.ID = id;
                model.Editable = login.Username == "admin";
                if (Request.IsAjaxRequest())
                    return Json(new
                    {
                        html = RenderPartialViewToString(Views.ExportPartial, model)
                    }, JsonRequestBehavior.DenyGet);
                return RedirectToAction(Views.Export);
            }
        }
        [LoginFilter]
        [HttpGet]
        public ActionResult Delivered(int id)
        {
            Export.Delivered(UserID, Employee.ID, Employee.BussinessID, id);
            var record = Export.GetExport(UserID, Employee.ID, id);
            if (Request.IsAjaxRequest())
            {
                return Json(new
                {
                    html = RenderPartialViewToString(Views.Products, record)
                }, JsonRequestBehavior.AllowGet);
            }
            var model = new ExportList();
            model.List = Export.History(UserID, Employee.ID, Employee.BussinessID);
            model.Current = record;
            return View(Views.History, model);
        }
        [LoginFilter]
        [HttpGet]
        public ActionResult Exported(int id)
        {
            Export.Exported(UserID, Employee.ID, Employee.BussinessID, id);
            var record = Export.GetExport(UserID, Employee.ID, id);
            if (Request.IsAjaxRequest())
            {
                return Json(new
                {
                    html = RenderPartialViewToString(Views.Products, record)
                }, JsonRequestBehavior.AllowGet);
            }
            var model = new ExportList();
            model.List = Export.History(UserID, Employee.ID, Employee.BussinessID);
            model.Current = record;
            return View(Views.History, model);
        }
        [LoginFilter]
        [HttpGet]
        public ActionResult Delete(int id)
        {
            var result = Export.Remove(UserID, Employee.ID, id, true);
            var model = new ExportList();
            model.List = Export.History(UserID, Employee.ID, Employee.BussinessID);
            model.Current = new ExportRecord();
            model.Result = result;
            model.Message = result ? "Xóa phiếu thành công" : "Xóa phiếu không thành công";
            if (Request.IsAjaxRequest())
                return Json(new
                {
                    html = RenderPartialViewToString(Views.HistoryPartial, model)
                }, JsonRequestBehavior.AllowGet);
            return View(Views.History, model);
        }
        [LoginFilter]
        [HttpGet]
        public ActionResult Update(int id)
        {
            var record = Export.GetExport(UserID, Employee.ID, id);
            var login = Login.Get(UserID);
            var model = new Export(record.EmployeeName,
                WarehouseInfo.FindAuthorized(UserID, Employee.ID, Employee.BussinessID, UserID, DbAction.Export.View),
                record.ToWarehouseID, record.Status, record.WarehouseID, record.Products.Select(p => new ImexItem(p)).ToArray(), record.Note, "", record.Result);
            model.ID = record.ID;
            model.Code = record.Code;
            model.Editable = login.Username == "admin";
            if (Request.IsAjaxRequest())
                return Json(new
                {
                    html = RenderPartialViewToString(Views.ExportPartial, model)
                }, JsonRequestBehavior.AllowGet);
            return View(Views.Export, model);
        }
        [LoginFilter]
        [HttpGet]
        public ActionResult Copy(int id)
        {
            var record = Export.GetExport(UserID, Employee.ID, id);
            record.ID = 0;
            var login = Login.Get(UserID);
            var model = new Export(record.EmployeeName,
                WarehouseInfo.FindAuthorized(UserID, Employee.ID, Employee.BussinessID, UserID, DbAction.Export.View),
                record.ToWarehouseID, record.Status, record.WarehouseID, record.Products.Select(p => new ImexItem(p)).ToArray(), record.Note, "", record.Result);
            model.ID = record.ID;
            model.Editable = login.Username == "admin";
            if (Request.IsAjaxRequest())
                return Json(new
                {
                    html = RenderPartialViewToString(Views.ExportPartial, model)
                }, JsonRequestBehavior.AllowGet);
            return View(Views.Export, model);
        }
        [LoginFilter]
        [HttpGet]
        public ActionResult Detail(int id)
        {
            var record = Export.GetExport(UserID, Employee.ID, id, true);
            foreach (var product in record.Products)
            {
                product.ProductUrl = RenderPartialViewToString("ProductUrl", product);
            }
            if (Request.IsAjaxRequest())
            {
                return Json(new
                {
                    html = RenderPartialViewToString(Views.Products, record)
                }, JsonRequestBehavior.AllowGet);
            }
            var model = new ExportList();
            model.List = Export.History(UserID, Employee.ID, Employee.BussinessID);
            model.Current = record;
            return View(Views.History, model);
        }
        [LoginFilter]
        [HttpGet]
        public ActionResult History()
        {
            var model = new ExportList();
            model.List = Export.History(UserID, Employee.ID, Employee.BussinessID);
            model.Current = new ExportRecord();
            if (Request.IsAjaxRequest())
                return Json(new
                {
                    html = RenderPartialViewToString(Views.HistoryPartial, model)
                }, JsonRequestBehavior.AllowGet);
            return View(Views.History, model);
        }
        [LoginFilter]
        [HttpPost]
        public ActionResult History(ImexFilter filter)
        {
            var model = new ExportList(filter);
            model.List = Export.History(UserID, Employee.ID, Employee.BussinessID, filter, true);
            model.Current = new ExportRecord();
            if (Request.IsAjaxRequest())
                return Json(new
                {
                    result = true,
                    html = RenderPartialViewToString(Views.HistoryPartial, model)
                }, JsonRequestBehavior.AllowGet);
            return View(Views.History, model);
        }
    }
}