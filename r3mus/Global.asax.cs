using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using r3mus.Models;
using System.Web.Http;
using Quartz;
using Quartz.Impl;
using r3mus.CRONJobs;

namespace r3mus
{
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            Database.SetInitializer<ApplicationDbContext>(null);

            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);

            GlobalConfiguration.Configure(WebApiConfig.Register);

            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);

            if (!Properties.Settings.Default.Debug)
            {
                StartCronJobs();
            }
        }

        private void StartCronJobs()
        {
            IScheduler sched;
            IJobDetail jobDetail;
            ITrigger trigger;


            var cronJobs = new r3mus_DBEntities().CRONJobs.Where(cronJob => cronJob.Enabled == true);

            cronJobs.ToList().ForEach(cronJob =>
            {
                sched = new StdSchedulerFactory().GetScheduler();
                sched.Start();
                
                jobDetail = JobBuilder.Create(Type.GetType(string.Concat("r3mus.CRONJobs.", cronJob.JobName)))
                    .WithIdentity(string.Format("{0}Instance", cronJob.JobName), string.Format("{0}Group", cronJob.JobName))
                    .Build();
                trigger = TriggerBuilder.Create()
                    .WithIdentity(string.Format("{0}Trigger", cronJob.JobName), string.Format("{0}TriggerGroup", cronJob.JobName))
                    .StartNow()
                    .WithSimpleSchedule(x => x.WithIntervalInMinutes(cronJob.Schedule).RepeatForever())
                    .Build();
                sched.ScheduleJob(jobDetail, trigger);
            });

            sched = new StdSchedulerFactory().GetScheduler();
            sched.Start();

            //jobDetail = JobBuilder.Create(Type.GetType("r3mus.CRONJobs.PreloadInfo"))
            //    .WithIdentity("PreloadInfoInstance", "PreloadInfoGroup")
            //    .Build();
            //trigger = TriggerBuilder.Create()
            //    .WithIdentity("PreloadInfoTrigger", "PreloadInfoTriggerGroup")
            //    .StartNow()
            //    .WithSimpleSchedule(x => x.WithIntervalInMinutes(15).RepeatForever())
            //    .Build();
            //sched.ScheduleJob(jobDetail, trigger);
        }
    }
}
