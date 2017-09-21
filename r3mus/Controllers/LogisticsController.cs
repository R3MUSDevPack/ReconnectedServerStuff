using EveAI.Live;
using EveAI.Live.Account;
using EveAI.Live.Utility;
using EveAI.SpaceStation;
using eZet.EveLib.EveXmlModule;
using eZet.EveLib.EveXmlModule.Models.Character;
using JKON.EveWho.Models;
using JKON.EveWho.Stations;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
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
    //[Authorize]
    public class LogisticsController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();
        protected UserManager<ApplicationUser> UserManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(new ApplicationDbContext()));
        
        [OverrideAuthorization]
        public ActionResult ContractStatus()
        {
            List<EveAI.Live.Utility.Contract> Contracts = new List<EveAI.Live.Utility.Contract>();
            Dictionary<long, EveCharacter> Names = new Dictionary<long, EveCharacter>();
            List<long> IDs = new List<long>();

            DateTime backDate = DateTime.Now.AddDays(-7).Date;

            try
            {
                var api = EveXml.CreateCorporationKey((long)Properties.Settings.Default.LogisticsCorpAPI, Properties.Settings.Default.LogisticsVCode);
                var contractResults = api.Corporation.GetContracts().Result.Contracts.Where(contract =>
                   (contract.Type == "Courier")
                   &&
                   (contract.Status != ContractList.ContractStatus.Deleted)
                   &&
                   ((contract.DateIssued >= backDate)
                   ||
                   (contract.Status == ContractList.ContractStatus.Outstanding)
                   ||
                   (contract.Status == ContractList.ContractStatus.InProgress))).ToList();

                contractResults.ForEach(c =>
                Contracts.Add(new EveAI.Live.Utility.Contract()
                {
                    AcceptorID = c.AcceptorId, AssigneeID = c.AssigneeId,
                    Availability = (Contract.ContractAvailability)c.Availability,
                    Collateral = Convert.ToDouble(c.Collateral), ContractID = c.ContractId,
                    DateAccepted = Convert.ToDateTime(c.DateAccepted),
                    DateCompleted = Convert.ToDateTime(c.DateCompleted),
                    DateIssued = Convert.ToDateTime(c.DateIssued),
                    DateExpired = Convert.ToDateTime(c.DateExpired),
                    EndStationID = (int)c.EndStationId, StartStationID = (int)c.StartStationId,
                    IssuerID = c.IssuerId,
                    NumDays = c.NumDays,
                    Price = Convert.ToDouble(c.Price), Reward = Convert.ToDouble(c.Reward),
                    Status = (Contract.ContractStatus)Enum.Parse(typeof(Contract.ContractStatus), c.Status.ToString()),
                    Title = c.Title, Volume = c.Volume
                }));
                
                if(User.Identity.IsAuthenticated && !User.IsInRole("Logistics"))
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
                        contract.StartStation = new EveAI.SpaceStation.Station() {
                            Name = startStation != null ? startStation.stationName : string.Concat("Structure ID ", contract.StartStationID.ToString())
                        };
                    }
                    if (contract.EndStation == null)
                    {
                        var endStation = Api.GetStation(contract.EndStationID);
                        contract.EndStation = new EveAI.SpaceStation.Station() {
                            Name = endStation != null ? endStation.stationName : string.Concat("Structure ID ", contract.EndStationID.ToString())
                        };
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
                    var toon2 = JKON.EveWho.Api.GetCorpMembers(Convert.ToInt64(Properties.Settings.Default.CorpAPI), Properties.Settings.Default.VCode).Where(member => member.Name == User.Identity.Name).FirstOrDefault();
                    var station = new Station()
                    { 
                        Name = toon2.Location
                    };
                    
                    Names.Add(id, toon1);
                    Names.Add(-1, new EveCharacter() { result = new JKON.EveWho.EveCharacter.Models.result() { characterID = -1, characterName = "Hauled By Reindeer (corp)" } });
                    Names.Add(-2, new EveCharacter() { result = new JKON.EveWho.EveCharacter.Models.result() { characterID = -2, characterName = "Santa Claus" } });

                    var contract = new EveAI.Live.Utility.Contract()
                    {
                        StartStation = new EveAI.SpaceStation.Station()
                        {
                            Name = "Lapland VI - Santas Workshop",
                            SolarSystem = new EveAI.Map.SolarSystem() { Name = "North Pole" }
                        },
                        EndStation = station,
                        DateIssued = new DateTime(2015, 12, 15),
                        DateAccepted = new DateTime(2015, 12, 17),
                        AcceptorID = -1,
                        IssuerID = -2,
                        Status = GetStatus(),
                        Volume = 10000,
                        Title = string.Format("Christmas Presents for {0}", toon2.Name)
                    };

                    if (contract.Status == EveAI.Live.Utility.Contract.ContractStatus.Outstanding)
                    {
                        Contracts.Insert(0, contract);
                    }
                    else if(contract.Status == EveAI.Live.Utility.Contract.ContractStatus.InProgress)
                    {
                        var index = Contracts.Where(c => c.Status == EveAI.Live.Utility.Contract.ContractStatus.Outstanding).Count();
                        Contracts.Insert(index, contract);
                    }
                }
            }
            catch(Exception ex)
            {
            }

            var result = new LogisticsContractsViewModel()
            {
                CharacterInfos = Names,
                TotalVolumeOutstanding = Contracts.Where(w => w.Status == Contract.ContractStatus.Outstanding).Sum(s => s.Volume),
                DeliveryPoints = new List<DestinationDeliveryModel>()
            }; ;
            var deliveryPoints = Contracts.GroupBy(g => g.EndStationID).Select(s => s.First()).Select(s => s.EndStation);
            deliveryPoints.ForEach(f =>
            {
                result.DeliveryPoints.Add(new DestinationDeliveryModel()
                {
                    Id = Contracts.Where(w => w.EndStation.Name == f.Name).First().EndStationID,
                    Destination = f.Name,
                    TotalVolume = Contracts.Where(w => w.EndStation.Name == f.Name && w.Status == Contract.ContractStatus.Outstanding).Sum(s => s.Volume),
                    DisplayContracts = User.Identity.IsAuthenticated ?
                        Contracts.Where(w => w.EndStation.Name == f.Name).ToList() : new List<Contract>()
                });
            });

            return View(result);
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