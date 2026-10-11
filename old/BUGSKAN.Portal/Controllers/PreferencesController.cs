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
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;

namespace BUGSKAN.Portal.Controllers
{
    public class PreferencesController : Controller
    {
        private BugSkanEntities db = new BugSkanEntities();

        // GET: Preferences
        public ActionResult Index()
        {
            var preferences = db.Preferences.Include(p => p.Site);
            return View(preferences.ToList());
        }

        // GET: Preferences/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Preference preference = db.Preferences.Find(id);
            if (preference == null)
            {
                return HttpNotFound();
            }
            return View(preference);
        }

        // GET: Preferences/Create
        public ActionResult Create()
        {
            ViewBag.DefaultSite = new SelectList(db.Sites, "SiteId", "Name");
            return View();
        }

        // POST: Preferences/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Id,DefaultSite,NotificationEmail,UserId")] Preference preference)
        {
            if (ModelState.IsValid)
            {
                preference.UserId = User.Identity.GetUserId();
                preference.Site = ViewBag.AddWebPreference;
                preference.NotificationEmail = preference.NotificationEmail;
                db.Preferences.Add(preference);
                db.SaveChanges();
                return RedirectToAction("Index","Home");

            }

            ViewBag.DefaultSite = new SelectList(db.Sites, "SiteId", "Name", preference.Site);
            return View(preference);
        }

        // GET: Preferences/Edit/5
        public ActionResult Edit(int? id)
        {

            //if (id == null)
            //{
            //    return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            //}

            using (var context = new BugSkanEntities())
            {
                var userid1 = User.Identity.GetUserId();
                var preferenceid = (from u in context.Preferences
                                    where u.UserId == userid1
                                    select u.Id).FirstOrDefault();

                Preference preference = db.Preferences.Find(preferenceid);

                if (preference == null)
                {
                    return HttpNotFound();
                }
                ViewBag.DefaultSite = new SelectList(db.Sites, "SiteId", "Name", preference.Site);
                return View(preference);
            }
         
        }

        // POST: Preferences/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Id,Site,NotificationEmail,UserId")] Preference preference)
        {
            if (ModelState.IsValid)
            {
                preference.UserId = User.Identity.GetUserId();
                using (var context = new BugSkanEntities())
                {
                    var userid1 = User.Identity.GetUserId();
                    var preferenceid = (from u in context.Preferences
                                        where u.UserId == userid1
                                        select u.Id).FirstOrDefault();
                    var preferencesite = (from u in context.Preferences
                                          where u.Id == preference.Id
                                          select u.Site).FirstOrDefault();
                    preference.Site = preferencesite;
                    preference.Id = preferenceid;
                    db.Entry(preference).State = EntityState.Modified;
                    db.SaveChanges();
                    return RedirectToAction("Index", "Home");
                }
            }
            ViewBag.DefaultSite = new SelectList(db.Sites, "SiteId", "Name", preference.Site);
            return View(preference);
        }

        // GET: Preferences/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Preference preference = db.Preferences.Find(id);
            if (preference == null)
            {
                return HttpNotFound();
            }
            return View(preference);
        }

        // POST: Preferences/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Preference preference = db.Preferences.Find(id);
            db.Preferences.Remove(preference);
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
