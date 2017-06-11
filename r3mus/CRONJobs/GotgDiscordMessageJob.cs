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
                        foreach (var link in Properties.Settings.Default.DiscordToSlackLinks)
                        {
                            var splt = link.Split('/');
                            var discordRoom = splt[0];
                            var slackRoom = splt[1];
                            var filter = splt[2];

                            var localMessages = GetMessages(discordRoom, settings.LastRun, client, filter);

                            SendMessages(localMessages, slackRoom);
                            messages.AddRange(localMessages);
                        }
                    }
                    settings.LastRun = messages.OrderByDescending(s => s.timestamp)
                        .LastOrDefault().timestamp.AddMilliseconds(-messages.LastOrDefault().timestamp.Millisecond).AddMinutes(1);

                    client.LogOut();
                }
                catch { }
            }
        }

        private void CrossPost1(CRONJob settings)
        {
            if (settings.Enabled)
            {
                try
                {
                    var client = new Client
                    {
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

                                foreach (var webhook in Properties.Settings.Default.DiscordLinkSlackWebhooks)
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
                catch { }
            }
        }

        private List<Message> GetMessages(string discordRoom, DateTime? last, Client client, string filter)
        {
            last = last.HasValue ? last : DateTime.Now.AddDays(-1);
            return client.GetMessages(Convert.ToInt64(discordRoom)).Where(w => w.timestamp > last.Value && w.content.Contains(filter)).OrderBy(msg => msg.timestamp).ToList();
        }

        private void SendMessages(List<Message> messages, string slackRoom)
        {
            messages.ForEach(message =>
            {
                foreach (var webhook in Properties.Settings.Default.DiscordLinkSlackWebhooks)
                {
                    Plugin.SendToRoom(FormatMessage(message), Properties.Settings.Default.JarvisSlackRoom, webhook, message.author.username);
                }
            });
        }

        private MessagePayload FormatMessage(Message message)
        {
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