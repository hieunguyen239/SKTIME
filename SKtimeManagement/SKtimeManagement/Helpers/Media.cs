using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;

namespace SKtimeManagement
{
    public class AdminMedia
    {
        public string Path { get; set; }
        public string FolderName { get; set; }
        public string[] Folders { get; set; }
        public string[] Files { get; set; }
    }
    public class FileManagement
    {
        private static string[] AvailableExtension
        {
            get
            {
                //new string[] { "jpg", "jpeg", "png", "gif", "pdf" }
                //SiteConfiguration.Setting.Data.AvailableExtentions.Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries)
                var extensions = SiteConfiguration.AvailableExtentions.Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries);
                return extensions.Select(e => String.Format(".{0}", e.Trim())).ToArray();
            }
        }
        public static string MediaFolder
        {
            get { return "Media"; }
        }
        private static string MediaPath
        {
            get
            {
                var path = String.Format("{0}/{1}", SiteConfiguration.ApplicationPath, MediaFolder);
                if (!Directory.Exists(path))
                    Directory.CreateDirectory(path);
                return path;
            }
        }
        public static List<MediaPath> MediaContent
        {
            get
            {
                var result = new List<MediaPath>();
                try
                {
                    var folders = Directory.GetDirectories(MediaPath);
                    result.AddRange(folders.Select(f => new MediaPath(new DirectoryInfo(f))));
                }
                catch (Exception e) { }
                return result;
            }
        }
        public static string MediaContentHTML(List<HTMLAttribute> attrs = null)
        {
            return String.Join("", MediaContent.Select(c => c.HTML(attrs)));
        }
        public static bool SaveFile(string folder, string fileName, byte[] data)
        {
            var result = false;
            try
            {
                if (!String.IsNullOrEmpty(folder) && !String.IsNullOrEmpty(fileName))
                {
                    var file = new FileInfo(String.Format("{0}/{1}/{2}", MediaPath, folder, fileName));
                    if (AvailableExtension.Contains(file.Extension))
                    {
                        File.WriteAllBytes(file.FullName, data);
                        result = true;
                    }
                }
            }
            catch (Exception e) { }
            return result;
        }
        public static bool SaveFile(string folder, string fileName, Stream stream, out FileInfo file)
        {
            var result = false;
            file = null;
            try
            {
                if (!String.IsNullOrEmpty(folder) && !String.IsNullOrEmpty(fileName))
                {
                    file = new FileInfo(String.Format("{0}/{1}/{2}", MediaPath, folder, fileName));
                    Functions.CheckDirectory(file.DirectoryName);
                    if (AvailableExtension.Contains(file.Extension))
                    {
                        using (var fileStream = System.IO.File.Create(file.FullName))
                        {
                            stream.CopyTo(fileStream);
                        }
                        result = true;
                    }
                }
            }
            catch (Exception e) { }
            return result;
        }
        public static bool SaveFile(string folder, string fileName, Stream stream)
        {
            FileInfo file;
            return SaveFile(folder, fileName, stream, out file);
        }
        public static bool RemoveFile(string folder, string fileName)
        {
            var result = false;
            try
            {
                if (!String.IsNullOrEmpty(folder) && !String.IsNullOrEmpty(fileName))
                {
                    var path = String.Format("{0}/{1}/{2}", MediaPath, folder, fileName);
                    if (File.Exists(path))
                    {
                        var file = new FileInfo(path);
                        if (AvailableExtension.Contains(file.Extension))
                        {
                            File.Delete(path);
                            result = true;
                        }
                    }
                }
            }
            catch (Exception e) { }
            return result;
        }
        public static bool SaveFolder(string folder, bool remove = false)
        {
            var result = false;
            try
            {
                var path = String.Format("{0}/{1}", MediaPath, folder);
                var info = new DirectoryInfo(path);
                if (info.Name.ToLower() != MediaFolder.ToLower())
                {
                    if (remove)
                    {
                        Directory.Delete(path, true);
                    }
                    else if (!Directory.Exists(path))
                    {
                        Directory.CreateDirectory(path);
                    }
                }
            }
            catch (Exception e) { }
            return result;
        }
        public static string[] GetDirectories(string path = "")
        {
            string[] result = null;
            try
            {
                var dirInfo = new DirectoryInfo(String.Format("{0}/{1}", MediaPath, path));
                result = dirInfo.GetDirectories().Select(d => d.Name).ToArray();
            }
            catch (Exception e) { }
            return result;
        }
        public static string[] GetFiles(string path = "")
        {
            string[] result = null;
            try
            {
                var files = Directory.GetFiles(String.Format("{0}/{1}", MediaPath, path));
                result = files.Select(f => new FileInfo(f)).Where(f => AvailableExtension.Contains(f.Extension)).Select(f => f.Name).ToArray();
            }
            catch (Exception e) { }
            return result;
        }
    }
    public class MediaPath
    {
        public MediaPath(DirectoryInfo path)
        {
            Name = path.Name;
            Childs = GetChilds(path);
        }
        public string Name { get; set; }
        public List<MediaPath> Childs { get; set; }
        public string HTML(List<HTMLAttribute> attrs = null)
        {
            var result = "";
            try
            {
                result = String.Format("<div {0} {1} fname=\"{2}\"><span>{2}</span>{3}</div>",
                    String.Join(" ", attrs.Select(a => String.Format("{0}=\"{1}\"", a.Key, a.Value))),
                    Childs != null && Childs.Count > 0 ? "hc=\"1\"" : "",
                    Name,
                    String.Join("", Childs.Select(c => c.HTML(attrs)))
                );
            }
            catch (Exception e) { }
            return result;
        }
        private List<MediaPath> GetChilds(DirectoryInfo info)
        {
            var result = new List<MediaPath>();
            try
            {
                var childs = info.GetDirectories();
                result = childs.Select(c => new MediaPath(c)).ToList();
            }
            catch (Exception e) { }
            return result;
        }
    }
    public class HTMLAttribute
    {
        public string Key { get; set; }
        public string Value { get; set; }
    }
}