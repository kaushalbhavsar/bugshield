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
using PagedList;
using System.Data.SqlClient;
using System.Data.Entity.Core.Objects;

namespace BUGSKAN.Portal.Controllers
{
    [Authorize]
    public class AttacksController : Controller
    {


        private BugSkanEntities db = new BugSkanEntities();
       public string GetDefaultSite()
        {

            var user = User.Identity.GetUserId();
            Preference host = db.Preferences.Where(t => t.UserId.Contains(user)).First();
            Session["CurrentSite"] = host.Site1.Url;

            return host.Site1.Url;


        }
        public ActionResult Chart1()
        {
            return View();
        }
        public ActionResult PieChart()
        {
            return View();
        }
        public ActionResult SampleChart()
        {
            return View();
        }
        public ActionResult PieChartHighchart()

        {
            return View();
        }
        public JsonResult PieChartData()
        {
            List<Rpt_AttackType_Today_Result> d = new List<Rpt_AttackType_Today_Result>();
            DateTime SearchDate = DateTime.UtcNow;
            DateTime date = SearchDate.Date.AddDays(-3);
            var d1 = db.Rpt_AttackType_Today("bugskan.com", date).ToList();
            foreach(var item in d1)
            {
                Rpt_AttackType_Today_Result d2 = new Rpt_AttackType_Today_Result();
                d2.name = item.name;
                d2.y = item.y;
                d.Add(d2);
            }
            return Json(d.ToList(), JsonRequestBehavior.AllowGet);
        }
        public JsonResult GetLogsPerSecondData(string host)
        {
            host = GetDefaultSite();
            List<GetLogsPerSecond_Result> d = new List<GetLogsPerSecond_Result>();
            var d1 = db.GetLogsPerSecond(host).ToList();
            foreach (var item in d1)
            {
                GetLogsPerSecond_Result d2 = new GetLogsPerSecond_Result();
                d2.theTime = item.theTime;
                d2.count = item.count;
                d.Add(d2);
            }
            return Json(d.ToList(), JsonRequestBehavior.AllowGet);
        }
        public ActionResult AttackByUrl(string sortOrder, int? page)
        {
            string host = GetDefaultSite();

            var logs = db.Rpt_AttackByURL(host).ToList();

            ViewBag.CurrentSort = sortOrder;
            ViewBag.UrlSortParm = String.IsNullOrEmpty(sortOrder) ? "url_desc" : "";
            ViewBag.CountSortParm = String.IsNullOrEmpty(sortOrder) ? "count_desc" : "";

            switch (sortOrder)
            {
                case "time_desc":
                    {
                        logs = logs.OrderByDescending(p => p.Url).ToList();   
                        break;
                    }
                case "count_desc":
                    {
                        logs = logs.OrderByDescending(p => p.count).ToList();
                        break;
                    }
                default:
                    {
                        break;
                    }
            }
            return View(logs);
            }
        public string AjaxMethodForPieChart()
        {
            DateTime SearchDate = DateTime.UtcNow;
            List<object> chartData = new List<object>();
            string host = "kaushalbhavsar.com";
           // string host =GetDefaultSite();
            DateTime StartDate = SearchDate.Date.AddDays(-6);
            DateTime date = DateTime.Now.Subtract(new TimeSpan(42, 0, 0, 0)).Date;
            var logs = db.Rpt_AttackType_Today(host, StartDate).ToList();
            var logs1 = db.Rpt_AttackCountByDate(host).ToList();
            //var array = JArray.FromObject(logs);
            List<Rpt_AttackType_Today_Result> cdata = new List<Rpt_AttackType_Today_Result>();
            //ViewBag.HostName = host;
            //List<long[]> arr = new List<long[]>();
            string data = "[";
            chartData.Add(new object[]
            {
               "Attacktype","count"
            });
            foreach (var l in logs)
            {
           
                data = data + "{"+ "name:" + "'" + l.name+ "',y:"  + l.y + "}," ;

                //Rpt_AttackType_Today_Result1 obj = new Rpt_AttackType_Today_Result1();
                //obj.name = l.name;
                //obj.y = Convert.ToInt32(l.y);
                //cdata.Add(obj);
                //cdata.Add(l);
                //chartData.Add(new object[]
                //             {
                //    l.name,l.y
                //             });
            }
            data = data.Remove(data.Length - 1);
            //data.Substring(0,data.Length-2);
            data = data + "]";
            //  return Json(cdata, JsonRequestBehavior.AllowGet);
            ViewBag.chartdata = data;
            return data;
        }
        public JsonResult GetAttackCountByDate()
        {
            DateTime SearchDate = DateTime.UtcNow;
            List<object> chartData = new List<object>();
            string host =GetDefaultSite();
            //string host = "kaushalbhavsar.com";
            // DateTime date = DateTime.Now.ToUniversalTime().Date;
            //DateTime date1 = DateTime.Parse(TempData["DateforChart"].ToString()).Date;
            DateTime StartDate = SearchDate.Date.AddDays(-4);
            DateTime date = DateTime.Now.Subtract(new TimeSpan(2, 0, 0, 0)).Date;
            var logs = db.Rpt_AttackCountByDate(host).ToList();
           
            foreach (var l in logs)
            {
                var newString = l.date.ToString().Substring(0, l.date.ToString().IndexOf(' '));
                chartData.Add(new object[]
                             {
newString,l.count
                             });
            }
            return Json(chartData, JsonRequestBehavior.AllowGet);
        }
        public ActionResult AttacksCustomDate(DateTime? sdate, DateTime? edate, string currentFilter, string sortOrder, int? page)
        {
            TempData["sdateForChart"] = sdate;
            TempData["edateForChart"] = edate;
            ViewBag.sdateForChart = sdate;
            ViewBag.edateForChart = edate;
            if (TempData["sdateForChart"] != null && TempData["edateForChart"] != null)
            {
                ViewBag.TimeSortParm = String.IsNullOrEmpty(sortOrder) ? "time_desc" : "";
                ViewBag.SourceSortParm = String.IsNullOrEmpty(sortOrder) ? "source_desc" : "";
                ViewBag.MethodSortParm = String.IsNullOrEmpty(sortOrder) ? "method_desc" : "";
                ViewBag.UrlSortParm = String.IsNullOrEmpty(sortOrder) ? "url_desc" : "";
                ViewBag.HostSortParm = String.IsNullOrEmpty(sortOrder) ? "host_desc" : "";
                ViewBag.AttackidSortParm = String.IsNullOrEmpty(sortOrder) ? "attackid_desc" : "";
                ViewBag.ResponseSortParm = String.IsNullOrEmpty(sortOrder) ? "response_desc" : "";
                ViewBag.DescriptionSortParm = String.IsNullOrEmpty(sortOrder) ? "description_desc" : "";

                DateTime SearchDate = DateTime.UtcNow;
                List<object> chartData = new List<object>();
                string host =GetDefaultSite();
                var logs = db.Rpt_AttackByCustomDateDetails(host, Convert.ToDateTime(TempData["sdateForChart"]), Convert.ToDateTime(TempData["edateForChart"])).ToList();

                switch (sortOrder)
                {
                    case "time_desc":
                        {
                            logs = logs.Where(s => s.Date >= sdate && s.Date <= edate && s.Host.Contains(host)).OrderByDescending(p => p.Time).ToList();                
                            break;
                        }

                    case "source_desc":
                        {
                            logs = logs.Where(s => s.Date >= sdate && s.Date <= edate && s.Host.Contains(host)).OrderByDescending(p => p.SourceIp).ToList();
                            break;
                        }
                    case "host_desc":
                        {
                            logs = logs.Where(s => s.Date >= sdate && s.Date <= edate && s.Host.Contains(host)).OrderByDescending(x => x.Host).ToList();
                            break;
                        }
                    case "method_desc":
                        {
                            logs = logs.Where(s => s.Date >= sdate && s.Date <= edate && s.Host.Contains(host)).OrderByDescending(x => x.Method).ToList();
                            ViewBag.TotalAttacks = logs.Count();
                            break;
                        }
                    case "url_desc":
                        {
                            logs = logs.Where(s => s.Date >= sdate && s.Date <= edate && s.Host.Contains(host)).OrderByDescending(x => x.Url).ToList();
                            break;
                        }
                    case "description_desc":
                        {
                            logs = logs.Where(s => s.Date >= sdate && s.Date <= edate && s.Host.Contains(host)).OrderByDescending(x => x.AttackId).ToList();
                            break;
                        }
                    case "response_desc":
                        {
                            logs = logs.Where(s => s.Date >= sdate && s.Date <= edate && s.Host.Contains(host)).OrderByDescending(x => x.ResponseCode).ToList();
                            break;
                        }
                    default:
                        {
                            logs = logs.Where(p => p.Date >= sdate && p.Date <= edate && p.Host.Contains(host)).OrderBy(x => x.Time).ToList();
                            break;
                        }
                }

                int pageSize = 25;
                int pageNumber = (page ?? 1);

                //  return View(products);
                return View(logs.ToPagedList(pageNumber, pageSize));
                // return View();
            }
            else
            {
                return View();
            }

        }
        public JsonResult AjaxMethodForUrl()
        {
            // if (ViewBag.sdateForChart != null && ViewBag.edateForChart != null)

            string host =GetDefaultSite();
            var logs = db.Rpt_AttackByURL(host).ToList();

            List<object> chartData = new List<object>();
            chartData.Add(new object[]
                {
               "URL","count"
                });
            foreach (var l in logs)
            {

                chartData.Add(new object[]
                             {
l.Url,l.count
                             });
            }
            return Json(chartData, JsonRequestBehavior.AllowGet);
        }
        // GET: Attacks
        public ActionResult Index(string date, string currentFilter, string searchString, string sortOrder, int? page)
        {
            Session["SelectedSite"] = "bugskan.com";
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


            DateTime StartDate = SearchDate.Date;

            DateTime EndDate = SearchDate.Date.AddDays(1);


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
                        products = products.Where(s => s.Date >= StartDate && s.Date <= EndDate && s.Host.Contains(searchString)).OrderByDescending(p => p.Time);
                        ViewBag.TotalAttacks = products.Count();
                        break;
                    }

                case "source_desc":
                    {
                        products = products.Where(s => s.Date >= StartDate && s.Date <= EndDate && s.Host.Contains(searchString)).OrderByDescending(p => p.SourceIp);
                        ViewBag.TotalAttacks = products.Count();
                        break;
                    }
                case "host_desc":
                    {
                        products = products.Where(s => s.Date >= StartDate && s.Date <= EndDate && s.Host.Contains(searchString)).OrderByDescending(x => x.Host);
                        ViewBag.TotalAttacks = products.Count();
                        break;
                    }
                case "method_desc":
                    {
                        products = products.Where(s => s.Date >= StartDate && s.Date <= EndDate && s.Host.Contains(searchString)).OrderByDescending(x => x.Method);
                        ViewBag.TotalAttacks = products.Count();
                        break;
                    }
                case "url_desc":
                    {
                        products = products.Where(s => s.Date >= StartDate && s.Date <= EndDate && s.Host.Contains(searchString)).OrderByDescending(x => x.Url);
                        ViewBag.TotalAttacks = products.Count();
                        break;
                    }
                case "attackid_desc":
                    {
                        products = products.Where(s => s.Date >= StartDate && s.Date <= EndDate && s.Host.Contains(searchString)).OrderByDescending(x => x.AttackId);
                        ViewBag.TotalAttacks = products.Count();
                        break;
                    }


                default:
                    {
                        products = products.Where(s => s.Date >= StartDate && s.Date <= EndDate && s.Host.Contains(searchString)).OrderBy(x => x.Time);
                        //  ViewBag.TotalAttacks = products.Count();
                        break;
                    }
            }


            int pageSize = 25;
            int pageNumber = (page ?? 1);

            //  return View(products);
            return View(products.ToPagedList(pageNumber, pageSize));
        }
        public ActionResult DetailsForAttackByUrl(string Url,string currentFilter, string sortOrder, int? page)
        {

            ViewBag.CurrentSort = sortOrder;
           
            ViewBag.TimeSortParm = String.IsNullOrEmpty(sortOrder) ? "time_desc" : "";
            ViewBag.SourceSortParm = String.IsNullOrEmpty(sortOrder) ? "source_desc" : "";    
            ViewBag.ResponseSortParm = String.IsNullOrEmpty(sortOrder) ? "response_desc" : "";
          
            //DateTime StartDate = DateTime.Today;
            //searchString = "bugskan.com";         //Comment this line
            string host =GetDefaultSite();

            var logs = db.Rpt_AttackByURLDetails(host, Url).ToList();

          
            switch (sortOrder)
            {
                case "time_desc":
                    {

                        logs = logs.OrderByDescending(p => p.time).ToList();
                        break;
                    }

          
                case "source_desc":
                    {
                        logs = logs.OrderByDescending(p => p.sourceip).ToList();
                        break;

                    }
                case "response_desc":
                    {
                        logs = logs.OrderByDescending(p => p.responsecode).ToList();
                        break;
                    }
                default:
                    {
                        break;
                    }
            }

            int pageSize = 25;
            int pageNumber = (page ?? 1);

            //  return View(products);
            //  return View(products.ToPagedList(pageNumber, pageSize));
            return View(logs.ToPagedList(pageNumber,pageSize));
        }
        public ActionResult DetailsForAttackByUrl1(string Url, string currentFilter, string sortOrder, int? page)
        {

            ViewBag.CurrentSort = sortOrder;
          
            if (Url == null)
                Url = Session["StoreUrl"].ToString();
            else
                Session["StoreUrl"] = Url;
            ViewBag.TimeSortParm = String.IsNullOrEmpty(sortOrder) ? "time_desc" : "";
            ViewBag.SourceSortParm = String.IsNullOrEmpty(sortOrder) ? "source_desc" : "";
            ViewBag.ResponseSortParm = String.IsNullOrEmpty(sortOrder) ? "response_desc" : "";

            //DateTime StartDate = DateTime.Today;
            //searchString = "bugskan.com";         //Comment this line
            string host = GetDefaultSite();

            var logs = db.Rpt_AttackByURLDetails(host, Url).ToList();


            switch (sortOrder)
            {
                case "time_desc":
                    {

                        logs = logs.OrderByDescending(p => p.time).ToList();
                        break;
                    }


                case "source_desc":
                    {
                        logs = logs.OrderByDescending(p => p.sourceip).ToList();
                        break;

                    }
                case "response_desc":
                    {
                        logs = logs.OrderByDescending(p => p.responsecode).ToList();
                        break;
                    }
                default:
                    {
                        break;
                    }
            }

            int pageSize = 25;
            int pageNumber = (page ?? 1);

            //  return View(products);
            //  return View(products.ToPagedList(pageNumber, pageSize));
            return View(logs.ToPagedList(pageNumber, pageSize));
        }
        public ActionResult AttackToday(string currentFilter, string sortOrder, int? page)
        {

            ViewBag.CurrentSort = sortOrder;
            var Logs = from s in db.ShieldLogs select s;
            ShieldLog shieldLog = new ShieldLog();
            ViewBag.TimeSortParm = String.IsNullOrEmpty(sortOrder) ? "time_desc" : "";
            ViewBag.SourceSortParm = String.IsNullOrEmpty(sortOrder) ? "source_desc" : "";
            ViewBag.MethodSortParm = String.IsNullOrEmpty(sortOrder) ? "method_desc" : "";
            ViewBag.UrlSortParm = String.IsNullOrEmpty(sortOrder) ? "url_desc" : "";
            ViewBag.HostSortParm = String.IsNullOrEmpty(sortOrder) ? "host_desc" : "";
            ViewBag.AttackidSortParm = String.IsNullOrEmpty(sortOrder) ? "attackid_desc" : "";
            ViewBag.ResponseSortParm = String.IsNullOrEmpty(sortOrder) ? "response_desc" : "";
            ViewBag.DescriptionSortParm = String.IsNullOrEmpty(sortOrder) ? "description_desc" : "";
            //DateTime StartDate = DateTime.Today;
            //searchString = "bugskan.com";         //Comment this line
            string host = GetDefaultSite();
            DateTime SearchDate = DateTime.Now;
            DateTime EndDate = SearchDate.Date;
            DateTime StartDate = SearchDate.Date.AddDays(-1);

            var products = from s in db.ShieldLogs select s;
    
            switch (sortOrder)
            {
                case "time_desc":
                    {
                        products = products.Where(s => s.Date >= StartDate && s.Date <= EndDate && s.Host.Contains(host)).OrderByDescending(p => p.Time);
                        ViewBag.TotalAttacks = products.Count();
                        break;
                    }

                case "source_desc":
                    {
                        products = products.Where(s => s.Date >= StartDate && s.Date <= EndDate && s.Host.Contains(host)).OrderByDescending(p => p.SourceIp);
                        ViewBag.TotalAttacks = products.Count();
                        break;
                    }
                case "host_desc":
                    {
                        products = products.Where(s => s.Date >= StartDate && s.Date <= EndDate && s.Host.Contains(host)).OrderByDescending(x => x.Host);
                        ViewBag.TotalAttacks = products.Count();
                        break;
                    }
                case "method_desc":
                    {
                        products = products.Where(s => s.Date >= StartDate && s.Date <= EndDate && s.Host.Contains(host)).OrderByDescending(x => x.Method);
                        ViewBag.TotalAttacks = products.Count();
                        break;
                    }
                case "url_desc":
                    {
                        products = products.Where(s => s.Date >= StartDate && s.Date <= EndDate && s.Host.Contains(host)).OrderByDescending(x => x.Url);
                        ViewBag.TotalAttacks = products.Count();
                        break;
                    }
                case "description_desc":
                    {
                        products = products.Where(s => s.Date >= StartDate && s.Date <= EndDate && s.Host.Contains(host)).OrderByDescending(x => x.AttackId);
                        ViewBag.TotalAttacks = products.Count();
                        break;
                    }
                case "response_desc":
                    {
                        products = products.Where(s => s.Date >= StartDate && s.Date <= EndDate && s.Host.Contains(host)).OrderByDescending(x => x.ResponseCode);
                        ViewBag.TotalAttacks = products.Count();
                        break;
                    }
                default:
                    {
                        products = products.Where(s => s.Date >= StartDate && s.Date <= EndDate && s.Host.Contains(host)).OrderBy(x => x.Time);
                        ViewBag.TotalAttacks = products.Count();
                        break;
                    }
            }
            if(products.Count()==0)
            {
                Session["NoAttacksToday"] = "No Attacks in last 24 hours";
            }
            int pageSize = 25;
            int pageNumber = (page ?? 1);

            //  return View(products);
            return View(products.ToPagedList(pageNumber, pageSize));

        }
        public ActionResult AttackThisMonth(string SourceIp, string sortOrder, int? page)
        {
            ViewBag.CurrentSort = sortOrder;
            ViewBag.TimeSortParm = String.IsNullOrEmpty(sortOrder) ? "time_desc" : "";
            ViewBag.SourceSortParm = String.IsNullOrEmpty(sortOrder) ? "source_desc" : "";
            ViewBag.MethodSortParm = String.IsNullOrEmpty(sortOrder) ? "method_desc" : "";
            ViewBag.UrlSortParm = String.IsNullOrEmpty(sortOrder) ? "url_desc" : "";
            ViewBag.HostSortParm = String.IsNullOrEmpty(sortOrder) ? "host_desc" : "";
            ViewBag.AttackidSortParm = String.IsNullOrEmpty(sortOrder) ? "attackid_desc" : "";
            ViewBag.ResponseSortParm = String.IsNullOrEmpty(sortOrder) ? "response_desc" : "";
            ViewBag.DescriptionSortParm = String.IsNullOrEmpty(sortOrder) ? "description_desc" : "";
            //DateTime StartDate = DateTime.Today;
           string host =GetDefaultSite();
            // searchString = "kaushalbhavsar.com";         //Comment this line

            DateTime SearchDate = DateTime.UtcNow;
            DateTime EndDate = SearchDate.Date;
            DateTime StartDate = new DateTime(SearchDate.Year, SearchDate.Month, 1);

            var products = from s in db.ShieldLogs select s;

            switch (sortOrder)
            {
                case "time_desc":
                    {
                        products = products.Where(s => s.Date >= StartDate && s.Date <= EndDate && s.Host.Contains(host)).OrderByDescending(p => p.Time);
                        ViewBag.TotalAttacks = products.Count();
                        break;
                    }

                case "source_desc":
                    {
                        products = products.Where(s => s.Date >= StartDate && s.Date <= EndDate && s.Host.Contains(host)).OrderByDescending(p => p.SourceIp);
                        ViewBag.TotalAttacks = products.Count();
                        break;
                    }
                case "host_desc":
                    {
                        products = products.Where(s => s.Date >= StartDate && s.Date <= EndDate && s.Host.Contains(host)).OrderByDescending(x => x.Host);
                        ViewBag.TotalAttacks = products.Count();
                        break;
                    }
                case "method_desc":
                    {
                        products = products.Where(s => s.Date >= StartDate && s.Date <= EndDate && s.Host.Contains(host)).OrderByDescending(x => x.Method);
                        ViewBag.TotalAttacks = products.Count();
                        break;
                    }
                case "url_desc":
                    {
                        products = products.Where(s => s.Date >= StartDate && s.Date <= EndDate && s.Host.Contains(host)).OrderByDescending(x => x.Url);
                        ViewBag.TotalAttacks = products.Count();
                        break;
                    }
                case "description_desc":
                    {
                        products = products.Where(s => s.Date >= StartDate && s.Date <= EndDate && s.Host.Contains(host)).OrderByDescending(x => x.AttackId);
                        ViewBag.TotalAttacks = products.Count();
                        break;
                    }
                case "response_desc":
                    {
                        products = products.Where(s => s.Date >= StartDate && s.Date <= EndDate && s.Host.Contains(host)).OrderByDescending(x => x.ResponseCode);
                        ViewBag.TotalAttacks = products.Count();
                        break;
                    }



                default:
                    {
                        products = products.Where(s => s.Date >= StartDate && s.Date <= EndDate && s.Host.Contains(host)).OrderBy(x => x.Time);
                        ViewBag.TotalAttacks = products.Count();
                        break;
                    }
            }
            if (products.Count() == 0)
            {
                Session["NoAttacksMonth"] = "No Attacks in this month";
            }
            int pageSize = 25;
            int pageNumber = (page ?? 1);

            //  return View(products);
            return View(products.ToPagedList(pageNumber, pageSize));

        }
        public ActionResult AttackThisWeek(string SourceIp,string sortOrder, int? page)
        {
            ViewBag.CurrentSort = sortOrder;

            ViewBag.TimeSortParm = String.IsNullOrEmpty(sortOrder) ? "time_desc" : "";
            ViewBag.SourceSortParm = String.IsNullOrEmpty(sortOrder) ? "source_desc" : "";
            ViewBag.MethodSortParm = String.IsNullOrEmpty(sortOrder) ? "method_desc" : "";
            ViewBag.UrlSortParm = String.IsNullOrEmpty(sortOrder) ? "url_desc" : "";
            ViewBag.HostSortParm = String.IsNullOrEmpty(sortOrder) ? "host_desc" : "";
            ViewBag.AttackidSortParm = String.IsNullOrEmpty(sortOrder) ? "attackid_desc" : "";
            ViewBag.ResponseSortParm = String.IsNullOrEmpty(sortOrder) ? "response_desc" : "";
            ViewBag.DescriptionSortParm = String.IsNullOrEmpty(sortOrder) ? "description_desc" : "";
            //DateTime StartDate = DateTime.Today;

            //searchString = "bugskan.com";                   //Comment this line
           string  host =GetDefaultSite();
            DateTime SearchDate = DateTime.UtcNow;
            DateTime EndDate = SearchDate.Date;
            int endday = (int)EndDate.DayOfWeek;
            DateTime StartDate = SearchDate.Date.AddDays(-7);

            var products = from s in db.ShieldLogs select s;

            switch (sortOrder)
            {
                case "time_desc":
                    {
                        products = products.Where(s => s.Date >= StartDate && s.Date <= EndDate && s.Host.Contains(host)).OrderByDescending(p => p.Time);
                        ViewBag.TotalAttacks = products.Count();
                        break;
                    }

                case "source_desc":
                    {
                        products = products.Where(s => s.Date >= StartDate && s.Date <= EndDate && s.Host.Contains(host)).OrderByDescending(p => p.SourceIp);
                        ViewBag.TotalAttacks = products.Count();
                        break;
                    }
                case "host_desc":
                    {
                        products = products.Where(s => s.Date >= StartDate && s.Date <= EndDate && s.Host.Contains(host)).OrderByDescending(x => x.Host);
                        ViewBag.TotalAttacks = products.Count();
                        break;
                    }
                case "method_desc":
                    {
                        products = products.Where(s => s.Date >= StartDate && s.Date <= EndDate && s.Host.Contains(host)).OrderByDescending(x => x.Method);
                        ViewBag.TotalAttacks = products.Count();
                        break;
                    }
                case "url_desc":
                    {
                        products = products.Where(s => s.Date >= StartDate && s.Date <= EndDate && s.Host.Contains(host)).OrderByDescending(x => x.Url);
                        ViewBag.TotalAttacks = products.Count();
                        break;
                    }
                case "description_desc":
                    {
                        products = products.Where(s => s.Date >= StartDate && s.Date <= EndDate && s.Host.Contains(host)).OrderByDescending(x => x.AttackId);
                        ViewBag.TotalAttacks = products.Count();
                        break;
                    }
                case "response_desc":
                    {
                        products = products.Where(s => s.Date >= StartDate && s.Date <= EndDate && s.Host.Contains(host)).OrderByDescending(x => x.ResponseCode);
                        ViewBag.TotalAttacks = products.Count();
                        break;
                    }
                default:
                    {
                        products = products.Where(s => s.Date >= StartDate && s.Date <= EndDate && s.Host.Contains(host)).OrderBy(x => x.Time);
                        ViewBag.TotalAttacks = products.Count();
                        break;
                    }
            }

            if (products.Count() == 0)
            {
               Session["NoAttacksWeek"] = "No Attacks in this week";
            }
            int pageSize = 25;
            int pageNumber = (page ?? 1);

            //  return View(products);
            return View(products.ToPagedList(pageNumber, pageSize));

        }


        public ActionResult AttackTypesThisWeek(string SourceIp)
        {
           

            //searchString = "bugskan.com";                   //Comment this line
            string host = GetDefaultSite();
            DateTime SearchDate = DateTime.UtcNow;
            DateTime EndDate = SearchDate.Date.AddDays(-1);
            int endday = (int)EndDate.DayOfWeek;
            DateTime StartDate = SearchDate.Date.AddDays(-7);

            var products = (from s in db.ShieldLogs select s).Distinct();

        
            return View(products);

        }


        // GET: Attacks/Details/5
        public ActionResult DetailsForToday(string id)
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
        public ActionResult DetailsForWeek(string id)
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
        public ActionResult DetailsForMonth(string id)
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

        public ActionResult DetailsIPForToday(string SourceIp,  string sortOrder, int? page)
        {
            var Logs = from s in db.ShieldLogs select s;
            ShieldLog shieldLog = new ShieldLog();
            ViewBag.TimeSortParm = String.IsNullOrEmpty(sortOrder) ? "time_desc" : "";
            ViewBag.SourceSortParm = String.IsNullOrEmpty(sortOrder) ? "source_desc" : "";
            ViewBag.MethodSortParm = String.IsNullOrEmpty(sortOrder) ? "method_desc" : "";
            ViewBag.UrlSortParm = String.IsNullOrEmpty(sortOrder) ? "url_desc" : "";
            ViewBag.HostSortParm = String.IsNullOrEmpty(sortOrder) ? "host_desc" : "";
            ViewBag.AttackidSortParm = String.IsNullOrEmpty(sortOrder) ? "attackid_desc" : "";
            // DateTime StartDate = DateTime.Today;
            string host = GetDefaultSite();

            DateTime SearchDate = DateTime.UtcNow;
            DateTime EndDate = SearchDate.Date.AddDays(-1);
            DateTime StartDate = SearchDate.Date.AddDays(-2);
            var LogsForIp = Logs.Where(s => s.Date >= StartDate && s.Date <= EndDate && s.SourceIp == SourceIp && s.Host.Contains(host));
            ViewBag.AttackNo = LogsForIp.Count();

            switch (sortOrder)
            {
                case "time_desc":
                    {
                        Logs = Logs.Where(s => s.SourceIp == SourceIp && s.Host.Contains(host)).OrderByDescending(p => p.Time);
                        ViewBag.TotalAttackNo = Logs.Count();
                        break;
                    }

                case "source_desc":
                    {
                        Logs = Logs.Where(s => s.SourceIp == SourceIp && s.Host.Contains(host)).OrderByDescending(p => p.SourceIp);
                        ViewBag.TotalAttackNo = Logs.Count();
                        break;
                    }
                case "host_desc":
                    {
                        Logs = Logs.Where(s => s.SourceIp == SourceIp && s.Host.Contains(host)).OrderByDescending(x => x.Host);
                        ViewBag.TotalAttackNo = Logs.Count();
                        break;
                    }
                case "method_desc":
                    {
                        Logs = Logs.Where(s => s.SourceIp == SourceIp && s.Host.Contains(host)).OrderByDescending(x => x.Method);
                        ViewBag.TotalAttackNo = Logs.Count();
                        break;
                    }
                case "url_desc":
                    {
                        Logs = Logs.Where(s => s.SourceIp == SourceIp && s.Host.Contains(host)).OrderByDescending(x => x.Url);
                        ViewBag.TotalAttackNo = Logs.Count();
                        break;
                    }
                case "attackid_desc":
                    {
                        Logs = Logs.Where(s => s.SourceIp == SourceIp && s.Host.Contains(host)).OrderByDescending(x => x.AttackId);
                        ViewBag.TotalAttackNo = Logs.Count();
                        break;
                    }
                default:
                    {
                        Logs = Logs.Where(p => p.SourceIp == SourceIp && p.Host.Contains(host)).OrderBy(x => x.Time);
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
        public ActionResult DetailsIPForMonth(string SourceIp, string sortOrder, int? page)
        {
            var Logs = from s in db.ShieldLogs select s;
            ShieldLog shieldLog = new ShieldLog();
            ViewBag.TimeSortParm = String.IsNullOrEmpty(sortOrder) ? "time_desc" : "";
            ViewBag.SourceSortParm = String.IsNullOrEmpty(sortOrder) ? "source_desc" : "";
            ViewBag.MethodSortParm = String.IsNullOrEmpty(sortOrder) ? "method_desc" : "";
            ViewBag.UrlSortParm = String.IsNullOrEmpty(sortOrder) ? "url_desc" : "";
            ViewBag.HostSortParm = String.IsNullOrEmpty(sortOrder) ? "host_desc" : "";
            ViewBag.AttackidSortParm = String.IsNullOrEmpty(sortOrder) ? "attackid_desc" : "";
            // DateTime StartDate = DateTime.Today;
            string host = GetDefaultSite();

            DateTime SearchDate = DateTime.UtcNow;
            DateTime EndDate = SearchDate.Date.AddDays(-1);
            DateTime StartDate = SearchDate.Date.AddDays(-2);
            var LogsForIp = Logs.Where(s => s.Date >= StartDate && s.Date <= EndDate && s.SourceIp == SourceIp && s.Host.Contains(host));
            ViewBag.AttackNo = LogsForIp.Count();

            switch (sortOrder)
            {
                case "time_desc":
                    {
                        Logs = Logs.Where(s => s.SourceIp == SourceIp && s.Host.Contains(host)).OrderByDescending(p => p.Time);
                        ViewBag.TotalAttackNo = Logs.Count();
                        break;
                    }

                case "source_desc":
                    {
                        Logs = Logs.Where(s => s.SourceIp == SourceIp && s.Host.Contains(host)).OrderByDescending(p => p.SourceIp);
                        ViewBag.TotalAttackNo = Logs.Count();
                        break;
                    }
                case "host_desc":
                    {
                        Logs = Logs.Where(s => s.SourceIp == SourceIp && s.Host.Contains(host)).OrderByDescending(x => x.Host);
                        ViewBag.TotalAttackNo = Logs.Count();
                        break;
                    }
                case "method_desc":
                    {
                        Logs = Logs.Where(s => s.SourceIp == SourceIp && s.Host.Contains(host)).OrderByDescending(x => x.Method);
                        ViewBag.TotalAttackNo = Logs.Count();
                        break;
                    }
                case "url_desc":
                    {
                        Logs = Logs.Where(s => s.SourceIp == SourceIp && s.Host.Contains(host)).OrderByDescending(x => x.Url);
                        ViewBag.TotalAttackNo = Logs.Count();
                        break;
                    }
                case "attackid_desc":
                    {
                        Logs = Logs.Where(s => s.SourceIp == SourceIp && s.Host.Contains(host)).OrderByDescending(x => x.AttackId);
                        ViewBag.TotalAttackNo = Logs.Count();
                        break;
                    }
                default:
                    {
                        Logs = Logs.Where(p => p.SourceIp == SourceIp && p.Host.Contains(host)).OrderBy(x => x.Time);
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
        public ActionResult DetailsIPForWeek(string SourceIp,  string sortOrder, int? page)
        {
            var Logs = from s in db.ShieldLogs select s;
            ShieldLog shieldLog = new ShieldLog();
            ViewBag.TimeSortParm = String.IsNullOrEmpty(sortOrder) ? "time_desc" : "";
            ViewBag.SourceSortParm = String.IsNullOrEmpty(sortOrder) ? "source_desc" : "";
            ViewBag.MethodSortParm = String.IsNullOrEmpty(sortOrder) ? "method_desc" : "";
            ViewBag.UrlSortParm = String.IsNullOrEmpty(sortOrder) ? "url_desc" : "";
            ViewBag.HostSortParm = String.IsNullOrEmpty(sortOrder) ? "host_desc" : "";
            ViewBag.AttackidSortParm = String.IsNullOrEmpty(sortOrder) ? "attackid_desc" : "";

            string host =GetDefaultSite();

            DateTime SearchDate = DateTime.UtcNow;
            DateTime EndDate = SearchDate.Date.AddDays(-1);
            DateTime StartDate = SearchDate.Date.AddDays(-2);
            var LogsForIp = Logs.Where(s => s.Date >= StartDate && s.Date <= EndDate && s.SourceIp == SourceIp && s.Host.Contains(host));
            ViewBag.AttackNo = LogsForIp.Count();

            switch (sortOrder)
            {
                case "time_desc":
                    {
                        Logs = Logs.Where(s => s.SourceIp == SourceIp && s.Host.Contains(host)).OrderByDescending(p => p.Time);
                        ViewBag.TotalAttackNo = Logs.Count();
                        break;
                    }

                case "source_desc":
                    {
                        Logs = Logs.Where(s => s.SourceIp == SourceIp && s.Host.Contains(host)).OrderByDescending(p => p.SourceIp);
                        ViewBag.TotalAttackNo = Logs.Count();
                        break;
                    }
                case "host_desc":
                    {
                        Logs = Logs.Where(s => s.SourceIp == SourceIp && s.Host.Contains(host)).OrderByDescending(x => x.Host);
                        ViewBag.TotalAttackNo = Logs.Count();
                        break;
                    }
                case "method_desc":
                    {
                        Logs = Logs.Where(s => s.SourceIp == SourceIp && s.Host.Contains(host)).OrderByDescending(x => x.Method);
                        ViewBag.TotalAttackNo = Logs.Count();
                        break;
                    }
                case "url_desc":
                    {
                        Logs = Logs.Where(s => s.SourceIp == SourceIp && s.Host.Contains(host)).OrderByDescending(x => x.Url);
                        ViewBag.TotalAttackNo = Logs.Count();
                        break;
                    }
                case "attackid_desc":
                    {
                        Logs = Logs.Where(s => s.SourceIp == SourceIp && s.Host.Contains(host)).OrderByDescending(x => x.AttackId);
                        ViewBag.TotalAttackNo = Logs.Count();
                        break;
                    }
                default:
                    {
                        Logs = Logs.Where(p => p.SourceIp == SourceIp && p.Host.Contains(host)).OrderBy(x => x.Time);
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

        // GET: Attacks/Create
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

        // POST: Attacks/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]


        // GET: Attacks/Edit/5
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

        // POST: Attacks/Edit/5
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
        // GET: Attacks/Delete/5
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

        // POST: Attacks/Delete/5
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
