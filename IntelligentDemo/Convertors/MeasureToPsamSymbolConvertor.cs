using IntelligentDemo.Models;
using PSAMControlLibrary;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows.Data;

namespace IntelligentDemo.Convertors
{
    public class MeasureToPsamSymbolConvertor : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var measure = value as MusicMeasure;
            if(measure != null)
            {
                List<MusicalSymbol> s = Convert(measure);
                return s;
            }

            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        public static List<MusicalSymbol> Convert(MusicMeasure measure)
        {
            var s = new List<MusicalSymbol>();
            foreach (var note in measure.Notes.OrderBy(n => n.Position))
            {
                var t = TranslateNote(note);
                var direction = note.Note > 71
                    ? NoteStemDirection.Down
                    : NoteStemDirection.Up;

                var duration = TranslateDuration(note);

                var psamNote = new Note(t.Note, t.Alter, t.Octave, duration.Duration, direction, NoteTieType.None, new List<NoteBeamType>() { NoteBeamType.Single });
                psamNote.NumberOfDots = duration.Dots;
                if (note.IsRepaired)
                {
                    psamNote.MusicalCharacterColor = System.Drawing.Color.Red;
                }
                s.Add(psamNote);
            }

            return s;
        }

        private static (string Note, int Alter, int Octave) TranslateNote(MusicNote note)
        {
            var octave = note.Note / 12 - 1;
            switch (note.Note % 12)
            {
                case 0:
                    return ("C", 0, octave);
                case 1:
                    return ("C", 1, octave);
                case 2:
                    return ("D", 0, octave);
                case 3:
                    return ("D", 1, octave);
                case 4:
                    return ("E", 0, octave);
                case 5:
                    return ("F", 0, octave);
                case 6:
                    return ("F", 1, octave);
                case 7:
                    return ("G", 0, octave);
                case 8:
                    return ("G", 1, octave);
                case 9:
                    return ("A", 0, octave);
                case 10:
                    return ("A", 1, octave);
                case 11:
                    return ("B", 0, octave);
                default:
                    throw new Exception("Unreachable code!");
            }
        }

        private static (MusicalSymbolDuration Duration, int Dots) TranslateDuration(MusicNote note)
        {
            switch (note.Duration)
            {
                case 1:
                    return (MusicalSymbolDuration.Sixteenth, 0);
                case 2:
                    return (MusicalSymbolDuration.Eighth, 0);
                case 4:
                    return (MusicalSymbolDuration.Quarter, 0);
                case 6:
                    return (MusicalSymbolDuration.Eighth, 1);
                case 8:
                    return (MusicalSymbolDuration.Half, 0);
                case 16:
                    return (MusicalSymbolDuration.Whole, 0);
                default:
                    throw new ArgumentException($"Don't know how to translate {note.Duration}/16ths of a note.");
            }
        }
    }
}
