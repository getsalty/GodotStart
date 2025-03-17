using System;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;

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

        public static void AddImportedProject(ProjectData project)
        {
            var importedProjects = config.Projects.ToList();
            importedProjects.Add(project);
            config.Projects = [.. importedProjects];

            var newConfig = JsonSerializer.Serialize(config);
            File.WriteAllText(path, newConfig);
        }

        public static void RemoveImportedProject(ProjectData project)
        {
            var importedProjects = config.Projects.ToList();
            importedProjects.Remove(project);
            config.Projects = [.. importedProjects];

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

        public static void UpdateWindowSizeType(Windows.Foundation.Size size)
        {
            config.WindowSize = new()
            {
                Height = (int)size.Height,
                Width = (int)size.Width
            };

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

            if (!string.IsNullOrWhiteSpace(json))
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

            [JsonPropertyName("projects")]
            public ProjectData[] Projects { get; set; } = [];

            [JsonPropertyName("windowSize")]
            public JsonSize WindowSize { get; set; } = new()
            {
                Height = 700,
                Width = 1200
            };
        }

        public class ProjectData
        {
            [JsonPropertyName("name")]
            public required string Name { get; set; }

            [JsonPropertyName("version")]
            public string? Version { get; set; }

            [JsonPropertyName("type")]
            public string? Type { get; set; }

            [JsonPropertyName("iconUID")]
            public string? IconUID { get; set; }

            [JsonPropertyName("directoryPath")]
            public required string DirectoryPath { get; set; }
        }

        public class JsonSize
        {
            [JsonPropertyName("height")]
            public required int Height { get; set; }

            [JsonPropertyName("width")]
            public required int Width { get; set; }
        }
    }
}


