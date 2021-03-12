using IntelligentDemo.Models;
using IntelligentDemo.Services;
using Microsoft.Expression.Encoder.Devices;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace IntelligentDemo.Pages
{
    public partial class CameraPage : UserControl
    {
        private const double DEFAULT_VOLUME = 0.5;

        private BassGenerator _bassLineGenerator = new BassGenerator();
        private EmotionService _emotionService = new EmotionService();
        private SongController _songController;
        private int? _nextIndex;
        private int _offlineCount;
        bool processingAutoMove;
        bool playing;
        bool initialized;

        public CameraPage(SongController controller)
        {
            InitializeComponent();

            _songController = controller;
        }

        public ObservableCollection<FeedbackViewModel> Images { get; set; } = new ObservableCollection<FeedbackViewModel>();

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
           if(!initialized)
            {
                initialized = true;

                var cam = EncoderDevices.FindDevices(EncoderDeviceType.Video).Last();
                WebcamViewer.VideoDevice = cam;
                WebcamViewer.ImageDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "VideoCaptures");
                WebcamViewer.StartPreview();

                DetailsList.ItemsSource = Images;
                VolumeSlider.Value = DEFAULT_VOLUME * 100;

                _songController.BarStarted += Controller_BarStarted;
            }
        }

        private void Controller_BarStarted(object sender, BarStartedEventArgs e)
        {
            if (playing)
            {
                if (e.BarNumber % 2 == 1)
                {
                    if (_nextIndex != null)
                    {
                        processingAutoMove = true;
                        DetailsList.SelectedIndex = _nextIndex.Value;
                        _nextIndex = null;
                        processingAutoMove = false;
                    }
                }

                if (e.BarNumber % 2 == 0 && Images.Any())
                {
                    SetNext((DetailsList.SelectedIndex + 1) % DetailsList.Items.Count);
                }
            }
        }

        private void SetNext(int index)
        {
            _nextIndex = index;
            var next = Images[_nextIndex.Value];
            _songController.SetNextBassBar(_bassLineGenerator.GetBassMeasure(next.Emotion));
        }

        private void DetailsList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count > 0)
            {
                DetailsList.ScrollIntoView(e.AddedItems[0]);

                if (playing && !processingAutoMove)
                {
                    SetNext(DetailsList.Items.IndexOf(e.AddedItems[0]));
                }
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

                    SetNext(DetailsList.SelectedIndex);
                }

                playing = true;
                PlayButton.Background = new SolidColorBrush(Color.FromRgb(0x10, 0x7c, 0x10));

                _songController.Start();
            }
            else
            {
                playing = false;
                _songController.SetNextBassBar(null);
                PlayButton.Background = new SolidColorBrush(Color.FromRgb(0x66, 0x66, 0x66));
            }
        }

        private async void Capture_Click(object sender, RoutedEventArgs e)
        {
            CaptureButton.IsEnabled = false;
            CaptureLight.Fill = new SolidColorBrush(Colors.Red);
            CaptureLight.Visibility = Visibility.Visible;
            await Task.Delay(200);
            CaptureLight.Visibility = Visibility.Hidden;
            await Task.Delay(200);
            CaptureLight.Visibility = Visibility.Visible;
            await Task.Delay(200);
            CaptureLight.Visibility = Visibility.Hidden;
            await Task.Delay(200);
            CaptureLight.Fill = new SolidColorBrush(Colors.White);
            CaptureLight.Visibility = Visibility.Visible;

            var path = WebcamViewer.TakeSnapshot();
            var img = CropImage(path);
            var bmp = new BitmapImage(new Uri(img));

            var result = new FeedbackViewModel { Emotion = "Analyzing...", Image = bmp };
            if (App.OfflineMode)
            {
                var offline = new string[] { "Happiness", "Surprise", "Anger", "Sadness" };
                result.Emotion = offline[_offlineCount % offline.Length];
                _offlineCount++;
            }
            else
            {
                result.Emotion = await _emotionService.DetectEmotionFromFile(path);
            }
            Images.Add(result);

            CaptureLight.Visibility = Visibility.Hidden;
            CaptureButton.IsEnabled = true;
        }

        private static string CropImage(string filePath)
        {
            var snap = new BitmapImage(new Uri(filePath));
            var dimension = Convert.ToInt32(snap.Width < snap.Height ? snap.Width : snap.Height);

            Int32Rect rect;
            if (snap.Width < snap.Height * 1.33)
            {
                // Padding is on top/bottom
                var width = Convert.ToInt32(snap.Width);
                var height = Convert.ToInt32(snap.Width * 0.75);

                var x = 0;
                var y = (Convert.ToInt32(snap.Height) - height) / 2;

                rect = new Int32Rect(x, y, width, height);
            }
            else
            {
                // Padding is on sides
                var width = Convert.ToInt32(snap.Height * 1.33);
                var height = Convert.ToInt32(snap.Height);

                var x = (Convert.ToInt32(snap.Width) - width) / 2;
                var y = 0;

                rect = new Int32Rect(x, y, width, height);
            }

            var crop = new CroppedBitmap(snap, rect);

            BitmapEncoder encoder = new JpegBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(crop));

            var path = filePath.Replace(".Jpeg", "_cropped.Jpeg");

            using (var fileStream = new FileStream(path, FileMode.Create))
            {
                encoder.Save(fileStream);
            }

            return path;
        }

        private void Slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            _songController?.SetBassVolume(e.NewValue / 100);
        }

        public class FeedbackViewModel : INotifyPropertyChanged
        {
            private ImageSource _image;
            private string _emotion;
            private bool _playing;

            public ImageSource Image
            {
                get { return _image; }
                set
                {
                    _image = value;
                    NotifyPropertyChanged();
                }
            }

            public string Emotion
            {
                get { return _emotion; }
                set
                {
                    _emotion = value;
                    NotifyPropertyChanged();
                }
            }

            public bool Playing
            {
                get { return _playing; }
                set
                {
                    _playing = value;
                    NotifyPropertyChanged();
                    NotifyPropertyChanged(nameof(Background));
                }
            }

            public Brush Background
            {
                get
                {
                    return _playing
                        ? new SolidColorBrush(Color.FromRgb(0, 255, 0))
                        : new SolidColorBrush(Color.FromRgb(255, 255, 255));
                }
            }

            public event PropertyChangedEventHandler PropertyChanged;

            private void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}
