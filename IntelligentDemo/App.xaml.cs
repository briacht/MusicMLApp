using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Navigation;

namespace IntelligentDemo
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
        }

        public static Secrets Secrets { get; } = Secrets.Load();

        public static bool OfflineMode = false;
    }

    public class Secrets
    {
        public static Secrets Load()
        {
            return JsonConvert.DeserializeObject<Secrets>(File.ReadAllText("secrets.json"));
        }

        public string FaceApiKey { get; set; }
        public string TextAnalyticsKey { get; set; }
        public Twitter Twitter { get; set; }
    }

    public class Twitter
    {
        public string ConsumerKey { get; set; }
        public string ConsumerSecret { get; set; }
        public string UserAccessToken { get; set; }
        public string UserAccessSecret { get; set; }
    }
}
