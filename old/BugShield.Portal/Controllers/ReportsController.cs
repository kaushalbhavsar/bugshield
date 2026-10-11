using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using BugShield.Portal.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.IO;
using System.Web.Script.Serialization;
using System.Globalization;

namespace BugShield.Portal.Controllers
{
    public class ReportsController : Controller
    {
        private BugSkanDBEntities db = new BugSkanDBEntities();

        // GET: Reports
        public ActionResult Index()
        {
            var shieldLogs = db.ShieldLogs.Include(s => s.BugDB);
            return View(shieldLogs.ToList());
        }

        // GET: Reports/Details/5
        public ActionResult Details(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ShieldLog shieldLog = db.ShieldLogs.Find(id);
            if (shieldLog == null)
            {
                return HttpNotFound();
            }
            return View(shieldLog);
        }
        public String GetAttackTypesByHost()
        {
            if (TempData["HostNameForChart"] != null)
            {
                string host = TempData["HostNameForChart"].ToString();
                //DateTime date = DateTime.Now.Subtract(new TimeSpan(1, 0, 0, 0)).Date;
                //// DateTime date = DateTime.Now.ToUniversalTime();
                //var logs = db.Rpt_AttackTypeByHost("bugskan.com", date).ToList();
                //AttackByHost attackobj = new Models.AttackByHost();
                ////Dictionary<string, int> arr = new Dictionary<string, int>();
                //List<AttackByHost> arr = new List<AttackByHost>();
                //foreach (var l in logs)
                //{
                //    attackobj.label = l.title;
                //    attackobj.data = (int)l.count;
                //    arr.Add(attackobj);

                //}
                //var result = JsonConvert.SerializeObject(arr);
                //return result;
                //string date1 = TempData["DateforChart"].ToString();
               // DateTime d = DateTime.Parse(date1);
                //dt will be DateTime type
               // DateTime dt = DateTime.ParseExact(date1, "yyyy-MM-dd", CultureInfo.InvariantCulture);
                //s will be MM/dd/yyyy format string
               // DateTime date = dt.ToUniversalTime();
                DateTime date = DateTime.Parse(TempData["DateforChart"].ToString());

                // DateTime date = DateTime.Now.Subtract(new TimeSpan(1, 0, 0, 0)).Date;
                var logs = db.Rpt_AttackTypeByHost(host, date).ToList();
                // // var logs = db.Rpt_AttackTypeByHost("bugskan.com", DateTime.Now.Date).ToList();
                //  AttackByHost attackobj = new Models.AttackByHost();
                // // List<AttackByHost> arr = new List<AttackByHost>();
                //  Dictionary<string, int> arr = new Dictionary<string, int>();
                List<long[]> arr = new List<long[]>();
                //string ticks = "[";
                int co = logs.Count;
                int i = 0;
                //  int c = 0;
                string res = "[[";
                foreach (var l in logs)
                {
                    //      //attackobj.label = l.title;
                    //      //attackobj.data = (int)l.count;
                    //     res = res + "[\"" + l.title + "\"," + l.count + "]";
                    //      //arr.Add(l.title, (int)l.count);
                    //    ticks = ticks + "[" + i + ",\"" + l.title + "\"]";
                    res = res + "[" + i + "," + l.count + "]";
                    //      AttackByHost a = new Models.AttackByHost();
                    //      arr.Add(l.title, (int)l.count);
                    //      //arr.Add(new long[] { dateticks, (long)l.count });
                    //      //arr.Add(new long[] { (long)c, (long)l.count });
                    //      //a.count = c;
                    //      //a.data = (int)l.count;
                    //      //a.label = l.title;
                    //      //arr.Add(a);
                    if (co != i + 1)
                    {
                        //         ticks = ticks + ",";
                        res = res + ",";
                    }
                    i = i + 1;

                    // c = c + 1;

                }
                //   ticks = ticks + "]";
                //  ticks = ticks.Replace("\"", "");
                //    ViewBag.Ticks = ticks;
                //   TempData["Ticks"] = ticks;
                res = res + "]]";
                //var res = "[[[0,27],[1,24]]]";
                return res;
            }
            else
            {
                return null;
            }
            //var result = JsonConvert.SerializeObject(arr);
            //var result1 = result.Replace(",", "]").Replace("{", "[[").Replace("}", "]]").Replace(":", ",");
            //return result1;

            //JavaScriptSerializer js = new JavaScriptSerializer();
            //string returnArray = js.Serialize(arr);
            //return returnArray;

        }
        //List<AttackByHost> arr = new List<AttackByHost>();
        //foreach (var l in logs)
        //{

        //    AttackByHost a = new Models.AttackByHost();
        //    a.data = (int)l.count;
        //    a.label = l.title;
        //    arr.Add(a);
        //}


        //var serializer = new JsonSerializer();
        //var stringWriter = new StringWriter();
        //using (var writer = new JsonTextWriter(stringWriter))
        //{
        //    writer.QuoteName = false;
        //    serializer.Serialize(writer, arr);
        //}
        //var json = stringWriter.ToString();


        //JsonSerializerSettings JS = new JsonSerializerSettings();
        //JS.TypeNameHandling = TypeNameHandling.Arrays;
        //string result = JsonConvert.SerializeObject(arr, JS);





        public String GetAttacksByDay()
        {
            if (TempData["HostNameForChart"] != null)
            {
                string host = TempData["HostNameForChart"].ToString();
                // string host ="bugskan.com";
                var logs = db.Rpt_AttackCountByDate(host).ToArray<AttackByDate>();

               
                //var array = JArray.FromObject(logs);


                List<long[]> arr = new List<long[]>();

            foreach (var l in logs)
            {


                DateTime outDate = (DateTime)(l.date);


                TimeSpan span = new TimeSpan(new DateTime(1970, 1, 1, 0, 0, 0).Ticks);
                DateTime time = outDate.Subtract(span);
                long d = (long)((time.Ticks / 10000));

                //System.TimeSpan span = new System.TimeSpan(System.DateTime.Parse("1/1/1970").Ticks);
                //System.DateTime time = outDate.Subtract(span);
                long dateticks = ((long)(time.Ticks / 10000));
                arr.Add(new long[] { dateticks, (long)l.count });

                //arr.Add(new long[] { outDate.Date.Day, (long)l.count });
                // arr1.Add(new long[] { (long)l.count});


            }

                var result = JsonConvert.SerializeObject(arr);
                return result;

            }
            else
            {
                return null;

            }
        }
        //  ViewBag.Chart2 = JsonConvert.SerializeObject(arr2);

        // JArray jarray = new JArray();




        //foreach (var log in logs)
        //{

        //    JToken t =
        //    jarray.Add(log.date);
        //}
        //js jsonArray = new JSONArray(readlocationFeed);


        //return PartialView("_GetAttacksByDay", logs);




        public String GetAttacksByHour()
        {
            if (TempData["HostNameForChart"] != null)
            {
                 string host = TempData["HostNameForChart"].ToString();
                // DateTime date = DateTime.Now.ToUniversalTime().Date;
                DateTime date1 = DateTime.Parse(TempData["DateforChart"].ToString()).Date;
               DateTime date = DateTime.Now.Subtract(new TimeSpan(2, 0, 0, 0)).Date;
                // DateTime date = DateTime.Parse(qdate);
                var logs = db.Rpt_AttackCountByHour(host, date1).ToList();
                //var array = JArray.FromObject(logs);

                //ViewBag.HostName = host;
                List<long[]> arr = new List<long[]>();

                foreach (var l in logs)
                {

                    arr.Add(new long[] { (long)l.OnHour, (long)l.Totals });
                }


                string result = JsonConvert.SerializeObject(arr);

                return result;
            }
            else
            {
                return null;
            }

        }
        // GET: Reports/Create
        public ActionResult Create()
        {
            ViewBag.AttackId = new SelectList(db.BugDBs, "Id", "Title");
            return View();
        }

        // POST: Reports/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "id,Date,Time,SourceIp,SourcePort,DestIp,DestPort,Method,Url,Host,UserAgent,PostData,ResponseCode,AttackId,IpInfo")] ShieldLog shieldLog)
        {
            if (ModelState.IsValid)
            {
                db.ShieldLogs.Add(shieldLog);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.AttackId = new SelectList(db.BugDBs, "Id", "Title", shieldLog.AttackId);
            return View(shieldLog);
        }

        // GET: Reports/Edit/5
        public ActionResult Edit(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ShieldLog shieldLog = db.ShieldLogs.Find(id);
            if (shieldLog == null)
            {
                return HttpNotFound();
            }
            ViewBag.AttackId = new SelectList(db.BugDBs, "Id", "Title", shieldLog.AttackId);
            return View(shieldLog);
        }

        // POST: Reports/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "id,Date,Time,SourceIp,SourcePort,DestIp,DestPort,Method,Url,Host,UserAgent,PostData,ResponseCode,AttackId,IpInfo")] ShieldLog shieldLog)
        {
            if (ModelState.IsValid)
            {
                db.Entry(shieldLog).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.AttackId = new SelectList(db.BugDBs, "Id", "Title", shieldLog.AttackId);
            return View(shieldLog);
        }

        // GET: Reports/Delete/5
        public ActionResult Delete(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ShieldLog shieldLog = db.ShieldLogs.Find(id);
            if (shieldLog == null)
            {
                return HttpNotFound();
            }
            return View(shieldLog);
        }

        // POST: Reports/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(string id)
        {
            ShieldLog shieldLog = db.ShieldLogs.Find(id);
            db.ShieldLogs.Remove(shieldLog);
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
