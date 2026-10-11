using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using BugShield.Portal.Models;

namespace BugShield.Portal.Controllers
{
    public class BugDBController : Controller
    {
        private BugSkanDBEntities db = new BugSkanDBEntities();

        // GET: BugDB
        public ActionResult Index()
        {
            return View(db.BugDBs.ToList());
        }

        // GET: BugDB/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            BugDB bugDB = db.BugDBs.Find(id);
            if (bugDB == null)
            {
                return HttpNotFound();
            }
            return View(bugDB);
        }

        // GET: BugDB/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: BugDB/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Id,Title,Description,Links,VideoUrl,TimeStamp")] BugDB bugDB)
        {
            if (ModelState.IsValid)
            {
                db.BugDBs.Add(bugDB);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(bugDB);
        }

        // GET: BugDB/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            BugDB bugDB = db.BugDBs.Find(id);
            if (bugDB == null)
            {
                return HttpNotFound();
            }
            return View(bugDB);
        }

        // POST: BugDB/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Id,Title,Description,Links,VideoUrl,TimeStamp")] BugDB bugDB)
        {
            if (ModelState.IsValid)
            {
                db.Entry(bugDB).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(bugDB);
        }

        // GET: BugDB/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            BugDB bugDB = db.BugDBs.Find(id);
            if (bugDB == null)
            {
                return HttpNotFound();
            }
            return View(bugDB);
        }

        // POST: BugDB/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            BugDB bugDB = db.BugDBs.Find(id);
            db.BugDBs.Remove(bugDB);
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
