using System.Text;
using System.Security.Cryptography;

namespace MinecraftThroughTime
{
    //User Temp folder
    class CDL
    {
        readonly string cacheDir = Path.Combine(Path.GetTempPath(), "MinecraftThroughTime");
        readonly HttpClient Client;

        public CDL()
        {
            if (!Directory.Exists(cacheDir))
            {
                Directory.CreateDirectory(cacheDir);
            }
            Client = new HttpClient();
        }

        /// <summary>
        /// Get the cache directory
        /// </summary>
        /// <returns>string with path</returns>
        public string GetCacheDir() => cacheDir;

        /// <summary>
        /// Get the SHA1 hash of a file
        /// </summary>
        /// <param name="data">bytes to hash</param>
        /// <returns>SHA1 String</returns>
        public static string GetSha1(byte[] data) => BitConverter.ToString(SHA1.HashData(data)).Replace("-", "").ToLower();

        /// <summary>
        /// Map a URL to a cache file path, safely. The URL is split into path
        /// segments (as before, so caching behaviour is unchanged), but "..",
        /// ".", rooted, and invalid segments are stripped, and the resulting
        /// path is verified to stay inside the cache directory — a crafted URL
        /// cannot escape it (path traversal / arbitrary file write).
        /// </summary>
        private string CachePath(string url)
        {
            string urli = url.Replace("https:", "").Replace("http:", "").Replace(":", " ").Replace("/", " ").Replace("?", "").Replace("&", "").Replace("=", "");
            var segments = new List<string>();
            foreach (string raw in urli.Split(' ', StringSplitOptions.RemoveEmptyEntries))
            {
                if (raw == ".." || raw == "." || Path.IsPathRooted(raw))
                    continue;
                string seg = raw;
                foreach (char c in Path.GetInvalidFileNameChars())
                    seg = seg.Replace(c, '_');
                if (seg.Length > 0)
                    segments.Add(seg);
            }
            string path = Path.GetFullPath(Path.Combine(cacheDir, Path.Combine(segments.ToArray())));
            string root = Path.GetFullPath(cacheDir) + Path.DirectorySeparatorChar;
            if (!path.StartsWith(root, StringComparison.Ordinal))
                throw new Exception("Refusing to write outside the cache directory");
            return path;
        }

        /// <summary>
        /// Reject non-http(s) URLs (e.g. file://) before fetching.
        /// </summary>
        private static string ValidateUrl(string url)
        {
            if (!Uri.TryCreate(url, UriKind.Absolute, out Uri? uri) ||
                (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps))
                throw new Exception("Refusing non-http(s) URL: " + url);
            return url;
        }

        /// <summary>
        /// Download a file and save it to the cache directory
        /// if it already exists, return the cached file
        /// </summary>
        /// <param name="url">url to download from</param>
        /// <returns></returns>
        private byte[] InternalDownload(string url)
        {
            //workaround to cache server jars as they are all called server.jar
            string path = CachePath(url);

            //make folder
            string? folder = Path.GetDirectoryName(path) ?? throw new Exception("Invalid path");
            if (!Directory.Exists(folder))
            {
                Directory.CreateDirectory(folder);
            }

            //if exists, return read
            if (File.Exists(path))
            {
                return File.ReadAllBytes(path);
            }

            //else download and save
            byte[] data = Client.GetByteArrayAsync(ValidateUrl(url)).Result;
            File.WriteAllBytes(path, data);
            return data;
        }

        /// <summary>
        /// Check if a file exists in the cache
        /// </summary>
        /// <param name="url">url to check</param>
        /// <returns>true/false</returns>
        public bool ExistsLocal(string url)
        {
            string path = CachePath(url);
            return File.Exists(path);
        }

        /// <summary>
        /// Check if a file exists on the server
        /// TODO: Not too safe, should be improved
        /// </summary>
        /// <param name="url">url to test</param>
        /// <returns>true/false</returns>
        public bool ExistsRemote(string url)
        {
            try
            {
                Client.GetAsync(url).Result.EnsureSuccessStatusCode();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// Truely download a file from the internet
        /// </summary>
        /// <param name="url">url to download from</param>
        /// <param name="path">full path including file to write to</param>
        /// <returns></returns>
        public byte[] Download(string url, string path)
        {
            byte[] data = InternalDownload(url);
            File.WriteAllBytes(path, data);
            return data;
        }

        /// <summary>
        /// Download a url as a string
        /// </summary>
        /// <param name="url">url to download</param>
        /// <returns>string/unhandled error</returns>
        public string DownloadString(string url) => Encoding.UTF8.GetString(InternalDownload(url));

        /// <summary>
        /// Download a url as a byte array
        /// </summary>
        /// <param name="url">url to download</param>
        /// <returns>byte[] or unhandled exception</returns>
        public byte[] DownloadBytes(string url) =>  InternalDownload(url);

        /// <summary>
        /// Temp Download a file and return the path
        /// </summary>
        /// <param name="url">url to download</param>
        /// <returns>path to file</returns>
        public string DownloadFresh(string url)
        {
            byte[] data = Client.GetByteArrayAsync(ValidateUrl(url)).Result;
            string path = CachePath(url);
            string? folder = Path.GetDirectoryName(path) ?? throw new Exception("Invalid path");
            if (!Directory.Exists(folder))
                Directory.CreateDirectory(folder);
            File.WriteAllBytes(path, data);
            return path;
        }

    }

}
