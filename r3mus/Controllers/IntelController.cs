using r3mus.ViewModels;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Hosting;
using System.Web.Mvc;

namespace r3mus.Controllers
{
    [Authorize]
    public class IntelController : Controller
    {
        public enum Map
        {
            Immensea,
            Tenerifis
        }

        //
        // GET: /Intel/
        public ActionResult GetIntelMap(Map map)
        {

            var vm = new IntelViewModel() { Title = map.ToString(), Map = string.Concat("/maps/", map.ToString(), ".svg") };
            
            return View(vm);
        }
	}
}