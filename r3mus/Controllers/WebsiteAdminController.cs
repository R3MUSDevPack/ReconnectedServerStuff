using r3mus.Models;
using r3mus.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using System.Data.Entity;
using System.Threading.Tasks;

using Microsoft.Owin.Security.DataProtection;
using Microsoft.AspNet.Identity.Owin;
using JKON.Slack;
using JKON.EveWho;
using JKON.EveApi.Corporation.Models;
using eZet.EveLib.EveXmlModule;

namespace r3mus.Controllers
{
    [Authorize(Roles = "Admin, Director, CEO")]
    public class WebsiteAdminController : Controller
    {
        private ApplicationDbContext db;
        private UserManager<ApplicationUser> userManager;

        public WebsiteAdminController()
        {
            db = new ApplicationDbContext();
            userManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(db));
            userManager.UserValidator = new UserValidator<ApplicationUser>(userManager) { AllowOnlyAlphanumericUserNames = false };
        }

        //
        // GET: /WebsiteAdmin/
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult TestSlack()
        {
            //RecruitmentController.SendMessage("Slack Test From Website");
            MessagePayload message = new MessagePayload();
            message.Attachments = new List<MessagePayloadAttachment>();

            message.Attachments.Add(new MessagePayloadAttachment()
            {
                Text = "Testing",
                TitleLink = string.Format(Properties.Settings.Default.EveWhoPilotURL, User.Identity.Name.Replace(" ", "+")),
                Title = "Check this out!",
                ThumbUrl = string.Format(Properties.Settings.Default.CharacterImageServerURL, "pilot", Api.GetCharacterID(User.Identity.Name))
            });
            RecruitmentController.SendMessage(message);

            return RedirectToAction("ViewUsers");
        }

        public ActionResult GetAllCorpMembers()
        {
            return View();
        }

        public List<Member> GetCorpMembers()
        {
            var members = db.CorpMembers.ToList<Member>();

            var cKey = EveXml.CreateCorporationKey(Convert.ToInt32(Properties.Settings.Default.CorpAPI), Properties.Settings.Default.VCode);
            var CEOName = cKey.Corporation.GetCorporationSheet().Result.CeoName;
            var resortModels_All = members.Where(member => member.Name == CEOName).ToList();
            members.Where(member => member.Title.Contains("XO") && !(member.Name == CEOName)).OrderBy(member => member.Title).OrderBy(member => member.MemberSince).ToList().ForEach(member => resortModels_All.Add(member));
            members.Where(member => member.Title.Contains("Director") && !(member.Name == CEOName) && !member.Title.Contains("XO")).OrderBy(member => member.Title).OrderBy(member => member.MemberSince).ToList().ForEach(member => resortModels_All.Add(member));
            members.Where(member => (member.Title != string.Empty && (!(member.Name == CEOName) && !member.Title.Contains("XO") && !member.Title.Contains("Director")))).OrderBy(member => member.Title).OrderByDescending(member => member.LastLogonDateTime).ToList().ForEach(member => resortModels_All.Add(member));
            members.Where(member => member.Title == string.Empty).OrderByDescending(member => member.LastLogonDateTime).ToList().ForEach(member => resortModels_All.Add(member));
            
            if(!resortModels_All.FirstOrDefault().Title.Contains("CEO"))
            {
                if(!(resortModels_All.FirstOrDefault().Title == string.Empty))
                {
                    resortModels_All.FirstOrDefault().Title = string.Concat("CEO, ", resortModels_All.FirstOrDefault().Title);
                }
                else
                {
                    resortModels_All.FirstOrDefault().Title = "CEO";
                }
            }

            var users = db.Users.Where(user => user.MemberType == ApplicationUser.IDType.Corporation.ToString()).ToList<ApplicationUser>();

            List<ApiInfo> apis = new List<ApiInfo>();

            resortModels_All.ForEach(member =>
            {
                try
                {
                    var user = users.Where(usr => usr.UserName == member.Name).FirstOrDefault();
                    //if (user == null)
                    //{
                    //    if (apis.Count() == 0)
                    //    {
                    //        apis = db.ApiInfoes.ToList();
                    //    }
                    //    apis.ForEach(api =>
                    //    {
                    //        try
                    //        {
                    //            var chars = api.GetDetails();
                    //            if (chars.Any(toon => toon.CharacterName == member.Name))
                    //            {
                    //                user = api.User;
                    //                return;
                    //            }
                    //        }
                    //        catch (Exception ex) { }
                    //    });
                    //}

                    member.Avatar = users.Where(usr => usr.UserName == member.Name).FirstOrDefault().Avatar;
                }
                catch (Exception e) { }
            });

            return resortModels_All;
        }

        //[OutputCache(Duration = 3600)]
        public ActionResult ViewUsers(r3mus.Models.ApplicationUser.IDType memberType = r3mus.Models.ApplicationUser.IDType.Corporation, int page = 1)
        {
            var users = db.Users.Where(user => user.MemberType == memberType.ToString()).ToList<ApplicationUser>();

            List<ApiInfo> apis = new List<ApiInfo>();

            //var members = db.CorpMembers.ToList<Member>();

            var userModels = new List<UserProfileViewModel>();

            var resortModels_All = GetCorpMembers();

            ////var members = Api.GetCorpMembers(Convert.ToInt64(Properties.Settings.Default.CorpAPI), Properties.Settings.Default.VCode);
            //var resortModels_All = members.Where(member => member.Title.Contains("CEO")).ToList();
            //members.Where(member => member.Title.Contains("Director") && !member.Title.Contains("CEO")).OrderBy(member => member.Title).OrderBy(member => member.MemberSince).ToList().ForEach(member => resortModels_All.Add(member));
            //members.Where(member => (member.Title != string.Empty && (!member.Title.Contains("CEO") && !member.Title.Contains("Director")))).OrderBy(member => member.Title).OrderByDescending(member => member.LastLogonDateTime).ToList().ForEach(member => resortModels_All.Add(member));
            //members.Where(member => member.Title == string.Empty).OrderByDescending(member => member.LastLogonDateTime).ToList().ForEach(member => resortModels_All.Add(member));

            int minNo = ((page - 1) * 10);
            int maxNo = (page * 10);

            var resortModels = new List<Member>();

            resortModels = resortModels_All.Skip(minNo).Take(10).ToList();

            ViewBag.MemberType = memberType;
            ViewBag.PreviousPage = (page - 1);
            ViewBag.NextPage = (page + 1);
            ViewBag.ShowPrevious = (minNo > 0);
            ViewBag.ShowNext = ((resortModels.Count == 10) && (minNo <= (resortModels_All.Count - 10)));

            resortModels.ForEach(member =>
            {
                    member.Title = member.Title.Replace(member.ID.ToString(), "").Trim().Trim(',');
                    var user = users.Where(usr => usr.UserName == member.Name).FirstOrDefault();
                    if (user == null)
                    {
                        var m = db.DeclaredToons.Where(t => t.ToonName == member.Name).FirstOrDefault();
                        if(m != null)
                        {
                            user = db.Users.Where(w => w.Id == m.User_Id).FirstOrDefault();
                        }
                        
                        if (user == null)
                        {
                            user = new ApplicationUser()
                            {
                                Id = 0.ToString(),
                                UserName = "Not Registered",
                                EmailAddress = "Not Registered",
                                MemberType = "Not Registered",
                                MemberSince = member.MemberSince,
                                Titles = new List<Title>()
                            };
                        }
                    }

                if(user != null)
                {
                    member.Title = string.Format("This is {0}", user.UserName);
                }

                userModels.Add(new UserProfileViewModel()
                    {
                        Id = user.Id,
                        MemberName = member.Name,
                        UserName = user.UserName,
                        EmailAddress = user.EmailAddress,
                        MemberSince = Convert.ToDateTime(member.MemberSince),
                        MemberType = user.MemberType,
                        Titles = member.Title,
                        WebsiteRoles = string.Join(", ", user.Roles.Select(role => role.RoleId).ToList()),
                        Avatar = user.Avatar,
                        CurrentLocation = member.Location,
                        LastLogon = Convert.ToDateTime(member.LastLogonDateTime),
                        ShipType = member.ShipType,
                        UserRoles = member.Roles.Split(new string[] { ", " }, StringSplitOptions.RemoveEmptyEntries).ToList()
                    });
            });

            return View(userModels);
        }        

        [OverrideAuthorization]
        public ActionResult ViewProfile(string id = "")
        {
            if(TempData["Message"] != null)
            {
                ViewBag.Message = TempData["Message"];
            }

            if (id == string.Empty)
            {
                try
                {
                    id = User.Identity.GetUserId();
                    if ((id == null) || (id == string.Empty))
                    {
                        return RedirectToAction("Login", "Account");
                    }
                }
                catch (Exception ex)
                {
                    return RedirectToAction("Login", "Account");
                }
            }

            ViewBag.FullAccessMask = Properties.Settings.Default.FullAPIAccessMask;

            TempData.Clear();
            TempData.Add("UserId", id);

            return View(GetUserProfile(id));
        }

        private UserProfileViewModel GetUserProfile(string id)
        {
            var currentUser = db.Users.Where(user => user.Id == id).FirstOrDefault();
            currentUser.LoadApiKeys();
            currentUser.Titles = db.Titles.Where(title => title.UserId == id).ToList();

            var member = Api.GetCorpMembers(Convert.ToInt64(Properties.Settings.Default.CorpAPI), Properties.Settings.Default.VCode).Where(mbr => mbr.Name == currentUser.UserName).FirstOrDefault();

            var roles = db.Roles.Select(role => role.Name).ToList();
            var userId = currentUser.Id;
            var userRoles = db.Roles.Where(role => role.Users.Any(user => user.UserId == currentUser.Id)).Select(role => role.Name).ToList();

            if (currentUser.Titles.Count == 0)
            {
                currentUser.Titles.Add(new Title() { UserId = currentUser.Id, TitleName = "Corp Member" });
            }

            var knownAlts = new List<string>();
            try
            {
                var apis = db.ApiInfoes.Where(api => api.User.Id == id).ToList();

                apis.ForEach(api =>
                {
                    var alts = api.GetDetails();
                    alts.ForEach(alt => knownAlts.Add(alt.CharacterName));
                });
            }
            catch (Exception ex) { }

            if (member != null)
            {
                return new UserProfileViewModel()
                {
                    Id = currentUser.Id,
                    UserName = currentUser.UserName,
                    EmailAddress = currentUser.EmailAddress,
                    MemberSince = Convert.ToDateTime(currentUser.MemberSince),
                    MemberType = currentUser.MemberType,
                    Avatar = currentUser.Avatar,
                    Titles = string.Join(", ", currentUser.Titles.Select(t => t.TitleName).Distinct().ToList()),
                    WebsiteRoles = string.Join(", ", currentUser.Roles.Select(role => role.RoleId).ToList()),
                    ApiKeys = db.ApiInfoes.Where(api => api.User.Id == currentUser.Id).ToList(),
                    UserRoles = userRoles,
                    AvailableRoles = roles,
                    CurrentLocation = member.Location,
                    LastLogon = Convert.ToDateTime(member.LastLogonDateTime),
                    ShipType = member.ShipType,
                    KnownAlts = string.Join(", ", knownAlts)
                };
            }
            else
            {
                return new UserProfileViewModel()
                {
                    Id = currentUser.Id,
                    UserName = currentUser.UserName,
                    EmailAddress = currentUser.EmailAddress,
                    MemberSince = Convert.ToDateTime(currentUser.MemberSince),
                    MemberType = currentUser.MemberType,
                    Avatar = currentUser.Avatar,
                    Titles = string.Join(", ", currentUser.Titles.Select(t => t.TitleName).ToList()),
                    WebsiteRoles = string.Join(", ", currentUser.Roles.Select(role => role.RoleId).ToList()),
                    ApiKeys = db.ApiInfoes.Where(api => api.User.Id == currentUser.Id).ToList(),
                    UserRoles = userRoles,
                    AvailableRoles = roles,
                    CurrentLocation = "Unknown",
                    LastLogon = new DateTime(),
                    ShipType = "Unknown",
                    KnownAlts = string.Join(", ", knownAlts)
                };
            }
        }

        [Authorize(Roles = "CEO, Admin, Director")]
        public ActionResult AssignRoles(FormCollection form)
        {
            string userId = form["userId"].ToString();
            ApplicationUser currentUser = db.Users.Where(user => user.Id == userId).FirstOrDefault();

            var roles = db.Roles.Select(role => role.Name).ToList();
            var userRoles = userManager.GetRoles(currentUser.Id).ToList();

            var account = new AccountController();

            foreach (var role in roles)
            {
                if ((form[role] != null) && !userRoles.Contains(role))
                {
                    account.UserManager.AddToRole(currentUser.Id, role);
                }
            }

            foreach (var role in userRoles)
            {
                if ((form[role] == null))
                {
                    account.UserManager.RemoveFromRole(currentUser.Id, role);
                }
            }
            
            return RedirectToAction("ViewProfile", new { id = currentUser.Id });
        }

        public ActionResult UpdateApiDetails(string originator, string id = "")
        {
            ApplicationUser currentUser = new ApplicationUser();

            if (id == string.Empty)
            {
                id = User.Identity.GetUserId();
            }

            currentUser = db.Users.Where(user => user.Id == id).FirstOrDefault();
            var cUsrTitles = currentUser.Titles;

            currentUser.GetDetails(true);

            db.Titles.Where(dbTitle => dbTitle.UserId == id).ToList().Where(dbTitle => cUsrTitles.Any(cUsrTitle => dbTitle.TitleName == cUsrTitle.TitleName)).ToList().ForEach(dbTitle => db.Entry(dbTitle).State = EntityState.Unchanged);
            db.Titles.Where(dbTitle => dbTitle.UserId == id).ToList().Where(dbTitle => !cUsrTitles.Any(cUsrTitle => dbTitle.TitleName == cUsrTitle.TitleName)).ToList().ForEach(dbTitle => db.Entry(dbTitle).State = EntityState.Deleted);

            db.SaveChanges();

            return RedirectToAction(originator, new { id = id });
        }

        public ActionResult DeleteApi(int id, string userId)
        {
            db.ApiInfoes.Where(api => api.Id == id).ToList().ForEach(api => db.Entry(api).State = EntityState.Deleted);
            db.SaveChanges();

            return RedirectToAction("ViewProfile", new { id = userId });
        }

        [HttpPost]
        [Authorize(Roles = "Admin, Director, CEO")]
        public async Task<ActionResult> ResetPassword(string userId = "")
        {
            if (userId != string.Empty)
            {
                var provider = new DpapiDataProtectionProvider("Website");
                userManager.UserTokenProvider = new DataProtectorTokenProvider<ApplicationUser>(provider.Create("Website")) { TokenLifespan = TimeSpan.FromHours(4) };

                var user = userManager.FindById(userId);
                var token = await userManager.GeneratePasswordResetTokenAsync(userId);

                string useName;

                if (User.Identity.Name.IndexOf(" ") != -1)
                {
                    useName = user.UserName.Substring(0, User.Identity.Name.IndexOf(" "));
                }
                else
                {
                    useName = user.UserName;
                }

                var result = await userManager.ResetPasswordAsync(userId, token, string.Concat("R3MUSUser_", useName));

                if(result.Succeeded)
                {
                    TempData.Add("Message", string.Format("Password reset confirmed: new password is {0}", string.Concat("R3MUSUser_", useName)));
                }
                else
                {
                    TempData.Add("Message", string.Format("Password reset failed: {0}", result.Errors.ToList()[0]));
                }

                await userManager.UpdateAsync(user);
            }

            return RedirectToAction("ViewProfile", new { id = userId });
        }

        public ActionResult EditApi(int id)
        {
            var apiInfo = db.ApiInfoes.Where(api => api.Id == id).FirstOrDefault();
            var userId = TempData["UserId"].ToString();

            TempData.Clear();
            TempData.Add("UserId", userId);
            ViewBag.UserId = userId;

            return View(apiInfo);
        }

        [HttpPost]
        public ActionResult EditApi(ApiInfo apiInfo)
        {
            var userId = TempData["UserId"].ToString();
            if(ModelState.IsValid)
            {
                db.Entry(apiInfo).State = EntityState.Modified;
                db.SaveChanges();
            }
            return RedirectToAction("ViewProfile", new { id = userId });
        }

        [OverrideAuthorization]
        public ActionResult CreateApi(string id)
        {
            var apiInfo = new ApiInfo();
            //ViewBag.UserId = id;
            TempData.Clear();
            TempData.Add("UserId", id);
            return View(apiInfo);
        }

        [HttpPost]
        [OverrideAuthorization]
        public ActionResult CreateApi(ApiInfo apiInfo)
        {
            UserManager<ApplicationUser> usrMgr = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(db));
            if (User.Identity.IsAuthenticated)
            {
                usrMgr.FindById(User.Identity.GetUserId()).AddApiInfo(apiInfo.ApiKey.ToString(), apiInfo.VerificationCode);
                apiInfo = usrMgr.FindById(User.Identity.GetUserId()).ApiKeys.Last();
            }
            else
            {
                string userID = TempData["UserID"].ToString();
                usrMgr.FindById(userID).AddApiInfo(apiInfo.ApiKey.ToString(), apiInfo.VerificationCode);
                apiInfo = usrMgr.FindById(userID).ApiKeys.Last();
            }

            db.ApiInfoes.Add(apiInfo);
            db.SaveChanges();
            
            return RedirectToAction("ViewProfile");
        }        
	}
}