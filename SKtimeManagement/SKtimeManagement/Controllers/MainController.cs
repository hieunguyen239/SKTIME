using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace SKtimeManagement
{
    public class MainController : BaseController
    {
        [HttpPost]
        [LoginFilter]
        public ActionResult StayAlive()
        {
            return Json(new {
                result = true
            }, JsonRequestBehavior.DenyGet);
        }
        [LoginFilter]
        public ActionResult Unauthorized()
        {
            if (Request.IsAjaxRequest())
                return Json(new
                {
                    html = RenderPartialViewToString(Views.UnauthorizedPartial)
                }, JsonRequestBehavior.AllowGet);
            return View(Views.Unauthorized);
        }
        [LoginFilter]
        public ActionResult Index()
        {
            var login = SKtimeManagement.Login.Get(UserID);
            object model;
            string view;
            var isAjax = Request.IsAjaxRequest();
            if (login.Name == "admin" || login.Type == LoginType.Office)
            {
                view = isAjax ? Views.SummaryAdminPartial : Views.SummaryAdmin;
                model = SummaryAdmin.Get(Employee.BussinessID);
            }
            else
            {
                view = isAjax ? Views.SummaryPartial : Views.Summary;
                model = Summary.Get(Employee.ID, Employee.BussinessID, UserID);
            }
            if (isAjax)
                return Json(new
                {
                    html = RenderPartialViewToString(view, model)
                }, JsonRequestBehavior.AllowGet);
            return View(view, model);
        }
        [LoginFilter]
        public ActionResult EmployeeSalary()
        {
            var model = SalaryReport.Get(UserID, Employee.ID, Employee.BussinessID, new EmployeeFilter() { ID = Employee.ID });
            if (Request.IsAjaxRequest())
                return Json(new
                {
                    html = RenderPartialViewToString(Views.SalaryPartial, model)
                }, JsonRequestBehavior.AllowGet);
            return View(Views.Salary, model);
        }
        #region User
        [HttpGet]
        public ActionResult Login()
        {
            return View(new Login());
        }
        [HttpPost]
        public ActionResult Login(Login login)
        {
            if (login.IsValid())
            {
                Session[SessionKey.Account] = login.ID;
                Session[SessionKey.AccountInfo] = login;
                Session[SessionKey.Employee] = login.EmployeeInfo;
                SiteConfiguration.BussinessInfo = BussinessInfo.Find(UserID, Employee.ID);
                if (RequestRoute != null)
                {
                    var route = RequestRoute;
                    Session.Remove(SessionKey.Route);
                    return RedirectToRoute(route);
                }
                return RedirectToAction("Index");
            }
            else
                return View(login);
        }
        [HttpGet]
        public ActionResult Logout()
        {
            Session.Remove(SessionKey.Account);
            return RedirectToAction("Login");
        }
        [LoginFilter]
        [HttpGet]
        public ActionResult ChangePassword()
        {
            if (Request.IsAjaxRequest())
                return Json(new
                {
                    html = RenderPartialViewToString(Views.ChangePasswordPartial, new ChangePasswordModel())
                }, JsonRequestBehavior.AllowGet);
            return View(Views.ChangePassword, new ChangePasswordModel());
        }
        [LoginFilter]
        [HttpPost]
        public ActionResult ChangePassword(ChangePasswordModel model)
        {
            var result = model.Submit(UserID);
            if (Request.IsAjaxRequest())
            {
                return Json(new {
                    result = result,
                    html = RenderPartialViewToString(Views.ChangePasswordPartial, model),
                    message = model.Message
                },
                JsonRequestBehavior.DenyGet);
            }
            else
            {
                if (result)
                    return RedirectToAction("Logout");
                return View(model);
            }
        }
        #endregion
        [LoginFilter]
        public ActionResult UploadFile(string folder)
        {
            var message = "Upload failed";
            var result = false;
            var fileName = "";
            var task = Task.Run(() =>
            {
                try
                {
                    foreach (string file in Request.Files)
                    {
                        var fileContent = Request.Files[file];
                        if (fileContent != null && fileContent.ContentLength > 0)
                        {
                            // get a stream
                            var stream = fileContent.InputStream;
                            // and optionally write the file to disk
                            fileName = Path.GetFileName(fileContent.FileName);
                            result = FileManagement.SaveFile(folder, fileName, stream);
                            if (result)
                                message = "File uploaded successfully";
                        }
                    }
                }
                catch (Exception e) { }
            });
            task.Wait();
            return Json(new
            {
                file = fileName,
                img = String.Format("/{0}/{1}/{2}", FileManagement.MediaFolder, folder, fileName)
            }, JsonRequestBehavior.DenyGet);
        }
        [LoginFilter]
        public ActionResult GetTransactions()
        {
            var now = DateTime.Now;
            var startDay = new DateTime(now.Year, now.Month, now.Day, 0, 0, 0);
            var endDay = startDay.AddDays(1);
            var model = new TransactionSummary() {
                Transactions = Transaction.Find(UserID, new TransactionClass[] { TransactionClass.Order, TransactionClass.Repair, TransactionClass.Warranty }, startDay, endDay),
                Incomes = IncomeInfo.Find(UserID, Employee.BussinessID, new IncomeFilter() { From = startDay, To = endDay }, null),
                Outcomes = OutcomeInfo.Find(UserID, Employee.BussinessID, new OutcomeFilter() { From = startDay, To = endDay }, null)
            };
            return Json(new
            {
                html = RenderPartialViewToString("OrderSummary", model)
            }, JsonRequestBehavior.AllowGet);
        }
        [LoginFilter]
        public ActionResult ProcessingServices()
        {
            var list = ServiceReport.ProcessingServices(UserID, Employee.BussinessID);
            return Json(new
            {
                html = RenderPartialViewToString("ProcessingServices", list)
            }, JsonRequestBehavior.AllowGet);
        }
        [LoginFilter]
        public ActionResult EmployeeList()
        {
            var list = EmployeeInfo.Find(UserID, Employee.ID, Employee.BussinessID, "", new EmployeeFilter() { Month = DateTime.Now.Month });
            return Json(new
            {
                html = RenderPartialViewToString("EmployeeList", list)
            }, JsonRequestBehavior.AllowGet);
        }
        [LoginFilter]
        [HttpGet]
        public ActionResult GetFine()
        {
            var list = EmployeeFine.Get(Employee.BussinessID, Employee.ID);
            if (Request.IsAjaxRequest())
                return Json(new
                {
                    html = RenderPartialViewToString("FineListPartial", list)
                }, JsonRequestBehavior.AllowGet);
            return View("FineList", list);
        }
    }
}