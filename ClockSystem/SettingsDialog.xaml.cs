using ClockSystem.Models;
using Microsoft.Win32;
using System.Windows;

namespace ClockSystem
{
    public partial class SettingsDialog : Window
    {
        public ConfigModel Config { get; private set; }

        public SettingsDialog(ConfigModel config)
        {
            InitializeComponent();
            Config = config;
            DataContext = Config;
        }

        private void BrowseMusicButton_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFileDialog
            {
                Filter = "音频文件 (*.mp3;*.wav;*.ogg)|*.mp3;*.wav;*.ogg|所有文件 (*.*)|*.*",
                Title = "选择音乐文件"
            };

            if (dialog.ShowDialog() == true)
            {
                Config.Path.MusicPath = dialog.FileName;
            }
        }

        private void BrowseBellButton_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFileDialog
            {
                Filter = "音频文件 (*.mp3;*.wav;*.ogg)|*.mp3;*.wav;*.ogg|所有文件 (*.*)|*.*",
                Title = "选择钟声文件"
            };

            if (dialog.ShowDialog() == true)
            {
                Config.Path.BellPath = dialog.FileName;
            }
        }

        private void BrowseHourButton_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as System.Windows.Controls.Button;
            if (button != null && int.TryParse(button.Tag.ToString(), out int hour))
            {
                var dialog = new OpenFileDialog
                {
                    Filter = "音频文件 (*.mp3;*.wav;*.ogg)|*.mp3;*.wav;*.ogg|所有文件 (*.*)|*.*",
                    Title = $"选择{hour}点报时语音文件"
                };

                if (dialog.ShowDialog() == true)
                {
                    for (int i = 0; i < Config.Path.HourPaths.Count; i++)
                    {
                        if (Config.Path.HourPaths[i].Key == hour)
                        {
                            Config.Path.HourPaths[i] = new ClockSystem.Models.KeyValuePair<int, string>(hour, dialog.FileName);
                            break;
                        }
                    }
                }
            }
        }

        private void BrowseHalfButton_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as System.Windows.Controls.Button;
            if (button != null && int.TryParse(button.Tag.ToString(), out int hour))
            {
                var dialog = new OpenFileDialog
                {
                    Filter = "音频文件 (*.mp3;*.wav;*.ogg)|*.mp3;*.wav;*.ogg|所有文件 (*.*)|*.*",
                    Title = $"选择{hour}点半报时语音文件"
                };

                if (dialog.ShowDialog() == true)
                {
                    for (int i = 0; i < Config.Path.HalfPaths.Count; i++)
                    {
                        if (Config.Path.HalfPaths[i].Key == hour)
                        {
                            Config.Path.HalfPaths[i] = new ClockSystem.Models.KeyValuePair<int, string>(hour, dialog.FileName);
                            break;
                        }
                    }
                }
            }
        }

        private void SelectAllHourButton_Click(object sender, RoutedEventArgs e)
        {
            for (int i = 0; i < Config.Switch.HourSwitches.Count; i++)
            {
                var hour = Config.Switch.HourSwitches[i].Key;
                Config.Switch.HourSwitches[i] = new ClockSystem.Models.KeyValuePair<int, bool>(hour, true);
            }
        }

        private void DeselectAllHourButton_Click(object sender, RoutedEventArgs e)
        {
            for (int i = 0; i < Config.Switch.HourSwitches.Count; i++)
            {
                var hour = Config.Switch.HourSwitches[i].Key;
                Config.Switch.HourSwitches[i] = new ClockSystem.Models.KeyValuePair<int, bool>(hour, false);
            }
        }

        private void SelectAllHalfButton_Click(object sender, RoutedEventArgs e)
        {
            for (int i = 0; i < Config.Switch.HalfSwitches.Count; i++)
            {
                var hour = Config.Switch.HalfSwitches[i].Key;
                Config.Switch.HalfSwitches[i] = new ClockSystem.Models.KeyValuePair<int, bool>(hour, true);
            }
        }

        private void DeselectAllHalfButton_Click(object sender, RoutedEventArgs e)
        {
            for (int i = 0; i < Config.Switch.HalfSwitches.Count; i++)
            {
                var hour = Config.Switch.HalfSwitches[i].Key;
                Config.Switch.HalfSwitches[i] = new ClockSystem.Models.KeyValuePair<int, bool>(hour, false);
            }
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}