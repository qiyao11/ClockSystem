using ClockSystem.Models;
using System.IO;
using System.Text.Json;

namespace ClockSystem.Services
{
    public class ConfigService
    {
        private readonly string _configPath = "clock.json";

        public ConfigModel LoadConfig()
        {
            if (!File.Exists(_configPath))
            {
                return CreateDefaultConfig();
            }

            try
            {
                var json = File.ReadAllText(_configPath);
                return JsonSerializer.Deserialize<ConfigModel>(json) ?? CreateDefaultConfig();
            }
            catch
            {
                return CreateDefaultConfig();
            }
        }

        public void SaveConfig(ConfigModel config)
        {
            try
            {
                var json = JsonSerializer.Serialize(config, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(_configPath, json);
            }
            catch
            {
                // 保存失败，忽略错误
            }
        }

        private ConfigModel CreateDefaultConfig()
        {
            var config = new ConfigModel();
            SaveConfig(config);
            return config;
        }
    }
}