using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace SKtimeManagement
{
    public class ErrorController : BaseController
    {
        public ActionResult Index()
        {
            if (Request.IsAjaxRequest())
                return Json(new {
                    result = true,
                    html = RenderPartialViewToString(Views.IndexPartial)
                }, JsonRequestBehavior.AllowGet);
            return View();
        }
        public ActionResult NotFound()
        {
            if (Request.IsAjaxRequest())
                return Json(new
                {
                    result = true,
                    html = RenderPartialViewToString(Views.NotFoundPartial)
                }, JsonRequestBehavior.AllowGet);
            return View();
        }
        public ActionResult AccessDenied()
        {
            if (Request.IsAjaxRequest())
                return Json(new
                {
                    result = true,
                    html = RenderPartialViewToString(Views.AccessDeniedPartial)
                }, JsonRequestBehavior.AllowGet);
            return View();
        }
    }
}