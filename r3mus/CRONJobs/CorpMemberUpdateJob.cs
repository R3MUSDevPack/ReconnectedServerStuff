using JKON.EveWho;
using Quartz;
using r3mus.Models;
using System;
using System.Linq;
using System.Data.Entity.Migrations;
using JKON.EveApi.Corporation.Models;
using System.Collections.Generic;
using System.Reflection;

namespace r3mus.CRONJobs
{
    public class CorpMemberUpdateJob : IJob
    {
        private readonly string ExpiredMessage = "Key has expired. Contact key owner for access renewal.";

        public void Execute(IJobExecutionContext context)
        {
            var name = MethodBase.GetCurrentMethod().DeclaringType.Name;
            var db = new ApplicationDbContext();
            SyncDeclaredToons();
            SyncCorpMembers(db.CRONJobs.Where(job => job.JobName == name).FirstOrDefault());
            db.SaveChanges();
        }

        private void SyncCorpMembers(CRONJob settings)
        {
            List<Member> members;
            if (settings.Enabled)
            {
                try
                {
                    members = Api.GetCorpMembers(Convert.ToInt64(Properties.Settings.Default.CorpAPI), Properties.Settings.Default.VCode);
                    if ((members != null) && (members.Count > 0))
                    {
                        using (var db = new ApplicationDbContext())
                        {
                            members.ForEach(member =>
                                db.CorpMembers.AddOrUpdate(member)
                            );
                            db.SaveChanges();

                            var membersToDelete = new List<Member>();
                            db.CorpMembers.ToList().ForEach(member =>
                            {
                                if (!members.Any(mem => mem.ID == member.ID))
                                {
                                    membersToDelete.Add(member);
                                }
                            });

                            membersToDelete.ForEach(member =>
                                db.CorpMembers.Remove(member)
                            );
                            settings.LastRun = DateTime.UtcNow;
                            db.SaveChanges();

                            var users = db.Users.ToList();
                            users.ForEach(usr =>
                            {
                                var member = members.Where(memb => memb.Name.ToLower() == usr.UserName.ToLower()).FirstOrDefault();
                                if (member != null)
                                {
                                    if (member.Name != usr.UserName)
                                    {
                                        usr.UserName = member.Name;
                                    }
                                    usr.GetDetails(true);
                                }
                            });
                            db.SaveChanges();
                        }
                    }
                }
                catch (Exception ex)
                {
                }
            }
        }

        private void SyncDeclaredToons()
        {
            using (var db = new ApplicationDbContext())
            {
                var newToons = new List<DeclaredToon>();
                var deleteApis = new List<ApiInfo>();

                db.ApiInfoes.ToList().ForEach(api =>
                {
                    try
                    {
                        var chars = api.GetDetails().Select(s => s.CharacterName).ToList();
                        
                        chars.ForEach(cName => newToons.Add(new DeclaredToon() { User_Id = api.User.Id, ToonName = cName }));
                    }
                    catch (Exception ex)
                    {
                        if((ex.InnerException != null) && (ex.InnerException.Message == ExpiredMessage))
                        {
                            deleteApis.Add(api);
                        }
                    }
                });
                db.DeclaredToons.RemoveRange(db.DeclaredToons);
                db.ApiInfoes.RemoveRange(deleteApis);
                db.SaveChanges();
                var toons = newToons.GroupBy(g => new { g.User_Id, g.ToonName }).Select(s => s.First()).ToList();
                toons.ForEach(t => db.DeclaredToons.Add(t));
                db.SaveChanges();
            }
        }
    }
}
