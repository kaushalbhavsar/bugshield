using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using BUGSKAN.Portal.Models;
using Microsoft.AspNet.Identity;

namespace BUGSKAN.Portal.Controllers
{
    public class SitesController : Controller
    {
        private BugSkanEntities db = new BugSkanEntities();
        public ActionResult GetDropdown()
        {
            string userId = User.Identity.GetUserId();
            var sites = from s in db.Sites select s;
            sites = sites.Where(s => s.UserId.Contains(userId));
            return PartialView("~/Views/Shared/_DropdownList.cshtml",sites);
        }
        [HttpPost]
        public ActionResult DropdownSite()
        {
            string selectedSite = Request.Form["SelectedSite"];
            switch (selectedSite)
            {
                case "+":
                    {
                        return RedirectToAction("Create");
                    }
                default:
                    {
                        var site = db.Sites.Find(Guid.Parse(selectedSite));
                        Session["SelectedSite"] = site.Url;
                        Session["SelectedSiteId"] = site.SiteId;
                        return RedirectToAction("Index","Home");
                    }
            }
        }


        // GET: Sites
        public ActionResult Index()
        {
            string userId = User.Identity.GetUserId();
            var products = from s in db.Sites select s;
            products = products.Where(s => s.UserId.Contains(userId));
            return View(products);
        }

        public ActionResult SitePicker()
        {
            string userId = User.Identity.GetUserId();
            var products = from s in db.Sites select s;
            products = products.Where(s => s.UserId.Contains(userId));
            return PartialView("~/Views/Shared/_SitePicker.cshtml", products);
        }

        [HttpPost]
        public ActionResult GetSite()
        {
            string selectedSite = Request.Form["SelectedSite"];

            switch (selectedSite)
            {
                case "-":
                    {
                        return View();
                    }
                case "+":
                    {
                        return RedirectToAction("Create", "Sites");
                    }

                default:
                    {
                        var site = db.Sites.Find(Guid.Parse(selectedSite));
                        ViewBag.SelectedSiteId = selectedSite;
                        ViewBag.SelectedSite = site.Url;

                        Session["SelectedSiteId"] = selectedSite;
                        Session["SelectedSite"] = site.Url;

                        return RedirectToAction("Index", "Home");

                    }
         
            }

           

        }

        // GET: Sites/Details/5
        public ActionResult Details(Guid? id) 
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Site site = db.Sites.Find(id);
            if (site == null)
            {
                return HttpNotFound();
            }
            return View(site);
        }

        // GET: Sites/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: Sites/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "SiteId,Name,Url,AddedDate,LastScan,Status,Notes,LoginUser,LoginPass,Technology,UserId,LoginUrl,LoginSuccess,IPAddress,Server,PlanName")] Site site)
        {
            if (ModelState.IsValid)
            {
                string CurrUser = User.Identity.GetUserId();
                SubscriptionsController sub = new SubscriptionsController();
                Subscription sb = new Subscription();
                var planname = (from subscrptn in db.Subscriptions.Where(x => x.UserId == CurrUser) select subscrptn.PlanName).First();
                site.SiteId = Guid.NewGuid();
                site.UserId = User.Identity.GetUserId();
                site.AddedDate = DateTime.Today.Date; 
                db.AddNewSiteAndPreferences(site.SiteId, site.Url, site.IPAddress, site.UserId);
                db.SaveChanges();
                return RedirectToAction("Index","Home");
            }
            return View(site);
        }
        // GET: Sites/Edit/5
        public ActionResult Edit(Guid? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Site site = db.Sites.Find(id);
            if (site == null)
            {
                return HttpNotFound();
            }
            return View(site);
        }

        // POST: Sites/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "SiteId,Name,Url,AddedDate,LastScan,Status,Notes,LoginUser,LoginPass,Technology,UserId,LoginUrl,LoginSuccess,IPAddress,Server,PlanName")] Site site)
        {
            if (ModelState.IsValid)
            {
                db.Entry(site).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(site);
        }

        // GET: Sites/Delete/5
        public ActionResult Delete(Guid? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Site site = db.Sites.Find(id);
            if (site == null)
            {
                return HttpNotFound();
            }
            return View(site);
        }

        // POST: Sites/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(Guid id)
        {
            Site site = db.Sites.Find(id);
            db.Sites.Remove(site);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
