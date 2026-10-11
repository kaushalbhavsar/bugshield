using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SendGrid;
using SendGrid.CSharp;
using SendGrid.Helpers;
using SendGrid.SmtpApi;
using Microsoft.AspNet.Identity;
using System.Net.Http;
using System.Net.Mail;
using SendGrid.CSharp.HTTP;
using SendGrid.Helpers.Mail;
using System.Web.Mvc;

namespace SendGridForEmail
{
    class Program
    {
        public object Properties { get; private set; }

        static void Main()
        {
           
            Console.WriteLine("Enter Email");
            string email = Console.ReadLine().ToString();
            Console.WriteLine("Enter Username");
            string uname = Console.ReadLine().ToString();
            Console.WriteLine("enter 1 for single site, enter 2 for multiple sites");
            string siteno = Console.ReadLine().ToString();
            Console.WriteLine("Enter Website");
            string website = Console.ReadLine().ToString();
            Console.WriteLine("Enter URL");
            string url = Console.ReadLine().ToString();
            mailfun(email,uname, siteno,website,url).Wait();
          
        }
            static async Task mailfun(string email, string uname, string siteno, string webs, string url )
        {
            string con;
            //  string apiKey = Properties.Settings.Default.SendGridAPIKey;
            string apiKey = "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";
            Environment.GetEnvironmentVariable("NAME_OF_THE_ENVIRONMENT_VARIABLE_FOR_YOUR_SENDGRID_KEY", EnvironmentVariableTarget.User);
            //  dynamic sg = await SendGridAPIClient(apiKey);
            var sg = new SendGridClient(apiKey);
            var from = new EmailAddress("ankit@bugskan.com", "BUGSKAN");
            var subject = "Website Hacked";
            var to = new EmailAddress(email);
            var content = new Content("text/html", "").ToString();
            if (siteno == "1")
            {
                 con = @"<html>

<head>
  <title></title>
</head>

<body>
  <div><font size=3.5 color=black face=calibri>Dear " + uname + @"
    <p>My name is Ankit. I am from BUGSKAN Cyber Security. It seems that your website " + webs + " been hacked. Your website is been showing the signs of hacking on " + url + @"
    <p>We recommend you lookup into the issue and fix the website ASAP. Not fixing it may result in blacklisting of the domain on search engines and loss of customer trust. </p>
    <p>In case you need assistance in cleaning the hacked website, we can help you. Also we can give you a thorough analysis of:</p>
    <ol><li>How the website was hacked</li>
    <li>List of vulnerabilities in website/server that resulted in the website hacking </li>
    <li>Set of recommended suggestions that you should implement in order to fix this issue PERMANENTLY.</li></ol>
    <p>We'd love you to help you out. Looking forward to hearing from you.</p>
    <p><br>Thank you.
    <br>Ankit.
    <br>BUGSKAN Cyber Security Solutions LLP
      <br>https://bugskan.com
<br> Contact: +91 - 8140934414</p></font>
</ div >
</ body > 
</ html >";
            }
            else
            {  con = @"<html>

<head>
  <title></title>
</head>

<body>
  <div><font size=3.5 color=black face=calibri>Dear " + uname + @"
    <p>My name is Ankit. I am from BUGSKAN Cyber Security. It seems that your websites " + webs + " been hacked. Your websites has been showing the signs of hacking on " + url + @"
    <p>We recommend you lookup into the issue and fix the website ASAP. Not fixing it may result in blacklisting of the domain on search engines and loss of customer trust. </p>
    <p>In case you need assistance in cleaning the hacked website, we can help you. Also we can give you a thorough analysis of:</p>
    <ol><li>How the website was hacked</li>
    <li>List of vulnerabilities in website/server that resulted in the website hacking </li>
    <li>Set of recommended suggestions that you should implement in order to fix this issue PERMANENTLY.</li></ol>
    <p>We'd love you to help you out. Looking forward to hearing from you.</p>
    <p><br>Thank you.
    <br>Ankit.
    <br>BUGSKAN Cyber Security Solutions LLP
      <br>https://bugskan.com
<br> Contact: +91 - 8140934414</p></font>
</ div >
</ body > 
</ html >"; }
            
                
  //  string con = " Dear Mr. @\n My name is Ankit.I am from BUGSKAN Cyber Security.It seems that your website "+""+".in has been hacked.Your website is been showing the signs of hacking on "+"http://sachonline.in/index.php."+""+ "" + System.Environment.NewLine + " We recommend you lookup into the issue and fix the website ASAP.Not fixing it may result in blacklisting of the domain on search engines and loss of customer trust." + System.Environment.NewLine + " In case you need assistance in cleaning the hacked website, we can help you.Also we can give you a thorough analysis of:" + System.Environment.NewLine + " How the website was hacked\n List of vulnerabilities in website / server that resulted in the website hacking " + System.Environment.NewLine + " Set of recommended suggestions that you should implement in order to fix this issue PERMANENTLY." + System.Environment.NewLine + " We'd love you to help you out. Looking forward to hearing from you." + System.Environment.NewLine + " Thank you." + System.Environment.NewLine + " Ankit." + System.Environment.NewLine + " BUGSKAN Cyber Security Solutions LLP " + System.Environment.NewLine + " https://bugskan.com " + System.Environment.NewLine + " Contact: +91 - 8140934414 ";
               var mail = MailHelper.CreateSingleEmail(from, to, subject, null, con);
            //mail.TemplateId = "a109dc38-c168-42e0-bce9-54ab7ffd5197";
            var response = await sg.SendEmailAsync(mail);
            return;


            //   EmailAddress em =Console.ReadKey();
            //    SendAsync(email).Wait();
            //}
            //public async Task SendAsync(string email)
            //{
            // string apiKey = "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";
            // //string apiKey = Properties.Settings.Default.SendGridAPIKey;
            // Environment.GetEnvironmentVariable("NAME_OF_THE_ENVIRONMENT_VARIABLE_FOR_YOUR_SENDGRID_KEY", EnvironmentVariableTarget.User);
            // //dynamic sg = await SendGridAPIClient(apiKey);
            // var sg = new SendGridClient(apiKey);
            // var from = new EmailAddress("connect@bugskan.com", "BUGSKAN");
            // var subject = "Website Hacked";
            // var to = new EmailAddress(email);

            // var content = new Content("text/html", "").ToString();
            // var msg = MailHelper.CreateSingleEmail(from, to, subject, null,content);
            // var mail = MailHelper.CreateSingleEmail(from, to, subject, null, content);

            // msg.TemplateId = "3647b35d-ba33-4756-ad5d-f1a2e2eab515";
            // Personalization p = new Personalization();
            // p.Substitutions = new Dictionary<string, string>();
            // //p.Substitutions.Add("-url-", message.Body);

            //// mail.Personalizations[0].Substitutions.Add("-url-", message.Body.ToString());
            // var response = sg.SendEmailAsync(msg);
            // return;
            //var myMessage = new SendGrid.SendGridMessage();
            //myMessage.AddTo("test@sendgrid.com");
            //myMessage.From = new MailAddress("drashti@pratikar.com", "Drashti");
            //myMessage.Subject = "Website Hacked";
            //myMessage.Text = "and easy to do anywhere, even with C#";

            //var transportWeb = new SendGrid.Web("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");
            //transportWeb.DeliverAsync(myMessage).Wait();

            //    var apiKey = Environment.GetEnvironmentVariable("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");
            //var client = new SendGridClient(apiKey);
            //var from = new EmailAddress("connect@bugskan.com", "BUGSKAN");
            //var subject = "Website Hacked";
            //var to = new EmailAddress(email, null);
            ////  var plainTextContent = "and easy to do anywhere, even with C#";
            //var htmlContent = new Content("text/html", "").ToString();
            //var msg = MailHelper.CreateSingleEmail(from, to, subject, null, htmlContent);
            //var response = await client.SendEmailAsync(msg).ConfigureAwait(true);




        }
    }
}
