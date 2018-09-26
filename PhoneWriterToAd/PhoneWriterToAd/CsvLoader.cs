using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace telefonyDoAD
{
    /// <summary>
    /// načte data z CSV do listu uživatelů (AdUser)
    /// </summary>
    class CsvLoader
    {
        private List<List<string>> loadedListList = new List<List<string>>(); //načtená data z CSV
        private List<AdUser> loadedUserList = new List<AdUser>();             //převedená data do uživatelů
        private string path { get; set; } = "";                               //cesta k CSV souboru

        public CsvLoader(string path)
        {
            this.path = path;
        }

        /// <summary>
        /// provede načtení dat
        /// </summary>
        public void load()
        {
            csvRead();
            convertToAdUser();
            checkLoadedData();
        }

        /// <summary>
        /// zobrazí v konzoli list uživatelů (name /GSM / GSMo / FIX / FIXo / DATA)
        /// </summary>
        public void showUserList()
        {
            foreach (AdUser itemUser in loadedUserList)
            {
                Console.WriteLine($"{itemUser.nameAcco} / {itemUser.csvGsm} / {itemUser.csvGsmo} / {itemUser.csvFix} / {itemUser.csvFixo} / {itemUser.csvData} ");
                if (!itemUser.telephoneLog.Equals(""))
                {
                    Console.WriteLine($" - log: {itemUser.telephoneLog} ");
                }
            }
            Console.ReadLine();
        }

        /// <summary>
        /// přesune přebývající čísla z (GSM do GSMo) a (Fix do Fixo) v loadedListList
        /// </summary>
        public void splitMultinumber()
        {
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

        /// <summary>
        /// vrátí načtené uživatele
        /// </summary>
        public List<AdUser> getUserList()
        {
            return loadedUserList;
        }

        /// <summary>
        /// zkontroluje zda existují načtení uživatelé
        /// </summary>
        private void checkLoadedData()
        {
            if (loadedUserList.Count < 1)
            {
                Exception ex = new Exception($"CSV soubor má {loadedUserList.Count} položek. Zkontrolujte cestu {path}");
                throw ex;
            }
        }

        /// <summary>
        /// načte data z CSV bez první řádky
        /// </summary>
        private void csvRead()
        {
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

        /// <summary>
        /// Přetvoří data do uživatelů (List<List<string>> do List<AdUser>>)
        /// </summary>
        private void convertToAdUser()
        {
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

        /// <summary>
        /// vrátí rozdělená čísla
        /// </summary>
        /// <param name="listForShortest">řetězec na rozdělení</param>
        /// <returns>
        /// na pozici 0 vrátí nejkratší stríng
        /// na pozici 1 vrátí vstup bez nejkratšího čísla oddělený ','
        /// na pozici 2 vrátí všechny části které byli přesunuty
        /// </returns>
        private string[] getShortestString(List<string> listForShortest)
        {
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

    }
}
