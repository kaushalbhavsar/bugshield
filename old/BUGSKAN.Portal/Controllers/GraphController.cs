using BUGSKAN.Portal.Models;
using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace BUGSKAN.Portal.Controllers
{
    public class GraphController : Controller
    {
        private BugSkanEntities db = new BugSkanEntities();
        public string GetDefaultSite()
        {

            var user = User.Identity.GetUserId();
            Preference host = db.Preferences.Where(t => t.UserId.Contains(user)).First();

            return host.Site1.Url;


        }
        public JsonResult GetLogsPerSecondData()
        {
            List<GetLogsPerSecond2_Result> d = new List<GetLogsPerSecond2_Result>();

            string host = GetDefaultSite();

            var d1 = db.GetLogsPerSecond2(host).ToList();
            foreach (var item in d1)
            {
                GetLogsPerSecond2_Result d2 = new GetLogsPerSecond2_Result();
                d2.theTime = item.theTime;
                d2.Column1 = item.Column1;
                d.Add(d2);
            }
            return Json(d.ToList(), JsonRequestBehavior.AllowGet);
        }

        public JsonResult RecentAttacksBlocked()
        {
            List<object> chartData = new List<object>();
            Session["ReRecentAttacksBlocked"] = 1;
            string host = GetDefaultSite();


            var logs = db.RecentAttacksBlocked(host).ToList();
            if(logs==null)
            {
                Session["ReRecentAttacksBlocked"] = 0;
            }
            foreach (var l in logs)
            {
                var newString = l.Date.ToString().Substring(0, l.Date.ToString().IndexOf(' '));
                chartData.Add(new object[]
                             {
                                 newString,l.AttackCount
                             });
            }
            return Json(chartData, JsonRequestBehavior.AllowGet);

        }


        public JsonResult RecentAttackTypeCount()
        {
            List<object> chartData = new List<object>();

            string host = GetDefaultSite();

            var logs = db.RecentAttackTypeCount(host).ToList();

            foreach (var l in logs)
            {

                chartData.Add(new object[]
                             {
                                 l.Title,l.AttackCount
                             });
            }
            return Json(chartData, JsonRequestBehavior.AllowGet);

        }

        public JsonResult RecentAttackUrlCount()
        {
            List<object> chartData = new List<object>();

            string host = GetDefaultSite();

            var logs = db.RecentAttackUrlCount(host).ToList();

            foreach (var l in logs)
            {

                chartData.Add(new object[]
                             {
                                 l.Url,l.AttackCount
                             });
            }
            return Json(chartData, JsonRequestBehavior.AllowGet);

        }

        public JsonResult IpAttacksBlocked(string ip)
        {
            List<object> chartData = new List<object>();

            string host = GetDefaultSite();


            var logs = db.IPAttacksBlocked(host, ip).ToList();

            foreach (var l in logs)
            {
                var newString = l.Date.ToString().Substring(0, l.Date.ToString().IndexOf(' '));
                chartData.Add(new object[]
                             {
                                 newString,l.AttackCount
                             });
            }
            return Json(chartData, JsonRequestBehavior.AllowGet);

        }


        public JsonResult IpAttackTypeCount(string ip)
        {
            List<object> chartData = new List<object>();

            string host = GetDefaultSite();

            var logs = db.IPAttackTypeCount(host, ip).ToList();

            foreach (var l in logs)
            {

                chartData.Add(new object[]
                             {
                                 l.Title,l.AttackCount
                             });
            }
            return Json(chartData, JsonRequestBehavior.AllowGet);

        }

        public JsonResult IpAttackUrlCount(string ip)
        {
            List<object> chartData = new List<object>();

            string host = GetDefaultSite();

            var logs = db.IpAttackUrlCount(host, ip).ToList();

            foreach (var l in logs)
            {

                chartData.Add(new object[]
                             {
                                 l.Url,l.AttackCount
                             });
            }
            return Json(chartData, JsonRequestBehavior.AllowGet);

        }


    }
}