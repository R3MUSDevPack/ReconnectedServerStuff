using JKON.EveWho;
using Quartz;
using r3mus.Models;
using System;
using System.Linq;
using System.Data.Entity.Migrations;
using JKON.EveApi.Corporation.Models;

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
                    var membersToDelete = db.CorpMembers.Where(existingMember => !members.Any(member => member.ID == existingMember.ID)).ToList();
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
