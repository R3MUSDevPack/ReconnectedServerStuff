using agsXMPP;
using agsXMPP.protocol.client;
using agsXMPP.Collections;
using agsXMPP.protocol.iq.roster;
using R3MUS.Devpack.Slack;
using System.Collections.Generic;
using System;

namespace r3mus.Models
{
    public class JabberListener
    {
        protected XmppClientConnection xmpp { get; set; }

        public JabberListener()
        {
            Jid jidSender = new Jid(Properties.Settings.Default.JabberName);
            xmpp = new XmppClientConnection(jidSender.Server);
            xmpp.Open(jidSender.User, Properties.Settings.Default.JabberPWd);

            xmpp.OnLogin += new ObjectHandler(xmpp_OnLogin);
        }

        private void xmpp_OnLogin(object sender)
        {
            Presence p = new Presence(ShowType.chat, "Online");
            p.Type = PresenceType.invisible;
            xmpp.Send(p);

            xmpp.MessageGrabber.Add(new Jid(Properties.Settings.Default.JabberBroadcaster),
                                     new BareJidComparer(),
                                     new MessageCB(MessageCallBack),
                                     null);
        }
        private void MessageCallBack(object sender, Message msg, object data)
        {
            if (msg.Body != null)
            {
                var payload = new MessagePayload();
                payload.Attachments = new List<MessagePayloadAttachment>();
                if (!msg.Body.Contains("@everyone"))
                {
                    msg.Body = string.Concat("@everyone: ", msg.Body);
                }
                payload.Attachments.Add(new MessagePayloadAttachment()
                {
                    Text = msg.Body.Replace("@everyone", "@channel"),
                    Title = string.Format("{0}: Message from {1}", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), "Marvin"),
                    Colour = "#ff0066"
                });
                Plugin.SendToRoom(payload, Properties.Settings.Default.FleetRoomName, Properties.Settings.Default.SlackWebhook, Properties.Settings.Default.FleetBotName);

                if (msg.Body.Contains("bog_all"))
                {
                    foreach (var hook in Properties.Settings.Default.DiscordLinkSlackWebhooks)
                    {
                        Plugin.SendToRoom(payload, Properties.Settings.Default.FleetRoomName, hook, Properties.Settings.Default.FleetBotName);
                    }
                }
            }
        }
    }
}
