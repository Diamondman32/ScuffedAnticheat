using System.Collections.Generic;
using System.IO;
using Terraria.ModLoader;
using System.Linq;
using System.Runtime.InteropServices;
using static Terraria.ModLoader.Core.TmodFile;
using System;
using System.Reflection;
using Terraria.Localization;
using SQLitePCL;
using Terraria;

namespace ScuffedAnticheatMod;

[Autoload(Side = ModSide.Server)]
internal class NativeFeatureSystem : ModSystem
{
    private readonly Dictionary<string, IntPtr> loadedLibs = new();

    public override void Load()
    {
        Directory.CreateDirectory(Path.Combine(Main.SavePath, "ScuffedAnticheatMod"));
        string path = ExtractPlatformBinaries();
        Assembly assembly = typeof(SQLite3Provider_e_sqlite3).Assembly;

        NativeLibrary.SetDllImportResolver(assembly, (name, assembly, dllSearchPath) =>
        {
            if (!loadedLibs.ContainsKey("sqlite3"))
            {
                IntPtr handle = NativeLibrary.Load(path);
                Mod.Logger.Info($"Successfully loaded native library: {path}. Handle: {handle}");
                loadedLibs["sqlite3"] = handle;
            }
            return loadedLibs["sqlite3"];
        });

        

        Batteries_V2.Init();
    }

    public override void Unload()
    {
        foreach (IntPtr handle in loadedLibs.Values)
        {
            NativeLibrary.Free(handle);
        }

        // ALMonoMicrophone.UnloadOpenAL();
    }

    private string ExtractPlatformBinaries()
    {
        string path = "lib/runtimes/" + GetSQLiteNativeResource();
        // string path = GetSQLiteNativeResource();
        // path = path[(path.LastIndexOf(Path.PathSeparator)+1)..];
        string destinationPath = Path.Combine(Main.SavePath, "ScuffedAnticheatMod", path[(path.LastIndexOfAny([Path.DirectorySeparatorChar, '/', '\\'])+1)..]);
        // string destinationPath = Path.Combine(Main.SavePath, "ScuffedAnticheatMod", path);

        if (!File.Exists(destinationPath))
        {
            // using FileStream fs = Mod.Code.GetFile(path);
            // byte[] buffer = new byte[fs.Length];
            // fs.Read(buffer, 0, (int)fs.Length);
            byte[] buffer = Mod.GetFileBytes(path);
            File.WriteAllBytes(destinationPath, buffer);
        }

        return destinationPath;
    }

    #nullable enable
    private static string? GetSQLiteNativeResource()
    {
        // This function was stolen from ChatGPT so hopefully it works :/
        string arch = RuntimeInformation.ProcessArchitecture.ToString().ToLowerInvariant();

        bool is64 = IntPtr.Size == 8;

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            return arch switch
            {
                "x86"      => "win-x86/native/e_sqlite3.dll",
                "x64"      => "win-x64/native/e_sqlite3.dll",
                "arm"      => "win-arm/native/e_sqlite3.dll",
                "arm64"    => "win-arm64/native/e_sqlite3.dll",
                _          => null
            };
        }

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            return arch switch
            {
                "x86"      => "linux-x86/native/libe_sqlite3.so",
                "x64"      => "linux-x64/native/libe_sqlite3.so",
                "arm"      => "linux-arm/native/libe_sqlite3.so",
                "arm64"    => "linux-arm64/native/libe_sqlite3.so",

                // Additional Linux variants (if you choose to pack them)
                "armel"    => "linux-armel/native/libe_sqlite3.so",
                "mips64"   => "linux-mips64/native/libe_sqlite3.so",
                "ppc64le"  => "linux-ppc64le/native/libe_sqlite3.so",
                "s390x"    => "linux-s390x/native/libe_sqlite3.so",

                // musl variants
                _ when arch == "arm-musl" =>
                    "linux-musl-arm/native/libe_sqlite3.so",

                _ when arch == "arm64-musl" =>
                    "linux-musl-arm64/native/libe_sqlite3.so",

                _ when arch == "x64-musl" =>
                    "linux-musl-x64/native/libe_sqlite3.so",

                // Alpine ARM-like variant
                _ when arch.Contains("alpine") && arch.Contains("arm") =>
                    is64
                    ? "alpine-arm64/native/libe_sqlite3.so"
                    : "alpine-arm/native/libe_sqlite3.so",

                _ when arch == "alpine-x64" =>
                    "alpine-x64/native/libe_sqlite3.so",

                _ => null
            };
        }

        if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            return arch switch
            {
                "x64"      => "osx-x64/native/libe_sqlite3.dylib",
                "arm64"    => "osx-arm64/native/libe_sqlite3.dylib",

                // Catalyst builds (rare)
                _ when arch == "maccatalyst-arm64" =>
                    "maccatalyst-arm64/native/libe_sqlite3.dylib",

                _ when arch == "maccatalyst-x64" =>
                    "maccatalyst-x64/native/libe_sqlite3.dylib",

                _ => null
            };
        }

        return null;
    }
}