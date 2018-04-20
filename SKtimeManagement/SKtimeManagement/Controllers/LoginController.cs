using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace SKtimeManagement
{
    public class LoginController : BaseController
    {
        [LoginFilter(NonAuthorized = true)]
        [HttpGet]
        public ActionResult Find(string id)
        {
            var result = Login.Find(UserID, Employee.ID, Employee.BussinessID, "", new LoginFilter() { Username = id });
            return Json(new
            {
                list = result.Data.Select(s => new {
                    ID = s.ID,
                    Name = s.Name
                }).ToArray()
            }, JsonRequestBehavior.AllowGet);
        }
        [LoginFilter]
        [HttpGet]
        public ActionResult List()
        {
            var data = Login.Find(UserID, Employee.ID, Employee.BussinessID);
            if (Request.IsAjaxRequest())
                return Json(new
                {
                    html = RenderPartialViewToString(Views.ListPartial, data)
                }, JsonRequestBehavior.AllowGet);
            return View(Views.List, data);
        }
        [LoginFilter]
        [HttpPost]
        public ActionResult List(LoginFilter filter)
        {
            var data = Login.Find(UserID, Employee.ID, Employee.BussinessID, "", filter, true);
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
        public ActionResult UnassignedList()
        {
            var list = Login.GetUnassigned(UserID, Employee.ID, Employee.BussinessID);
            return Json(new
            {
                id = list.Select(l => l.ID).ToArray(),
                name = list.Select(l => l.Username).ToArray()
            }, JsonRequestBehavior.DenyGet);
        }
        [LoginFilter(NonAuthorized = true)]
        [HttpPost]
        public ActionResult DataList()
        {
            var list = Login.Find(UserID, Employee.ID, Employee.BussinessID);
            return Json(new
            {
                id = list.Data.Select(e => e.ID).ToArray(),
                name = list.Data.Select(e => e.Username).ToArray()
            }, JsonRequestBehavior.DenyGet);
        }
        [LoginFilter]
        [HttpGet]
        public ActionResult Permission(int id)
        {
            var list = Login.GetPermissions(UserID, Employee.ID, id);
            return Json(new
            {
                permissions = list.ToArray()
            }, JsonRequestBehavior.AllowGet);
        }
        [LoginFilter]
        [HttpGet]
        public ActionResult Create()
        {
            Session.Remove(SessionKey.Warehouse);
            if (Request.IsAjaxRequest())
                return Json(new
                {
                    html = RenderPartialViewToString(Views.SavePartial, new Login(Employee.BussinessID))
                }, JsonRequestBehavior.AllowGet);
            return View(Views.Save, new Login(Employee.BussinessID));
        }
        [LoginFilter]
        [HttpGet]
        public ActionResult Update(int id)
        {
            Session.Remove(SessionKey.Warehouse);
            var model = Login.Get(UserID, Employee.ID, id);
            if (Request.IsAjaxRequest())
                return Json(new
                {
                    html = RenderPartialViewToString(Views.SavePartial, model)
                }, JsonRequestBehavior.AllowGet);
            return View(Views.Save, model);
        }
        [LoginFilter]
        [HttpPost]
        public ActionResult Save(Login info)
        {
            var result = info.Save(ModelState, UserID, Employee.ID, SessionValue<List<int>>(SessionKey.Warehouse));
            var isAjaxRequest = Request.IsAjaxRequest();
            string view; object model;
            if (result)
            {
                view = isAjaxRequest ? Views.ListPartial : Views.List;
                model = Login.Find(UserID, Employee.ID, Employee.BussinessID, "Luu thông tin thành công");
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
            var message = "Xóa thông tin thành công";
            if (id == 1)
                message = "Không thể xóa tài khoản admin";
            else
                Login.Remove(UserID, Employee.ID, id);
            var model = Login.Find(UserID, Employee.ID, Employee.BussinessID, message);
            if (Request.IsAjaxRequest())
                return Json(new
                {
                    html = RenderPartialViewToString(Views.ListPartial, model)
                }, JsonRequestBehavior.AllowGet);
            return View(Views.List, model);
        }
        [LoginFilter]
        [HttpGet]
        public ActionResult AddWarehouse(int id, int subID)
        {
            if (subID > 0)
            {
                Login.AddWarehouse(UserID, Employee.ID, subID, id);
            }
            else
            {
                var ids = SessionValue<List<int>>(SessionKey.Warehouse);
                if (ids == null)
                    ids = new List<int>();
                if (!ids.Contains(id))
                    ids.Add(id);
                Session[SessionKey.Warehouse] = ids;
            }
            return Json(new
            {
                result = true
            }, JsonRequestBehavior.AllowGet);
        }
        [LoginFilter]
        [HttpGet]
        public ActionResult RemoveWarehouse(int id, int subID)
        {
            if (subID > 0)
            {
                Login.RemoveWarehouse(UserID, Employee.ID, subID, id);
            }
            else
            {
                var ids = SessionValue<List<int>>(SessionKey.Warehouse);
                if (ids == null)
                    ids = new List<int>();
                ids.Remove(id);
                Session[SessionKey.Warehouse] = ids;
            }
            return Json(new
            {
                result = true
            }, JsonRequestBehavior.AllowGet);
        }
        [LoginFilter(NonAuthorized = true)]
        [HttpPost]
        public ActionResult WarehouseList(int id)
        {
            var list = WarehouseInfo.KeyListWithLogin(UserID, Employee.ID, Employee.BussinessID, id);
            return Json(new
            {
                id = list.Select(l => l.ID).ToArray(),
                name = list.Select(l => l.Name).ToArray(),
                tagged = list.Select(l => l.Tagged != null).ToArray()
            }, JsonRequestBehavior.DenyGet);
        }
        [LoginFilter(NonAuthorized = true)]
        [HttpPost]
        public ActionResult AuthorizedWarehouse()
        {
            var list = WarehouseInfo.FindAuthorized(UserID, Employee.ID, Employee.BussinessID, UserID, DbAction.Login.View);
            return Json(new
            {
                id = list.Select(l => l.ID).ToArray(),
                name = list.Select(l => l.Name).ToArray(),
            }, JsonRequestBehavior.DenyGet);
        }
    }
}