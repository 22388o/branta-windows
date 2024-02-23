﻿using Branta.Automation.Wallets;
using Branta.Classes;
using Branta.Domain;
using Branta.Enums;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Windows.Forms;

namespace Branta.Automation;

public class VerifyWallets : BaseAutomation
{
    public ObservableCollection<Wallet> Wallets { get; } = new();

    public static readonly List<BaseWallet> WalletTypes = new()
    {
        new BlockStreamGreen(),
        new Sparrow(),
        new Wasabi(),
        new Trezor()
    };

    private bool _isFirstRun = true;

    public VerifyWallets(NotifyIcon notifyIcon, Settings settings) : base(notifyIcon, settings,
        (int)settings.WalletVerification.WalletVerifyEvery.TotalSeconds)
    {
    }

    public override void Run()
    {
        Trace.WriteLine("Started: Verify Wallets");
        var sw = Stopwatch.StartNew();

        var previousWalletStatus = Wallets
            .DistinctBy(w => w.Name)
            .ToDictionary(w => w.Name, w => w.Status);
        
        var bufferedWallets = new List<Wallet>();

        foreach (var walletType in WalletTypes)
        {
            var wallet = Verify(walletType);

            if (wallet.Status != WalletStatus.Verified &&
                previousWalletStatus.GetValueOrDefault(walletType.Name, WalletStatus.Verified) == WalletStatus.Verified &&
                Settings.WalletVerification.WalletStatusChangeEnabled)
            {
                NotifyIcon.ShowBalloonTip(new Notification
                {
                    Message = $"{walletType.Name} failed verification.",
                    Icon = ToolTipIcon.Warning
                });
            }

            if (_isFirstRun)
            {
                Dispatcher.Invoke(() => Wallets.Add(wallet));
            }
            else
            {
                bufferedWallets.Add(wallet);
            }
        }

        if (!_isFirstRun)
        {
            Dispatcher.Invoke(() => Wallets.Set(bufferedWallets));
        }

        _isFirstRun = false;

        sw.Stop();
        Trace.WriteLine($"Stopped: Verify Wallets. Took {sw.Elapsed}");
    }

    public static Wallet Verify(BaseWallet walletType)
    {
        if (!Directory.Exists(walletType.GetPath()))
        {
            return null;
        }

        var version = walletType.GetVersion();

        WalletStatus status;

        var expectedHash = version != null ? walletType.CheckSums.GetValueOrDefault(version) : null;

        if (expectedHash == null)
        {
            status = WalletStatus.VersionNotSupported;
        }
        else
        {
            try
            {
                var hash = CreateMd5ForFolder(walletType.GetPath());
                Trace.WriteLine($"Expected: {expectedHash}; Actual: {hash}");
                status = hash == expectedHash ? WalletStatus.Verified : WalletStatus.NotVerified;
            }
            catch
            {
                status = WalletStatus.NotVerified;
            }
        }

        return new Wallet
        {
            Name = walletType.Name,
            Version = version,
            Status = status
        };
    }

    public static string CreateMd5ForFolder(string path)
    {
        var files = Directory
            .GetFiles(path, "*", SearchOption.AllDirectories)
            .OrderBy(p => p).ToList();

        var md5 = MD5.Create();

        for (var i = 0; i < files.Count; i++)
        {
            var file = files[i];

            var relativePath = file.Substring(path.Length + 1);
            var pathBytes = Encoding.UTF8.GetBytes(relativePath.ToLower());
            md5.TransformBlock(pathBytes, 0, pathBytes.Length, pathBytes, 0);

            var contentBytes = File.ReadAllBytes(file);
            if (i == files.Count - 1)
                md5.TransformFinalBlock(contentBytes, 0, contentBytes.Length);
            else
                md5.TransformBlock(contentBytes, 0, contentBytes.Length, contentBytes, 0);
        }

        return BitConverter.ToString(md5.Hash).Replace("-", "").ToLower();
    }
}
