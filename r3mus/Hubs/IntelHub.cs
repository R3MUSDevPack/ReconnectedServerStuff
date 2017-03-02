using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.AspNet.SignalR;
using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace r3mus.Hubs
{
    public class IntelHub : Hub
    {
        Models.r3mus_DBEntities db = new Models.r3mus_DBEntities();

        static List<string> users = new List<string>();

        public override Task OnConnected()
        {
            string clientId = GetClientId();

            if (users.IndexOf(clientId) == -1)
            {
                users.Add(clientId);
            }
            SendUserCount();
            return base.OnConnected();
        }

        public override Task OnReconnected()
        {
            string clientId = GetClientId();
            if (users.IndexOf(clientId) == -1)
            {
                users.Add(clientId);
            }
            SendUserCount();
            return base.OnReconnected();
        }

        public override Task OnDisconnected(bool stopCalled)
        {
            string clientId = GetClientId();

            if (users.IndexOf(clientId) > -1)
            {
                users.Remove(clientId);
            }
            SendUserCount();
            return base.OnDisconnected(stopCalled);
        }
        private string GetClientId()
        {
            string clientId = "";
            if (!(Context.QueryString["clientId"] == null))
            {
                //clientId passed from application 
                clientId = Context.QueryString["clientId"].ToString();
            }

            if (clientId.Trim() == "")
            {
                //default clientId: connectionId 
                clientId = Context.ConnectionId;
            }            return clientId;

        }

        public void ReportIntel(LogLine message)
        {
            if (!db.LogMessages.Any(msg => (msg.Message == message.Message) && (msg.UserName == message.UserName) && ((DateTime.UtcNow - message.LogDateTime).Minutes < 2)))
            {
                db.LogMessages.Add(
                    new Models.LogMessage() { LogDateTime = message.LogDateTime, Message = message.Message, UserName = message.UserName }
                    );
                db.SaveChangesAsync();
                Console.WriteLine(string.Concat(message.LogDateTime.ToString("yyyy-MM-dd HH:mm:ss"), " > ", message.UserName, " > ", message.Message));
                Clients.All.pingIntel(message);
            }
        }        

        public void ImLogging(string loggerName)
        {
            var OnlineUsers = db.OnlineUsers;
            var MessageHistory = db.LogMessages;

            if (!OnlineUsers.Any(user => user.LoggerName == loggerName))
            {
                OnlineUsers.Add(new Models.OnlineUser() { LoggerName = loggerName, LastKnownDateTime = DateTime.Now });
            }
            else
            {
                OnlineUsers.Where(user => user.LoggerName == loggerName).FirstOrDefault().LastKnownDateTime = DateTime.Now;
            }

            Cleanup();

            db.SaveChanges();

            SendUserCount();
        }

        private void Cleanup()
        {
            var OnlineUsers = db.OnlineUsers;
            var MessageHistory = db.LogMessages;

            var past = DateTime.Now.AddMinutes(-5);

            OnlineUsers.Where(user =>
                user.LastKnownDateTime < past
                ).ToList().ForEach(user =>
                    OnlineUsers.Remove(user)
                );

            MessageHistory.Where(msg =>
                msg.LogDateTime < past
                ).ToList().ForEach(msg =>
                    MessageHistory.Remove(msg)
                );
            db.SaveChanges();
        }

        public void SendUserCount()
        {
            var baseTime = DateTime.Now.AddMinutes(-5);
            Clients.All.pingUserCount(db.OnlineUsers.Where(user => user.LastKnownDateTime > baseTime).Count());
            Clients.All.pingConnectionCount(users.Count);
        }
        public void SendHistory()
        {
            Cleanup();
            db.LogMessages.ToList().ForEach(message =>
                Clients.Caller.pingIntel(message)
            );
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