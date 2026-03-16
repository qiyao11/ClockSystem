using System;
using System.Windows;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace ClockSystem
{
    public partial class TimeAdjustDialog : Window, INotifyPropertyChanged
    {
        private int _year;
        private int _month;
        private int _day;
        private int _hour;
        private int _minute;
        private int _second;

        public event PropertyChangedEventHandler PropertyChanged;

        public int Year
        {
            get => _year;
            set
            {
                _year = value;
                OnPropertyChanged();
            }
        }

        public int Month
        {
            get => _month;
            set
            {
                _month = value;
                OnPropertyChanged();
            }
        }

        public int Day
        {
            get => _day;
            set
            {
                _day = value;
                OnPropertyChanged();
            }
        }

        public int Hour
        {
            get => _hour;
            set
            {
                _hour = value;
                OnPropertyChanged();
            }
        }

        public int Minute
        {
            get => _minute;
            set
            {
                _minute = value;
                OnPropertyChanged();
            }
        }

        public int Second
        {
            get => _second;
            set
            {
                _second = value;
                OnPropertyChanged();
            }
        }

        public DateTime SelectedTime { get; private set; }

        public TimeAdjustDialog(DateTime currentTime)
        {
            InitializeComponent();
            DataContext = this;

            // 初始化当前时间
            Year = currentTime.Year;
            Month = currentTime.Month;
            Day = currentTime.Day;
            Hour = currentTime.Hour;
            Minute = currentTime.Minute;
            Second = currentTime.Second;
        }

        private void ApplyButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                SelectedTime = new DateTime(Year, Month, Day, Hour, Minute, Second);
                DialogResult = true;
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("时间设置错误: " + ex.Message, "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}