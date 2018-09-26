using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.DirectoryServices;
using System.DirectoryServices.Protocols;
using System.DirectoryServices.AccountManagement;
using System.Configuration;

namespace telefonyDoAD
{
    class Program
    {
        private static string strErrorLog = "";   //text pro uložení výpisu chyb
        private static string strChangesLog = ""; //text pro uložení výpisu provedených operací

        /// <summary>
        /// vstup proramu
        /// </summary>
        static void Main(string[] args)
        {
            int errorCount = 0;
            Console.WriteLine("Start programu na zápis telefonů do AD.");
            
            
            string path = "\\\\server\\C$\\Export\\Export.csv";

            //načte nastavení z app.config
            try
            {
                path = ConfigurationManager.AppSettings["path"];
            }
            catch (Exception ex)
            {
                errorCount++;
                errorLog(null, new EventArgsLog { strLog = "(config error) " + ex.Message });
            }

            //načte CSV soubor
            List<AdUser> userListCsv = new List<AdUser>();
            try
            {
                CsvLoader myCsv = new CsvLoader(path);
                myCsv.load();
                myCsv.splitMultinumber();
                userListCsv = myCsv.getUserList();
                Console.WriteLine($"CSV načteno ({userListCsv.Count()}).");
            }
            catch (Exception ex)
            {
                errorCount++;
                errorLog(null, new EventArgsLog { strLog = "(CSV error) " + ex.Message});
            }

            //načte list AD uživatelů 
            List<AdUser> userListAd = new List<AdUser>();
            AdConnection adconn = null;
            try
            {
                adconn = new AdConnection(errorLog, consoleLog);
                userListAd = adconn.userListAd;
                if (userListAd.Count() > 0)
                {
                    Console.WriteLine($"AD user list nahrán ({userListAd.Count()}).");
                }
            }
            catch(Exception ex)
            {
                errorLog(null, new EventArgsLog { strLog = "(ADload error) " + ex.Message });
            }

            //rozdělí CSV data do správných sloupců
            List<telephoneUser> diferencesList = null;
            try
            {
                NumberRedistribution redistribution = new NumberRedistribution(userListCsv, userListAd);
                //rozdělí CSV do atributů 
                redistribution.processCsvToAtrib();
                //sloučí CSV s uživately z AD
                redistribution.makeFinalList();
                //umožní zaznamenat změny v getFinalDiferences
                redistribution.callBackEditLog = consoleLog;
                //vytřídí pouze operace potřebující změnu
                diferencesList = redistribution.getFinalDiferences();
                Console.WriteLine($"CSV rozřazeno. Změny ({diferencesList.Count()})");
            }
            catch (Exception ex)
            {
                errorCount++;
                errorLog(null, new EventArgsLog { strLog = "(Number transformation error) " + ex.Message });
            }

            //zapíše změny uživatelům v AD
            if ((errorCount == 0) & (adconn != null) & (diferencesList != null))
            {
                adconn.writeChangesToAD(diferencesList);
                Console.WriteLine("Uživatelé upraveni.");
            }
            else
            {
                errorLog(null, new EventArgsLog { strLog = "Program se ukončil bez úpravy AD!"});
            }


            //odešle report email nebo error email
            if (!strErrorLog.Equals(""))
            {
                //send error log
                SendEmailError mail = new SendEmailError(strErrorLog, strChangesLog);
            }
            else
            {
                if (!strChangesLog.Equals(""))
                {
                    //send report
                    SendEmailReport mail = new SendEmailReport(strChangesLog);
                }
            }
            System.Threading.Thread.Sleep(5000);
        }

        /// <summary>
        /// zapíše chybovou operaci
        /// </summary>
        /// <param name="eventArgsLog">obsahuje text pro zápis do logu</param>
        private static void errorLog(object obj, EventArgsLog eventArgsLog)
        {
            Console.WriteLine("Error: " + eventArgsLog.strLog.ToString());
            strErrorLog += Environment.NewLine + eventArgsLog.strLog.ToString() ;
        }

        /// <summary>
        /// zobrazí text v konzoli
        /// </summary>
        private static void consoleLog(object obj, EventArgsLog eventArgsLog)
        {
            Console.WriteLine(eventArgsLog.strLog.ToString());
            strChangesLog += Environment.NewLine + eventArgsLog.strLog.ToString();
        }
    }

}
