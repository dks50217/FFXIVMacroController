using FFXIVMacroControllerApp.Model;
using Octokit;
using System.Linq;
using System;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;

namespace FFXIVMacroControllerApp.Service
{
    public interface IUpdateService
    {
        public Task CheckForUpdateAsync();
        public Func<Task>? OnUpdateConfirm { get; set; }
        public Func<Task>? OnUpdateEnd { get; set; }
        public Action<int>? OnDownloadProgress { get; set; }
    }

    public class UpdateService : IUpdateService
    {
        private string? DownloadUrl { get; set; }
        private string? DownloadVersion { get; set; }
        public Func<Task>? OnUpdateConfirm { get; set; }
        public Func<Task>? OnUpdateEnd { get; set; }
        public Action<int>? OnDownloadProgress { get; set; }

        private const string TargetUsername = "dks50217";
        private const string TargetRepository = "FFXIVMacroController";

        public async Task CheckForUpdateAsync()
        {
            var localVersion = GetLocalVersion();
            var remoteItem = await GetRemoteVersionAsync();

            if (remoteItem == null || localVersion == null)
                return; // 靜默失敗，不影響啟動

            // 移除 tag 前綴的 "v" 再解析
            var remoteTag = remoteItem.Version.TrimStart('v');
            if (!Version.TryParse(remoteTag, out var remoteVersion))
                return;

            if (remoteVersion <= localVersion)
                return;

            var result = MessageBox.Show(
                $"有新版本可用 ({remoteItem.Version})，是否下載更新？",
                "更新提示",
                MessageBoxButton.YesNo);

            if (result != MessageBoxResult.Yes)
                return;

            if (OnUpdateConfirm is not null)
                await OnUpdateConfirm.Invoke();

            DownloadUrl = remoteItem.DownloadURL;
            DownloadVersion = remoteItem.Version;

            await DownloadAndInstallUpdate();

            if (OnUpdateEnd is not null)
                await OnUpdateEnd.Invoke();
        }

        private Version? GetLocalVersion()
        {
            return Assembly.GetExecutingAssembly().GetName().Version;
        }

        private async Task<GithubVersionModel?> GetRemoteVersionAsync()
        {
            try
            {
                var github = new GitHubClient(new ProductHeaderValue("FFXIVMacroController-UpdateChecker"));
                var releases = await github.Repository.Release.GetAll(TargetUsername, TargetRepository);

                if (releases.Count == 0)
                    return null;

                var latest = releases[0];

                // 找名稱包含 "FFXIVMacroController" 且副檔名為 .zip 的 asset
                var asset = latest.Assets
                    .FirstOrDefault(a => a.Name.Contains("FFXIVMacroController", StringComparison.OrdinalIgnoreCase)
                                      && a.Name.EndsWith(".zip", StringComparison.OrdinalIgnoreCase))
                    ?? latest.Assets.FirstOrDefault();

                if (asset == null)
                    return null;

                Console.WriteLine($"Latest release: {latest.TagName}, asset: {asset.Name}");

                return new GithubVersionModel
                {
                    DownloadURL = asset.BrowserDownloadUrl,
                    Version = latest.TagName
                };
            }
            catch (Exception ex)
            {
                Console.WriteLine($"檢查版本失敗（靜默）: {ex.Message}");
                return null; // 靜默失敗
            }
        }

        private async Task DownloadAndInstallUpdate()
        {
            try
            {
                using var httpClient = new HttpClient();
                httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("FFXIVMacroController-Updater");

                using var response = await httpClient.GetAsync(DownloadUrl, HttpCompletionOption.ResponseHeadersRead);

                if (!response.IsSuccessStatusCode)
                {
                    MessageBox.Show($"下載失敗: {response.StatusCode}");
                    return;
                }

                var totalBytes = response.Content.Headers.ContentLength ?? 0;
                var baseDir = AppDomain.CurrentDomain.BaseDirectory;
                var fileName = $"FFXIVMacroController_{DownloadVersion}.zip";
                var targetFilePath = Path.Combine(baseDir, fileName);

                // 下載並回報進度
                using (var stream = await response.Content.ReadAsStreamAsync())
                using (var fileStream = new FileStream(targetFilePath, System.IO.FileMode.Create))
                {
                    var buffer = new byte[81920];
                    long downloaded = 0;
                    int read;

                    while ((read = await stream.ReadAsync(buffer)) > 0)
                    {
                        await fileStream.WriteAsync(buffer.AsMemory(0, read));
                        downloaded += read;

                        if (totalBytes > 0)
                        {
                            var progress = (int)(downloaded * 100 / totalBytes);
                            OnDownloadProgress?.Invoke(progress);
                        }
                    }
                } // fileStream 在這裡關閉，解除檔案鎖定，避免 PS1 解壓時失敗

                // 啟動 update.ps1
                var scriptPath = Path.Combine(baseDir, "update.ps1");
                var exePath = Path.Combine(baseDir, "FFXIVMacroControllerApp.exe");

                Process.Start(new ProcessStartInfo
                {
                    FileName = "powershell.exe",
                    Arguments = $"-ExecutionPolicy Bypass -File \"{scriptPath}\" -ExePath \"{exePath}\"",
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
