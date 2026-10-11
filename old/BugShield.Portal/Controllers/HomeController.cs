using BugShield.Portal.Models;
using Microsoft.AspNet.Identity;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.Linq;
using MongoDB.Bson;
using MongoDB.Driver;
using System.Web;
using System.Web.Mvc;
using MongoDB.Driver.Builders;


namespace BugShield.Portal.Controllers
{
    public class HomeController : Controller
    {
      
        private BugSkanDBEntities db = new BugSkanDBEntities();
       
        string Constring= "Data Source=52.163.95.111;Initial Catalog=BugSkanDB;User ID=bugskan;Password=H@rdH@xx0127;MultipleActiveResultSets=True;Application Name=EntityFramework";
        public ActionResult Index(string host, string DateforChart)
        {
        
            if (DateforChart != null && host != null)

            {
                //if (DateforChart == "")
                //{
                //    DateforChart = DateTime.Today.Date.ToString();

                //}
                TempData["HostNameForChart"] = host;
                DateTime d = DateTime.Parse(DateforChart);
                //dt will be DateTime type
                DateTime dt = DateTime.ParseExact(DateforChart, "yyyy-MM-dd", CultureInfo.InvariantCulture);
                //s will be MM/dd/yyyy format string
                DateTime date1 = dt.ToUniversalTime();

                TempData["DateforChart"] = date1;
                TempData.Keep();
                

            }

            var user = User.Identity.GetUserId();
            List<SelectListItem> hostlist = new List<SelectListItem>();
            var listItems = (from s in db.Sites.Where(s => s.UserId.Contains(user)) select s.Url).ToList();
            foreach (var items in listItems)
            {
                hostlist.Add(new SelectListItem() { Text = items });
            }
            ViewBag.ListofHosts = hostlist;


           // ViewBag.Ticks = GetTicks(host);

            // return Redirect("/Reports/GetAttackTypesByHost");
            
            return View();
        }
        //public JsonResult AjaxMethod()
        //{
        //    //var products = from s in db.ShieldLogs select s.AttackId;
        //    //var prod = from s in db.ShieldLogs select s.Date;
        //    //var user = User.Identity.GetUserId();
        //    //List<object> chartData = new List<object>();
        //    //chartData.Add(new object[]
        //    //                  {
        //    //            "AttackId","Date"
        //    //                  });
        //    //chartData.Add(new object[]
        //    //                    {
        //    //            products,prod
        //    //                    });
        //    //var user = User.Identity.GetUserId();

        //    string MyConnection = "Server=52.163.95.111; Database=mlogc; Uid=bugskan;Pwd=H@rdAdm1n";
        //    if (TempData["HostNameForChart"] != null)
        //    {
        //        // var logs = db.Rpt_AttackCountByHour(TempData["HostNameForChart"].ToString(), DateTime.Parse(TempData["DateforChart"].ToString())).ToList();
        //        List<ShieldLog> chartData = new List<ShieldLog>();
        //        string query = "SELECT *";
        //        query += " FROM ShieldLog WHERE Host=" + TempData["HostNameForChart"].ToString();
        //        SqlConnection myConn = new SqlConnection("Server=52.163.95.111; Database=mlogc; Uid=bugskan;Pwd=H@rdAdm1n");
        //        SqlCommand objComm = new SqlCommand(query, myConn);
        //        myConn.Open();
        //        SqlDataReader sdr = objComm.ExecuteReader();
        //        while (sdr.Read())
        //        {
                   
        //        }
        //        string constr = ConfigurationManager.ConnectionStrings["Server=52.163.95.111; Database=mlogc; Uid=bugskan;Pwd=H@rdAdm1n"].ConnectionString;
        //        List <object> chartData = new List<object>();
        //        chartData.Add(new object[]
        //                        {
        //                    "Date", "AttackId"
        //                        });
        //        using (SqlConnection con = new SqlConnection(constr))
        //        {
        //            using (SqlCommand cmd = new SqlCommand(query.ToString()))
        //            {
        //                cmd.CommandType = CommandType.Text;
        //                cmd.Connection = con;
        //                con.Open();
        //                using (SqlDataReader sdr = cmd.ExecuteReader())
        //                {
        //                    while (sdr.Read())
        //                    {
        //                        chartData.Add(new object[]
        //                        {
        //                    sdr["Date"], sdr["AttackId"]
        //                        });
        //                    }
        //                }

        //                con.Close();
        //            }
        //        }

        //        return Json(chartData);
        //    }
        //    else
        //        return null;
        //}

        //public SelectList GetHostNames()
        //{
        //    var user = User.Identity.GetUserId();
        //    IEnumerable<SelectListItem> HostList = (from m in db.Sites.Where(m => m.UserId.Contains(user)) select m).AsEnumerable().Select(m => new SelectListItem() { Text = m.Url , Value = m.Url.ToString() });
        //    return new SelectList(HostList, "Value", "Text", Url);
        //}


        //public ActionResult GetHostName(Site hostname)
        //{
        //    var user = User.Identity.GetUserId();
        //    hostname = new Site() { Url = user };
        //    //mydropdownlist ddlList = new mydropdownlist() { istateid = ddlListPostData.istateid, icityid = ddlListPostData.icityid, iareaid = ddlListPostData.iareaid };
        //    return View(hostname);

        //}

        //using (var db = new BugSkanDBEntities())
        //{
        //    var model = new Site();
        //    model.Userid = db.Sites.ToList();
        //    return View(model);
        //}
        //var user = User.Identity.GetUserId();
        //List<SelectListItem> hostlist = new List<SelectListItem>();
        //var listItems = (from s in db.Sites.Where(s => s.UserId.Contains(user)) select s.Url).ToList();
        //foreach (var items in listItems)
        //{
        //    hostlist.Add(new SelectListItem() { Text = items });
        //}
        //ViewBag.ListofHost = hostlist;
        //return View();
    
        public String GetTicks(string host)
        {
            // ViewBag.HostNameForChart = host;
            TempData["HostNameForChart"] = host;
            DateTime date = DateTime.Now.Subtract(new TimeSpan(1, 0, 0, 0)).Date;
            var logs = db.Rpt_AttackTypeByHost(host, date).ToList();
            string ticks = "[";
            int co = logs.Count;
            int i = 0;
            foreach (var l in logs)
            {

                ticks = ticks + "[" + i + ",\"" + l.title + "\"]";
                if (co != i + 1)
                {
                    ticks = ticks + ",";
                }
                i = i + 1;
            }
            ticks = ticks + "]";
            //ViewBag["Ticks"] = ticks;
            TempData["Ticks"] = ticks;
            return ticks;

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


    }
}