using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.AspNet.SignalR;

namespace r3mus.Hubs
{
    public class IntelHub : Hub
    {
        public void ReportIntel(LogLine message)
        {
            Clients.All.pingIntel(message.LogDateTime.ToString("yyyy-MM-dd HH:mm:ss"), message.UserName, message.Message);
        }
    }

    public class LogLine
    {
        public DateTime LogDateTime { get; set; }
        public string UserName { get; set; }
        public string Message { get; set; }

        public LogLine(string line)
        {
            var split = line.Split(new string[] { " ] " }, StringSplitOptions.RemoveEmptyEntries);
            LogDateTime = Convert.ToDateTime(split[0].Replace("[ ", ""));
            split = split[1].Split(new string[] { " > " }, StringSplitOptions.RemoveEmptyEntries);
            UserName = split[0];
        }
    }
}