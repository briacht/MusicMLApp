using IntelligentDemo.Models;
using Microsoft.Azure.CognitiveServices.Language.TextAnalytics;
using Microsoft.Azure.CognitiveServices.Language.TextAnalytics.Models;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace IntelligentDemo.Services
{
    public class FeedbackService
    {
        private TextAnalyticsAPI _textAnalyticsAPI;

        public FeedbackService()
        {
            _textAnalyticsAPI = new TextAnalyticsAPI
            {
                AzureRegion = AzureRegions.Westus,
                SubscriptionKey = App.Secrets.TextAnalyticsKey
            };
        }

        public async Task<IEnumerable<Feedback>> GetFeedback()
        {
            var data = await LoadFeedback();

            var inputs = data.Select(f => new MultiLanguageInput("en", f.Key, f.Value.Text)).ToList();

            var result = await _textAnalyticsAPI.SentimentAsync(new MultiLanguageBatchInput(inputs));

            foreach (var document in result.Documents)
            {
                data[document.Id].Score = document.Score;
            }

            return data.Values;
        }

        private async Task<Dictionary<string, Feedback>> LoadFeedback()
        {
            var data = await Task.Run(() => File.ReadAllText("Feedback.json"));
            var result = JsonConvert.DeserializeObject<Feedback[]>(data);

            for (int i = 0; i < result.Length; i++)
            {
                result[i].Id = i.ToString();
            }

            return result.ToDictionary(f => f.Id);
        }
    }
}
