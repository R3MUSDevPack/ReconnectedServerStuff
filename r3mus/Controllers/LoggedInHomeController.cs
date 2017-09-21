using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using r3mus.Models;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using r3mus.Infrastructure;
using System.Text;
using System.IO;
using System.Web.Script.Serialization;
using System.Threading.Tasks;
using Teamspeak_Plugin;
using System.Data.Entity.Infrastructure;
using System.Net.Http;
using System.Net.Http.Headers;
using Newtonsoft.Json;
using System.Security.Cryptography;
using r3mus.ViewModels;
using System.Data.Entity.Validation;
using R3MUS.Devpack.Slack;
using r3mus.ActionFilters;

namespace r3mus.Controllers
{
    [Authorize] 
    public class LoggedInHomeController : AsyncController
    {
        private ApplicationDbContext db = new ApplicationDbContext();
        protected UserManager<ApplicationUser> UserManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(new ApplicationDbContext()));
        
        [UriInterceptFilter]
        // GET: /LoggedInHome/
        public ActionResult Index()
        {
            KeyValuePair<string, string> TSDetails = GetTSDetails();
            ViewBag.TSName = TSDetails.Key;

            var currentUser = UserManager.FindById(User.Identity.GetUserId());
            currentUser.LoadApiKeys();
            ViewBag.FullAPIAccessMask = Properties.Settings.Default.FullAPIAccessMask;

            if (TempData["Message"] != null)
            {
                ViewBag.Message = TempData["Message"].ToString();
            }
            if (TempData["ErrorMessage"] != null)
            {
                ViewBag.ErrorMessage = TempData["ErrorMessage"].ToString();
            }

            LatestNew latestNewsItem;
            List<Wardec> wardecs = new List<Wardec>();

            try
            {
                latestNewsItem = new ApplicationDbContext().LatestNewsItem.Where(newsItem => newsItem.Category == "Internal News").FirstOrDefault();
            }
            catch 
            {
                latestNewsItem = new LatestNew()
                {
                    Topic = "No news available",
                    Date = DateTime.Now,
                    Avatar = "No news available",
                    Post = "",
                    UserName = "HAL10000"
                };
            }
            try
            {
                wardecs = db.LiveWardecs.ToList();
            }
            catch { }

            var vm = new WelcomeViewModel()
            {
                LatestInternalNewsItem = latestNewsItem,
                Apis = currentUser.ApiKeys.GroupBy(api => api.ApiKey).Select(api => api.First()).ToList(),
                LiveWardecs = wardecs
            };
            
            return View(vm);
        }

        // GET: /LoggedInHome/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ApiInfo apiinfo = db.ApiInfoes.Find(id);
            if (apiinfo == null)
            {
                return HttpNotFound();
            }
            return View(apiinfo);
        }

        // GET: /LoggedInHome/Create
        [OverrideAuthorization]
        public ActionResult Create(string userId = "")
        {
            if (TempData["Message"] != null)
            {
                ViewBag.Message = TempData["Message"]; 
            }
            return View();
        }

        // POST: /LoggedInHome/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [OverrideAuthorization]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include="Id,ApiKey,VerificationCode")] ApiInfo apiinfo)
        {
            if (ModelState.IsValid)
            {
                UserManager<ApplicationUser> usrMgr = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(db));
                if (User.Identity.IsAuthenticated)
                {
                    usrMgr.FindById(User.Identity.GetUserId()).AddApiInfo(apiinfo.ApiKey.ToString(), apiinfo.VerificationCode);
                    apiinfo = usrMgr.FindById(User.Identity.GetUserId()).ApiKeys.Last();
                }
                else
                {
                    string userID = TempData["UserID"].ToString();
                    usrMgr.FindById(userID).AddApiInfo(apiinfo.ApiKey.ToString(), apiinfo.VerificationCode);
                    apiinfo = usrMgr.FindById(userID).ApiKeys.Last();
                }


                db.ApiInfoes.Add(apiinfo);
                db.SaveChanges();

                return RedirectToAction("Index");
            }

            return View(apiinfo);
        }

        // GET: /LoggedInHome/Edit/5
        public ActionResult Edit(int apiKey)
        {
            if (apiKey == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ApiInfo apiinfo = db.ApiInfoes.Find(apiKey);
            if (apiinfo == null)
            {
                return HttpNotFound();
            }
            return View(apiinfo);
        }

        // POST: /LoggedInHome/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include="Id,ApiKey,VerificationCode")] ApiInfo apiinfo)
        {
            if (ModelState.IsValid)
            {
                db.Entry(apiinfo).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(apiinfo);
        }

        // GET: /LoggedInHome/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ApiInfo apiinfo = db.ApiInfoes.Find(id);
            if (apiinfo == null)
            {
                return HttpNotFound();
            }
            return View(apiinfo);
        }

        // POST: /LoggedInHome/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            ApiInfo apiinfo = db.ApiInfoes.Find(id);
            db.ApiInfoes.Remove(apiinfo);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        public ActionResult RegisterForTeamspeak()
        {
            return View();
        }

        public KeyValuePair<string, string> GetTSDetails()
        {
            string lookupName = string.Empty;
            string groupName = string.Empty;

            if (UserManager.FindById(User.Identity.GetUserId()).MemberType == ApplicationUser.IDType.Corporation.ToString())
            {
                groupName = Properties.Settings.Default.TS_CorpGroup;
                lookupName = Properties.Settings.Default.CorpTicker;
            }
            else if (UserManager.FindById(User.Identity.GetUserId()).MemberType == ApplicationUser.IDType.Alliance.ToString())
            {
                groupName = Properties.Settings.Default.TS_AlliGroup;
                lookupName = Properties.Settings.Default.AllianceTicker;
            }
            else if (UserManager.FindById(User.Identity.GetUserId()).MemberType == ApplicationUser.IDType.SharedComms.ToString())
            {
                var user = UserManager.FindById(User.Identity.GetUserId());
                user.IsValid();
                groupName = user.CorpTicker;
                lookupName = user.CorpTicker;
            }
            lookupName = string.Concat("[", lookupName, "] ", User.Identity.Name);

            return new KeyValuePair<string, string>(lookupName, groupName);
        }

        [HttpPost]
        public async Task<ActionResult> RegisterForTeamspeak(int? eh)
        {
            KeyValuePair<string, string> TSDetails = GetTSDetails();

            Teamspeak TS_Plug = new Teamspeak();
            await TS_Plug.AddClient(TSDetails.Key, TSDetails.Value, Properties.Settings.Default.TSURL, Properties.Settings.Default.TS_Password);
            TempData.Add("Message", TS_Plug.Message);

            return RedirectToAction("Index");
        }

        public ActionResult RegisterForMoodle()
        {
            return View();
        }

        [HttpPost]
        public ActionResult RegisterForMoodle(int? eh)
        {
            string moodleURL = string.Concat("http://", Properties.Settings.Default.MoodleBaseURL, "/webservice/rest/server.php");
            string function = "core_user_create_users";
            string[] nameComponents = User.Identity.Name.Split(new Char[] { ' ' });

            try
            {
                MoodleUser mUser = new MoodleUser() { 
                    username = HttpUtility.UrlEncode(User.Identity.Name.Replace(@"'", "").Replace(" ", "").ToLower()), 
                    password = HttpUtility.UrlEncode("r3MuSU53r#"), 
                    email = HttpUtility.UrlEncode(UserManager.FindById(User.Identity.GetUserId()).EmailAddress),
                    firstname = nameComponents[0]
                };
                if(nameComponents.Length == 1){
                    mUser.lastname = "R3man";
                }
                else if (nameComponents.Length == 2){
                    mUser.lastname = nameComponents[1];
                }
                else if (nameComponents.Length == 3){
                    mUser.lastname = string.Concat(nameComponents[1], " ", nameComponents[2]);
                };
 
                String postData = String.Format("users[0][username]={0}&users[0][password]={1}&users[0][firstname]={2}&users[0][lastname]={3}&users[0][email]={4}", mUser.username, mUser.password, mUser.firstname, mUser.lastname, mUser.email);
                string createRequest = string.Format("{0}?wstoken={1}&wsfunction={2}&moodlewsrestformat=json", moodleURL,  Properties.Settings.Default.MoodleToken, function);

                HttpWebRequest req = (HttpWebRequest)WebRequest.Create(createRequest);
                req.Method = "POST";
                req.ContentType = "application/x-www-form-urlencoded";
                byte[] formData = UTF8Encoding.UTF8.GetBytes(postData);
                req.ContentLength = formData.Length;
                using (Stream post = req.GetRequestStream())
                {
                    post.Write(formData, 0, formData.Length);
                }
                // Get the Response
                HttpWebResponse resp = (HttpWebResponse)req.GetResponse();
                Stream resStream = resp.GetResponseStream();
                StreamReader reader = new StreamReader(resStream);
                string contents = reader.ReadToEnd();
 
                // Deserialize
                JavaScriptSerializer serializer = new JavaScriptSerializer();
                if (contents.Contains("exception"))
                {
                    // Error
                    MoodleException moodleError = serializer.Deserialize<MoodleException>(contents);

                    TempData.Add("Message", "An error occurred: Please contact Clyde en Marland with this message; ");
                    
                    TempData.Add("ErrorMessage", moodleError.debuginfo);
                }
                else
                {
                    // Good
                    TempData.Add("Message", "Registration Complete!");
                    List<MoodleCreateUserResponse> newUsers = serializer.Deserialize<List<MoodleCreateUserResponse>>(contents);
                }
            }
            catch (Exception ex)
            {
                TempData.Add("Message", "An error occurred: Please contact Clyde en Marland with this message; ");
                TempData.Add("ErrorMessage", ex.Message);
            }
            return RedirectToAction("Index");
        }

        public ActionResult RedirectToMoodle()
        {
            return PartialView("_RedirectToMoodle");
        }

        public ActionResult RedirectToForum()
        {
            string useName;

            if (User.Identity.Name.IndexOf(" ") != -1)
            {
                useName = User.Identity.Name.Substring(0, User.Identity.Name.IndexOf(" "));
            }
            else
            {
                useName = User.Identity.Name;
            }
            useName = useName.Replace("'", "");

            @ViewBag.PW = string.Concat("R3MUSUser_", useName);
            return PartialView("_RedirectToForums");
        }

        public ActionResult RegisterForHipchat()
        {
            return View();
        }

        public ActionResult RegisterForForums()
        {
            try
            {
                var r3musForum = new r3musForumDBContext();
                var siteUser = UserManager.FindById(User.Identity.GetUserId());
                var forumRole = r3musForum.MembershipRoles.Where(role => role.RoleName == "Standard Members").FirstOrDefault();

                var forumUser = r3musForum.MembershipUsers.Where(user => (user.UserName.ToLower() == siteUser.UserName.ToLower()) || (user.Email == siteUser.EmailAddress)).FirstOrDefault();
                string useName;

                if (siteUser.UserName.IndexOf(" ") != -1)
                {
                    useName = siteUser.UserName.Substring(0, siteUser.UserName.IndexOf(" "));
                }
                else
                {
                    useName = siteUser.UserName;
                }
                useName = useName.Replace("'", "");

                if (forumUser != null)
                {
                    forumUser.PasswordSalt = string.Concat("R3MUS_", useName);
                    forumUser.Password = GenerateSaltedHash(string.Concat("R3MUSUser_", useName), forumUser.PasswordSalt);
                    r3musForum.SaveChanges();
                    TempData.Add("Message", "Your access to the forums should now be restored. You can now access the forums using the link in Services at the top of the page.");
                }
                else
                {
                    var len = siteUser.Avatar.Length;
                    forumUser = new MembershipUser()
                    {
                        Id = new Guid(siteUser.Id),
                        UserName = siteUser.UserName,
                        Password = string.Concat("R3MUSUser_", useName),
                        Email = siteUser.EmailAddress,
                        PasswordSalt = string.Concat("R3MUS_", useName),
                        Avatar = siteUser.Avatar,
                        IsApproved = true,
                        IsLockedOut = false,
                        CreateDate = DateTime.Now,
                        LastLoginDate = DateTime.Now,
                        LastPasswordChangedDate = DateTime.Now,
                        LastLockoutDate = DateTime.Now,
                        FailedPasswordAttemptCount = 3,
                        FailedPasswordAnswerAttempt = 3,
                        Slug = useName,
                        DisableEmailNotifications = true,
                        IsExternalAccount = true
                    };
                    forumUser.Password = GenerateSaltedHash(forumUser.Password, forumUser.PasswordSalt);
                    r3musForum.MembershipUsers.Add(forumUser);

                    var roleLink = new MembershipUsersInRole()
                    {
                        Id = Guid.NewGuid(),
                        UserIdentifier = forumUser.Id,
                        RoleIdentifier = forumRole.Id,
                        MembershipUser = forumUser,
                        MembershipRole = forumRole
                    };

                    r3musForum.MembershipUsersInRoles.Add(roleLink);
                    r3musForum.SaveChanges();

                    TempData.Add("Message", "Registration Complete! You can now access the forums using the link in Services at the top of the page.");
                }
            }
            catch(DbEntityValidationException ex)
            {
                TempData.Add("Message", "An error occurred: Please contact Clyde en Marland with this message; ");
                TempData.Add("ErrorMessage", ex.Message);
            }
            catch (Exception ex)
            {
                TempData.Add("Message", "An error occurred: Please contact Clyde en Marland with this message; ");
                TempData.Add("ErrorMessage", ex.Message);
            }
            return RedirectToAction("Index");
        }

        [HttpPost]
        public ActionResult RegisterForHipchat(int? id)
        {
            try
            {
                Hipchat_Plugin.Hipchat.CreateUser(Properties.Settings.Default.HipchatToken, User.Identity.Name, UserManager.FindById(User.Identity.GetUserId()).EmailAddress, "R3MUS", "password");
                TempData.Add("Message", string.Format("You can now log in to Hipchat using the email address you used to register ({0}) and the password 'password', which you should change.", UserManager.FindById(User.Identity.GetUserId()).EmailAddress));
            }
            catch (Exception ex)
            {
                TempData.Add("Message", string.Format("Could not create a Hipchat account for {0}.", UserManager.FindById(User.Identity.GetUserId()).EmailAddress));
                TempData.Add("ErrorMessage", ex.Message);
            }
            return RedirectToAction("Index");
        }

        [HttpPost]
        public ActionResult RegisterForSlack()
        {
            //var result = string.Empty;

            var result = Plugin.SendUserInvite("R3MUS", Properties.Settings.Default.SlackToken, UserManager.FindById(User.Identity.GetUserId()).EmailAddress);

            //using (var client = new HttpClient())
            //{
            //    client.BaseAddress = new Uri(string.Format("{0}://{1}{2}", Request.Url.Scheme, Request.Url.Authority, Url.Content("~")));
            //    client.DefaultRequestHeaders.Accept.Clear();
            //    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            //    try
            //    {                    
            //        var task = Task.Factory.StartNew(() => JsonConvert.DeserializeObject<string>(client.GetStringAsync(string.Format("api/SlackRegistry/r3mus/{0}/{1}",
            //            UserManager.FindById(User.Identity.GetUserId()).EmailAddress,
            //            Properties.Settings.Default.SlackToken
            //            )).Result));
            //        task.Wait();
            //        result = task.Result;
            //    }
            //    catch(Exception ex)
            //    {
            //        result = string.Format("false:{0}", ex.Message);
            //    }
            //}

            //if (result.Contains("false"))
            //{
            //    TempData.Add("Message", string.Format("Could not send an invitation: {0}", result));
            //}
            //else if (result.Contains("true"))
            //{
            //    TempData.Add("Message", string.Format("Slack invitation sent to {0}; please check your email inbox.", UserManager.FindById(User.Identity.GetUserId()).EmailAddress));
            //}
            //else
            //{
            //    TempData.Add("Message", string.Format("Could not send an invitation: unknown reason."));
            //}

            if(result == false)
            {
                TempData.Add("Message", string.Format("Could not send an invitation."));
            }
            else
            {
                TempData.Add("Message", string.Format("Slack invitation sent to {0}; please check your email inbox.", UserManager.FindById(User.Identity.GetUserId()).EmailAddress));
            }

            return RedirectToAction("Index");
        }

        [HttpPost]
        public ActionResult SubmitSuggestion(FormCollection form)
        {
            try
            {
                MessagePayload message = new MessagePayload();
                message.Attachments = new List<MessagePayloadAttachment>();

                message.Attachments.Add(new MessagePayloadAttachment()
                {
                    Text = form["SuggestionText"].ToString(),
                    Title = "Begging your pardons, Sirs & Madams, but might I make a suggestion?",
                    ThumbUrl = "http://www.r3mus.org/Images/kryten.png"
                });

                Plugin.SendToRoom(message, Properties.Settings.Default.SuggestionRoomName, Properties.Settings.Default.SlackWebhook, "Kryten");

                TempData.Add("Message", "Thank you for your input.");
            }
            catch (Exception ex)
            {
                TempData.Add("Message", ex.Message);
                ModelState.AddModelError("Error1", ex.Message);
            }
            return RedirectToAction("Index");
        }

        private static async Task<string> SlackRegistry_WebService(string baseURL)
        {
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(baseURL);
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                
                using (var response = await client.GetAsync(""))
                {
                    return await response.Content.ReadAsAsync<string>();
                }
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        private static string GenerateSaltedHash(string plainText, string salt)
        {
            // http://stackoverflow.com/questions/2138429/hash-and-salt-passwords-in-c-sharp

            var plainTextBytes = Encoding.UTF8.GetBytes(plainText);
            var saltBytes = Encoding.UTF8.GetBytes(salt);

            // Combine the two lists
            var plainTextWithSaltBytes = new List<byte>(plainTextBytes.Length + saltBytes.Length);
            plainTextWithSaltBytes.AddRange(plainTextBytes);
            plainTextWithSaltBytes.AddRange(saltBytes);

            // Produce 256-bit hashed value i.e. 32 bytes
            HashAlgorithm algorithm = new SHA256Managed();
            var byteHash = algorithm.ComputeHash(plainTextWithSaltBytes.ToArray());
            return Convert.ToBase64String(byteHash);
        }        
    }
}
