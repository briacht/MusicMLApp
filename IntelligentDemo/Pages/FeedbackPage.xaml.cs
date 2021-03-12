using IntelligentDemo.Convertors;
using IntelligentDemo.Models;
using IntelligentDemo.Services;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace IntelligentDemo.Pages
{
    public partial class FeedbackPage : UserControl, IDisposable
    {
        private LightController _lightController = new LightController();
        private FeedbackService _feedbackService = new FeedbackService();
        private SongController _songController;
        bool playing;
        bool enabled;
        bool initialized;

        public FeedbackPage(SongController controller)
        {
            _songController = controller;

            InitializeComponent();
        }

        private async void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            if (!initialized)
            {
                initialized = true;
                if (App.OfflineMode)
                {
                    var json = File.ReadAllText("Offline/OfflineFeedback.json");
                    var data = JsonConvert.DeserializeObject<Feedback[]>(json);
                    DetailsList.ItemsSource = data;
                }
                else
                {
                    try
                    {

                        var data = await _feedbackService.GetFeedback();
                        DetailsList.ItemsSource = data;
                    }
                    catch (Exception)
                    {
                        Message.Text = "Failed to connect to service";
                    }
                }

                _songController.BarStarted += SongController_BarStarted;
            }
        }

        private void SongController_BarStarted(object sender, BarStartedEventArgs e)
        {
            if (playing)
            {
                if (e.BarNumber > 1 && e.BarNumber % 4 == 1)
                {
                    DetailsList.SelectedIndex = (DetailsList.SelectedIndex + 1) % DetailsList.Items.Count;
                }
            }
        }

        public void Dispose()
        {
            _lightController.Dispose();
        }

        private void DetailsList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (enabled)
            {
                SetColorForCurrent();
            }
        }

        private void Play_Click(object sender, RoutedEventArgs e)
        {
            if (!playing)
            {
                if (DetailsList.Items.Count > 0)
                {
                    if (DetailsList.SelectedIndex < 0)
                    {
                        DetailsList.SelectedIndex = 0;
                    }

                    if (enabled)
                    {
                        SetColorForCurrent();
                    }
                }

                playing = true;
                PlayButton.Background = new SolidColorBrush(Color.FromRgb(0x10, 0x7c, 0x10));
                _songController.Start();
            }
            else
            {
                playing = false;
                PlayButton.Background = new SolidColorBrush(Color.FromRgb(0x66, 0x66, 0x66));
            }
        }

        private void Enable_Click(object sender, RoutedEventArgs e)
        {
            if(!enabled)
            {
                enabled = true;
                SetColorForCurrent();
                EnableButton.Background = _lightController.IsConnected
                    ? new SolidColorBrush(Color.FromRgb(0x10, 0x7c, 0x10))
                    : new SolidColorBrush(Color.FromRgb(0xd8, 0x3b, 0x01));
            }
            else
            {
                enabled = false;
                EnableButton.Background = new SolidColorBrush(Color.FromRgb(0x66, 0x66, 0x66));
                _lightController.SetColor(Colors.Black);
            }
        }

        private void SetColorForCurrent()
        {
            var item = (Feedback)DetailsList.SelectedItem;
            if (item != null)
            {
                _lightController.SetColor(ScoreToColorConvertor.Convert(item.Score));
                DetailsList.ScrollIntoView(item);
            }
        }
    }
}
