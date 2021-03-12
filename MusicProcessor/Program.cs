using IntelligentDemo.Models;
using IntelligentDemo.Services;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;

namespace MusicProcessor
{
    class Program
    {
        static void Main(string[] args)
        {
            var path = args[0];

            var json = File.ReadAllText(path);
            var song = JsonConvert.DeserializeObject<List<MusicMeasure>>(json);

            var repairer = new MusicRepairer("../../../../IntelligentDemo/Services/MusicModel.zip");
            repairer.Repair(song);

            json = JsonConvert.SerializeObject(song);
            File.WriteAllText(path, json);
        }
    }
}
