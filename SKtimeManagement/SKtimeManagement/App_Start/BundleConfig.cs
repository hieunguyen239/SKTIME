using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Optimization;

namespace SKtimeManagement
{
    public class BundleConfig
    {
        public static void RegisterBundles(BundleCollection bundles)
        {
            bundles.Add(new ScriptBundle("~/Script/Base").Include(
                "~/Scripts/jquery-3.1.1.min.js",
                "~/Scripts/moment.min.js",
                "~/Scripts/bootstrap-datetimepicker.min.js",
                "~/Scripts/Table.js",
                "~/Scripts/Form.js",
                "~/Scripts/Chart.min.js",
                "~/Scripts/App/base.min.js"
            ));
            bundles.Add(new ScriptBundle("~/Script/signalR").Include(
                "~/Scripts/jquery.signalR-2.2.2.min.js"
                ));
            bundles.Add(new ScriptBundle("~/Script/signalRSample").Include(
                "~/Scripts/App/SampleSignalR.js"
                ));

            bundles.Add(new StyleBundle("~/Style/Base").Include(
                "~/Content/bootstrap.css",
                "~/Content/bootstrap-datetimepicker.css",
                "~/Content/App/base.css"
            ));
            bundles.Add(new StyleBundle("~/Style/Home").Include(
                "~/Content/App/home.css"
            ));
            bundles.Add(new StyleBundle("~/Style/Login").Include(
                "~/Content/App/login.css"
            ));

            bundles.UseCdn = true;
        }
    }
}