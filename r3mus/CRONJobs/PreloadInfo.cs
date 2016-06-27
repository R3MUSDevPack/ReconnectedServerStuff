using Quartz;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace r3mus.CRONJobs
{
    public class PreloadInfo : IJob
    {
        private const string Method = "POST";
        private const string ContentType = "application/json";
        private const string UserAgent = "Mozilla/4.0 (compatible; MSIE 6.0; Windows NT 5.1; SV1; .NET CLR 1.1.4322; .NET CLR 2.0.50727)";
        private const string Referer = "http://www.r3mus.org";

        public void Execute(IJobExecutionContext context)
        {
            Get("/api/GetAllMembersInfo");
        }

        private void Get(string method)
        {
            var request = (HttpWebRequest)WebRequest.Create(new Uri(string.Format("{0}{1}", Referer, method)));
            request.Method = Method;
            request.ContentType = ContentType;
            request.UserAgent = UserAgent;
            request.Referer = Referer;

            try
            {
                request.GetResponse();
            }
            catch (Exception ex)
            {
            }
        }
    }
}
