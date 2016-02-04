using JKON.EveWho;
using Quartz;
using r3mus.Models;
using System;
using System.Linq;
using System.Data.Entity.Migrations;
using JKON.EveApi.Corporation.Models;
using System.Collections.Generic;

namespace r3mus.CRONJobs
{
    public class CorpMemberUpdateJob : IJob
    {
        public void Execute(IJobExecutionContext context)   
        {
            SyncCorpMembers();
        }

        private void SyncCorpMembers()
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
                        if(!members.Any(mem => mem.ID == member.ID))
                        {
                            membersToDelete.Add(member);
                        }
                    });

                    membersToDelete.ForEach(member =>
                        db.CorpMembers.Remove(member)
                    );
                    db.SaveChanges();
                }
            }
            catch (Exception ex)
            {
            }
        }
    }
}
