﻿using r3mus.Models;
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

        //[OutputCache(Duration = 3600)]
        public ActionResult ViewUsers(r3mus.Models.ApplicationUser.IDType memberType = r3mus.Models.ApplicationUser.IDType.Corporation, int page = 1)
        {
            var users = db.Users.Where(user => user.MemberType == memberType.ToString()).ToList<ApplicationUser>();

            var members = db.CorpMembers.ToList<Member>();

            var userModels = new List<UserProfileViewModel>();

            List<ApiInfo> apis = new List<ApiInfo>();

            //var members = Api.GetCorpMembers(Convert.ToInt64(Properties.Settings.Default.CorpAPI), Properties.Settings.Default.VCode);
            var resortModels_All = members.Where(member => member.Title.Contains("CEO")).ToList();
            members.Where(member => member.Title.Contains("Director") && !member.Title.Contains("CEO")).OrderBy(member => member.Title).OrderBy(member => member.MemberSince).ToList().ForEach(member => resortModels_All.Add(member));
            members.Where(member => (member.Title != string.Empty && (!member.Title.Contains("CEO") && !member.Title.Contains("Director")))).OrderBy(member => member.Title).OrderByDescending(member => member.LastLogonDateTime).ToList().ForEach(member => resortModels_All.Add(member));
            members.Where(member => member.Title == string.Empty).OrderByDescending(member => member.LastLogonDateTime).ToList().ForEach(member => resortModels_All.Add(member));

            int minNo = ((page - 1) * 20);
            int maxNo = (page * 20);

            var resortModels = new List<Member>();

            resortModels = resortModels_All.Skip(minNo).Take(20).ToList();

            ViewBag.MemberType = memberType;
            ViewBag.PreviousPage = (page - 1);
            ViewBag.NextPage = (page + 1);
            ViewBag.ShowPrevious = (minNo > 0);
            ViewBag.ShowNext = ((resortModels.Count == 20) && (minNo <= (resortModels_All.Count - 20)));

            resortModels.ForEach(member =>
            {
                    member.Title = member.Title.Replace(member.ID.ToString(), "").Trim().Trim(',');
                    var user = users.Where(usr => usr.UserName == member.Name).FirstOrDefault();
                    if (user == null)
                    {
                        if (apis.Count() == 0)
                        {
                            apis = db.ApiInfoes.ToList();
                        }
                        apis.ForEach(api =>
                            {
                                try
                                {
                                    var chars = api.GetDetails();
                                    if (chars.Any(toon => toon.CharacterName == member.Name))
                                    {
                                        user = api.User;
                                        return;
                                    }
                                }
                                catch (Exception ex) { }
                            });
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
                        //try
                        //{
                        //    user.Avatar = ApplicationUser.GetAvatar(JKON.EveWho.Api.GetCharacterID(member.Name), EveAI.Live.ImageServer.ImageSize.Size128px);
                        //}
                        //catch (Exception ex) { }
                    }
                    int titleIsId;
                    int.TryParse(member.Title, out titleIsId);
                    if ((titleIsId > 0) && (titleIsId != null))
                    {
                        try
                        {
                            var chkUser = Api.GetCharacter(titleIsId);
                            member.Title = member.Title.Replace(titleIsId.ToString(), string.Format("This is {0}", chkUser.result.characterName));
                        }
                        catch (Exception ex1) { }
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

            TempData.Clear();
            TempData.Add("UserId", id);

            return View(GetUserProfile(id));
        }

        private UserProfileViewModel GetUserProfile(string id)
        {
            var currentUser = db.Users.Where(user => user.Id == id).FirstOrDefault();
            //var currentUser = userManager.FindById(id);
            currentUser.LoadApiKeys();
            currentUser.Titles = db.Titles.Where(title => title.UserId == id).ToList();

            var member = Api.GetCorpMembers(Convert.ToInt64(Properties.Settings.Default.CorpAPI), Properties.Settings.Default.VCode).Where(mbr => mbr.Name == currentUser.UserName).FirstOrDefault();

            var roles = db.Roles.Select(role => role.Name).ToList();
            var userId = currentUser.Id;
            //var userRoles = userManager.GetRoles(userId.ToString()).ToList();
            var userRoles = db.Roles.Where(role => role.Users.Any(user => user.UserId == currentUser.Id)).Select(role => role.Name).ToList();

            if (currentUser.Titles.Count == 0)
            {
                currentUser.Titles.Add(new Title() { UserId = currentUser.Id, TitleName = "Corp Member" });
            }

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
                    Titles = string.Join(", ", currentUser.Titles.Select(t => t.TitleName).ToList()),
                    WebsiteRoles = string.Join(", ", currentUser.Roles.Select(role => role.RoleId).ToList()),
                    ApiKeys = db.ApiInfoes.Where(api => api.User.Id == currentUser.Id).ToList(),
                    UserRoles = userRoles,
                    AvailableRoles = roles,
                    CurrentLocation = member.Location,
                    LastLogon = Convert.ToDateTime(member.LastLogonDateTime),
                    ShipType = member.ShipType
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
                    ShipType = "Unknown"
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

        public ActionResult CreateApi(string id)
        {
            var apiInfo = new ApiInfo();
            //ViewBag.UserId = id;
            TempData.Clear();
            TempData.Add("UserId", id);
            return View(apiInfo);
        }

        [HttpPost]
        public ActionResult CreateApi(ApiInfo apiInfo)
        {
            string userId = TempData["UserId"].ToString();

            apiInfo.User = db.Users.Where(user => user.Id == userId).FirstOrDefault();

            db.ApiInfoes.Add(apiInfo);
            db.SaveChanges();            

            return RedirectToAction("ViewProfile", new { id = userId });
        }        
	}
}