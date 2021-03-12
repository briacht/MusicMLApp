using IntelligentDemo.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace ParseIncomingSong
{
    public class JsonLoader
    {
        public static List<MusicMeasure> LoadSong(string path)
        {
            var result = LoadJson(path);

            var notes = result.Where(r => r.Notes.Count() > 1).SelectMany(m => m.Notes).ToList();

            for (int i = 0; i < notes.Count; i++)
            {
                if (i % 6 == 0)
                {
                    notes[i].Note = 0;
                }
            }

            return result;
        }

        private static List<MusicMeasure> LoadJson(string fileName)
        {
            string json = File.ReadAllText(fileName);
            var midiData = JsonConvert.DeserializeObject<Rootobject>(json);
            var notes = midiData.tracks[1].notes;

            var measures = notes.Select(n => new { Measure = Math.Floor(n.time / 4), Note = n })
                 .GroupBy(n => n.Measure)
                 .OrderBy(m => m.Key);

            var result = new List<MusicMeasure>();

            foreach (var bar in measures)
            {
                var startTime = bar.Key * 4;

                result.Add(new MusicMeasure
                {
                    Notes = bar.Select(n => new MusicNote
                    {
                        Note = (byte)n.Note.midi,
                        Velocity = 127,
                        Position = Convert.ToByte((n.Note.time - startTime) * 4 + 1),
                        Duration = (byte)(n.Note.duration * 4)
                    }).ToList()
                });
            }

            return result;
        }

        public class Rootobject
        {
            public Header header { get; set; }
            public int startTime { get; set; }
            public float duration { get; set; }
            public Track[] tracks { get; set; }
        }

        public class Header
        {
            public int PPQ { get; set; }
            public int bpm { get; set; }
            public int[] timeSignature { get; set; }
            public string name { get; set; }
        }

        public class Track
        {
            public int startTime { get; set; }
            public float duration { get; set; }
            public int length { get; set; }
            public Note[] notes { get; set; }
            public Controlchanges controlChanges { get; set; }
            public int id { get; set; }
            public string name { get; set; }
            public int instrumentNumber { get; set; }
            public string instrument { get; set; }
            public string instrumentFamily { get; set; }
            public int channelNumber { get; set; }
            public bool isPercussion { get; set; }
        }

        public class Controlchanges
        {
            public _7[] _7 { get; set; }
            public _10[] _10 { get; set; }
            public _32[] _32 { get; set; }
            public _91[] _91 { get; set; }
            public _121[] _121 { get; set; }
        }

        public class _7
        {
            public int number { get; set; }
            public int time { get; set; }
            public float value { get; set; }
        }

        public class _10
        {
            public int number { get; set; }
            public int time { get; set; }
            public float value { get; set; }
        }

        public class _32
        {
            public int number { get; set; }
            public int time { get; set; }
            public int value { get; set; }
        }

        public class _91
        {
            public int number { get; set; }
            public int time { get; set; }
            public float value { get; set; }
        }

        public class _121
        {
            public int number { get; set; }
            public int time { get; set; }
            public int value { get; set; }
        }

        public class Note
        {
            public string name { get; set; }
            public int midi { get; set; }
            public float time { get; set; }
            public float velocity { get; set; }
            public float duration { get; set; }
        }
    }
}
