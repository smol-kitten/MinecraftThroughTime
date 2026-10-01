using System.Text;
using MinecraftThroughTime;

namespace MinecraftThroughTime.Tests
{
    public class BakedProfileTests : IDisposable
    {
        readonly string dir = Directory.CreateTempSubdirectory("mtt-bake-").FullName;

        public void Dispose() => Directory.Delete(dir, true);

        //stand-in for a signed exe: arbitrary bytes, ends with something that is not a payload
        //(a real signed exe ends with its certificate table)
        static byte[] FakeExe(int seed = 1)
        {
            byte[] b = new byte[64 * 1024];
            new Random(seed).NextBytes(b);
            Encoding.ASCII.GetBytes("MZ").CopyTo(b, 0);
            return b;
        }

        string WriteExe(byte[] content, string name = "MinecraftThroughTime.exe")
        {
            string p = Path.Combine(dir, name);
            File.WriteAllBytes(p, content);
            return p;
        }

        [Fact]
        public void Plain_exe_has_no_baked_profile()
        {
            string exe = WriteExe(FakeExe());
            Assert.Null(BakedProfile.Find(exe));
        }

        [Fact]
        public void Path_bake_writes_identical_copy_and_sidecar()
        {
            string exe = WriteExe(FakeExe());
            string? baked = Bake.BakeProfile(@"C:\profiles\profile.json", exe);

            Assert.Equal(Path.Combine(dir, "MinecraftThroughTime_PathBaked.exe"), baked);
            Assert.Equal(File.ReadAllBytes(exe), File.ReadAllBytes(baked!));
            Assert.True(File.Exists(Path.Combine(dir, "MinecraftThroughTime_PathBaked.mttprofile")));

            BakedProfile.Baked? b = BakedProfile.Find(baked!);
            Assert.NotNull(b);
            Assert.False(b!.Full);
            Assert.Equal(@"C:\profiles\profile.json", b.Value);
        }

        [Fact]
        public void Full_bake_writes_identical_copy_and_sidecar()
        {
            string exe = WriteExe(FakeExe());
            string json = "{\"version\":1,\"entries\":[{\"version\":\"1.0\",\"date\":\"2024-01-01\"}]}";
            string profile = Path.Combine(dir, "profile.json");
            File.WriteAllText(profile, json);

            string? baked = Bake.BakeFully(profile, exe);

            Assert.Equal(Path.Combine(dir, "MinecraftThroughTime_FullyBaked.exe"), baked);
            Assert.Equal(File.ReadAllBytes(exe), File.ReadAllBytes(baked!));

            BakedProfile.Baked? b = BakedProfile.Find(baked!);
            Assert.NotNull(b);
            Assert.True(b!.Full);
            Assert.Equal(json, b.Value);
        }

        [Fact]
        public void Rebake_replaces_copy_and_sidecar()
        {
            string exe = WriteExe(FakeExe());
            Bake.BakeProfile("https://example.org/old.json", exe);
            string? baked = Bake.BakeProfile("https://example.org/new.json", exe);
            Assert.Equal("https://example.org/new.json", BakedProfile.Find(baked!)!.Value);
            Assert.Equal(File.ReadAllBytes(exe), File.ReadAllBytes(baked!));
        }

        [Fact]
        public void Path_longer_than_512_is_refused()
        {
            string exe = WriteExe(FakeExe());
            Assert.Null(Bake.BakeProfile(new string('a', 513), exe));
            Assert.Single(Directory.GetFiles(dir));
        }

        [Fact]
        public void Legacy_path_bake_appended_to_exe_is_read()
        {
            byte[] exe = FakeExe().Concat(Encoding.UTF8.GetBytes("[MTT]profiles/server.json")).ToArray();
            BakedProfile.Baked? b = BakedProfile.Find(WriteExe(exe, "old_PathBaked.exe"));
            Assert.NotNull(b);
            Assert.False(b!.Full);
            Assert.Equal("profiles/server.json", b.Value);
        }

        [Fact]
        public void Legacy_full_bake_appended_to_exe_is_read()
        {
            //the layout older versions wrote: <exe>[MTT]<json>[MTTDL]<int32>[MTTFBP]
            string json = "{\"entries\":[]}";
            byte[] raw = Encoding.UTF8.GetBytes(json);
            byte[] exe = FakeExe()
                .Concat(Encoding.UTF8.GetBytes("[MTT]")).Concat(raw)
                .Concat(Encoding.UTF8.GetBytes("[MTTDL]")).Concat(BitConverter.GetBytes(raw.Length))
                .Concat(Encoding.UTF8.GetBytes("[MTTFBP]")).ToArray();

            BakedProfile.Baked? b = BakedProfile.Find(WriteExe(exe, "old_FullyBaked.exe"));
            Assert.NotNull(b);
            Assert.True(b!.Full);
            Assert.Equal(json, b.Value);
        }

        [Fact]
        public void Sidecar_wins_over_legacy_appended_data()
        {
            byte[] exe = FakeExe().Concat(Encoding.UTF8.GetBytes("[MTT]legacy.json")).ToArray();
            string p = WriteExe(exe, "x_PathBaked.exe");
            File.WriteAllBytes(BakedProfile.SidecarPath(p), BakedProfile.PathPayload("sidecar.json"));
            Assert.Equal("sidecar.json", BakedProfile.Find(p)!.Value);
        }

        [Fact]
        public void Empty_or_garbage_sidecar_falls_back_to_legacy()
        {
            byte[] exe = FakeExe().Concat(Encoding.UTF8.GetBytes("[MTT]legacy.json")).ToArray();
            string p = WriteExe(exe, "x_PathBaked.exe");
            File.WriteAllBytes(BakedProfile.SidecarPath(p), Encoding.UTF8.GetBytes("not a profile"));
            Assert.Equal("legacy.json", BakedProfile.Find(p)!.Value);
        }

        [Fact]
        public void Corrupt_full_length_is_not_trusted()
        {
            byte[] exe = FakeExe()
                .Concat(Encoding.UTF8.GetBytes("[MTTDL]")).Concat(BitConverter.GetBytes(int.MaxValue))
                .Concat(Encoding.UTF8.GetBytes("[MTTFBP]")).ToArray();
            Assert.Null(BakedProfile.Find(WriteExe(exe)));
        }

        [Fact]
        public void Tiny_streams_do_not_throw()
        {
            Assert.Null(BakedProfile.ReadTail(new MemoryStream()));
            Assert.Null(BakedProfile.ReadTail(new MemoryStream(Encoding.ASCII.GetBytes("[MT"))));
            Assert.Equal("", BakedProfile.ReadTail(new MemoryStream(Encoding.ASCII.GetBytes("[MTT]")))!.Value);
        }
    }
}
