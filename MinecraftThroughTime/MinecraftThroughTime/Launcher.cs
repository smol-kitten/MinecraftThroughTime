using System.Text.Json;
using System.Text.Json.Serialization;

namespace MinecraftThroughTime
{
    class Launcher
    {
        /// <summary>
        /// load the launcher profiles
        /// </summary>
        /// <returns>c# object with the launcher profiles</returns>
        public static Launcher_Profiles GetProfile()
        {
            string path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), ".minecraft", "launcher_profiles.json");

            if (!File.Exists(path))
            {
                Console.WriteLine("Launcher profiles not found");
                Program.Exit(1);
            }

            string json = File.ReadAllText(path);
            Launcher_Profiles profiles = JsonSerializer.Deserialize<Launcher_Profiles>(json) ?? throw new Exception("Failed to deserialize launcher profiles");

            //Find MTT profile
            Launcher_Profiles.Profile? profile = null;

            foreach (var p in profiles.Profiles)
            {
                if (p.Value.Name == "Minecraft Through Time")
                {
                    profile = p.Value;
                    break;
                }
            }

            //if MTT profile not found, create it
            if(profile == null)
            {
                //add MTT profile
                profile = new Launcher_Profiles.Profile() { Created = DateTime.Now, Icon = "Lectern_Book", JavaArgs = "-Xmx2G", LastUsed = DateTime.Now, LastVersionId = "1.17", Name = "Minecraft Through Time", Type = "custom" };
                profiles.Profiles.Add("Minecraft Through Time", profile);
            }

            return profiles;
        }

        /// <summary>
        /// Set the version for the MTT profile
        /// </summary>
        /// <param name="version"></param>
        public static void SetVersion(string version)
        {
            Launcher_Profiles launcher_Profiles = GetProfile();
            //set version for MTT profile
            launcher_Profiles.Profiles["Minecraft Through Time"].LastVersionId = version;
            launcher_Profiles.Profiles["Minecraft Through Time"].LastUsed = DateTime.Now;

            string path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), ".minecraft", "launcher_profiles.json");
            string json = JsonSerializer.Serialize(launcher_Profiles);
            File.WriteAllText(path, json);
            Console.WriteLine("Version set to " + version);
        }

        /// <summary>
        /// get the version for the MTT profile
        /// </summary>
        /// <returns>version string, if not found, returns 1.17(TBC)</returns>
        public static string GetVersion()
        {
            Launcher_Profiles launcher_Profiles = GetProfile();
            return launcher_Profiles.Profiles["Minecraft Through Time"].LastVersionId;
        }
    }

    /*
     Stuff below is to parse the json data
    */

    class Launcher_Profiles
    {
        [JsonPropertyName("profiles")]
        public required Dictionary<string, Profile> Profiles { get; set; }

        [JsonPropertyName("settings")]
        public Settings GameSettings { get; set; } = new Settings() { KeepLauncherOpen = false, ProfileSorting = "ByLastPlayed" };

        [JsonPropertyName("version")]
        public int Version { get; set; }

        public class Profile
        {
            [JsonPropertyName("created")]
            public DateTime Created { get; set; }

            [JsonPropertyName("icon")]
            public string Icon { get; set; } = "Lectern_Book";

            [JsonPropertyName("javaArgs")]
            public string JavaArgs { get; set; } = "";

            [JsonPropertyName("lastUsed")]
            public DateTime LastUsed { get; set; }

            [JsonPropertyName("lastVersionId")]
            public string LastVersionId { get; set; } = "1.17";

            [JsonPropertyName("name")]
            public string Name { get; set; } = "Minecraft Through Time";

            [JsonPropertyName("type")]
            public string Type { get; set; } = "custom";
        }

        public class Settings
        {
            [JsonPropertyName("crashAssistance")]
            public bool CrashAssistance { get; set; }

            [JsonPropertyName("enableAdvanced")]
            public bool EnableAdvanced { get; set; }

            [JsonPropertyName("enableAnalytics")]
            public bool EnableAnalytics { get; set; }

            [JsonPropertyName("enableHistorical")]
            public bool EnableHistorical { get; set; }

            [JsonPropertyName("enableReleases")]
            public bool EnableReleases { get; set; }

            [JsonPropertyName("enableSnapshots")]
            public bool EnableSnapshots { get; set; }

            [JsonPropertyName("keepLauncherOpen")]
            public bool KeepLauncherOpen { get; set; }

            [JsonPropertyName("profileSorting")]
            public string ProfileSorting { get; set; } = "ByLastPlayed";

            [JsonPropertyName("showGameLog")]
            public bool ShowGameLog { get; set; }

            [JsonPropertyName("showMenu")]
            public bool ShowMenu { get; set; }

            [JsonPropertyName("soundOn")]
            public bool SoundOn { get; set; }
        }
    }
}
