using BUGSKAN.Portal.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Newtonsoft.Json;
using MaxMind.GeoIP2.Responses;

namespace BUGSKAN.Portal.Controllers
{
    public class NotificationsController : Controller
    {
        // GET: Notifications

        private BugSkanEntities db = new BugSkanEntities();

        //public string GetNotifications()
        //{
        //    var list = (from t in db.Notifications
        //                orderby t.NotificationId
        //                select t).Take(5).ToList<Notification>();

        //    List<string> IpInfo = new List<string>();

        //    foreach (var itm in list)
        //    {
        //        var loc = (from t in db.ShieldLogs
        //                   where t.id == itm.EventId
        //                   select t.IpInfo);

        //        CityResponse city = (CityResponse)JsonConvert.DeserializeObject<CityResponse>(loc.ToString());

        //        IpInfo.Add()

        //    }

        //    return JsonConvert.SerializeObject(list);
        //}

        public ActionResult Index()
        {
            return View();
        }

        // GET: Notifications/Details/5
        public ActionResult Details(int id)
        {
            return View();
        }

        // GET: Notifications/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: Notifications/Create
        [HttpPost]
        public ActionResult Create(FormCollection collection)
        {
            try
            {
                // TODO: Add insert logic here

                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }

        // GET: Notifications/Edit/5
        public ActionResult Edit(int id)
        {
            return View();
        }

        // POST: Notifications/Edit/5
        [HttpPost]
        public ActionResult Edit(int id, FormCollection collection)
        {
            try
            {
                // TODO: Add update logic here

                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }

        // GET: Notifications/Delete/5
        public ActionResult Delete(int id)
        {
            return View();
        }

        // POST: Notifications/Delete/5
        [HttpPost]
        public ActionResult Delete(int id, FormCollection collection)
        {
            try
            {
                // TODO: Add delete logic here

                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }
    }
}
