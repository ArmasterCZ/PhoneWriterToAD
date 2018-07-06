using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.DirectoryServices;
using System.DirectoryServices.Protocols;
using System.DirectoryServices.AccountManagement;

namespace telefonyDoAD
{
    class AdConnection
    {
        /// <summary>
        /// get all user from AD or to write changes from List (telephoneUser).
        /// </summary>

        public EventHandler<EventArgsLog> callBackErrorLog;
        public EventHandler<EventArgsLog> callBackEditLog;
        public List<AdUser> userListAd { get; private set; } = new List<AdUser>();

        public AdConnection(EventHandler<EventArgsLog> errorCallBack, EventHandler<EventArgsLog> editCallBack)
        {
            //set callbacks
            callBackErrorLog = errorCallBack;
            callBackEditLog = editCallBack;
            //load user from AD
            loadUserListAd();
        }

        //public AdConnection(List<AdUser> csvList, EventHandler<EventArgsLog> errorCallBack, EventHandler<EventArgsLog> editCallBack, bool showOnly)
        //{
        //    //set callbacks
        //    callBackErrorLog = errorCallBack;
        //    callBackEditLog = editCallBack;
        //    //load user from AD
        //    loadUserListAd();
        //    //write changes to user in AD
        //    writeDiferencesToAd(csvList, showOnly);
        //}

        public void loadUserListAd()
        {
            //load all user from AD to local list
            userListAd = new List<AdUser>();
            using (var context = new PrincipalContext(ContextType.Domain, "sitel.cz"))
            {
                using (var searcher = new PrincipalSearcher(new UserPrincipal(context)))
                {
                    foreach (var result in searcher.FindAll())
                    {
                        AdUser localAdUser = new AdUser();

                        DirectoryEntry de = result.GetUnderlyingObject() as DirectoryEntry;
                        if (de.Properties["samAccountName"].Value.ToString() != null)
                        {
                            localAdUser.nameGiven = "" + de.Properties["givenName"].Value;
                            localAdUser.nameSurn = "" + de.Properties["sn"].Value;
                            localAdUser.nameAcco = "" + de.Properties["samAccountName"].Value;
                            localAdUser.nameFull = "" + de.Properties["userPrincipalName"].Value;
                            localAdUser.tHomePhone = "" + de.Properties["homePhone"].Value;
                            localAdUser.tHomePhoneOthers = "" + de.Properties["otherHomePhone"].Value;
                            localAdUser.tIpPhone = "" + de.Properties["ipPhone"].Value;
                            localAdUser.tIpPhoneOthers = "" + de.Properties["otherIpPhone"].Value;
                            localAdUser.tMob = "" + de.Properties["Mobile"].Value;
                            localAdUser.tMobOthers = "" + de.Properties["otherMobile"].Value;
                            localAdUser.tTel = "" + de.Properties["TelephoneNumber"].Value;
                            localAdUser.tTelOthers = "" + de.Properties["otherTelephone"].Value;
                        }
                        else
                        {
                            Exception exc = new Exception("Error in load users from AD. Foud user with no account name. Loading Closed.");
                            throw exc;
                        }

                        userListAd.Add(localAdUser);
                    }
                }
            }

        }

        public void writeDiferencesToAd(List<AdUser> listToCompare, bool showOnly)
        {
            //compare local "UserListAd" with sended list "listToCompare" and write diferences (local == From, sended == For)

            List<string> adUserFindedByCsv = new List<string>();
            List<string> adUserDefault = new List<string>();
            List<string> adUserIgnored = new List<string>();

            foreach (AdUser user in userListAd)
            {
                List<string> attributes = new List<string>();
                List<string> attribData = new List<string>();

                //search equalent in csv list
                AdUser userInCsv = listToCompare.Find(x => x.nameAcco.Equals(user.nameAcco));

                bool ignoreThisUser = isUserInIgnored(user);
                //user isnt in ignore list
                if (!ignoreThisUser)
                {
                    //user was found in CSV List
                    if (userInCsv != null)
                    {
                        //check difference between current attributes and edited attribute, mark changes for edit

                        List<List<string>> list = markDiferencesAgainCsv(user, userInCsv);
                        attributes = list[0];
                        attribData = list[1];

                        adUserFindedByCsv.Add(user.nameAcco);

                    } else
                    {
                        //user isnt in CSV list
                        //clear all number atributes and add static number

                        List<List<string>> list = markDiferencesDefault(user);
                        attributes = list[0];
                        attribData = list[1];

                        adUserDefault.Add(user.nameAcco);
                    }

                    //write changes to AD
                    if (attributes.Count() > 0)
                    {
                        string nameAcco = user.nameAcco;
                        if (!showOnly)
                        {
                            try
                            {
                                editUserInAd(nameAcco, attributes.ToArray(), attribData.ToArray());
                                editLog($"    --(Uživatel upraven {nameAcco})------------------------");
                            }
                            catch (Exception ex)
                            {
                                errorLog($"Exception AdConnection:\n\n {ex.ToString()}");
                            }
                        }

                    }
                } //ignored user
                else
                {
                    adUserIgnored.Add(user.nameAcco);
                }

            }//

            Console.WriteLine($"Uživatelů zkontrolováno vuci CSV: [{adUserFindedByCsv.Count()}]");
            Console.WriteLine($"Uživatelů zkontrolováno default : [{adUserDefault.Count()}]");
            Console.WriteLine($"Uživatelů přeskočeno            : [{adUserIgnored.Count()}]");

        }

        private void editUserInAd(string userName, string[] parameters, string[] newAtrValues)
        {
            //edit single atribute for single user
            string oldValue = "";
            if (!((parameters.Length > 0) & (newAtrValues.Length > 0) & (parameters.Length == newAtrValues.Length)))
            {
                Exception ex = new Exception($"Zapsání atributů pro ({ userName }) se nezdařilo. Nebyl zadán správný počet parametrů a hodnot");
                throw ex;
            }

            //search user with parameters
            DirectorySearcher search = new DirectorySearcher();
            search.Filter = "(sAMAccountName=" + userName + ")";
            foreach (string parameter in parameters)
            {
                search.PropertiesToLoad.Add(parameter);
            }
            SearchResult result = search.FindOne();

            //check if user was found in AD
            if (result != null)
            {
                //edit all marked atributes
                for (int count = 0; count < parameters.Count(); count++)
                {

                    string parameter = parameters[count];
                    string newAtrValue = newAtrValues[count];

                    if (parameter.Equals(""))
                    {
                        //atribute need name
                        //Exception ex = new Exception($"Nemohl být upraven uživatel ({ userName }) v AD. Pokus o úpravu prázdného atributu");
                        //throw ex;
                        errorLog($"Nemohl být upraven uživatel ({ userName }) v AD. Pokus o úpravu prázdného atributu");
                    }
                    else
                    {
                        DirectoryEntry entryToUpdate = null;
                        try
                        {
                            //edit target atribute (parameter) for user (userName) from AD
                            entryToUpdate = result.GetDirectoryEntry();
                            //check current value of atribute (need check for no value)
                            if (entryToUpdate.Properties[parameter].Count > 0)
                            {
                                oldValue = "" + entryToUpdate.Properties[parameter][0].ToString();
                            }
                            else
                            {
                                oldValue = "";
                            }

                            //edit to new value or clear
                            if (!newAtrValue.Equals(""))
                            {
                                entryToUpdate.Properties[parameter].Value = newAtrValue;
                            }
                            else
                            {
                                entryToUpdate.Properties[parameter].Clear();
                            }

                            //write to AD
                            entryToUpdate.CommitChanges();
                        }
                        catch (Exception)
                        {
                            //Exception ex = new Exception($"Nemohl být upraven uživatel ({ userName }) v AD. Atribut ({parameter}). Oprávnění?");
                            //throw ex;
                            errorLog($"Nemohl být upraven uživatel ({ userName }) v AD. Atribut ({parameter}). Oprávnění?");
                        }
                        finally
                        {
                            if (entryToUpdate != null)
                            {
                                entryToUpdate.Close();
                            }
                        }
                    } //if parameter ""
                } //for all
            } //if user null
            else
            {
                //Exception ex = new Exception($"Uživatel ({ userName }) nenalezen v AD. Nemohly být upraveny atributy.");
                //throw ex;
                errorLog($"Uživatel ({ userName }) nenalezen v AD. Nemohly být upraveny atributy.");
            }
        }

        private void clearUserTelephonesInAd (string userName)
        {
            //remove telephones data from user
            string[] parameters = new string[] { "homePhone", "otherHomePhone", "ipPhone", "otherIpPhone", "Mobile", "otherMobile", "TelephoneNumber", "otherTelephone" };
            string[] newAtrValues = new string[] { "", "", "", "", "", "", "", "" };
            editUserInAd(userName, parameters, newAtrValues);
    }

        private void errorLog(string message)
        {
            //use callback to report errors
            if (callBackErrorLog != null)
            {
                callBackErrorLog(null, new EventArgsLog { strLog = message });
            }
        }

        private void editLog(string message)
        {
            //use callback to report logs
            if (callBackEditLog != null)
            {
                callBackEditLog(null, new EventArgsLog { strLog = message });
            }
        }

        public void writeChangesToAD(List<telephoneUser> telephoneUserList)
        {
            foreach (telephoneUser telephoneUser in telephoneUserList)
            {
                //write changes to AD
                if (telephoneUser.attributes.Count() > 0)
                {
                    string nameAcco = telephoneUser.accountName;

                    try
                    {
                        editUserInAd(nameAcco, telephoneUser.attributes.ToArray(), telephoneUser.attribData.ToArray());
                        editLog($"    -(Uživatel upraven {nameAcco})");
                    }
                    catch (Exception ex)
                    {
                        errorLog($"Exception AdConnection:\n\n {ex.ToString()}");
                    }

                }
            }

        }

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

        private AdUser lastRedistribution(AdUser adUser, AdUser csvUser)
        {
            //number redistribution based on information from AD
            //can write tIpPhone
            //cange "," in numbers to ";"

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

        private List<List<string>> markDiferencesAgainCsv(AdUser user, AdUser userInCsv)
        {
            //return 2 List with data to change. [0] == List<string> attributes, [1] == List<string> attribData
            List<List<string>> finalList = new List<List<string>>();
            List<string> attributes = new List<string>();
            List<string> attribData = new List<string>();

            //redistribute telephones based on AD atributes
            userInCsv = lastRedistribution(user, userInCsv);

            //check diference between numbers
            if (!user.tTel.Equals(userInCsv.tTel))
            {
                editLog($"    {user.nameAcco} (tTel):       z [{user.tTel}] , na [{userInCsv.tTel}] ");
                attributes.Add("TelephoneNumber");
                attribData.Add(userInCsv.tTel);
            }
            //static field for impot from AD
            //if (!user.tTelOthers.Equals(userInCsv.tTelOthers))
            //{
            //    editLog($"    {user.nameAcco} (tTelOthers): z [{user.tTelOthers}] , na [{userInCsv.tTelOthers}] ");
            //}
            if (!user.tIpPhone.Equals(userInCsv.tIpPhone))
            {
                editLog($"    {user.nameAcco} (tIpPhone):         z [{user.tIpPhone}] , na [{userInCsv.tIpPhone}] ");
                attributes.Add("ipPhone");
                attribData.Add(userInCsv.tIpPhone);
            }
            if (!user.tIpPhoneOthers.Equals(userInCsv.tIpPhoneOthers))
            {
                editLog($"    {user.nameAcco} (tIpPhoneOthers):   z [{user.tIpPhoneOthers}] , na [{userInCsv.tIpPhoneOthers}] ");
                attributes.Add("otherIpPhone");
                attribData.Add(userInCsv.tIpPhoneOthers);
            }
            if (!user.tMob.Equals(userInCsv.tMob))
            {
                editLog($"    {user.nameAcco} (tMob):             z [{user.tMob}] , na [{userInCsv.tMob}] ");
                attributes.Add("Mobile");
                attribData.Add(userInCsv.tMob);
            }
            if (!user.tMobOthers.Equals(userInCsv.tMobOthers))
            {
                editLog($"    {user.nameAcco} (tMobOthers):       z [{user.tMobOthers}] , na [{userInCsv.tMobOthers}] ");
                attributes.Add("otherMobile");
                attribData.Add(userInCsv.tMobOthers);
            }
            if (!user.tHomePhone.Equals(userInCsv.tHomePhone))
            {
                editLog($"    {user.nameAcco} (tHomePhone):       z [{user.tHomePhone}] , na [{userInCsv.tHomePhone}] ");
                attributes.Add("homePhone");
                attribData.Add(userInCsv.tHomePhone);
            }
            if (!user.tHomePhoneOthers.Equals(userInCsv.tHomePhoneOthers))
            {
                editLog($"    {user.nameAcco} (tHomePhoneOthers): z [{user.tHomePhoneOthers}] , na [{userInCsv.tHomePhoneOthers}] ");
                attributes.Add("otherHomePhone");
                attribData.Add(userInCsv.tHomePhoneOthers);
            }

            finalList.Add(attributes);
            finalList.Add(attribData);

            return finalList;
        }

        private List<List<string>> markDiferencesDefault(AdUser user)
        {
            //return 2 List with data to change. [0] == List<string> attributes, [1] == List<string> attribData
            List<List<string>> finalList = new List<List<string>>();
            List<string> attributes = new List<string>();
            List<string> attribData = new List<string>();

            //clear all number atributes and add static number

            if (!user.tTel.Equals(""))
            {
                editLog($"    {user.nameAcco} (tTel):       z [{user.tTel}] , na [] ");
                attributes.Add("TelephoneNumber");
                attribData.Add("");
            }
            //edited to corespond politic
            if (!user.tIpPhone.Equals(user.tTelOthers))
            {
                editLog($"    {user.nameAcco} (tIpPhone):         z [{user.tIpPhone}] , na [{user.tTelOthers}] ");
                attributes.Add("ipPhone");
                attribData.Add(user.tTelOthers);
            }
            if (!user.tIpPhoneOthers.Equals(""))
            {
                editLog($"    {user.nameAcco} (tIpPhoneOthers):   z [{user.tIpPhoneOthers}] , na [] ");
                attributes.Add("otherIpPhone");
                attribData.Add("");
            }
            if (!user.tMob.Equals(""))
            {
                editLog($"    {user.nameAcco} (tMob):             z [{user.tMob}] , na [] ");
                attributes.Add("Mobile");
                attribData.Add("");
            }
            if (!user.tMobOthers.Equals(""))
            {
                editLog($"    {user.nameAcco} (tMobOthers):       z [{user.tMobOthers}] , na [] ");
                attributes.Add("otherMobile");
                attribData.Add("");
            }
            if (!user.tHomePhone.Equals(""))
            {
                editLog($"    {user.nameAcco} (tHomePhone):       z [{user.tHomePhone}] , na [] ");
                attributes.Add("homePhone");
                attribData.Add("");
            }
            if (!user.tHomePhoneOthers.Equals(""))
            {
                editLog($"    {user.nameAcco} (tHomePhoneOthers): z [{user.tHomePhoneOthers}] , na [] ");
                attributes.Add("otherHomePhone");
                attribData.Add("");
            }

            finalList.Add(attributes);
            finalList.Add(attribData);

            return finalList;
        }

        /*
        private void editUserOneInAd(string userName, string parameter, string newValue)
        {
            //edit single atribute for single user

            try
            {
                DirectorySearcher search = new DirectorySearcher();
                search.Filter = "(sAMAccountName=" + userName + ")";
                search.PropertiesToLoad.Add(parameter);
                SearchResult result = search.FindOne();

                if (result != null)
                {
                    try
                    {
                        //edit target atribute (parameter) for user (userName) from AD
                        using (DirectoryEntry entryToUpdate = result.GetDirectoryEntry())
                        {
                            //check current value of atribute (need check for no value)
                            string oldValue = "";
                            if (entryToUpdate.Properties[parameter].Count > 0)
                            {
                                oldValue = "" + entryToUpdate.Properties[parameter][0].ToString();
                            }
                            else
                            {
                                oldValue = "";
                            }

                            //write new value
                            entryToUpdate.Properties[parameter].Value = newValue;
                            entryToUpdate.CommitChanges();
                        }
                    }
                    catch (Exception ex)
                    {
                        Exception ex1 = new Exception($"Nemohl být upraven uživatel ({ userName }) v AD. Atribut ({parameter}). Oprávnění?");
                        throw ex1;
                    }

                }
                else
                {
                    Exception ex = new Exception($"Uživatel ({ userName }) nenalezen v AD. Nemohl být upraven atribut ({parameter}).");
                    throw ex;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception:\n\n" + ex.ToString());
                throw ex;
            }
        }

        public void editTestUser()
        {
            string userName = "testUser6";
            string parameter = "description";
            string newValue = "nova hodnota6";
            editUserOneInAd(userName, parameter, newValue);

            userName = "testUser7";
            parameter = "description";
            newValue = "nova hodnota7";
            editUserOneInAd(userName, parameter, newValue);

            string userName2 = "testUser8";
            string[] parameters = { "description", "title" };
            string[] newValues = { "descr item8", "" };
            editUserInAd(userName2, parameters, newValues);
        }
        /**/

        /* functional load of AD user/s
        private static List<AdUser> getAllAdUser()
        {
            List<AdUser> UserListAd = new List<AdUser>();
            using (var context = new PrincipalContext(ContextType.Domain, "sitel.cz"))
            {
                using (var searcher = new PrincipalSearcher(new UserPrincipal(context)))
                {
                    foreach (var result in searcher.FindAll())
                    {
                        AdUser localAdUser = new AdUser();

                        DirectoryEntry de = result.GetUnderlyingObject() as DirectoryEntry;
                        if (de.Properties["samAccountName"].Value.ToString() != null)
                        {
                            localAdUser.nameGiven       = "" + de.Properties["givenName"].Value;
                            localAdUser.nameSurn        = "" + de.Properties["sn"].Value;
                            localAdUser.nameAcco        = "" + de.Properties["samAccountName"].Value;
                            localAdUser.nameFull        = "" + de.Properties["userPrincipalName"].Value;
                            localAdUser.tHomePhone      = "" + de.Properties["homePhone"].Value;
                            localAdUser.tHomePhoneOthers= "" + de.Properties["otherHomePhone"].Value;
                            localAdUser.tIpPhone        = "" + de.Properties["ipPhone"].Value;
                            localAdUser.tIpPhoneOthers  = "" + de.Properties["otherIpPhone"].Value;
                            localAdUser.tMob            = "" + de.Properties["Mobile"].Value;
                            localAdUser.tMobOthers      = "" + de.Properties["otherMobile"].Value;
                            localAdUser.tTel            = "" + de.Properties["TelephoneNumber"].Value;
                            localAdUser.tTelOthers      = "" + de.Properties["otherTelephone"].Value;
                        }
                        else
                        {
                            Exception exc = new Exception("Error in load users from AD. Foud user with no account name. Loading Closed.");
                            throw exc;
                        }


                        userListAd.Add(localAdUser);
                    }
                }
            }
            return userListAd;
        }

        private static void getOneAdUser()
        {
            string searchedUser = "testUser10";
            using (var context = new PrincipalContext(ContextType.Domain, "sitel.cz"))
            {
                using (var user = UserPrincipal.FindByIdentity(context, IdentityType.Name, searchedUser))
                {
                    //user.SamAccountName = "SamAccountName";
                    //user.UserPrincipalName = "UserPrincipalName";
                    //user.Surname = "Surname";
                    //user.GivenName = "GivenName";
                    //user.MiddleName = "MiddleName";
                    //user.DisplayName = "DisplayName";
                    //user.EmailAddress = "EmailAddress";
                    //user.Save();
                }
            }
        }

        private static void readAllAdUser()
        {
            using (var context = new PrincipalContext(ContextType.Domain, "sitel.cz"))
            {
                using (var searcher = new PrincipalSearcher(new UserPrincipal(context)))
                {
                    foreach (var result in searcher.FindAll())
                    {
                        DirectoryEntry de = result.GetUnderlyingObject() as DirectoryEntry;
                        Console.WriteLine("First Name         : " + de.Properties["givenName"].Value);
                        Console.WriteLine("Last Name          : " + de.Properties["sn"].Value);
                        Console.WriteLine("SAM account name   : " + de.Properties["samAccountName"].Value);
                        Console.WriteLine("User principal name: " + de.Properties["userPrincipalName"].Value);
                        Console.WriteLine("homePhone          : " + de.Properties["homePhone"].Value);
                        Console.WriteLine("otherHomePhone     : " + de.Properties["otherHomePhone"].Value);
                        Console.WriteLine("ipPhone            : " + de.Properties["ipPhone"].Value);
                        Console.WriteLine("otherIpPhone       : " + de.Properties["otherIpPhone"].Value);
                        Console.WriteLine("Mobile             : " + de.Properties["Mobile"].Value);
                        Console.WriteLine("otherMobile        : " + de.Properties["otherMobile"].Value);
                        Console.WriteLine("TelephoneNumber    : " + de.Properties["TelephoneNumber"].Value);
                        Console.WriteLine("otherTelephone     : " + de.Properties["otherTelephone"].Value);
                    }
                }
            }
            Console.ReadLine();

        }*/

    }
}
