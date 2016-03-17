using r3mus.ViewModels;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Hosting;
using System.Web.Mvc;
using System.Web.UI.HtmlControls;

namespace r3mus.Controllers
{
    [Authorize]
    public class IntelController : Controller
    {
        public enum Map
        {
            Aridia,
            Black_Rise,
            Branch,
            Cache,
            Catch,
            Cloud_Ring,
            Cobalt_Edge,
            Curse,
            Deklein,
            Delve,
            Derelik,
            Detorid,
            Devoid,
            Domain,
            Esoteria,
            Essence,
            Etherium_Reach,
            Everyshore,
            Fade,
            Feythabolis,
            Fountain,
            Geminate,
            Genesis,
            Great_Wildlands,
            Heimatar,
            Immensea,
            Impass,
            Insmother,
            Kador,
            Khanid,
            Lonetrek,
            Malpais,
            Metropolis,
            Molden_Heath,
            Oasa,
            Omist,
            Outer_Passage,
            Outer_Ring,
            Paragon_Soul,
            Period_Basis,
            Perrigen_Falls,
            Placid,
            Providence,
            Pure_Blind,
            Querious,
            Scalding_Pass,
            Sinq_Laison,
            Solitude,
            Stain,
            Syndicate,
            Tenal,
            Tenerifis,
            The_Bleak_Lands,
            The_Citadel,
            The_Forge,
            The_Kalevala_Expanse,
            The_Spire,
            Tribute,
            Vale_of_the_Silent,
            Venal,
            Verge_Vendor,
            Wicked_Creek
        }

        //
        // GET: /Intel/
        public ActionResult GetIntelMap(Map map)
        {
            var vm = new IntelViewModel() { Title = map.ToString(), Map = string.Concat("/maps/", map.ToString(), ".svg") };
            //var vm = new IntelViewModel() { Title = map.ToString(), Map = string.Concat("/maps/Immensea_noJB.svg") };

            return View(vm);
        }

        public ActionResult MapSelection()
        {
            var q = new Queue<Map>((IEnumerable<Map>)Enum.GetValues(typeof(r3mus.Controllers.IntelController.Map)));

            var col1 = new ArrayList();
            var col2 = new ArrayList();
            var col3 = new ArrayList();
            var col4 = new ArrayList();
            var col5 = new ArrayList();

            while (q.Count > 0)
            {
                var map = q.Dequeue();
                if (col4.Count > col5.Count)
                {
                    col5.Add(map);
                }
                else if (col3.Count > col4.Count)
                {
                    col4.Add(map);
                }
                else if (col2.Count > col3.Count)
                {
                    col3.Add(map);
                }
                else if (col1.Count > col2.Count)
                {
                    col2.Add(map);
                }
                else
                {
                    col1.Add(map);
                }
            }

            return PartialView("_MapSelection", new MapSelectionViewModel() { Column1 = col1, Column2 = col2, Column3 = col3, Column4 = col4, Column5 = col5 });
        }
	}
}