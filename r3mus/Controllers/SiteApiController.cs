using JKON.EveApi.Corporation.Models;
using JKON.EveWho;
using r3mus.Models;
using r3mus.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using WebApi.OutputCache.V2;

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
            return db.DeclaredToons.Where(corpy => corpy.ToonName.Contains(partialName)).Select(corpy => corpy.ToonName);
            //return db.DeclaredToons.Where(corpy => corpy.ToonName.Contains(partialName)).GroupBy(corpy => corpy.ToonName).Select(c => c.First().ToonName);
        }

        [Route("api/GetUserProfileUrl/{name?}")]
        [HttpGet]
        public string GetUserProfileUrl(string name)
        {
            if (db.Users.Where(corpy => corpy.UserName == name).FirstOrDefault() != null)
            {
                return string.Format("/WebsiteAdmin/ViewProfile/{0}", (db.Users.Where(corpy => corpy.UserName == name).FirstOrDefault().Id));
            }
            else
            {
                return string.Format("/WebsiteAdmin/ViewProfile/{0}", db.DeclaredToons.Where(t => t.ToonName == name).First().User_Id);
            }
        }

        [Route("api/GetAllMembersInfo")]
        [CacheOutput(ClientTimeSpan = 900, ServerTimeSpan = 900)]
        [HttpGet]
        public IEnumerable<UserProfileViewModel> GetAllMembersInfo()
        {
            var users = db.Users.Where(user => user.MemberType == "Corporation").ToList<ApplicationUser>();
            var members = db.CorpMembers.ToList<Member>();
            var userModels = new List<UserProfileViewModel>();
            
            members.ForEach(member =>
            {
                member.Title = member.Title.Replace(member.ID.ToString(), "").Trim().Trim(',');
                var user = users.Where(usr => usr.UserName == member.Name).FirstOrDefault();
                if (user == null)
                {
                    var apis = db.ApiInfoes.ToList();
                    apis.ForEach(api =>
                    {
                        try
                        {
                            var chars = api.GetDetails();
                            if (chars.Any(toon => toon.CharacterName == member.Name))
                            {
                                user = api.User;
                                return;
                            }
                        }
                        catch (Exception ex) { }
                    });
                    if (user == null)
                    {
                        user = new ApplicationUser()
                        {
                            Id = 0.ToString(),
                            UserName = "Not Registered",
                            EmailAddress = "Not Registered",
                            MemberType = "Not Registered",
                            MemberSince = member.MemberSince,
                            Titles = new List<Title>()
                        };
                        user.Titles.Add(new Title() { TitleName = "NOT REGISTERED" });
                    }
                }

                var knownAlts = new List<string>();
                try
                {
                    var apis = db.ApiInfoes.Where(api => api.User.Id == user.Id).ToList();

                    apis.ForEach(api =>
                    {
                        var alts = api.GetDetails();
                        alts.ForEach(alt => knownAlts.Add(alt.CharacterName));
                    });
                }
                catch (Exception ex) { }

                userModels.Add(new UserProfileViewModel()
                {
                    MemberName = member.Name,
                    MemberSince = Convert.ToDateTime(member.MemberSince),
                    Titles = member.Title,
                    CurrentLocation = member.Location,
                    LastLogon = Convert.ToDateTime(member.LastLogonDateTime),
                    ShipType = member.ShipType,
                    KnownAlts = string.Join(", ", knownAlts)
                });
            });

            return userModels;
        }
    }
}