using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using SendGrid;
using SendGrid.CSharp;
using SendGrid.Helpers;
using Microsoft.AspNet.Identity;
using System.Net.Http;
using System.Net.Mail;
using SendGrid.CSharp.HTTP;
using SendGrid.Helpers.Mail;

namespace SendGridWindApp
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void label3_Click(object sender, EventArgs e)
        {

        }

        private void btnSend_Click(object sender, EventArgs e)
        {
            try
            {
                string con;
                //  string apiKey = Properties.Settings.Default.SendGridAPIKey;
                string apiKey = "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";
                Environment.GetEnvironmentVariable("NAME_OF_THE_ENVIRONMENT_VARIABLE_FOR_YOUR_SENDGRID_KEY", EnvironmentVariableTarget.User);
                //  dynamic sg = await SendGridAPIClient(apiKey);
                var sg = new SendGridClient(apiKey);
                var from = new EmailAddress("ankit@bugskan.com", "BUGSKAN");
                var subject = "Website Hacked";
                var to = new EmailAddress(txtEmail.Text);
                var content = new Content("text/html", "").ToString();
                if (chkSingle.Checked)
                {
                    con = @"<html>

<head>
  <title></title>
</head>

<body>
  <div><font size=3.5 color=black face=calibri>Dear " + txtUsername.Text + @"
    <p>My name is Ankit. I am from BUGSKAN Cyber Security. It seems that your website " + txtWebsite.Text + " been hacked. Your website is been showing the signs of hacking on " + txtURL.Text + @"
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
                    var mail = MailHelper.CreateSingleEmail(from, to, subject, null, con);
                    var response = sg.SendEmailAsync(mail);
                    // MessageBox.Show("Email Sent");
                }
                else if (chkMultiple.Checked)
                {
                    con = @"<html>

<head>
  <title></title>
</head>

<body>
  <div><font size=3.5 color=black face=calibri>Dear " + txtUsername.Text + @"
    <p>My name is Ankit. I am from BUGSKAN Cyber Security. It seems that your websites " + txtWebsite.Text + " been hacked. Your websites has been showing the signs of hacking on " + txtURL.Text + @"
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
                    var mail = MailHelper.CreateSingleEmail(from, to, subject, null, con);
                    var response = sg.SendEmailAsync(mail);
                   // MessageBox.Show("Email Sent");

                }
                else
                {
                    MessageBox.Show("select no. of sites");
                }
                txtEmail.Text = "";
                txtURL.Text = "";
                txtWebsite.Text = "";
                txtURL.Text = "";
                txtUsername.Text = "";
                txtEmail.Focus();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Email not sent" + ex);
            }
        }
    }
}
