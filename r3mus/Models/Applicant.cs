using eZet.EveLib.EveXmlModule;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Net;
using System.Web;

namespace r3mus.Models
{
    public class Applicant
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        public int Id { get; set; }

        public string Name { get; set; }

        [Display(Name = "Email Address")]
        public string EmailAddress { get; set; }

        [Display(Name = "Time Zone")]
        public string TimeZone { get; set; }

        [Display(Name = "Api Key")]
        public int ApiKey { get; set; }

        [Display(Name = "Verification Code")]
        public string VerificationCode { get; set; }

        public virtual List<ApplicantApiInfo> APIInfoes { get; set; }

        public string Information { get; set; }
        public string Age { get; set; }

        [Display(Name = "Played for")]
        public string ToonAge { get; set; }

        public string Source { get; set; }
    }

    public class ApplicantApiInfo
    {
        public int Id { get; set; }

        [Display(Name = "Api Key")]
        public int ApiKey { get; set; }

        [Display(Name = "Verification Code")]
        [DataType(DataType.MultilineText)]
        [UIHint("DisplayVCode")]
        public string VerificationCode { get; set; }

        [NotMapped]
        [Display(Name = "Access Mask")]
        [UIHint("AccessMaskHighlight")]
        public long AccessMask
        {
            get
            {
                try
                {
                    var req = WebRequest.Create("https://api.eveonline.com");
                    req.Timeout = 2000;
                    HttpWebResponse response = (HttpWebResponse)req.GetResponse();
                    return EveXml.CreateApiKey(Convert.ToInt32(ApiKey), VerificationCode).Init().AccessMask;
                }
                catch (Exception ex) { return -1; }
            }
        }

        public virtual Applicant Applicant { get; set; }

        public List<Character> GetDetails()
        {
            CharacterKey key = EveXml.CreateCharacterKey(ApiKey, VerificationCode);
            var chars = key.Characters.ToList();

            //var charList = key.GetCharacterList();
            return chars;
        }

        public bool ValidateAccessMask(ApplicationUser.IDType type)
        {
            bool result = false;
            long allianceAccessMask = 8388608;
            long corpOldAccessMask = 268435455;
            long corpNewAccessMask = Properties.Settings.Default.FullAPIAccessMask;

            try
            {
                var mask = AccessMask;

                if (type == ApplicationUser.IDType.Corporation)
                {
                    result = ((mask == corpOldAccessMask) || (mask == corpNewAccessMask));
                }
                else if (type == ApplicationUser.IDType.Alliance)
                {
                    result = (mask == allianceAccessMask);
                }
            }
            catch (Exception ex) { }

            return result;
        }
    }

}