using Delives.pk.Utilities;
using Services.Models;
using Services.Services;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Delives.pk.Controllers
{
    public class PartnerController : Controller
    {
       
        [HttpPost]
        public ActionResult PartnerCreate(ListItemLocal model) 
        {
            try
            {
                model.Id = ListService.Create(model);
                var relativePath = ConfigurationManager.AppSettings["saveImagesIn"];
                if (model.LogoImage != null)
                    model.LogoImage = Functions.SaveFile(model.Logo, relativePath, Server.MapPath(relativePath), model.Id + "_Logo");
                if (model.BgImage != null)
                    model.BgImage = Functions.SaveFile(model.Logo, relativePath, Server.MapPath(relativePath), model.Id + "_Background");
                ListService.UpdateImages(model);
            }
            catch (Exception dsd)
            {

            }
            return View(model);
        }

        [HttpGet]
        public ActionResult PartnerCreate()
        {
            var cats = ListService.GetCategories(true);
            return View(new ListItemLocal { Categoreis= cats});
        }


        [HttpGet]
        public ActionResult View()
        {
            var items = ListService.GetItemsForList_AdminPanel();
            return View(items);
        }
        private string SaveFile(HttpPostedFileBase file, string path, string serverPath, string fileName = null, int type = 0)
        {
            string _path = null;
            string baseUrl = Request.Url.Scheme + "://" + Request.Url.Authority + Request.ApplicationPath.TrimEnd('/') + "/";
            try
            {
                if (file.ContentLength > 0)
                {
                    //string _FileName = Path.GetFileName(file.FileName);
                    _path = Path.Combine(Server.MapPath(path), fileName);
                    file.SaveAs(_path);
                }
                ViewBag.Message = "File Uploaded Successfully!!";
                return ConfigurationManager.AppSettings["saveImagesIn"] + path.TrimStart('~', '/') + "/" + fileName;
            }
            catch
            {
                ViewBag.Message = "File upload failed!!";
                return ConfigurationManager.AppSettings["imagesBaseURL"] + path.TrimStart('~', '/') + "/default.png" ;
            }
        }
    }
}