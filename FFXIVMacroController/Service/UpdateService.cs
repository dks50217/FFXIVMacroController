using FFXIVMacroControllerApp.Model;
using Microsoft.AspNetCore.Components;
using Octokit;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace FFXIVMacroControllerApp.Service
{
    public interface IUpdateService
    {
        public Task CheckForUpdateAsync();
        public Func<Task> OnUpdateConfirm { get; set; }
        public Func<Task> OnUpdateEnd { get; set; }
    }

    public class UpdateService : IUpdateService
    {
        private string? DownloadUrl { get; set; }
        private string? DownloadVersion { get; set; }
        public Func<Task>? OnUpdateConfirm { get; set; }
        public Func<Task>? OnUpdateEnd { get; set; }

        public async Task CheckForUpdateAsync()
        {
            var localVersion = GetLocalVersion();
            var remoteItem = await GetRemoteVersionAsync();

            if (remoteItem == null || localVersion == null)
            {
                MessageBox.Show("無法檢查更新。");
                return;
            }

            if (remoteItem.Version.CompareTo($"v{localVersion}") > 0)
            {
                var result = MessageBox.Show("有新版本可用，是否下載更新？", "更新提示", MessageBoxButton.YesNo);

                if (result == MessageBoxResult.Yes)
                {
                    if (OnUpdateConfirm is not null)
                    {
                        await OnUpdateConfirm.Invoke();
                    }

                    DownloadUrl = remoteItem.DownloadURL;
                    DownloadVersion = remoteItem.Version;

                    await DownloadAndInstallUpdate();

                    if (OnUpdateEnd is not null)
                    {
                        await OnUpdateEnd.Invoke();
                    }
                }
            }
        }

        private Version? GetLocalVersion()
        {
            return Assembly.GetExecutingAssembly().GetName().Version;
        }

        private async Task<GithubVersionModel?> GetRemoteVersionAsync()
        {
            try
            {
                string targetUsername = "dks50217";
                string targetRepositoryName = "FFXIVMacroController";

                var github = new GitHubClient(new Octokit.ProductHeaderValue("GitHubUpdateChecker"));

                var releases = await github.Repository.Release.GetAll(targetUsername, targetRepositoryName);

                if (releases.Count > 0)
                {
                    var latestRelease = releases[0];

                    Console.WriteLine($"Latest Release Tag: {latestRelease.TagName}");

                    var downloadUrl = latestRelease.Assets.First().BrowserDownloadUrl;

                    var result = new GithubVersionModel
                    {
                        DownloadURL = downloadUrl,
                        Version = latestRelease.TagName
                    };

                    return result;
                }
                else
                {
                    Console.WriteLine("No releases found for the repository.");
                    return null;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"檢查版本時出錯：{ex.Message}");
                return null;
            }
        }

        private async Task DownloadAndInstallUpdate()
        {
            try
            {
                using var httpClient = new HttpClient();

                using var response = await httpClient.GetAsync(DownloadUrl, HttpCompletionOption.ResponseHeadersRead);

                if (!response.IsSuccessStatusCode)
                {
                    Console.WriteLine($"下載失敗: {response.StatusCode}");
                    return;
                }

                using var stream = await response.Content.ReadAsStreamAsync();
                var fileName = $"FFXIVMacroController_{DownloadVersion}.zip";
                var targetFilePath = Path.Combine(Directory.GetCurrentDirectory(), fileName);
                using var fileStream = new FileStream(targetFilePath, System.IO.FileMode.Create, FileAccess.Write, FileShare.None);
                await stream.CopyToAsync(fileStream);

                string scriptPath = "update.ps1";
                string arguments = $"-File \"{scriptPath}\"";

                // 啟動 PowerShell 腳本
                Process.Start(new ProcessStartInfo
                {
                    FileName = "powershell.exe",
                    Arguments = arguments,
                    UseShellExecute = false,
                    CreateNoWindow = false
                });

                System.Windows.Application.Current.Dispatcher.Invoke(() =>
                {
                    System.Windows.Application.Current.Shutdown();
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show($"下載安裝更新時出錯：{ex.Message}");
            }
        }
    }
}
