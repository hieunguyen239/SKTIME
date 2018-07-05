using NPOI.HSSF.UserModel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace SKtimeManagement
{
    public class ImportController : BaseController
    {
        [LoginFilter]
        [HttpPost]
        public ActionResult ProductList(ProductFilter filter)
        {
            var data = ImexItem.FindAll(UserID, Employee.ID, filter.Action, Employee.BussinessID, filter);
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
            var model = new Import(Employee.Name,
                WarehouseInfo.FindAuthorized(UserID, Employee.ID, Employee.BussinessID, UserID, DbAction.Import.View));
            if (Request.IsAjaxRequest())
                return Json(new
                {
                    html = RenderPartialViewToString(Views.ImportPartial, model)
                }, JsonRequestBehavior.AllowGet);
            return View(Views.Import, model);
        }
        [LoginFilter]
        [HttpGet]
        public ActionResult CreateFromExport(int id)
        {
            var record = Export.GetExport(UserID, Employee.ID, id);
            var model = new Import(Employee.Name,
                WarehouseInfo.FindAuthorized(UserID, Employee.ID, Employee.BussinessID, UserID, DbAction.Import.View), selected: record.Products.Select(i => new ImexItem(i, record.WarehouseID, record.WarehouseName)).ToArray());
            if (Request.IsAjaxRequest())
                return Json(new
                {
                    html = RenderPartialViewToString(Views.ImportPartial, model)
                }, JsonRequestBehavior.AllowGet);
            return View(Views.Import, model);
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
                    FileManagement.SaveFile("Import/Import", fileName, stream, out fileInfo);
                    list = ImexItem.Read(UserID, Employee.ID, fileInfo, Employee.BussinessID);
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
            string fileName = "ImportTemplate.xls";
            var file = String.Format("{0}/Content/Template/{1}", SiteConfiguration.ApplicationPath, fileName);
            UpdateTemplate(file);
            byte[] fileBytes = System.IO.File.ReadAllBytes(file);
            return File(fileBytes, System.Net.Mime.MediaTypeNames.Application.Octet, fileName);
        }
        private void UpdateTemplate(string fileName)
        {
            var warehouses = WarehouseInfo.FindAuthorized(UserID, Employee.ID, Employee.BussinessID, UserID, DbAction.Import.View);
            if (warehouses.Count > 0)
            {
                HSSFWorkbook hssfwb;
                using (FileStream file = new FileStream(fileName, FileMode.Open, FileAccess.Read))
                {
                    hssfwb = new HSSFWorkbook(file);
                }

                var sheet = hssfwb.GetSheetAt(0);
                var rowCount = sheet.LastRowNum;
                for (var i = 1; i <= rowCount; i++)
                {
                    var row = sheet.GetRow(i);
                    if (row.Cells.FirstOrDefault(c => c.ColumnIndex == 10) != null)
                        row.Cells.FirstOrDefault(c => c.ColumnIndex == 10).SetCellValue("");
                }
                for (var i = 0; i < warehouses.Count; i++)
                {
                    if (i + 1 > rowCount)
                        sheet.CreateRow(i + 1);
                    var row = sheet.GetRow(i + 1);
                    if (row.Cells.FirstOrDefault(c => c.ColumnIndex == 10) == null)
                        row.CreateCell(10);
                    row.Cells.FirstOrDefault(c => c.ColumnIndex == 10).SetCellValue(warehouses[i].Name);
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
            var product = ProductInfo.Get(UserID, Employee.ID, id);
            if (Request.IsAjaxRequest())
                return Json(new
                {
                    result = true,
                    html = RenderPartialViewToString(Views.List, new ImexItem(product, subID))
                }, JsonRequestBehavior.AllowGet);
            return RedirectToAction("Create");
        }
        [LoginFilter]
        [HttpPost]
        public ActionResult Remove(int id, int whid)
        {
            var list = SessionValue<List<ImexItem>>(SessionKey.ImportList);
            if (list == null)
                list = new List<ImexItem>();
            list.Remove(list.FirstOrDefault(i => i.ID == id && i.WarehouseID == whid));
            Session[SessionKey.ImportList] = list;
            return Json(new
            {
                data = ImexItem.ToJson(list)
            }, JsonRequestBehavior.DenyGet);
        }
        [LoginFilter]
        [HttpPost]
        public ActionResult Submit(int id, ImexItem[] items, string note, string receipt)
        {
            var action = id > 0 ? DbAction.Import.Modify : DbAction.Import.Create;
            var message = "";
            int? recordID = null;
            int warehouseID = 0;
            if (id > 0 && !EmployeeInfo.IsAdmin(UserID, Employee.ID, Employee.ID, action))
            {
                message = "Bạn không có quyền sử dụng chức năng này";
            }
            else
            {
                message = "Lưu thông tin không thành công";
                if (items != null && items.Length > 0)
                {
                    var warehouses = items.GroupBy(i => i.WarehouseID);
                    foreach (var warehouse in warehouses)
                    {
                        recordID = SKtimeManagement.Import.Submit(id, UserID, Employee.ID, Employee.BussinessID, warehouse.Key, warehouse.ToArray(), note, receipt);
                        warehouseID = warehouse.Key;
                    }
                    if (recordID.HasValue)
                    {
                        message = "Lưu thông tin thành công";
                    }
                }
                else
                    message = "Không có sản phẩm được chọn";
            }
            if (!recordID.HasValue)
            {
                var model = new Import(Employee.Name,
                    WarehouseInfo.FindAuthorized(UserID, Employee.ID, Employee.BussinessID, UserID, DbAction.Import.View),
                    warehouseID, items, note, message)
                {
                    Receipt = receipt
                };
                model.ID = id;
                if (Request.IsAjaxRequest())
                    return Json(new
                    {
                        html = RenderPartialViewToString(Views.ImportPartial, model)
                    }, JsonRequestBehavior.DenyGet);
                return View(Views.Import, model);
            }
            else
            {
                if (Request.IsAjaxRequest())
                    return Json(new
                    {
                        result = true,
                        redirect = Url.Action("Detail", "Import", new { id = recordID.Value })
                    }, JsonRequestBehavior.DenyGet);
                return RedirectToAction(Views.Detail, new { id = recordID.Value });
            }
        }
        [LoginFilter]
        [HttpGet]
        public ActionResult Delete(int id)
        {
            var model = new ImportList();
            model.Current = new ImportRecord();
            var unremovable = ImexItem.Unremovable(UserID, Employee.ID, DbAction.Import.Remove, id);
            if (unremovable.Count > 0)
                model.Message = "Số lượng sản phẩm tồn kho ít hơn số lượng sản phẩm phiếu xuất";
            else
            {
                model.Result = Import.Remove(UserID, Employee.ID, id, true);
                model.Message = model.Result ? "Xóa phiếu thành công" : "Xóa phiếu không thành công";
            }
            if (!model.Result)
                model.Current = Import.Get(UserID, Employee.ID, id);
            model.List = SKtimeManagement.Import.History(UserID, Employee.ID, Employee.BussinessID);
            if (Request.IsAjaxRequest())
                return Json(new
                {
                    html = RenderPartialViewToString(Views.HistoryPartial, model)
                }, JsonRequestBehavior.AllowGet);
            return View(Views.History, model);
        }
        [LoginFilter]
        [HttpGet]
        public ActionResult Detail(int id)
        {
            var record = Import.Get(UserID, Employee.ID, id, true);
            foreach (var product in record.Products)
            {
                product.Url = RenderPartialViewToString("ProductUrl", product);
            }
            if (Request.IsAjaxRequest())
            {
                return Json(new
                {
                    html = RenderPartialViewToString(Views.Products, record)
                }, JsonRequestBehavior.AllowGet);
            }
            var model = new ImportList();
            model.List = Import.History(UserID, Employee.ID, Employee.BussinessID);
            model.Current = record;
            return View(Views.History, model);
        }
        [LoginFilter]
        [HttpGet]
        public ActionResult Update(int id)
        {
            var record = Import.Get(UserID, Employee.ID, id);
            var model = new Import(record.EmployeeName,
                WarehouseInfo.FindAuthorized(UserID, Employee.ID, Employee.BussinessID, UserID, DbAction.Import.View),
                record.WarehouseID, record.Products.ToArray(), record.Note)
            {
                Receipt = record.Receipt
            };
            model.ID = record.ID;
            model.Code = record.Code;
            if (Request.IsAjaxRequest())
                return Json(new
                {
                    html = RenderPartialViewToString(Views.ImportPartial, model)
                }, JsonRequestBehavior.AllowGet);
            return View(Views.Import, model);
        }
        [LoginFilter]
        [HttpGet]
        public ActionResult Copy(int id)
        {
            var record = Import.Get(UserID, Employee.ID, id);
            record.ID = 0;
            var model = new Import(record.EmployeeName,
                WarehouseInfo.FindAuthorized(UserID, Employee.ID, Employee.BussinessID, UserID, DbAction.Import.View),
                record.WarehouseID, record.Products.ToArray(), record.Note)
            {
                Receipt = record.Receipt
            };
            model.ID = record.ID;
            model.SelectedWarehouse = null;
            if (Request.IsAjaxRequest())
                return Json(new
                {
                    html = RenderPartialViewToString(Views.ImportPartial, model)
                }, JsonRequestBehavior.AllowGet);
            return View(Views.Import, model);
        }
        [LoginFilter]
        [HttpGet]
        public ActionResult History()
        {
            var model = new ImportList();
            model.List = SKtimeManagement.Import.History(UserID, Employee.ID, Employee.BussinessID);
            model.Current = new ImportRecord();
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
            var model = new ImportList(filter);
            model.List = SKtimeManagement.Import.History(UserID, Employee.ID, Employee.BussinessID, filter, true);
            model.Current = new ImportRecord();
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