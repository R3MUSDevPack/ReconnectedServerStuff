﻿using System;
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
using System.Text.RegularExpressions;
using eZet.EveLib.StaticDataModule;
using eZet.EveLib.StaticDataModule.Models;
using R3MUS.Devpack.Slack;
using JKON.EveWho.Universe;
using JKON.EveWho.Types;
using System.Threading;

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
                        SendLossMessage(HyperFormatLolMessage(arg));
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
        public static DateTime RoundToHour(DateTime input)
        {
            //return new DateTime(input.Year, input.Month, input.Day, input.Hour, 0, 0);

            long ticks = input.Ticks;
            return new DateTime(ticks - ticks % 36000000000, input.Kind);
        }

        private static void CheckKills(string corpName, long corpId)
        {
            string killKey = "StartDate_Kills";
            string lossKey = "StartDate_Losses";

            DateTime LatestKill = Convert.ToDateTime(ConfigurationSettings.AppSettings[killKey]);//.AddMinutes(1);
            DateTime LatestLoss = Convert.ToDateTime(ConfigurationSettings.AppSettings[lossKey]);//.AddMinutes(1);

            if(Properties.Settings.Default.Debug)
            {
                Console.WriteLine(string.Concat("Last Kill: ", LatestKill.ToString("yyyy-MM-dd HH:mm:ss")));
                Console.WriteLine(string.Concat("Last Loss: ", LatestLoss.ToString("yyyy-MM-dd HH:mm:ss")));
            }
            
            List<ZkbResponse.ZkbKill> Kills;
            List<ZkbResponse.ZkbKill> Losses;

            try
            {
                Kills = GetZKBResponse(corpId, RoundToHour(LatestKill), ZKBType.Kill).Value.Where(kill => kill.KillTime > LatestKill).ToList<ZkbResponse.ZkbKill>();
                if (Properties.Settings.Default.Debug)
                {
                    Console.WriteLine(string.Concat(Kills.Count().ToString(), " Kills"));
                }
                if (Kills.Count() > 0)
                {
                    Kills.ForEach(kill =>
                    {
                        //Console.WriteLine(FormatKillMessage(kill, corpName, corpId));
                        SendKillMessage(HyperFormatKillMessage(kill, corpName, corpId));
                    });

                    UpdateRunTime(Kills.Max(kill => kill.KillTime), killKey);
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
                Losses = GetZKBResponse(corpId, RoundToHour(LatestLoss), ZKBType.Loss).Value.Where(kill => kill.KillTime > LatestLoss).ToList<ZkbResponse.ZkbKill>();
                //var check = GetZKBResponse(corpId, RoundToHour(LatestLoss), ZKBType.Loss).Value;
                if (Properties.Settings.Default.Debug)
                {
                    Console.WriteLine(string.Concat(Losses.Count().ToString(), " Losses"));
                }
                if (Losses.Count() > 0)
                {
                    Losses.ForEach(kill => {
                        //Console.WriteLine(FormatKillMessage(kill, corpName, corpId));
                        SendLossMessage(HyperFormatKillMessage(kill, corpName, corpId));
                    });

                    UpdateRunTime(Losses.Max(kill => kill.KillTime), lossKey);
                }
            }
            catch (Exception Ex)
            {
                if (Properties.Settings.Default.Debug)
                {
                    SendPM(Ex.Message);
                }
            }
            if (Properties.Settings.Default.Debug)
            {
                Console.ReadLine();
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

        private static void SendKillMessage(MessagePayload message)
        {
            if (Properties.Settings.Default.Plugin.ToUpper() == "HIPCHAT")
            {
                //Hipchat.SendToRoom(message, Properties.Settings.Default.RoomName, Properties.Settings.Default.HipchatToken);
            }
            else if (Properties.Settings.Default.Plugin.ToUpper() == "SLACK")
            {
                //message = Linkify(message);
                if (Properties.Settings.Default.SlackWebhook != string.Empty)
                {
                    Plugin.SendToRoom(message, Properties.Settings.Default.KillRoomName, Properties.Settings.Default.SlackWebhook, Properties.Settings.Default.BotName);
                }
            }
        }

        private static void SendLossMessage(MessagePayload message)
        {
            if (Properties.Settings.Default.Plugin.ToUpper() == "HIPCHAT")
            {
                //Hipchat.SendToRoom(message, Properties.Settings.Default.RoomName, Properties.Settings.Default.HipchatToken);
            }
            else if (Properties.Settings.Default.Plugin.ToUpper() == "SLACK")
            {
                //message = Linkify(message);
                Plugin.SendToRoom(message, Properties.Settings.Default.LossRoomName, Properties.Settings.Default.SlackWebhook, Properties.Settings.Default.BotName);
            }
        }

        //private static void SendMessage(string message)
        //{
        //    if (Properties.Settings.Default.Plugin.ToUpper() == "HIPCHAT")
        //    {
        //        Hipchat.SendToRoom(message, Properties.Settings.Default.KillRoomName, Properties.Settings.Default.HipchatToken);
        //    }
        //    else if (Properties.Settings.Default.Plugin.ToUpper() == "SLACK")
        //    {
        //        //message = Linkify(message);
        //        Slack.SendToRoom(message, Properties.Settings.Default.KillRoomName, Properties.Settings.Default.SlackWebhook);
        //    }
        //}

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
                Plugin.SendDirectMessage(message, "ClydeenMarland", Properties.Settings.Default.SlackWebhook);
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
                Plugin.SendDirectMessage(message, "ClydeenMarland", Properties.Settings.Default.SlackWebhook);
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
            //EveAI.Map.SolarSystem system = GetSolarSystem(kill.SolarSystemId);

            var system = GetSolarSystem(kill.SolarSystemId);

            ZkbResponse.ZkbStats stats = kill.Stats;

            string killTitle = string.Format(Properties.Settings.Default.MessageFormatLine1, corpName, type, kill.KillTime.ToString());
            //messageLines.Add(killTitle);

            string useName;

            switch(kill.Victim.CharacterName)
            {
                case null:
                case "":
                    useName = string.Format("{0} ({1})", kill.Victim.CorporationName,
                        kill.Victim.AllianceName != string.Empty ? kill.Victim.AllianceName : "No Alliance");
                    break;
                default:
                    useName = kill.Victim.CharacterName;
                    break;
            }

            string killLine1 = string.Format(Properties.Settings.Default.MessageFormatLine2, useName, GetProductType(kill.Victim.ShipTypeId).Name, system.Name, system.Constellation.Region.Name);
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
            //EveAI.Map.SolarSystem system = GetSolarSystem(kill.SolarSystemId);

            var system = GetSolarSystem(kill.SolarSystemId);

            ZkbResponse.ZkbStats stats = kill.Stats;

            messageLines.Add(string.Format(Properties.Settings.Default.MessageFormatLine1, corpName, type, kill.KillTime.ToString()));
            messageLines.Add(string.Format(Properties.Settings.Default.MessageFormatLine2, kill.Victim.CharacterName, 
                GetProductType(kill.Victim.ShipTypeId).Name, 
                system.Name, system.Constellation.Region.Name));

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

        private static EveAI.Map.SolarSystem GetSolarSystem_Old (int systemId)
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

        private static SolarSystem GetSolarSystem(long systemId)
        {
            return new SolarSystem(systemId);
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

        private static ItemType GetProductType(long typeId)
        {
            try
            {
                var type = new ItemType(typeId);
                if (type.Name.ToLower() == "#system")
                {
                    type.Name = LookupShipName(typeId);
                }
                return type;
            }
            catch
            {
                var erroredType = new ItemType();
                erroredType.Id = typeId;
                erroredType.Name = GetProductType_Old((int)typeId).Name;
                return erroredType;
            }
        }

        private static ProductType GetProductType_Old(int shipTypeId)
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
        
        private static string LookupShipName(long shipTypeId)
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
