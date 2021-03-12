using IntelligentDemo.Services;
using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;
using Tweetinvi;
using Tweetinvi.Models;
using Tweetinvi.Parameters;
using Tweetinvi.Streaming;

namespace IntelligentDemo.Pages
{
    /// <summary>
    /// Interaction logic for TwitterPage.xaml
    /// </summary>
    public partial class TwitterPage : UserControl, IDisposable
    {
        private const double DEFAULT_VOLUME = 0.75;
        private const string HASHTAG = "#mlnetdemo";

        private ConcurrentQueue<Models.Tweet> _tweetQueue = new ConcurrentQueue<Models.Tweet>();
        private DispatcherTimer _tweetQueueTimer;

        private PercussionGenerator _percussionGenerator = new PercussionGenerator();
        private EmotionService _emotionService = new EmotionService();
        private SongController _songController;

        private Task _streamTask;
        private int? _nextIndex;
        bool processingAutoMove;
        bool playing;
        bool initialized;

        public TwitterPage(SongController controller)
        {
            InitializeComponent();
            _songController = controller;
        }

        public ObservableCollection<Models.Tweet> Tweets = new ObservableCollection<Models.Tweet>();

        private async void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            if (!initialized)
            {
                initialized = true;
                Auth.SetUserCredentials(App.Secrets.Twitter.ConsumerKey, App.Secrets.Twitter.ConsumerSecret, App.Secrets.Twitter.UserAccessToken, App.Secrets.Twitter.UserAccessSecret);

                DetailsList.ItemsSource = Tweets;
                VolumeSlider.Value = DEFAULT_VOLUME * 100;
                HashTagDisplay.Text = HASHTAG;

                if (App.OfflineMode)
                {
                    LoadSampleTweets();
                }
                else
                {
                    await LoadRecentTweets();
                    ConnectToTwitterStream();

                    ProcessTweetQueue();
                    _tweetQueueTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(5) };
                    _tweetQueueTimer.Tick += (_, __) => ProcessTweetQueue();
                    _tweetQueueTimer.Start();
                }

                _songController.BarStarted += Controller_BarStarted;
            }
        }

        private async Task LoadRecentTweets()
        {
            var searchParameter = new SearchTweetsParameters(HASHTAG)
            {
                SearchType = SearchResultType.Popular,
                MaximumNumberOfResults = 10,
                Filters = TweetSearchFilters.Images
            };

            var search = Search.SearchTweets(HASHTAG);

            // TODO null when no Internet
            if (search != null)
            {
                foreach (var tweet in search.Take(20))
                {
                    await ProcessIncomingTweet(tweet);
                }
            }
        }

        private void LoadSampleTweets()
        {
            var json = System.IO.File.ReadAllText("Offline/OfflineTweets.json");
            var tweets = JsonConvert.DeserializeObject<Models.Tweet[]>(json);
            foreach (var tweet in tweets)
            {
                tweet.ImageUrl = System.IO.Path.GetFullPath(tweet.ImageUrl);
                Tweets.Add(tweet);
            }
        }

        private IFilteredStream _twitterStream;
        private async Task ConnectToTwitterStream()
        {
            var stream = Stream.CreateFilteredStream();
            stream.AddTrack(HASHTAG);
            stream.MatchingTweetReceived += async (s, e) => await ProcessIncomingTweet(e.Tweet);
            _twitterStream = stream;
            await stream.StartStreamMatchingAllConditionsAsync();
        }

        private async Task ProcessIncomingTweet(ITweet tweet)
        {
            if (tweet.Media.Count != 0 && tweet.Media[0].MediaType == "photo" && tweet.IsRetweet == false && tweet.InReplyToUserId == null)
            {
                var imgUrl = tweet.Entities.Medias[0].MediaURL;
                var emotion = await _emotionService.DetectEmotionFromUrl(imgUrl);

                var result = new Models.Tweet
                {
                    ImageUrl = imgUrl,
                    Emotion = emotion,
                    Text = tweet.Text,
                    Handle = tweet.CreatedBy.ScreenName,
                    Name = tweet.CreatedBy.Name
                };

                _tweetQueue.Enqueue(result);
            }
        }

        private void ProcessTweetQueue()
        {
            // Don't take too many, so we don't go over Congnitive quota
            Models.Tweet tweet;
            for (int i = 0; i < 5; i++)
            {
                if(_tweetQueue.TryDequeue(out tweet))
                {
                    Tweets.Add(tweet);
                }
            }
        }

        private void DetailsList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count > 0)
            {
                DetailsList.ScrollIntoView(e.AddedItems[0]);

                if (playing && !processingAutoMove)
                {
                    SetNext((DetailsList.SelectedIndex));
                }
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

                if (e.BarNumber % 2 == 0 && DetailsList.Items.Count > 0)
                {
                    SetNext((DetailsList.SelectedIndex + 1) % DetailsList.Items.Count);
                }
            }
        }

        private void SetNext(int index)
        {
            _nextIndex = index;

            var next = Tweets[_nextIndex.Value];
            var measure = _percussionGenerator.GetPercussionMeasure(next.Emotion);
            _songController.SetNextPercussionBar(measure);
        }

        private void Slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            _songController?.SetPercussionVolume(e.NewValue / 100);
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
                _songController.SetNextPercussionBar(null);
                PlayButton.Background = new SolidColorBrush(Color.FromRgb(0x66, 0x66, 0x66));
            }
        }

        public void Dispose()
        {
            _streamTask?.Dispose();
        }
    }
}
