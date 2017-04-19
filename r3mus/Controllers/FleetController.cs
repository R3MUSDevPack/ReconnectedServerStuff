using MoreLinq;
using r3mus.Models;
using r3mus.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace r3mus.Controllers
{
    public class FleetController : Controller
    {
        r3mus_DBEntities dbEnt;

        public FleetController()
        {
            dbEnt = new r3mus_DBEntities();
        }
        // GET: Fleet
        public ActionResult Index()
        {
            var fleets = dbEnt.Fleets.Where(w => w.Void == false).ToList();
            var commanders = fleets.Select(s => s.Commander).Distinct();

            var summary = new List<FleetCountViewModel>();
            var month = DateTime.Now.Month;
            var lastMonth = DateTime.Now.AddMonths(-1).Month;
            commanders.ForEach(f => 
                summary.Add(new FleetCountViewModel() {
                    Commander = f,
                    MonthCount = fleets.Where(w => w.Commander == f && w.Time.Month == month && w.Void == false).Count(),
                    LastMonthCount = fleets.Where(w => w.Commander == f && w.Time.Month == lastMonth && w.Void == false).Count()
                })
            );

            var model = new FleetSummaryViewModel()
            {
                Fleets = fleets,
                Summary = summary
            };

            return View(model);
        }

        public ActionResult Create()
        {
            var model = new FleetCreationViewModel();
            model.Fleet.Commander = User.Identity.Name;
            model.Fleet.Time = DateTime.UtcNow;
            return View(model);
        }

        public ActionResult Details(int id)
        {
            var fleet = dbEnt.Fleets.Where(w => w.Id == id).First();
            dbEnt.Entry(fleet).Collection(c => c.FleetCompositions).Load();
            return View(fleet);
        }
        
        [HttpPost]
        public ActionResult Create(FormCollection form)
        {
            var Fleet = new Fleet() {
                Commander = form["Fleet.Commander"].ToString(),
                Time = Convert.ToDateTime(form["Fleet.Time"]),
                Name = form["Fleet.Name"].ToString()
            };
            var mem = form["Members"].Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
            Fleet.FleetCompositions = new List<FleetComposition>();
            mem.ToList().ForEach(f => Fleet.FleetCompositions.Add(new FleetComposition() { Fleet =  Fleet, MemberName = f }));

            dbEnt.Fleets.Add(Fleet);
            dbEnt.FleetCompositions.AddRange(Fleet.FleetCompositions);
            dbEnt.SaveChanges();

            return RedirectToAction("Index");
        }
    }
}