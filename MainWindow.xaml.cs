using System;
using System.IO;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System.Windows;
using System.Diagnostics;

namespace CPPPatcher
{
    public partial class MainWindow : Window
    {
        private const string ServerUrl = "";
        private const string LocalPackFolder = "pack/";
        private const string LogFileName = "autopatch.log";

        public MainWindow()
        {
            InitializeComponent();
            PatchFiles();
        }

        private async void PatchFiles()
        {
            try
            {
                var serverFiles = await GetFileList(ServerUrl + "file_list.json");
                foreach (var fileInfo in serverFiles)
                {
                    var localFilePath = Path.Combine(LocalPackFolder, fileInfo.Name ?? string.Empty);
                    if (!File.Exists(localFilePath) || await FileHash(localFilePath) != fileInfo.Hash)
                    {
                        Log($"Updating {fileInfo.Name}...");
                        await DownloadFile(ServerUrl + fileInfo.Name, localFilePath);
                        await Task.Delay(1000); // Add delay between downloads
                    }
                    else
                    {
                        Log($"{fileInfo.Name} is up to date.");
                    }
                }
            }
            catch (Exception ex)
            {
                Log($"Error: {ex.Message}");
            }
        }

        private async Task<FileInfo[]> GetFileList(string url)
        {
            using var client = new HttpClient();
            var response = await client.GetStringAsync(url);
            var fileList = JsonSerializer.Deserialize<FileInfo[]>(response, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
            });
            return fileList ?? Array.Empty<FileInfo>();
        }

        private async Task DownloadFile(string url, string localPath)
        {
            using var client = new HttpClient();
            var response = await client.GetAsync(url);
            response.EnsureSuccessStatusCode();

            var directory = Path.GetDirectoryName(localPath);
            if (directory != null)
            {
                Directory.CreateDirectory(directory);
            }

            using var fs = new FileStream(localPath, FileMode.Create, FileAccess.Write, FileShare.None);
            await response.Content.CopyToAsync(fs);
        }

        private async Task<string> FileHash(string filepath)
        {
            using var md5 = MD5.Create();
            using var stream = File.OpenRead(filepath);
            var hash = await md5.ComputeHashAsync(stream);
            return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
        }

        private void Log(string message)
        {
            using var logFile = new StreamWriter(LogFileName, true);
            logFile.WriteLine($"{DateTime.Now} - {message}");
        }

        private void StartGameButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Process.Start(new ProcessStartInfo("") { UseShellExecute = true });
                Log("Game started successfully.");
            }
            catch (Exception ex)
            {
                Log($"Failed to start the game: {ex.Message}");
            }
        }

        private void DiscordButton_Click(object sender, RoutedEventArgs e)
        {
            Process.Start(new ProcessStartInfo("") { UseShellExecute = true });
        }

        private void WebsiteButton_Click(object sender, RoutedEventArgs e)
        {
            Process.Start(new ProcessStartInfo("") { UseShellExecute = true });
        }

        private void ExitButton_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }
    }

    public class FileInfo
    {
        public required string Name { get; set; }
        public required string Hash { get; set; }
    }
}