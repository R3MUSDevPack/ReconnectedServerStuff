using r3mus.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace r3mus.Controllers
{
    public class SiteApiController : ApiController
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        [Route("api/SearchApplicants/{partialName?}")]
        [HttpGet]
        public IEnumerable<string> GetApplicants(string partialName)
        {
            return db.Applicants.Where(app => app.Name.Contains(partialName)).Select(app => app.Name);
        }

        [Route("api/GetApplication/{name?}")]
        [HttpGet]
        public string GetApplication(string name)
        {
            return string.Format("/Recruitment/ReviewApplication/{0}", (db.Applicants.Where(app => app.Name == name).FirstOrDefault().Id.ToString()));
        }

        [Route("api/SearchMembers/{partialName?}")]
        [HttpGet]
        public IEnumerable<string> GetMembers(string partialName)
        {
            return db.Users.Where(corpy => corpy.UserName.Contains(partialName)).Select(corpy => corpy.UserName);
        }

        [Route("api/GetUserProfileUrl/{name?}")]
        [HttpGet]
        public string GetUserProfileUrl(string name)
        {
            return string.Format("/WebsiteAdmin/ViewProfile/{0}", (db.Users.Where(corpy => corpy.UserName == name).FirstOrDefault().Id));
        }
    }
}