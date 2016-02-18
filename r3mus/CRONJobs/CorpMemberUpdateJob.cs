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
        public void Execute(IJobExecutionContext context)
        {
            var name = MethodBase.GetCurrentMethod().DeclaringType.Name;
            var db = new r3mus_DBEntities();
            SyncCorpMembers(db.CRONJobs.Where(job => job.JobName == name).FirstOrDefault());
        }

        private void SyncCorpMembers(CRONJob settings)
        {
            if (settings.Enabled)
            {
                try
                {
                    var members = Api.GetCorpMembers(Convert.ToInt64(Properties.Settings.Default.CorpAPI), Properties.Settings.Default.VCode);
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
                    }
                }
                catch (Exception ex)
                {
                }
            }
        }
    }
}
