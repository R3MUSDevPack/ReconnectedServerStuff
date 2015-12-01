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

            StartCronJobs();
        }

        private void StartCronJobs()
        {
            IScheduler sched;
            IJobDetail job;
            ITrigger trigger;

            sched = new StdSchedulerFactory().GetScheduler();
            sched.Start();
            job = JobBuilder.Create<CorpMemberUpdateJob>()
                .WithIdentity("MemberUpdateInstance", "MemberUpdateGroup")
                .Build();
            trigger = TriggerBuilder.Create()
                .WithIdentity("MemberUpdateTrigger", "MemberUpdateTriggerGroup")
                .StartNow()
                .WithSimpleSchedule(x => x.WithIntervalInSeconds(900).RepeatForever())
                .Build();
            sched.ScheduleJob(job, trigger);
        }
    }
}
