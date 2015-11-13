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
                
            }
            catch(Exception ex)
            {

            }

            return View(new LogisticsContractsViewModel() { DisplayContracts = Contracts, CharacterInfos = Names });
        }
	}
}