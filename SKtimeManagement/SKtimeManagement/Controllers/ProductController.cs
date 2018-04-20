using NPOI.HSSF.UserModel;
using NPOI.HSSF.Util;
using NPOI.SS.UserModel;
using NPOI.SS.Util;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace SKtimeManagement
{
    public class ProductController : BaseController
    {
        private void RefreshSessionValue()
        {
            Session.Remove(SessionKey.Tag);
            Session.Remove(SessionKey.Quantity);
            Session.Remove(SessionKey.Import);
        }
        #region Product
        [LoginFilter]
        [HttpGet]
        public ActionResult Find()
        {
            var data = new FindList();
            data.Warehouses = WarehouseInfo.FindAuthorized(UserID, Employee.ID, Employee.BussinessID, UserID, DbAction.Product.View);
            if (Request.IsAjaxRequest())
                return Json(new
                {
                    html = RenderPartialViewToString(Views.FindPartial, data)
                }, JsonRequestBehavior.AllowGet);
            return View(Views.Find, data);
        }
        [LoginFilter]
        [HttpPost]
        public ActionResult Find(ProductFilter filter)
        {
            var data = ProductInfo.FindAll(UserID, Employee.ID, Employee.BussinessID, "", filter);
            ProductInfo product = null;
            var exist = false;
            if (data.Data.Count == 1)
            {
                product = data.Data.FirstOrDefault();
                exist = true;
            }
            else if (data.Data.Count == 0 && !String.IsNullOrEmpty(filter.Code))
            {
                product = ProductInfo.FindList(UserID, Employee.BussinessID, Employee.BussinessID, filter, false, 100, DbAction.Product.View).FirstOrDefault();
                if (product == null)
                    product = new ProductInfo()
                    {
                        Code = filter.Code
                    };
            }
            return Json(new
            {
                html = RenderPartialViewToString(Views.Selector, new FindItem(product, 0, exist))
            }, JsonRequestBehavior.AllowGet);
        }
        [LoginFilter]
        [HttpPost]
        public ActionResult FindFinish(int warehouseID, int? tagID, FindItem[] items)
        {
            var filter = new ProductFilter()
            {
                WarehouseID = warehouseID,
                TagID = tagID
            };
            var products = ProductInfo.FindAll(UserID, Employee.ID, Employee.BussinessID, "", filter, true, null);
            var model = new FindList(filter);
            model.Warehouses = WarehouseInfo.FindAuthorized(UserID, Employee.ID, Employee.BussinessID, UserID, DbAction.Product.View);
            var warehouse = model.Warehouses.FirstOrDefault(i => i.ID == warehouseID);
            if (products.Data.Count > 0 && warehouse != null)
            {
                if (items != null)
                {
                    foreach (var item in items)
                    {
                        var product = products.Data.FirstOrDefault(i => i.Code == item.Code);
                        if (product == null)
                        {
                            product = ProductInfo.FindList(UserID, Employee.BussinessID, Employee.BussinessID, new ProductFilter() { Code =  item.Code }, false, 100, DbAction.Product.View).FirstOrDefault();
                            model.Data.Add(new FindItem(item.Code, warehouse.Name, item.Quantity, product));
                        }
                        else
                            model.Data.Add(new FindItem(product, item.Quantity));
                    }
                }
                model.Data.AddRange(products.Data.Where(i => model.Data.FirstOrDefault(d => d.Code == i.Code) == null).Select(i => new FindItem(i)));
            }
            if (Request.IsAjaxRequest())
                return Json(new
                {
                    html = RenderPartialViewToString(Views.FindPartial, model)
                }, JsonRequestBehavior.AllowGet);
            return View(Views.Find, model);
        }
        [LoginFilter]
        [HttpPost]
        public ActionResult FindDownload(int warehouseID, int? tagID, FindItem[] items)
        {
            var result = false;
            try
            {
                var filter = new ProductFilter()
                {
                    WarehouseID = warehouseID,
                    TagID = tagID
                };
                var products = ProductInfo.FindAll(UserID, Employee.ID, Employee.BussinessID, "", filter, true, null);
                var model = new FindList(filter);
                model.Warehouses = WarehouseInfo.FindAuthorized(UserID, Employee.ID, Employee.BussinessID, UserID, DbAction.Product.View);
                var warehouse = model.Warehouses.FirstOrDefault(i => i.ID == warehouseID);
                if (products.Data.Count > 0 && warehouse != null)
                {
                    if (items != null)
                    {
                        foreach (var item in items)
                        {
                            var product = products.Data.FirstOrDefault(i => i.Code == item.Code);
                            if (product == null)
                            {
                                product = ProductInfo.FindList(UserID, Employee.BussinessID, Employee.BussinessID, new ProductFilter() { Code = item.Code }, false, 100, DbAction.Product.View).FirstOrDefault();
                                model.Data.Add(new FindItem(item.Code, warehouse.Name, item.Quantity, product));
                            }
                            else
                                model.Data.Add(new FindItem(product, item.Quantity));
                        }
                    }
                    model.Data.AddRange(products.Data.Where(i => model.Data.FirstOrDefault(d => d.Code == i.Code) == null).Select(i => new FindItem(i)));
                }
                var fileName = String.Format("Products_{0}.xls", DateTime.Now.ToString("ddMMyyyyHHmmss"));
                var file = String.Format("{0}/Content/Download/{1}", SiteConfiguration.ApplicationPath, fileName);
                Functions.CheckDirectory(String.Format("{0}/Content/Download/", SiteConfiguration.ApplicationPath));
                SaveFind(file, model);
                Session[SessionKey.Download] = fileName;
                result = true;
            }
            catch { }
            return Json(new
            {
                result = result
            }, JsonRequestBehavior.DenyGet);
        }
        private void SaveFind(string fileName, FindList list)
        {
            var products = list.Data;
            var warehouse = list.Warehouses.FirstOrDefault(i => i.ID == list.Filter.WarehouseID);
            var workbook = new HSSFWorkbook();
            var worksheet = workbook.CreateSheet("Report");
            var index = 0;
            ExcelWorker.CreateRow(worksheet, index++, new ExcelCell[] {
                ExcelWorker.CreateCell(workbook, "Kho"),
                ExcelWorker.CreateCell(workbook, warehouse.Name),
                ExcelWorker.CreateCell(workbook, ""),
                ExcelWorker.CreateCell(workbook, "Tổng tồn"),
                ExcelWorker.CreateCell(workbook, list.Data.Where(i => i.Exist).Sum(i => i.Max).GetCurrencyString()),
                ExcelWorker.CreateCell(workbook, ""),
                ExcelWorker.CreateCell(workbook, "Tổng tìm"),
                ExcelWorker.CreateCell(workbook, list.Data.Sum(i => i.Quantity).GetCurrencyString())
            });
            ExcelWorker.CreateRow(worksheet, index++, new ExcelCell[] {
                ExcelWorker.CreateCell(workbook, "Đủ hàng"),
                ExcelWorker.CreateCell(workbook, list.Data.Count(i => i.Exist && i.Quantity == i.Max).GetCurrencyString()),
                ExcelWorker.CreateCell(workbook, ""),
                ExcelWorker.CreateCell(workbook, "Thiếu hàng"),
                ExcelWorker.CreateCell(workbook, list.Data.Count(i => i.Exist && i.Quantity < i.Max).GetCurrencyString()),
                ExcelWorker.CreateCell(workbook, ""),
                ExcelWorker.CreateCell(workbook, "Dư hàng"),
                ExcelWorker.CreateCell(workbook, list.Data.Count(i => i.Exist && i.Quantity > i.Max).GetCurrencyString()),
                ExcelWorker.CreateCell(workbook, ""),
                ExcelWorker.CreateCell(workbook, "Không tồn tại"),
                ExcelWorker.CreateCell(workbook, list.Data.Count(i => !i.Exist).GetCurrencyString())
            });
            index++;
            ExcelWorker.CellStyles = new List<ICellStyle>();
            ExcelWorker.CreateRow(worksheet, index++, new ExcelCell[] {
                ExcelWorker.CreateCell(workbook, "Thông tin", HSSFColor.RoyalBlue.Index, HSSFColor.White.Index),
                ExcelWorker.CreateCell(workbook, "Nhóm", HSSFColor.RoyalBlue.Index, HSSFColor.White.Index),
                ExcelWorker.CreateCell(workbook, "Mã", HSSFColor.RoyalBlue.Index, HSSFColor.White.Index),
                ExcelWorker.CreateCell(workbook, "Tên", HSSFColor.RoyalBlue.Index, HSSFColor.White.Index),
                ExcelWorker.CreateCell(workbook, "Kho", HSSFColor.RoyalBlue.Index, HSSFColor.White.Index),
                ExcelWorker.CreateCell(workbook, "Đơn vị", HSSFColor.RoyalBlue.Index, HSSFColor.White.Index),
                ExcelWorker.CreateCell(workbook, "Tồn kho", HSSFColor.RoyalBlue.Index, HSSFColor.White.Index),
                ExcelWorker.CreateCell(workbook, "Số lượng tìm", HSSFColor.RoyalBlue.Index, HSSFColor.White.Index)
            });
            for (var i = 0; i < products.Count; i++)
            {
                var product = products[i];
                var message = "Đủ hàng";
                if (!product.Exist)
                {
                    message = "Không tồn tại";
                }
                else if (product.Quantity > product.Max)
                {
                    message = String.Format("Dư hàng({0})", product.Max - product.Quantity);
                }
                else if (product.Quantity < product.Max)
                {
                    message = String.Format("Thiếu hàng({0})", product.Quantity - product.Max);
                }
                ExcelWorker.CreateRow(worksheet, index++, new ExcelCell[] {
                    ExcelWorker.CreateCell(workbook, message),
                    ExcelWorker.CreateCell(workbook, product.TagName),
                    ExcelWorker.CreateCell(workbook, product.Code),
                    ExcelWorker.CreateCell(workbook, product.Name),
                    ExcelWorker.CreateCell(workbook, product.WarehouseName),
                    ExcelWorker.CreateCell(workbook, product.Unit),
                    ExcelWorker.CreateCell(workbook, product.Max.GetCurrencyString()),
                    ExcelWorker.CreateCell(workbook, product.Quantity.GetCurrencyString())
                });
            }
            using (var fs = new FileStream(fileName, FileMode.OpenOrCreate, FileAccess.Write))
            {
                workbook.Write(fs);
            }
        }
        [LoginFilter]
        [HttpGet]
        public ActionResult Check()
        {
            var data = new ProductList();
            data.Warehouses = WarehouseInfo.FindAuthorized(UserID, Employee.ID, Employee.BussinessID, UserID, DbAction.Product.View);
            if (Request.IsAjaxRequest())
                return Json(new
                {
                    html = RenderPartialViewToString(Views.CheckPartial, data)
                }, JsonRequestBehavior.AllowGet);
            return View(Views.Check, data);
        }
        [LoginFilter]
        [HttpPost]
        public ActionResult Check(ProductFilter filter)
        {
            var data = ProductInfo.Check(UserID, Employee.ID, Employee.BussinessID, filter, null);
            data.Warehouses = WarehouseInfo.FindAuthorized(UserID, Employee.ID, Employee.BussinessID, UserID, DbAction.Product.View);
            if (Request.IsAjaxRequest())
                return Json(new
                {
                    html = RenderPartialViewToString(Views.CheckPartial, data)
                }, JsonRequestBehavior.AllowGet);
            return View(Views.Check, data);
        }
        [LoginFilter]
        [HttpPost]
        public ActionResult DownloadCheck(ProductFilter filter)
        {
            var result = false;
            try
            {
                var data = ProductInfo.Check(UserID, Employee.ID, Employee.BussinessID, filter, null);
                if (data != null)
                {
                    var fileName = String.Format("Products_{0}.xls", DateTime.Now.ToString("ddMMyyyyHHmmss"));
                    var file = String.Format("{0}/Content/Download/{1}", SiteConfiguration.ApplicationPath, fileName);
                    Functions.CheckDirectory(String.Format("{0}/Content/Download/", SiteConfiguration.ApplicationPath));
                    SaveCheck(file, data.Data);
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
        private void SaveCheck(string fileName, List<ProductInfo> products)
        {
            var workbook = new HSSFWorkbook();
            var worksheet = workbook.CreateSheet("Report");
            ExcelWorker.CellStyles = new List<ICellStyle>();
            ExcelWorker.CreateRow(worksheet, 0, new ExcelCell[] {
                ExcelWorker.CreateCell(workbook, "Nhóm", HSSFColor.RoyalBlue.Index, HSSFColor.White.Index),
                ExcelWorker.CreateCell(workbook, "Mã", HSSFColor.RoyalBlue.Index, HSSFColor.White.Index),
                ExcelWorker.CreateCell(workbook, "Tên", HSSFColor.RoyalBlue.Index, HSSFColor.White.Index),
                ExcelWorker.CreateCell(workbook, "Đơn vị", HSSFColor.RoyalBlue.Index, HSSFColor.White.Index),
                ExcelWorker.CreateCell(workbook, "Giá bán lẻ", HSSFColor.RoyalBlue.Index, HSSFColor.White.Index),
                ExcelWorker.CreateCell(workbook, "Kho", HSSFColor.RoyalBlue.Index, HSSFColor.White.Index),
                ExcelWorker.CreateCell(workbook, "Tồn đầu kỳ", HSSFColor.RoyalBlue.Index, HSSFColor.White.Index),
                ExcelWorker.CreateCell(workbook, "Nhập", HSSFColor.RoyalBlue.Index, HSSFColor.White.Index),
                ExcelWorker.CreateCell(workbook, "Xuất", HSSFColor.RoyalBlue.Index, HSSFColor.White.Index),
                ExcelWorker.CreateCell(workbook, "Tồn cuối kỳ", HSSFColor.RoyalBlue.Index, HSSFColor.White.Index),
                ExcelWorker.CreateCell(workbook, "Số lượng bán", HSSFColor.RoyalBlue.Index, HSSFColor.White.Index),
                ExcelWorker.CreateCell(workbook, "Số lượng trả", HSSFColor.RoyalBlue.Index, HSSFColor.White.Index)
            });
            for (var i = 0; i < products.Count; i++)
            {
                var product = products[i];
                ExcelWorker.CreateRow(worksheet, i + 1, new ExcelCell[] {
                    ExcelWorker.CreateCell(workbook, product.TagName),
                    ExcelWorker.CreateCell(workbook, product.Code),
                    ExcelWorker.CreateCell(workbook, product.Name),
                    ExcelWorker.CreateCell(workbook, product.Unit),
                    ExcelWorker.CreateCell(workbook, product.PriceString),
                    ExcelWorker.CreateCell(workbook, product.WarehouseName),
                    ExcelWorker.CreateCell(workbook, product.StartQuantityString),
                    ExcelWorker.CreateCell(workbook, product.ImportQuantityString),
                    ExcelWorker.CreateCell(workbook, product.ExportQuantityString),
                    ExcelWorker.CreateCell(workbook, product.EndQuantityString),
                    ExcelWorker.CreateCell(workbook, product.SaleQuantityString),
                    ExcelWorker.CreateCell(workbook, product.ReturnedString)
                });
            }
            ExcelWorker.CreateRow(worksheet, products.Count + 1, new ExcelCell[] {
                    ExcelWorker.CreateCell(workbook, ""),
                    ExcelWorker.CreateCell(workbook, ""),
                    ExcelWorker.CreateCell(workbook, ""),
                    ExcelWorker.CreateCell(workbook, ""),
                    ExcelWorker.CreateCell(workbook, ""),
                    ExcelWorker.CreateCell(workbook, "Tổng cộng"),
                    ExcelWorker.CreateCell(workbook, products.Sum(i => i.StartQuantity).GetCurrencyString()),
                    ExcelWorker.CreateCell(workbook, products.Sum(i => i.ImportQuantity).GetCurrencyString()),
                    ExcelWorker.CreateCell(workbook, products.Sum(i => i.ExportQuantity).GetCurrencyString()),
                    ExcelWorker.CreateCell(workbook, products.Sum(i => i.EndQuantity).GetCurrencyString()),
                    ExcelWorker.CreateCell(workbook, products.Sum(i => i.SaleQuantity).GetCurrencyString()),
                    ExcelWorker.CreateCell(workbook, products.Sum(i => i.Returned).GetCurrencyString())
                });
            using (var fs = new FileStream(fileName, FileMode.OpenOrCreate, FileAccess.Write))
            {
                workbook.Write(fs);
            }
        }
        [LoginFilter]
        [HttpPost]
        public ActionResult DownloadAll(ProductFilter filter)
        {
            var result = false;
            try
            {
                var data = ProductInfo.Find(UserID, Employee.ID, Employee.BussinessID, "", filter, true, null);
                if (data != null)
                {
                    var fileName = String.Format("Products_{0}.xls", DateTime.Now.ToString("ddMMyyyyHHmmss"));
                    var file = String.Format("{0}/Content/Download/{1}", SiteConfiguration.ApplicationPath, fileName);
                    Functions.CheckDirectory(String.Format("{0}/Content/Download/", SiteConfiguration.ApplicationPath));
                    SaveAll(file, data.Data);
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
        private void SaveAll(string fileName, List<ProductInfo> products)
        {
            var workbook = new HSSFWorkbook();
            var worksheet = workbook.CreateSheet("Report");
            ExcelWorker.CellStyles = new List<ICellStyle>();
            ExcelWorker.CreateRow(worksheet, 0, new ExcelCell[] {
                ExcelWorker.CreateCell(workbook, "Nhóm", HSSFColor.RoyalBlue.Index, HSSFColor.White.Index),
                ExcelWorker.CreateCell(workbook, "Mã", HSSFColor.RoyalBlue.Index, HSSFColor.White.Index),
                ExcelWorker.CreateCell(workbook, "Tên", HSSFColor.RoyalBlue.Index, HSSFColor.White.Index),
                ExcelWorker.CreateCell(workbook, "Đơn vị", HSSFColor.RoyalBlue.Index, HSSFColor.White.Index),
                ExcelWorker.CreateCell(workbook, "Giá bán lẻ", HSSFColor.RoyalBlue.Index, HSSFColor.White.Index),
                ExcelWorker.CreateCell(workbook, "Xuất xứ", HSSFColor.RoyalBlue.Index, HSSFColor.White.Index),
                ExcelWorker.CreateCell(workbook, "Dòng sản phẩn", HSSFColor.RoyalBlue.Index, HSSFColor.White.Index),
                ExcelWorker.CreateCell(workbook, "Loại máy", HSSFColor.RoyalBlue.Index, HSSFColor.White.Index),
                ExcelWorker.CreateCell(workbook, "Giới tính", HSSFColor.RoyalBlue.Index, HSSFColor.White.Index),
                ExcelWorker.CreateCell(workbook, "Loại kính", HSSFColor.RoyalBlue.Index, HSSFColor.White.Index),
                ExcelWorker.CreateCell(workbook, "Chất liệu dây", HSSFColor.RoyalBlue.Index, HSSFColor.White.Index),
                ExcelWorker.CreateCell(workbook, "Chất liệu vỏ", HSSFColor.RoyalBlue.Index, HSSFColor.White.Index),
                ExcelWorker.CreateCell(workbook, "Kiểu vỏ", HSSFColor.RoyalBlue.Index, HSSFColor.White.Index),
                ExcelWorker.CreateCell(workbook, "Màu mặt số", HSSFColor.RoyalBlue.Index, HSSFColor.White.Index),
                ExcelWorker.CreateCell(workbook, "Bề dày vỏ", HSSFColor.RoyalBlue.Index, HSSFColor.White.Index),
                ExcelWorker.CreateCell(workbook, "Kích thước dây", HSSFColor.RoyalBlue.Index, HSSFColor.White.Index),
                ExcelWorker.CreateCell(workbook, "Đường kính (mm)", HSSFColor.RoyalBlue.Index, HSSFColor.White.Index),
                ExcelWorker.CreateCell(workbook, "Độ chịu nước", HSSFColor.RoyalBlue.Index, HSSFColor.White.Index),
                ExcelWorker.CreateCell(workbook, "Chức năng", HSSFColor.RoyalBlue.Index, HSSFColor.White.Index),
                ExcelWorker.CreateCell(workbook, "Phong cách", HSSFColor.RoyalBlue.Index, HSSFColor.White.Index),
                ExcelWorker.CreateCell(workbook, "Bảo hành chính hãng", HSSFColor.RoyalBlue.Index, HSSFColor.White.Index),
                ExcelWorker.CreateCell(workbook, "Bảo hành SKtime", HSSFColor.RoyalBlue.Index, HSSFColor.White.Index),
                ExcelWorker.CreateCell(workbook, "Nhà cung cấp", HSSFColor.RoyalBlue.Index, HSSFColor.White.Index),
                ExcelWorker.CreateCell(workbook, "Mô tả", HSSFColor.RoyalBlue.Index, HSSFColor.White.Index)
            });
            for (var i = 0; i < products.Count; i++)
            {
                var product = products[i];
                ExcelWorker.CreateRow(worksheet, i + 1, new ExcelCell[] {
                    ExcelWorker.CreateCell(workbook, product.TagName),
                    ExcelWorker.CreateCell(workbook, product.Code),
                    ExcelWorker.CreateCell(workbook, product.Name),
                    ExcelWorker.CreateCell(workbook, product.Unit),
                    ExcelWorker.CreateCell(workbook, product.PriceString),
                    ExcelWorker.CreateCell(workbook, product.MadeIn),
                    ExcelWorker.CreateCell(workbook, product.Type),
                    ExcelWorker.CreateCell(workbook, product.Engine),
                    ExcelWorker.CreateCell(workbook, product.Gender),
                    ExcelWorker.CreateCell(workbook, product.MirrorType),
                    ExcelWorker.CreateCell(workbook, product.TrapMaterial),
                    ExcelWorker.CreateCell(workbook, product.CaseMaterial),
                    ExcelWorker.CreateCell(workbook, product.CaseType),
                    ExcelWorker.CreateCell(workbook, product.FrontColor),
                    ExcelWorker.CreateCell(workbook, product.CaseWidth),
                    ExcelWorker.CreateCell(workbook, product.TrapSize),
                    ExcelWorker.CreateCell(workbook, product.Diameter),
                    ExcelWorker.CreateCell(workbook, product.WaterResistance),
                    ExcelWorker.CreateCell(workbook, product.Functions),
                    ExcelWorker.CreateCell(workbook, product.Style),
                    ExcelWorker.CreateCell(workbook, product.OriginalWarranty),
                    ExcelWorker.CreateCell(workbook, product.BussinessWarranty),
                    ExcelWorker.CreateCell(workbook, product.SupplierName),
                    ExcelWorker.CreateCell(workbook, product.Description)
                });
            }
            using (var fs = new FileStream(fileName, FileMode.OpenOrCreate, FileAccess.Write))
            {
                workbook.Write(fs);
            }
        }
        [LoginFilter(NonAuthorized = true)]
        [HttpGet]
        public ActionResult NewUniqueCode()
        {
            return Json(new
            {
                code = BaseModel.NewUniqueCode(UserID, Employee.ID, Employee.BussinessID, "Product")
            }, JsonRequestBehavior.AllowGet);
        }
        [LoginFilter]
        [HttpGet]
        public ActionResult Detail(int id)
        {
            var data = ProductInfo.Get(UserID, Employee.ID, id, true);
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
            var data = ProductInfo.Find(UserID, Employee.ID, Employee.BussinessID);
            if (Request.IsAjaxRequest())
                return Json(new
                {
                    html = RenderPartialViewToString(Views.ListPartial, data)
                }, JsonRequestBehavior.AllowGet);
            return View(Views.List, data);
        }
        [LoginFilter]
        [HttpPost]
        public ActionResult List(ProductFilter filter)
        {
            var data = ProductInfo.Find(UserID, Employee.ID, Employee.BussinessID, "", filter, true, null);
            if (Request.IsAjaxRequest())
                return Json(new
                {
                    result = true,
                    html = RenderPartialViewToString(Views.ListPartial, data)
                }, JsonRequestBehavior.DenyGet);
            return View(Views.List, data);
        }
        [LoginFilter]
        [HttpGet]
        public ActionResult ListByTag(int id)
        {
            var data = ProductInfo.Find(UserID, Employee.ID, Employee.BussinessID, "", new ProductFilter() { TagID = id }, false, null);
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
        public ActionResult Download(ProductFilter filter)
        {
            var result = false;
            try
            {
                var data = ProductInfo.FindAll(UserID, Employee.ID, Employee.BussinessID, "", filter, true, null);
                if (data != null)
                {
                    var fileName = String.Format("Products_{0}.xls", DateTime.Now.ToString("ddMMyyyyHHmmss"));
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
        private void SaveDownload(string fileName, List<ProductInfo> products)
        {
            var workbook = new HSSFWorkbook();
            var worksheet = workbook.CreateSheet("Report");
            ExcelWorker.CellStyles = new List<ICellStyle>();
            ExcelWorker.CreateRow(worksheet, 0, new ExcelCell[] {
                ExcelWorker.CreateCell(workbook, "Mã", HSSFColor.RoyalBlue.Index, HSSFColor.White.Index),
                ExcelWorker.CreateCell(workbook, "Tên", HSSFColor.RoyalBlue.Index, HSSFColor.White.Index),
                ExcelWorker.CreateCell(workbook, "Kho", HSSFColor.RoyalBlue.Index, HSSFColor.White.Index),
                ExcelWorker.CreateCell(workbook, "Số lượng", HSSFColor.RoyalBlue.Index, HSSFColor.White.Index),
                ExcelWorker.CreateCell(workbook, "Số lượng trả", HSSFColor.RoyalBlue.Index, HSSFColor.White.Index),
                ExcelWorker.CreateCell(workbook, "Xuất xứ", HSSFColor.RoyalBlue.Index, HSSFColor.White.Index),
                ExcelWorker.CreateCell(workbook, "Thời gian bảo hành (chính hãng)", HSSFColor.RoyalBlue.Index, HSSFColor.White.Index),
                ExcelWorker.CreateCell(workbook, "Thời gian bảo hành (đơn vị)", HSSFColor.RoyalBlue.Index, HSSFColor.White.Index),
                ExcelWorker.CreateCell(workbook, "Đơn vị", HSSFColor.RoyalBlue.Index, HSSFColor.White.Index),
                ExcelWorker.CreateCell(workbook, "Giá bán lẻ", HSSFColor.RoyalBlue.Index, HSSFColor.White.Index),
                ExcelWorker.CreateCell(workbook, "Điểm tích lũy", HSSFColor.RoyalBlue.Index, HSSFColor.White.Index),
                ExcelWorker.CreateCell(workbook, "Mô tả", HSSFColor.RoyalBlue.Index, HSSFColor.White.Index)
            });
            for (var i = 0; i < products.Count; i++)
            {
                var product = products[i];
                ExcelWorker.CreateRow(worksheet, i + 1, new ExcelCell[] {
                    ExcelWorker.CreateCell(workbook, product.Code),
                    ExcelWorker.CreateCell(workbook, product.Name),
                    ExcelWorker.CreateCell(workbook, product.WarehouseName),
                    ExcelWorker.CreateCell(workbook, product.Quantity.ToString()),
                    ExcelWorker.CreateCell(workbook, product.Returned.ToString()),
                    ExcelWorker.CreateCell(workbook, product.MadeIn),
                    ExcelWorker.CreateCell(workbook, product.OriginalWarranty),
                    ExcelWorker.CreateCell(workbook, product.BussinessWarranty),
                    ExcelWorker.CreateCell(workbook, product.Unit),
                    ExcelWorker.CreateCell(workbook, product.Price.ToString()),
                    ExcelWorker.CreateCell(workbook, product.Point.ToString()),
                    ExcelWorker.CreateCell(workbook, product.Description)
                });
            }
            using (var fs = new FileStream(fileName, FileMode.OpenOrCreate, FileAccess.Write))
            {
                workbook.Write(fs);
            }
        }
        [LoginFilter]
        [HttpGet]
        public ActionResult Create()
        {
            RefreshSessionValue();
            var code = BaseModel.NewUniqueCode(UserID, Employee.ID, Employee.BussinessID, "Product");
            if (Request.IsAjaxRequest())
                return Json(new
                {
                    html = RenderPartialViewToString(Views.SavePartial, new ProductInfo(Employee.BussinessID, code))
                }, JsonRequestBehavior.AllowGet);
            return View(Views.Save, new ProductInfo(Employee.BussinessID, code));
        }
        [LoginFilter]
        [HttpGet]
        public ActionResult Update(int id)
        {
            RefreshSessionValue();
            var model = ProductInfo.Get(UserID, Employee.ID, id);
            if (Request.IsAjaxRequest())
                return Json(new
                {
                    html = RenderPartialViewToString(Views.SavePartial, model)
                }, JsonRequestBehavior.AllowGet);
            return View(Views.Save, model);
        }
        [LoginFilter]
        [HttpPost]
        public ActionResult Save(ProductInfo info)
        {
            var result = info.Save(ModelState, UserID, Employee.ID, Employee.BussinessID, SessionValue<List<int>>(SessionKey.Tag), SessionValue<List<ProductQuantity>>(SessionKey.Quantity));
            var isAjaxRequest = Request.IsAjaxRequest();
            string view; object model;
            if (result)
            {
                view = isAjaxRequest ? Views.ListPartial : Views.List;
                model = ProductInfo.Find(UserID, Employee.ID, Employee.BussinessID, "Luu thông tin thành công");
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
            var products = ImexItem.Find(UserID, Employee.ID, DbAction.Product.View, Employee.BussinessID, new ProductFilter() { ProductID = id });
            var message = "";
            if (products.FirstOrDefault(i => i.Quantity > 0) != null)
                message = "Sản phẩm vẫn còn tồn";
            else if (ProductInfo.Remove(UserID, Employee.ID, id))
                message = "Xóa thông tin thành công";
            var model = ProductInfo.Find(UserID, Employee.ID, Employee.BussinessID, message);
            if (Request.IsAjaxRequest())
                return Json(new
                {
                    html = RenderPartialViewToString(Views.ListPartial, model)
                }, JsonRequestBehavior.AllowGet);
            return View(Views.List, model);
        }
        #endregion
        #region Tag
        [LoginFilter]
        [HttpGet]
        public ActionResult AddTag(int id, int subID)
        {
            if (subID > 0)
            {
                ProductInfo.AddTag(UserID, Employee.ID, subID, new int[] { id });
            }
            else
            {
                var tagIDs = SessionValue<List<int>>(SessionKey.Tag);
                if (tagIDs == null)
                    tagIDs = new List<int>();
                if (!tagIDs.Contains(id))
                    tagIDs.Add(id);
                Session[SessionKey.Tag] = tagIDs;
            }
            return Json(new
            {
                result = true
            }, JsonRequestBehavior.AllowGet);
        }
        [LoginFilter]
        [HttpGet]
        public ActionResult RemoveTag(int id, int subID)
        {
            if (subID > 0)
            {
                ProductInfo.RemoveTag(UserID, Employee.ID, subID, new int[] { id });
            }
            else
            {
                var tagIDs = SessionValue<List<int>>(SessionKey.Tag);
                if (tagIDs != null)
                    tagIDs.Remove(id);
                Session[SessionKey.Tag] = tagIDs;
            }
            return Json(new
            {
                result = true
            }, JsonRequestBehavior.AllowGet);
        }
        #endregion
        #region Quantity
        [LoginFilter]
        [HttpGet]
        public ActionResult Quantity(int id, int subID, int value)
        {
            return Json(new
            {
                html = RenderPartialViewToString("Quantity", new ProductQuantity(subID, id, value))
            }, JsonRequestBehavior.AllowGet);
        }
        [LoginFilter]
        [HttpGet]
        public ActionResult Quantities(int id)
        {
            return Json(new
            {
                html = RenderPartialViewToString("Quantities", ProductQuantity.Find(UserID, Employee.ID, id))
            }, JsonRequestBehavior.AllowGet);
        }
        [LoginFilter]
        [HttpGet]
        public ActionResult EditQuantity(int id, int subID, int value)
        {
            return Json(new
            {
                html = RenderPartialViewToString("EditQuantity", new ProductQuantity(subID, id, value))
            }, JsonRequestBehavior.AllowGet);
        }
        [LoginFilter]
        [HttpGet]
        public ActionResult SaveQuantity(int id, int subID, int value)
        {
            if (subID > 0)
            {
                ProductQuantity.Save(UserID, Employee.ID, new ProductQuantity[] { new ProductQuantity(subID, id, value) });
            }
            else
            {
                var quantities = SessionValue<List<ProductQuantity>>(SessionKey.Quantity);
                if (quantities == null)
                    quantities = new List<ProductQuantity>();
                quantities.Add(new ProductQuantity(subID, id, value));
                Session[SessionKey.Quantity] = quantities;
            }
            return Json(new
            {
                result = true,
                html = RenderPartialViewToString("Quantity", new ProductQuantity(subID, id, value))
            }, JsonRequestBehavior.AllowGet);
        }
        [LoginFilter]
        [HttpGet]
        public ActionResult RemoveQuantity(int id, int subID)
        {
            if (subID > 0)
            {
                ProductQuantity.Remove(UserID, Employee.ID, new ProductQuantity[] { new ProductQuantity(subID, id) });
            }
            else
            {
                var quantities = SessionValue<List<ProductQuantity>>(SessionKey.Quantity);
                if (quantities != null)
                    quantities.Remove(quantities.FirstOrDefault(q => q.ProductID == subID && q.WarehouseID == id));
                Session[SessionKey.Quantity] = quantities;
            }
            return Json(new
            {
                result = true,
                remove = true
            }, JsonRequestBehavior.AllowGet);
        }
        #endregion
        #region Import
        [LoginFilter]
        [HttpGet]
        public ActionResult Import()
        {
            if (Request.IsAjaxRequest())
                return Json(new
                {
                    html = RenderPartialViewToString(Views.ImportPartial, new ProductList())
                }, JsonRequestBehavior.AllowGet);
            return View(Views.Import, new ProductList());
        }
        [LoginFilter]
        [HttpPost]
        public ActionResult UploadImportData()
        {
            var model = new ProductList();
            foreach (string f in Request.Files)
            {
                var fileContent = Request.Files[f];
                if (fileContent != null && fileContent.ContentLength > 0)
                {
                    // get a stream
                    var stream = fileContent.InputStream;
                    var fileName = String.Format("{1}_{0}", Path.GetFileName(fileContent.FileName), DateTime.Now.ToString("yyyy-MM-dd HH-mm-ss"));
                    FileInfo fileInfo;
                    FileManagement.SaveFile("Import/Product", fileName, stream, out fileInfo);
                    model.Data = ProductInfo.Read(UserID, Employee.ID, fileInfo, Employee.BussinessID);
                    Session[SessionKey.Import] = model;
                }
                break;
            }
            if (Request.IsAjaxRequest())
                return Json(new
                {
                    html = RenderPartialViewToString(Views.ImportPartial, model)
                }, JsonRequestBehavior.AllowGet);
            return View(Views.Import, model);
        }
        [LoginFilter]
        [HttpPost]
        public ActionResult SubmitImportData()
        {
            var list = SessionValue<ProductList>(SessionKey.Import);
            if (list != null && list.Data != null && list.Data.Count > 0)
            {
                ProductInfo.Import(UserID, Employee.BussinessID, Employee.ID, list.Data);
            }
            var model = ProductInfo.Find(UserID, Employee.ID, Employee.BussinessID);
            if (Request.IsAjaxRequest())
                return Json(new
                {
                    result = true,
                    html = RenderPartialViewToString(Views.ListPartial, model)
                }, JsonRequestBehavior.AllowGet);
            return View(Views.List, model);
        }
        [LoginFilter]
        [HttpGet]
        public ActionResult ImportTemplate()
        {
            string fileName = "ProductImport.xls";
            var file = String.Format("{0}/Content/Template/{1}", SiteConfiguration.ApplicationPath, fileName);
            UpdateTemplate(file);
            byte[] fileBytes = System.IO.File.ReadAllBytes(file);
            return File(fileBytes, System.Net.Mime.MediaTypeNames.Application.Octet, fileName);
        }
        private void UpdateTemplate(string fileName)
        {
            var suppliers = SupplierInfo.Find(UserID, Employee.ID, Employee.BussinessID);
            var tags = TagInfo.Find(UserID, Employee.ID, Employee.BussinessID);
            if (suppliers.Data.Count > 0 || tags.Data.Count > 0)
            {
                HSSFWorkbook hssfwb;
                using (FileStream file = new FileStream(fileName, FileMode.Open, FileAccess.Read))
                {
                    hssfwb = new HSSFWorkbook(file);
                }

                var sheet = hssfwb.GetSheetAt(0);
                var rowCount = sheet.LastRowNum;
                var supplierIndex = 30;
                var tagIndex = 31;
                for (var i = 0; i < suppliers.Data.Count; i++)
                {
                    if (i + 1 > rowCount)
                        sheet.CreateRow(i + 1);
                    var row = sheet.GetRow(i + 1);
                    if (row.Cells.FirstOrDefault(c => c.ColumnIndex == supplierIndex) == null)
                        row.CreateCell(supplierIndex);
                    row.Cells.FirstOrDefault(c => c.ColumnIndex == supplierIndex).SetCellValue(suppliers.Data[i].Name);
                }
                rowCount = sheet.LastRowNum;
                for (var i = 0; i < tags.Data.Count; i++)
                {
                    if (i + 1 > rowCount)
                        sheet.CreateRow(i + 1);
                    var row = sheet.GetRow(i + 1);
                    if (row.Cells.FirstOrDefault(c => c.ColumnIndex == tagIndex) == null)
                        row.CreateCell(tagIndex);
                    row.Cells.FirstOrDefault(c => c.ColumnIndex == tagIndex).SetCellValue(tags.Data[i].Name);
                }
                using (var fs = new FileStream(fileName, FileMode.OpenOrCreate, FileAccess.Write))
                {
                    hssfwb.Write(fs);
                }
            }
        }
        #endregion
    }
}