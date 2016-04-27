﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using EveAI.Live;
using EveAI.Live.Account;
using EveAI.Live.Character;
using EveAI.Live.Corporation;

using EveAI.Product;
using Hipchat_Plugin;
using Slack_Plugin;
using JKON.EveWho.Models;
using JKON.EveWho.Stations;
using System.Configuration;
using System.Reflection;
using JKON.Slack;

namespace ContractNotifyBot
{
    class Program
    {
        static void Main(string[] args)
        {
            CheckContracts();
        }

        private static void CheckContracts()
        {
            EveApi api;
            List<EveAI.Live.Utility.Contract> Contracts;
            Dictionary<long, EveCharacter> Names = new Dictionary<long, EveCharacter>();
            List<long> IDs = new List<long>();
            var now = DateTime.Now;

            DateTime lastFullRunTime = GetLastRunTime();
            DateTime lastCompCheckTime = GetLastCompCheckTime();
            Console.WriteLine(string.Format("Start Time : {0}", lastFullRunTime.ToString("yyyy-MM-dd HH:mm:ss")));

            try
            {
                api = new EveApi("Clyde en Marland's Contract Notifier", (long)Properties.Settings.Default.CorpAPI, Properties.Settings.Default.VCode);
                Contracts = api.GetCorporationContracts().ToList().Where(contract => ((contract.DateIssued > lastFullRunTime) || (contract.DateCompleted > lastCompCheckTime))).ToList();
                Contracts = Contracts.Where(Contract => ((Contract.Type == EveAI.Live.Utility.Contract.ContractType.Courier) &&
                        ((Contract.Status == EveAI.Live.Utility.Contract.ContractStatus.Outstanding) || (Contract.Status == EveAI.Live.Utility.Contract.ContractStatus.Completed)))
                ).ToList();

                foreach(EveAI.Live.Utility.Contract Contract in Contracts)
                {
                    IDs.Add(Contract.IssuerID);
                }

                foreach (long Id in IDs)
                {
                    if (!Names.ContainsKey(Id))
                    {
                        Names.Add(Id, JKON.EveWho.Api.GetCharacter(Id));
                    }
                }
                
                foreach (EveAI.Live.Utility.Contract Contract in Contracts)
                {
                    //if ((Contract.Type == EveAI.Live.Utility.Contract.ContractType.Courier) && 
                    //    ((Contract.Status == EveAI.Live.Utility.Contract.ContractStatus.Outstanding) || (Contract.Status == EveAI.Live.Utility.Contract.ContractStatus.Completed)))
                    //{
                        //Console.WriteLine(string.Format("Contract notification: {0}", FormatMessage(Contract, Names[Contract.IssuerID].result.characterName)));
                        SendMessage(HyperFormatMessage(Contract, Names[Contract.IssuerID].result.characterName));
                        if ((Contract.Status == EveAI.Live.Utility.Contract.ContractStatus.Outstanding) && (Contract.DateIssued > lastFullRunTime)) { lastFullRunTime = Contract.DateIssued; }
                        else if ((Contract.Status == EveAI.Live.Utility.Contract.ContractStatus.Completed) && (Contract.DateCompleted > lastCompCheckTime)) { lastCompCheckTime = Contract.DateCompleted; }
                    //}
                }
                UpdateRunTime(lastFullRunTime);
                UpdateCompCheckTime(lastCompCheckTime);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            if(Environment.UserInteractive)
            {
                System.Threading.Thread.Sleep(10000);
            }
        }

        private static void UpdateRunTime(DateTime writeThis)
        {
            Console.WriteLine(string.Format("New LastRunTime: {0}", writeThis.ToString("yyyy-MM-dd HH:mm:ss")));
            Configuration config = ConfigurationManager.OpenExeConfiguration(Assembly.GetExecutingAssembly().Location);
            Console.WriteLine(string.Format("Updating Last Run Time in {0}", config.FilePath));
            config.AppSettings.Settings.Remove("LastCheckedAt");
            config.AppSettings.Settings.Add("LastCheckedAt", writeThis.ToString("yyyy-MM-dd HH:mm:ss"));
            config.Save();
        }

        private static void UpdateCompCheckTime(DateTime writeThis)
        {
            Console.WriteLine(string.Format("New LastCompCheckTime: {0}", writeThis.ToString("yyyy-MM-dd HH:mm:ss")));
            Configuration config = ConfigurationManager.OpenExeConfiguration(Assembly.GetExecutingAssembly().Location);
            Console.WriteLine(string.Format("Updating Last Completion Check Time in {0}", config.FilePath));
            config.AppSettings.Settings.Remove("LastCompCheckedAt");
            config.AppSettings.Settings.Add("LastCompCheckedAt", writeThis.ToString("yyyy-MM-dd HH:mm:ss"));
            config.Save();
        }

        //private static MessagePayload HyperFormatMessage(EveAI.Live.Utility.Contract contract, string IssuerName)
        //{
        //    MessagePayload message = new MessagePayload();
        //    message.Attachments = new List<MessagePayloadAttachment>();

        //    string type;
        //    List<string> messageLines = new List<string>();

        //    messageLines.Add(string.Format(Properties.Settings.Default.MessageFormatLine1, contract.DateIssued.ToString("yyyy-MM-dd hh:mm:ss")));
        //    messageLines.Add(string.Format(Properties.Settings.Default.MessageFormatLine2, IssuerName));
        //    messageLines.Add(string.Format(Properties.Settings.Default.MessageFormatLine3, contract.Reward.ToString()));

        //    var startStation = contract.StartStation;
        //    var endStation = contract.EndStation;
        //    string startStationName = string.Empty;
        //    string endStationName = string.Empty;


        //    if (startStation == null)
        //    {
        //        startStationName = Api.GetStation(contract.StartStationID).stationName;
        //    }
        //    else
        //    {
        //        startStationName = contract.StartStation.Name;
        //    }
        //    if (endStation == null)
        //    {
        //        endStationName = Api.GetStation(contract.EndStationID).stationName;
        //    }
        //    else
        //    {
        //        endStationName = contract.EndStation.Name;
        //    }

        //    try
        //    {
        //        messageLines.Add(string.Format(Properties.Settings.Default.MessageFormatLine4, startStationName, endStationName)); //, contract.StartStation.Name, contract.EndStation.Name));
        //    }
        //    catch (Exception ex) { }
        //    messageLines.Add(string.Format(Properties.Settings.Default.MessageFormatLine5, contract.Volume.ToString()));

        //    //message = String.Join("\n", messageLines.ToArray());

        //    message.Attachments.Add(new MessagePayloadAttachment()
        //    {
        //        Text = String.Join("\n", messageLines.ToArray()),
        //        TitleLink = string.Format(Properties.Settings.Default.StationURL, Api.GetStation(contract.EndStationID).),
        //        Title = "",
        //        ThumbUrl = string.Format(Properties.Settings.Default.StationURL, JKON.EveWho.Api.GetCharacterID(IssuerName), 64.ToString()),
        //    });

        //    return message;
        //}

        private static string FormatMessage(EveAI.Live.Utility.Contract contract, string IssuerName)
        {
            string type;
            List<string> messageLines = new List<string>();
            string message = string.Empty;

            messageLines.Add(string.Format(Properties.Settings.Default.MessageFormatLine1, contract.DateIssued.ToString("yyyy-MM-dd hh:mm:ss")));
            messageLines.Add(string.Format(Properties.Settings.Default.MessageFormatLine2, IssuerName));
            messageLines.Add(string.Format(Properties.Settings.Default.MessageFormatLine3, contract.Reward.ToString()));

            var startStation = contract.StartStation;
            var endStation = contract.EndStation;
            string startStationName = string.Empty;
            string endStationName = string.Empty;


            if(startStation == null)
            {
                startStationName = Api.GetStation(contract.StartStationID).stationName;
            }
            else 
            {
                startStationName = contract.StartStation.Name;
            }
            if(endStation == null)
            {
                endStationName = Api.GetStation(contract.EndStationID).stationName;
            }
            else
            {
                endStationName = contract.EndStation.Name;
            }

            try
            {
                messageLines.Add(string.Format(Properties.Settings.Default.MessageFormatLine4, startStationName, endStationName)); //, contract.StartStation.Name, contract.EndStation.Name));
            }
            catch (Exception ex) { }
            messageLines.Add(string.Format(Properties.Settings.Default.MessageFormatLine5, contract.Volume.ToString()));

            message = String.Join("\n", messageLines.ToArray());
            return message;
        }

        private static MessagePayload HyperFormatMessage(EveAI.Live.Utility.Contract contract, string IssuerName)
        {
            string type;
            List<string> messageLines = new List<string>();
            var message = new MessagePayload();

            //messageLines.Add(string.Format(Properties.Settings.Default.MessageFormatLine1, contract.DateIssued.ToString("yyyy-MM-dd hh:mm:ss")));
            messageLines.Add(string.Format(Properties.Settings.Default.MessageFormatLine2, IssuerName));
            messageLines.Add(string.Format(Properties.Settings.Default.MessageFormatLine3, contract.Reward.ToString()));

            var startStation = contract.StartStation;
            var endStation = contract.EndStation;
            string startStationName = string.Empty;
            string endStationName = string.Empty;

            if (startStation == null)
            {
                startStationName = Api.GetStation(contract.StartStationID).stationName;
            }
            else
            {
                startStationName = contract.StartStation.Name;
            }
            if (endStation == null)
            {
                endStationName = Api.GetStation(contract.EndStationID).stationName;
            }
            else
            {
                endStationName = contract.EndStation.Name;
            }

            try
            {
                messageLines.Add(string.Format(Properties.Settings.Default.MessageFormatLine4, startStationName, endStationName)); //, contract.StartStation.Name, contract.EndStation.Name));
            }
            catch (Exception ex) { }
            messageLines.Add(string.Format(Properties.Settings.Default.MessageFormatLine5, contract.Volume.ToString()));

            var colour = string.Empty;
            var title = string.Empty;

            if(contract.Status == EveAI.Live.Utility.Contract.ContractStatus.Outstanding)
            {
                colour = "#FF99C2";
                title = string.Format(Properties.Settings.Default.MessageFormatLine1, contract.DateIssued.ToString("yyyy-MM-dd HH:mm:ss"), "Received");
            }
            else
            {
                colour = "#A6D785";
                title = string.Format(Properties.Settings.Default.MessageFormatLine1, contract.DateCompleted.ToString("yyyy-MM-dd HH:mm:ss"), "Completed");
            }
            message.Attachments = new List<MessagePayloadAttachment>();
            
            message.Attachments.Add(new MessagePayloadAttachment()
            {
                Text = String.Join("\n", messageLines.ToArray()),
                Title = title,
                ThumbUrl = "http://www.r3mus.org/Images/logo.png",
                AuthorName = Properties.Settings.Default.BotName,
                Colour = colour
            });
            return message;
        }

        private static void SendMessage(string message)
        {
            if (Properties.Settings.Default.Plugin.ToUpper() == "HIPCHAT")
            {
                Hipchat.SendToRoom(message, Properties.Settings.Default.RoomName, Properties.Settings.Default.HipchatToken);
            }
            else if (Properties.Settings.Default.Plugin.ToUpper() == "SLACK")
            {
                Slack.SendToRoom(message, Properties.Settings.Default.RoomName, Properties.Settings.Default.SlackWebhook);
            }
        }

        private static void SendMessage(MessagePayload message)
        {
            Slack.SendToRoom(message, Properties.Settings.Default.RoomName, Properties.Settings.Default.SlackWebhook);
        }

        private static DateTime GetLastRunTime()
        {
            try
            {
                return Convert.ToDateTime(ConfigurationSettings.AppSettings["LastCheckedAt"]);
            }
            catch (Exception ex)
            {
                return new DateTime();
            }
        }

        private static DateTime GetLastCompCheckTime()
        {
            try
            {
                return Convert.ToDateTime(ConfigurationSettings.AppSettings["LastCompCheckedAt"]);
            }
            catch (Exception ex)
            {
                return new DateTime();
            }
        }
    }
}
