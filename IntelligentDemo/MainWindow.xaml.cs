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

        public MainWindow()
        {
            InitializeComponent();

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

        
        private void TogglePage(UIElement page)
        {
            if (_displayed.Contains(page))
            {
                _displayed.Remove(page);
                Redraw();
            }
            else
            {
                _displayed.Add(page);
                Redraw();
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
