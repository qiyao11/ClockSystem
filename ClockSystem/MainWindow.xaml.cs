﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿using ClockSystem.ViewModels;
using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace ClockSystem
{
    public partial class MainWindow : Window
    {
        private MainViewModel _viewModel;
        private DispatcherTimer _clockTimer;

        public MainWindow()
        {
            InitializeComponent();
            _viewModel = new MainViewModel();
            DataContext = _viewModel;

            // 订阅ViewModel事件
            _viewModel.OpenTimeAdjustDialog += OpenTimeAdjustDialog;
            _viewModel.OpenSettingsDialog += OpenSettingsDialog;

            // 初始化时钟定时器
            _clockTimer = new DispatcherTimer();
            _clockTimer.Interval = TimeSpan.FromMilliseconds(16);
            _clockTimer.Tick += (s, e) => DrawClock();
            _clockTimer.Start();

            // 订阅日志消息
            _viewModel.LogMessageReceived += (message) =>
            {
                Dispatcher.Invoke(() =>
                {
                    LogTextBlock.Text += message + "\n";
                    // 自动滚动到底部
                    var scrollViewer = FindVisualChild<ScrollViewer>(LogTextBlock.Parent as FrameworkElement);
                    scrollViewer?.ScrollToEnd();
                });
            };
        }

        private void DrawClock()
        {
            if (ClockCanvas == null) return;

            ClockCanvas.Children.Clear();
            var width = ClockCanvas.ActualWidth;
            var height = ClockCanvas.ActualHeight;

            if (width <= 0 || height <= 0) return;

            var size = Math.Min(width, height) - 40; // 增加边距，避免时钟过大
            var centerX = width / 2;
            var centerY = height / 2;

            // 灯光状态
            bool isNight = _viewModel.IsLightOn;

            // 绘制表盘
            var dial = new Ellipse
            {
                Width = size,
                Height = size,
                Fill = isNight ? Brushes.Black : Brushes.White,
                Stroke = isNight ? Brushes.Black : Brushes.White,
                StrokeThickness = 5
            };
            Canvas.SetLeft(dial, centerX - size / 2);
            Canvas.SetTop(dial, centerY - size / 2);
            ClockCanvas.Children.Add(dial);

            var numeralColor = isNight ? Brushes.LimeGreen : Brushes.DarkSlateGray;
            var handColor = isNight ? Brushes.LimeGreen : Brushes.DarkSlateGray;
            var secHandColor = isNight ? Brushes.Red : Brushes.DarkRed;

            // 绘制刻度
            for (int i = 0; i < 12; i++)
            {
                var angle = Math.PI / 6 * i;
                var length = size / 2;
                var tickLength = size / 20;

                var x1 = centerX + Math.Sin(angle) * (length - tickLength);
                var y1 = centerY - Math.Cos(angle) * (length - tickLength);
                var x2 = centerX + Math.Sin(angle) * length;
                var y2 = centerY - Math.Cos(angle) * length;

                var tick = new Line
                {
                    X1 = x1, Y1 = y1, X2 = x2, Y2 = y2,
                    Stroke = numeralColor,
                    StrokeThickness = 3
                };
                ClockCanvas.Children.Add(tick);

                // 绘制数字
                var num = i == 0 ? 12 : i;
                var numX = centerX + Math.Sin(angle) * (length - tickLength * 2);
                var numY = centerY - Math.Cos(angle) * (length - tickLength * 2);

                var text = new TextBlock
                {
                    Text = num.ToString(),
                    FontSize = size / 20,
                    Foreground = numeralColor,
                    FontWeight = FontWeights.Bold,
                    TextAlignment = TextAlignment.Center,
                    Width = size / 10,
                    Height = size / 15
                };
                Canvas.SetLeft(text, numX - size / 20);
                Canvas.SetTop(text, numY - size / 30);
                ClockCanvas.Children.Add(text);
            }

            // 绘制分钟刻度
            for (int i = 0; i < 60; i++)
            {
                if (i % 5 != 0)
                {
                    var angle = Math.PI / 30 * i;
                    var length = size / 2;
                    var tickLength = size / 40;

                    var x1 = centerX + Math.Sin(angle) * (length - tickLength);
                    var y1 = centerY - Math.Cos(angle) * (length - tickLength);
                    var x2 = centerX + Math.Sin(angle) * length;
                    var y2 = centerY - Math.Cos(angle) * length;

                    var tick = new Line
                    {
                        X1 = x1, Y1 = y1, X2 = x2, Y2 = y2,
                        Stroke = numeralColor,
                        StrokeThickness = 1
                    };
                    ClockCanvas.Children.Add(tick);
                }
            }

            // 获取当前时间（使用高精度时间以实现平滑动画）
            var now = _viewModel.HighPrecisionTime;
            var hours = now.Hour % 12;
            var minutes = now.Minute;
            var seconds = now.Second + now.Millisecond / 1000.0;

            // 计算指针角度（使用更高精度的计算）
            var hourAngle = Math.PI / 6 * (hours + minutes / 60.0 + seconds / 3600.0);
            var minuteAngle = Math.PI / 30 * (minutes + seconds / 60.0);
            var secondAngle = Math.PI / 30 * seconds;

            // 绘制时针
            var hourLength = size / 3;
            var hx = centerX + Math.Sin(hourAngle) * hourLength;
            var hy = centerY - Math.Cos(hourAngle) * hourLength;
            var hourHand = new Line
            {
                X1 = centerX, Y1 = centerY, X2 = hx, Y2 = hy,
                Stroke = handColor,
                StrokeThickness = size / 30,
                StrokeEndLineCap = PenLineCap.Round
            };
            ClockCanvas.Children.Add(hourHand);

            // 绘制分针
            var minuteLength = size / 2 - 10;
            var mx = centerX + Math.Sin(minuteAngle) * minuteLength;
            var my = centerY - Math.Cos(minuteAngle) * minuteLength;
            var minuteHand = new Line
            {
                X1 = centerX, Y1 = centerY, X2 = mx, Y2 = my,
                Stroke = handColor,
                StrokeThickness = size / 50,
                StrokeEndLineCap = PenLineCap.Round
            };
            ClockCanvas.Children.Add(minuteHand);

            // 绘制秒针
            var secondLength = size / 2 - 5;
            var sx = centerX + Math.Sin(secondAngle) * secondLength;
            var sy = centerY - Math.Cos(secondAngle) * secondLength;
            var secondHand = new Line
            {
                X1 = centerX, Y1 = centerY, X2 = sx, Y2 = sy,
                Stroke = secHandColor,
                StrokeThickness = size / 100,
                StrokeEndLineCap = PenLineCap.Round
            };
            ClockCanvas.Children.Add(secondHand);

            // 绘制中心圆点
            var centerDot = new Ellipse
            {
                Width = size / 50,
                Height = size / 50,
                Fill = Brushes.Gray
            };
            Canvas.SetLeft(centerDot, centerX - size / 100);
            Canvas.SetTop(centerDot, centerY - size / 100);
            ClockCanvas.Children.Add(centerDot);
        }

        private void OpenTimeAdjustDialog()
        {
            var dialog = new TimeAdjustDialog(_viewModel.MasterTime);
            if (dialog.ShowDialog() == true)
            {
                _viewModel.AdjustTime(dialog.SelectedTime);
            }
        }

        private void OpenSettingsDialog()
        {
            var dialog = new SettingsDialog(_viewModel.LoadConfig());
            if (dialog.ShowDialog() == true)
            {
                _viewModel.SaveConfig(dialog.Config);
            }
        }

        private static T FindVisualChild<T>(DependencyObject parent) where T : DependencyObject
        {
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);
                if (child is T t) return t;
                var foundChild = FindVisualChild<T>(child);
                if (foundChild != null) return foundChild;
            }
            return null;
        }
    }
}