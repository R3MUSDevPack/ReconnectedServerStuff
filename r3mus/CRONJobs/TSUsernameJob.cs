using Quartz;
using r3mus.Models;
using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Teamspeak_Plugin;

namespace r3mus.CRONJobs
{
    public class TSUsernameJob : IJob
    {
        ApplicationDbContext db;

        public void Execute(IJobExecutionContext context)
        {
            var name = MethodBase.GetCurrentMethod().DeclaringType.Name;
            db = new ApplicationDbContext();
            try
            {
                CheckNames(db.CRONJobs.Where(job => job.JobName == name).FirstOrDefault());
            }
            catch (Exception ex)
            {
                //RecruitmentController.SendMessage(ex.Message);
            }
            db.SaveChanges();
        }

        public void CheckNames(CRONJob settings)
        {
            if(settings.Enabled)
            {
                var validUsers = db.Users.ToList();
                validUsers = validUsers.Where(w => w.IsValid()).ToList();
                var tsPlug = new Teamspeak();
                var t = Task.Run(() => tsPlug.CheckUserNames(
                    validUsers.Select(s => string.Format("[{0}] {1}", s.CorpTicker, s.UserName)).ToList(), 
                    Properties.Settings.Default.TSURL, Properties.Settings.Default.TS_Password));
                t.Wait();
                settings.LastRun = DateTime.UtcNow;
            }
        }
    }
}