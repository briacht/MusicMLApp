using IntelligentDemo.Convertors;
using IntelligentDemo.Models;
using PSAMControlLibrary;
using PSAMWPFControlLibrary;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Controls;

namespace IntelligentDemo.ViewModels
{
    public class MeasureViewModel
    {
        public MeasureViewModel(MusicMeasure measure)
        {
            Measure = measure;

            var viewer = BuildVisualMeasure(measure);
            Width = viewer.Width;
            var grid = new Grid { Width = Width, Height = 100 };
            grid.Children.Add(viewer.Viewer);
            DisplayGrid = grid;
        }

        public MusicMeasure Measure { get; set; }
        public Grid DisplayGrid { get; set; }
        public double Width { get; set; }

        public static IncipitViewerWPF BuildClef(uint beats, uint beatType)
        {
            var result = new IncipitViewerWPF();
            result.AddMusicalSymbol(new Clef(ClefType.GClef, 2));
            result.AddMusicalSymbol(new TimeSignature(TimeSignatureType.Numbers, beats, beatType));
            return result;
        }

        private static (IncipitViewerWPF Viewer, int Width) BuildVisualMeasure(MusicMeasure measure)
        {
            var result = new IncipitViewerWPF();
            var symbols = MeasureToPsamSymbolConvertor.Convert(measure);
            foreach (var symbol in symbols)
            {
                result.AddMusicalSymbol(symbol);
            }
            result.AddMusicalSymbol(new Barline());

            return (result, CalculateWidth(symbols));
        }

        private static int CalculateWidth(List<MusicalSymbol> symbols)
        {
            var width = 0;
            foreach (var note in symbols.OfType<Note>())
            {
                switch (note.Duration)
                {
                    case MusicalSymbolDuration.Whole:
                        width += 50;
                        break;
                    case MusicalSymbolDuration.Half:
                        width += 30;
                        break;
                    case MusicalSymbolDuration.Quarter:
                        width += 18;
                        break;
                    case MusicalSymbolDuration.Eighth:
                        width += 15;
                        break;
                    case MusicalSymbolDuration.Sixteenth:
                        width += 14;
                        break;
                    default:
                        width += 14;
                        break;
                }

                if (note.NumberOfDots > 0) width += 16;
                width += note.NumberOfDots * 6;
                width += note.Alter * 9;
            }

            // barline
            width += 17;

            return width;
        }

        //private static int CalculateWidth(MusicMeasure measure)
        //{
        //    var width = 0;
        //    foreach (var note in measure.Notes)
        //    {
        //        switch (note.Duration)
        //        {
        //            case 16:
        //                width += 50;
        //                break;
        //            case 8:
        //                width += 30;
        //                break;
        //            case 6:
        //                width += 36;
        //                break;
        //            case 4:
        //                width += 18;
        //                break;
        //            case 2:
        //                width += 15;
        //                break;
        //            default:
        //                width += 14;
        //                break;
        //        }
        //    }

        //    // barline
        //    width += 17;

        //    return width;
        //}
    }
}
