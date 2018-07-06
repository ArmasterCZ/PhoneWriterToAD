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
        private static string strErrorLog = "";
        private static string strChangesLog = "";

        static void Main(string[] args)
        {
            int errorCount = 0;
            Console.WriteLine("Start programu na zápis telefonů do AD.");
            
            //load aplication setting from app.config
            string path = "\\\\server\\C$\\Export\\Export.csv";
            //bool noWriteInAd = true;
            try
            {
                //noWriteInAd = Convert.ToBoolean(ConfigurationManager.AppSettings["testRun"]);
                path = ConfigurationManager.AppSettings["path"];
            }
            catch (Exception ex)
            {
                errorCount++;
                errorLog(null, new EventArgsLog { strLog = "(config error) " + ex.Message });
            }

            //load CSV file
            List<AdUser> userListCsv = new List<AdUser>();
            try
            {
                CsvLoader myCsv = new CsvLoader(path);
                myCsv.load();
                myCsv.splitMultinumber();
                //myCsv.showUserList();
                userListCsv = myCsv.getUserList();
                Console.WriteLine($"CSV načteno ({userListCsv.Count()}).");
            }
            catch (Exception ex)
            {
                errorCount++;
                errorLog(null, new EventArgsLog { strLog = "(CSV error) " + ex.Message});
            }

            //load list of AD user 
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

            //split CSV data to right collums
            List<telephoneUser> diferencesList = null;
            try
            {
                NumberRedistribution redistribution = new NumberRedistribution(userListCsv, userListAd);
                //split CSV data 
                redistribution.processCsvToAtrib();
                //merge CSV with all AD users
                redistribution.makeFinalList();
                //log changes potencial changes in getFinalDiferences
                redistribution.callBackEditLog = consoleLog;
                //get only changes
                diferencesList = redistribution.getFinalDiferences();
                Console.WriteLine($"CSV rozřazeno. Změny ({diferencesList.Count()})");
            }
            catch (Exception ex)
            {
                errorCount++;
                errorLog(null, new EventArgsLog { strLog = "(Number transformation error) " + ex.Message });
            }

            //edit user in AD
            if ((errorCount == 0) & (adconn != null) & (diferencesList != null))
            {
                //adconn = new AdConnection(userListCsv, errorLog, consoleLog, noWriteInAd);
                adconn.writeChangesToAD(diferencesList);
                Console.WriteLine("Uživatelé upraveni.");
            }
            else
            {
                errorLog(null, new EventArgsLog { strLog = "Program se ukončil bez úpravy AD!"});
            }


            //send report email
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
            //Console.ReadLine();
        }

        private static void errorLog(object obj, EventArgsLog eventArgsLog)
        {
            Console.WriteLine("Error: " + eventArgsLog.strLog.ToString());
            strErrorLog += Environment.NewLine + eventArgsLog.strLog.ToString() ;
        }

        private static void consoleLog(object obj, EventArgsLog eventArgsLog)
        {
            Console.WriteLine(eventArgsLog.strLog.ToString());
            strChangesLog += Environment.NewLine + eventArgsLog.strLog.ToString();
        }
    }

}
