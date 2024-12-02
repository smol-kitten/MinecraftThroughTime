using System.Text.Json;
using System.Text.Json.Serialization;
using System.Security.Cryptography;

namespace MinecraftThroughTime
{
    class Update
    {

        static readonly CDL cDL = new();

        /// <summary>
        /// Gets the version TO RUN as defined by the profile
        /// </summary>
        /// <param name="profile">
        ///     Path to the profile file, or a URL to the profile file
        /// </param>
        /// <param name="increment">
        ///     Ignore the profile and increment the version
        /// </param>
        /// <param name="currentVersion">
        ///     Current version, required if increment is true
        /// </param>
        /// <returns>
        ///     String of the version
        ///     Ex. "1.16.5" or "1.17"
        /// </returns>
        public static string GetExpectedVersion(string profile, bool increment)
        {
            //load profile
            MTTProfile mttProfile = GetProfile(profile);

            if (increment)
            {
                //increment version

                //get current version
                string currentVersion = Launcher.GetVersion();

                //find index of current version
                int index = -1;
                for (int i = 0; i < mttProfile.Entries.Count; i++)
                {
                    if (mttProfile.Entries[i].Version == currentVersion)
                    {
                        index = i;
                        break;
                    }
                }

                //if version not found throw error
                if (index == -1)
                {
                    Console.WriteLine("Version " +currentVersion + " not found in profile");
                    Environment.Exit(1);
                }

                //increment version index by 1 and return version
                return mttProfile.Entries[index + 1].Version;
            }

            //find latest version on or after current date but DateTime.Now < entry.Date
            for (int i = 0; i < mttProfile.Entries.Count; i++)
            {
                if (mttProfile.Entries[i].Date != "")
                    if (DateTime.Parse(mttProfile.Entries[i].Date) > DateTime.Now)
                    {
                        return mttProfile.Entries[i].Version;
                    }
            }

            //if no version found throw error
            Console.WriteLine("No version match found");
            Environment.Exit(1);
            //for compiler 
            return "";
        }

        /// <summary>
        /// Gets the version currently running
        /// </summary>
        /// <param name="profile">string path to profile</param>
        /// <param name="path">string path to server jar</param>
        /// <returns></returns>
        private static string GetCurrentServerVersion(string profile, string path)
        {
            //get server jar
            byte[] data = File.ReadAllBytes(path);

            //get sha1
            SHA1 Sha1 = SHA1.Create();
            byte[] hash = Sha1.ComputeHash(data);
            string sha1String = BitConverter.ToString(hash).Replace("-", "").ToLower();

            //load profile
            MTTProfile mttProfile = GetProfile(profile);

            //find version with matching sha1
            for(int i = 0; i < mttProfile.Entries.Count; i++)
            {
                if(mttProfile.Entries[i].Sha1 == sha1String)
                {
                    return mttProfile.Entries[i].Version;
                }
            }
            Console.WriteLine("No version match found");
            Environment.Exit(1);
            //for compiler
            return "";
        }

        /// <summary>
        /// Gets the expected server version to run
        /// </summary>
        /// <param name="profile">string path to profile</param>
        /// <param name="increment">should increment version</param>
        /// <param name="path">string path to server jar</param>
        /// <returns></returns>
        public static string GetExpectedServerVersion(string profile, bool increment, string path)
        {
            //load profile
            MTTProfile mttProfile = GetProfile(profile);

            if (increment)
            {
                //increment version

                //get current version
                string currentVersion = GetCurrentServerVersion(profile, path);

                //find index of current version
                int index = -1;
                for (int i = 0; i < mttProfile.Entries.Count; i++)
                {
                    if (mttProfile.Entries[i].Version == currentVersion)
                    {
                        index = i;
                        break;
                    }
                }

                //if version not found throw error
                if (index == -1)
                {
                    Console.WriteLine("Version " + currentVersion + " not found in profile");
                    Environment.Exit(1);
                }

                //increment version index by 1 and return version
                return mttProfile.Entries[index + 1].Version;
            }

            //find latest version on or after current date but DateTime.Now < entry.Date
            for(int i = 0; i < mttProfile.Entries.Count; i++)
            {
                if(mttProfile.Entries[i].Date != "")
                    if(DateTime.Parse(mttProfile.Entries[i].Date) > DateTime.Now)
                    {
                        return mttProfile.Entries[i].Version;
                    }
            }

            //if no version found throw error
            Console.WriteLine("No version match found");
            Environment.Exit(1);
            //for compiler
            return "";
        }

        /// <summary>
        /// Gets the profile from a file or URL
        /// </summary>
        /// <param name="profile">
        ///     Path or URL to the profile file
        /// </param>
        /// <returns></returns>
        public static MTTProfile GetProfile(string profile)
        {
            //load profile into vmv2
            //if profile is url load from online
            MTTProfile mttProfile;
            if (profile.StartsWith("http"))
            {
                //download profile
                string profileJson = cDL.DownloadString(profile);
                mttProfile = JsonSerializer.Deserialize<MTTProfile>(profileJson) ?? throw new InvalidOperationException("Failed to deserialize profile JSON.");
            }
            else
            {
                //if profile is file load from file
                string profileJson = File.ReadAllText(profile);
                mttProfile = JsonSerializer.Deserialize<MTTProfile>(profileJson) ?? throw new InvalidOperationException("Failed to deserialize profile JSON.");
            }

            return mttProfile;
        }

        /// <summary>
        /// Gets URL for server jar
        /// </summary>
        /// <param name="version">Version to find</param>
        /// <returns>
        /// URL to server jar
        /// </returns>
        public static string getServerJar(string version, string profile)
        {
            MTTProfile mttProfile = GetProfile(profile);

            //find server jar for version
            for (int i = 0; i < mttProfile.Entries.Count; i++)
            {
                if (mttProfile.Entries[i].Version == version)
                {
                    return mttProfile.Entries[i].ServerJar;
                }
            }

            //if no version found throw error
            Console.WriteLine("No server jar found for version");
            Environment.Exit(1);
            //for compiler 
            return "";
        }

        /// <summary>
        /// Updates the server jar to the specified version
        /// </summary>
        /// <param name="version">version to update to</param>
        /// <param name="serverJar">path to server jar</param>
        public static void UpdateServer(string version, string serverJar, string profile)
        {
            //get server jar
            string serverJarUrl = getServerJar(version, profile);

            //download server jar
            cDL.Download(serverJarUrl, serverJar);

            //log4j fix
            L4JF(version, serverJar);

            Console.WriteLine("Server jar updated to version " + version);
        }

        /// <summary>
        /// Sets the version for the MTT profile
        /// </summary>
        /// <param name="version"></param>
        public static void UpdateClient(string version) => Launcher.SetVersion(version);

        /// <summary>
        /// Log4j fix for server
        /// </summary>
        /// <param name="version">version of server</param>
        /// <param name="serverJar">path to server jar</param>
        private static void L4JF(string version, string serverJar)
        {
            string? path = Path.GetDirectoryName(serverJar) ?? throw new Exception("Invalid path");

            string param = "";

            //if 1.17 use command line fix
            if (version == "1.17")
            {
                param = "-Dlog4j2.formatMsgNoLookups=true";
            }

            //1.12-1.16.5 use log4j fix
            if (version == "1.12" || version == "1.13" || version == "1.14" || version == "1.15" || version == "1.16" || version == "1.16.1" || version == "1.16.2" || version == "1.16.3" || version == "1.16.4" || version == "1.16.5")
            {
                param = " -Dlog4j.configurationFile=log4j2_112-116.xml";
                cDL.Download("https://launcher.mojang.com/v1/objects/02937d122c86ce73319ef9975b58896fc1b491d1/log4j2_112-116.xml", Path.Combine(path, "log4j2_112-116.xml"));
            }

            //1.7-1.11.2 other file
            if (version == "1.7" || version == "1.8" || version == "1.9" || version == "1.10" || version == "1.11" || version == "1.11.1" || version == "1.11.2")
            {
                param = " -Dlog4j.configurationFile=log4j2_17-111.xml";
                cDL.Download("https://launcher.mojang.com/v1/objects/4bb89a97a66f350bc9f73b3ca8509632682aea2e/log4j2_17-111.xml", Path.Combine(path, "log4j2_7-112.xml"));
            }

            //write params to file to be included by server
            File.WriteAllText(Path.Combine(path, "include.txt"), param);
        }

    }

    class MTTProfile
    {
        [JsonPropertyName("version")]
        public int Version { get; set; }
        [JsonPropertyName("startDate")]
        public string StartDate { get; set; }
        [JsonPropertyName("interval")]
        public int Interval { get; set; }
        [JsonPropertyName("entries")]
        public List<MTTProfileEntry> Entries { get; set; }

        public class MTTProfileEntry
        {
            [JsonPropertyName("version")]
            public string Version { get; set; }
            [JsonPropertyName("serverJar")]
            public string ServerJar { get; set; }
            [JsonPropertyName("date")]
            public string Date { get; set; }
            [JsonPropertyName("sha1")]
            public string Sha1 { get; set; }

            public MTTProfileEntry()
            {
                Version = "";
                ServerJar = "";
                Date = "";
                Sha1 = "";
            }
        }

        public MTTProfile()
        {
            Entries = new List<MTTProfileEntry>();
            StartDate = "";
        }

    }

}
