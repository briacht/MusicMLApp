using IntelligentDemo.Models;
using System.Collections.Generic;
using System.Linq;

namespace IntelligentDemo.Services
{
    public class PercussionGenerator
    {
        private Dictionary<string, IEnumerable<MusicNote>> _lines = InitializePercussionLines();
        private int _count;

        public IEnumerable<MusicNote> GetPercussionMeasure(string emotion)
        {
            var line = _lines.ContainsKey(emotion.ToLower())
                ? _lines[emotion.ToLower()]
                : _lines.ElementAt(_count % _lines.Count).Value;

            _count++;

            return line;
        }

        private static Dictionary<string, IEnumerable<MusicNote>> InitializePercussionLines()
        {
            var result = new Dictionary<string, IEnumerable<MusicNote>>();

            result["anger"] = new List<MusicNote>
            {
                new MusicNote{ Note = 49, Duration = 16, Velocity = 127, Position = 1},
            };

            result["contempt"] = new List<MusicNote>
            {
                new MusicNote{ Note = 41, Duration = 16, Velocity = 127, Position = 1},
                new MusicNote{ Note = 41, Duration = 16, Velocity = 127, Position = 9},
            };

            result["disgust"] = new List<MusicNote>
            {
                new MusicNote{ Note = 37, Duration = 4, Velocity = 127, Position = 1},
                new MusicNote{ Note = 37, Duration = 2, Velocity = 127, Position = 9},
                new MusicNote{ Note = 37, Duration = 2, Velocity = 127, Position = 11},
            };

            result["fear"] = new List<MusicNote>
            {
                new MusicNote{ Note = 37, Duration = 4, Velocity = 127, Position = 1},
                new MusicNote{ Note = 37, Duration = 2, Velocity = 127, Position = 9},
                new MusicNote{ Note = 37, Duration = 2, Velocity = 127, Position = 11},
            };

            result["happiness"] = new List<MusicNote>
            {
                new MusicNote{ Note = 81, Duration = 4, Velocity = 127, Position = 1},
                new MusicNote{ Note = 81, Duration = 2, Velocity = 127, Position = 9},
                new MusicNote{ Note = 81, Duration = 2, Velocity = 127, Position = 11},
            };

            result["neutral"] = new List<MusicNote>
            {
                new MusicNote{ Note = 56, Duration = 4, Velocity = 127, Position = 1},
                new MusicNote{ Note = 56, Duration = 2, Velocity = 127, Position = 9},
                new MusicNote{ Note = 56, Duration = 2, Velocity = 127, Position = 11},
            };

            result["sadness"] = new List<MusicNote>
            {
                new MusicNote{ Note = 76, Duration = 4, Velocity = 127, Position = 1},
                new MusicNote{ Note = 77, Duration = 4, Velocity = 127, Position = 7},
                new MusicNote{ Note = 77, Duration = 4, Velocity = 127, Position = 9},
            };

            result["surprise"] = new List<MusicNote>
            {
                new MusicNote{ Note = 60, Duration = 4, Velocity = 127, Position = 1},
                new MusicNote{ Note = 60, Duration = 2, Velocity = 127, Position = 9},
                new MusicNote{ Note = 61, Duration = 2, Velocity = 127, Position = 11},
            };

            return result;
        }
    }
}
