using MinecraftThroughTime;
using System.Text.Json;
using System.Text.Json.Serialization;
using static MinecraftThroughTime.MTTProfile;
using static MinecraftThroughTime.vmv2;


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
            vmv2 vm = new();
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
                    json = System.IO.File.ReadAllText(manifest);
                }
                catch(Exception)
                {
                    System.Console.WriteLine("Failed to read manifest from file: " + manifest);
                    System.Environment.Exit(1);
                }
            }

            vm = JsonSerializer.Deserialize<vmv2>(json) ?? throw new InvalidOperationException("Failed to deserialize vm JSON.");

            if (vm.versions == null)
            {
                System.Console.WriteLine("No versions found in manifest");
                System.Environment.Exit(1);
            }
            
            //sort by "releaseTime": "2011-07-07T22:00:00+00:00", ASC
            vm.versions.Sort((x, y) => x.releaseTime.CompareTo(y.releaseTime));

            MTTProfile mttProfile = MakeMTTProfile(ref vm, types, server, interval, allowUnofficial);
            json = JsonSerializer.Serialize(mttProfile);
            System.IO.File.WriteAllText(output, json);

            System.Console.WriteLine("Profile created");
            System.Console.WriteLine("Profile saved to " + output);
            System.Console.WriteLine("Profile:");
            System.Console.WriteLine("-   contains " + mttProfile.Entries.Count + " entries");
            System.Console.WriteLine("-   will start on " + mttProfile.StartDate);
            System.Console.WriteLine("-   will increment by " + mttProfile.Interval + " days");
            if(interval > 0)
                System.Console.WriteLine("-   will take " + (mttProfile.Interval * mttProfile.Entries.Count) + " days to reach last version");
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
        public static MTTProfile MakeMTTProfile(ref vmv2 vm, string types, bool server, int interval, bool allowUnofficial)
        {
            MTTProfile profile = new MTTProfile();

            string[] AllowedTypes = types.Split(",");

            profile.StartDate = System.DateTime.Now.ToString("yyyy-MM-dd");
            DateTime Date = System.DateTime.Now;
            profile.Interval = interval;

            MTTProfileEntry met;

            CDL cDL = new CDL();

            if (vm.versions == null)
                return profile;

            foreach(MCVersion version in vm.versions)
            {
                if(AllowedTypes.Contains(version.type))
                {
                    if(interval != -1)
                        Date = Date.AddDays(interval);
                    met = MakeVersionEntry(version, (interval != -1) ? Date.ToString("yyyy-MM-dd") : "");
                    if(server)
                    {
                        if(met.ServerJar == null || met.ServerJar == "") { 
                            if(allowUnofficial)
                            {
                                if(cDL.ExistsRemote("http://176.9.46.26:9000/mtt/Unofficial/" + version.id + "/server.jar"))
                                {
                                    met.ServerJar = "http://176.9.46.26:9000/mtt/Unofficial/" + version.id + "/server.jar";
                                    met.Sha1 = cDL.GetSha1(cDL.DownloadBytes(met.ServerJar));
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
            version_manifest vm = getVM(version.url);

            MTTProfileEntry entry = new MTTProfileEntry();

            entry.Version = version.id;
            entry.Date = Date;

            if(vm.downloads.server != null && vm.downloads.server.url != "")
            {
                entry.ServerJar = vm.downloads.server.url;
                entry.Sha1 = vm.downloads.server.sha1;
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
            string json = "";
            try
            {
                CDL cDL = new CDL();
                json = cDL.DownloadString(url);
                return JsonSerializer.Deserialize<version_manifest>(json) ?? throw new InvalidOperationException("Failed to deserialize VM JSON.");
            }
            catch(Exception)
            {
                System.Environment.Exit(1);
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
            string ext = System.IO.Path.GetExtension(path);
            string newPath = path.Replace(ext, "_baked" + ext);
            if (File.Exists(newPath))
                File.Delete(newPath);
            File.Copy(path, newPath);

            //write the path into the exe
            byte[] data = System.Text.Encoding.UTF8.GetBytes("[MTT]"+urlOrPath);

            //write the url into the exe
            using(System.IO.FileStream fs = new System.IO.FileStream(newPath, System.IO.FileMode.Open, System.IO.FileAccess.Write))
            {
                fs.Seek(0, System.IO.SeekOrigin.End);
                fs.Write(data, 0, data.Length);
            }

            return true;
        }
    }

    public class version_manifest
    {
        [JsonPropertyName("assets")]
        public string assets { get; set; }
        [JsonPropertyName("downloads")]
        public Downloads downloads { get; set; }
        [JsonPropertyName("id")]
        public string id { get; set; }

        public class Downloads
        {
            [JsonPropertyName("client")]
            public Client client { get; set; }
            [JsonPropertyName("server")]
            public Server server { get; set; }

            public class Client
            {
                [JsonPropertyName("sha1")]
                public string sha1 { get; set; }
                [JsonPropertyName("size")]
                public int size { get; set; }
                [JsonPropertyName("url")]
                public string url { get; set; }

                public Client() {
                    sha1 = "";
                    size = 0;
                    url = "";
                }
            }
            public class Server
            {
                [JsonPropertyName("sha1")]
                public string sha1 { get; set; }
                [JsonPropertyName("size")]
                public int size { get; set; }
                [JsonPropertyName("url")]
                public string url { get; set; }

                public Server() {
                    sha1 = "";
                    size = 0;
                    url = "";
                }
            }

            public Downloads()
            {
                client = new Client();
                server = new Server();
            }
        }

        public version_manifest()
        {
            assets = "";
            downloads = new Downloads();
            id = "";
        }
    }

    class vmv2
    {
        [JsonPropertyName("latest")]
        public Latest? latest { get; set; }

        [JsonPropertyName("versions")]
        public List<MCVersion>? versions { get; set; }

        public class MCVersion
        {
            [JsonPropertyName("id")]
            public required string id { get; set; }
            [JsonPropertyName("type")]
            public required string type { get; set; }
            [JsonPropertyName("url")]
            public required string url { get; set; }
            [JsonPropertyName("time")]
            public required string time { get; set; }
            [JsonPropertyName("releaseTime")]
            public required string releaseTime { get; set; }
            [JsonPropertyName("sha1")]
            public required string sha1 { get; set; }
            [JsonPropertyName("complianceLevel")]
            public int complianceLevel { get; set; }
        }

        public class Latest
        {
            [JsonPropertyName("release")]
            public required string release { get; set; }
            [JsonPropertyName("snapshot")]
            public required string snapshot { get; set; }
        }
    }
}
