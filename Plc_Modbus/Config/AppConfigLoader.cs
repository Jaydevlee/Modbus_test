using System.Text.Json;

namespace Plc_Modbus.Config
{
    public static class AppConfigLoader
    {
        public static AppConfig Load()
        {
            string path = Path.Combine(AppContext.BaseDirectory, "config.json");
            if (!File.Exists(path))
            {
                throw new FileNotFoundException("config.json 파일을 찾을 수 없습니다.", path);
            }

            AppConfig? config = JsonSerializer.Deserialize<AppConfig>(
                File.ReadAllText(path),
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            if (config is null)
            {
                throw new InvalidOperationException("config.json을 읽을 수 없습니다.");
            }

            Validate(config.PlcSettings);
            return config;
        }

        private static void Validate(PlcSettings settings)
        {
            if (string.IsNullOrWhiteSpace(settings.Host))
                throw new InvalidOperationException("PlcSettings.Host가 필요합니다.");
            if (settings.Port is < 1 or > 65535)
                throw new InvalidOperationException("PlcSettings.Port는 1~65535여야 합니다.");
            if (settings.RegisterCount == 0)
                throw new InvalidOperationException("PlcSettings.RegisterCount는 1 이상이어야 합니다.");
            if (settings.PollIntervalMs < 100)
                throw new InvalidOperationException("PollIntervalMs는 100ms 이상이어야 합니다.");
            if (settings.MetricIntervalMs < settings.PollIntervalMs)
                throw new InvalidOperationException("MetricIntervalMs는 PollIntervalMs 이상이어야 합니다.");
        }
    }
}
