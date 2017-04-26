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
        public void Execute(IJobExecutionContext context)
        {
            var name = MethodBase.GetCurrentMethod().DeclaringType.Name;
            var db = new r3mus_DBEntities();
            CrossPost(db.CRONJobs.Where(job => job.JobName == name).FirstOrDefault());
            db.SaveChanges();
        }

        private void CrossPost(CRONJob settings)
        {
            if (settings.Enabled)
            {
                //Plugin.SendDirectMessage("Executing Jarvis", "clydeenmarland", Properties.Settings.Default.SlackWebhook);
                try
                {
                    var client = new Client {
                        UserName = Properties.Settings.Default.JarvisEmail,
                        Password = Properties.Settings.Default.JarvisPassword
                    };
                    if (client.Logon())
                    {
                        var messages = client.GetMessages(Properties.Settings.Default.JarvisDiscordRoom);
                        
                        if (settings.LastRun == null)
                        {
                            messages = messages.OrderBy(msg => msg.timestamp).ToList();
                            messages.Where(msg => ((DateTime.Now - msg.timestamp).Days < 1) && ((msg.content.Contains("To: coalition_pings")))).ToList().ForEach(msg =>
                            {
                                var senderlines = msg.content.Split(new[] { "\n" }, StringSplitOptions.None);

                                var payload = new MessagePayload();
                                payload.Text = "@channel: Coalition Broadcast";
                                payload.Attachments = new List<MessagePayloadAttachment>();
                                payload.Attachments.Add(new MessagePayloadAttachment()
                                {
                                    Text = new Censor().CensorText(string.Join("\n", senderlines.Skip(1))),
                                    Title = string.Format("{0}", msg.timestamp.ToString("yyyy-MM-dd HH:mm:ss")),
                                    Colour = "#ff6600"
                                });
                                Plugin.SendToRoom(payload, Properties.Settings.Default.JarvisSlackRoom, Properties.Settings.Default.SlackWebhook, msg.author.username);
                                
                                foreach(var webhook in Properties.Settings.Default.DiscordLinkSlackWebhooks)
                                {
                                    Plugin.SendToRoom(payload, Properties.Settings.Default.JarvisSlackRoom, webhook, msg.author.username);
                                }
                            });
                        }
                        else
                        {
                            messages = messages.OrderBy(msg => msg.timestamp).ToList();
                            messages = messages.Where(msg =>
                            (msg.timestamp > settings.LastRun.Value
                            &&
                            (msg.content.Contains("To: coalition_pings"))
                            )).ToList();
                            //Plugin.SendDirectMessage(string.Format("{0} messages to send", messages.Count.ToString()), "clydeenmarland", Properties.Settings.Default.SlackWebhook);

                            messages.ForEach(msg =>
                            {
                                var senderlines = msg.content.Split(new[] { "\n" }, StringSplitOptions.None);
                                var payload = new MessagePayload();
                                //payload.Text = string.Format("@channel: From {0}", msg.author.username);
                                payload.Text = "@channel: Coalition Broadcast";
                                payload.Attachments = new List<MessagePayloadAttachment>();
                                payload.Attachments.Add(new MessagePayloadAttachment()
                                {
                                    Text = new Censor().CensorText(string.Join("\n", senderlines.Skip(1))),
                                    Title = string.Format("{0}", msg.timestamp.ToString("yyyy-MM-dd HH:mm:ss")),
                                    //Title = string.Format("{0}: Message from {1}", msg.timestamp.ToString("yyyy-MM-dd HH:mm:ss"), msg.author.username),
                                    Colour = "#ff6600"
                                });
                                Plugin.SendToRoom(payload, Properties.Settings.Default.JarvisSlackRoom, Properties.Settings.Default.SlackWebhook, msg.author.username);
                                foreach (var webhook in Properties.Settings.Default.DiscordLinkSlackWebhooks)
                                {
                                    Plugin.SendToRoom(payload, Properties.Settings.Default.JarvisSlackRoom, webhook, msg.author.username);
                                }
                            });
                        }
                        settings.LastRun = messages.LastOrDefault().timestamp.AddMilliseconds(-messages.LastOrDefault().timestamp.Millisecond).AddMinutes(1);

                        client.LogOut();
                    }
                    else
                    {
                        //Plugin.SendDirectMessage("Logged in failed", "clydeenmarland", Properties.Settings.Default.SlackWebhook);
                    }
                }
                catch (Exception ex)
                {
                    //Plugin.SendDirectMessage(ex.Message, "clydeenmarland", Properties.Settings.Default.SlackWebhook);
                }
            }
        }

        private List<Message> GetMessages(long roomId)
        {
            var client = new Client
            {
                UserName = Properties.Settings.Default.JarvisEmail,
                Password = Properties.Settings.Default.JarvisPassword
            };
            return client.GetMessages(room);
        }

        private void SendMessages(List<Message> messages, string room)
        {

        }
    }
}