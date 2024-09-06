using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO.Enumeration;
using System.Net.Http.Headers;

namespace MinecraftThroughTime
{
    internal class Program
    {
        static void Main(string[] args)
        {

            //update -s (server)/-c (client), [-f <profileFile>] 
            //make -f <version_manifestv2.json> -o <outputFolder> -t [old_alpha,old_beta,snapshot,release] [-s (only versions with server)] -i <interval>(how many days till next version)

            if(args.Length == 0)
            {
                sfc(ConsoleColor.Red); sbc(ConsoleColor.Black);
                System.Console.WriteLine("No arguments given. Use 'update' or 'make'");
                sfc(ConsoleColor.Yellow); sbc(ConsoleColor.Black);
                System.Console.WriteLine("update server/client [-f <profileFile/url>] [-j <serverJarPath>] -i");
                sfc(ConsoleColor.White); sbc(ConsoleColor.Black);
                System.Console.WriteLine("       -f uses profile from path, tries relative profile.json if not provided. Can also use url to download profile");
                System.Console.WriteLine("       -j uses server jar from path, tries relative server.jar if not provided");
                System.Console.WriteLine("       -v force version");
                sfc(ConsoleColor.Yellow); sbc(ConsoleColor.Black);
                System.Console.WriteLine("make [-f <version_manifestv2.json>] [-o <outputFile>] -t [old_alpha,old_beta,snapshot,release] [-s (only versions with server)] -i <interval>");
                sfc(ConsoleColor.White); sbc(ConsoleColor.Black);
                System.Console.WriteLine("       -f uses version_manifestv2.json from path");
                System.Console.WriteLine("          if not given, tries %appdata%\\.minecraft\\versions\\version_manifest_v2.json");
                System.Console.WriteLine("       -o output file");
                System.Console.WriteLine("          fallsback to relative profile.json");
                System.Console.WriteLine("       -t types of versions to use, comma seperated");
                System.Console.WriteLine("       -s only versions with server version avaliable");
                System.Console.WriteLine("       -i interval in days between version changes, use -1 to only increment manually");
                System.Console.WriteLine("       -u unoffical, allow server jar´s i found on the internet MIGHT BE INSECURE");
                sfc(ConsoleColor.Yellow); sbc(ConsoleColor.Black);
                System.Console.WriteLine("cache clean/open/list");
                sfc(ConsoleColor.White); sbc(ConsoleColor.Black);
                System.Console.WriteLine("       clean deletes cache");
                System.Console.WriteLine("       open opens cache directory");
                System.Console.WriteLine("       list lists cache directory");
                sfc(ConsoleColor.Yellow); sbc(ConsoleColor.Black);
                System.Console.WriteLine("bake <url/path>");
                sfc(ConsoleColor.White); sbc(ConsoleColor.Black);
                System.Console.WriteLine("       Bakes a profile path or url into the executable");
                System.Console.WriteLine("       Usefull for portable applications");
                System.Console.WriteLine("       Currently only path to a profile not full profile.");
                System.Console.WriteLine("");
                sfc(ConsoleColor.Blue); sbc(ConsoleColor.Black);
                System.Console.WriteLine("Example: make -f version_manifestv2.json -o output -t old_alpha,old_beta,snapshot,release -s -i 7");
                System.Console.WriteLine("Example: update server");
                System.Console.WriteLine("Example: update client -f profile.json");
                System.Console.WriteLine("Example: update client -v 1.17");
                System.Console.WriteLine("Example: cache");
                sfc(ConsoleColor.White); sbc(ConsoleColor.Black);
                //End Application with code 1
                System.Environment.Exit(1);
            }

            if(args[0] == "update")
            {
                Update(args);
            }
            else if(args[0] == "make")
            {
                Make(args);
            }
            else if(args[0] == "cache")
            {
                Cache(args);
            }else if(args[0] == "bake")
            {
                Bake(args);
            }
            System.Console.WriteLine("Invalid arguments. Use 'update' or 'make'");
            System.Environment.Exit(1);
        }

        /*
         Helpers
        */
        static void sfc (System.ConsoleColor color)   { Console.ForegroundColor = color; }
        static void sbc(System.ConsoleColor color) { Console.BackgroundColor = color; }

        /// <summary>
        /// Makes a new profile
        /// </summary>
        /// <param name="args">CMD Args</param>
        static void Make(string[] args)
        {
            if(args.Length == 1)
            {
                sfc(ConsoleColor.Red); sbc(ConsoleColor.Black);
                System.Console.WriteLine("No arguments given. Use 'make -f <version_manifestv2.json> -o <outputFolder> -t [old_alpha,old_beta,snapshot,release] -i <interval>'");
                sfc(ConsoleColor.White); sbc(ConsoleColor.Black);
                System.Environment.Exit(1);
            }
            string manifest = "";
            manifest = bakedfile();
            string output = "";
            string types = "";
            bool server = false;
            bool unof = false;
            int interval = 0;
            //Parse arguments
            for(int i = 1; i < args.Length; i++)
            {
                if(args[i] == "-f")
                {
                    manifest = args[i + 1];
                }
                else if(args[i] == "-o")
                {
                    output = args[i + 1];
                }
                else if(args[i] == "-t")
                {
                    types = args[i + 1];
                }
                else if(args[i] == "-s")
                {
                    server = true;
                }
                else if(args[i] == "-i")
                {
                    interval = int.Parse(args[i + 1]);
                }
                else if(args[i] == "-u")
                {
                    unof = true;
                }
            }

            //manifest fallback
            if(manifest == "")
            {
                manifest = System.IO.Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.ApplicationData), ".minecraft","versions","version_manifest_v2.json");
            }

            //output fallback
            if(output == "")
            {
                output = "profile.json";
            }

            //Test for existence of manifest
            if(!System.IO.File.Exists(manifest))
            {
                sfc(ConsoleColor.Red); sbc(ConsoleColor.Black);
                System.Console.WriteLine("Version manifest not found");
                sfc(ConsoleColor.White); sbc(ConsoleColor.Black);
                System.Environment.Exit(1);
            }
            //Test for valid arguments
            if(output == "" || types == "" || interval == 0)
            {
                sfc(ConsoleColor.Red); sbc(ConsoleColor.Black);
                System.Console.WriteLine("Invalid arguments. Use 'make [-f <version_manifestv2.json>] -o <outputFile> -t <old_alpha,old_beta,snapshot,release> -i <interval>'");
                sfc(ConsoleColor.White); sbc(ConsoleColor.Black);
                System.Environment.Exit(1);
            }
            MinecraftThroughTime.Make.MakeProfile(manifest, output, types, server, interval, unof);
            System.Environment.Exit(0);
        }

        /// <summary>
        /// Updates the client or server according to the arguments and profile
        /// </summary>
        /// <param name="args">CMD Args</param>
        static void Update(string[] args)
        {
            string baked = bakedfile();
            if(baked != "")
            {
                sfc(ConsoleColor.Green); sbc(ConsoleColor.Black);
                System.Console.WriteLine("Baked profile found, using it");
                sfc(ConsoleColor.White); sbc(ConsoleColor.Black);
                System.Console.WriteLine("Profile: " + baked);
            }

            if(args.Length == 1)
            {
                sfc(ConsoleColor.Red); sbc(ConsoleColor.Black);
                System.Console.WriteLine("No arguments given. Use 'update server' or 'update client'");
                sfc(ConsoleColor.White); sbc(ConsoleColor.Black);
                System.Environment.Exit(1);
            }

            //Get profile and server jar path
            bool increment = false;
            string profile = baked;
            string serverJar = "";
            string version = "";
            //Parse arguments
            for(int i = 2; i < args.Length; i++)
            {
                if(args[i] == "-f")
                {
                    profile = args[i + 1];
                }
                else if(args[i] == "-j")
                {
                    serverJar = args[i + 1];
                }
                else if(args[i] == "-i")
                {
                    increment = true;
                }
                else if(args[i] == "-v")
                {
                    version = args[i + 1];
                }
            }

            CDL cDL = new CDL();
            //if file url use cDL DownloadFresh
            if(!System.IO.File.Exists(profile) && cDL.ExistsRemote(profile))
            {
                profile = cDL.DownloadFresh(profile);
            }

            //Test for existence of profile and server jar
            if(profile == "")
                profile = "profile.json";

           

            //Test for existence of profile
            if(!System.IO.File.Exists(profile) && !cDL.ExistsRemote(profile))
            {
                sfc(ConsoleColor.Red); sbc(ConsoleColor.Black);
                System.Console.WriteLine("Profile file not found");
                sfc(ConsoleColor.White); sbc(ConsoleColor.Black);
                System.Environment.Exit(1);
            }

           
            //Update Server
            if(args[1] == "server")
            {
                System.Console.WriteLine("Updating server");
                if(serverJar == "")
                    serverJar = "server.jar";

                /*if(!System.IO.File.Exists(serverJar))
                {
                    System.Console.WriteLine("Server jar not found");
                    Console.Write("Ignore? (y/n): ");
                    string input = Console.ReadLine();
                    if(input.ToLower() != "y")
                        System.Environment.Exit(1);
                }*/ //Just allow, userinteraction is shit

                //Calculate version if not given
                if(version == "")
                    version = MinecraftThroughTime.Update.GetExpectedServerVersion(profile, increment, serverJar);

                MinecraftThroughTime.Update.UpdateServer(version, serverJar, profile);
            }
            //Update Client
            else if(args[1] == "client")
            {
                //Calculate version if not given
                if(version == "")
                    version = MinecraftThroughTime.Update.GetExpectedVersion(profile, increment);
                System.Console.WriteLine("Updating client");
                MinecraftThroughTime.Update.UpdateClient(version);
            }
            else
            {
                sfc(ConsoleColor.Red);
                System.Console.WriteLine("Invalid argument. Use 'update client' or 'update server'");
                sfc(ConsoleColor.Black);
                System.Environment.Exit(1);
            }
            System.Environment.Exit(0);
        }

        /// <summary>
        /// Cache Management
        /// </summary>
        /// <param name="args">CMD Args</param>
        static void Cache(string[] args)
        {
            MinecraftThroughTime.CDL cDL = new MinecraftThroughTime.CDL();
            System.Console.WriteLine("Cache directory: " + cDL.GetCacheDir());

            //args 1 clean, open, list
            if(args.Length == 1)
            {
                sfc(ConsoleColor.Red); sbc(ConsoleColor.Black);
                System.Console.WriteLine("No arguments given. Use 'cache clean', 'cache open' or 'cache list'");
                sfc(ConsoleColor.White); sbc(ConsoleColor.Black);
                System.Environment.Exit(1);
            }

            //clean cache, delete all files
            if(args[1] == "clean")
            {
                System.Console.WriteLine("Cleaning cache");
                System.IO.Directory.Delete(cDL.GetCacheDir(), true);
                System.Environment.Exit(0);
            }
            //open cache directory in explorer
            else if(args[1] == "open")
            {
                System.Diagnostics.Process.Start("explorer.exe", cDL.GetCacheDir());
                System.Environment.Exit(0);
            }
            //list cache directory
            else if(args[1] == "list")
            {
                System.Console.WriteLine("Listing cache");
                //files and cummulativ size
                string[] files = System.IO.Directory.GetFiles(cDL.GetCacheDir(), "*", System.IO.SearchOption.AllDirectories);
                long size = 0;
                foreach(string file in files)
                {
                    System.Console.WriteLine(file);
                    size += new System.IO.FileInfo(file).Length;
                }
                sfc(ConsoleColor.Green); sbc(ConsoleColor.Black);
                System.Console.WriteLine("Total size: " + size + " bytes");
                sfc(ConsoleColor.White); sbc(ConsoleColor.Black);
                System.Environment.Exit(0);
            }

            System.Environment.Exit(0);
        }

        static void Bake(string[] args)
        {
            if(args.Length == 1)
            {
                sfc(ConsoleColor.Red); sbc(ConsoleColor.Black);
                System.Console.WriteLine("No arguments given. Use 'bake <url/path>'");
                sfc(ConsoleColor.White); sbc(ConsoleColor.Black);
                System.Environment.Exit(1);
            }

            CDL cDL = new CDL();

            string path = args[1];
            if(System.IO.File.Exists(path) || (System.Uri.IsWellFormedUriString(path, UriKind.Absolute) && cDL.ExistsRemote(path)))
            {
                bool r = MinecraftThroughTime.Bake.BakeProfile(path, false);
                if(r)
                {
                    sfc(ConsoleColor.Green); sbc(ConsoleColor.Black);
                    System.Console.WriteLine("Profile baked");
                    sfc(ConsoleColor.White); sbc(ConsoleColor.Black);
                    System.Environment.Exit(0);
                }
                else
                {
                    sfc(ConsoleColor.Red); sbc(ConsoleColor.Black);
                    System.Console.WriteLine("Failed to bake profile");
                    sfc(ConsoleColor.White); sbc(ConsoleColor.Black);
                    System.Environment.Exit(1);
                }
            }
            else
            {
                sfc(ConsoleColor.Red); sbc(ConsoleColor.Black);
                System.Console.WriteLine("File not found");
                sfc(ConsoleColor.White); sbc(ConsoleColor.Black);
                System.Environment.Exit(1);
            }
            System.Environment.Exit(0);
        }

        /// <summary>
        /// Load baked file
        /// </summary>
        /// <returns>empty if not found, path if found</returns>
        static string bakedFile = "";
        static string bakedfile()
        {
            if(bakedFile != "")
                return bakedFile;

            string path = Environment.ProcessPath;

            FileStream fs = File.OpenRead(path);
            fs.Seek(0, SeekOrigin.End);
            //read backwards till [MTT], example [MTT]C:\Users\user\profile.json. This is the baked file(path)
            //path max 512 + 5, stop if not found in 517 bytes
            byte[] buffer = new byte[0];
            int read = 0;
            string str = "";
            for(int i = 0; i < 517; i++)
            {
                //read until [ or max
                fs.Seek(-i, SeekOrigin.End);
                if(fs.ReadByte() == 91)
                {
                    if(fs.ReadByte() == 77 && fs.ReadByte() == 84 && fs.ReadByte() == 84 && fs.ReadByte() == 93)
                    {
                        //found [MTT],set buffer size to length without [MTT]
                        buffer = new byte[i - 5];
                        fs.Seek(-i + 5, SeekOrigin.End);
                        read = fs.Read(buffer, 0, i - 5);
                        str = System.Text.Encoding.UTF8.GetString(buffer, 0, read);
                        fs.Close();
                        bakedFile = str;
                        return str;
                    }
                }
            }
            Console.WriteLine(str);


            return "";
        }
    }
}
