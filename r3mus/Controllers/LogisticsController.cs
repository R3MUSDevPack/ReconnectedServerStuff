using EveAI.Live;
using EveAI.Live.Account;
using JKON.EveWho.Models;
using JKON.EveWho.Stations;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using r3mus.Models;
using r3mus.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace r3mus.Controllers
{
    [Authorize]
    public class LogisticsController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();
        protected UserManager<ApplicationUser> UserManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(new ApplicationDbContext()));

        public ActionResult ContractStatus()
        {
            EveApi api;
            List<EveAI.Live.Utility.Contract> Contracts = new List<EveAI.Live.Utility.Contract>();
            Dictionary<long, EveCharacter> Names = new Dictionary<long, EveCharacter>();
            List<long> IDs = new List<long>();

            DateTime backDate = DateTime.Now.AddDays(-7).Date;

            try
            {
                api = new EveAI.Live.EveApi("Clyde en Marland's Contract Notifier", (long)Properties.Settings.Default.LogisticsCorpAPI, Properties.Settings.Default.LogisticsVCode);

                Contracts = api.GetCorporationContracts().ToList().Where(contract =>
                    (contract.Type == EveAI.Live.Utility.Contract.ContractType.Courier)
                    &&
                    (contract.Status != EveAI.Live.Utility.Contract.ContractStatus.Deleted)
                    &&
                    ((contract.DateIssued >= backDate)
                    ||
                    (contract.Status == EveAI.Live.Utility.Contract.ContractStatus.Outstanding)
                    ||
                    (contract.Status == EveAI.Live.Utility.Contract.ContractStatus.InProgress))).ToList();

                if(!User.IsInRole("Logistics"))
                {
                    try
                    {
                        var user = UserManager.FindById(User.Identity.GetUserId());
                        user.LoadApiKeys();

                        var apiKeys = user.ApiKeys.GroupBy(apiKey => apiKey.ApiKey).Select(apiKey => apiKey.First()).ToList();
                        var toons = new List<AccountCharacter>();

                        apiKeys.ForEach(apiKey => toons.AddRange(JKON.EveWho.EveWho.GetCharacters(apiKey.ApiKey, apiKey.VerificationCode)));

                        Contracts = Contracts.Where(contract => toons.Any(toon => toon.CharacterID.Equals(contract.IssuerID))).ToList();                                                
                    }
                    catch(Exception ex)
                    {
                        ViewBag.Message(ex.Message);
                        return RedirectToAction("Index", "LoggedInHome");
                    }
                }

                IDs = Contracts.Select(contract => contract.IssuerID).ToList();

                Contracts.ForEach(contract =>
                {
                    if(contract.StartStation == null) {
                        var startStation = Api.GetStation(contract.StartStationID);
                        contract.StartStation = new EveAI.SpaceStation.Station() { Name = startStation.stationName };
                    }
                    if (contract.EndStation == null)
                    {
                        var endStation = Api.GetStation(contract.EndStationID);
                        contract.EndStation = new EveAI.SpaceStation.Station() { Name = endStation.stationName };
                    }
                });

                Contracts.Select(contract => contract.IssuerID).ToList().ForEach(id =>
                        {
                            if (!Names.ContainsKey(id))
                            {
                                Names.Add(id, JKON.EveWho.Api.GetCharacter(id));
                            }
                        });
                Contracts.Select(contract => contract.AcceptorID).ToList().ForEach(id =>
                {
                    if (!Names.ContainsKey(id))
                    {
                        Names.Add(id, JKON.EveWho.Api.GetCharacter(id));
                    }
                });
                
                if((DateTime.Now.Month == 12) && (DateTime.Now.Day > 17) && (DateTime.Now.Day < 25))
                {
                    
                    var id = JKON.EveWho.Api.GetCharacterID(User.Identity.Name);
                    var toon1 = JKON.EveWho.Api.GetCharacter(id);
                    var toon2 = JKON.EveWho.Api.GetCorpMembers(Convert.ToInt64(Properties.Settings.Default.CorpAPI), Properties.Settings.Default.VCode).Where(member => member.ID == id).FirstOrDefault();
                    var station = new EveAI.DataCore().Stations.Where(station1 => station1.Name == toon2.Location).FirstOrDefault();

                    Names.Add(id, toon1);
                    Names.Add(-1, new EveCharacter() { result = new JKON.EveWho.EveCharacter.Models.result() { characterID = -1, characterName = "Hauled By Reindeer (corp)" } });

                    Contracts.Add(new EveAI.Live.Utility.Contract()
                    {
                        StartStation = new EveAI.SpaceStation.Station() {
                            Name = "Lapland VI - Santas Workshop",
                            SolarSystem = new EveAI.Map.SolarSystem() { Name = "North Pole" }
                        },
                        EndStation = station,
                        DateIssued = new DateTime(2015, 12, 15),
                        DateAccepted = new DateTime(2015, 12, 17),
                        AcceptorID = -1,
                        Status = GetStatus(),
                        Title = "Christmas Presents"
                    });
                }
            }
            catch(Exception ex)
            {

            }

            return View(new LogisticsContractsViewModel() { DisplayContracts = Contracts, CharacterInfos = Names });
        }

        private EveAI.Live.Utility.Contract.ContractStatus GetStatus()
        {
            if(DateTime.Now.Day < 24)
            {
                return EveAI.Live.Utility.Contract.ContractStatus.Outstanding;
            }
            else
            {
                return EveAI.Live.Utility.Contract.ContractStatus.InProgress;
            }
        }
	}
}