using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Godot_Start.Services
{
    public class GodotDownloads
    {
        public static readonly string path = "D:\\GitRepos\\GodotStart\\Godot Start\\downloads\\";

        public static async Task DownloadVersion(string url, string name)
        {
            if (!Path.Exists(path))
            {
                Directory.CreateDirectory(path);
            }

            using var client = new HttpClient();
            using var s = await client.GetStreamAsync(url);
            using var fs = new FileStream(path + name, FileMode.OpenOrCreate);
            await s.CopyToAsync(fs);
            s.Close();
            s.Dispose();
            fs.Dispose();
            client.Dispose();


            var unzippedDir = path + (name.Contains("_win64.exe") ? name[0..^8] : name[0..^4]);
            System.IO.Compression.ZipFile.ExtractToDirectory(path + name, unzippedDir);
        }

        public static void DeleteVersion(string name)
        {
            if (!Path.Exists(path))
            {
                return;
            }

            try
            {
                File.Delete(path + name);

                var unzippedDir = path + (name.Contains("_win64.exe") ? name[0..^8] : name[0..^4]);
                Directory.Delete(unzippedDir, true);
            }
            catch (Exception)
            {
            }
        }
    }
}
