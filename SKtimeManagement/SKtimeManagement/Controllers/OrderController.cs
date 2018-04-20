using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace SKtimeManagement
{
    public class OrderController : BaseController
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
            var model = new Export(
                WarehouseInfo.FindAuthorized(UserID, Employee.ID, Employee.BussinessID, UserID, DbAction.Order.View),
                new OrderRecord() {
                    EmployeeID = Employee.ID,
                    EmployeeName = Employee.Name
                }, login.Username == "admin");
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
                    FileManagement.SaveFile("Import/Order", fileName, stream, out fileInfo);
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
            string fileName = "OrderTemplate.xls";
            byte[] fileBytes = System.IO.File.ReadAllBytes(String.Format("{0}/Content/Template/{1}", SiteConfiguration.ApplicationPath, fileName));
            return File(fileBytes, System.Net.Mime.MediaTypeNames.Application.Octet, fileName);
        }
        [LoginFilter]
        [HttpGet]
        public ActionResult Add(int id, int subID)
        {
            var product = ImexItem.Get(UserID, Employee.ID, DbAction.Order.View, id, subID);
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
                total = list.Sum(i => i.Quantity * i.Price)
            }, JsonRequestBehavior.DenyGet);
        }
        [LoginFilter]
        [HttpPost]
        public ActionResult Submit(OrderRecord record)
        {
            record.EmployeeName = Employee.Name;
            var action = record.ID > 0 ? DbAction.Order.Modify : DbAction.Order.Create;
            var unavailable = ImexItem.Unavailable(UserID, Employee.ID, record.WarehouseID, action, record.Items, record.ID);
            var orderID = 0;
            var message = "";
            var result = false;
            var login = Login.Get(UserID);
            //if (record.ID > 0 && login.Username != "admin" && login.Type != LoginType.Office)
            //    message = "Bạn không có quyền cập nhật hóa đơn";
            if (unavailable.Count > 0)
            {
                message = String.Format("Số lượng vượt mức tồn kho:<br/>{0}", String.Join("<br/>", unavailable.Select(i => i.Name)));
            }
            else if (record.ClientID == 0 && (String.IsNullOrEmpty(record.ClientName) || String.IsNullOrEmpty(record.ClientPhone)))
                message = "Thiếu thông tin khách hàng (tên, số điện thoại)";
            else if (record.Items.Sum(i => i.Quantity * i.Price) - record.Discount - record.Paid != 0)
                message = "Tổng tiền khuyến mãi và đã trả không bằng tổng tiền hóa đơn";
            else
            {
                message = "Lưu thông tin không thành công";
                if (record.Items != null && record.Items.Length > 0)
                {
                    if (login.Username != "admin" && record.ID == 0)
                        record.SubmitDate = DateTime.Now;
                    if (login.Username != "admin")
                        record.NewEmployeeID = null;
                    if ((orderID = SKtimeManagement.Export.SaveOrder(UserID, Employee.ID, Employee.BussinessID, record)) > 0)
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
                model.List = Export.OrderHistory(UserID, Employee.ID, Employee.BussinessID);
                model.Current = Export.GetOrder(UserID, Employee.ID, orderID);
                foreach (var product in model.Current.Products)
                {
                    product.ProductUrl = RenderPartialViewToString("ProductUrl", product);
                }
                foreach (var product in model.Current.ReturnProducts)
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
                var data = ImexItem.Find(UserID, Employee.ID, action, Employee.BussinessID);
                var model = new Export(
                    WarehouseInfo.FindAuthorized(UserID, Employee.ID, Employee.BussinessID, UserID, DbAction.Order.View),
                    record, login.Username == "admin", message, result);
                if (record.Return)
                {
                    var returnRecord = new OrderRecord(Export.GetOrder(UserID, Employee.ID, record.ID), true);
                    var items = model.Selected.ToList();
                    items.AddRange(returnRecord.Items);
                    model.Selected = items.ToArray();
                }
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
        public ActionResult Delete(int id)
        {
            var result = Export.RemoveOrder(UserID, Employee.ID, id, true);
            var model = new ExportList();
            model.List = Export.OrderHistory(UserID, Employee.ID, Employee.BussinessID);
            model.Current = new ExportRecord();
            model.Result = result;
            model.Message = result ? "Xóa hóa đơn thành công" : "Xóa hóa đơn không thành công";
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
            var record = Export.GetOrder(UserID, Employee.ID, id, true);
            foreach (var product in record.Products)
            {
                product.ProductUrl = RenderPartialViewToString("ProductUrl", product);
            }
            foreach (var product in record.ReturnProducts)
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
            model.List = Export.OrderHistory(UserID, Employee.ID, Employee.BussinessID);
            model.Current = record;
            return View(Views.History, model);
        }
        [LoginFilter]
        [HttpGet]
        public ActionResult Update(int id)
        {
            var login = Login.Get(UserID);
            var model = new Export(
                WarehouseInfo.FindAuthorized(UserID, Employee.ID, Employee.BussinessID, UserID, DbAction.Order.View), 
                new OrderRecord(Export.GetOrder(UserID, Employee.ID, id)),
                login.Username == "admin");
            if (Request.IsAjaxRequest())
                return Json(new
                {
                    html = RenderPartialViewToString(Views.ExportPartial, model)
                }, JsonRequestBehavior.AllowGet);
            return View(Views.Export, model);
        }
        [LoginFilter]
        [HttpGet]
        public ActionResult Exchange(int id)
        {
            var login = Login.Get(UserID);
            var record = new OrderRecord(Export.GetOrder(UserID, Employee.ID, id), true);
            var model = new Export(
                WarehouseInfo.FindAuthorized(UserID, Employee.ID, Employee.BussinessID, UserID, DbAction.Order.View),
                record,
                login.Username == "admin")
            { Return = true };
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
            var record = new OrderRecord(Export.GetOrder(UserID, Employee.ID, id));
            record.ID = 0;
            var login = Login.Get(UserID);
            var model = new Export(
                WarehouseInfo.FindAuthorized(UserID, Employee.ID, Employee.BussinessID, UserID, DbAction.Order.View),
                record,
                login.Username == "admin");
            if (Request.IsAjaxRequest())
                return Json(new
                {
                    html = RenderPartialViewToString(Views.ExportPartial, model)
                }, JsonRequestBehavior.AllowGet);
            return View(Views.Export, model);
        }
        //[LoginFilter]
        //[HttpPost]
        //public ActionResult Update(ExportRecord record)
        //{
        //    var result = Export.UpdateOrder(UserID, Employee.ID, record);
        //    record = Export.GetOrder(UserID, Employee.ID, record.ID);
        //    if (Request.IsAjaxRequest())
        //    {
        //        return Json(new
        //        {
        //            result = result,
        //            html = RenderPartialViewToString(Views.Products, record)
        //        }, JsonRequestBehavior.AllowGet);
        //    }
        //    var model = new ExportList();
        //    model.List = Export.History(UserID, Employee.ID, Employee.BussinessID);
        //    model.Current = record;
        //    return View(Views.History, model);
        //}
        [LoginFilter]
        [HttpGet]
        public ActionResult ExportOrder(int id)
        {
            var result = Export.ExportOrder(UserID, Employee.ID, id);
            var record = Export.GetOrder(UserID, Employee.ID, id);
            if (Request.IsAjaxRequest())
            {
                return Json(new
                {
                    result = result,
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
            model.List = Export.OrderHistory(UserID, Employee.ID, Employee.BussinessID);
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
            model.List = Export.OrderHistory(UserID, Employee.ID, Employee.BussinessID, filter, true);
            model.Current = new ExportRecord();
            if (Request.IsAjaxRequest())
                return Json(new
                {
                    result = true,
                    html = RenderPartialViewToString(Views.HistoryPartial, model)
                }, JsonRequestBehavior.AllowGet);
            return View(Views.History, model);
        }
        [LoginFilter]
        [HttpGet]
        public ActionResult GetTransactions(int id)
        {
            var transactions = Transaction.Get(UserID, Employee.ID, Employee.Name, TransactionClass.Order, id, DbAction.Order.View);
            var login = SessionValue<Login>(SessionKey.AccountInfo);
            transactions.Editable = login.Username == "admin" || login.Type == LoginType.Office;
            return Json(new {
                html = RenderPartialViewToString("TransactionList", transactions)
            }, JsonRequestBehavior.AllowGet);
        }
        [LoginFilter]
        [HttpPost]
        public ActionResult AddTransaction(Transaction tran)
        {
            var transactions = tran.Save(UserID, Employee.ID, Employee.Name, TransactionClass.Order, DbAction.Order.View);
            var login = SessionValue<Login>(SessionKey.AccountInfo);
            transactions.Editable = login.Username == "admin" || login.Type == LoginType.Office;
            return Json(new
            {
                result = true,
                html = RenderPartialViewToString("TransactionList", transactions)
            }, JsonRequestBehavior.DenyGet);
        }
        [LoginFilter]
        [HttpGet]
        public ActionResult RemoveTransaction(int id, int subID)
        {
            var transactions = Transaction.Remove(UserID, Employee.ID, Employee.Name, id, subID, TransactionClass.Order, DbAction.Order.View);
            var login = SessionValue<Login>(SessionKey.AccountInfo);
            transactions.Editable = login.Username == "admin" || login.Type == LoginType.Office;
            return Json(new
            {
                html = RenderPartialViewToString("TransactionList", transactions)
            }, JsonRequestBehavior.AllowGet);
        }
    }
}