using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;

namespace IntelligentDemo.Models.Services
{
    public class MelodyGenerator
    {
        public List<MusicMeasure> GetMelody()
        {
            var json = File.ReadAllText(@"Services/melody-damaged.json");
            var data = JsonConvert.DeserializeObject<List<MusicMeasure>>(json);
            return data;
        }
    }
}
