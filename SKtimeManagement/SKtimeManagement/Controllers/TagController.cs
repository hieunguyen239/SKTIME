using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace SKtimeManagement
{
    public class TagController : BaseController
    {
        [LoginFilter(NonAuthorized = true)]
        [HttpGet]
        public ActionResult Find(string id)
        {
            var result = TagInfo.Find(UserID, Employee.ID, Employee.BussinessID, "", new TagFilter() { Name = id });
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
            var data = TagInfo.Find(UserID, Employee.ID, Employee.BussinessID);
            if (Request.IsAjaxRequest())
                return Json(new
                {
                    html = RenderPartialViewToString(Views.ListPartial, data)
                }, JsonRequestBehavior.AllowGet);
            return View(Views.List, data);
        }
        [LoginFilter]
        [HttpPost]
        public ActionResult List(TagFilter filter)
        {
            var data = TagInfo.Find(UserID, Employee.ID, Employee.BussinessID, "", filter, true);
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
        public ActionResult KeyList(int id)
        {
            var list = TagInfo.KeyList(UserID, Employee.ID, Employee.BussinessID, id);
            return Json(new
            {
                id = list.Select(l => l.ID).ToArray(),
                name = list.Select(l => l.Name).ToArray(),
                tagged = list.Select(l => l.Tagged != null).ToArray()
            }, JsonRequestBehavior.AllowGet);
        }
        [LoginFilter]
        [HttpGet]
        public ActionResult Create()
        {
            if (Request.IsAjaxRequest())
                return Json(new
                {
                    html = RenderPartialViewToString(Views.SavePartial, new TagInfo(Employee.BussinessID))
                }, JsonRequestBehavior.AllowGet);
            return View(Views.Save, new TagInfo(Employee.BussinessID));
        }
        [LoginFilter]
        [HttpGet]
        public ActionResult Update(int id)
        {
            var model = TagInfo.Get(UserID, Employee.ID, id);
            if (Request.IsAjaxRequest())
                return Json(new
                {
                    html = RenderPartialViewToString(Views.SavePartial, model)
                }, JsonRequestBehavior.AllowGet);
            return View(Views.Save, model);
        }
        [LoginFilter]
        [HttpPost]
        public ActionResult Save(TagInfo info)
        {
            var result = info.Save(ModelState, UserID, Employee.ID, Employee.BussinessID);
            var isAjaxRequest = Request.IsAjaxRequest();
            string view; object model;
            if (result)
            {
                view = isAjaxRequest ? Views.ListPartial : Views.List;
                model = TagInfo.Find(UserID, Employee.ID, Employee.BussinessID, "Luu thông tin thành công");
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
            TagInfo.Remove(UserID, Employee.ID, id);
            var model = TagInfo.Find(UserID, Employee.ID, Employee.BussinessID, "Xóa thông tin thành công");
            if (Request.IsAjaxRequest())
                return Json(new
                {
                    html = RenderPartialViewToString(Views.ListPartial, model)
                }, JsonRequestBehavior.AllowGet);
            return View(Views.List, model);
        }
    }
}