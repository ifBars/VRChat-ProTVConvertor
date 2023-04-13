using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace ProTVConverter
{
    public class Version
    {
        private readonly string _owner;
        private readonly string _repo;
        private readonly HttpClient _client;

        public Version(string owner, string repo)
        {
            _owner = owner;
            _repo = repo;
            _client = new HttpClient();
            _client.DefaultRequestHeaders.Add("User-Agent", "GitHubHelper"); // Required by GitHub API
        }

        public async Task<string> GetLatestReleaseVersion()
        {
            var response = await _client.GetAsync($"https://api.github.com/repos/{_owner}/{_repo}/releases");
            response.EnsureSuccessStatusCode();
            var json = await response.Content.ReadAsStringAsync();
            var result = JsonConvert.DeserializeObject<dynamic>(json);
            return result[0].tag_name;
        }

        public string getVersion()
        {
            string ver = "v2.1.0BETA";
            return ver;
        }

        public async Task<string> checkUpdate()
        {
            var latestVersion = await GetLatestReleaseVersion();
            string update = latestVersion != getVersion() ? $"An update is available ({latestVersion})" : "No updates available";
            if (latestVersion != getVersion())
            {
                Console.WriteLine();
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("=======================================");
                Console.WriteLine("|            UPDATE AVAILABLE         |");
                Console.WriteLine("=======================================");
                Console.ResetColor();
                Console.WriteLine($"New version: {latestVersion}");
                Console.WriteLine();
                Console.WriteLine("Press any key to continue...");
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("No updates available");
                Console.ResetColor();
                Console.WriteLine("Press any key to continue...");
            }
            return update;
        }
    }
}
