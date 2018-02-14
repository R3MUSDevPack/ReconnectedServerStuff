using Quartz;
using r3mus.Controllers;
using r3mus.Models;
using R3MUS.Devpack.Discord;
using R3MUS.Devpack.Slack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web;

namespace r3mus.CRONJobs
{
    public class GotgDiscordMessageJob : IJob
    {
        public void Execute(IJobExecutionContext context)
        {
            var name = MethodBase.GetCurrentMethod().DeclaringType.Name;
            var db = new r3mus_DBEntities();
            CrossPost(db.CRONJobs.Where(job => job.JobName == name).FirstOrDefault());
            db.SaveChanges();
        }

        private void CrossPost(CRONJob settings)
        {
            if(settings.Enabled)
            {
                var messages = new List<Message>();
                try
                {
                    var client = new Client
                    {
                        UserName = Properties.Settings.Default.JarvisEmail,
                        Password = Properties.Settings.Default.JarvisPassword
                    };
                    if (client.Logon())
                    {
						PostError("Logged on to Discord");
                        foreach (var link in Properties.Settings.Default.DiscordToSlackLinks)
						{
							PostError(link);
							var splt = link.Split('/');
                            var discordRoom = splt[0];
                            var slackRoom = splt[1];
                            var filter = splt[2];

                            var localMessages = GetMessages(discordRoom, settings.LastRun, client, filter);

							PostError(string.Format("{0} messages", localMessages.Count.ToString()));

							SendMessages(localMessages, slackRoom);
                            messages.AddRange(localMessages);
                        }
                    }
                    settings.LastRun = messages.OrderByDescending(s => s.timestamp)
                        .LastOrDefault().timestamp.AddMilliseconds(-messages.LastOrDefault().timestamp.Millisecond).AddMinutes(1);

                    client.LogOut();
                }
                catch(Exception ex)
				{
					Console.WriteLine(ex.Message);
				}
            }
        }

		private void PostError(string text)
		{
			var message = new JKON.Slack.MessagePayload();
			message.Attachments = new List<JKON.Slack.MessagePayloadAttachment>();

			message.Attachments.Add(new JKON.Slack.MessagePayloadAttachment()
			{
				Text = text,
				TitleLink = "Discord to Slack Error",
				Title = "Check this out!"
			});
			RecruitmentController.SendMessage(message, "it_testing");
		}
		
        private List<Message> GetMessages(string discordRoom, DateTime? last, Client client, string filter)
		{
			PostError("GetMessages");
			last = last.HasValue ? last : DateTime.Now.AddDays(-1);
            return client.GetMessages(Convert.ToInt64(discordRoom)).Where(w => w.timestamp > last.Value && w.content.Contains(filter)).OrderBy(msg => msg.timestamp).ToList();
        }

        private void SendMessages(List<Message> messages, string slackRoom)
        {
			PostError("SendMessages");

			messages.ForEach(message =>
            {
                foreach (var webhook in Properties.Settings.Default.DiscordLinkSlackWebhooks)
                {
					PostError(string.Format("{0} - {1} - {2}", webhook, slackRoom, message.author.username));
					PostError(Newtonsoft.Json.JsonConvert.SerializeObject(message));
					Plugin.SendToRoom(FormatMessage(message), slackRoom, webhook, message.author.username);
                }
            });
        }

        private MessagePayload FormatMessage(Message message)
		{
			PostError("FormatMessage");
			var senderlines = message.content.Split(new[] { "\n" }, StringSplitOptions.None);
            var payload = new MessagePayload();
            payload.Text = "@channel: Coalition Broadcast";
            payload.Attachments = new List<MessagePayloadAttachment>();
            payload.Attachments.Add(new MessagePayloadAttachment()
            {
                Text = new Censor().CensorText(string.Join("\n", senderlines.Skip(1))),
                Title = string.Format("{0}", message.timestamp.ToString("yyyy-MM-dd HH:mm:ss")),
                Colour = "#ff6600"
            });
            return payload;
        }

        private List<Message> GetMessages(long roomId)
        {
            var client = new Client
            {
                UserName = Properties.Settings.Default.JarvisEmail,
                Password = Properties.Settings.Default.JarvisPassword
            };
            return client.GetMessages(roomId);
        }        
    }
}