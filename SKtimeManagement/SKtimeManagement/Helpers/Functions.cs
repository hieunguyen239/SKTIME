using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Web;

namespace SKtimeManagement
{
    public class Functions
    {
        public static Random Random = new Random();
        public static void CopyFolder(string from, string to)
        {
            if (!Directory.Exists(to))
                Directory.CreateDirectory(to);
            foreach (var file in Directory.GetFiles(from).Select(f => new FileInfo(f)))
            {
                File.Copy(file.FullName, String.Format(@"{0}\{1}", to, file.Name), true);
            }
            foreach (var folder in Directory.GetDirectories(from).Select(f => new DirectoryInfo(f)))
            {
                CopyFolder(folder.FullName, String.Format(@"{0}\{1}", to, folder.Name));
            }
        }
        public static string CheckDirectory(string path)
        {
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);
            return path;
        }
        public static void EmptyFolder(string path)
        {
            Directory.Delete(path, true);
            Directory.CreateDirectory(path);
        }
        public static string RandomNumberString(int length, int min = 0, int max = 9)
        {
            if (length <= 0)
                return "";
            var arr = new string[length];
            for (var i = 0; i < arr.Length; i++)
            {
                arr[i] = Functions.Random.Next(min, max).ToString();
            }
            return String.Join("", arr);
        }
        public static Color RandomColor()
        {
            var red = Random.Next(0, 255);
            var green = Random.Next(0, 255);
            var blue = Random.Next(0, 255);
            return Color.FromArgb(255, red, green, blue);
        }
    }
}