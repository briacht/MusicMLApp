using IntelligentDemo.Models;
using System.Collections.Generic;
using System.Linq;

namespace IntelligentDemo.Services
{
    public class BassGenerator
    {
        private Dictionary<string, IEnumerable<MusicNote>> _lines = InitializeBassLines();
        private int _count;

        public IEnumerable<MusicNote> GetBassMeasure(string emotion)
        {
            var line = _lines.ContainsKey(emotion.ToLower())
                ? _lines[emotion.ToLower()]
                : _lines.ElementAt(_count % _lines.Count).Value;

            _count++;

            return line;
        }

        private static Dictionary<string, IEnumerable<MusicNote>> InitializeBassLines()
        {
            var result = new Dictionary<string, IEnumerable<MusicNote>>();

            result["anger"] = new List<MusicNote>
            {
                new MusicNote{ Note = 36, Duration = 8, Velocity = 127, Position = 1},
                new MusicNote{ Note = 36, Duration = 8, Velocity = 127, Position = 9},
            };

            result["contempt"] = new List<MusicNote>
            {
                new MusicNote{ Note = 36, Duration = 8, Velocity = 127, Position = 1},
                new MusicNote{ Note = 36, Duration = 8, Velocity = 127, Position = 9},
            };

            result["disgust"] = new List<MusicNote>
            {
                new MusicNote{ Note = 40, Duration = 4, Velocity = 127, Position = 1},
                new MusicNote{ Note = 38, Duration = 4, Velocity = 127, Position = 5},
                new MusicNote{ Note = 36, Duration = 8, Velocity = 127, Position = 9},
            };

            result["fear"] = new List<MusicNote>
            {
                new MusicNote{ Note = 40, Duration = 2, Velocity = 100, Position = 1},
                new MusicNote{ Note = 40, Duration = 2, Velocity = 100, Position = 3},
                new MusicNote{ Note = 40, Duration = 2, Velocity = 100, Position = 5},
                new MusicNote{ Note = 40, Duration = 2, Velocity = 100, Position = 7},
                new MusicNote{ Note = 40, Duration = 2, Velocity = 100, Position = 9},
                new MusicNote{ Note = 40, Duration = 2, Velocity = 100, Position = 11},
                new MusicNote{ Note = 40, Duration = 2, Velocity = 100, Position = 13},
                new MusicNote{ Note = 40, Duration = 2, Velocity = 100, Position = 15},
            };

            result["happiness"] = new List<MusicNote>
            {
                new MusicNote{ Note = 36, Duration = 4, Velocity = 127, Position = 1},
                new MusicNote{ Note = 38, Duration = 4, Velocity = 127, Position = 5},
                new MusicNote{ Note = 38, Duration = 4, Velocity = 127, Position = 9},
                new MusicNote{ Note = 40, Duration = 4, Velocity = 127, Position = 13},
            };

            result["neutral"] = new List<MusicNote>
            {
                new MusicNote{ Note = 36, Duration = 8, Velocity = 127, Position = 1},
                new MusicNote{ Note = 36, Duration = 8, Velocity = 127, Position = 9},
            };

            result["sadness"] = new List<MusicNote>
            {
                new MusicNote{ Note = 38, Duration = 8, Velocity = 100, Position = 1},
                new MusicNote{ Note = 36, Duration = 8, Velocity = 100, Position = 9},
            };

            result["surprise"] = new List<MusicNote>
            {
                new MusicNote{ Note = 36, Duration = 4, Velocity = 127, Position = 1},
                new MusicNote{ Note = 36, Duration = 4, Velocity = 127, Position = 5},
                new MusicNote{ Note = 36, Duration = 4, Velocity = 127, Position = 9},
                new MusicNote{ Note = 40, Duration = 4, Velocity = 127, Position = 13},
            };

            return result;
        }
    }
}
