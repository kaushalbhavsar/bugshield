using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using BugShield.Portal.Models;
using System.Data.Entity.Core.Objects;
using PagedList;
using MaxMind.GeoIP2;
using MaxMind.Db;
using Microsoft.AspNet.Identity;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Builders;
using MongoDB.Driver.Linq;

namespace BugShield.Portal.Controllers
{
   
    //MongoClient client;
    //IMongoDatabase database;
    //protected static MongoClient _client;
    //protected static MongoDatabase _database;
    //MongoClient _client;
    //MongoServer _server;
    //MongoDatabase _db;

    public class ShieldLogsController : Controller
    {
      //  private MongoDatabase database;
        private BugSkanDBEntities db = new BugSkanDBEntities();
       


        //public ActionResult GetUsers()
        //{
        //    var connectionstring = "mongodb://localhost";
        //    var client = new MongoClient(connectionstring);

        //    var server = client.GetServer();

        //    database = server.GetDatabase("Football");
        //    MongoServer objServer = MongoServer.Create("Server=localhost:27017");
        //    MongoDatabase objDatabse = objServer.GetDatabase("MVCTestDB");
        //    System.Collections.Generic.List UserDetails = objDatabse.GetCollection("Users").FindAll().ToList();
        //    return View(UserDetails);
        //}
        //    public IEnumerable<ShieldLog> GetLogItems(DateTime from, DateTime to)

        //    {
        //        var mongo = new Mango();
        //        mongo.Connect();
        //        var db = mongo.getDB("testmongo");
        //        var server =MongoServer.Create(ConnectionString);
        //        List<ShieldLog> logs = new List<ShieldLog>();

        //        MongoCredential mongoCredential = MongoCredential.CreateCredential("bugshield",
        //                            "bugskan", "H@rdH@xx0127");


        //        MongoClientSettings mongoClientSettings = new MongoClientSettings
        //        {
        //            Credentials = new[] { mongoCredential },
        //            Server = new MongoServerAddress("52.163.95.111", 27017)
        //        };


        //        _client = new MongoClient(mongoClientSettings);


        //        _database = _client.GetDatabase("bugshield");
        //        var docs = _collection.Find(Query.And(Query.GTE("time", from), Query.LTE("time", to)))
        //            .SetSortOrder(SortBy.Descending("time"));
        //        foreach (var doc in docs)
        //        {
        //            LogItem log = new LogItem()
        //            {
        //                ID = doc.GetValue("_id").AsObjectId,
        //                Date = TimeZone.CurrentTimeZone.ToLocalTime(doc.GetValue("time").ToUniversalTime()),
        //                OS = doc.GetValue("os", "").ToString(),
        //                Version = doc.GetValue("version", "").ToString(),
        //                Manufacturer = doc.GetValue("manufacturer", "").ToString(),
        //                Model = doc.GetValue("model", "").ToString(),
        //                Language = doc.GetValue("lang", "").ToString(),
        //                ScreenSize = doc.GetValue("screen", "").ToString(),
        //                Orientation = doc.GetValue("orientation", "").ToString(),
        //                Timezone = doc.GetValue("timezone", "").ToString(),
        //                Level = doc.GetValue("level", "").ToString(),
        //                Message = doc.GetValue("log", "").ToString()
        //            };
        //            if (!string.IsNullOrEmpty(log.Message) && log.Message.Length > 40)
        //                log.Message = log.Message.Substring(0, 40) + " ...";

        //            logs.Add(log);
        //        }
        //        return logs;
        //    }
        public ActionResult Index(string date, string currentFilter, string searchString, string sortOrder, int? page)
        {
            ViewBag.CurrentSort = sortOrder;

            DateTime SearchDate = new DateTime();
            if (date != null)
            {
                try
                {

                    SearchDate = DateTime.Parse(date);
                }
                catch
                {
                    return View();
                }
            }

            else
            {
                SearchDate = DateTime.UtcNow;
            }

            ViewBag.TimeSortParm = String.IsNullOrEmpty(sortOrder) ? "time_desc" : "";
            ViewBag.SourceSortParm = String.IsNullOrEmpty(sortOrder) ? "source_desc" : "";
            ViewBag.MethodSortParm = String.IsNullOrEmpty(sortOrder) ? "method_desc" : "";
            ViewBag.UrlSortParm = String.IsNullOrEmpty(sortOrder) ? "url_desc" : "";
            ViewBag.HostSortParm = String.IsNullOrEmpty(sortOrder) ? "host_desc" : "";
            ViewBag.AttackidSortParm = String.IsNullOrEmpty(sortOrder) ? "attackid_desc" : "";


            DateTime EndDate = SearchDate.Date.AddDays(-1);

            DateTime  StartDate = SearchDate.Date.AddDays(-2);


            if (searchString != null)
            {
                page = 1;
            }
            else
            {
                searchString = currentFilter;
            }
            var products = from s in db.ShieldLogs select s;
            var user = User.Identity.GetUserId();
            List<SelectListItem> hostlist = new List<SelectListItem>();
            var listItems = (from s in db.Sites.Where(s => s.UserId.Contains(user)) select s.Url).ToList();
            foreach (var items in listItems)
            {
                hostlist.Add(new SelectListItem() { Text = items });
            }
            ViewBag.ListofHost = hostlist;
            ViewBag.CurrentFilter = searchString;

            //}


            switch (sortOrder)
            {
                case "time_desc":
                    {
                        products = products.Where(s => s.Date >= StartDate && s.Date < EndDate && s.Host.Contains(searchString)).OrderByDescending(p => p.Time);
                        ViewBag.TotalAttacks = products.Count();
                        break;
                    }

                case "source_desc":
                    {
                        products = products.Where(s => s.Date >= StartDate && s.Date < EndDate && s.Host.Contains(searchString)).OrderByDescending(p => p.SourceIp);
                        ViewBag.TotalAttacks = products.Count();
                        break;
                    }
                case "host_desc":
                    {
                        products = products.Where(s => s.Date >= StartDate && s.Date < EndDate && s.Host.Contains(searchString)).OrderByDescending(x => x.Host);
                        ViewBag.TotalAttacks = products.Count();
                        break;
                    }
                case "method_desc":
                    {
                        products = products.Where(s => s.Date >= StartDate && s.Date < EndDate && s.Host.Contains(searchString)).OrderByDescending(x => x.Method);
                        ViewBag.TotalAttacks = products.Count();
                        break;
                    }
                case "url_desc":
                    {
                        products = products.Where(s => s.Date >= StartDate && s.Date < EndDate && s.Host.Contains(searchString)).OrderByDescending(x => x.Url);
                        ViewBag.TotalAttacks = products.Count();
                        break;
                    }
                case "attackid_desc":
                    {
                        products = products.Where(s => s.Date >= StartDate && s.Date < EndDate && s.Host.Contains(searchString)).OrderByDescending(x => x.AttackId);
                        ViewBag.TotalAttacks = products.Count();
                        break;
                    }


                default:
                    {
                        products = products.Where(p => p.Date >= StartDate && p.Date < EndDate && p.Host.Contains(searchString)).OrderBy(x => x.Time);
                        ViewBag.TotalAttacks = products.Count();
                        break;
                    }
            }


            int pageSize = 25;
            int pageNumber = (page ?? 1);

            //  return View(products);
            return View(products.ToPagedList(pageNumber, pageSize));


            // DateTime ruleDate = Convert.ToDateTime(rule.data);
            //var q = db.ShieldLogs.Where(n => DbFunctions.TruncateTime(n.Date) > startDate);
            //IEnumerable<ShieldLog> data = (IEnumerable<ShieldLog>)q.ToList();

        }

        //private ActionResult View(object p)
        //{
        //    throw new NotImplementedException();
        //}

        // GET: ShieldLogs/Details/5
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

            ViewBag.Title = shieldLog.BugDB.Id + " - " + shieldLog.BugDB.Title;
            return View(shieldLog);
        }
        public ActionResult DetailsIP(string SourceIp, string searchString, string sortOrder, int? page)
        {
            var Logs = from s in db.ShieldLogs select s;
            ShieldLog shieldLog = new ShieldLog();
            ViewBag.TimeSortParm = String.IsNullOrEmpty(sortOrder) ? "time_desc" : "";
            ViewBag.SourceSortParm = String.IsNullOrEmpty(sortOrder) ? "source_desc" : "";
            ViewBag.MethodSortParm = String.IsNullOrEmpty(sortOrder) ? "method_desc" : "";
            ViewBag.UrlSortParm = String.IsNullOrEmpty(sortOrder) ? "url_desc" : "";
            ViewBag.HostSortParm = String.IsNullOrEmpty(sortOrder) ? "host_desc" : "";
            ViewBag.AttackidSortParm = String.IsNullOrEmpty(sortOrder) ? "attackid_desc" : "";
            DateTime StartDate = DateTime.Today;

            var LogsForIp = Logs.Where(s => s.Date == StartDate && s.SourceIp == SourceIp && s.Host.Contains(searchString));
            ViewBag.AttackNo = LogsForIp.Count();

            //var LogsForIp = (from IpLogs in db.ShieldLogs.Where(x => x.SourceIp == SourceIp && x.Host.Contains(searchString)) join bug in db.BugDBs on IpLogs.AttackId equals bug.Id select new { IpLogs.Time, IpLogs.Method, IpLogs.Url, IpLogs.Host, IpLogs.ResponseCode,IpLogs.IpInfo, bug.Title });

            switch (sortOrder)
            {
                case "time_desc":
                    {
                        Logs = Logs.Where(s => s.SourceIp == SourceIp && s.Host.Contains(searchString)).OrderByDescending(p => p.Time);
                        ViewBag.TotalAttackNo = Logs.Count();
                        break;
                    }

                case "source_desc":
                    {
                        Logs = Logs.Where(s => s.SourceIp == SourceIp && s.Host.Contains(searchString)).OrderByDescending(p => p.SourceIp);
                        ViewBag.TotalAttackNo = Logs.Count();
                        break;
                    }
                case "host_desc":
                    {
                        Logs = Logs.Where(s => s.SourceIp == SourceIp && s.Host.Contains(searchString)).OrderByDescending(x => x.Host);
                        ViewBag.TotalAttackNo = Logs.Count();
                        break;
                    }
                case "method_desc":
                    {
                        Logs = Logs.Where(s => s.SourceIp == SourceIp && s.Host.Contains(searchString)).OrderByDescending(x => x.Method);
                        ViewBag.TotalAttackNo = Logs.Count();
                        break;
                    }
                case "url_desc":
                    {
                        Logs = Logs.Where(s => s.SourceIp == SourceIp && s.Host.Contains(searchString)).OrderByDescending(x => x.Url);
                        ViewBag.TotalAttackNo = Logs.Count();
                        break;
                    }
                case "attackid_desc":
                    {
                        Logs = Logs.Where(s => s.SourceIp == SourceIp && s.Host.Contains(searchString)).OrderByDescending(x => x.AttackId);
                        ViewBag.TotalAttackNo = Logs.Count();
                        break;
                    }
                default:
                    {
                        Logs = Logs.Where(p => p.SourceIp == SourceIp && p.Host.Contains(searchString)).OrderBy(x => x.Time);
                        ViewBag.TotalAttackNo = Logs.Count();
                        break;
                    }
            }


            int pageSize = 25;
            int pageNumber = (page ?? 1);
            shieldLog.id = User.Identity.GetUserId();
            ViewBag.SourceIp = SourceIp;
            foreach (var item in Logs)
            {
                ViewBag.IpInfo = item.IpInfo;
                ViewBag.SearchString = item.Host;
            }
            shieldLog.SourceIp = SourceIp;
            return View(Logs.ToPagedList(pageNumber, pageSize));

        }
        // GET: ShieldLogs/Create
        public ActionResult Create()
        {


            return View();
        }

        // POST: ShieldLogs/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.

        [Authorize]
        [HttpPost]

        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "id,Date,Time,SourceIp,SourcePort,DestIp,DestPort,Method,Url,Host,UserAgent,PostData,ResponseCode,AttackId")] ShieldLog shieldLog)
        {
            if (ModelState.IsValid)
            {
                shieldLog.id = Guid.NewGuid().ToString();
                db.ShieldLogs.Add(shieldLog);
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(shieldLog);
        }

        // GET: ShieldLogs/Edit/5

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
            return View(shieldLog);
        }

        // POST: ShieldLogs/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.

        [HttpPost]
        [ValidateAntiForgeryToken]

        public ActionResult Edit([Bind(Include = "id,Date,Time,SourceIp,SourcePort,DestIp,DestPort,Method,Url,Host,UserAgent,PostData,ResponseCode,AttackId")] ShieldLog shieldLog)
        {
            if (ModelState.IsValid)
            {
                db.Entry(shieldLog).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(shieldLog);
        }

        // GET: ShieldLogs/Delete/5

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

        // POST: ShieldLogs/Delete/5
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