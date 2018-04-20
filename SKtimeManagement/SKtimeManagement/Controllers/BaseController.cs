using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using System.Web.Script.Serialization;
using System.Xml;

namespace SKtimeManagement
{
    public class BaseController : Controller
    {
        public RouteValueDictionary RequestRoute
        {
            get
            {
                if (Session[SessionKey.Route] == null)
                    return null;
                else
                    return (RouteValueDictionary)Session[SessionKey.Route];
            }
        }
        public T SessionValue<T>(string key)
        {
            if (Session[key] == null)
                return default(T);
            else
                return (T)Session[key];
        }
        public Login AccountInfo
        {
            get
            {
                if (Session[SessionKey.AccountInfo] == null)
                    return null;
                else
                    return (Login)Session[SessionKey.AccountInfo];
            }
        }
        public EmployeeInfo Employee
        {
            get
            {
                if (Session[SessionKey.Employee] == null)
                    return null;
                else
                    return (EmployeeInfo)Session[SessionKey.Employee];
            }
        }
        public int UserID
        {
            get
            {
                if (Session[SessionKey.Account] == null)
                    return -1;
                else
                    return Int32.Parse(Session[SessionKey.Account].ToString());
            }
        }
        public bool AccessGranted
        {
            get
            {
                return Session[SessionKey.Account] != null;
            }
        }
        protected override void Initialize(System.Web.Routing.RequestContext requestContext)
        {
            base.Initialize(requestContext);
            Session.Timeout = 30;
        }
        public string ErrorXML
        {
            get
            {
                var errorFields = ModelState.Where(f => !ModelState.IsValidField(f.Key));
                var xDoc = new XmlDocument();
                var root = xDoc.CreateElement("error");
                foreach (var errorField in errorFields)
                {
                    var error = xDoc.CreateElement("field");
                    error.SetAttribute("name", errorField.Key);
                    error.SetAttribute("message", String.Join(";", errorField.Value.Errors.Select(e => e.ErrorMessage)));
                    root.AppendChild(error);
                    errorField.Value.Errors.Clear();
                }
                xDoc.AppendChild(root);
                return xDoc.InnerXml;
            }
        }
        [LoginFilter(NonAuthorized = true)]
        [HttpGet]
        public ActionResult Download()
        {
            var fileName = SessionValue<string>(SessionKey.Download);
            if (!String.IsNullOrEmpty(fileName))
            {
                Session.Remove(SessionKey.Download);
                var file = String.Format("{0}/Content/Download/{1}", SiteConfiguration.ApplicationPath, fileName);
                byte[] fileBytes = System.IO.File.ReadAllBytes(file);
                return File(fileBytes, System.Net.Mime.MediaTypeNames.Application.Octet, fileName);
            }
            return RedirectToAction("Index", "Main");
        }
        public new ContentResult Json(object data, JsonRequestBehavior behavior)
        {
            JavaScriptSerializer serializer = new JavaScriptSerializer();
            serializer.MaxJsonLength = Int32.MaxValue; // Whatever max length you want here
            ContentResult result = new ContentResult();
            result.Content = serializer.Serialize(data);
            result.ContentType = "application/json";
            return result;
        }
        #region Render PartialViewToString
        protected string RenderPartialViewToString()
        {
            return RenderPartialViewToString(null, null);
        }
        protected string RenderPartialViewToString(string viewName)
        {
            return RenderPartialViewToString(viewName, null);
        }
        protected string RenderPartialViewToString(object model)
        {
            return RenderPartialViewToString(null, model);
        }
        public string RenderPartialViewToString(string viewName, object model)
        {
            if (string.IsNullOrEmpty(viewName))
                viewName = ControllerContext.RouteData.GetRequiredString("action");

            ViewData.Model = model;

            using (StringWriter sw = new StringWriter())
            {
                ViewEngineResult viewResult = ViewEngines.Engines.FindPartialView(ControllerContext, viewName);
                ViewContext viewContext = new ViewContext(ControllerContext, viewResult.View, ViewData, TempData, sw);
                viewResult.View.Render(viewContext, sw);

                return sw.GetStringBuilder().ToString();
            }
        }
        #endregion
    }
    public class LoginFilterAttribute : ActionFilterAttribute
    {
        public bool NonAuthorized { get; set; }
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            var loginID = (int?)filterContext.HttpContext.Session[SessionKey.Account];
            var action = filterContext.ActionDescriptor.ActionName;
            var controller = filterContext.ActionDescriptor.ControllerDescriptor.ControllerName;
            if (!loginID.HasValue)
            {
                filterContext.HttpContext.Session[SessionKey.Route] = filterContext.RouteData.Values;
                filterContext.Result = new RedirectToRouteResult(new RouteValueDictionary(new
                {
                    action = "Login",
                    controller = "Main",
                    reason = "",
                    area = ""
                }));
            }
            else if (controller != "Main" && !NonAuthorized && !Login.Authorize(loginID.Value, DbAction.GetPermission(controller, action)))
            {
                filterContext.Result = new RedirectToRouteResult(new RouteValueDictionary(new
                {
                    action = "Unauthorized",
                    controller = "Main",
                    reason = "",
                    area = ""
                }));
            }
        }
    }
}