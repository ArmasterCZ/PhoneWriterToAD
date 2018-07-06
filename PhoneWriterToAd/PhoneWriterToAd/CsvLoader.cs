using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace telefonyDoAD
{
    class CsvLoader
    {
        private List<List<string>> loadedListList = new List<List<string>>();
        private List<AdUser> loadedUserList = new List<AdUser>();
        private string path { get; set; } = "";

        public CsvLoader(string path)
        {
            this.path = path;
        }

        public void load()
        {
            csvRead();
            convertToAdUser();
            
            //throw exception if there is no loaded data
            if (loadedUserList.Count < 1)
            {
                Exception ex = new Exception($"CSV soubor má {loadedUserList.Count} položek. Zkontrolujte cestu {path}");
                throw ex;
            }
        }

        private void csvRead()
        {
            //read CSV file (-first line)

            Encoding encoding = Encoding.GetEncoding(1252);
            //Encoding encoding = Encoding.Default;

            string[] Lines = new string[0];
            try
            {
                Lines = File.ReadAllLines(path, encoding);
            }
            catch (Exception ex)
            {
                ex = new Exception("Nebylo možné načíst soubor. Zkonrolujte zda není otevřený: " + path);
                throw ex;
            }
            

            //prepare number of collums
            int numberCollumn = 7;
            List<List<string>> localLoadedListList = new List<List<string>> { };
            for (int i = 0; i < numberCollumn; i++)
            {
                localLoadedListList.Add(new List<string>());
            }

            //split to collums
            numberCollumn = localLoadedListList.Count();

            //remove fisrt line
            for (int p = 1; p < Lines.Count(); p++)
            {
                string line = Lines[p];
                string[] splitedLine = line.Split(';');
                int splitedLineLength = splitedLine.Count();

                //add to lists
                for (int i = 0; i < numberCollumn; i++)
                {
                    if (i < splitedLineLength)
                    {
                        localLoadedListList.ElementAt(i).Add(splitedLine[i]);
                    }
                    else
                    {
                        Exception exc = new Exception("Error in load CSV file. Line in csv isnt have expected number of items. Loading Closed.");
                        throw exc;
                    }
                }
            }

            loadedListList = localLoadedListList;

        }

        private void convertToAdUser()
        {
            //convert List<List<string>> to List<AdUser>>

            //List<AdUser> localLoadedUserList = new List<AdUser>();
            int listNumber = loadedListList.Count();

            if (listNumber > 6)
            {
                int listLenght = loadedListList[0].Count;
                for (int i = 0; i < listLenght; i++)
                {
                    if (loadedListList[0][i] != "")
                    {
                        AdUser adUserNew = new AdUser();
                        adUserNew.nameAcco = loadedListList[0][i];
                        adUserNew.nameFull = loadedListList[1][i];

                        adUserNew.csvGsm = loadedListList[2][i];
                        adUserNew.csvData = loadedListList[3][i];
                        adUserNew.csvGsmo = loadedListList[4][i];
                        adUserNew.csvFix = loadedListList[5][i];
                        adUserNew.csvFixo = loadedListList[6][i];

                        loadedUserList.Add(adUserNew);
                    }

                }

            }
        }

        private void testMethod()
        {
            List<string> listForShortest = new List<string> { "2561", "300", "886 882 884", "20", "5624" };
            string[] field = getShortestString(listForShortest);
            Console.WriteLine(field[0]);
            Console.WriteLine(field[1]);
            Console.WriteLine(" test trim .. ".Trim());


            Console.ReadLine();
            //result: 20 ..//.. 2561, 886 882 884, 300, 5624
        }

        private string[] getShortestString(List<string> listForShortest)
        {
            //on position 0 return shortest string
            //on 1 position return merge of "listForShortest" and other strings (-shortest) separated by ','
            //on 2 position return all strings that have been moved

            string[] finalField = new string[3];

            string shortestString = "";
            string otherStrings = "";
            string movedStrings = "";

            foreach (string listItem in listForShortest)
            {

                if ((listItem.Length < shortestString.Length) | (shortestString.Equals("")))
                {
                    //item is shortest
                    if (!(shortestString.Equals("")))
                    {
                        //put last shortest string in otherStrings
                        if (otherStrings.Equals(""))
                        {
                            otherStrings += shortestString;
                            movedStrings += shortestString;
                        }
                        else
                        {
                            otherStrings += ", " + shortestString;
                            movedStrings += ", " + shortestString;
                        }
                    }
                    shortestString = listItem;

                }
                else
                {
                    //item is longer add to otherStrings
                    if (otherStrings.Equals(""))
                    {
                        otherStrings += listItem;
                        movedStrings += listItem;
                    }
                    else
                    {
                        otherStrings += ", " + listItem;
                        movedStrings += ", " + listItem;
                    }
                }

            }

            finalField[0] = shortestString;
            finalField[1] = otherStrings;
            finalField[2] = movedStrings;

            return finalField;
        }

        public void showUserList()
        {
            // show list of users "name /GSM / GSMo / FIX / FIXo / DATA"
            foreach (AdUser itemUser in loadedUserList)
            {
                //Console.WriteLine($"{itemUser.nameAcco} / {itemUser.csvGsm} / {itemUser.csvGsmo} / {itemUser.csvFix} / {itemUser.csvFixo} / {itemUser.csvData} / log: {itemUser.telephoneLog} ");
                Console.WriteLine($"{itemUser.nameAcco} / {itemUser.csvGsm} / {itemUser.csvGsmo} / {itemUser.csvFix} / {itemUser.csvFixo} / {itemUser.csvData} ");
                if (!itemUser.telephoneLog.Equals(""))
                {
                    Console.WriteLine($" - log: {itemUser.telephoneLog} ");
                }

            }
            Console.ReadLine();
        }

        public void splitMultinumber()
        {
            //(1) transfer numbers from (GSM to GSMo) and (Fix to Fixo) if they have multiple numbers (in loadedUserList)
            foreach (AdUser itemUser in loadedUserList)
            {
                string gsm = itemUser.csvGsm;
                string fix = itemUser.csvFix;
                string gsmo = itemUser.csvGsmo.Trim();
                string fixo = itemUser.csvFixo.Trim();

                if (gsm.Contains(","))
                {
                    List<string> splittedGsm = (gsm.Split(',')).ToList();
                    string[] splittedGsmCatche = getShortestString(splittedGsm);
                    //split mutltinumber GSM
                    itemUser.csvGsm = splittedGsmCatche[0];
                    if (gsmo.Equals(""))
                    {
                        itemUser.csvGsmo = splittedGsmCatche[1];
                    }
                    else
                    {
                        itemUser.csvGsmo = gsmo + ", " + splittedGsmCatche[1];
                    }

                    //log
                    string oldGsmo = gsmo;
                    string newGsmo = itemUser.csvGsmo;
                    if (!oldGsmo.Equals(newGsmo))
                    {
                        itemUser.telephoneLog += $"(1) číslo přesunuto z GSM do GSMo. [{splittedGsmCatche[2]}];";
                        //itemUser.telephoneLog += $"(1) číslo přesunuto z GSM do GSMo. GSMo původní [{oldGsmo}] nové [{newGsmo}];";
                    }

                }

                if (fix.Contains(","))
                {
                    List<string> splittedFix = (fix.Split(',')).ToList();
                    string[] splittedFixCatche = getShortestString(splittedFix);
                    //split mutltinumber Fix
                    itemUser.csvFix = splittedFixCatche[0];
                    if (gsmo.Equals(""))
                    {
                        itemUser.csvFixo = splittedFixCatche[1];
                    }
                    else
                    {
                        itemUser.csvFixo = fixo + ", " + splittedFixCatche[1];
                    }

                    //log
                    string oldFixo = gsmo;
                    string newFixo = itemUser.csvGsmo;
                    if (!oldFixo.Equals(newFixo))
                    {
                        itemUser.telephoneLog += $"(1) číslo přesunuto z FIX do FIXo. [{splittedFixCatche[2]}];";
                    }
                }

            }
        }

        public List<AdUser> getUserList()
        {
            return loadedUserList;
        }
    }
}
