using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace SKtimeManagement
{
    public class SupplierController : BaseController
    {
        [LoginFilter(NonAuthorized = true)]
        [HttpGet]
        public ActionResult Find(string id)
        {
            var result = SupplierInfo.Find(UserID, Employee.ID, Employee.BussinessID, "", new SupplierFilter() { Name = id });
            return Json(new
            {
                list = result.Data.Select(i => new {
                    ID = i.ID,
                    Name = i.Name
                }).ToArray()
            }, JsonRequestBehavior.AllowGet);
        }
        [LoginFilter]
        [HttpGet]
        public ActionResult List()
        {
            var data = SupplierInfo.Find(UserID, Employee.ID, Employee.BussinessID);
            if (Request.IsAjaxRequest())
                return Json(new
                {
                    html = RenderPartialViewToString(Views.ListPartial, data)
                }, JsonRequestBehavior.AllowGet);
            return View(Views.List, data);
        }
        [LoginFilter]
        [HttpPost]
        public ActionResult List(SupplierFilter filter)
        {
            var data = SupplierInfo.Find(UserID, Employee.ID, Employee.BussinessID, "", filter, true);
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
        public ActionResult KeyList()
        {
            var list = SupplierInfo.Find(UserID, Employee.ID, Employee.BussinessID);
            return Json(new
            {
                id = list.Data.Select(l => l.ID).ToArray(),
                name = list.Data.Select(l => l.Name).ToArray()
            }, JsonRequestBehavior.DenyGet);
        }
        [LoginFilter]
        [HttpGet]
        public ActionResult Create()
        {
            if (Request.IsAjaxRequest())
                return Json(new
                {
                    html = RenderPartialViewToString(Views.SavePartial, new SupplierInfo(Employee.BussinessID))
                }, JsonRequestBehavior.AllowGet);
            return View(Views.Save, new SupplierInfo(Employee.BussinessID));
        }
        [LoginFilter]
        [HttpGet]
        public ActionResult Update(int id)
        {
            var model = SupplierInfo.Get(UserID, Employee.ID, id);
            if (Request.IsAjaxRequest())
                return Json(new
                {
                    html = RenderPartialViewToString(Views.SavePartial, model)
                }, JsonRequestBehavior.AllowGet);
            return View(Views.Save, model);
        }
        [LoginFilter]
        [HttpPost]
        public ActionResult Save(SupplierInfo info)
        {
            var result = info.Save(ModelState, UserID, Employee.ID, Employee.BussinessID);
            var isAjaxRequest = Request.IsAjaxRequest();
            string view; object model;
            if (result)
            {
                view = isAjaxRequest ? Views.ListPartial : Views.List;
                model = SupplierInfo.Find(UserID, Employee.ID, Employee.BussinessID, "Luu thông tin thành công");
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
            SupplierInfo.Remove(UserID, Employee.ID, id);
            var model = SupplierInfo.Find(UserID, Employee.ID, Employee.BussinessID, "Xóa thông tin thành công");
            if (Request.IsAjaxRequest())
                return Json(new
                {
                    html = RenderPartialViewToString(Views.ListPartial, model)
                }, JsonRequestBehavior.AllowGet);
            return View(Views.List, model);
        }
    }
}