using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
        public static Launcher_Profiles getProfile()
        {
            string path = System.IO.Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.ApplicationData), ".minecraft", "launcher_profiles.json");

            if (!System.IO.File.Exists(path))
            {
                System.Console.WriteLine("Launcher profiles not found");
                System.Environment.Exit(1);
            }

            string json = System.IO.File.ReadAllText(path);
            Launcher_Profiles profiles = JsonSerializer.Deserialize<Launcher_Profiles>(json);

            if (profiles == null)
            {
                System.Console.WriteLine("Failed to deserialize launcher profiles");
                System.Environment.Exit(1);
            }

            //Find MTT profile
            Launcher_Profiles.Profile profile = null;

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
                profile = new Launcher_Profiles.Profile();
                profile.Created = DateTime.Now;
                profile.Icon = "Lectern_Book";
                profile.JavaArgs = "-Xmx2G";
                profile.LastUsed = DateTime.Now;
                profile.LastVersionId = "1.17";
                profile.Name = "Minecraft Through Time";
                profile.Type = "custom";
                profiles.Profiles.Add("Minecraft Through Time", profile);
            }

            return profiles;
        }

        /// <summary>
        /// Set the version for the MTT profile
        /// </summary>
        /// <param name="version"></param>
        public static void setVersion(string version)
        {
            Launcher_Profiles launcher_Profiles = getProfile();
            //set version for MTT profile
            launcher_Profiles.Profiles["Minecraft Through Time"].LastVersionId = version;
            launcher_Profiles.Profiles["Minecraft Through Time"].LastUsed = DateTime.Now;

            string path = System.IO.Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.ApplicationData), ".minecraft", "launcher_profiles.json");
            string json = JsonSerializer.Serialize(launcher_Profiles);
            System.IO.File.WriteAllText(path, json);
            Console.WriteLine("Version set to " + version);
        }

        /// <summary>
        /// get the version for the MTT profile
        /// </summary>
        /// <returns>version string, if not found, returns 1.17(TBC)</returns>
        public static string getVersion()
        {
            Launcher_Profiles launcher_Profiles = getProfile();
            return launcher_Profiles.Profiles["Minecraft Through Time"].LastVersionId;
        }
    }

    /*
     Stuff below is to parse the json data
    */

    class Launcher_Profiles
    {
        [JsonPropertyName("profiles")]
        public Dictionary<string, Profile> Profiles { get; set; }

        [JsonPropertyName("settings")]
        public Settings GameSettings { get; set; }

        [JsonPropertyName("version")]
        public int Version { get; set; }

        public class Profile
        {
            [JsonPropertyName("created")]
            public DateTime Created { get; set; }

            [JsonPropertyName("icon")]
            public string Icon { get; set; }

            [JsonPropertyName("javaArgs")]
            public string JavaArgs { get; set; }

            [JsonPropertyName("lastUsed")]
            public DateTime LastUsed { get; set; }

            [JsonPropertyName("lastVersionId")]
            public string LastVersionId { get; set; }

            [JsonPropertyName("name")]
            public string Name { get; set; }

            [JsonPropertyName("type")]
            public string Type { get; set; }
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
            public string ProfileSorting { get; set; }

            [JsonPropertyName("showGameLog")]
            public bool ShowGameLog { get; set; }

            [JsonPropertyName("showMenu")]
            public bool ShowMenu { get; set; }

            [JsonPropertyName("soundOn")]
            public bool SoundOn { get; set; }
        }
    }
}
