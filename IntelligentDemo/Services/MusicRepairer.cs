using IntelligentDemo.Models;
using Microsoft.ML;
using Microsoft.ML.Runtime.Api;
using System;
using System.Collections.Generic;
using System.Linq;

namespace IntelligentDemo.Services
{
    public class MusicRepairer
    {
        private string _modelPath;

        public MusicRepairer(string modelPath)
        {
            _modelPath = modelPath;
        }

        public void Repair(List<MusicMeasure> measures)
        {
            var model = LoadModel();

            foreach (var measure in measures)
            {
                foreach (var note in measure.Notes.Where(n => n.Note == 0))
                {
                    var knownNotes = measure.Notes.Where(n => n.Note != 0).Select(n => n.Note);

                    var feature = BuildFeature(knownNotes);

                    var noteName = model.Predict(feature).Note;

                    var newNote = ConvertToNoteNumber(noteName, knownNotes);
                    note.Note = newNote;

                    note.IsRepaired = true;
                }
            }
        }

        private MusicNotes BuildFeature(IEnumerable<byte> knownNotes)
        {
            return new MusicNotes
            {
                Chorale = 1,
                Key = 0,
                N_60 = knownNotes.Contains((byte)60) ? 1 : 0,
                N_61 = knownNotes.Contains((byte)61) ? 1 : 0,
                N_62 = knownNotes.Contains((byte)62) ? 1 : 0,
                N_63 = knownNotes.Contains((byte)63) ? 1 : 0,
                N_64 = knownNotes.Contains((byte)64) ? 1 : 0,
                N_65 = knownNotes.Contains((byte)65) ? 1 : 0,
                N_66 = knownNotes.Contains((byte)66) ? 1 : 0,
                N_67 = knownNotes.Contains((byte)67) ? 1 : 0,
                N_68 = knownNotes.Contains((byte)68) ? 1 : 0,
                N_69 = knownNotes.Contains((byte)69) ? 1 : 0,
                N_70 = knownNotes.Contains((byte)70) ? 1 : 0,
                N_71 = knownNotes.Contains((byte)71) ? 1 : 0,
                N_72 = knownNotes.Contains((byte)72) ? 1 : 0,
                N_73 = knownNotes.Contains((byte)73) ? 1 : 0,
                N_74 = knownNotes.Contains((byte)74) ? 1 : 0,
                N_75 = knownNotes.Contains((byte)75) ? 1 : 0,
                N_76 = knownNotes.Contains((byte)76) ? 1 : 0,
                N_77 = knownNotes.Contains((byte)77) ? 1 : 0,
                N_78 = knownNotes.Contains((byte)78) ? 1 : 0,
                N_79 = knownNotes.Contains((byte)79) ? 1 : 0
            };
        }

        private byte ConvertToNoteNumber(string note, IEnumerable<byte> knownNotes)
        {
            var noteNumber = ConvertNoteNameToNumber(note);
            var adjusted = AdjustToMeasureOctave(noteNumber, knownNotes);
            return (byte)adjusted;
        }

        private int ConvertNoteNameToNumber(string note)
        {
            switch (note)
            {
                case "C":
                    return 0;
                case "C#":
                    return 1;
                case "D":
                    return 2;
                case "D#":
                    return 3;
                case "E":
                    return 4;
                case "F":
                    return 5;
                case "F#":
                    return 6;
                case "G":
                    return 7;
                case "G#":
                    return 8;
                case "A":
                    return 9;
                case "A#":
                    return 10;
                case "B":
                    return 11;
                default:
                    throw new ArgumentException();
            }
        }

        private byte AdjustToMeasureOctave(int note, IEnumerable<byte> knownNotes)
        {
            // Find note within octave that average is in
            var avg = (int)knownNotes.Select(n => Convert.ToInt32(n)).Average();
            var octave = avg / 12;
            var result = octave * 12 + note;

            // Check if the corresponding note in the octave above/below
            // is closer to the average
            if (result - avg > 6)
            {
                result -= 12;
            }
            else if (avg - result > 6)
            {
                result += 12;
            }

            return (byte)result;
        }

        private PredictionModel<MusicNotes, MusicNotesPrediction> LoadModel()
        {
            return PredictionModel.ReadAsync<MusicNotes, MusicNotesPrediction>(_modelPath).Result;
        }

        class MusicNotes
        {
            public float Chorale;
            public float Key;
            public float Measure;
            public string Note;
            public float N_60;
            public float N_61;
            public float N_62;
            public float N_63;
            public float N_64;
            public float N_65;
            public float N_66;
            public float N_67;
            public float N_68;
            public float N_69;
            public float N_70;
            public float N_71;
            public float N_72;
            public float N_73;
            public float N_74;
            public float N_75;
            public float N_76;
            public float N_77;
            public float N_78;
            public float N_79;

            [ColumnName("Label")]
            public string Label;
        }

        public class MusicNotesPrediction
        {
            [ColumnName("PredictedLabel")]
            public string Note;
        }
    }
}
