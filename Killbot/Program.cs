using System;
using System.Collections.Generic;
using System.Linq;
using System.Configuration;
using System.Reflection;

using EveAI.Product;
using EveAI.Live;
using EveAI.Live.Account;
using EveAI.Live.Corporation;

using eZet.EveLib.ZKillboardModule;
using eZet.EveLib.ZKillboardModule.Models;

using Hipchat_Plugin;
using Slack_Plugin;
using System.Text.RegularExpressions;
using JKON.Slack;
using eZet.EveLib.StaticDataModule;
using eZet.EveLib.StaticDataModule.Models;

namespace Killbot
{
    class Program
    {
        enum ZKBType
        {
            Kill, 
            Loss
        }

        static void Main(string[] args)
        {
            if ((args.Length > 0) && (args[0].Contains("lol")))
            {
                args.ToList().Skip(1).ToList().ForEach(arg =>
                {
                    try
                    {
                        SendMessage(HyperFormatLolMessage(arg));
                    }
                    catch (Exception ex)
                    {
                        SendPM(ex.Message);
                    }
                });
            }
            else
            {
                try
                {
                    MessagePayload p = new MessagePayload();
                    p.Attachments = new List<MessagePayloadAttachment>();

                    if ((Properties.Settings.Default.CorpId == null) || (Properties.Settings.Default.CorpId == string.Empty))
                    {
                        CorporationSheet corpSheet = GetCorpDetails();
                        if (Properties.Settings.Default.Debug)
                        {
                            SendPM(string.Format("Corpsheet for {0} obtained.", corpSheet.Ticker));
                        }
                        CheckKills(corpSheet.Ticker, corpSheet.CorporationID);
                    }
                    else
                    {
                        CheckKills(Properties.Settings.Default.CorpTicker, Convert.ToInt64(Properties.Settings.Default.CorpId));
                    }
                }
                catch (Exception ex)
                {
                    SendPM(ex.Message);
                }
            }
        }

        //private static void CheckKills()
        //{
        //    //string killKey = "StartDate_Kills";
        //    //string lossKey = "StartDate_Losses";

        //    //DateTime LatestKill = Convert.ToDateTime(ConfigurationSettings.AppSettings[killKey]).AddSeconds(1);
        //    //DateTime LatestLoss = Convert.ToDateTime(ConfigurationSettings.AppSettings[lossKey]).AddSeconds(1);

        //    //KeyValuePair<DateTime, List<ZkbResponse.ZkbKill>> Kills;
        //    //KeyValuePair<DateTime, List<ZkbResponse.ZkbKill>> Losses;

        //    try
        //    {
        //        Kills = GetZKBResponse(corpId, LatestKill, ZKBType.Kill);
        //        if (Kills.Value.Count() > 0)
        //        {
        //            Kills.Value.ForEach(kill => {
        //                //Console.WriteLine(FormatKillMessage(kill, corpName, corpId));
        //                SendMessage(HyperFormatKillMessage(kill, corpName, corpId));
        //            });

        //            UpdateRunTime(Kills.Key, killKey);
        //        }
        //    }
        //    catch (Exception Ex)
        //    {
        //        if (Properties.Settings.Default.Debug)
        //        {
        //            SendPM(Ex.Message);
        //        }
        //    }

        //    try
        //    {
        //        Losses = GetZKBResponse(corpId, LatestLoss, ZKBType.Loss);
        //        if (Losses.Value.Count() > 0)
        //        {
        //            Losses.Value.ForEach(kill => {
        //                //Console.WriteLine(FormatKillMessage(kill, corpName, corpId));
        //                SendMessage(HyperFormatKillMessage(kill, corpName, corpId));
        //            });

        //            UpdateRunTime(Losses.Key, lossKey);
        //        }
        //    }
        //    catch (Exception Ex)
        //    {
        //        if (Properties.Settings.Default.Debug)
        //        {
        //            SendPM(Ex.Message);
        //        }
        //    }
        //}

        private static void CheckKills(string corpName, long corpId)
        {
            string killKey = "StartDate_Kills";
            string lossKey = "StartDate_Losses";

            DateTime LatestKill = Convert.ToDateTime(ConfigurationSettings.AppSettings[killKey]).AddMinutes(1);
            DateTime LatestLoss = Convert.ToDateTime(ConfigurationSettings.AppSettings[lossKey]).AddMinutes(1);
            
            KeyValuePair<DateTime, List<ZkbResponse.ZkbKill>> Kills;
            KeyValuePair<DateTime, List<ZkbResponse.ZkbKill>> Losses;

            try
            {
                Kills = GetZKBResponse(corpId, LatestKill, ZKBType.Kill);
                if (Kills.Value.Count() > 0)
                {
                    Kills.Value.ForEach(kill => {
                        //Console.WriteLine(FormatKillMessage(kill, corpName, corpId));
                        SendMessage(HyperFormatKillMessage(kill, corpName, corpId));
                    });

                    UpdateRunTime(Kills.Key, killKey);
                }
            }
            catch (Exception Ex)
            {
                if (Properties.Settings.Default.Debug)
                {
                    SendPM(Ex.Message);
                }
            }

            try
            {
                Losses = GetZKBResponse(corpId, LatestLoss, ZKBType.Loss);
                if (Losses.Value.Count() > 0)
                {
                    Losses.Value.ForEach(kill => {
                        //Console.WriteLine(FormatKillMessage(kill, corpName, corpId));
                        SendMessage(HyperFormatKillMessage(kill, corpName, corpId));
                    });

                    UpdateRunTime(Losses.Key, lossKey);
                }
            }
            catch (Exception Ex)
            {
                if (Properties.Settings.Default.Debug)
                {
                    SendPM(Ex.Message);
                }
            }
        }

        private static KeyValuePair<DateTime, List<ZkbResponse.ZkbKill>> GetZKBResponse(long corpId, DateTime startTime, ZKBType type)
        {
            ZkbResponse Kills;
            ZKillboard kb = new ZKillboard();
            ZKillboardOptions Options = new ZKillboardOptions();
            List<ZkbResponse.ZkbKill> OrderedKills;
            Options.CorporationId.Add(corpId);

            if (startTime > DateTime.MinValue)
            {
                Options.StartTime = startTime;
            }
            if (Properties.Settings.Default.Debug)
            {
                SendPM(string.Format("Using StartTime {0}.", startTime.ToString("yyyy-MM-dd HH:mm:ss")));
            }

            if(type == ZKBType.Kill)
            {
                Kills = kb.GetKills(Options);
            }
            else
            {
                Kills = kb.GetLosses(Options);
            }
            OrderedKills = Kills.OrderBy(Kill => Kill.KillTime).ToList();

            return new KeyValuePair<DateTime, List<ZkbResponse.ZkbKill>>(OrderedKills.Last().KillTime, OrderedKills);
        }

        private static void SendMessage(MessagePayload message)
        {
            if (Properties.Settings.Default.Plugin.ToUpper() == "HIPCHAT")
            {
                //Hipchat.SendToRoom(message, Properties.Settings.Default.RoomName, Properties.Settings.Default.HipchatToken);
            }
            else if (Properties.Settings.Default.Plugin.ToUpper() == "SLACK")
            {
                //message = Linkify(message);
                Slack.SendToRoom(message, Properties.Settings.Default.RoomName, Properties.Settings.Default.SlackWebhook);
            }
        }

        private static void SendMessage(string message)
        {
            if (Properties.Settings.Default.Plugin.ToUpper() == "HIPCHAT")
            {
                Hipchat.SendToRoom(message, Properties.Settings.Default.RoomName, Properties.Settings.Default.HipchatToken);
            }
            else if (Properties.Settings.Default.Plugin.ToUpper() == "SLACK")
            {
                //message = Linkify(message);
                Slack.SendToRoom(message, Properties.Settings.Default.RoomName, Properties.Settings.Default.SlackWebhook);
            }
        }

        private static string Linkify(string SearchText)
        {
            Regex regx = new Regex(@"\b(((\S+)?)(@|mailto\:|(news|(ht|f)tp(s?))\://)\S+)\b", RegexOptions.IgnoreCase);
            SearchText = SearchText.Replace("&nbsp;", " ");
            MatchCollection matches = regx.Matches(SearchText);

            foreach (Match match in matches)
            {
                if (match.Value.StartsWith("http"))
                { // if it starts with anything else then dont linkify -- may already be linked!
                    SearchText = SearchText.Replace(match.Value, "<a href='" + match.Value + "'>" + match.Value + "</a>");
                }
            }

            return SearchText;
        }

        private static void SendPM(string message)
        {
            if (Properties.Settings.Default.Plugin.ToUpper() == "HIPCHAT")
            {
                Hipchat.SendPM(message, Properties.Settings.Default.HipchatToken, "Clyde en Marland");
            }
            else if (Properties.Settings.Default.Plugin.ToUpper() == "SLACK")
            {
                Slack.SendPM(message, "ClydeenMarland", Properties.Settings.Default.SlackWebhook);
            }
        }

        private static void SendPM(MessagePayload message)
        {
            if (Properties.Settings.Default.Plugin.ToUpper() == "HIPCHAT")
            {
                //Hipchat.SendPM(message, Properties.Settings.Default.HipchatToken, "Clyde en Marland");
            }
            else if (Properties.Settings.Default.Plugin.ToUpper() == "SLACK")
            {
                Slack.SendPM(message, "ClydeenMarland", Properties.Settings.Default.SlackWebhook);
            }
        }

        private static MessagePayload HyperFormatLolMessage(string name)
        {
            MessagePayload message = new MessagePayload();
            message.Attachments = new List<MessagePayloadAttachment>();

            string type;
            List<string> messageLines = new List<string>();
            
            type = "KILL";
            
            string killTitle = string.Format(Properties.Settings.Default.MessageFormatLine1, "R3MUS", type, DateTime.Now.AddMinutes(-10).ToString());
            //messageLines.Add(killTitle);
            string killLine1 = string.Format("{0} lost a capsule", name, "", "Jita", "The Forge");
            messageLines.Add(killLine1);
            
            messageLines.Add(string.Format(Properties.Settings.Default.MessageFormatLine3, "The Mittani", "Procurer"));
            
            messageLines.Add(string.Format(Properties.Settings.Default.MessageFormatLine4, "stelios102", "Venture"));
            messageLines.Add(string.Format(Properties.Settings.Default.MessageFormatLine4, "Vas Enyo", "Gallente Shuttle"));

            messageLines.Add(string.Format(Properties.Settings.Default.MessageFormatLine5, "200,000,000"));
            messageLines.Add(string.Empty);

            message.Attachments.Add(new MessagePayloadAttachment()
            {
                Text = String.Join("\n", messageLines.ToArray()),
                TitleLink = "http://www.troll.me/images/pissed-off-obama/you-have-been-pwned-by-the-troll-king.jpg",
                Title = killTitle,
                ThumbUrl = string.Empty
            });

            if (type == "KILL")
            {
                message.Attachments.First().Colour = "#00FF00";
            }
            else if (type == "LOSS")
            {
                message.Attachments.First().Colour = "#FF0000";
            }

            return message;
        }

        private static MessagePayload HyperFormatKillMessage(ZkbResponse.ZkbKill kill, string corpName, long corpId)
        {
            MessagePayload message = new MessagePayload();
            message.Attachments = new List<MessagePayloadAttachment>();

            string type;
            List<string> messageLines = new List<string>();

            if (kill.Victim.CorporationId == corpId)
            {
                type = "LOSS";
            }
            else
            {
                type = "KILL";
            }
            EveAI.Map.SolarSystem system = GetSolarSystem(kill.SolarSystemId);

            ZkbResponse.ZkbStats stats = kill.Stats;

            string killTitle = string.Format(Properties.Settings.Default.MessageFormatLine1, corpName, type, kill.KillTime.ToString());
            //messageLines.Add(killTitle);
            string killLine1 = string.Format(Properties.Settings.Default.MessageFormatLine2, kill.Victim.CharacterName, GetProductType(kill.Victim.ShipTypeId).Name, system.Name, system.Region.Name);
            messageLines.Add(killLine1);

            foreach (ZkbResponse.ZkbAttacker Attacker in kill.Attackers)
            {
                if (Attacker.FinalBlow == true)
                {
                    messageLines.Add(string.Format(Properties.Settings.Default.MessageFormatLine3, Attacker.CharacterName, GetProductType(Attacker.ShipTypeId).Name));
                }
            }
            foreach (ZkbResponse.ZkbAttacker Attacker in kill.Attackers)
            {
                if (Attacker.CorporationId == corpId)
                {
                    messageLines.Add(string.Format(Properties.Settings.Default.MessageFormatLine4, Attacker.CharacterName, GetProductType(Attacker.ShipTypeId).Name));
                }
            }
            if (stats != null)
            {
                messageLines.Add(string.Format(Properties.Settings.Default.MessageFormatLine5, stats.TotalValue.ToString("N2")));
            }
            string killUrl = string.Format(Properties.Settings.Default.KillURL, kill.KillId.ToString());
            messageLines.Add(string.Empty);
            
            message.Attachments.Add(new MessagePayloadAttachment() { 
                Text = String.Join("\n", messageLines.ToArray()),
                TitleLink = killUrl, 
                Title = killTitle,
                ThumbUrl = string.Format(Properties.Settings.Default.ShipImageUrl, kill.Victim.ShipTypeId.ToString())
            });

            if(type == "KILL")
            {
                message.Attachments.First().Colour = "#00FF00";
            }
            else if (type == "LOSS")
            {
                message.Attachments.First().Colour = "#FF0000";
            }

            return message;
        }

        private static string FormatKillMessage(ZkbResponse.ZkbKill kill, string corpName, long corpId)
        {
            string type;
            List<string> messageLines = new List<string>();
            string message = string.Empty;

            if (kill.Victim.CorporationId == corpId)
            {
                type = "LOSS";
            }
            else
            {
                type = "KILL";
            }
            EveAI.Map.SolarSystem system = GetSolarSystem(kill.SolarSystemId);

            ZkbResponse.ZkbStats stats = kill.Stats;

            messageLines.Add(string.Format(Properties.Settings.Default.MessageFormatLine1, corpName, type, kill.KillTime.ToString()));
            messageLines.Add(string.Format(Properties.Settings.Default.MessageFormatLine2, kill.Victim.CharacterName, 
                GetProductType(kill.Victim.ShipTypeId).Name, 
                system.Name, system.Region.Name));

            foreach (ZkbResponse.ZkbAttacker Attacker in kill.Attackers)
            {
                if (Attacker.FinalBlow == true)
                {
                    messageLines.Add(string.Format(Properties.Settings.Default.MessageFormatLine3, Attacker.CharacterName, GetProductType(Attacker.ShipTypeId).Name));
                }
            }
            foreach (ZkbResponse.ZkbAttacker Attacker in kill.Attackers)
            {
                if (Attacker.CorporationId == corpId)
                {
                    messageLines.Add(string.Format(Properties.Settings.Default.MessageFormatLine4, Attacker.CharacterName, GetProductType(Attacker.ShipTypeId).Name));
                }
            }
            if (stats != null)
            {
                messageLines.Add(string.Format(Properties.Settings.Default.MessageFormatLine5, stats.TotalValue.ToString("N2")));
            }
            string killUrl = string.Format(Properties.Settings.Default.KillURL, kill.KillId.ToString());
            messageLines.Add(string.Format(Properties.Settings.Default.MessageFormatLine6, killUrl));

            message = String.Join("\n", messageLines.ToArray());
            return message;
        }

        private static EveAI.Map.SolarSystem GetSolarSystem(int systemId)
        {
            EveApi API;
            APIKeyInfo KeyInfo;
            EveAI.Map.SolarSystem system;

            try
            {
                API = new EveApi("Clyde en Marland's API Checker", Properties.Settings.Default.CorpAPI, Properties.Settings.Default.VCode);
                KeyInfo = API.GetApiKeyInfo();

                system = EveApi.EveApiCore.FindSolarSystem(systemId);
            }
            catch (Exception Ex)
            {
                system = new EveAI.Map.SolarSystem();
                system.Name = string.Concat("Error querying API server; ", Ex.Message);
            }

            return system;
        }

        private static CorporationSheet GetCorpDetails()
        {
            EveApi API = new EveApi("Clyde en Marland's KillBot", Properties.Settings.Default.CorpAPI, Properties.Settings.Default.VCode);

            return API.GetCorporationSheet();
        }

        private static InvType GetProductType1(int productId)
        {
            return new EveStaticData().GetInvType(productId);
        }

        private static ProductType GetProductType(int shipTypeId)
        {
            EveApi API;
            APIKeyInfo KeyInfo;
            ProductType PType;

            try
            {
                API = new EveApi("Clyde en Marland's API Checker", Properties.Settings.Default.CorpAPI, Properties.Settings.Default.VCode);
                KeyInfo = API.GetApiKeyInfo();

                PType = EveApi.EveApiCore.FindProductType(shipTypeId);
            }
            catch (Exception Ex)
            {
                PType = new ProductType();
                PType.Name = "Error querying API server";
                PType.Description = Ex.Message;
            }

            if (PType == null)
            {
                PType = new ProductType();
                PType.Name = LookupShipName(shipTypeId);
            }

            return PType;
        }
        
        private static string LookupShipName(int shipTypeId)
        {
            string result = string.Empty;

            try
            {
                result = ConfigurationManager.AppSettings[shipTypeId.ToString()];
            }
            catch (Exception ex)
            {
            }
            if ((result == string.Empty) || result == null)
            {
                result = string.Concat("unknown ship ID: ", shipTypeId.ToString());
            }

            return result;
        }

        private static void UpdateRunTime(DateTime writeThis, string key)
        {

            if (Properties.Settings.Default.Debug)
            {
                SendPM(string.Format("Updating run time: {0}.", writeThis.ToString("yyyy-MM-dd HH:mm:ss")));
            }

            Configuration config = ConfigurationManager.OpenExeConfiguration(Assembly.GetExecutingAssembly().Location);
            config.AppSettings.Settings.Remove(key);
            config.AppSettings.Settings.Add(key, writeThis.ToString("yyyy-MM-dd HH:mm:ss"));
            config.Save();
        }
    }
}
