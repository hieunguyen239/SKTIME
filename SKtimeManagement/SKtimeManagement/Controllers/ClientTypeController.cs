using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace SKtimeManagement
{
    public class ClientTypeController : BaseController
    {
        [LoginFilter]
        [HttpGet]
        public ActionResult List()
        {
            var data = ClientTypeInfo.Find(UserID, Employee.ID, Employee.BussinessID);
            if (Request.IsAjaxRequest())
                return Json(new
                {
                    html = RenderPartialViewToString(Views.ListPartial, data)
                }, JsonRequestBehavior.AllowGet);
            return View(Views.List, data);
        }
        [LoginFilter]
        [HttpPost]
        public ActionResult List(ClientTypeFilter filter)
        {
            var data = ClientTypeInfo.Find(UserID, Employee.ID, Employee.BussinessID, "" , filter, true);
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
            var list = ClientTypeInfo.KeyList(UserID, Employee.ID, Employee.BussinessID);
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
            if (Request.IsAjaxRequest())
                return Json(new
                {
                    html = RenderPartialViewToString(Views.SavePartial, new ClientTypeInfo(Employee.BussinessID))
                }, JsonRequestBehavior.AllowGet);
            return View(Views.Save, new ClientTypeInfo(Employee.BussinessID));
        }
        [LoginFilter]
        [HttpGet]
        public ActionResult Update(int id)
        {
            var model = ClientTypeInfo.Get(UserID, Employee.ID, id);
            if (Request.IsAjaxRequest())
                return Json(new
                {
                    html = RenderPartialViewToString(Views.SavePartial, model)
                }, JsonRequestBehavior.AllowGet);
            return View(Views.Save, model);
        }
        [LoginFilter]
        [HttpPost]
        public ActionResult Save(ClientTypeInfo info)
        {
            var result = info.Save(ModelState, UserID, Employee.ID);
            var isAjaxRequest = Request.IsAjaxRequest();
            string view; object model;
            if (result)
            {
                view = isAjaxRequest ? Views.ListPartial : Views.List;
                model = ClientTypeInfo.Find(UserID, Employee.ID, Employee.BussinessID, "Luu thông tin thành công");
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
            ClientTypeInfo.Remove(UserID, Employee.ID, id);
            var model = ClientTypeInfo.Find(UserID, Employee.ID, Employee.BussinessID, "Xóa thông tin thành công");
            if (Request.IsAjaxRequest())
                return Json(new
                {
                    html = RenderPartialViewToString(Views.ListPartial, model)
                }, JsonRequestBehavior.AllowGet);
            return View(Views.List, model);
        }
    }
}