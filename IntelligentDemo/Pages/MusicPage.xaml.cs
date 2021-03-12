using IntelligentDemo.Models;
using IntelligentDemo.Models.Services;
using IntelligentDemo.Services;
using IntelligentDemo.ViewModels;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace IntelligentDemo.Pages
{
    public partial class MusicPage : UserControl
    {
        private const double DEFAULT_VOLUME = 0.5;

        private SongController _songController;
        private MelodyGenerator _melodyGenerator = new MelodyGenerator();
        private int? _controllerBarWhenPlayStarted;
        private MeasureViewModel[] _measures;
        bool initialized;
        bool playing;

        public MusicPage(SongController controller)
        {
            InitializeComponent();
            _songController = controller;
        }

        private async void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            if (!initialized)
            {
                initialized = true;
                VolumeSlider.Value = DEFAULT_VOLUME * 100;

                var data = _melodyGenerator.GetMelody();

                var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "song.json");
                var json = JsonConvert.SerializeObject(data);
                File.WriteAllText(path, json);

                // Hack: The learner we are using depends on native binaries and they are only
                //       available on x64. But, the webcam and MIDI approach we are using only
                //       work on x86. So we run prediction in a separate process using a json
                //       file to pass the data.
                var startInfo = new ProcessStartInfo()
                {
                    CreateNoWindow = true,
                    UseShellExecute = false,
                    FileName = @"..\..\..\..\MusicProcessor\bin\x64\Debug\MusicProcessor.exe",
                    Arguments = $"\"{path}\"",
                    WindowStyle = ProcessWindowStyle.Hidden,
                };

                await Task.Run(() => Process.Start(startInfo).WaitForExit());

                json = File.ReadAllText(path);
                data = JsonConvert.DeserializeObject<List<MusicMeasure>>(json);

                _measures = data.Select(m => new MeasureViewModel(m)).ToArray();
                Redraw();

                _songController.BarStarted += _songController_BarStarted;
            }
        }

        private void _songController_BarStarted(object sender, BarStartedEventArgs e)
        {
            if (playing)
            {
                if (!_controllerBarWhenPlayStarted.HasValue)
                {
                    _controllerBarWhenPlayStarted = e.BarNumber;
                }


                var current = (e.BarNumber - _controllerBarWhenPlayStarted.Value) % _measures.Length;
                var next = (current + 1) % _measures.Length;
                var previous = current == 0
                    ? _measures.Length - 1
                    : current - 1;

                _measures[previous].DisplayGrid.Background = Brushes.White;
                _measures[current].DisplayGrid.Background = Brushes.Yellow;

                _songController.SetNextMelodyBar(_measures[next].Measure.Notes);
            }
        }

        private void Slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            _songController?.SetMelodyVolume(e.NewValue / 100);
        }

        private void Play_Click(object sender, RoutedEventArgs e)
        {
            if (!playing)
            {
                _songController.SetNextMelodyBar(_measures[0].Measure.Notes);

                playing = true;
                PlayButton.Background = new SolidColorBrush(Color.FromRgb(0x10, 0x7c, 0x10));

                _songController.Start();
            }
            else
            {
                playing = false;
                _songController.SetNextMelodyBar(null);
                _controllerBarWhenPlayStarted = null;
                PlayButton.Background = new SolidColorBrush(Color.FromRgb(0x66, 0x66, 0x66));

                foreach (var item in _measures)
                {
                    item.DisplayGrid.Background = Brushes.White;
                }
            }
        }

        private void Redraw()
        {
            var columns = 7;
            MusicPanel.Children.Clear();

            StackPanel current = null;
            for (int i = 0; i < _measures.Length; i++)
            {
                if (i % (columns - 1) == 0)
                {
                    current = new StackPanel { Orientation = Orientation.Horizontal };
                    MusicPanel.Children.Add(current);

                    var grid = new Grid { Width = 40, Height = 100 };
                    grid.Children.Add(MeasureViewModel.BuildClef(4, 4));
                    current.Children.Add(grid);
                }

                current.Children.Add(_measures[i].DisplayGrid);
            }
        }
    }
}
