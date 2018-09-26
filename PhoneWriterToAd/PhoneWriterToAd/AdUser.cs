using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace telefonyDoAD
{
    /// <summary>
    /// třída pro uložení vybraných uživatelských atributů z AD
    /// (stará verze bez slovníku)
    /// </summary>
    class AdUser
    {
        public string nameGiven;        //křestní
        public string nameSurn;         //příjmení
        public string nameFull;         //Příjmení + křestní
        public string nameAcco;         //uživatelské jméno (jvaldauf)   //samaccountname
        private string namePrincipal;   // nameAcco + @společnost.cz
        public string description;      // pozice (ADMINISTRATOR)        //description
        public string title;            // pozice (ADMINISTRATOR)        //Title
        public string office;           // středisko přesně (17120)
        public string department;       // středisko obecné (17000)
        public string tHomePhone;       //pevná dlouhá                   //homePhone
        public string tHomePhoneOthers; //pevná dlouhá další             //otherHomePhone
        public string tIpPhone;         //pevná zkrácená                 //ipPhone
        public string tIpPhoneOthers;   //pevná zkrácená další           //otherIpPhone
        public string tMob;             //mobil dlouhá                   //Mobile
        public string tMobOthers;       //mobil dlouhá další             //otherMobile
        public string tTel;             //mobil zkrácená                 //TelephoneNumber
        public string tTelOthers;       //mobil zkrácená další           //otherTelephone
        public string csvGsm;           //sloupce exportu CSV
        public string csvData;          //sloupce exportu CSV
        public string csvGsmo;          //sloupce exportu CSV
        public string csvFix;           //sloupce exportu CSV
        public string csvFixo;          //sloupce exportu CSV
        public string telephoneLog;     //seznam provedených změn

        public AdUser()
        {
            cleanse();
        }

        public AdUser(string nameAcco)
        {
            this.nameGiven = "";
            this.nameSurn = "";
            this.nameFull = "";
            this.nameAcco = nameAcco;
            this.namePrincipal = "";
            this.description = "";
            this.title = "";
            this.office = "";
            this.department = "";
            this.telephoneLog = "";

            this.csvGsm = "";
            this.csvData = "";
            this.csvGsmo = "";
            this.csvFix = "";
            this.csvFixo = "";

            this.tHomePhone       = "";
            this.tHomePhoneOthers = "";
            this.tIpPhoneOthers   = "";
            this.tIpPhone         = "";
            this.tMob             = "";
            this.tMobOthers       = "";
            this.tTel             = "";
            this.tTelOthers       = "";
        }

        /// <summary>
        /// smaže všechny vlastnosti
        /// </summary>
        public void cleanse()
        {
            this.nameGiven = "";
            this.nameSurn = "";
            this.nameFull = "";
            this.nameAcco = "";
            this.namePrincipal = "";
            this.description = "";
            this.title = "";
            this.office = "";
            this.department = "";

            this.tHomePhone       = "";
            this.tHomePhoneOthers = "";
            this.tIpPhoneOthers   = "";
            this.tIpPhone         = "";
            this.tMob             = "";
            this.tMobOthers       = "";
            this.tTel             = "";
            this.tTelOthers       = "";

            this.csvGsm  = "";
            this.csvData = "";
            this.csvGsmo = "";
            this.csvFix  = "";
            this.csvFixo = "";

            this.telephoneLog = "";

        }

        /// <summary>
        /// porovná nameAcco vůči předanému objektu
        /// </summary>
        /// <param name="user">porovná nameAcco</param>
        /// <returns></returns>
        public bool equalAccountName(AdUser user)
        {
            if (this.nameAcco.Equals(user.nameAcco))
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
