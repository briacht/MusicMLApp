using IntelligentDemo.Pages;
using IntelligentDemo.Services;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace IntelligentDemo
{
    public partial class MainWindow : Window, IDisposable
    {
        private static Brush brush = new SolidColorBrush(Colors.Black);
        private static Brush background = new SolidColorBrush(Colors.White);
        private static Thickness thickness = new Thickness(1);
        private static CornerRadius radius = new CornerRadius(10);
        private static Thickness margin = new Thickness(10);

        private SongController _controller = new SongController();
        private List<UIElement> _displayed = new List<UIElement>();
        private Lazy<UIElement> _musicPage;
        private Lazy<UIElement> _cameraPage;
        private Lazy<UIElement> _feedbackPage;
        private Lazy<UIElement> _twitterPage;

        public MainWindow()
        {
            InitializeComponent();

            _cameraPage = new Lazy<UIElement>(() => WrapInBorder(new CameraPage(_controller)));
            _feedbackPage = new Lazy<UIElement>(() => WrapInBorder(new FeedbackPage(_controller)));
            _twitterPage = new Lazy<UIElement>(() => WrapInBorder(new TwitterPage(_controller)));
            _musicPage = new Lazy<UIElement>(() => WrapInBorder(new MusicPage(_controller)));
        }

        public void Dispose()
        {
            _controller.Dispose();
        }

        private void Music_Click(object sender, RoutedEventArgs e)
        {
            TogglePage(_musicPage.Value);
        }

        private void Feedback_Click(object sender, RoutedEventArgs e)
        {
            TogglePage(_feedbackPage.Value);

        }

        private void Camera_Click(object sender, RoutedEventArgs e)
        {
            TogglePage(_cameraPage.Value);

        }

        private void Twitter_Click(object sender, RoutedEventArgs e)
        {
            TogglePage(_twitterPage.Value);

        }

        private void SplitScreenCheckbox_Checked(object sender, RoutedEventArgs e)
        {
        }

        private void SplitScreenCheckbox_Unchecked(object sender, RoutedEventArgs e)
        {
            _displayed.Clear();
        }

        private void TogglePage(UIElement page)
        {
            if (_displayed.Contains(page))
            {
                _displayed.Remove(page);
                Redraw();
            }
            else
            {
                if (!SplitScreenCheckbox.IsChecked.GetValueOrDefault())
                {
                    _displayed.Clear();
                }

                if (!_displayed.Contains(page))
                {
                    _displayed.Add(page);
                    Redraw();
                }
            }
        }

        private UIElement WrapInBorder(UserControl page)
        {
            var element = new Border { BorderBrush = brush, BorderThickness = thickness, CornerRadius = radius, Margin = margin, Background = background };
            element.Child = new ContentControl { Content = page };
            return element;
        }

        private void Redraw()
        {
            MainGrid.RowDefinitions.Clear();
            MainGrid.Children.Clear();
            MainGrid.ColumnDefinitions.Clear();

            if (_displayed.Count > 1)
            {
                MainGrid.ColumnDefinitions.Add(new ColumnDefinition());
                MainGrid.ColumnDefinitions.Add(new ColumnDefinition());
            }

            var rows = _displayed.Count / 2 + _displayed.Count % 2;

            for (int i = 0; i < rows; i++)
            {
                MainGrid.RowDefinitions.Add(new RowDefinition());

                var left = _displayed[i * 2];
                left.SetValue(Grid.RowProperty, i);
                left.SetValue(Grid.ColumnProperty, 0);
                MainGrid.Children.Add(left);

                if (i * 2 + 1 < _displayed.Count)
                {
                    var right = _displayed[i * 2 + 1];
                    right.SetValue(Grid.RowProperty, i);
                    right.SetValue(Grid.ColumnProperty, 1);
                    MainGrid.Children.Add(right);
                }
            }
        }
    }
}
