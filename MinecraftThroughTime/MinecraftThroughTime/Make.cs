using System.Text.Json;
using System.Text.Json.Serialization;
using static MinecraftThroughTime.MTTProfile;
using static MinecraftThroughTime.Vmv2;


namespace MinecraftThroughTime
{
    class Make
    {
        /// <summary>
        /// Create a profile from a version manifest
        /// </summary>
        /// <param name="manifest">Path or URL to the version manifest</param>
        /// <param name="output">Where to save the profile</param>
        /// <param name="types">Comma separated list of version types to include</param>
        /// <param name="server">only include versions with server jars</param>
        /// <param name="interval">interval between versions in days, -1 for no interval</param>
        /// <param name="allowUnofficial">allow unofficial server jars</param>
        public static void MakeProfile(string manifest, string output, string types, bool server, int interval, bool allowUnofficial = false)
        {
            Vmv2 vm = new();
            CDL cDL = new();

            string json = "";

            //load the manifest
            if(manifest.StartsWith("http"))
            {
                json = cDL.DownloadString(manifest);
            }
            else
            {
                try
                {
                    json = File.ReadAllText(manifest);
                }
                catch(Exception)
                {
                    Console.WriteLine("Failed to read manifest from file: " + manifest);
                    Environment.Exit(1);
                }
            }

            vm = JsonSerializer.Deserialize<Vmv2>(json) ?? throw new InvalidOperationException("Failed to deserialize vm JSON.");

            if (vm.Versions == null)
            {
                Console.WriteLine("No versions found in manifest");
                Environment.Exit(1);
            }
            
            //sort by "releaseTime": "2011-07-07T22:00:00+00:00", ASC
            vm.Versions.Sort((x, y) => x.ReleaseTime.CompareTo(y.ReleaseTime));

            MTTProfile mttProfile = MakeMTTProfile(ref vm, types, server, interval, allowUnofficial);
            json = JsonSerializer.Serialize(mttProfile);
            File.WriteAllText(output, json);

            Console.WriteLine("Profile created");
            Console.WriteLine("Profile saved to " + output);
            Console.WriteLine("Profile:");
            Console.WriteLine("-   contains " + mttProfile.Entries.Count + " entries");
            Console.WriteLine("-   will start on " + mttProfile.StartDate);
            Console.WriteLine("-   will increment by " + mttProfile.Interval + " days");
            if(interval > 0)
                Console.WriteLine("-   will take " + (mttProfile.Interval * mttProfile.Entries.Count) + " days to reach last version");
        }

        /// <summary>
        /// Make a profile from a version manifest
        /// </summary>
        /// <param name="vm">Version manifest</param>
        /// <param name="types">which types to include</param>
        /// <param name="server">only include versions with server jars</param>
        /// <param name="interval">interval between versions in days, -1 for no interval</param>
        /// <param name="allowUnofficial">allow unofficial server jars</param>
        /// <returns></returns>
        public static MTTProfile MakeMTTProfile(ref Vmv2 vm, string types, bool server, int interval, bool allowUnofficial)
        {
            MTTProfile profile = new();

            string[] AllowedTypes = types.Split(",");

            profile.StartDate = DateTime.Now.ToString("yyyy-MM-dd");
            DateTime Date = DateTime.Now;
            profile.Interval = interval;

            MTTProfileEntry met;

            CDL cDL = new();

            if (vm.Versions == null)
                return profile;

            foreach(MCVersion version in vm.Versions)
            {
                if(AllowedTypes.Contains(version.Type))
                {
                    if(interval != -1)
                        Date = Date.AddDays(interval);
                    met = MakeVersionEntry(version, (interval != -1) ? Date.ToString("yyyy-MM-dd") : "");
                    if(server)
                    {
                        if(met.ServerJar == null || met.ServerJar == "") { 
                            if(allowUnofficial)
                            {
                                if(cDL.ExistsRemote("http://176.9.46.26:9000/mtt/Unofficial/" + version.Id + "/server.jar"))
                                {
                                    met.ServerJar = "http://176.9.46.26:9000/mtt/Unofficial/" + version.Id + "/server.jar";
                                    met.Sha1 = CDL.GetSha1(cDL.DownloadBytes(met.ServerJar));
                                }
                                else
                                    continue;
                            }
                            else
                                continue;
                        }
                    }
                    profile.Entries.Add(met);
                }
            }

            return profile;
        }

        /// <summary>
        /// Create a profile entry from a version
        /// </summary>
        /// <param name="version">MCVersion</param>
        /// <param name="Date">Start date, when the version should be available</param>
        /// <returns>c# object with the profile entry</returns>
        public static MTTProfileEntry MakeVersionEntry(MCVersion version, string Date)
        {
            version_manifest vm = getVM(version.Url);

            MTTProfileEntry entry = new()
            {
                Version = version.Id,
                Date = Date
            };

            if (vm.Downloads.Server != null && vm.Downloads.Server.Url != "")
            {
                entry.ServerJar = vm.Downloads.Server.Url;
                entry.Sha1 = vm.Downloads.Server.Sha1;
            }else
            {
                entry.ServerJar = "";
            }

            return entry;
        }

        /// <summary>
        /// Get a version manifest from a url
        /// </summary>
        /// <param name="url">URL to the version manifest</param>
        /// <returns>verson manifest object</returns>
        public static version_manifest getVM(string url)
        {
            string json;
            try
            {
                CDL cDL = new();
                json = cDL.DownloadString(url);
                return JsonSerializer.Deserialize<version_manifest>(json) ?? throw new InvalidOperationException("Failed to deserialize VM JSON.");
            }
            catch(Exception)
            {
                Environment.Exit(1);
            }

            return new version_manifest();
        }
    }

    public class Bake
    {
        /// <summary>
        /// Bake a profile(path/url) into the exe
        /// </summary>
        /// <param name="urlOrPath">path to bake (max. 512 chars)</param>
        /// <param name="full_profile">Not Implemented, bake full file instead of path</param>
        /// <returns>true/false</returns>
        public static bool BakeProfile(string urlOrPath, bool full_profile)
        {
            //if more than 512 bytes, return false
            if(urlOrPath.Length > 512)
                return false;

            //copy self with "_baked" suffix
            string? path = Environment.ProcessPath;
            if (path == null)
                return false;

            string ext = Path.GetExtension(path);
            string newPath = path.Replace(ext, "_baked" + ext);

            if (File.Exists(newPath))
                File.Delete(newPath);
            File.Copy(path, newPath);

            //write the path into the exe
            byte[] data = System.Text.Encoding.UTF8.GetBytes("[MTT]"+urlOrPath);

            //write the url into the exe
            using FileStream fs = new(newPath, FileMode.Open, FileAccess.Write);
            fs.Seek(0, SeekOrigin.End);
            fs.Write(data, 0, data.Length);

            return true;
        }
    }

    public class version_manifest
    {
        [JsonPropertyName("assets")]
        public string Assets { get; set; }
        [JsonPropertyName("downloads")]
        public FileDownloads Downloads { get; set; }
        [JsonPropertyName("id")]
        public string Id { get; set; }

        public class FileDownloads
        {
            [JsonPropertyName("client")]
            public ClientDownloads Client { get; set; }
            [JsonPropertyName("server")]
            public ServerDownloads Server { get; set; }

            public class ClientDownloads
            {
                [JsonPropertyName("sha1")]
                public string Sha1 { get; set; }
                [JsonPropertyName("size")]
                public int Size { get; set; }
                [JsonPropertyName("url")]
                public string Url { get; set; }

                public ClientDownloads() {
                    Sha1 = "";
                    Size = 0;
                    Url = "";
                }
            }
            public class ServerDownloads
            {
                [JsonPropertyName("sha1")]
                public string Sha1 { get; set; }
                [JsonPropertyName("size")]
                public int Size { get; set; }
                [JsonPropertyName("url")]
                public string Url { get; set; }

                public ServerDownloads() {
                    Sha1 = "";
                    Size = 0;
                    Url = "";
                }
            }

            public FileDownloads()
            {
                Client = new();
                Server = new();
            }
        }

        public version_manifest()
        {
            Assets = "";
            Downloads = new();
            Id = "";
        }
    }

    class Vmv2
    {
        [JsonPropertyName("latest")]
        public LatestVersion? Latest { get; set; }

        [JsonPropertyName("versions")]
        public List<MCVersion>? Versions { get; set; }

        public class MCVersion
        {
            [JsonPropertyName("id")]
            public required string Id { get; set; }
            [JsonPropertyName("type")]
            public required string Type { get; set; }
            [JsonPropertyName("url")]
            public required string Url { get; set; }
            [JsonPropertyName("time")]
            public required string Time { get; set; }
            [JsonPropertyName("releaseTime")]
            public required string ReleaseTime { get; set; }
            [JsonPropertyName("sha1")]
            public required string Sha1 { get; set; }
            [JsonPropertyName("complianceLevel")]
            public int ComplianceLevel { get; set; }
        }

        public class LatestVersion
        {
            [JsonPropertyName("release")]
            public required string Release { get; set; }
            [JsonPropertyName("snapshot")]
            public required string Snapshot { get; set; }
        }
    }
}
