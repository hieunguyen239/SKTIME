using NPOI.HSSF.UserModel;
using NPOI.HSSF.Util;
using NPOI.SS.UserModel;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace SKtimeManagement
{
    public class SalaryController : BaseController
    {
        [LoginFilter]
        [HttpGet]
        public ActionResult Calculate()
        {
            var data = new EmployeeFilter();
            if (Request.IsAjaxRequest())
                return Json(new
                {
                    html = RenderPartialViewToString(Views.CalculatePartial, data)
                }, JsonRequestBehavior.AllowGet);
            return View(Views.Calculate, data);
        }
        [LoginFilter]
        [HttpPost]
        public ActionResult FindEmployee(EmployeeFilter filter)
        {
            var data = EmployeeInfo.Find(UserID, Employee.ID, Employee.BussinessID, "", filter);
            return Json(new
            {
                html = RenderPartialViewToString("EmployeeList", data)
            }, JsonRequestBehavior.DenyGet);
        }
        [LoginFilter]
        [HttpGet]
        public ActionResult ForEmployee(int id)
        {
            var data = SalaryCalculator.Get(UserID, Employee.ID, new SalaryInfo(id), true);
            if (Request.IsAjaxRequest())
                return Json(new
                {
                    html = RenderPartialViewToString(Views.ForEmployeePartial, data)
                }, JsonRequestBehavior.AllowGet);
            return View(Views.ForEmployee, data);
        }
        [LoginFilter]
        [HttpPost]
        public ActionResult ForEmployee(SalaryInfo info)
        {
            if (!info.Month.HasValue)
            {
                info.Month = DateTime.Now.AddMonths(-1);
            }
            var data = SalaryCalculator.Get(UserID, Employee.ID, info, false);
            if (Request.IsAjaxRequest())
                return Json(new
                {
                    html = RenderPartialViewToString(Views.ForEmployeePartial, data)
                }, JsonRequestBehavior.AllowGet);
            return View(Views.ForEmployee, data);
        }
        [LoginFilter]
        [HttpPost]
        public ActionResult AddOffset(int id, decimal offset, decimal value)
        {
            var data = SaleOffset.Add(UserID, Employee.ID, id, offset, value);
            return Json(new
            {
                html = RenderPartialViewToString("Offsets", data)
            }, JsonRequestBehavior.DenyGet);
        }
        [LoginFilter]
        [HttpGet]
        public ActionResult RemoveOffset(int id)
        {
            var data = SaleOffset.Remove(UserID, Employee.ID, id);
            return Json(new
            {
                html = RenderPartialViewToString("Offsets", data)
            }, JsonRequestBehavior.AllowGet);
        }
        [LoginFilter]
        [HttpPost]
        public ActionResult AddDayOff(int id, string date, string note)
        {
            DateTime offDate;
            List<EmployeeOffDay> data;
            if (DateTime.TryParseExact(date, Constants.DateTimeString, CultureInfo.InvariantCulture, DateTimeStyles.None, out offDate))
            {
                data = EmployeeOffDay.Add(UserID, Employee.ID, id, offDate, note);
            }
            else
            {
                data = EmployeeOffDay.Get(UserID, Employee.ID, id);
            }
            return Json(new
            {
                html = RenderPartialViewToString("OffDays", data)
            }, JsonRequestBehavior.DenyGet);
        }
        [LoginFilter]
        [HttpGet]
        public ActionResult RemoveDayOff(int id)
        {
            var data = EmployeeOffDay.Remove(UserID, Employee.ID, id);
            return Json(new
            {
                html = RenderPartialViewToString("OffDays", data)
            }, JsonRequestBehavior.AllowGet);
        }
        [LoginFilter]
        [HttpGet]
        public ActionResult History()
        {
            var data = new EmployeeFilter();
            if (Request.IsAjaxRequest())
                return Json(new
                {
                    html = RenderPartialViewToString(Views.ListPartial, data)
                }, JsonRequestBehavior.AllowGet);
            return View(Views.List, data);
        }
        [LoginFilter]
        [HttpPost]
        public ActionResult FindSalary(EmployeeFilter filter)
        {
            var data = SalaryCalculator.Find(Employee.BussinessID, filter);
            return Json(new
            {
                html = RenderPartialViewToString("SalaryList", data)
            }, JsonRequestBehavior.DenyGet);
        }
        [LoginFilter]
        [HttpGet]
        public ActionResult Detail(int id)
        {
            var data = SalaryCalculator.Get(UserID, Employee.ID, SalaryCalculator.Get(id));
            return Json(new
            {
                html = RenderPartialViewToString(Views.Detail, data)
            }, JsonRequestBehavior.AllowGet);
        }
    }
}