using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Text;


using System.Collections.Generic;
using System.Linq;

using System.Threading.Tasks;
using System.Data.SqlClient;
using System.Data;
using System.Globalization;
using System.Text.RegularExpressions;

namespace BUGSKAN.Consumer
{
    class Program
    {
        static void Main(string[] args)
        {
            var factory = new ConnectionFactory() { HostName = "localhost" , Port = 5672, UserName="bugskan", Password="H@rdH@xx0127"};
            using (var connection = factory.CreateConnection())
            using (var channel = connection.CreateModel())
            {
                channel.QueueDeclare(queue: "bugskan-log",
                                     durable: false,
                                     exclusive: false,
                                     autoDelete: false,
                                     arguments: null);

                var consumer = new EventingBasicConsumer(channel);
                consumer.Received += (model, ea) =>
                {
                    var body = ea.Body;
                    var message = Encoding.UTF8.GetString(body);
                    Console.WriteLine(" [x] Received {0}", message);

                    string MyConnection = "data source=localhost;initial catalog=BugSkanDB;user id=bugskan;password=H@rdH@xx0127;MultipleActiveResultSets=True;";

                    SqlConnection conn = new SqlConnection(MyConnection);

                    SqlCommand cmd;

                    conn.Open();


                    try
                    {
                        string url = message.Substring(message.IndexOf("host") + 7, message.IndexOf("request") - message.IndexOf("host") - 11);

                        string t = message.Substring(message.IndexOf("time") + 8, message.IndexOf("remoteIP") - message.IndexOf("time") - 13);
                        var regex = new Regex(Regex.Escape(":"));
                        string t1 = regex.Replace(t, " ", 1);

                        string timeStamp = DateTime.Parse(t1.Trim()).ToString("MM/dd/yyyy hh:mm:ss tt", CultureInfo.InvariantCulture);

                        string logDetails = message.Replace("[" + t + "]", "").Replace("\"host\":\"", "").Replace("\"time\":\"", "").Replace("\", \"remoteIP", "\"remoteIP").Replace("\", \"request\":\"", "\"request\":\"");
                        var regex1 = new Regex(Regex.Escape("" + url + ""));
                        logDetails = regex1.Replace(logDetails, "", 1);

                        Console.WriteLine("Inserting record {0}");
                        cmd = conn.CreateCommand();
                        cmd.CommandText = "InsertLogFromRabbit";
                        cmd.CommandType = CommandType.StoredProcedure;


                       cmd.Parameters.AddWithValue("@LogDetails",logDetails);
                        cmd.Parameters.AddWithValue("@Url",url);
                       cmd.Parameters.AddWithValue("@TimeStamp", timeStamp);
                       cmd.ExecuteNonQuery();

                      //  return true;
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Failed :(" + Environment.NewLine + ex.Message);

                      //  return false;
                    }

                    finally
                    {
                        Console.WriteLine("{0} record inserted ");
                        conn.Close();
                    }

                   
                };
                channel.BasicConsume(queue: "bugskan-log",
                                     noAck: true,
                                     consumer: consumer);

                Console.WriteLine(" Press [enter] to exit.");
                Console.ReadLine();
            }
        }
    }
}
