namespace MinecraftThroughTime
{
    internal class Program
    {

        public static bool ConsoleMode = false;

        public static void Exit(int Code)
        {
            if (!ConsoleMode) Environment.Exit(Code);
        }

        static void Main(string[] args)
        {

            //update -s (server)/-c (client), [-f <profileFile>] 
            //make -f <version_manifestv2.json> -o <outputFolder> -t [old_alpha,old_beta,snapshot,release] [-s (only versions with server)] -i <interval>(how many days till next version)

            if (args.Length == 0)
            {
                Console.WriteLine("Minecraft Through Time");
                Console.WriteLine("Type 'exit' to exit");
                Console.WriteLine("Type 'help' for help");
                ConsoleMode = true;
                while (true)
                {
                    Sbc(ConsoleColor.Black); Sfc(ConsoleColor.Red);
                    Console.Write("> ");
                    Sfc(ConsoleColor.White); Sbc(ConsoleColor.Black);
                    string inputraw = Console.ReadLine() ?? "";
                    string[] input = inputraw.Split(" ");
                    if (input[0] == "exit")
                    {
                        Exit(0);
                        return;
                    }
                    Main(input);
                }
            }

            switch (args[0])
            {
                case "update":
                    Update(args);
                    break;
                case "make":
                    Make(args);
                    break;
                case "cache":
                    Cache(args);
                    break;
                case "bake":
                    Bake(args);
                    break;
                case "help":
                    Help();
                    break;
                default:
                    Console.WriteLine("Invalid arguments. Use 'help' to get a list of commands");
                    break;
            }

            //Its ok, it is only called if not in console mode
            Exit(1);
        }

        /*
         Helpers
        */
        static void Sfc(ConsoleColor color) { Console.ForegroundColor = color; }
        static void Sbc(ConsoleColor color) { Console.BackgroundColor = color; }

        /// <summary>
        /// Help
        /// </summary>
        /// <returns></returns>
        static void Help()
        {
            Sfc(ConsoleColor.Red); Sbc(ConsoleColor.Black);
            Console.WriteLine("No arguments given. Use 'update' or 'make'");
            Sfc(ConsoleColor.Yellow); Sbc(ConsoleColor.Black);
            Console.WriteLine("update server/client [-f <profileFile/url>] [-j <serverJarPath>] -i");
            Sfc(ConsoleColor.White); Sbc(ConsoleColor.Black);
            Console.WriteLine("       -f uses profile from path, tries relative profile.json if not provided. Can also use url to download profile");
            Console.WriteLine("       -j uses server jar from path, tries relative server.jar if not provided");
            Console.WriteLine("       -v force version");
            Console.WriteLine("       -i increment version, if not given, calculates next version based on profile, if given, gets next version in profile");
            Sfc(ConsoleColor.Yellow); Sbc(ConsoleColor.Black);
            Console.WriteLine("make [-f <version_manifestv2.json>] [-o <outputFile>] -t [old_alpha,old_beta,snapshot,release] [-s (only versions with server)] -i <interval>");
            Sfc(ConsoleColor.White); Sbc(ConsoleColor.Black);
            Console.WriteLine("       -f uses version_manifestv2.json from path");
            Console.WriteLine("          if not given, tries %appdata%\\.minecraft\\versions\\version_manifest_v2.json");
            Console.WriteLine("       -o output file");
            Console.WriteLine("          fallsback to relative profile.json");
            Console.WriteLine("       -t types of versions to use, comma seperated");
            Console.WriteLine("       -s only versions with server version avaliable");
            Console.WriteLine("       -i interval in days between version changes, use -1 to only increment manually");
            Console.WriteLine("       -u unoffical, allow server jar´s i found on the internet MIGHT BE INSECURE");
            Sfc(ConsoleColor.Yellow); Sbc(ConsoleColor.Black);
            Console.WriteLine("cache clean/open/list");
            Sfc(ConsoleColor.White); Sbc(ConsoleColor.Black);
            Console.WriteLine("       clean deletes cache");
            Console.WriteLine("       open opens cache directory");
            Console.WriteLine("       list lists cache directory");
            Sfc(ConsoleColor.Yellow); Sbc(ConsoleColor.Black);
            Console.WriteLine("bake <url/path> [full]");
            Sfc(ConsoleColor.White); Sbc(ConsoleColor.Black);
            Console.WriteLine("       Bakes a profile path or url into the executable");
            Console.WriteLine("       Usefull for portable applications");
            Console.WriteLine("       Currently only path to a profile not full profile.");
            Console.WriteLine("       Use 'full' to bake full profile instead of just path");
            Console.WriteLine("");
            Sfc(ConsoleColor.Blue); Sbc(ConsoleColor.Black);
            Console.WriteLine("Example: make -f version_manifestv2.json -o output -t old_alpha,old_beta,snapshot,release -s -i 7");
            Console.WriteLine("Example: update server");
            Console.WriteLine("Example: update client -f profile.json");
            Console.WriteLine("Example: update client -v 1.17");
            Console.WriteLine("Example: cache");
            Sfc(ConsoleColor.White); Sbc(ConsoleColor.Black);
        }

        /// <summary>
        /// Makes a new profile
        /// </summary>
        /// <param name="args">CMD Args</param>
        static void Make(string[] args)
        {
            if (args.Length == 1)
            {
                Sfc(ConsoleColor.Red); Sbc(ConsoleColor.Black);
                Console.WriteLine("No arguments given. Use 'make -f <version_manifestv2.json> -o <outputFolder> -t [old_alpha,old_beta,snapshot,release] -i <interval>'");
                Sfc(ConsoleColor.White); Sbc(ConsoleColor.Black);
                Exit(1);
                return;
            }
            string manifest;
            manifest = Bakedfile();
            string output = "";
            string types = "";
            bool server = false;
            bool unof = false;
            int interval = 0;
            //Parse arguments
            for (int i = 1; i < args.Length; i++)
            {
                switch (args[i])
                {
                    case "-f":
                        manifest = args[i + 1];
                        break;
                    case "-o":
                        output = args[i + 1];
                        break;
                    case "-t":
                        types = args[i + 1];
                        break;
                    case "-s":
                        server = true;
                        break;
                    case "-i":
                        interval = int.Parse(args[i + 1]);
                        break;
                    case "-u":
                        unof = true;
                        break;
                }
            }

            //manifest fallback
            if (manifest == "")
            {
                manifest = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), ".minecraft", "versions", "version_manifest_v2.json");
            }

            //output fallback
            if (output == "") output = "profile.json";

            //Test for existence of manifest
            if (!File.Exists(manifest))
            {
                Sfc(ConsoleColor.Red); Sbc(ConsoleColor.Black);
                Console.WriteLine("Version manifest not found");
                Sfc(ConsoleColor.White); Sbc(ConsoleColor.Black);
                Exit(1);
                return;
            }
            //Test for valid arguments
            if (output == "" || types == "" || interval == 0)
            {
                Sfc(ConsoleColor.Red); Sbc(ConsoleColor.Black);
                Console.WriteLine("Invalid arguments. Use 'make [-f <version_manifestv2.json>] -o <outputFile> -t <old_alpha,old_beta,snapshot,release> -i <interval>'");
                Sfc(ConsoleColor.White); Sbc(ConsoleColor.Black);
                Exit(1);
            }
            MinecraftThroughTime.Make.MakeProfile(manifest, output, types, server, interval, unof);
            Exit(0);
            return;
        }

        /// <summary>
        /// Updates the client or server according to the arguments and profile
        /// </summary>
        /// <param name="args">CMD Args</param>
        static void Update(string[] args)
        {
            string baked = Bakedfile();
            if (baked != "")
            {
                Sfc(ConsoleColor.Green); Sbc(ConsoleColor.Black);
                Console.WriteLine("Baked profile path/url found, using it");
                Sfc(ConsoleColor.White); Sbc(ConsoleColor.Black);
                Console.WriteLine("Profile: " + baked);
            }

            if (args.Length == 1)
            {
                Sfc(ConsoleColor.Red); Sbc(ConsoleColor.Black);
                Console.WriteLine("No arguments given. Use 'update server' or 'update client'");
                Sfc(ConsoleColor.White); Sbc(ConsoleColor.Black);
                Exit(1);
                return;
            }

            //Get profile and server jar path
            bool increment = false;
            string profile = baked;
            string serverJar = "";
            string version = "";
            //Parse arguments
            for (int i = 2; i < args.Length; i++)
            {
                switch (args[i])
                {
                    case "-f":
                        profile = args[i + 1];
                        break;
                    case "-j":
                        serverJar = args[i + 1];
                        break;
                    case "-i":
                        increment = true;
                        break;
                    case "-v":
                        version = args[i + 1];
                        break;
                }
            }

            CDL cDL = new();

            //if file url use cDL DownloadFresh
            if (!File.Exists(profile) && cDL.ExistsRemote(profile))
            {
                profile = cDL.DownloadFresh(profile);
            }

            //Test for existence of profile and server jar
            if (profile == "")
                profile = "profile.json";

            //Test for existence of profile
            if (!File.Exists(profile) && !cDL.ExistsRemote(profile))
            {
                Sfc(ConsoleColor.Red); Sbc(ConsoleColor.Black);
                Console.WriteLine("Profile file not found");
                Sfc(ConsoleColor.White); Sbc(ConsoleColor.Black);
                Exit(1);
                return;
            }

            //Update Server
            if (args[1] == "server")
            {
                Console.WriteLine("Updating server");
                if (serverJar == "")
                    serverJar = "server.jar";

                if (!File.Exists(serverJar) && ConsoleMode)
                {
                    Console.WriteLine("Server jar not found");
                    Console.Write("Ignore? (y/n): ");

                    string Input = Console.ReadLine() ?? "";
                    if (Input.ToLower() != "y")
                    {
                        Sbc(ConsoleColor.Black); Sfc(ConsoleColor.Red);
                        Console.WriteLine("Aborted");
                        Sfc(ConsoleColor.White); Sbc(ConsoleColor.Black);
                        return;
                    }

                } //Just allow, userinteraction is shit // UPDATE: allow if non interactive, but ask in interactive mdoe

                //Calculate version if not given
                if (version == "")
                    version = MinecraftThroughTime.Update.GetExpectedServerVersion(profile, increment, serverJar);

                MinecraftThroughTime.Update.UpdateServer(version, serverJar, profile);
            }
            //Update Client
            else if (args[1] == "client")
            {
                //Calculate version if not given
                if (version == "")
                    version = MinecraftThroughTime.Update.GetExpectedVersion(profile, increment);
                Console.WriteLine("Updating client");
                MinecraftThroughTime.Update.UpdateClient(version);
            }
            else
            {
                Sfc(ConsoleColor.Red);
                Console.WriteLine("Invalid argument. Use 'update client' or 'update server'");
                Sfc(ConsoleColor.Black);
                Exit(1);
                return;
            }
            Exit(0);
            return;
        }

        /// <summary>
        /// Cache Management
        /// </summary>
        /// <param name="args">CMD Args</param>
        static void Cache(string[] args)
        {
            CDL cDL = new();
            Console.WriteLine("Cache directory: " + cDL.GetCacheDir());

            //args 1 clean, open, list
            if (args.Length == 1)
            {
                Sfc(ConsoleColor.Red); Sbc(ConsoleColor.Black);
                Console.WriteLine("No arguments given. Use 'cache clean', 'cache open' or 'cache list'");
                Sfc(ConsoleColor.White); Sbc(ConsoleColor.Black);
                Exit(1);
                return;
            }

            //clean cache, delete all files
            if (args[1] == "clean")
            {
                Console.WriteLine("Cleaning cache");
                Directory.Delete(cDL.GetCacheDir(), true);
                Exit(0);
                return;
            }
            //open cache directory in explorer
            else if (args[1] == "open")
            {
                System.Diagnostics.Process.Start("explorer.exe", cDL.GetCacheDir());
                Exit(0);
                return;
            }
            //list cache directory
            else if (args[1] == "list")
            {
                Console.WriteLine("Listing cache");
                //files and cummulativ size
                string[] files = Directory.GetFiles(cDL.GetCacheDir(), "*", SearchOption.AllDirectories);
                long size = 0;
                foreach (string file in files)
                {
                    Console.WriteLine(file);
                    size += new FileInfo(file).Length;
                }
                Sfc(ConsoleColor.Green); Sbc(ConsoleColor.Black);
                Console.WriteLine("Total size: " + size + " bytes");
                Sfc(ConsoleColor.White); Sbc(ConsoleColor.Black);
                Exit(0);
                return;
            }

            Exit(0);
            return;
        }

        static void Bake(string[] args)
        {
            if (args.Length == 1)
            {
                Sfc(ConsoleColor.Red); Sbc(ConsoleColor.Black);
                Console.WriteLine("No arguments given. Use 'bake <url/path> [full]'");
                Sfc(ConsoleColor.White); Sbc(ConsoleColor.Black);
                Exit(1);
                return;
            }

            CDL cDL = new CDL();

            string path = args[1];
            bool fully = (args.Length >= 3) ? (args[2] == "full") ? true : false : false;
            if (File.Exists(path) || (Uri.IsWellFormedUriString(path, UriKind.Absolute) && cDL.ExistsRemote(path)))
            {
                bool r = (args.Length >= 3) ? MinecraftThroughTime.Bake.BakeFully(path) : MinecraftThroughTime.Bake.BakeProfile(path);
                if (r)
                {
                    Sfc(ConsoleColor.Green); Sbc(ConsoleColor.Black);
                    Console.WriteLine("Profile baked");
                    Sfc(ConsoleColor.White); Sbc(ConsoleColor.Black);
                    Exit(0);
                    return;
                }
                else
                {
                    Sfc(ConsoleColor.Red); Sbc(ConsoleColor.Black);
                    Console.WriteLine("Failed to bake profile");
                    Sfc(ConsoleColor.White); Sbc(ConsoleColor.Black);
                    Exit(1);
                    return;
                }
            }
            else
            {
                Sfc(ConsoleColor.Red); Sbc(ConsoleColor.Black);
                Console.WriteLine("File not found");
                Sfc(ConsoleColor.White); Sbc(ConsoleColor.Black);
                Exit(1);
                return;
            }
        }

        /// <summary>
        /// Load baked file
        /// </summary>
        /// <returns>
        /// empty path if not found, path if found in a BakedProfile
        /// </returns>
        static string bakedFile = "";
        static string Bakedfile()
        {
            //if(bakedFile != "") return bakedFile;
            if (bakedFile != "") return bakedFile;

            string? path = Environment.ProcessPath;
            if (path == null) return "";

            //Prep filestream
            FileStream fs = File.OpenRead(path);
            fs.Seek(0, SeekOrigin.End);

            //if ends with exactly [MTTFBP], then it is a full profil, process with FullyBaked
            byte[] full = new byte[8];
            fs.Seek(-8, SeekOrigin.End);
            _ = fs.Read(full, 0, 8);
            if (System.Text.Encoding.UTF8.GetString(full) == "[MTTFBP]")
            {
                fs.Close();
                return FullyBaked();
            }

            //Reset to end
            fs.Seek(0, SeekOrigin.End);

            //read backwards till [MTT], example [MTT]C:\Users\user\profile.json. This is the baked file(path)
            //path max 512 + 5, stop if not found in 517 bytes
            byte[] buffer;
            int read;
            string str = "";
            for (int i = 0; i < 517; i++)
            {
                //read until [ or max
                fs.Seek(-i, SeekOrigin.End);
                if (fs.ReadByte() == 91)
                {
                    if (fs.ReadByte() == 77 && fs.ReadByte() == 84 && fs.ReadByte() == 84 && fs.ReadByte() == 93)
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

        /// <summary>
        /// Read full profile from baked file
        /// <executable binary>[MTT]<jsonstring>[MTTDL]<int32>[MTTFBP]
        /// </summary>
        /// <returns>
        ///     BakedProfile with FullProfile set to true and Data in form of Raw Json string
        /// </returns>
        static string FullyBaked()
        {
            string? path = Environment.ProcessPath;
            if (path == null) return "";

            //Prep filestream
            FileStream fs = File.OpenRead(path);
            fs.Seek(0, SeekOrigin.End);

            //Reset to end
            fs.Seek(0, SeekOrigin.End);

            //Offset DATALENGTH, 2 bytes for int32, 8 bytes for [MTTFBP]
            int LEN_DATALENGTH = 2;
            int LEN_INT32 = 4;
            int LEN_MTTFBP = 8;
            int endoffset = LEN_DATALENGTH + LEN_INT32 + LEN_MTTFBP;

            //read backwards in between [MTT] and [MTTDL]<int32>[MTTFBP]
            //flexible length, read int32 for buffer size
            byte[] buffer;
            int read;
            byte[] intbuffer = new byte[4];
            fs.Seek(-(LEN_INT32 + LEN_MTTFBP), SeekOrigin.End);
            _ = fs.Read(intbuffer, 0, 4);

            //update buffer size to read length
            int size = BitConverter.ToInt32(intbuffer, 0);

            //get data, offset from end = endoffset + size
            buffer = new byte[size];
            fs.Seek(-endoffset - size, SeekOrigin.End);
            read = fs.Read(buffer, 0, size);
            string data = System.Text.Encoding.UTF8.GetString(buffer, 0, read);
            fs.Close();


            //Make temp file
            string temp = Path.GetTempFileName();
            File.WriteAllText(temp, data);
            return temp;
        }
    }
}
