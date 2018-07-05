using NPOI.HSSF.UserModel;
using NPOI.HSSF.Util;
using NPOI.SS.UserModel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace SKtimeManagement.Controllers
{
    public class RepairController : BaseController
    {
        [LoginFilter]
        [HttpGet]
        public ActionResult Create()
        {
            var model = new RepairModel();
            if (Request.IsAjaxRequest())
                return Json(new
                {
                    html = RenderPartialViewToString(Views.SavePartial, model)
                }, JsonRequestBehavior.AllowGet);
            return View(Views.Save, model);
        }
        [LoginFilter]
        [HttpPost]
        public ActionResult SearchProduct(ProductFilter filter, int recordID)
        {
            filter.ForRepair = true;
            var list = ImexItem.Find(UserID, Employee.ID, filter.Action, Employee.BussinessID, filter);
            var model = new ImexItemList(recordID, filter.WarehouseID.Value, list);
            return Json(new
            {
                result = true,
                html = RenderPartialViewToString("ProductList", model)
            }, JsonRequestBehavior.AllowGet);
        }
        [LoginFilter]
        [HttpPost]
        public ActionResult ProductList(ProductFilter filter)
        {
            var list = ProductInfo.FindList(UserID, Employee.ID, Employee.BussinessID, filter, false, 100, DbAction.Warranty.Create);
            if (Request.IsAjaxRequest())
                return Json(new
                {
                    result = true,
                    html = RenderPartialViewToString("Products", list)
                }, JsonRequestBehavior.DenyGet);
            var model = new WarrantyModel(list);
            return View(Views.Save, model);
        }
        [LoginFilter]
        [HttpGet]
        public ActionResult GetProducts(int id)
        {
            var model = Repair.GetProducts(UserID, Employee.ID, id);
            return Json(new
            {
                result = true,
                html = RenderPartialViewToString("RepairProduct", model)
            }, JsonRequestBehavior.AllowGet);
        }
        [LoginFilter]
        [HttpGet]
        public ActionResult AddProduct(int id, int subID)
        {
            var available = ImexItem.Available(id, subID, TransactionClass.Repair);
            if (available > 0)
                Repair.AddProduct(UserID, Employee.ID, Employee.BussinessID, id, subID);
            var model = Repair.GetProducts(UserID, Employee.ID, subID);
            return Json(new
            {
                result = true,
                html = RenderPartialViewToString("RepairProduct", model)
            }, JsonRequestBehavior.AllowGet);
        }
        [LoginFilter]
        [HttpGet]
        public ActionResult RemoveProduct(int id, int subID)
        {
            Repair.RemoveProduct(UserID, Employee.ID, id);
            var model = Repair.GetProducts(UserID, Employee.ID, subID);
            return Json(new
            {
                result = true,
                html = RenderPartialViewToString("RepairProduct", model)
            }, JsonRequestBehavior.AllowGet);
        }
        [LoginFilter]
        [HttpGet]
        public ActionResult SelectProduct(int id)
        {
            var model = new RepairModel();
            model.SelectedProduct = ProductInfo.Get(UserID, Employee.ID, id, false, DbAction.Repair.Create);
            if (Request.IsAjaxRequest())
                return Json(new
                {
                    html = RenderPartialViewToString("Client", model)
                }, JsonRequestBehavior.AllowGet);
            return View(Views.Save, model);
        }
        [LoginFilter]
        [HttpPost]
        public ActionResult SelectProduct(Repair record)
        {
            var model = new RepairModel();
            if (record.ProductID.HasValue)
                model.SelectedProduct = ProductInfo.Get(UserID, Employee.ID, record.ProductID.Value, false, DbAction.Repair.Create);
            model.Record = record;
            var login = Login.Get(UserID);
            model.Editable = login.Type == LoginType.Office || login.Username == "admin";
            if (Request.IsAjaxRequest())
                return Json(new
                {
                    html = RenderPartialViewToString("SelectedProduct", model)
                }, JsonRequestBehavior.AllowGet);
            return View(Views.Save, model);
        }
        [LoginFilter]
        [HttpPost]
        public ActionResult Submit(Repair record)
        {
            var result = false;
            var list = new RepairList();
            var model = new RepairModel();
            var action = record.ID > 0 ? DbAction.Repair.Modify : DbAction.Repair.Create;
            var message = "";
            if (!record.ProductID.HasValue && String.IsNullOrEmpty(record.ProductName))
                message = "Thiếu thông tin sản phẩm";
            else if (record.WarehouseID == 0)
                message = "Chưa chọn kho";
            else
            {
                var warehouses = WarehouseInfo.FindAuthorized(UserID, Employee.ID, Employee.BussinessID, UserID, action);
                record.ReceiveWarehouseID = warehouses.FirstOrDefault().ID;
                model.Record = record.Save(ModelState, UserID, Employee.ID, Employee.BussinessID, Employee.Name);
                if (result = model.Record != null)
                    message = "Lưu thông tin thành công";
            }
            if (result)
            {
                if (record.ProductID.HasValue)
                    model.SelectedProduct = ProductInfo.Get(UserID, Employee.ID, record.ProductID.Value, false, action);
                var login = Login.Get(UserID);
                model.ViewInternalNote = login.Type == LoginType.Mechanic || login.Type == LoginType.Office || login.Username == "admin";
            }
            else
                model.Record = record;
            model.Record.Messages = new List<string>() { message };
            list.Current = model;
            if (Request.IsAjaxRequest())
                return Json(new
                {
                    result = result,
                    message = message,
                    html = result ? RenderPartialViewToString(Views.HistoryPartial, list) : null
                }, JsonRequestBehavior.DenyGet);
            return RedirectToAction("History");
        }
        [LoginFilter]
        [HttpPost]
        public ActionResult TransferDate(Repair record)
        {
            var result = false;
            var message = "Lưu thông tin không thành công";
            RepairModel model = null;
            if (!record.TransferDate.HasValue)
            {
                message = "Chưa chọn thời gian";
            }
            else if (AccountInfo.Type != LoginType.Sale && AccountInfo.Username != "admin")
            {
                message = "Bạn chưa có quyền sử dụng chức năng này";
            }
            else if (result = record.SaveTransferDate(UserID, Employee.ID))
            {
                message = "Lưu thông tin thành công";
                model = Repair.Get(UserID, Employee.ID, record.ID);
                var login = Login.Get(UserID);
                model.ViewInternalNote = login.Type == LoginType.Mechanic || login.Type == LoginType.Office || login.Username == "admin";
            }
            if (Request.IsAjaxRequest())
                return Json(new
                {
                    result = result,
                    message = message,
                    html = result ? RenderPartialViewToString(Views.Detail, model) : null
                }, JsonRequestBehavior.DenyGet);
            var list = new RepairList();
            list.Current = model;
            return View(Views.History, list);
        }
        [LoginFilter]
        [HttpPost]
        public ActionResult ReceivedDate(Repair record)
        {
            var result = false;
            var message = "Lưu thông tin không thành công";
            RepairModel model = null;
            if (!record.ReceivedDate.HasValue)
            {
                message = "Chưa chọn thời gian";
            }
            else if (AccountInfo.Type != LoginType.Office && AccountInfo.Username != "admin")
            {
                message = "Bạn chưa có quyền sử dụng chức năng này";
            }
            else if (result = record.SaveReceivedDate(UserID, Employee.ID))
            {
                message = "Lưu thông tin thành công";
                model = Repair.Get(UserID, Employee.ID, record.ID);
                var login = Login.Get(UserID);
                model.ViewInternalNote = login.Type == LoginType.Mechanic || login.Type == LoginType.Office || login.Username == "admin";
            }
            if (Request.IsAjaxRequest())
                return Json(new
                {
                    result = result,
                    message = message,
                    html = result ? RenderPartialViewToString(Views.Detail, model) : null
                }, JsonRequestBehavior.DenyGet);
            var list = new RepairList();
            list.Current = model;
            return View(Views.History, list);
        }
        [LoginFilter]
        [HttpPost]
        public ActionResult ProcessedDate(Repair record)
        {
            var result = false;
            var message = "Lưu thông tin không thành công";
            RepairModel model = null;
            if (!record.ProcessedDate.HasValue)
            {
                message = "Chưa chọn thời gian";
            }
            else if (AccountInfo.Type != LoginType.Office && AccountInfo.Username != "admin")
            {
                message = "Bạn chưa có quyền sử dụng chức năng này";
            }
            else if (result = record.SaveProcessedDate(UserID, Employee.ID))
            {
                message = "Lưu thông tin thành công";
                model = Repair.Get(UserID, Employee.ID, record.ID);
                var login = Login.Get(UserID);
                model.ViewInternalNote = login.Type == LoginType.Mechanic || login.Type == LoginType.Office || login.Username == "admin";
            }
            if (Request.IsAjaxRequest())
                return Json(new
                {
                    result = result,
                    message = message,
                    html = result ? RenderPartialViewToString(Views.Detail, model) : null
                }, JsonRequestBehavior.DenyGet);
            var list = new RepairList();
            list.Current = model;
            return View(Views.History, list);
        }
        [LoginFilter]
        [HttpPost]
        public ActionResult ReturnedDate(Repair record)
        {
            var result = false;
            var message = "Lưu thông tin không thành công";
            RepairModel model = null;
            if (!record.ReturnedDate.HasValue)
            {
                message = "Chưa chọn thời gian";
            }
            else if (AccountInfo.Type != LoginType.Sale && AccountInfo.Username != "admin")
            {
                message = "Bạn chưa có quyền sử dụng chức năng này";
            }
            else if (result = record.SaveReturnedDate(UserID, Employee.ID))
            {
                message = "Lưu thông tin thành công";
                model = Repair.Get(UserID, Employee.ID, record.ID);
                var login = Login.Get(UserID);
                model.ViewInternalNote = login.Type == LoginType.Mechanic || login.Type == LoginType.Office || login.Username == "admin";
            }
            if (Request.IsAjaxRequest())
                return Json(new
                {
                    result = result,
                    message = message,
                    html = result ? RenderPartialViewToString(Views.Detail, model) : null
                }, JsonRequestBehavior.DenyGet);
            var list = new RepairList();
            list.Current = model;
            return View(Views.History, list);
        }
        [LoginFilter]
        [HttpPost]
        public ActionResult FinishDate(Repair record)
        {
            var result = false;
            var message = "Lưu thông tin không thành công";
            RepairModel model = null;
            if (AccountInfo.Type != LoginType.Sale && AccountInfo.Username != "admin")
            {
                message = "Bạn chưa có quyền sử dụng chức năng này";
            }
            else if (result = record.SaveFinishDate(UserID, Employee.ID))
            {
                message = "Lưu thông tin thành công";
                model = Repair.Get(UserID, Employee.ID, record.ID);
                var login = Login.Get(UserID);
                model.ViewInternalNote = login.Type == LoginType.Mechanic || login.Type == LoginType.Office || login.Username == "admin";
            }
            if (Request.IsAjaxRequest())
                return Json(new
                {
                    result = result,
                    message = message,
                    html = result ? RenderPartialViewToString(Views.Detail, model) : null
                }, JsonRequestBehavior.DenyGet);
            var list = new RepairList();
            list.Current = model;
            return View(Views.History, list);
        }
        [LoginFilter]
        [HttpGet]
        public ActionResult Detail(int id)
        {
            var model = Repair.Get(UserID, Employee.ID, id);
            var login = Login.Get(UserID);
            model.ViewInternalNote = login.Type == LoginType.Mechanic || login.Type == LoginType.Office || login.Username == "admin";
            if (Request.IsAjaxRequest())
                return Json(new
                {
                    html = RenderPartialViewToString(Views.Detail, model)
                }, JsonRequestBehavior.AllowGet);
            var list = new RepairList();
            list.Current = model;
            return View(Views.History, list);
        }
        [LoginFilter]
        [HttpGet]
        public ActionResult Update(int id)
        {
            var model = Repair.Get(UserID, Employee.ID, id);
            model.Edit = true;
            var login = Login.Get(UserID);
            model.Editable = login.Type == LoginType.Office || login.Username == "admin";
            if (Request.IsAjaxRequest())
                return Json(new
                {
                    html = RenderPartialViewToString("Edit", model)
                }, JsonRequestBehavior.AllowGet);
            var list = new RepairList();
            list.Current = model;
            return View(Views.History, list);
        }
        [LoginFilter]
        [HttpGet]
        public ActionResult Remove(int id)
        {
            Repair.Remove(UserID, Employee.ID, id);
            var list = Repair.Find(UserID, Employee.ID, Employee.BussinessID);
            if (Request.IsAjaxRequest())
                return Json(new
                {
                    result = true,
                    html = RenderPartialViewToString(Views.HistoryPartial, list)
                }, JsonRequestBehavior.AllowGet);
            return View(Views.History, list);
        }
        [LoginFilter]
        [HttpGet]
        public ActionResult History()
        {
            var list = new RepairList();
            if (Request.IsAjaxRequest())
                return Json(new
                {
                    html = RenderPartialViewToString(Views.HistoryPartial, list)
                }, JsonRequestBehavior.AllowGet);
            return View(Views.History, list);
        }
        [LoginFilter]
        [HttpPost]
        public ActionResult History(RepairFilter filter)
        {
            var list = Repair.Find(UserID, Employee.ID, Employee.BussinessID, filter, true);
            if (Request.IsAjaxRequest())
                return Json(new
                {
                    result = true,
                    html = RenderPartialViewToString(Views.HistoryPartial, list)
                }, JsonRequestBehavior.DenyGet);
            return View(Views.History, list);
        }
        [LoginFilter]
        [HttpGet]
        public ActionResult RepairHistory(int id, int subID)
        {
            var list = Repair.GetHistory(UserID, id, subID);
            return Json(new
            {
                html = RenderPartialViewToString("RepairHistory", list)
            }, JsonRequestBehavior.AllowGet);
        }
        [LoginFilter]
        [HttpGet]
        public ActionResult GetMechanicNote(int id, string subID)
        {
            var notes = Repair.GetMechanicNotes(UserID, Employee.ID, id);
            notes.ID = subID;
            notes.RemoveUrl = "RemoveMechanicNote";
            return Json(new
            {
                html = RenderPartialViewToString("RepairNotes", notes)
            }, JsonRequestBehavior.AllowGet);
        }
        [LoginFilter]
        [HttpPost]
        public ActionResult SaveMechanicNote(RepairNote note, string subID)
        {
            var notes = Repair.SaveMechanicNote(UserID, Employee.ID, note);
            notes.ID = subID;
            notes.RemoveUrl = "RemoveMechanicNote";
            return Json(new
            {
                html = RenderPartialViewToString("RepairNotes", notes)
            }, JsonRequestBehavior.DenyGet);
        }
        [LoginFilter]
        [HttpGet]
        public ActionResult RemoveMechanicNote(int id, string subID)
        {
            var notes = Repair.RemoveMechanicNote(UserID, Employee.ID, id);
            notes.ID = subID;
            notes.RemoveUrl = "RemoveMechanicNote";
            return Json(new
            {
                html = RenderPartialViewToString("RepairNotes", notes)
            }, JsonRequestBehavior.AllowGet);
        }
        [LoginFilter]
        [HttpGet]
        public ActionResult GetInternalNote(int id, string subID)
        {
            var notes = Repair.GetInternalNotes(UserID, Employee.ID, id);
            notes.ID = subID;
            notes.RemoveUrl = "RemoveInternalNote";
            return Json(new
            {
                html = RenderPartialViewToString("RepairNotes", notes)
            }, JsonRequestBehavior.AllowGet);
        }
        [LoginFilter]
        [HttpPost]
        public ActionResult SaveInternalNote(RepairNote note, string subID)
        {
            var notes = Repair.SaveInternalNote(UserID, Employee.ID, note);
            notes.ID = subID;
            notes.RemoveUrl = "RemoveInternalNote";
            return Json(new
            {
                html = RenderPartialViewToString("RepairNotes", notes)
            }, JsonRequestBehavior.DenyGet);
        }
        [LoginFilter]
        [HttpGet]
        public ActionResult RemoveInternalNote(int id, string subID)
        {
            var notes = Repair.RemoveInternalNote(UserID, Employee.ID, id);
            notes.ID = subID;
            notes.RemoveUrl = "RemoveInternalNote";
            return Json(new
            {
                html = RenderPartialViewToString("RepairNotes", notes)
            }, JsonRequestBehavior.AllowGet);
        }
        [LoginFilter]
        [HttpPost]
        public ActionResult Download(RepairFilter filter)
        {
            var result = false;
            try
            {
                var data = Repair.Find(UserID, Employee.ID, Employee.BussinessID, filter, true, null);
                var fileName = String.Format("Repair_{0}.xls", DateTime.Now.ToString("ddMMyyyyHHmmss"));
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
        private void SaveDownload(string fileName, RepairList list)
        {
            var workbook = new HSSFWorkbook();
            var worksheet = workbook.CreateSheet("Report");
            ExcelWorker.CellStyles = new List<ICellStyle>();
            ExcelWorker.CreateRow(worksheet, 0, new ExcelCell[] {
                ExcelWorker.CreateCell(workbook, "Kho", HSSFColor.RoyalBlue.Index, HSSFColor.White.Index),
                ExcelWorker.CreateCell(workbook, "Mã phiếu", HSSFColor.RoyalBlue.Index, HSSFColor.White.Index),
                ExcelWorker.CreateCell(workbook, "Nhân viên", HSSFColor.RoyalBlue.Index, HSSFColor.White.Index),
                ExcelWorker.CreateCell(workbook, "Mã khách hàng", HSSFColor.RoyalBlue.Index, HSSFColor.White.Index),
                ExcelWorker.CreateCell(workbook, "Khách hàng", HSSFColor.RoyalBlue.Index, HSSFColor.White.Index),
                ExcelWorker.CreateCell(workbook, "Ngày tạo", HSSFColor.RoyalBlue.Index, HSSFColor.White.Index),
                ExcelWorker.CreateCell(workbook, "Ngày chuyển đến NVBH", HSSFColor.RoyalBlue.Index, HSSFColor.White.Index),
                ExcelWorker.CreateCell(workbook, "Ngày NVBH nhận", HSSFColor.RoyalBlue.Index, HSSFColor.White.Index),
                ExcelWorker.CreateCell(workbook, "Ngày chuyển đến CH", HSSFColor.RoyalBlue.Index, HSSFColor.White.Index),
                ExcelWorker.CreateCell(workbook, "Ngày CH nhận", HSSFColor.RoyalBlue.Index, HSSFColor.White.Index),
                ExcelWorker.CreateCell(workbook, "Ngày giao hàng", HSSFColor.RoyalBlue.Index, HSSFColor.White.Index),
                ExcelWorker.CreateCell(workbook, "Phí", HSSFColor.RoyalBlue.Index, HSSFColor.White.Index),
                ExcelWorker.CreateCell(workbook, "Khuyến mãi", HSSFColor.RoyalBlue.Index, HSSFColor.White.Index),
                ExcelWorker.CreateCell(workbook, "Ghi chú", HSSFColor.RoyalBlue.Index, HSSFColor.White.Index),
                ExcelWorker.CreateCell(workbook, "Khác", HSSFColor.RoyalBlue.Index, HSSFColor.White.Index)
            });
            for (var i = 0; i < list.Data.Count; i++)
            {
                var record = list.Data[i];
                ExcelWorker.CreateRow(worksheet, i + 1, new ExcelCell[] {
                    ExcelWorker.CreateCell(workbook, record.WarehouseName),
                    ExcelWorker.CreateCell(workbook, record.Code),
                    ExcelWorker.CreateCell(workbook, record.EmployeeName),
                    ExcelWorker.CreateCell(workbook, record.ClientCode),
                    ExcelWorker.CreateCell(workbook, record.ClientName),
                    ExcelWorker.CreateCell(workbook, record.SubmitDate.ToString(Constants.DateTimeString)),
                    ExcelWorker.CreateCell(workbook, record.TransferDate.HasValue ? record.TransferDate.Value.ToString(Constants.DateString) : ""),
                    ExcelWorker.CreateCell(workbook, record.ReceivedDate.HasValue ? record.ReceivedDate.Value.ToString(Constants.DateString) : ""),
                    ExcelWorker.CreateCell(workbook, record.ProcessedDate.HasValue ? record.ProcessedDate.Value.ToString(Constants.DateString) : ""),
                    ExcelWorker.CreateCell(workbook, record.ReturnedDate.HasValue ? record.ReturnedDate.Value.ToString(Constants.DateString) : ""),
                    ExcelWorker.CreateCell(workbook, record.FinishDate.ToString(Constants.DateString)),
                    ExcelWorker.CreateCell(workbook, record.FeeString),
                    ExcelWorker.CreateCell(workbook, record.DiscountString),
                    ExcelWorker.CreateCell(workbook, record.Note),
                    ExcelWorker.CreateCell(workbook, record.Other)
                });
            }
            ExcelWorker.CreateRow(worksheet, list.Data.Count + 1, new ExcelCell[] {
                    ExcelWorker.CreateCell(workbook, ""),
                    ExcelWorker.CreateCell(workbook, ""),
                    ExcelWorker.CreateCell(workbook, ""),
                    ExcelWorker.CreateCell(workbook, ""),
                    ExcelWorker.CreateCell(workbook, ""),
                    ExcelWorker.CreateCell(workbook, ""),
                    ExcelWorker.CreateCell(workbook, ""),
                    ExcelWorker.CreateCell(workbook, ""),
                    ExcelWorker.CreateCell(workbook, ""),
                    ExcelWorker.CreateCell(workbook, ""),
                    ExcelWorker.CreateCell(workbook, "Tổng cộng"),
                    ExcelWorker.CreateCell(workbook, list.Data.Sum(i => i.Fee).GetCurrencyString()),
                    ExcelWorker.CreateCell(workbook, list.Data.Sum(i => i.Discount).GetCurrencyString()),
                    ExcelWorker.CreateCell(workbook, ""),
                    ExcelWorker.CreateCell(workbook, "")
                });
            using (var fs = new FileStream(fileName, FileMode.OpenOrCreate, FileAccess.Write))
            {
                workbook.Write(fs);
            }
        }
        [LoginFilter]
        [HttpGet]
        public ActionResult GetTransactions(int id)
        {
            var transactions = Transaction.Get(UserID, Employee.ID, Employee.Name, TransactionClass.Repair, id, DbAction.Repair.View);
            var login = SessionValue<Login>(SessionKey.AccountInfo);
            transactions.Editable = login.Username == "admin" || login.Type == LoginType.Office;
            return Json(new
            {
                html = RenderPartialViewToString("TransactionList", transactions)
            }, JsonRequestBehavior.AllowGet);
        }
        [LoginFilter]
        [HttpPost]
        public ActionResult AddTransaction(Transaction tran)
        {
            var transactions = tran.Save(UserID, Employee.ID, Employee.Name, TransactionClass.Repair, DbAction.Repair.View);
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
            var transactions = Transaction.Remove(UserID, Employee.ID, Employee.Name, id, subID, TransactionClass.Repair, DbAction.Repair.View);
            var login = SessionValue<Login>(SessionKey.AccountInfo);
            transactions.Editable = login.Username == "admin" || login.Type == LoginType.Office;
            return Json(new
            {
                html = RenderPartialViewToString("TransactionList", transactions)
            }, JsonRequestBehavior.AllowGet);
        }
    }
}