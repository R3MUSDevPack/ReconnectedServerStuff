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
            Console.WriteLine(string.Concat(message.LogDateTime.ToString("yyyy-MM-dd HH:mm:ss"), " > ", message.UserName, " > ", message.Message));
            Clients.All.pingIntel(message);
        }
    }

    public class LogLine
    {
        public DateTime LogDateTime { get; set; }
        public string UserName { get; set; }
        public string Message { get; set; }

        public LogLine()
        {
        }
    }
}