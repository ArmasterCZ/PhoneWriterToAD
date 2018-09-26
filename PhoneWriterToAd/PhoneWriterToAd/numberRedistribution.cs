using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace telefonyDoAD
{
    /// <summary>
    /// This class transform numbers to specific AD attribute based on complicated conditions.
    /// </summary>
    class NumberRedistribution
    {
        private List<AdUser> userListCSV = new List<AdUser>();        //list of CSV user
        private List<AdUser> userListAd = new List<AdUser>();         //list of AD user
        private List<AdUser> userListFinal = new List<AdUser>();      //final list of user with sorted attributes
        public EventHandler<EventArgsLog> callBackEditLog;            //call back for changes

        public NumberRedistribution(List<AdUser> csv, List<AdUser> ad)
        {
            this.userListCSV = csv;
            this.userListAd = ad;
        }

        /// <summary>
        /// redistribute number in userListCSV to atributes in userListCSV
        /// </summary>
        public void processCsvToAtrib()
        {

            foreach (AdUser user in userListCSV)
            {
                //FIX 
                if (user.csvFix != "")
                {
                    if (user.csvFix.Length < 4)
                    {
                        if (user.csvFix.First() == '8')
                        {
                            user.tTel = user.csvFix;
                            user.tHomePhone = "";
                            user.tIpPhone = "";
                        }
                        else
                        {
                            user.tHomePhone = "267 198 " + user.csvFix;
                            user.tIpPhone = user.csvFix;
                        }
                    }
                    else
                    {
                        user.tHomePhone = addTelSpaces(user.csvFix);
                        user.tIpPhone = "";
                    }
                }
                else
                {
                    user.tHomePhone = "";
                    user.tIpPhone = "";
                }

                //GSM
                if (user.csvGsm != "")
                {
                    if ((user.tTel.Equals("")) & (!user.tIpPhone.Equals("")))
                    {
                        user.tTel = user.tIpPhone;
                    }

                    user.tMob = addTelSpaces(user.csvGsm);
                }
                else
                {
                    user.tTel = "";
                    user.tMob = "";
                }

                //FIXo
                user.tIpPhoneOthers = user.csvFixo;

                //GSMo
                user.tMobOthers = user.csvGsmo;


            } //foreach user
        }

        /// <summary>
        /// compare local "UserListAd" with sended list "userListCSV" and create "userListFinal"
        /// </summary>
        public void makeFinalList()
        {
            foreach (AdUser user in userListAd)
            {
                //search equalent in csv list
                AdUser userInCsv = userListCSV.Find(x => x.nameAcco.Equals(user.nameAcco));
                AdUser finalUser = null;

                bool ignoreThisUser = isUserInIgnored(user);
                //user is in ignore list
                if (!ignoreThisUser)
                {
                    //user was found in CSV List
                    if (userInCsv != null)
                    {
                        finalUser = redistributionBasedOnAd(user, userInCsv);
                    }
                    else
                    {
                        //user isnt in CSV list
                        //clear all number atributes and add static number
                        finalUser = redistributionNoInCsv(user);
                    }

                    if (finalUser != null)
                    {
                        userListFinal.Add(finalUser);
                    }
                } //ignored user

            }
        }

        /// <summary>
        /// return userListFinal
        /// </summary>
        public List<AdUser> getFinalUserList()
        {
            return userListFinal;
        }

        /// <summary>
        /// return final diference between userListFinal and userListAd
        /// </summary>
        /// <returns>list of needed changes in AD</returns>
        public List<telephoneUser> getFinalDiferences()
        {
            List<telephoneUser> finalList = new List<telephoneUser>();

            foreach (AdUser user in userListAd)
            {

                /*if (user.nameAcco.Equals("jvaldauf"))
                {
                    Console.WriteLine();
                }*/

                AdUser userfound = userListFinal.Find(x => x.nameAcco.Equals(user.nameAcco));
                telephoneUser diferenceUser = new telephoneUser(user.nameAcco);

                //user was found in List
                if (userfound != null)
                {
                    //check diference between numbers
                    if (!user.tTel.Equals(userfound.tTel))
                    {
                        //editLog($"    {user.nameAcco} (tTel):       z [{user.tTel}] , na [{userfound.tTel}] ");
                        diferenceUser.attributes.Add("TelephoneNumber");
                        diferenceUser.attribData.Add(userfound.tTel);
                        editLog($"  {user.nameAcco} ({"TelephoneNumber"}) - z [{user.tTel}], na [{userfound.tTel}]");
                    }
                    /* static field for impot from AD
                    if (!user.tTelOthers.Equals(userfound.tTelOthers))
                    {
                        editLog($"    {user.nameAcco} (tTelOthers): z [{user.tTelOthers}] , na [{userfound.tTelOthers}] ");
                    }*/
                    if (!user.tIpPhone.Equals(userfound.tIpPhone))
                    {
                        //editLog($"    {user.nameAcco} (tIpPhone):         z [{user.tIpPhone}] , na [{userfound.tIpPhone}] ");
                        diferenceUser.attributes.Add("ipPhone");
                        diferenceUser.attribData.Add(userfound.tIpPhone);
                        editLog($"  {user.nameAcco} ({"ipPhone"}) - z [{user.tIpPhone}], na [{userfound.tIpPhone}]");
                    }
                    if (!user.tIpPhoneOthers.Equals(userfound.tIpPhoneOthers))
                    {
                        //editLog($"    {user.nameAcco} (tIpPhoneOthers):   z [{user.tIpPhoneOthers}] , na [{userfound.tIpPhoneOthers}] ");
                        diferenceUser.attributes.Add("otherIpPhone");
                        diferenceUser.attribData.Add(userfound.tIpPhoneOthers);
                        editLog($"  {user.nameAcco} ({"otherIpPhone"}) - z [{user.tIpPhoneOthers}], na [{userfound.tIpPhoneOthers}]");
                    }
                    if (!user.tMob.Equals(userfound.tMob))
                    {
                        //editLog($"    {user.nameAcco} (tMob):             z [{user.tMob}] , na [{userfound.tMob}] ");
                        diferenceUser.attributes.Add("Mobile");
                        diferenceUser.attribData.Add(userfound.tMob);
                        editLog($"  {user.nameAcco} ({"Mobile"}) - z [{user.tMob}], na [{userfound.tMob}]");
                    }
                    if (!user.tMobOthers.Equals(userfound.tMobOthers))
                    {
                        //editLog($"    {user.nameAcco} (tMobOthers):       z [{user.tMobOthers}] , na [{userfound.tMobOthers}] ");
                        diferenceUser.attributes.Add("otherMobile");
                        diferenceUser.attribData.Add(userfound.tMobOthers);
                        editLog($"  {user.nameAcco} ({"otherMobile"}) - z [{user.tMobOthers}], na [{userfound.tMobOthers}]");
                    }
                    if (!user.tHomePhone.Equals(userfound.tHomePhone))
                    {
                        //editLog($"    {user.nameAcco} (tHomePhone):       z [{user.tHomePhone}] , na [{userfound.tHomePhone}] ");
                        diferenceUser.attributes.Add("homePhone");
                        diferenceUser.attribData.Add(userfound.tHomePhone);
                        editLog($"  {user.nameAcco} ({"homePhone"}) - z [{user.tHomePhone}], na [{userfound.tHomePhone}]");
                    }
                    if (!user.tHomePhoneOthers.Equals(userfound.tHomePhoneOthers))
                    {
                        //editLog($"    {user.nameAcco} (tHomePhoneOthers): z [{user.tHomePhoneOthers}] , na [{userfound.tHomePhoneOthers}] ");
                        diferenceUser.attributes.Add("otherHomePhone");
                        diferenceUser.attribData.Add(userfound.tHomePhoneOthers);
                        editLog($"  {user.nameAcco} ({"otherHomePhone"}) - z [{user.tHomePhoneOthers}], na [{userfound.tHomePhoneOthers}]");
                    }

                    //add changed user to list
                    if (diferenceUser.attributes.Count() > 0)
                    {
                        finalList.Add(diferenceUser);
                    }

                }

            }
            return finalList;

        }

        /// <summary>
        /// insert space in telephone number
        /// </summary>
        /// <param name="strTelephone">telephone number</param>
        /// <returns>change input(823854870) to (823 854 870) </returns>
        private string addTelSpaces(string strTelephone)
        {
            string finalString = strTelephone;
            if (strTelephone.Length > 3)
            {
                finalString = finalString.Insert(3, " ");
            }
            if (strTelephone.Length > 6)
            {
                finalString = finalString.Insert(7, " ");
            }
            return finalString;

        }

        /// <summary>
        /// check if user is in ignored list
        /// </summary>
        /// <param name="user">user to check</param>
        /// <returns>is in ignored</returns>
        private bool isUserInIgnored(AdUser user)
        {
            List<AdUser> adUserIngnoredList = new List<AdUser>() {
            new AdUser { nameAcco = "testUser10" } ,
            new AdUser { nameAcco = "testUser9" },
            new AdUser { nameAcco = "testUser8" },
            new AdUser { nameAcco = "testUser7" },
            new AdUser { nameAcco = "testUser6" },
            new AdUser { nameAcco = "testUser5" },
            new AdUser { nameAcco = "testUser4" },
            new AdUser { nameAcco = "testUser3" },
            new AdUser { nameAcco = "testUser2" },
            new AdUser { nameAcco = "testUser1" },
            new AdUser { nameAcco = "ftester" }
            };
            AdUser userInList = adUserIngnoredList.Find(x => x.nameAcco.Equals(user.nameAcco));
            if (userInList != null)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// number redistribution based on information from AD
        /// can write tIpPhone and change "," in numbers to ";"
        /// </summary>
        /// <param name="adUser">CSV user</param>
        /// <param name="csvUser">AD user</param>
        /// <returns>user with redistributed numbers</returns>
        private AdUser redistributionBasedOnAd(AdUser adUser, AdUser csvUser)
        {
            AdUser localUser = csvUser;
            if ((localUser.tIpPhone.Equals("")) && (!adUser.tTelOthers.Equals("")))
            {
                //Console.WriteLine();
                localUser.tIpPhone = adUser.tTelOthers;
                localUser.telephoneLog += $"(4) číslo přesunuto z (static) otherTelephone do ipPhone. [{adUser.tTelOthers}];";
            }

            localUser.tTelOthers = localUser.tTelOthers.Replace(",", ";");
            localUser.tMobOthers = localUser.tMobOthers.Replace(",", ";");
            localUser.tIpPhoneOthers = localUser.tIpPhoneOthers.Replace(",", ";");
            localUser.tHomePhoneOthers = localUser.tHomePhoneOthers.Replace(",", ";");

            return localUser;
        }

        /// <summary>
        /// redistribution for user existing only in AD
        /// </summary>
        /// <param name="user">user</param>
        /// <returns>user with no telephone and check otherTelephone</returns>
        private AdUser redistributionNoInCsv(AdUser user)
        {
            if (!user.tIpPhone.Equals(user.tTelOthers))
            {
                user.tIpPhone = user.tTelOthers;
                user.telephoneLog += $"(4) číslo přesunuto z (static) otherTelephone do ipPhone. [{user.tTelOthers}];";
            }
            else
            {
                user.tIpPhone = "";
            }
            user.tIpPhoneOthers = "";
            user.tMob = "";
            user.tMobOthers = "";
            user.tHomePhone = "";
            user.tHomePhoneOthers = "";
            user.tTel = "";

            return user;
        }

        /// <summary>
        /// use callback to report log
        /// </summary>
        /// <param name="message">message</param>
        private void editLog(string message)
        {
            if (callBackEditLog != null)
            {
                callBackEditLog(null, new EventArgsLog { strLog = message });
            }
        }
    }
}
