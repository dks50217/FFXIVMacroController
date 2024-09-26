using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FFXIVMacroControllerWpfApp.Helper
{
    using FFXIVMacroControllerWpfApp.Model;
    using Octokit;
    using System;
    using System.Diagnostics;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Reflection;
    using System.Threading.Channels;
    using System.Threading.Tasks;
    using System.Windows;

    public class UpdateChecker
    {
        private string? DownloadUrl { get; set; }

        public async Task CheckForUpdateAsync()
        {
            var localVersion = GetLocalVersion();
            var remoteItem = await GetRemoteVersionAsync();


            if (remoteItem == null || localVersion == null)
            {
                MessageBox.Show("無法檢查更新。");
                return;
            }

            if (remoteItem.Version.CompareTo(localVersion) > 0)
            {
                var result = MessageBox.Show("有新版本可用，是否下載更新？", "更新提示", MessageBoxButton.YesNo);

                if (result == MessageBoxResult.Yes)
                {
                    DownloadUrl = remoteItem.DownloadURL;

                    DownloadAndInstallUpdate();
                }
            }
            else
            {
                MessageBox.Show("已是最新版本。");
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

                    var result = new GithubVersionModel
                    {
                        DownloadURL = latestRelease.AssetsUrl,
                        Version = new Version(latestRelease.TagName)
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

        private void DownloadAndInstallUpdate()
        {
            try
            {
                var process = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = DownloadUrl,
                        UseShellExecute = true
                    }
                };

                process.Start();

                System.Windows.Application.Current.Shutdown();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"下載安裝更新時出錯：{ex.Message}");
            }
        }
    }

}
