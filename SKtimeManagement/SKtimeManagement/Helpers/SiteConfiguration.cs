using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;

namespace SKtimeManagement
{
    public class SiteConfiguration
    {
        private static string GetAppSettingValue(string key, string defaultValue = "")
        {
            return ConfigurationManager.AppSettings[key] ?? defaultValue;
        }
        public static string ApplicationPath { get; set; }
        public static string SitePrefix { get { return GetAppSettingValue("site-prefix"); } }
        public static string AdminEmail { get { return GetAppSettingValue("admin-email"); } }
        public static string DbUser { get { return GetAppSettingValue("dbuser"); } }
        public static string DbPass { get { return GetAppSettingValue("dbpass"); } }
        public static string DbServer { get { return GetAppSettingValue("dbserver"); } }
        public static string AvailableExtentions { get { return "jpg,jpeg,png,xls"; } }
        public static bool UseLocalDb { get { return Boolean.Parse(GetAppSettingValue("use-local-db", "true")); } }
        public static BussinessInfo BussinessInfo { get; set; }
    }
}