﻿using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Security.Cryptography;

namespace Branta.Classes;

public static class Helper
{
    public static string GetBrantaVersionWithoutCommitHash()
    {
        var version = Assembly.GetEntryAssembly()!.GetCustomAttribute<AssemblyInformationalVersionAttribute>()!
            .InformationalVersion;

        return version.Split("+")[0];
    }

    public static void OpenLink(string url)
    {
        Process.Start(new ProcessStartInfo("cmd", $"/c start {url}") { CreateNoWindow = true });
    }

    public static string CalculateSha256(string filePath)
    {
        using var sha256 = SHA256.Create();
        using var stream = File.OpenRead(filePath);
        var hashBytes = sha256.ComputeHash(stream);
        return BitConverter.ToString(hashBytes).Replace("-", "").ToLower();
    }

    public static string CalculateSha512(string filePath, bool base64Encoding = false)
    {
        using var sha512 = SHA512.Create();
        using var stream = File.OpenRead(filePath);
        var hashBytes = sha512.ComputeHash(stream);

        return base64Encoding
            ? Convert.ToBase64String(hashBytes)
            : BitConverter.ToString(hashBytes).Replace("-", "").ToLower();
    }
}