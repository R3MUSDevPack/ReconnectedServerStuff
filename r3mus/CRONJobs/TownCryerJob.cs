using JKON.Slack;
using Quartz;
using r3mus.Controllers;
using r3mus.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Web;

namespace r3mus.CRONJobs
{
    public class TownCryerJob : IJob
    {
        ApplicationDbContext db;

        public void Execute(IJobExecutionContext context)
        {
            var name = MethodBase.GetCurrentMethod().DeclaringType.Name;
            db = new ApplicationDbContext();
            try
            {
                MakeAnnoucements(db.CRONJobs.Where(job => job.JobName == name).FirstOrDefault());
            }
            catch(Exception ex)
            {
                //RecruitmentController.SendMessage(ex.Message);
            }
            db.SaveChanges();
        }

        private void MakeAnnoucements(CRONJob settings)
        {
            if (settings.Enabled)
            {
                var announcements = db.Announcements.Where(ann => ann.Date > settings.LastRun).OrderBy(o => o.Date)
                    .ToList();

                if (announcements.Any())
                {
                    announcements.ForEach(ann =>
                    {
                        ann.Post = ann.Post.Replace("\r", "");
                        ann.Post = Regex.Replace(ann.Post, @"<[^>]+>|&nbsp;", "").Trim();
                        ann.Post = Regex.Replace(ann.Post, @"\s{2,}", " ");
                        RecruitmentController.SendMessage(FormatMessage(ann), Properties.Settings.Default.NewsRoomName);
                        RecruitmentController.SendMessage(FormatMessage(ann), Properties.Settings.Default.MainRoomName);
                    });
                    settings.LastRun = announcements.Last().Date;
                }
            }
        }
        private MessagePayload FormatMessage(Announcement ann)
        {
            MessagePayload message = new MessagePayload() {Text = "CODE PINK" };
            message.Attachments = new List<MessagePayloadAttachment>();
            string colour = "#FF3399";

            message.Attachments.Add(new MessagePayloadAttachment()
            {
                Text = ann.Post,
                TitleLink = string.Format("http://forums.r3mus.org/chat/{0}", ann.Topic.ToLower().Replace(" ", "-")),
                Title = ann.Topic,
                ThumbUrl = "http://www.r3mus.org/Images/logo.png",
                AuthorName = ann.UserName,
                AuthorIcon = string.Format("http://image.eveonline.com/Character/{0}_32.jpg", JKON.EveWho.Api.GetCharacterID(ann.UserName)),
                Colour = colour
            });

            return message;
        }
    }
}