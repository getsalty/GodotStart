using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Xml.Linq;
using Windows.Media.Protection.PlayReady;

namespace Godot_Start.Services
{
    public class Settings
    {
        //private static readonly string path = System.AppDomain.CurrentDomain.BaseDirectory + "Godot Start.exe.config";
        private static readonly string path = "D:\\GitRepos\\GodotStart\\Godot Start\\.config";
        public static readonly Config config = ReadConfig();


        public static void UpdateTimestamp()
        {
            config.LastUpdated = DateTime.Now;

            var newConfig = JsonSerializer.Serialize(config);
            File.WriteAllText(path, newConfig);
        }

        public static void AddInstalledVersion(string name)
        {
            var downloadList = config.Downloads.ToList();
            downloadList.Add(name);
            config.Downloads = [.. downloadList];

            var newConfig = JsonSerializer.Serialize(config);
            File.WriteAllText(path, newConfig);
        }

        public static void RemoveInstalledVersion(string name)
        {
            var downloadList = config.Downloads.ToList();
            downloadList.Remove(name);
            config.Downloads = [.. downloadList];

            var newConfig = JsonSerializer.Serialize(config);
            File.WriteAllText(path, newConfig);
        }

        public static void UpdatedSelectedVersion(string? name)
        {
            config.SelectedVersion = name;

            var newConfig = JsonSerializer.Serialize(config);
            File.WriteAllText(path, newConfig);
        }

        public static void AddSelectedVersionType(string name)
        {
            var selectedVersionTypes = config.SelectedVersionTypes.ToList();
            selectedVersionTypes.Add(name);
            config.SelectedVersionTypes = [.. selectedVersionTypes];

            var newConfig = JsonSerializer.Serialize(config);
            File.WriteAllText(path, newConfig);
        }

        public static void RemoveSelectedVersionType(string name)
        {
            var selectedVersionTypes = config.SelectedVersionTypes.ToList();
            selectedVersionTypes.Remove(name);
            config.SelectedVersionTypes = [.. selectedVersionTypes];

            var newConfig = JsonSerializer.Serialize(config);
            File.WriteAllText(path, newConfig);
        }

        private static Config ReadConfig()
        {
            if (!File.Exists(path))
            {
                var stream = File.Create(path);
                stream.Close();
            }

            var json = IngestConfigJson();

            var config = JsonSerializer.Deserialize<Config>(json);
            return config ?? new();
        }

        private static string IngestConfigJson()
        {
            var json = File.ReadAllText(path);

            if (json != "")
            {
                return json;
            }

            var newValue = "{}";
            File.WriteAllText(path, newValue);
            return newValue;
        }

        public class Config
        {
            [JsonPropertyName("lastUpdated")]
            public DateTime? LastUpdated { get; set; }

            [JsonPropertyName("downloads")]
            public string[] Downloads { get; set; } = [];

            [JsonPropertyName("selectedVersion")]
            public string? SelectedVersion { get; set; }

            [JsonPropertyName("selectedVersionTypes")]
            public string[] SelectedVersionTypes { get; set; } = ["Stable"];
        }
    }
}


