using Dynamic;
using NPOI.SS.UserModel;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Script.Serialization;

namespace SKtimeManagement
{
    public static class HTMLExtensions
    {
        public static string PageUrl(this UrlHelper url)
        {
            return String.Format("{0}://{1}:{2}/{3}/", new object[] { url.RequestContext.HttpContext.Request.Url.Scheme, url.RequestContext.HttpContext.Request.Url.Host, url.RequestContext.HttpContext.Request.Url.Port, SiteConfiguration.SitePrefix });
        }
        public static bool ImageAvailable(this UrlHelper url, string folder, string image)
        {
            var result = false;
            try
            {
                var path = String.Format("{0}/{1}/{2}/{3}", SiteConfiguration.ApplicationPath, FileManagement.MediaFolder, folder, image);
                return File.Exists(path);
            }
            catch { }
            return result;
        }
        public static string Image(this UrlHelper url, string folder, string image)
        {
            if (url.ImageAvailable(folder, image))
                return url.Content(String.Format("~/{0}/{1}/{2}", FileManagement.MediaFolder, folder, image));
            else
                return url.Content(String.Format("~/Content/Images/image-not-found.png"));
        }
        public static string GenerateUrlAction(this UrlHelper url, string action = "Index", string controller = "Main", object routeValues = null, bool absolute = false)
        {
            if (!String.IsNullOrEmpty(action) && !String.IsNullOrEmpty(controller))
            {
                if (absolute)
                {
                    string scheme = url.RequestContext.HttpContext.Request.Url.Scheme;
                    return url.Action(action, controller, routeValues, scheme);
                }
                return url.Action(action, controller, routeValues);
            }
            return string.Empty;
        }
        public static string JsonSerialize(this HtmlHelper helper, object obj)
        {
            return new JavaScriptSerializer().Serialize(obj.Clone().EncodeValue()).RemoveLineBreak().Replace(@"\r\n", "").Replace(@"\t", "");
        }
    }
    public static class Extensions
    {
        public static bool Copy(this FileInfo file, string toPath, string backupPath = null)
        {
            var current = String.Format(@"{0}\{1}", toPath, file.Name);
            if (!String.IsNullOrEmpty(backupPath) && File.Exists(current))
                File.Copy(current, String.Format(@"{0}\{1}", backupPath, file.Name), true);
            File.Copy(file.FullName, current, true);
            return true;
        }
        public static bool Copy(this DirectoryInfo folder, string toPath, string backupPath = null)
        {
            if (folder.Name == ".svn")
                return true;
            var to = String.Format(@"{0}\{1}", toPath, folder.Name);
            var backup = !String.IsNullOrEmpty(backupPath) ? String.Format(@"{0}\{1}", backupPath, folder.Name) : null;
            if (!String.IsNullOrEmpty(backup) && !Directory.Exists(backup))
                Directory.CreateDirectory(backup);
            if (!Directory.Exists(to))
                Directory.CreateDirectory(to);
            folder.GetFiles().Select(f => f.Copy(to, backup)).ToArray();
            folder.GetDirectories().Select(f => f.Copy(to, backup)).ToArray();
            return true;
        }
        public static object Clone(this object obj)
        {
            var serializer = new JavaScriptSerializer();
            var json = serializer.Serialize(obj);
            return serializer.Deserialize<object>(json);
        }
        public static object EncodeValue(this object obj)
        {
            if (obj != null)
            {
                var pros = obj.GetType().GetProperties();
                foreach (var pro in pros)
                {
                    if (pro.PropertyType.IsBaseType())
                    {
                        var type = pro.PropertyType.IsNullableType() ? Nullable.GetUnderlyingType(pro.PropertyType) : pro.PropertyType;
                        if (type.Name.Contains("String"))
                        {
                            pro.SetValue(obj, XmlData.RenderXML(pro.GetValue(obj).ToString()));
                        }
                    }
                    else
                    {
                        pro.SetValue(obj, EncodeValue(pro.GetValue(obj)));
                    }
                }
            }
            return obj;
        }
        public static bool IsNullableType(this Type type)
        {
            return type.IsGenericType && type.GetGenericTypeDefinition().Equals(typeof(Nullable<>));
        }
        public static bool IsBaseType(this Type type)
        {
            if (type.IsNullableType())
                type = Nullable.GetUnderlyingType(type);
            return type.FullName.Split('.').Length == 2 && type.FullName.StartsWith("System");
        }
        public static string RemoveLineBreak(this string str)
        {
            return str.Replace("\r\n", "").Replace("\n", "");
        }
        public static string DbValue(this decimal? obj)
        {
            return obj.HasValue ? obj.Value.ToString() : "null";
        }
        public static string DbValue(this int? obj)
        {
            return obj.HasValue ? obj.Value.ToString() : "null";
        }
        public static string DbValue(this DateTime? obj, string format = Constants.DatabaseDatetimeString)
        {
            return obj.HasValue ? String.Format("'{0}'", obj.Value.ToString(format)) : "null";
        }
        public static string DbValue(this DateTime obj, string format = Constants.DatabaseDatetimeString)
        {
            return String.Format("'{0}'", obj.ToString(format));
        }
        public static string DbValue(this bool value)
        {
            return value ? "1" : "0";
        }
        public static string DbValue(this bool? value)
        {
            return value != null ? value.HasValue ? "1" : "0" : "null";
        }
        public static T ChangeType<T>(this string str)
        {
            try
            {
                return (T)Convert.ChangeType(str, typeof(T));
            }
            catch
            {
                return default(T);
            }
        }
        public static T GetCellValue<T>(this IRow row, int index)
        {
            try
            {
                if (row.LastCellNum >= index)
                {
                    var cell = row.GetCell(index);
                    switch (cell.CellType)
                    {
                        case CellType.Numeric: return cell.NumericCellValue.ToString().ChangeType<T>();
                        case CellType.String: return cell.StringCellValue.ChangeType<T>();
                        default: return default(T);
                    }
                }
            }
            catch (Exception e) { }
            return default(T);
        }
        public static decimal Round(this decimal value, int digits = 3)
        {
            var round = Convert.ToDecimal(Math.Pow(10, digits));
            return Math.Round(value / round, 0) * round;
        }
        public static string GetCurrencyShortString(this decimal value)
        {
            var ext = "";
            if (value >= 1000000)
            {
                value /= 1000000;
                ext = "M";
            }
            else if (value >= 1000)
            {
                value /= 1000;
                ext = "K";
            }
            return String.Format("{0}{1}", value.GetCurrencyString(), ext);
        }
        public static string GetCurrencyShortString(this int value)
        {
            var ext = "";
            if (value >= 1000000)
            {
                value /= 1000000;
                ext = "M";
            }
            else if (value >= 1000)
            {
                value /= 1000;
                ext = "K";
            }
            return String.Format("{0}{1}", value.GetCurrencyString(), ext);
        }
        public static string GetCurrencyString(this decimal value)
        {
            return value.ToString("N0");
        }
        public static string GetCurrencyFullString(this decimal value)
        {
            var result = "";
            var str = Convert.ToInt64(value).ToString();
            for (var i = 0; i < str.Length; i++)
            {
                var index = str.Length - 1 - i;
                var current = str[i].ToString();
                var pos = index % 3;
                if (pos == 1 && current == "1")
                    result += " mười";
                else
                {
                    switch (current)
                    {
                        case "9": result += " chín"; break;
                        case "8": result += " tám"; break;
                        case "7": result += " bảy"; break;
                        case "6": result += " sáu"; break;
                        case "5": result += " năm"; break;
                        case "4": result += " bốn"; break;
                        case "3": result += " ba"; break;
                        case "2": result += " hai"; break;
                        case "1": result += " một"; break;
                        default: break;
                    }
                    if (current != "0")
                    {
                        switch (pos)
                        {
                            case 2: result += " trăm"; break;
                            case 1: result += " mươi"; break;
                            default: break;
                        }
                    }
                }
                if (index == 9)
                    result += " tỷ";
                else if (index == 6)
                    result += " triệu";
                else if (index == 3)
                    result += " ngàn";
            }
            result += " đồng";
            result = result.Trim();
            result = result[0].ToString().ToUpper() + result.Substring(1);
            return result;
        }
        public static string GetCurrencyString(this int value)
        {
            return value.ToString("N0");
        }
        public static string GetFullString(this decimal value)
        {
            var result = "";
            var str = value.ToString();
            var index = str.Length - 1;
            return result;
        }
        public static string BarcodeImage(this string code)
        {
            if (code.Length == 13)
            {
                var ean13 = new Ean13();
                ean13.CountryCode = code.Substring(0, 3);
                ean13.ManufacturerCode = code.Substring(3, 4);
                ean13.ProductCode = code.Substring(7, 5);
                ean13.ChecksumDigit = code.Substring(12, 1);
                System.Drawing.Bitmap bmp = ean13.CreateBitmap();
                using (MemoryStream ms = new MemoryStream())
                {
                    bmp.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
                    byte[] byteImage = ms.ToArray();

                    Convert.ToBase64String(byteImage);
                    return "data:image/png;base64," + Convert.ToBase64String(byteImage);
                }
            }
            if (code.Length == 14)
            {
                var ean13 = new Ean13();
                ean13.CountryCode = code.Substring(0, 3);
                ean13.ManufacturerCode = code.Substring(3, 5);
                ean13.ProductCode = code.Substring(8, 5);
                ean13.ChecksumDigit = code.Substring(13, 1);
                ean13.Scale = 1.5f;
                System.Drawing.Bitmap bmp = ean13.CreateBitmap();
                using (MemoryStream ms = new MemoryStream())
                {
                    bmp.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
                    byte[] byteImage = ms.ToArray();

                    Convert.ToBase64String(byteImage);
                    return "data:image/png;base64," + Convert.ToBase64String(byteImage);
                }
            }
            return null;
        }
        public static string ToWebString(this Color color, double alpha = 1)
        {
            return String.Format("rgba({0}, {1}, {2}, {3})", color.R, color.G, color.B, alpha);
        }
        public static string Value(this DayOfWeek value)
        {
            var result = "";
            switch (value)
            {
                case DayOfWeek.Monday: result = "Thứ hai"; break;
                case DayOfWeek.Tuesday: result = "Thứ ba"; break;
                case DayOfWeek.Wednesday: result = "Thứ tư"; break;
                case DayOfWeek.Thursday: result = "Thứ năm"; break;
                case DayOfWeek.Friday: result = "Thứ sáu"; break;
                case DayOfWeek.Saturday: result = "Thứ bảy"; break;
                case DayOfWeek.Sunday: result = "Chủ nhật"; break;
                default: break;
            }
            return result;
        }
        public static string JsonValue(this bool value)
        {
            return value ? "true" : "false";
        }
    }
}