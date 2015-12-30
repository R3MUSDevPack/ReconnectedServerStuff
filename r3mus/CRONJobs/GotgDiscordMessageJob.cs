using Quartz;
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
        private string SlackRoom = "fleets";
        private string DiscordEmail = "clydeenmarland@r3mus.org";
        private string DiscordPassword = "p33K4800";
        private long DiscordChannel = 126512244916748288;

        public void Execute(IJobExecutionContext context)
        {
            var name = MethodBase.GetCurrentMethod().DeclaringType.Name;
            var db = new r3mus_DBEntities();
            CrossPost(db.CRONJobs.Where(job => job.JobName == name).FirstOrDefault());
        }

        private void CrossPost(CRONJob settings)
        {
            if (settings.Enabled)
            {
                try
                {
                    var client = new Client { UserName = DiscordEmail, Password = DiscordPassword };
                    if (client.Logon())
                    {
                        var messages = client.GetMessages(DiscordChannel);

                        if (settings.LastRun == null)
                        {
                            messages = messages.OrderBy(msg => msg.timestamp).ToList();
                            messages.Where(msg => ((DateTime.Now - msg.timestamp).Days < 1) && (msg.content.Contains("To: coalition_pings"))).ToList().ForEach(msg =>
                            {
                                var payload = new MessagePayload();
                                payload.Attachments = new List<MessagePayloadAttachment>();
                                payload.Attachments.Add(new MessagePayloadAttachment()
                                {
                                    Text = msg.content.Replace("@everyone", "@channel"),
                                    Title = string.Format("{0}: Message from {1}", msg.timestamp.ToString("yyyy-MM-dd HH:mm:ss"), msg.author.username),
                                    Colour = "#ff6600"
                                });
                                Plugin.SendToRoom(payload, SlackRoom, Properties.Settings.Default.SlackWebhook, msg.author.username);
                            });
                        }
                        else
                        {
                            messages = messages.OrderBy(msg => msg.timestamp).ToList();
                            messages.Where(msg =>
                            (msg.timestamp.AddMilliseconds(-msg.timestamp.Millisecond) > settings.LastRun.Value.AddMilliseconds(-settings.LastRun.Value.Millisecond)
                            &&
                            (msg.content.Contains("To: coalition_pings"))
                            )).ToList().ForEach(msg =>
                            {
                                var payload = new MessagePayload();
                                payload.Attachments = new List<MessagePayloadAttachment>();
                                payload.Attachments.Add(new MessagePayloadAttachment()
                                {
                                //AuthorName = msg.author.username,
                                //AuthorIcon = "http://www.r3mus.org/Images/jarvis.png",
                                Text = msg.content.Replace("@everyone", "@channel"),
                                    Title = string.Format("{0}: Message from {1}", msg.timestamp.ToString("yyyy-MM-dd HH:mm:ss"), msg.author.username),
                                    Colour = "#ff6600"
                                });
                                Plugin.SendToRoom(payload, SlackRoom, Properties.Settings.Default.SlackWebhook, msg.author.username);
                            });
                        }
                        settings.LastRun = messages.LastOrDefault().timestamp.AddMinutes(1);

                        client.LogOut();
                    }
                }
                catch (Exception ex)
                {

                }
            }
        }
    }
}