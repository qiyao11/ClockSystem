using ClockSystem.Models;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace ClockSystem.Services
{
    public class ClockService
    {
        private readonly ConfigService _configService;
        private readonly AudioService _audioService;
        private bool _running = true;
        private Func<DateTime> _timeProvider;
        public bool LightOn { get; private set; } = false;

        // 记录上次报时的小时，避免重复报时
        private int _lastHourAlarm = -1;
        private int _lastHalfAlarm = -1;

        public event Action<string> LogMessage;

        public ClockService(ConfigService configService, AudioService audioService)
        {
            _configService = configService;
            _audioService = audioService;
            _timeProvider = () => DateTime.Now;
        }

        public void SetTimeProvider(Func<DateTime> provider)
        {
            _timeProvider = provider;
        }

        public void Start()
        {
            // 初始化灯光状态，不记录日志
            InitializeLightStatus();
            Task.Run(() => RunClock());
        }

        private void InitializeLightStatus()
        {
            try
            {
                var now = _timeProvider();
                var config = _configService.LoadConfig();
                LightOn = CalculateLightStatus(now, config);
            }
            catch
            {
                // 初始化失败时使用默认状态
                LightOn = false;
            }
        }

        public void Stop()
        {
            _running = false;
        }

        private void RunClock()
        {
            while (_running)
            {
                try
                {
                    var now = _timeProvider();
                    var config = _configService.LoadConfig();

                    CheckLightStatus(now, config);
                    CheckHourAlarm(now, config);
                    CheckHalfAlarm(now, config);

                    Thread.Sleep(1000);
                }
                catch
                {
                    // 忽略错误，继续运行
                }
            }
        }

        private void CheckHourAlarm(DateTime now, ConfigModel config)
        {
            if (now.Minute == 59 && now.Second >= 30)
            {
                var nextHour = (now.Hour + 1) % 24;
                
                // 避免重复报时
                if (_lastHourAlarm == nextHour)
                {
                    return;
                }
                
                bool enabled = false;
                foreach (var item in config.Switch.HourSwitches)
                {
                    if (item.Key == nextHour)
                    {
                        enabled = item.Value;
                        break;
                    }
                }
                
                if (enabled)
                {
                    _lastHourAlarm = nextHour;
                    Task.Run(() => ExecuteHourAlarm(nextHour, config, now));
                }
            }
        }

        private void ExecuteHourAlarm(int hour, ConfigModel config, DateTime now)
        {
            Log("整点报时 - " + hour + "点", now);

            try
            {
                // 播放音乐
                if (!string.IsNullOrEmpty(config.Path.MusicPath))
                {
                    _audioService.PlayAudio(config.Path.MusicPath);
                }

                // 播放钟声
                if (!string.IsNullOrEmpty(config.Path.BellPath))
                {
                    var bellCount = hour % 12;
                    bellCount = bellCount == 0 ? 12 : bellCount;
                    for (int i = 0; i < bellCount; i++)
                    {
                        _audioService.PlayAudio(config.Path.BellPath);
                        Thread.Sleep(500);
                    }
                }

                // 播放整点语音
                string hourPath = null;
                foreach (var item in config.Path.HourPaths)
                {
                    if (item.Key == hour)
                    {
                        hourPath = item.Value;
                        break;
                    }
                }
                if (!string.IsNullOrEmpty(hourPath))
                {
                    _audioService.PlayAudio(hourPath);
                }

                Log("整点报时完成", now);
            }
            catch
            {
                Log("整点报时执行失败", now);
            }
        }

        private void CheckHalfAlarm(DateTime now, ConfigModel config)
        {
            if (now.Minute == 29 && now.Second >= 30)
            {
                var currentHour = (now.Hour + 1) % 24;
                
                // 避免重复报时
                if (_lastHalfAlarm == currentHour)
                {
                    return;
                }
                
                bool enabled = false;
                foreach (var item in config.Switch.HalfSwitches)
                {
                    if (item.Key == currentHour)
                    {
                        enabled = item.Value;
                        break;
                    }
                }
                
                if (enabled)
                {
                    _lastHalfAlarm = currentHour;
                    Task.Run(() => ExecuteHalfAlarm(currentHour, config, now));
                }
            }
        }

        private void ExecuteHalfAlarm(int hour, ConfigModel config, DateTime now)
        {
            Log("半点报时 - " + hour + "点半", now);

            try
            {
                // 播放音乐
                if (!string.IsNullOrEmpty(config.Path.MusicPath))
                {
                    _audioService.PlayAudio(config.Path.MusicPath);
                }

                // 播放钟声
                if (!string.IsNullOrEmpty(config.Path.BellPath))
                {
                    _audioService.PlayAudio(config.Path.BellPath);
                }

                // 播放半点语音
                string halfPath = null;
                foreach (var item in config.Path.HalfPaths)
                {
                    if (item.Key == hour)
                    {
                        halfPath = item.Value;
                        break;
                    }
                }
                if (!string.IsNullOrEmpty(halfPath))
                {
                    _audioService.PlayAudio(halfPath);
                }

                Log("半点报时完成", now);
            }
            catch
            {
                Log("半点报时执行失败", now);
            }
        }

        private void CheckLightStatus(DateTime now, ConfigModel config)
        {
            bool newLightOn = CalculateLightStatus(now, config);

            if (newLightOn != LightOn)
            {
                LightOn = newLightOn;
                Log("钟表灯光 " + (LightOn ? "开启" : "关闭"), now);
            }
        }

        private bool CalculateLightStatus(DateTime now, ConfigModel config)
        {
            var onTime = TimeSpan.Parse(config.Light.LightOnTime);
            var offTime = TimeSpan.Parse(config.Light.LightOffTime);
            var currentTime = new TimeSpan(now.Hour, now.Minute, now.Second);

            if (onTime < offTime)
            {
                return currentTime >= onTime && currentTime < offTime;
            }
            else
            {
                return currentTime >= onTime || currentTime < offTime;
            }
        }

        private void Log(string message, DateTime? time = null)
        {
            var logTime = time ?? _timeProvider();
            var logMessage = "[" + logTime.ToString("HH:mm:ss") + "] " + message;
            LogMessage?.Invoke(logMessage);
        }
    }
}