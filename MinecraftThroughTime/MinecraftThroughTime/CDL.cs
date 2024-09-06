using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;
using System.Runtime.Intrinsics.Arm;
using System.IO;

namespace MinecraftThroughTime
{
    //User Temp folder
    class CDL
    {
        string cacheDir = Path.Combine(System.IO.Path.GetTempPath(), "MinecraftThroughTime");
        HttpClient client;

        public CDL()
        {
            if (!System.IO.Directory.Exists(cacheDir))
            {
                System.IO.Directory.CreateDirectory(cacheDir);
            }
            client = new HttpClient();

        }

        /// <summary>
        /// Get the cache directory
        /// </summary>
        /// <returns>string with path</returns>
        public string GetCacheDir() {
            return cacheDir;
        }

        /// <summary>
        /// Get the SHA1 hash of a file
        /// </summary>
        /// <param name="data">bytes to hash</param>
        /// <returns>SHA1 String</returns>
        public string GetSha1(byte[] data)
        {
            SHA1 shnya = SHA1.Create();
            byte[] hash = shnya.ComputeHash(data);
            return BitConverter.ToString(hash).Replace("-", "").ToLower();
        }

        /// <summary>
        /// Download a file and save it to the cache directory
        /// if it already exists, return the cached file
        /// </summary>
        /// <param name="url">url to download from</param>
        /// <returns></returns>
        private byte[] internalDownload(string url)
        {
            //workaround to cache server jars as they are all called server.jar
            string urli = url.Replace("https:", "").Replace("http:", "").Replace(":", " ").Replace("/", " ").Replace("?", "").Replace("&", "").Replace("=", "");

            string path = Path.Combine(cacheDir, Path.Combine(urli.Split(" ")));

            //make folder
            string folder = Path.GetDirectoryName(path);
            if (!System.IO.Directory.Exists(folder))
            {
                System.IO.Directory.CreateDirectory(folder);
            }
            
            //if exists, return read
            if (System.IO.File.Exists(path))
            {
                return System.IO.File.ReadAllBytes(path);
            }

            //else download and save
            byte[] data = client.GetByteArrayAsync(url).Result;
            System.IO.File.WriteAllBytes(path, data);
            return data;
        }

        /// <summary>
        /// Check if a file exists in the cache
        /// </summary>
        /// <param name="url">url to check</param>
        /// <returns>true/false</returns>
        public bool ExistsLocal(string url)
        {
            string urli = url.Replace("https:", "").Replace("http:", "").Replace(":", " ").Replace("/", " ").Replace("?", "").Replace("&", "").Replace("=", "");
            string path = Path.Combine(cacheDir, Path.Combine(urli.Split(" ")));
            return System.IO.File.Exists(path);
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
                client.GetAsync(url).Result.EnsureSuccessStatusCode();
                return true;
            }
            catch (Exception e)
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
            byte[] data = internalDownload(url);
            System.IO.File.WriteAllBytes(path, data);
            return data;
        }

        /// <summary>
        /// Download a url as a string
        /// </summary>
        /// <param name="url">url to download</param>
        /// <returns>string/unhandled error</returns>
        public string DownloadString(string url)
        {
            return System.Text.Encoding.UTF8.GetString(internalDownload(url));
        }

        /// <summary>
        /// Download a url as a byte array
        /// </summary>
        /// <param name="url">url to download</param>
        /// <returns>byte[] or unhandled exception</returns>
        public byte[] DownloadBytes(string url)
        {
            return internalDownload(url);
        }

        /// <summary>
        /// Temp Download a file and return the path
        /// </summary>
        /// <param name="url">url to download</param>
        /// <returns>path to file</returns>
        public string DownloadFresh(string url)
        {
            byte[] data = client.GetByteArrayAsync(url).Result;
            string path = Path.Combine(cacheDir, Path.Combine(url.Replace("https:", "").Replace("http:", "").Replace(":", " ").Replace("/", " ").Replace("?", "").Replace("&", "").Replace("=", "").Split(" ")));
            System.IO.File.WriteAllBytes(path, data);
            return path;
        }

    }

}
