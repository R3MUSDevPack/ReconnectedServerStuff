using JKON.EveWho.Corporation;
using JKON.EveWho.Types;
using JKON.EveWho.Universe;
using JKON.EveWho.Wars;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace JKON.EveWho
{
    public static class ESI
    {
        private static string _baseURI = "https://esi.tech.ccp.is/latest";
        private static string _baseURITail = "?datasource=tranquility";
        private static string _baseTailExtra = "&language=en-us";

        private static string _universe = "universe";
        private static string _regions = "regions";
        private static string _constellations = "constellations";
        private static string _systems = "systems";
        private static string _types = "types";
        private static string _wars = "wars";
        private static string _corp = "corporations";
        private static string _alliance = "alliances";

        private static string BaseRequest(string uri)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(uri);
            request.UserAgent = "R3MUS.Devpack.EveWho-Clyde-en-Marland";
            WebResponse response = request.GetResponse();
            Stream stream = response.GetResponseStream();
            StreamReader reader = new StreamReader(stream);

            return reader.ReadToEnd();
        }

        private static object Deserialize (this string me, Type t)
        {
            return JsonConvert.DeserializeObject(me, t);
        }

        public static void GetRegion (this Region me)
        {
            var reqUri = string.Format("{0}/{1}/{2}/{3}", _baseURI, _universe, _regions, me.Id.ToString(), _baseURITail);

            var obj = (Region)BaseRequest(reqUri).Deserialize(typeof(Region));
            me.Constellation_Ids = obj.Constellation_Ids;
            me.Name = obj.Name;
        }

        public static void GetConstellation(this Constellation me)
        {
            var reqUri = string.Format("{0}/{1}/{2}/{3}", _baseURI, _universe, _constellations, me.Id.ToString(), _baseURITail);

            var obj = (Constellation)BaseRequest(reqUri).Deserialize(typeof(Constellation));
            me.Name = obj.Name;
            me.Position = obj.Position;
            me.Region_Id = obj.Region_Id;
            me.Systems = obj.Systems;
        }

        public static void GetSolarSystem(this SolarSystem me)
        {
            var reqUri = string.Format("{0}/{1}/{2}/{3}", _baseURI, _universe, _systems, me.Id.ToString(), _baseURITail);
            
            var obj = (SolarSystem)BaseRequest(reqUri).Deserialize(typeof(SolarSystem));
            me.Name = obj.Name;
            me.Planets = obj.Planets;
            me.Constellation_Id = obj.Constellation_Id;
            me.Position = obj.Position;
            me.SecurityStatus = obj.SecurityStatus;
            me.Stargates = obj.Stargates;
        }

        public static void GetItemType(this ItemType me)
        {
            var reqUri = string.Format("{0}/{1}/{2}/{3}", _baseURI, _universe, _types, me.Id.ToString(), _baseURITail);
            
            var obj = (JKON.EveWho.Types.ItemType)BaseRequest(reqUri).Deserialize(typeof(JKON.EveWho.Types.ItemType));
            me.Capacity = obj.Capacity;
            me.Description = obj.Description;
            me.Dogma_Attributes = obj.Dogma_Attributes;
            me.Dogma_Effects = obj.Dogma_Effects;
            me.Graphic_Id = obj.Graphic_Id;
            me.Group_Id = obj.Group_Id;
            me.Name = obj.Name;
            me.Portion_Size = obj.Portion_Size;
            me.Published = obj.Published;
            me.Radius = obj.Radius;
            me.Volume = obj.Volume;
        }

        public static Wars.Wars GetWars()
        {
            var reqUri = string.Format("{0}/{1}/{2}", _baseURI, _wars, _baseURITail);

            var warList = (List<long>)BaseRequest(reqUri).Deserialize(typeof(List<long>));

            return new Wars.Wars() { WarIds = warList.ToArray<long>() };
        }

        public static void GetWar(this War me)
        {
            var reqUri = string.Format("{0}/{1}/{2}/{3}", _baseURI, _wars, me.Id.ToString(), _baseURITail);

            var obj = (War)BaseRequest(reqUri).Deserialize(typeof(War));
            me.Aggressor = obj.Aggressor;
            me.Declared = obj.Declared;
            me.Defender = obj.Defender;
            me.Mutual = obj.Mutual;
            me.OpenForAllies = obj.OpenForAllies;
            me.StartTime = obj.StartTime;
        }

        public static void GetCorporation(this Corporation.Corporation me)
        {
            var reqUri = string.Format("{0}/{1}/{2}/{3}", _baseURI, _corp, me.Id.ToString(), _baseURITail);

            var obj = (Corporation.Corporation)BaseRequest(reqUri).Deserialize(typeof(Corporation.Corporation));
            me.Alliance_Id = obj.Alliance_Id;
            me.CEO_Id = obj.CEO_Id;
            me.Creation_Date = obj.Creation_Date;
            me.Creator_Id = obj.Creator_Id;
            me.Corporation_Description = obj.Corporation_Description;
            me.Member_Count = obj.Member_Count;
            me.Corporation_Name = obj.Corporation_Name;
            me.Tax_Rate = obj.Tax_Rate;
            me.Ticker = obj.Ticker;
            me.Url = obj.Url;
        }
        public static void GetAlliance(this Alliance me)
        {
            var reqUri = string.Format("{0}/{1}/{2}/{3}", _baseURI, _alliance, me.Id.ToString(), _baseURITail);

            var obj = (Alliance)BaseRequest(reqUri).Deserialize(typeof(Alliance));
            me.Executor_Corp = obj.Executor_Corp;
            me.Date_Founded = obj.Date_Founded;
            me.Alliance_Name = obj.Alliance_Name;
            me.Ticker = obj.Ticker;
        }
    }
}
