using AlbedoBot.Core;
using Newtonsoft.Json;
using System.IO;
using System.Threading.Tasks;

namespace AlbedoBot.Services
{
    public class ConfigService
    {
        public static string Path { get; set; }
        public static Config Config { get; set; }

        public ConfigService()
        {
            Path = "config.json";
        }

        public async Task InitializeAsync()
        {
            if (!File.Exists(Path))
            {
                await CreateFile();
            }
            var data = await File.ReadAllTextAsync(Path);
            Config = JsonConvert.DeserializeObject<Config>(data);
        }

        private Task CreateFile()
        {
            var config = new Config
            {
                Token = "",
                Prefix = "!",
                GameStatus = ""
            };

            var data = JsonConvert.SerializeObject(config);
            File.WriteAllText(Path, data);
            return Task.CompletedTask;
        }
    }
}
