using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Web;

namespace Delives.pk.Utilities
{
    public class Functions
    {
        public static string SaveFile(HttpPostedFileBase file, string relativePath, string serverPath, string fileName = null, int type = 0)
        {
            string _path = null;
            string fileExtention = Path.GetExtension(file.FileName);
            fileName = fileName + fileExtention;
            //string baseUrl = Request.Url.Scheme + "://" + Request.Url.Authority + Request.ApplicationPath.TrimEnd('/') + "/";
            try
            {
                if (file.ContentLength > 0)
                {
                    //string _FileName = Path.GetFileName(file.FileName);
                    _path = Path.Combine(serverPath, fileName);
                    file.SaveAs(_path);
                }
                return ConfigurationManager.AppSettings["imagesBaseURL"] + relativePath.TrimStart('~', '/') + "/" + fileName;
            }
            catch
            {
                return ConfigurationManager.AppSettings["imagesBaseURL"] + relativePath.TrimStart('~', '/') + "/default.png";
            }
        }
    }
}