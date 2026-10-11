using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin;
using Microsoft.Owin.Security;
using BUGSKAN.Portal.Models;
using SendGrid;
using SendGrid.Helpers.Mail;
using System.Net.Mail;
using System.Net.Mime;
using System.IO;
using System.Web.Mvc;

namespace BUGSKAN.Portal
{
    public class EmailService : IIdentityMessageService
    {
        public async Task SendAsync(IdentityMessage message)
        {

            string apiKey = Properties.Settings.Default.SendGridAPIKeyForVari;
            Environment.GetEnvironmentVariable("NAME_OF_THE_ENVIRONMENT_VARIABLE_FOR_YOUR_SENDGRID_KEY", EnvironmentVariableTarget.User);
          //  dynamic sg = await SendGridAPIClient(apiKey);
            var sg = new SendGridClient(apiKey);
            var from = new EmailAddress("drashtiarunpatel@gmail.com", "BUGSKAN");
            var subject = message.Body;
            var to = new EmailAddress(message.Destination);
           // var to = ViewBag.EmailDestination;
            var content = new Content("text/html", "").ToString();
            Random rnd = new Random();
           
            int myRandomNo;


            myRandomNo = rnd.Next(10000000, 99999999);
            //var contents = @"<html><head>trial</head><body>zksfnj</body></html>";
            //<html>

            //<head>
            //  <title></title>
            //</head>

            //                      <body>
            //<table class=body-wrap>
            //	<tbody>
            //		<tr>
            //			<td>&nbsp;</td>
            //			<td class=container width=600>
            //			<div class=content>
            //			<table cellpadding = 0 cellspacing=0 class=main width=100%>
            //				<tbody>
            //					<tr>
            //						<td class=content-wrap>
            //						<table cellpadding = 0 cellspacing=0 width=100%>
            //							<tbody>
            //								<tr>
            //									<td>&nbsp;</td>
            //								</tr>
            //								<tr>
            //									<td>&nbsp;</td>
            //								</tr>
            //								<tr>
            //									<td align = center >< span class=sg-image data-imagelibrary=%7B%22width%22%3A%22262%22%2C%22height%22%3A%2285%22%2C%22alt_text%22%3A%22%22%2C%22alignment%22%3A%22%22%2C%22border%22%3A0%2C%22src%22%3A%22https%3A//marketing-image-production.s3.amazonaws.com/uploads/f4a734f135f0c26802caf2cce6a99c4e56d2c3e12d5a0761729010b1f9439ef430605d0fe901b4eced0cf483958f86a3e9160a7c4c81a37ab9dcf76e6cdee036.png%22%2C%22classes%22%3A%7B%22sg-image%22%3A1%7D%2C%22link%22%3A%22%22%7D><img alt = bugskan class=logo height=85 src=https://marketing-image-production.s3.amazonaws.com/uploads/f4a734f135f0c26802caf2cce6a99c4e56d2c3e12d5a0761729010b1f9439ef430605d0fe901b4eced0cf483958f86a3e9160a7c4c81a37ab9dcf76e6cdee036.png style=width: 262px; height: 85px; width=262 /></span></td>
            //								</tr>
            //								<tr>
            //									<td>&nbsp;</td>
            //								</tr>
            //								<tr>
            //									<td>&nbsp;</td>
            //								</tr>
            //								<tr>
            //									<td>&nbsp;</td>
            //								</tr>
            //								<tr>
            //									<td class=content-block>Please confirm your email address by clicking the link below.</td>
            //								</tr>
            //								<tr>
            //									<td>&nbsp;</td>
            //								</tr>
            //								<tr>
            //									<td>&nbsp;</td>
            //								</tr>
            //								<tr>
            //									<td class=content-block>We may need to send you critical information about our service and it is important that we have an accurate email address.</td>
            //								</tr>
            //								<tr>
            //									<td>&nbsp;</td>
            //								</tr>
            //								<tr>
            //									<td class=content-block>If you are receiving this email and you have not registered an account on BUGSKAN, you can safely ignore this message.</td>
            //								</tr>
            //								<tr>
            //									<td>&nbsp;</td>
            //								</tr>
            //								<tr>
            //									<td>&nbsp;</td>
            //								</tr>
            //								<tr>
            //									<td align = center >< a href=http://localhost:2975/Account/Register style=  text-decoration: none;  color: #FFF;  background-color: #d40000;  border: solid #d40000;  border-width: 10px 20px;  line-height: 2;  font-weight: bold;  text-align: center;  cursor: pointer;  display: inline-block;  border-radius: 5px;  text-transform: capitalize; > Confirm email address</a></td>
            //								</tr>
            //								<tr>
            //									<td>&nbsp;</td>
            //								</tr>
            //								<tr>
            //									<td>&nbsp;</td>
            //								</tr>
            //								<tr>
            //									<td class=content-block>&nbsp;Regards</td>
            //								</tr>
            //								<tr>
            //									<td class=content-block>&nbsp;BUGSKAN Team</td>
            //								</tr>
            //							</tbody>
            //						</table>
            //						</td>
            //					</tr>
            //				</tbody>
            //			</table>

            //			<div class=footer style=width: 100%;  clear: both;  color: #999;  padding: 20px; >
            //			<table width = 100% >

            //                <tbody>

            //                    <tr>
            //                    </tr >

            //                    <tr>

            //                    </t>

            //                    <tr>

            //                        <td class=aligncenter content-block>Follow<a href=http://twitter.com/bugskan style=color:#d40000;> @Bugskan</a> on Twitter.</td>
            //					</tr>
            //					<tr>
            //						<td style = direction:ltr;text-align:left 

            //                         <div style= text-align: center;> You are receiving this email on behalf of <a href = https://bugskan.com style= color:#d40000; > BUGSKAN & nbsp; Cyber Security Solutions LLP</a>.&nbsp;</div>

            //						<hr/>
            //						<hr/>.</td>
            //					</tr>
            //				</tbody>
            //			</table>
            //			</div>
            //			</div>
            //			</td>
            //			<td>&nbsp;</td>
            //		</tr>
            //	</tbody>
            //</table>
            //</div>
            //</body>
            //</html>";
            var contents = string.Format(@" 

<head>
  <title></title>
</head>
          
                      <body>
<table class=body-wrap>
	<tbody>
		<tr>
			<td>&nbsp;</td>
			<td class=container width=600>
			<div class=content>
			<table cellpadding = 0 cellspacing=0 class=main width=100%>
				<tbody>
					<tr>
						<td class=content-wrap>
						<table cellpadding = 0 cellspacing=0 width=100%>
							<tbody>
								<tr>
									<td>&nbsp;</td>
								</tr>
								<tr>
									<td>&nbsp;</td>
								</tr>
								<tr>
									<td align = center><span class=sg-image data-imagelibrary=%7B%22width%22%3A%22262%22%2C%22height%22%3A%2285%22%2C%22alt_text%22%3A%22%22%2C%22alignment%22%3A%22%22%2C%22border%22%3A0%2C%22src%22%3A%22https%3A//marketing-image-production.s3.amazonaws.com/uploads/f4a734f135f0c26802caf2cce6a99c4e56d2c3e12d5a0761729010b1f9439ef430605d0fe901b4eced0cf483958f86a3e9160a7c4c81a37ab9dcf76e6cdee036.png%22%2C%22classes%22%3A%7B%22sg-image%22%3A1%7D%2C%22link%22%3A%22%22%7D><img alt=bugskan class=logo height=85 src=https://marketing-image-production.s3.amazonaws.com/uploads/f4a734f135f0c26802caf2cce6a99c4e56d2c3e12d5a0761729010b1f9439ef430605d0fe901b4eced0cf483958f86a3e9160a7c4c81a37ab9dcf76e6cdee036.png style=width: 262px; height: 85px; width=262/></span></td>
								</tr>
								<tr>
									<td>&nbsp;</td>
								</tr>
								<tr>
									<td>&nbsp;</td>
								</tr>
								<tr>
									<td>&nbsp;</td>
								</tr>
								<tr>
									<td class=content-block>Please confirm your email address by clicking the link below. Enter code:" + myRandomNo.ToString() + @"</td>
								</tr>
								<tr>
									<td>&nbsp;</td>
								</tr>
								<tr>
									<td>&nbsp;</td>
								</tr>
								<tr>
									<td class=content-block>We may need to send you critical information about our service and it is important that we have an accurate email address.</td>
								</tr>
								<tr>
									<td>&nbsp;</td>
								</tr>
								<tr>
									<td class=content-block>If you are receiving this email and you have not registered an account on BUGSKAN, you can safely ignore this message.</td>
								</tr>
								<tr>
									<td>&nbsp;</td>
								</tr>
								<tr>
									<td>&nbsp;</td>
								</tr>
								<tr>
									<td align=center ><a href=http://localhost:2975/Account/ConfirmEmail?userId= + style=text-decoration: none;  color: #FFF;  background-color: #d40000;  border: solid #d40000;  border-width: 10px 20px;  line-height: 2;  font-weight: bold;  text-align: center;  cursor: pointer;  display: inline-block;  border-radius: 5px;  text-transform: capitalize;> Confirm email address</a></td>
								</tr>
								<tr>
									<td>&nbsp;</td>
								</tr>
								<tr>
									<td>&nbsp;</td>
								</tr>
								<tr>
									<td class=content-block>&nbsp;Regards</td>
								</tr>
								<tr>
									<td class=content-block>&nbsp;BUGSKAN Team</td>
								</tr>
							</tbody>
						</table>
						</td>
					</tr>
				</tbody>
			</table>

			<div class=footer style=width: 100%;  clear: both;  color: #999;  padding: 20px; >
			<table width = 100% >

                <tbody>

                    <tr>
                    </tr >

                    <tr>

                    </t>

                    <tr>

                        <td class=aligncenter content-block>Follow<a href=http://twitter.com/bugskan style=color:#d40000;> @Bugskan</a> on Twitter.</td>
					</tr>
					<tr>
						<td style = direction:ltr;text-align:left 
 
                         <div style= text-align:center;> You are receiving this email on behalf of <a href = https://bugskan.com style= color:#d40000; > BUGSKAN&nbsp;Cyber Security Solutions LLP</a>.&nbsp;</div>

						<hr/>
						<hr/>.</td>
					</tr>
				</tbody>
			</table>
			</div>
			</div>
			</td>
			<td>&nbsp;</td>
		</tr>
	</tbody>
</table>
</div>
</body>
</html>");
            var mail = MailHelper.CreateSingleEmail(from, to, subject, null, contents);
         //  mail.TemplateId = "a109dc38-c168-42e0-bce9-54ab7ffd5197";
          
             
            var response = await sg.SendEmailAsync(mail);
            return;
            //  var signuptemplate = File.ReadAllText(System.IO.Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory, "SignUp.txt"));



            // signuptemplate = signuptemplate.Replace("-url-", message.Body.ToString());
            //Mail mail = new Mail(from, subject, to, content);









            Personalization p = new Personalization();



          //  p.Substitutions = new Dictionary<string, string>();
           // p.Substitutions.Add("-url-", message.Body);
            //
           // mail.Personalizations[0].Substitutions.Add("-url-", message.Body.ToString());

          
            //var response = await sg.client.mail.send.post(requestBody: mail.Get());
           

        }
    }


    public class SmsService : IIdentityMessageService
    {
        public Task SendAsync(IdentityMessage message)
        {
            // Plug in your SMS service here to send a text message.
            return Task.FromResult(0);
        }
    }

    // Configure the application user manager used in this application. UserManager is defined in ASP.NET Identity and is used by the application.
    public class ApplicationUserManager : UserManager<ApplicationUser>
    {
        public ApplicationUserManager(IUserStore<ApplicationUser> store)
            : base(store)
        {
        }

        public static ApplicationUserManager Create(IdentityFactoryOptions<ApplicationUserManager> options, IOwinContext context)
        {
            var manager = new ApplicationUserManager(new UserStore<ApplicationUser>(context.Get<ApplicationDbContext>()));
            // Configure validation logic for usernames
            manager.UserValidator = new UserValidator<ApplicationUser>(manager)
            {
                AllowOnlyAlphanumericUserNames = false,
                RequireUniqueEmail = true
            };

            // Configure validation logic for passwords
            manager.PasswordValidator = new PasswordValidator
            {
                RequiredLength = 6,
                RequireNonLetterOrDigit = true,
                RequireDigit = true,
                RequireLowercase = true,
                RequireUppercase = true,
            };

            // Configure user lockout defaults
            manager.UserLockoutEnabledByDefault = true;
            manager.DefaultAccountLockoutTimeSpan = TimeSpan.FromMinutes(5);
            manager.MaxFailedAccessAttemptsBeforeLockout = 5;

            // Register two factor authentication providers. This application uses Phone and Emails as a step of receiving a code for verifying the user
            // You can write your own provider and plug it in here.
            manager.RegisterTwoFactorProvider("Phone Code", new PhoneNumberTokenProvider<ApplicationUser>
            {
                MessageFormat = "Your security code is {0}"
            });
            manager.RegisterTwoFactorProvider("Email Code", new EmailTokenProvider<ApplicationUser>
            {
                Subject = "Security Code",
                BodyFormat = "Your security code is {0}"
            });
            manager.EmailService = new EmailService();
            manager.SmsService = new SmsService();
            var dataProtectionProvider = options.DataProtectionProvider;
            if (dataProtectionProvider != null)
            {
                manager.UserTokenProvider =
                    new DataProtectorTokenProvider<ApplicationUser>(dataProtectionProvider.Create("ASP.NET Identity"));
            }
            return manager;
        }
    }

    // Configure the application sign-in manager which is used in this application.
    public class ApplicationSignInManager : SignInManager<ApplicationUser, string>
    {
        public ApplicationSignInManager(ApplicationUserManager userManager, IAuthenticationManager authenticationManager)
            : base(userManager, authenticationManager)
        {
        }

        public override Task<ClaimsIdentity> CreateUserIdentityAsync(ApplicationUser user)
        {
            return user.GenerateUserIdentityAsync((ApplicationUserManager)UserManager);
        }

        public static ApplicationSignInManager Create(IdentityFactoryOptions<ApplicationSignInManager> options, IOwinContext context)
        {
            return new ApplicationSignInManager(context.GetUserManager<ApplicationUserManager>(), context.Authentication);
        }
    }
}
