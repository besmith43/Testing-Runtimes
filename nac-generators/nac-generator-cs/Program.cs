using System;
using System.IO;
using System.Net;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using McMaster.Extensions.CommandLineUtils;

/*
to do list

add network printer functionality
    for macs: bash command = ping "ip-address"; arp -a | grep "\<149.149.140.5\>" | awk '{print $4}'
add menu if no ethernet nics are found for user selection
    do I want to have a cmd line flag to do the menu directly?
 */

namespace Generate_NACException
{
    class Program
    {
        public static int Main(string[] args) => CommandLineApplication.Execute<Program>(args);

        [Option(Description = "Verbose")]
        public bool Verbose { get; }

        [Option(Description = "Version", ShortName = "V")]
        public bool Version { get; }

        // ? means that the variable can be null
        [Option(Template = "--printerip <IP-Address>", Description = "Printer IP Address (Ex. 150.150.120.55)")]
        public string? PrinterIP { get; }

        [Option(Template = "--printermac <MAC-Address>", Description = "Printer MAC Address (Ex: 509b4d2b921f)")]
        public string? PrinterMAC { get; }

        public static string VersionNumber = "2.0";

        private void OnExecute()
        {
            if (Version)
            {
                GetVersion();
            }

            bool verbose = false;

            if (Verbose)
            {
                verbose = true;
            }

            if (PrinterIP != null)
            {
                GenPrinterIP(PrinterIP, verbose);
            }
            else if (PrinterMAC != null)
            {
                GenPrinterMAC(PrinterMAC, verbose);
            }

            // default mode

            string hostname = Environment.MachineName.ToUpper();

            GenerateInfo info = new GenerateInfo(hostname);
            string csvContent = info.StartGenerateInfo();

            if (csvContent.Equals("no ethernet mac addresses found") || csvContent.Equals("non-standard OS"))
            {
                Console.WriteLine(csvContent);
                Process.GetCurrentProcess().Kill();
            }

            SaveContentToFile(csvContent, hostname, verbose);
        }

        // see http://codebuckets.com/2017/10/19/getting-the-root-directory-path-for-net-core-applications/ for original text
        public static string GetApplicationRootDebug()
        {
            return Environment.CurrentDirectory;
            
            /*
            var exePath = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().CodeBase);
            Regex appPathMatcher = new Regex(@"(?<!fil)[A-Za-z]:\\+[\S\s]*?(?=\\+bin)");
            var appRoot = appPathMatcher.Match(exePath).Value;
            return appRoot; */
        }

        public static string GetApplicationRootRelease()
        {
            return Path.GetDirectoryName(Process.GetCurrentProcess().MainModule.FileName);

            //return Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location);
            //return Path.GetDirectoryName(Environment.CurrentDirectory);
/*
            string tempPath = Environment.CurrentDirectory;

            if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows) && !tempPath.Contains("Desktop"))
            {
                return $"{ tempPath }/Desktop";
            }
            else
            {
                return tempPath;
            } */
        }

        public static void SaveContentToFile(string content, string devicename, bool verboseFlag)
        {
            string path = "";
            string FileName = "";

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                #if DEBUG
                    path = GetApplicationRootDebug();
                #else
                    path = GetApplicationRootRelease();
                #endif

                FileName = $"{ path }\\{ GenerateFileName(devicename) }";
            }
            else
            {
                #if DEBUG
                    path = GetApplicationRootDebug();
                #else
                    path = GetApplicationRootRelease();
                #endif

                FileName = $"{ path }/{ GenerateFileName(devicename) }";
            }

            if (!File.Exists(FileName))
            {
                try
                {
                	using(StreamWriter sw = File.CreateText(FileName))
                	{
                    		sw.WriteLine(content);
                	}
		        }
                catch
                {
                    Console.WriteLine($"Couldn't write to path: { FileName }");
                }
            }
            else
            {
                //Console.WriteLine("CSV already exists");
                //Console.WriteLine("Would you like to replace it? (y/n)");
                //string Answer = Console.ReadLine();

                bool Answer = Prompt.GetYesNo($"CSV already exists.{ Environment.NewLine }Would you like to replace it?", true);

                //if (Answer == "y" || Answer == "Y" || Answer.ToLower() == "yes")
                if(Answer)
                {
                    File.Delete(FileName);
                    using (StreamWriter sw = File.CreateText(FileName))
                    {
                        sw.WriteLine(content);
                    }
                }
            }

            if (verboseFlag)
            {
                Console.WriteLine($"Path: { FileName }");
                Console.WriteLine($"CSV Content: { content }");
            }
        }

        public static string GenerateFileName(string host)
        {
            string FormatedDate = $"{ DateTime.Today.ToString("d").Replace("/","") }";

            return $"{ FormatedDate }-{ host }.csv";
        }

        public static void GetVersion()
        {
            Console.WriteLine($"Generate-NACException Version: { VersionNumber }");
            Process.GetCurrentProcess().Kill();
        }

        
        // will only work if the printer is in the same building
        public static void GenPrinterIP(string printerIP, bool verboseFlag)
        {
            bool answer = Prompt.GetYesNo($"This feature will only work if you are in the same building as the printer you are trying to generate a csv for.{ Environment.NewLine }Would you like to continue?", true);

            if (!answer)
            {
                Process.GetCurrentProcess().Kill();
            }

            // first validate the ip address string, then ping, then arp, then generate string

            if (isIP(printerIP))
            {
                Console.WriteLine($"{ printerIP } is a valid ip address");
                // run ping, then arp, then generate string
                // will most likely need to figure out wget to obtain an awk exe
                // see https://www.arclab.com/en/kb/csharp/download-file-from-internet-to-string-or-file.html for more info
            }
            else
            {
                Console.WriteLine("The IP Addres given is not valid.");
                Process.GetCurrentProcess().Kill();
            }

            // need to ping printer with verified ip address

            //var pingProcess = System.Diagnostics.Process.Start("ping", printerIP);
            //pingProcess.WaitForExit();

            // need to cat arp output to a file

            //var arpProcess = System.Diagnostics.Process.Start("arp", $"-a { printerIP } > \"{ Environment.CurrentDirectory }\\arp.txt\"");
            //arpProcess.WaitForExit();

            //string awkArgs = "\"{print $4}\" \"" + Environment.CurrentDirectory + "\\arp.txt\"";

            string awkOutput = "";

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                awkOutput = BundleAwk.runAwkWinPWSH(printerIP, Environment.CurrentDirectory);
            }
            else
            {
                awkOutput = BundleAwk.runAwkUnix(printerIP);
            }
            

            //File.Delete(Environment.CurrentDirectory + "\\arp.txt");

            // cushion mac address given as output from awk

            awkOutput = awkOutput.Replace(Environment.NewLine, "");

            awkOutput = awkOutput.ToUpper();

            awkOutput = awkOutput.Replace("-", ":");
/* 
            // get printer name from user input

            Console.WriteLine("What is the hostname of the Printer?");
            string printerName = Console.ReadLine();

            // get printer room number location from user input

            Console.WriteLine("What is the building and room number? (Ex. HEND001B)");
            string roomNumber = Console.ReadLine();

            GenerateInfo info = new GenerateInfo(printerName);
            string csvContent = info.StartGeneratePrinterInfo(awkOutput, roomNumber);

            SaveContentToFile(csvContent, printerName, verboseFlag);
*/
            FinishPrinter(awkOutput, verboseFlag);

            Process.GetCurrentProcess().Kill();
        }

        public static void GenPrinterMAC(string printerMAC, bool verboseFlag)
        {
            // cushion mac address into proper format

            printerMAC = printerMAC.ToUpper();

            if (printerMAC.Length < 12)
            {
                Console.WriteLine("The MAC Address entered is not long enough.");
                Process.GetCurrentProcess().Kill();
            }
            else if (printerMAC.Length == 12)
            {
                printerMAC = printerMAC.Insert(2, ":");
                printerMAC = printerMAC.Insert(5, ":");
                printerMAC = printerMAC.Insert(8, ":");
                printerMAC = printerMAC.Insert(11, ":");
                printerMAC = printerMAC.Insert(14, ":");
            }
            else
            {
                Console.WriteLine("The MAC Address entered is too long");
            }

            FinishPrinter(printerMAC, verboseFlag);

            Process.GetCurrentProcess().Kill();
        }

        public static void FinishPrinter(string macAddress, bool verboseFlag)
        {
            // get printer name from user input

            Console.WriteLine("What is the hostname of the Printer?");
            string printerName = Console.ReadLine();

            // get printer room number location from user input

            Console.WriteLine("What is the building and room number? (Ex. HEND001B)");
            string roomNumber = Console.ReadLine();

            GenerateInfo info = new GenerateInfo(printerName);
            string csvContent = info.StartGeneratePrinterInfo(macAddress, roomNumber);

            SaveContentToFile(csvContent, printerName, verboseFlag);
        }

        public static bool isIP(string host)
        {
            IPAddress ip;
            return IPAddress.TryParse(host, out ip);
        }
    }
}
