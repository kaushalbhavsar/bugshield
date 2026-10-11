using Microsoft.CSharp;
using CsvHelper;
using MaxMind.GeoIP2;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Threading;




namespace MySqlWatcher
{

    class LogItem
    {
        public Guid Id { get; set; }

        public DateTime Date { get; set; }

        public DateTime Time { get; set; }

        public string SourceIp { get; set; }

        public string SourcePort { get; set; }



        public string DestIp { get; set; }

        public string DestPort { get; set; }

        public string IpDetails { get; set; }

        public string Method { get; set; }

        public string Url { get; set; }

        public string Host { get; set; }

        public string UserAgent { get; set; }

        public string PostData { get; set; }

        public string ResponseCode { get; set; }

        public int AttackId { get; set; }

        public LogItem()
        {

            Id = Guid.NewGuid();
        }
    }
    class Program
    {
        //protected static IMongoClient _client;
        //protected static IMongoDatabase _database;
        //protected static IMongoClient _client1;
        //protected static IMongoDatabase _database1;
        static void Main(string[] args)
        {

            while (true)
            {
                Thread.Sleep(200);
                DataTable dt = FetchData();

                if (dt != null && dt.Rows.Count > 0)
                {
                    DeleteRecord(dt.Rows[0][0].ToString());

                    ProcessData(dt);
                }


            }

        }


        static void ProcessPartA(DataRow dr, LogItem itm)
        {
            string PartA = StripString(dr["PartA"].ToString());


            string datetime = PartA.Substring(0, PartA.IndexOf("]"));
            string remainingstr = PartA.Substring(PartA.IndexOf("]") + 2);

            itm.Date = DateTime.Parse(datetime.Substring(0, datetime.IndexOf(":")));

            itm.Time = DateTime.Parse(datetime.Substring(datetime.IndexOf(":") + 1, datetime.IndexOf("+") - datetime.IndexOf(":") - 2));

            string[] strs = remainingstr.Split(' ');

            itm.SourceIp = strs[1];

            itm.SourcePort = strs[2];
            itm.DestIp = strs[3];

            itm.DestPort = strs[4];


        }

        static void ProcessPartB(DataRow dr, LogItem itm)
        {

            string str = dr["PartB"].ToString();
            if (str.Length > 1)
            {

                if (str != "NULL")
                {

                    //GET / HTTP/1.1\nHost: 13.67.59.162:80\nConnection: Close\nAccept-Encoding: identity\n


                    string PartB = (str.Substring(str.IndexOf("\\n") + 2));

                    itm.Method = PartB.Substring(0, PartB.IndexOf(' '));

                    string rem = PartB.Substring(PartB.IndexOf(' ') + 1);

                    if (!String.IsNullOrEmpty(rem))
                    {

                        if (rem.Contains(' '))
                        {
                            itm.Url = rem.Substring(0, rem.IndexOf(' '));



                            string rem2 = rem.Substring(rem.IndexOf(' '));


                            if (!String.IsNullOrEmpty(rem2))
                            {


                                string[] rems = rem2.Split(new string[] { "\\n" }, StringSplitOptions.None);

                                foreach (string r in rems)
                                {
                                    if (r.Contains("Host:"))
                                    {
                                        itm.Host = r.Split(':')[1].Trim();
                                    }

                                    if (r.Contains("User-Agent:"))
                                    {
                                        itm.UserAgent = r.Split(':')[1].Trim();
                                    }
                                }
                            }
                        }
                    }

                    //itm.UserAgent = rems[3].Substring(rems[3].IndexOf(':') + 2);
                    //itm.Host = rems[4].Split(':')[1].Trim();
                }
            }
        }

        static void ProcessPartC(DataRow dr, LogItem itm)
        {
            string str = dr["PartC"].ToString();

            if (str != "NULL")
            {
                str = (str.Substring(str.IndexOf("\\n") + 2));
            }

            itm.PostData = str;

        }


        static void ProcessPartF(DataRow dr, LogItem itm)
        {

            //HTTP/1.1 404 Not Found\nX-Powered-By: PHP/5.4.45\nExpires: Wed, 11 Jan 1984 05:00:00 GMT\nCache-Control: no-cache, must-revalidate, max-age=0\nLink: <http://kaushalbhavsar.com/wp-json/>; rel="https://api.w.org/"\nContent-Type: text/html; charset=UTF-8\nKeep-Alive: timeout=5, max=100\nConnection: Keep-Alive\nTransfer-Encoding: chunked\n
            string str = dr["PartF"].ToString();

            if ((str.Length > 1))
            {
                if ((str != "NULL"))
                {
                    str = (str.Substring(str.IndexOf("\\n") + 2));
                }
                if ((str.Length > 1))
                {

                    itm.ResponseCode = str.Substring(str.IndexOf(' '), str.IndexOf("\\n") - str.IndexOf(' '));
                }
            }
        }

        static void ProcessPartH(DataRow dr, LogItem itm)
        {
            //Apache-Handler: proxy-server\nStopwatch: 1482259106563503 1369288 (- - -)\nStopwatch2: 1482259106563503 1369288; combined=1047, p1=276, p2=400, p3=61, p4=106, p5=133, sr=42, sw=71, l=0, gc=0\nResponse-Body-Transformed: Dechunked\nProducer: ModSecurity for Apache/2.9.0 (http://www.modsecurity.org/); OWASP_CRS/3.0.0.\nServer: Apache/2.4.18 (Ubuntu)\nEngine-Mode: "ENABLED"\n
            //Message: Warning. detected SQLi using libinjection with fingerprint 'n(f()' [file "/usr/share/modsecurity-crs/owasp-modsecurity-crs/rules/REQUEST-942-APPLICATION-ATTACK-SQLI.conf"] [line "68"] [id "942100"] [rev "1"] [msg "SQL Injection Attack Detected via libinjection"] [data "Matched Data: n(f() found within ARGS:e44e: die(pi());"] [severity "CRITICAL"] [ver "OWASP_CRS/3.0.0"] [maturity "1"] [accuracy "8"] [tag "application-multi"] [tag "language-multi"] [tag "platform-multi"] [tag "attack-sqli"] [tag "OWASP_CRS/WEB_ATTACK/SQL_INJECTION"] [tag "WASCTC/WASC-19"] [tag "OWASP_TOP_10/A1"] [tag "OWASP_AppSensor/CIE1"] [tag "PCI/6.5.2"]\nMessage: Access denied with code 403 (phase 2). Operator GE matched 5 at TX:anomaly_score. [file "/usr/share/modsecurity-crs/owasp-modsecurity-crs/rules/REQUEST-949-BLOCKING-EVALUATION.conf"] [line "57"] [id "949110"] [msg "Inbound Anomaly Score Exceeded (Total Score: 5)"] [severity "CRITICAL"] [tag "application-multi"] [tag "language-multi"] [tag "platform-multi"] [tag "attack-generic"]\nMessage: Warning. Operator GE matched 5 at TX:inbound_anomaly_score. [file "/usr/share/modsecurity-crs/owasp-modsecurity-crs/rules/RESPONSE-980-CORRELATION.conf"] [line "73"] [id "980130"] [msg "Inbound Anomaly Score Exceeded (Total Inbound Score: 5 - SQLI=5,XSS=0,RFI=0,LFI=0,RCE=0,PHPI=0,HTTP=0,SESS=0): SQL Injection Attack Detected via libinjection"] [tag "event-correlation"]\nAction: Intercepted (phase 2)\nApache-Handler: proxy-server\nStopwatch: 1482289540172661 1576 (- - -)\nStopwatch2: 1482289540172661 1576; combined=1216, p1=299, p2=745, p3=0, p4=0, p5=145, sr=43, sw=27, l=0, gc=0\nResponse-Body-Transformed: Dechunked\nProducer: ModSecurity for Apache/2.9.0 (http://www.modsecurity.org/); OWASP_CRS/3.0.0.\nServer: Apache/2.4.18 (Ubuntu)\nEngine-Mode: "ENABLED"\n

            string str = dr["PartH"].ToString();

            if (str != "NULL")
            {
                str = (str.Substring(str.IndexOf("\\n") + 2));

                if (str.Contains("id \""))
                {

                    try
                    {
                        itm.AttackId = Convert.ToInt32(str.Substring(str.IndexOf("id \"") + 4, 6));
                    }
                    catch
                    {
                        itm.AttackId = 0;
                    }
                }

            }
        }

        static void ProcessLocation(LogItem itm)
        {
            using (var reader = new DatabaseReader("GeoLite2-City.mmdb"))
            {
                try
                {
                    // Replace "City" with the appropriate method for your database, e.g.,
                    // "Country".
                    var city = reader.City(itm.SourceIp);

                    itm.IpDetails = JsonConvert.SerializeObject(city);
                    //Console.WriteLine(city.Country.IsoCode); // 'US'
                    //Console.WriteLine(city.Country.Name); // 'United States'

                    //Console.WriteLine(city.MostSpecificSubdivision.Name); // 'Minnesota'
                    //Console.WriteLine(city.MostSpecificSubdivision.IsoCode); // 'MN'

                    //Console.WriteLine(city.City.Name); // 'Minneapolis'

                    //Console.WriteLine(city.Postal.Code); // '55455'

                    //Console.WriteLine(city.Location.Latitude); // 44.9733
                    //Console.WriteLine(city.Location.Longitude); // -93.2323
                }
                catch (Exception ex)
                {
                    itm.IpDetails = "NULL";

                }
            }
        }


        static void ProcessData(DataTable dt)
        {


            //MongoCredential mongoCredential = MongoCredential.CreateCredential("bugshield",
            //                    "bugskan", "H@rdH@xx0127");


            //MongoClientSettings mongoClientSettings = new MongoClientSettings
            //{
            //    Credentials = new[] { mongoCredential },
            //    Server = new MongoServerAddress("52.163.253.181", 27017)
            //};


            //_client = new MongoClient(mongoClientSettings);


            //_database = _client.GetDatabase("bugshield");
            foreach (DataRow dr in dt.Rows)
            {

                LogItem itm = new LogItem();

                ProcessPartA(dr, itm);

                ProcessPartB(dr, itm);

                ProcessPartC(dr, itm);

                ProcessPartF(dr, itm);

                ProcessPartH(dr, itm);

                ProcessLocation(itm);
                if (itm.Url == null)
                    itm.Url = "NULL";
                if (itm.Host == null)
                    itm.Host = "NULL";
                if (itm.UserAgent == null)
                    itm.UserAgent = "NULL";
                if (itm.PostData == null)
                    itm.PostData = "NULL";
                if (itm.ResponseCode == null)
                    itm.ResponseCode = "NULL";
                if (itm.IpDetails == null)
                    itm.IpDetails = "NULL";

                StoreData(itm, dt.Rows.IndexOf(dr));

                BuildNotification(itm);

                //var itm1 = new BsonDocument
                //{

                //    { "Date",itm.Date},
                //    { "Time",itm.Time},
                //    { "SourceIp",itm.SourceIp},
                //    { "SourcePort",itm.SourcePort},
                //    { "DestIp",itm.DestIp},
                //    { "DestPort",itm.DestPort},
                //    { "Method",itm.Method},
                //    { "Url",itm.Url},
                //    { "Host",itm.Host},
                //    { "UserAgent",itm.UserAgent},
                //    { "PostData",itm.PostData},
                //    { "ResponseCode",itm.ResponseCode},
                //    { "AttackId",itm.AttackId},
                //    { "IpInfo",itm.IpDetails}

                //        };
                //var collection = _database.GetCollection<BsonDocument>("ShieldLogData");
                // collection.InsertOne(itm1);








                //Console.Clear();
                //Console.Write("Processed {0} records", dt.Rows.IndexOf(dr));

                //using (var csv = new CsvWriter(new StreamWriter("file.csv", true)))
                //{
                //    csv.Configuration.HasHeaderRecord = false;
                //    csv.Configuration.Delimiter = ",";
                //    csv.WriteRecord(itm);
                //}
            }
        }

        private static bool BuildNotification(LogItem itm)
        {
            string MyConnection = "data source=52.163.253.181;initial catalog=BugSkanDB;user id=bugskan;password=H@rdH@xx0127;MultipleActiveResultSets=True;";

            SqlConnection conn = new SqlConnection(MyConnection);

            SqlCommand cmd;

            conn.Open();


            try
            {


                cmd = conn.CreateCommand();
                cmd.CommandText = "InsertNotification";
                cmd.CommandType = CommandType.StoredProcedure;


                cmd.Parameters.AddWithValue("@EventId", itm.Id);
                cmd.Parameters.AddWithValue("@notificationDate", (itm.Date) + itm.Time.TimeOfDay);


                cmd.ExecuteNonQuery();

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Failed :(" + Environment.NewLine + ex.Message);

                return false;
            }

            finally
            {
                conn.Close();
            }

        }

        static string StripString(string str)
        {
            return str.Substring(str.IndexOf("\\n") + 3);
        }


        static bool StoreData(LogItem itm, int id)
        {
            string MyConnection = "data source=52.163.253.181;initial catalog=BugSkanDB;user id=bugskan;password=H@rdH@xx0127;MultipleActiveResultSets=True;";

            SqlConnection conn = new SqlConnection(MyConnection);

            SqlCommand cmd;

            conn.Open();


            try
            {

               
                cmd = conn.CreateCommand();
                cmd.CommandText = "InsertLog";
                cmd.CommandType = CommandType.StoredProcedure;


                cmd.Parameters.AddWithValue("@id", itm.Id);
                cmd.Parameters.AddWithValue("@Date", itm.Date);

                cmd.Parameters.AddWithValue("@Time", itm.Time);

                cmd.Parameters.AddWithValue("@SourceIp", itm.SourceIp);
                cmd.Parameters.AddWithValue("@SourcePort", itm.SourcePort);
                cmd.Parameters.AddWithValue("@DestIp", itm.DestIp);
                cmd.Parameters.AddWithValue("@DestPort", itm.DestPort);
                cmd.Parameters.AddWithValue("@Method", itm.Method);
                cmd.Parameters.AddWithValue("@Url", itm.Url);
                cmd.Parameters.AddWithValue("@Host", itm.Host);
                cmd.Parameters.AddWithValue("@UserAgent", itm.UserAgent);
                cmd.Parameters.AddWithValue("@PostData", itm.PostData);
                cmd.Parameters.AddWithValue("@ResponseCode", itm.ResponseCode);
                cmd.Parameters.AddWithValue("@AttackId", itm.AttackId);
                cmd.Parameters.AddWithValue("@IpInfo", itm.IpDetails);

                Console.WriteLine("{0} > Attack from {1} on {2}{3}  \nPostdata: {4} \nUserAgent: {5}", (itm.Date + itm.Time.TimeOfDay), itm.SourceIp, itm.Host, itm.Url, itm.PostData, itm.UserAgent);

                cmd.ExecuteNonQuery();

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Failed :(" + Environment.NewLine + ex.Message);

                return false;
            }

            finally
            {
                conn.Close();
            }
        }


        static Boolean DeleteRecord(string id)
        {
            string MyConnection = "Server=52.163.253.181; Database=mlogc; Uid=bugskan;Pwd=H@rdH@xx0127";

            MySqlConnection conn = new MySqlConnection(MyConnection);

            MySqlCommand cmd;

            conn.Open();


            try
            {
                Console.WriteLine("Removing record with id {0}", id);
                cmd = conn.CreateCommand();

                cmd.CommandText = "Delete from Data where Id=" + id;

                cmd.ExecuteNonQuery();


                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Failed :(");
                return false;
            }
            finally
            {
                conn.Close();
            }
        }

        static DataTable FetchData()
        {
            string MyConnection = "Server=52.163.253.181; Database=mlogc; Uid=bugskan;Pwd=H@rdH@xx0127";

            MySqlConnection conn = new MySqlConnection(MyConnection);

            MySqlCommand cmd;




            try
            {
                conn.Open();
                Console.Write(".");
                cmd = conn.CreateCommand();



                cmd.CommandText = "Select * from Data order by Id desc limit 1";
                MySqlDataAdapter adp = new MySqlDataAdapter(cmd);
                DataSet ds = new DataSet();
                adp.Fill(ds);



                return ds.Tables[0];
            }
            catch (Exception ex)
            {
                Console.WriteLine("Failed : " + ex.Message);

                return null;
            }

            finally
            {
                conn.Close();
            }
        }
    }
}
