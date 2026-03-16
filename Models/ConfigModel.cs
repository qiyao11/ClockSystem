using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace ClockSystem.Models
{
    public class ConfigModel
    {
        public PathConfig Path { get; set; } = new PathConfig();
        public SwitchConfig Switch { get; set; } = new SwitchConfig();
        public LightConfig Light { get; set; } = new LightConfig();
    }

    public class PathConfig
    {
        public string MusicPath { get; set; } = "Resources/music.mp3";
        public string BellPath { get; set; } = "Resources/bell.mp3";
        public ObservableCollection<KeyValuePair<int, string>> HourPaths { get; set; } = new ObservableCollection<KeyValuePair<int, string>>();
        public ObservableCollection<KeyValuePair<int, string>> HalfPaths { get; set; } = new ObservableCollection<KeyValuePair<int, string>>();

        public PathConfig()
        {
            // 初始化24小时的路径
            for (int i = 0; i < 24; i++)
            {
                HourPaths.Add(new ClockSystem.Models.KeyValuePair<int, string>(i, $"Resources/hour_{i}.mp3"));
                HalfPaths.Add(new ClockSystem.Models.KeyValuePair<int, string>(i, $"Resources/half_{i}.mp3"));
            }
        }
    }

    public class SwitchConfig
    {
        public ObservableCollection<KeyValuePair<int, bool>> HourSwitches { get; set; } = new ObservableCollection<KeyValuePair<int, bool>>();
        public ObservableCollection<KeyValuePair<int, bool>> HalfSwitches { get; set; } = new ObservableCollection<KeyValuePair<int, bool>>();

        public SwitchConfig()
        {
            // 初始化24小时的开关，默认6-21点开启
            for (int i = 0; i < 24; i++)
            {
                HourSwitches.Add(new ClockSystem.Models.KeyValuePair<int, bool>(i, i >= 6 && i <= 21));
                HalfSwitches.Add(new ClockSystem.Models.KeyValuePair<int, bool>(i, i >= 6 && i <= 21));
            }
        }
    }

    public class LightConfig
    {
        public string LightOnTime { get; set; } = "18:00";
        public string LightOffTime { get; set; } = "06:00";
    }

    public class KeyValuePair<TKey, TValue>
    {
        public TKey Key { get; set; }
        public TValue Value { get; set; }

        public KeyValuePair(TKey key, TValue value)
        {
            Key = key;
            Value = value;
        }
    }
}