using ClockSystem.Models;
using ClockSystem.Services;
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;

namespace ClockSystem.ViewModels
{
    public class MainViewModel : INotifyPropertyChanged
    {
        private readonly ConfigService _configService;
        private readonly ClockService _clockService;
        private DateTime _masterTime;
        private bool _useSystemTime = true;
        private double _timeOffset = 0;
        private double _highPrecisionOffset = 0;
        private System.Diagnostics.Stopwatch _precisionStopwatch;
        private DateTime _lastSyncTime;
        private string _logText = "";

        public event PropertyChangedEventHandler PropertyChanged;
        public event Action<string> LogMessageReceived;

        public DateTime MasterTime
        {
            get => _masterTime;
            set
            {
                if (_masterTime != value)
                {
                    _masterTime = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(TimeDisplay));
                }
            }
        }

        public DateTime HighPrecisionTime => _masterTime.AddMilliseconds(_highPrecisionOffset);

        public string TimeDisplay => MasterTime.ToString("yyyy-MM-dd HH:mm:ss");

        public string LogText
        {
            get => _logText;
            set
            {
                if (_logText != value)
                {
                    _logText = value;
                    OnPropertyChanged();
                }
            }
        }

        public bool UseSystemTime
        {
            get => _useSystemTime;
            set => _useSystemTime = value;
        }



        private void StartTimeUpdate()
        {
            Task.Run(() =>
            {
                var stopwatch = System.Diagnostics.Stopwatch.StartNew();
                _precisionStopwatch = System.Diagnostics.Stopwatch.StartNew();
                _lastSyncTime = DateTime.Now;
                var lastSecond = -1;
                
                while (true)
                {
                    try
                    {
                        var currentTime = DateTime.Now;
                        var elapsed = stopwatch.Elapsed.TotalMilliseconds;
                        
                        if (UseSystemTime)
                        {
                            MasterTime = currentTime;
                        }
                        else
                        {
                            MasterTime = currentTime.AddSeconds(_timeOffset);
                        }
                        
                        // 更新高精度偏移量（不重置，保持连续性）
                        _highPrecisionOffset = elapsed;
                        
                        // 每秒同步一次基准时间，但不重置 Stopwatch
                        if (currentTime.Second != lastSecond)
                        {
                            lastSecond = currentTime.Second;
                            _lastSyncTime = currentTime;
                        }
                        
                        // 触发更新以实现平滑动画
                        OnPropertyChanged(nameof(HighPrecisionTime));
                        
                        // 使用更精确的延迟控制
                        var targetElapsed = 16;
                        var actualElapsed = stopwatch.Elapsed.TotalMilliseconds;
                        var delay = Math.Max(1, (int)(targetElapsed - actualElapsed));
                        
                        stopwatch.Restart();
                        Thread.Sleep(delay);
                    }
                    catch
                    {
                        // 忽略错误
                    }
                }
            });
        }

        public void SyncSystemTime()
        {
            UseSystemTime = true;
            _timeOffset = 0;
            MasterTime = DateTime.Now;
        }

        public void AdjustTime(DateTime newTime)
        {
            UseSystemTime = false;
            MasterTime = newTime;
            _timeOffset = (newTime - DateTime.Now).TotalSeconds;
        }

        public ConfigModel LoadConfig()
        {
            return _configService.LoadConfig();
        }

        public void SaveConfig(ConfigModel config)
        {
            _configService.SaveConfig(config);
        }

        public bool IsLightOn => _clockService.LightOn;

        // 命令
        public ICommand SyncSystemTimeCommand { get; set; }
        public ICommand OpenTimeAdjustCommand { get; set; }
        public ICommand OpenSettingsCommand { get; set; }

        // 事件
        public event Action OpenTimeAdjustDialog;
        public event Action OpenSettingsDialog;

        public MainViewModel()
        {
            _configService = new ConfigService();
            var audioService = new AudioService();
            _clockService = new ClockService(_configService, audioService);

            _clockService.LogMessage += (message) =>
            {
                LogText += message + "\n";
                LogMessageReceived?.Invoke(message);
            };

            MasterTime = DateTime.Now;
            _clockService.SetTimeProvider(() => MasterTime);
            _clockService.Start();

            StartTimeUpdate();

            // 初始化命令
            SyncSystemTimeCommand = new RelayCommand(_ => SyncSystemTime());
            OpenTimeAdjustCommand = new RelayCommand(_ => OpenTimeAdjustDialog?.Invoke());
            OpenSettingsCommand = new RelayCommand(_ => OpenSettingsDialog?.Invoke());
        }

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        // 简单的命令实现
        public class RelayCommand : ICommand
        {
            private readonly Action<object> _execute;
            private readonly Func<object, bool> _canExecute;

            public RelayCommand(Action<object> execute, Func<object, bool> canExecute = null)
            {
                _execute = execute;
                _canExecute = canExecute;
            }

            public event EventHandler CanExecuteChanged;

            public bool CanExecute(object parameter)
            {
                return _canExecute == null || _canExecute(parameter);
            }

            public void Execute(object parameter)
            {
                _execute(parameter);
            }
        }
    }
}