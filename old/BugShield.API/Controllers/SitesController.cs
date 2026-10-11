using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Net;
using System.Web;
using System.Web.Mvc;
using BugShield.API.Models;

namespace BugShield.API.Controllers
{
    public class SitesController : Controller
    {
        private BugSkanDBEntities db = new BugSkanDBEntities();

        // GET: Sites
        public async Task<ActionResult> Index()
        {
            return View(await db.Sites.ToListAsync());
        }

        // GET: Sites/Details/5
        public async Task<ActionResult> Details(Guid? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Site site = await db.Sites.FindAsync(id);
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
        public async Task<ActionResult> Create([Bind(Include = "SiteId,Name,Url,AddedDate,LastScan,Status,Notes,LoginUser,LoginPass,Technology,UserId,LoginUrl,LoginSuccess,IPAddress,Server,PlanName")] Site site)
        {
            if (ModelState.IsValid)
            {
                site.SiteId = Guid.NewGuid();
                db.Sites.Add(site);
                await db.SaveChangesAsync();
                return RedirectToAction("Index");
            }

            return View(site);
        }

        // GET: Sites/Edit/5
        public async Task<ActionResult> Edit(Guid? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Site site = await db.Sites.FindAsync(id);
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
        public async Task<ActionResult> Edit([Bind(Include = "SiteId,Name,Url,AddedDate,LastScan,Status,Notes,LoginUser,LoginPass,Technology,UserId,LoginUrl,LoginSuccess,IPAddress,Server,PlanName")] Site site)
        {
            if (ModelState.IsValid)
            {
                db.Entry(site).State = EntityState.Modified;
                await db.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            return View(site);
        }

        // GET: Sites/Delete/5
        public async Task<ActionResult> Delete(Guid? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Site site = await db.Sites.FindAsync(id);
            if (site == null)
            {
                return HttpNotFound();
            }
            return View(site);
        }

        // POST: Sites/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(Guid id)
        {
            Site site = await db.Sites.FindAsync(id);
            db.Sites.Remove(site);
            await db.SaveChangesAsync();
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
