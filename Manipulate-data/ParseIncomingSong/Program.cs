using Newtonsoft.Json;
using System.IO;
using System.Linq;

namespace ParseIncomingSong
{
    class Program
    {
        static void Main(string[] args)
        {
            var input = "melody.json";
            var data = JsonLoader.LoadSong(input);

            var notes = data.Where(r => r.Notes.Count() > 1).SelectMany(m => m.Notes).ToList();
            for (int i = 0; i < notes.Count; i++)
            {
                if (i % 2 == 0)
                {
                    notes[i].Note = 0;
                }
            }

            var output = "melody-out.json";
            var json = JsonConvert.SerializeObject(data, Formatting.Indented);
            File.WriteAllText(output, json);
        }
    }
}
