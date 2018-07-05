using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace SKtimeManagement
{
    public class SettingController : BaseController
    {
        #region Bussiness
        [LoginFilter]
        [HttpGet]
        public ActionResult Bussiness()
        {
            var info = BussinessInfo.Find(UserID, Employee.ID);
            if (Request.IsAjaxRequest())
                return Json(new {
                    html = RenderPartialViewToString(Views.BussinessPartial, info)
                }, JsonRequestBehavior.AllowGet);
            return View(Views.Bussiness, info);
        }
        [LoginFilter]
        [HttpPost]
        public ActionResult BussinessUpate(BussinessInfo info)
        {
            var result = info.Save(ModelState, UserID, Employee.ID);
            if (Request.IsAjaxRequest())
            {
                return Json(new
                {
                    result = result,
                    html = RenderPartialViewToString(Views.BussinessPartial, info),
                },
                JsonRequestBehavior.DenyGet);
            }
            else
            {
                return View(Views.Bussiness, info);
            }
        }
        #endregion
        #region Store
        [LoginFilter(NonAuthorized = true)]
        [HttpGet]
        public ActionResult FindStore(string id)
        {
            var stores = StoreInfo.Find(UserID, Employee.ID, Employee.BussinessID, "", new StoreFilter() { Name = id });
            return Json(new
            {
                list = stores.Data.Select(s => new {
                    ID = s.ID,
                    Name = s.Name
                }).ToArray()
            }, JsonRequestBehavior.AllowGet);
        }
        [LoginFilter]
        [HttpGet]
        public ActionResult Store()
        {
            var stores = StoreInfo.Find(UserID, Employee.ID, Employee.BussinessID);
            if (Request.IsAjaxRequest())
                return Json(new
                {
                    html = RenderPartialViewToString(Views.StorePartial, stores)
                }, JsonRequestBehavior.AllowGet);
            return View(Views.Store, stores);
        }
        [LoginFilter]
        [HttpPost]
        public ActionResult Store(StoreFilter filter)
        {
            var stores = StoreInfo.Find(UserID, Employee.ID, Employee.BussinessID, "", filter, true);
            if (Request.IsAjaxRequest())
                return Json(new
                {
                    result = true,
                    html = RenderPartialViewToString(Views.StorePartial, stores)
                }, JsonRequestBehavior.AllowGet);
            return View(Views.Store, stores);
        }
        [LoginFilter(NonAuthorized = true)]
        [HttpPost]
        public ActionResult StoreList()
        {
            var list = StoreInfo.Find(UserID, Employee.ID, Employee.BussinessID);
            return Json(new {
                id = list.Data.Select(d => d.ID).ToArray(),
                name = list.Data.Select(d => d.Name).ToArray()
            }, JsonRequestBehavior.DenyGet);
        }
        [LoginFilter]
        [HttpGet]
        public ActionResult StoreCreate()
        {
            var model = new StoreInfo();
            if (Request.IsAjaxRequest())
                return Json(new
                {
                    html = RenderPartialViewToString(Views.StoreSavePartial, model)
                }, JsonRequestBehavior.AllowGet);
            return View(Views.StoreSave, model);
        }
        [LoginFilter]
        [HttpGet]
        public ActionResult StoreUpdate(int id)
        {
            var model = StoreInfo.Get(UserID, Employee.ID, id);
            if (Request.IsAjaxRequest())
                return Json(new
                {
                    html = RenderPartialViewToString(Views.StoreSavePartial, model)
                }, JsonRequestBehavior.AllowGet);
            return View(Views.StoreSave, model);
        }
        [LoginFilter]
        [HttpPost]
        public ActionResult StoreSave(StoreInfo store)
        {
            var result = store.Save(ModelState, UserID, Employee.ID, Employee.BussinessID);
            var isAjaxRequest = Request.IsAjaxRequest();
            string view; object model;
            if (result)
            {
                view = isAjaxRequest ? Views.StorePartial : Views.Store;
                model = StoreInfo.Find(UserID, Employee.ID, Employee.BussinessID, "Luu thông tin thành công");
            }
            else
            {
                view = isAjaxRequest ? Views.StoreSavePartial : Views.StoreSave;
                model = store;
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
        public ActionResult StoreRemove(int id)
        {
            StoreInfo.Remove(UserID, Employee.ID, id);
            var model = StoreInfo.Find(UserID, Employee.ID, Employee.BussinessID, "Xóa thông tin thành công");
            if (Request.IsAjaxRequest())
                return Json(new
                {
                    html = RenderPartialViewToString(Views.StorePartial, model)
                }, JsonRequestBehavior.AllowGet);
            return View(Views.Store, model);
        }
        #endregion
    }
}