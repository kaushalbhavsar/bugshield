using System;
using System.Globalization;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;
using BUGSKAN.Portal.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using SendGrid;
using SendGrid.Helpers.Mail;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Configuration;

namespace BUGSKAN.Portal.Controllers
{
    [Authorize]
    public class AccountController : Controller
    {
        private BugSkanEntities db = new BugSkanEntities();
        private ApplicationSignInManager _signInManager;
        private ApplicationUserManager _userManager;
     
        public AccountController()
        {
        }

        public AccountController(ApplicationUserManager userManager, ApplicationSignInManager signInManager )
        {
            UserManager = userManager;
            SignInManager = signInManager;
        }

        public ApplicationSignInManager SignInManager
        {
            get
            {
                return _signInManager ?? HttpContext.GetOwinContext().Get<ApplicationSignInManager>();
            }
            private set 
            { 
                _signInManager = value; 
            }
        }

        public ApplicationUserManager UserManager
        {
            get
            {
                return _userManager ?? HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>();
            }
            private set
            {
                _userManager = value;
            }
        }

        //
        // GET: /Account/Login
        [AllowAnonymous]
        public ActionResult Login(string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        //
        // POST: /Account/Login
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Login(LoginViewModel model, string returnUrl)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // This doesn't count login failures towards account lockout
            // To enable password failures to trigger account lockout, change to shouldLockout: true
            var result = await SignInManager.PasswordSignInAsync(model.Email, model.Password, model.RememberMe, shouldLockout: false);
            switch (result)
            {
                case SignInStatus.Success:
                    using (var context = new BugSkanEntities())
                    {
                       
                        var userid1 = User.Identity.GetUserId();
                        var userid = (from u in context.AspNetUsers
                                           where u.Email == model.Email
                                           select u.Id).FirstOrDefault();
                        var founduserid = (from u in context.Sites
                                           where u.UserId == userid.ToString()
                                           select u.SiteId).FirstOrDefault();
                        bool ConfirmEmailCheck=(from u in context.AspNetUsers
                                               where u.Email==model.Email
                                               select u.EmailConfirmed).First();
                        //if (ConfirmEmailCheck == true)
                        //{
                            if (founduserid == Guid.Parse("00000000-0000-0000-0000-000000000000"))
                                return RedirectToAction("Create", "Sites");
                          //  else
                             //   return RedirectToLocal(returnUrl);
                       // }
                        else
                        {
                            return RedirectToAction("Index", "Home", new { Email = model.Email });
                        }
                    }
                  
                case SignInStatus.LockedOut:
                    return View("Lockout");
                case SignInStatus.RequiresVerification:
                    return RedirectToAction("SendCode", new { ReturnUrl = returnUrl, RememberMe = model.RememberMe });
                case SignInStatus.Failure:
                    {
                        ViewBag.LoginErrorMsg = "Invalid Email and Password combination";
                        return View(model);
                    }
                    
                default:
                    ModelState.AddModelError("", "Invalid login attempt.");
                    ViewBag.LoginErrorMsg = "Invalid Email and Password combination";
                    return View(model);
            }
        }

        //
        // GET: /Account/VerifyCode
        [AllowAnonymous]
        public async Task<ActionResult> VerifyCode(string provider, string returnUrl, bool rememberMe)
        {
            // Require that the user has already logged in via username/password or external login
            if (!await SignInManager.HasBeenVerifiedAsync())
            {
                return View("Error");
            }
            return View(new VerifyCodeViewModel { Provider = provider, ReturnUrl = returnUrl, RememberMe = rememberMe });
        }

        //
        // POST: /Account/VerifyCode
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> VerifyCode(VerifyCodeViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // The following code protects for brute force attacks against the two factor codes. 
            // If a user enters incorrect codes for a specified amount of time then the user account 
            // will be locked out for a specified amount of time. 
            // You can configure the account lockout settings in IdentityConfig
            var result = await SignInManager.TwoFactorSignInAsync(model.Provider, model.Code, isPersistent:  model.RememberMe, rememberBrowser: model.RememberBrowser);
            switch (result)
            {
                case SignInStatus.Success:
                    return RedirectToLocal(model.ReturnUrl);               
                case SignInStatus.LockedOut:
                    return View("Lockout");
                case SignInStatus.Failure:
                default:
                    ModelState.AddModelError("", "Invalid code.");
                    return View(model);
            }
        }

        //
        // GET: /Account/Register
        [AllowAnonymous]
        public ActionResult Register()
        {
            return View();
        }

        //
        // POST: /Account/Register
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Register(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = new ApplicationUser { UserName = model.Email, Email = model.Email };
                var result = await UserManager.CreateAsync(user, model.Password);
                if (result.Succeeded)
                {
                    await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);

                    // For more information on how to enable account confirmation and password reset please visit http://go.microsoft.com/fwlink/?LinkID=320771
                    // Send an email with this link
                    string code = await UserManager.GenerateEmailConfirmationTokenAsync(user.Id);
                    var callbackUrl = Url.Action("ConfirmEmail", "Account", new { userId = user.Id, email = user.Email }, protocol: Request.Url.Scheme);
                     //await UserManager.SendEmailAsync(user.Id, "Confirm your account", callbackUrl);
                    //await UserManager.SendEmailAsync();
                    // await SendEmailFunc();
                    AspNetUsersController aspuc = new AspNetUsersController();
                    ViewBag.GetUserId = User.Identity.GetUserId();
                   // AspNetUser aspNetUser = db.AspNetUsers.Find(User.Identity.GetUserId());
                        AspNetUser anuObj = new AspNetUser();
                        //anuObj.Id = user.Id;
                        //anuObj.TempConfCode = myRandomNo.ToString();
                        //aspuc.Edit(anuObj);
                   

                    ViewBag.EmailDestination = model.Email;
                    string apiKey = Properties.Settings.Default.SendGridAPIKeyForVari;
                   // string apiKey = "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";
                    Environment.GetEnvironmentVariable("NAME_OF_THE_ENVIRONMENT_VARIABLE_FOR_YOUR_SENDGRID_KEY", EnvironmentVariableTarget.User);
                    //  dynamic sg = await SendGridAPIClient(apiKey);
                    var sg = new SendGridClient(apiKey);
                    var from = new EmailAddress("drashtiarunpatel@gmail.com", "BUGSKAN");
                    var subject = "Confirm your BugSkan Account";
                  //  var to = ViewBag.EmailDestination;
                    var to = new EmailAddress(ViewBag.EmailDestination);
                    var content = new Content("text/html", "").ToString();
                    var contents = @" 

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
									<td class=content-block>Please confirm your email address by clicking the link below. </td>
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
									<td align=center ><a href="+callbackUrl+@" style=text-decoration: none;  color: #FFF;  background-color: #d40000;  border: solid #d40000;  border-width: 10px 20px;  line-height: 2;  font-weight: bold;  text-align: center;  cursor: pointer;  display: inline-block;  border-radius: 5px;  text-transform: capitalize;> Confirm email address</a></td>
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
</html>";
                    var mail = MailHelper.CreateSingleEmail(from, to, subject, null, contents);
                    var response = await sg.SendEmailAsync(mail);
                    SubscriptionsController sub = new SubscriptionsController();
                    Subscription sb = new Subscription();

                    ViewBag.GetUserId = user.Id.ToString();
                   sb.UserId = user.Id;
                
                    ActionResult res = sub.Create(sb); 
                    ViewBag.EmailErrorMsg = "Verification required. Please check your email for further instructions.";
                    return RedirectToAction("Confirm", "Account", new { Email = user.Email });

                }
                else
                {
                    ViewBag.EmailErrorMsg = "Email already registered";
                }
            }
            return View();
        }
        public ActionResult Confirm(string Email)
        {
            ViewBag.Email = Email; return View();
        }

        [HttpPost]
        public JsonResult IsAlreadySigned(string Email)
        {

            BugSkanEntities db = new BugSkanEntities();
        // Assume these details coming from database  

        var RegEmailId = (from u in db.AspNetUsers
                              where u.Email.ToUpper() == Email.ToUpper()
                              select new { u.Email }).FirstOrDefault();

            var status=true;
            if (RegEmailId != null)
            {
                //Already registered  
                return Json(status, JsonRequestBehavior.AllowGet);
            }
            else 
            //else
            //{
            //    //Available to use  
            //    status = true;
            //}

            return Json("Email exist", JsonRequestBehavior.AllowGet);
        }

        //
        // GET: /Account/ConfirmEmail
        [AllowAnonymous]
        public async Task<ActionResult> ConfirmEmail(string userId)
        {
            BugSkanEntities db = new BugSkanEntities();
            List<CheckEmailStatus_Result> d = new List<CheckEmailStatus_Result>();
            AspNetUsersController aspc = new AspNetUsersController();
            string query = "Update AspNetUsers set EmailConfirmed = 'true' where Id = {0}";
            int EmailConfirmUpdate = db.Database.ExecuteSqlCommand(query,userId);

            if (userId == null)
            {
                return View("Error");
            }
            else
            {
                return RedirectToAction("Create", "Sites");
               

            }
        }

        //
        // GET: /Account/ForgotPassword
        [AllowAnonymous]
        public ActionResult ForgotPassword()
        {
            return View();
        }

        //
        // POST: /Account/ForgotPassword
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ForgotPassword(ForgotPasswordViewModel model)
        {
            var RegEmailId = (from u in db.AspNetUsers
                              where u.Email.ToUpper() == model.Email.ToUpper()
                              select new { u.Email }).FirstOrDefault();
            if (RegEmailId != null) { 

            if (ModelState.IsValid)
            {

                using (var context = new BugSkanEntities())
                {
                    var founduserid = (from u in context.AspNetUsers
                                       where u.Email == model.Email
                                       select u.Id).FirstOrDefault();

                        if (founduserid != null)
                        {
                            var user1 = new ApplicationUser { Id = founduserid, Email = model.Email };

                            var user = await UserManager.FindByNameAsync(founduserid);
                            //if (user == null || !(await UserManager.IsEmailConfirmedAsync(user.Id)))
                            //{
                            //    // Don't reveal that the user does not exist or is not confirmed
                            //    return View("ForgotPasswordConfirmation");
                            //}
                            Environment.GetEnvironmentVariable("NAME_OF_THE_ENVIRONMENT_VARIABLE_FOR_YOUR_SENDGRID_KEY", EnvironmentVariableTarget.User);
                            //  dynamic sg = await SendGridAPIClient(apiKey);
                            ViewBag.ForgotPassUser = User.Identity.GetUserId();
                            //var id = user.Id;
                            string code = await UserManager.GeneratePasswordResetTokenAsync(user1.Id);
                            var callbackUrl = Url.Action("ResetPassword", "Account", new { userId = user1.Id, code = code, Email = user1.Email }, protocol: Request.Url.Scheme);
                            string apiKey = Properties.Settings.Default.SendGridAPIKeyForVari;
                            var sg = new SendGridClient(apiKey);
                            var from = new EmailAddress("drashtiarunpatel@gmail.com", "BUGSKAN");
                            var subject = "Reset your password";
                            var to = new EmailAddress(model.Email);
                            Session["UserEmailAddress"] = to.ToString();
                            var content = new Content("text/html", "").ToString();
                            // var code = await UserManager.GeneratePasswordResetTokenAsync(user.Id);
                            //var callbackUrl = Url.Action("ResetPassword", "Account",  new { UserId = user.Id, UEmail=user.Email}, protocol: Request.Url.Scheme);
                            // await UserManager.SendEmailAsync(user.Id, "Reset Password",
                            //"Please reset your password by clicking here: <a href=\"" + callbackUrl + "\">link</a>");
                            var contents = @" 

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
									<td class=content-block>Please reset your password by clicking the link below. </td>
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
									<td align=center ><a href=" + callbackUrl + @" style=text-decoration: none;  color: #FFF;  background-color: #d40000;  border: solid #d40000;  border-width: 10px 20px;  line-height: 2;  font-weight: bold;  text-align: center;  cursor: pointer;  display: inline-block;  border-radius: 5px;  text-transform: capitalize;> Reset Password </a></td>
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
</html>";
                            var mail = MailHelper.CreateSingleEmail(from, to, subject, null, contents);
                            var response = await sg.SendEmailAsync(mail);
                            ViewBag.MsgForForgotPass = "Please check your email to reset password";
                            return View();
                        }
                    }
                }
            }
            else
            {
                ViewBag.MsgForForgotPass = "Email not registered";


            }
            return View(model);
        }

          
           
        

        //
        // GET: /Account/ForgotPasswordConfirmation
        [AllowAnonymous]
        public ActionResult ForgotPasswordConfirmation()
        {
            return View();
        }

        //
        // GET: /Account/ResetPassword
        [AllowAnonymous]
        public ActionResult ResetPassword(string code, string Email)

        {
            Session["CodeForForgetPass"] = code;
           // var user = new ApplicationUser { UserName = model.Email, Email = model.Email };
             return code == null ? View("Error") : View();
           // return View();
        }

        //
        // POST: /Account/ResetPassword
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ResetPassword(ResetPasswordViewModel model)

        {
            model.Code = Session["CodeForForgetPass"].ToString();
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            
            // var user = await UserManager.FindByNameAsync(model.Email);
            using (var context = new BugSkanEntities())
            {
                var founduserid = (from u in context.AspNetUsers
                                   where u.Email == model.Email
                                   select u.Id).FirstOrDefault();

                if (founduserid != null)
                {
                    var user1 = new ApplicationUser { Id = founduserid, Email = model.Email };

                    var user = await UserManager.FindByNameAsync(founduserid);
                    // var user = new ApplicationUser { UserName = model.Email, Email = model.Email };
                    if (user1 == null)
                    {
                        // Don't reveal that the user does not exist
                        return RedirectToAction("ResetPasswordConfirmation", "Account");
                    }
                    var result = await UserManager.ResetPasswordAsync(user1.Id, model.Code, model.Password);
                    if (result.Succeeded)
                    {
                        return RedirectToAction("Login", "Account");
                        ViewBag.MsgForResetPass = "Your password has been changed. Please Login again";
                    }
                    AddErrors(result);
                }
            }
            return View();
        }

        //
        // GET: /Account/ResetPasswordConfirmation
        [AllowAnonymous]
        public ActionResult ResetPasswordConfirmation()
        {
            return View();
        }

        //
        // POST: /Account/ExternalLogin
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult ExternalLogin(string provider, string returnUrl)
        {
            // Request a redirect to the external login provider
            return new ChallengeResult(provider, Url.Action("ExternalLoginCallback", "Account", new { ReturnUrl = returnUrl }));
        }

        //
        // GET: /Account/SendCode
        [AllowAnonymous]
        public async Task<ActionResult> SendCode(string returnUrl, bool rememberMe)
        {
            var userId = await SignInManager.GetVerifiedUserIdAsync();
            if (userId == null)
            {
                return View("Error");
            }
            var userFactors = await UserManager.GetValidTwoFactorProvidersAsync(userId);
            var factorOptions = userFactors.Select(purpose => new SelectListItem { Text = purpose, Value = purpose }).ToList();
            return View(new SendCodeViewModel { Providers = factorOptions, ReturnUrl = returnUrl, RememberMe = rememberMe });
        }

        //
        // POST: /Account/SendCode
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> SendCode(SendCodeViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View();
            }

            // Generate the token and send it
            if (!await SignInManager.SendTwoFactorCodeAsync(model.SelectedProvider))
            {
                return View("Error");
            }
            return RedirectToAction("VerifyCode", new { Provider = model.SelectedProvider, ReturnUrl = model.ReturnUrl, RememberMe = model.RememberMe });
        }

        //
        // GET: /Account/ExternalLoginCallback
        [AllowAnonymous]
        public async Task<ActionResult> ExternalLoginCallback(string returnUrl)
        {
            var loginInfo = await AuthenticationManager.GetExternalLoginInfoAsync();
            if (loginInfo == null)
            {
                return RedirectToAction("Login");
            }

            // Sign in the user with this external login provider if the user already has a login
            var result = await SignInManager.ExternalSignInAsync(loginInfo, isPersistent: false);
            switch (result)
            {
                case SignInStatus.Success:
                    return RedirectToLocal(returnUrl);
                case SignInStatus.LockedOut:
                    return View("Lockout");
                case SignInStatus.RequiresVerification:
                    return RedirectToAction("SendCode", new { ReturnUrl = returnUrl, RememberMe = false });
                case SignInStatus.Failure:
                default:
                    // If the user does not have an account, then prompt the user to create an account
                    ViewBag.ReturnUrl = returnUrl;
                    ViewBag.LoginProvider = loginInfo.Login.LoginProvider;
                    return View("ExternalLoginConfirmation", new ExternalLoginConfirmationViewModel { Email = loginInfo.Email });
            }
        }

        //
        // POST: /Account/ExternalLoginConfirmation
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ExternalLoginConfirmation(ExternalLoginConfirmationViewModel model, string returnUrl)
        {
            if (User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "Manage");
            }

            if (ModelState.IsValid)
            {
                // Get the information about the user from the external login provider
                var info = await AuthenticationManager.GetExternalLoginInfoAsync();
                if (info == null)
                {
                    return View("ExternalLoginFailure");
                }
                var user = new ApplicationUser { UserName = model.Email, Email = model.Email };
                var result = await UserManager.CreateAsync(user);
                if (result.Succeeded)
                {
                    result = await UserManager.AddLoginAsync(user.Id, info.Login);
                    if (result.Succeeded)
                    {
                        await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);
                        return RedirectToLocal(returnUrl);
                    }
                }
                AddErrors(result);
            }

            ViewBag.ReturnUrl = returnUrl;
            return View(model);
        }


        [Authorize]
        public RedirectResult Logout()
        {
            //FormsAuthentication.SignOut();

            return this.Redirect("/");
        }

        //
        // POST: /Account/LogOff
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult LogOff()
        {
            AuthenticationManager.SignOut(DefaultAuthenticationTypes.ApplicationCookie);
            return RedirectToAction("Index", "Home");
        }
        //
        // GET: /Account/ExternalLoginFailure
        [AllowAnonymous]
        public ActionResult ExternalLoginFailure()
        {
            return View();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_userManager != null)
                {
                    _userManager.Dispose();
                    _userManager = null;
                }

                if (_signInManager != null)
                {
                    _signInManager.Dispose();
                    _signInManager = null;
                }
            }

            base.Dispose(disposing);
        }

        #region Helpers
        // Used for XSRF protection when adding external logins
        private const string XsrfKey = "XsrfId";

        private IAuthenticationManager AuthenticationManager
        {
            get
            {
                return HttpContext.GetOwinContext().Authentication;
            }
        }

        public object CheckEmailConfirmStatus_Result { get; private set; }

        private void AddErrors(IdentityResult result)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error);
            }
        }

        private ActionResult RedirectToLocal(string returnUrl)
        {
            if (Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            return RedirectToAction("Index", "Home");
        }

        internal class ChallengeResult : HttpUnauthorizedResult
        {
            public ChallengeResult(string provider, string redirectUri)
                : this(provider, redirectUri, null)
            {
            }

            public ChallengeResult(string provider, string redirectUri, string userId)
            {
                LoginProvider = provider;
                RedirectUri = redirectUri;
                UserId = userId;
            }

            public string LoginProvider { get; set; }
            public string RedirectUri { get; set; }
            public string UserId { get; set; }

            public override void ExecuteResult(ControllerContext context)
            {
                var properties = new AuthenticationProperties { RedirectUri = RedirectUri };
                if (UserId != null)
                {
                    properties.Dictionary[XsrfKey] = UserId;
                }
                context.HttpContext.GetOwinContext().Authentication.Challenge(properties, LoginProvider);
            }
        }
        #endregion
    }
}