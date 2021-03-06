﻿using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using r3mus.Models;
using r3mus.ViewModels;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using r3mus.Filters;
using Hipchat_Plugin;
using Slack_Plugin;
using JKON.Slack;
using JKON.EveWho;
using System.Threading.Tasks;

namespace r3mus.Controllers
{
    [Authorize]
    public class RecruitmentController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();
        private RecruitmentStatEntities ent = new RecruitmentStatEntities();
        private ApplicantEntities appent = new ApplicantEntities();
        private r3mus_MonthRecruitmentStatEntities mnthEnt = new r3mus_MonthRecruitmentStatEntities();

        //protected UserManager<ApplicationUser> UserManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(new ApplicationDbContext()));

        //
        // GET: /Recruitment/
        public ActionResult Index()
        {
            var statsVM = new RecruitmentStatsViewModel();
            statsVM.SubmittersLastWeek = ent.LastWeeksSubmissionStats.ToList();
            statsVM.MailersLastWeek = ent.LastWeeksMailStats.ToList();
            statsVM.SubmittersLastMonth = mnthEnt.LastMonthsSubmissionStats.ToList();
            statsVM.MailersLastMonth = mnthEnt.LastMonthsMailStats.ToList();
            statsVM.MailsToSend = db.RecruitmentMailees.Where(m => (m.MailerId == null) && (m.CorpId_AtLastCheck >= 1000000) && (m.CorpId_AtLastCheck <= 1000200)).Count();
            statsVM.ApplicationsToProcess = appent.ApplicantLists.Where(a => (a.Status != ApplicationReviewViewModel.ApplicationStatus.Accepted.ToString()) && (a.Status != ApplicationReviewViewModel.ApplicationStatus.Rejected.ToString())).Count();

            return View(statsVM);
        }

        public ActionResult Home()
        {
            var statsVM = new RecruitmentStatsViewModel();
            statsVM.SubmittersLastWeek = ent.LastWeeksSubmissionStats.ToList();
            statsVM.MailersLastWeek = ent.LastWeeksMailStats.ToList();
            statsVM.SubmittersLastMonth = mnthEnt.LastMonthsSubmissionStats.ToList();
            statsVM.MailersLastMonth = mnthEnt.LastMonthsMailStats.ToList();
            statsVM.MailsToSend = db.RecruitmentMailees.Where(m =>
                (m.MailerId == null && !m.Name.Contains("Citizen") && !m.Name.Contains("Trader") && !m.Name.Contains("Miner"))).ToList()
            .Where(m =>
                ((m.InNPCCorp)
                && (m.DateOfBirthInRange))).Count();
            statsVM.ApplicationsToProcess = appent.ApplicantLists.Where(a => (a.Status != ApplicationReviewViewModel.ApplicationStatus.Accepted.ToString()) && (a.Status != ApplicationReviewViewModel.ApplicationStatus.Rejected.ToString())).Count();

            return View(statsVM);
        }

        [Authorize(Roles = "Recruiter, Screener, Director, CEO, Admin")]
        public ActionResult GetNames()
        {
            var mailees = appent.MarkRecruitmentMailees(User.Identity.GetUserId());

            ViewBag.Mailees = string.Join(",", mailees.Select(m => m.Name).ToArray());

            return View();
        }

        [HttpPost]
        //[ActionName("GetNames")]
        [r3mus.Filters.ApiKeyAttribute.MultipleButton(Name = "GetNames", Argument = "sent")]
        public ActionResult UpdateNamesAfterMailing(FormCollection form)
        {
            var names = form["mailees"].ToString();
            CloseRecruitmentMailees(names);

            return RedirectToAction("Home");
        }

        private async Task CloseRecruitmentMailees(string names)
        {
            await Task.Run(() => appent.CloseRecruitmentMailees(names));
        }

        [HttpPost]
        [r3mus.Filters.ApiKeyAttribute.MultipleButton(Name = "GetNames", Argument = "unlock")]
        public ActionResult UnlockNames(FormCollection form)
        {
            var names = form["mailees"].ToString();
            appent.UnlockRecruitmentMailees(names);

            return RedirectToAction("Home");
        }

        [Authorize()]
        public ActionResult AddNames()
        {
            return View();
        }

        [HttpPost]
        [Authorize()]
        [ActionName("AddNames")]
        public ActionResult AddNamesToDB(FormCollection form)
        {
            string[] names = form["names"].ToString().Split(new string[]{Environment.NewLine}, StringSplitOptions.None).Distinct().ToArray();
            IList<RecruitmentMailee> mailees = new List<RecruitmentMailee>();
            var archive = new r3mus_ArchiveDBEntities().RecruitmentMailees;
            
            foreach (string name in names)
            {
                if ((db.RecruitmentMailees.Where(m => m.Name == name).Any() == false) && (archive.Where(m => m.Name == name).Any() == false))
                {
                    RecruitmentMailee mailee = new RecruitmentMailee() { Name = name, Submitted = DateTime.Now };

                    db.RecruitmentMailees.Add(mailee);
                }
            }

            db.SaveChanges();

            return RedirectToAction("Home");
        }

        [OverrideAuthorization]
        public ActionResult Apply()
        {
            var model = new ApplicantViewModel();
            @ViewBag.FullAccessMask = Properties.Settings.Default.FullAPIAccessMask.ToString();

            if(User.Identity.IsAuthenticated)
            {
                model.Information = string.Concat("Vouched for by ", User.Identity.Name);
            }

            return View(model);
        }

        [OverrideAuthorization]
        [HttpPost]
        [ActionName("Apply")]
        public ActionResult SubmitApplication(ApplicantViewModel model)
        {
            ApiInfo api = new ApiInfo() { ApiKey = Convert.ToInt32(model.ApiKey),
                VerificationCode = model.VerificationCode };
            bool goodAccessMask;

            if (User != null)
            {
                goodAccessMask = true;
                if (!model.Information.Contains("Vouched for by "))
                {
                    model.Information = string.Concat("Vouched for by ", User.Identity.Name, ". ", model.Information);
                }
            }
            else
            {
                goodAccessMask = api.ValidateAccessMask(ApplicationUser.IDType.Corporation);
            }

            var applicant = new Applicant() { Name = model.UserName, EmailAddress = model.Email,
                TimeZone = model.TimeZone, Information = model.Information, Age = model.Age,
                ToonAge = model.ToonAge, Source = model.Source };
            
            bool goodTZ = (model.TimeZone != "Select a Time Zone");

            if (goodAccessMask && goodTZ)
            {
                applicant.ApiKey = Convert.ToInt32(model.ApiKey);
                applicant.VerificationCode = model.VerificationCode;

                db.Applicants.Add(applicant);
                db.SaveChanges();
                
                var application = new Application() { ApplicantId = applicant.Id, Notes = "New application",
                    Status = ApplicationReviewViewModel.ApplicationStatus.Applied.ToString(),
                    DateTimeCreated = DateTime.Now };
                db.Applications.Add(application);
                db.SaveChanges();

                MessagePayload message = new MessagePayload();
                message.Attachments = new List<MessagePayloadAttachment>();

                message.Attachments.Add(new MessagePayloadAttachment()
                {
                    Text = string.Format(Properties.Settings.Default.NewApp_MessageFormatLine2, 
                        applicant.Name, application.DateTimeCreated.ToString("yyyy-MM-dd HH:mm:ss")),
                    TitleLink = string.Format(Properties.Settings.Default.EveWhoPilotURL, 
                        applicant.Name.Replace(" ", "+")),
                    Title = Properties.Settings.Default.NewApp_MessageFormatLine1,
                    ThumbUrl = string.Format(Properties.Settings.Default.CharacterImageServerURL, 
                        Api.GetCharacterID(applicant.Name), 64.ToString()),
                    Colour = "#FFC200"
                });
                RecruitmentController.SendMessage(message);

                TempData.Clear();
                TempData.Add("Message", "Thank you for your application. A recruiter will contact you shortly.");

                return RedirectToAction("Index", "Home");
            }

            if (!goodAccessMask)
            {
                ViewBag.Message = string.Format("The access mask for the API key you have submitted is not correct ({0}, should be {1}). Please correct this and try again.", api.AccessMask, Properties.Settings.Default.FullAPIAccessMask);
            }
            else if(!goodTZ)
            {
                ViewBag.Message = "Please select a valid Time Zone.";
            }
            return View(model);
        }

        [Authorize(Roles = "Recruiter, Screener, Director, CEO, Admin")]
        public ActionResult ShowApplications(bool review = false)
        {
            IQueryable<ApplicantList> apps;
            if (!review)
            {
                apps = appent.ApplicantLists.Where(a => 
                    (a.Status != ApplicationReviewViewModel.ApplicationStatus.Accepted.ToString()) 
                    && (a.Status != ApplicationReviewViewModel.ApplicationStatus.Rejected.ToString()) 
                    && (a.Status != ApplicationReviewViewModel.ApplicationStatus.Withdrawn.ToString()))
                    .OrderByDescending(a => a.Applied);
                ViewBag.Title = "Applications To Process";
            }
            else
            {
                apps = appent.ApplicantLists.Where(a => 
                    (a.Status == ApplicationReviewViewModel.ApplicationStatus.Accepted.ToString()) 
                    || (a.Status == ApplicationReviewViewModel.ApplicationStatus.Rejected.ToString())
                    || (a.Status == ApplicationReviewViewModel.ApplicationStatus.Withdrawn.ToString()))
                    .OrderByDescending(a => a.Applied);
                ViewBag.Title = "Processed Applications";
            }

            return View(apps);
        }

        [Authorize(Roles = "Screener, Director, CEO, Admin")]
        public ActionResult ReviewApplication(int? id)
        {
            UserManager<ApplicationUser> UserManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(db));

            if (id == null)
            {
                return RedirectToAction("ShowApplications");
            }
            var newReviewModel = new ApplicationReviewViewModel() { };

            newReviewModel.ApplicationInfo = db.Applications.Where(a => a.ApplicantId == id).ToList<Application>();
            if ((newReviewModel.ApplicationInfo.Last().Status == ApplicationReviewViewModel.ApplicationStatus.Accepted.ToString()) || (newReviewModel.ApplicationInfo.Last().Status == ApplicationReviewViewModel.ApplicationStatus.Rejected.ToString()))
            {
                ViewBag.Complete = true;
            }
            else
            {
                ViewBag.Complete = false;
                if(newReviewModel.ApplicationInfo.Last().Status.Replace(" ", "") == ApplicationReviewViewModel.ApplicationStatus.Applied.ToString())
                {
                    newReviewModel.NewReviewItemStatus = ApplicationReviewViewModel.ApplicationStatus.InScreening;
                }
                else if(newReviewModel.ApplicationInfo.Last().Status.Replace(" ", "") == ApplicationReviewViewModel.ApplicationStatus.NeedsAPIMail.ToString())
                {
                    newReviewModel.NewReviewItemStatus = ApplicationReviewViewModel.ApplicationStatus.APIMailSent;
                }
                else if (newReviewModel.ApplicationInfo.Last().Status.Replace(" ", "") == ApplicationReviewViewModel.ApplicationStatus.AwaitingInvitation.ToString())
                {
                    newReviewModel.NewReviewItemStatus = ApplicationReviewViewModel.ApplicationStatus.Accepted;
                }
                else
                {
                    newReviewModel.NewReviewItemStatus = (ApplicationReviewViewModel.ApplicationStatus)Enum.Parse(typeof(ApplicationReviewViewModel.ApplicationStatus), newReviewModel.ApplicationInfo.Last().Status.Replace(" ", ""));
                }
            }

            newReviewModel.Applicant = db.Applicants.Where(a => a.Id == id).FirstOrDefault();
            if((newReviewModel.Applicant.TimeZone == null) || (newReviewModel.Applicant.TimeZone == string.Empty))
            {
                newReviewModel.Applicant.TimeZone = "Unknown";
            }
            newReviewModel.NewReviewItem = new Application() { ApplicantId = newReviewModel.Applicant.Id, Reviewer = UserManager.FindById(User.Identity.GetUserId()) };

            return View(newReviewModel);
        }

        [HttpPost]
        [ActionName("ReviewApplication")]
        [Authorize(Roles = "Screener, Director, CEO, Admin")]
        public ActionResult SubmitNewReviewItem(ApplicationReviewViewModel model)
        {            
            UserManager<ApplicationUser> UserManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(db));

            if(ModelState.IsValid)
            {
                model.NewReviewItem.Reviewer = UserManager.FindByName(model.NewReviewItem.Reviewer.UserName);
                model.Applicant = db.Applicants.Where(a => a.Id == model.NewReviewItem.ApplicantId).FirstOrDefault();
                model.NewReviewItem.Status = model.NewReviewItemStatus.GetDisplayName();
                
                model.NewReviewItem.DateTimeCreated = DateTime.Now;
                
                db.Applications.Add(model.NewReviewItem);
                db.SaveChanges();
                
                MessagePayload message = new MessagePayload();
                message.Attachments = new List<MessagePayloadAttachment>();

                message.Attachments.Add(new MessagePayloadAttachment()
                {
                    Text = string.Format(Properties.Settings.Default.AppUpdate_MessageFormatLine2, model.Applicant.Name, model.NewReviewItem.Status, model.NewReviewItem.Reviewer.UserName, model.NewReviewItem.DateTimeCreated.ToString("yyyy-MM-dd HH:mm:ss")),
                    TitleLink = string.Format(Properties.Settings.Default.EveWhoPilotURL, model.Applicant.Name.Replace(" ", "+")),
                    Title = Properties.Settings.Default.AppUpdate_MessageFormatLine1,
                    ThumbUrl = string.Format(Properties.Settings.Default.CharacterImageServerURL, Api.GetCharacterID(model.Applicant.Name), 64.ToString()),
                    Colour = "#FFC200"
                });
                RecruitmentController.SendMessage(message);

                if (model.NewReviewItemStatus == ApplicationReviewViewModel.ApplicationStatus.InScreening)
                {
                    return RedirectToAction("ReviewApplication", new { id = model.NewReviewItem.ApplicantId });
                }
                else
                {
                    return RedirectToAction("ShowApplications");
                }
            }
            return View(model);
        }

        [OverrideAuthorization]
        public ActionResult APIScreenshot()
        {
            return PartialView("_APIScreenshot");
        }

        public static void SendMessage(string message)
        {
            if (Properties.Settings.Default.Plugin.ToUpper() == "HIPCHAT")
            {
                Hipchat.SendToRoom(message, Properties.Settings.Default.RecruitmentRoomName, Properties.Settings.Default.HipchatToken);
            }
            else if (Properties.Settings.Default.Plugin.ToUpper() == "SLACK")
            {
                Slack.SendToRoom(message, Properties.Settings.Default.RecruitmentRoomName, Properties.Settings.Default.SlackWebhook);
            }
        }
        public static void SendMessage(MessagePayload message, string roomName = "")
        {
            if (roomName == string.Empty)
            {
                roomName = Properties.Settings.Default.RecruitmentRoomName;
            }
            if (Properties.Settings.Default.Plugin.ToUpper() == "SLACK")
            {
                Slack.SendToRoom(message, roomName, Properties.Settings.Default.SlackWebhook);
            }
        }
	}
}
