using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using static Godot_Start.Services.Settings;

namespace Godot_Start.Services
{
    public class VersionChecker
    {
        private static readonly string versionURL = "https://api.github.com/repos/godotengine/godot-builds/releases";
        private static readonly string path = "D:\\GitRepos\\GodotStart\\Godot Start\\.versions";
        private static readonly HttpClient client = new();

        public static async Task<List<Version>?> GetVersionsAPI()
        {
            try
            {
                client.DefaultRequestHeaders.TryAddWithoutValidation("Accept", "text/html,application/xhtml+xml,application/xml,application/json");
                client.DefaultRequestHeaders.TryAddWithoutValidation("User-Agent", "Mozilla/5.0 (Windows NT 6.2; WOW64; rv:19.0) Gecko/20100101 Firefox/19.0");

                var response = await client.GetAsync(versionURL);
                if (response.IsSuccessStatusCode)
                {
                    var versions = await response.Content.ReadFromJsonAsync<List<Version>>();

                    if (versions is not null)
                    {
                        versions = [.. versions.OrderByDescending(version => version.CreatedAt)];
                    }

                    VersionList list = new()
                    {
                        Versions = versions
                    };

                    SetVersionConfig(JsonSerializer.Serialize(list));

                    return versions;
                }
            }
            catch (Exception)
            {
                throw;
            }

            return null;
        }

        public static List<Version>? GetVersionsConfig()
        {
            if (!File.Exists(path))
            {
                var stream = File.Create(path);
                stream.Close();
            }

            var json = File.ReadAllText(path);

            if (json == "")
            {
                var newValue = "{}";
                File.WriteAllText(path, newValue);
                json = newValue;
            }

            var config = JsonSerializer.Deserialize<VersionList>(json);

            var result = config?.Versions;
            if (result is not null)
            {
                result = [.. result.OrderByDescending(version => version.CreatedAt)];
            }

            return result;
        }

        public static void SetVersionConfig(string value)
        {
            File.WriteAllText(path, value);
        }
    }

    public class VersionList
    {
        public required List<Version>? Versions { get; set; }
    }

    public class Version
    {
        [JsonPropertyName("url")]
        public required string Url { get; set; }

        [JsonPropertyName("assets_url")]
        public required string AssetsUrl { get; set; }

        [JsonPropertyName("upload_url")]
        public required string UploadUrl { get; set; }

        [JsonPropertyName("html_url")]
        public required string HtmlUrl { get; set; }

        [JsonPropertyName("id")]
        public required long Id { get; set; }

        [JsonPropertyName("node_id")]
        public required string NodeId { get; set; }

        [JsonPropertyName("tag_name")]
        public required string TagName { get; set; }

        [JsonPropertyName("target_commitish")]
        public required string TargetCommitish { get; set; }

        [JsonPropertyName("name")]
        public required string Name { get; set; }

        [JsonPropertyName("draft")]
        public required bool Draft { get; set; }

        [JsonPropertyName("prerelease")]
        public required bool Prerelease { get; set; }

        [JsonPropertyName("created_at")]
        public required DateTime CreatedAt { get; set; }

        [JsonPropertyName("published_at")]
        public required DateTime PublishedAt { get; set; }

        [JsonPropertyName("tarball_url")]
        public required string TarballUrl { get; set; }

        [JsonPropertyName("zipball_url")]
        public required string ZipballUrl { get; set; }

        [JsonPropertyName("body")]
        public required string Body { get; set; }

        [JsonPropertyName("assets")]
        public required Asset[] Assets { get; set; }
    }

    public class Asset
    {
        [JsonPropertyName("url")]
        public required string Url { get; set; }

        [JsonPropertyName("id")]
        public required long Id { get; set; }

        [JsonPropertyName("node_id")]
        public required string NodeId { get; set; }

        [JsonPropertyName("name")]
        public required string Name { get; set; }

        [JsonPropertyName("label")]
        public required string Label { get; set; }

        [JsonPropertyName("content_type")]
        public required string ContentType { get; set; }

        [JsonPropertyName("state")]
        public required string State { get; set; }

        [JsonPropertyName("size")]
        public required long Size { get; set; }

        [JsonPropertyName("download_count")]
        public required long DownloadCount { get; set; }

        [JsonPropertyName("browser_download_url")]
        public required string BrowserDownloadUrl { get; set; }
    }
}
