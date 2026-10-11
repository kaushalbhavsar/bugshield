using BUGSKAN.Portal.Models;
using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace BUGSKAN.Portal.Controllers
{
    public class HomeController : Controller
    {

        private BugSkanEntities db = new BugSkanEntities();

        public ActionResult LiveMap()
        {

            return View();
        }

        public string GetDefaultSite()
        {
          

                var user = User.Identity.GetUserId();

            Preference host = db.Preferences.Where(t => t.UserId.Contains(user)).First();
                Session["CurrentSite"] = host.Site1.Url;
            return host.Site1.Url.ToString();
        }

        [Authorize]

        public ActionResult Index()
        {
            ViewBag.Current = "Dashboard";
            string host = GetDefaultSite();
            //if (Session["SelectedSite"] == null)
            //{

            //    string userId = User.Identity.GetUserId();
            //    var products = from s in db.Sites select s;
            //    var site = products.Where(s => s.UserId.Contains(userId)).FirstOrDefault();


            //    string selectedSite = site.SiteId.ToString();


            //    Session.Add("SelectedSiteId", selectedSite);

            //    Session.Add("SelectedSite", site.Url);

            //}

            return View();
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

       
        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }


        public ActionResult Preferences()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }

        public ActionResult Support()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }
    }
}