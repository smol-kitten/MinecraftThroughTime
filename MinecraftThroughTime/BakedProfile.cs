namespace MinecraftThroughTime
{
    /// <summary>
    /// Read and write baked profiles.
    /// </summary>
    /// <remarks>
    /// A bake writes the profile to a sidecar file next to the exe copy:
    /// MinecraftThroughTime_PathBaked.exe + MinecraftThroughTime_PathBaked.mttprofile.
    /// The exe copy stays byte-identical to the original, so its Authenticode signature stays valid.
    /// Older versions appended the same payload to the exe itself, which breaks the signature
    /// (the certificate table must be the last thing in the file). Those copies are still read.
    ///
    /// Payload formats (sidecar file content or legacy exe tail):
    ///   path/url bake: [MTT]&lt;path or url&gt;
    ///   full bake:     [MTT]&lt;json&gt;[MTTDL]&lt;int32 length of json&gt;[MTTFBP]
    /// </remarks>
    public static class BakedProfile
    {
        public const string SidecarExtension = ".mttprofile";

        const string MTT = "[MTT]";
        const string MTTDL = "[MTTDL]";
        const string MTTFBP = "[MTTFBP]";

        //path max 512 + 5 bytes for [MTT]
        const int MaxPathPayload = 512 + 5;

        /// <summary>
        /// A baked profile: a path/url, or the full profile json
        /// </summary>
        public record Baked(bool Full, string Value);

        /// <summary>
        /// Sidecar file that belongs to an exe
        /// </summary>
        public static string SidecarPath(string exePath) => Path.ChangeExtension(exePath, SidecarExtension);

        public static byte[] PathPayload(string urlOrPath) => System.Text.Encoding.UTF8.GetBytes(MTT + urlOrPath);

        public static byte[] FullPayload(byte[] rawdata)
        {
            byte[] data = System.Text.Encoding.UTF8.GetBytes(MTT);
            data = data.Concat(rawdata).ToArray();
            data = data.Concat(System.Text.Encoding.UTF8.GetBytes(MTTDL)).ToArray();
            data = data.Concat(BitConverter.GetBytes(rawdata.Length)).ToArray();
            data = data.Concat(System.Text.Encoding.UTF8.GetBytes(MTTFBP)).ToArray();
            return data;
        }

        /// <summary>
        /// Find the baked profile of an exe: sidecar first, then the legacy appended data
        /// </summary>
        /// <returns>null if the exe has no baked profile</returns>
        public static Baked? Find(string exePath)
        {
            string sidecar = SidecarPath(exePath);
            if (File.Exists(sidecar))
            {
                using FileStream sfs = File.OpenRead(sidecar);
                Baked? b = ReadTail(sfs);
                if (b != null) return b;
            }

            if (!File.Exists(exePath)) return null;
            using FileStream fs = File.OpenRead(exePath);
            return ReadTail(fs);
        }

        /// <summary>
        /// Read a payload from the end of a stream (a sidecar file or a legacy baked exe)
        /// </summary>
        /// <returns>null if the stream does not end with a payload</returns>
        public static Baked? ReadTail(Stream fs)
        {
            long len = fs.Length;

            //if ends with exactly [MTTFBP], then it is a full profile
            //<...>[MTT]<json>[MTTDL]<int32>[MTTFBP]
            int endoffset = MTTDL.Length + 4 + MTTFBP.Length;
            if (len >= endoffset + MTT.Length && Ascii(fs, len - MTTFBP.Length, MTTFBP.Length) == MTTFBP)
            {
                byte[] intbuffer = new byte[4];
                fs.Seek(len - 4 - MTTFBP.Length, SeekOrigin.Begin);
                fs.ReadExactly(intbuffer, 0, 4);
                int size = BitConverter.ToInt32(intbuffer, 0);

                long start = len - endoffset - size;
                if (size < 0 || start < MTT.Length) return null;
                if (Ascii(fs, start - MTT.Length, MTT.Length) != MTT) return null;
                if (Ascii(fs, len - endoffset, MTTDL.Length) != MTTDL) return null;

                byte[] buffer = new byte[size];
                fs.Seek(start, SeekOrigin.Begin);
                fs.ReadExactly(buffer, 0, size);
                return new Baked(true, System.Text.Encoding.UTF8.GetString(buffer));
            }

            //read backwards till [MTT], example [MTT]C:\Users\user\profile.json. This is the baked file(path)
            //stop if not found in 517 bytes
            for (int i = MTT.Length; i <= MaxPathPayload && i <= len; i++)
            {
                if (Ascii(fs, len - i, MTT.Length) == MTT)
                {
                    byte[] buffer = new byte[i - MTT.Length];
                    fs.Seek(len - i + MTT.Length, SeekOrigin.Begin);
                    fs.ReadExactly(buffer, 0, buffer.Length);
                    return new Baked(false, System.Text.Encoding.UTF8.GetString(buffer));
                }
            }
            return null;
        }

        static string Ascii(Stream fs, long offset, int count)
        {
            byte[] b = new byte[count];
            fs.Seek(offset, SeekOrigin.Begin);
            fs.ReadExactly(b, 0, count);
            return System.Text.Encoding.ASCII.GetString(b);
        }
    }
}
