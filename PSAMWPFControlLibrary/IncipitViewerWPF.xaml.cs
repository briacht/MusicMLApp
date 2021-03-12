/*
Polish System for Archivising Music WPF Control Library (PSAM WPF Control Library)
http://www.archiwistykamuzyczna.pl/index.php?article=download&lang=en#psamcontrollibrary

Copyright (c) 2010, Jacek Salamon
All rights reserved.

Redistribution and use in source and binary forms, with or without modification, 
are permitted provided that the following conditions are met:

* Redistributions of source code must retain the above copyright notice, this list
  of conditions and the following disclaimer.
* Redistributions in binary form must reproduce the above copyright notice, this list
  of conditions and the following disclaimer in the documentation and/or other
  materials provided with the distribution.
* Neither the name of Jacek Salamon nor the names of contributors may be used to
  endorse or promote products derived from this software without specific prior
  written permission.
 
THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY
EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES
OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT
SHALL THE COPYRIGHT HOLDER OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL,
SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT
OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION)
HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR
TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE,
EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE. 

============================================================================================
Fugue Icons
Copyright (C) 2009 Yusuke Kamiyamane. All rights reserved.
The icons are licensed under a Creative Commons Attribution 3.0 license.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.ComponentModel;
using System.Xml;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading;
using PSAMControlLibrary;


namespace PSAMWPFControlLibrary
{
    /// <summary>
    /// Interaction logic for IncipitViewerWPF.xaml
    /// </summary>
    public partial class IncipitViewerWPF : UserControl, IIncipitViewer
    {
        #region Private fields

        private XmlDocument xmlIncipit = new XmlDocument();
        private string shortIncipit;
        private List<MusicalSymbol> incipit = new List<MusicalSymbol>();
        private int incipitID;
        private bool isSelected = false;
        private bool drawOnlySelectionAndButtons = false;
        private bool drawOnParentControl = false;

        #endregion

        #region Properties
        [Category("Incipit viewer properties")]
        [Description("InnerXml of the MusicXml incipit which is loaded into the control.")]
        public string XmlIncipitString { get { return xmlIncipit.InnerXml; } }
        public string ShortIncipit { get { return shortIncipit; } set { shortIncipit = value; } }
        [Category("Incipit viewer properties")]
        [Description("Database ID of currently displayed incipit.")]
        public int IncipitID { get { return incipitID; } set { incipitID = value; } }
        [Category("Incipit viewer properties")]
        [Description("If true the selection box is displayed over the control.")]
        public bool IsSelected { get { return isSelected; } set { isSelected = value; } }
        [Category("Incipit viewer properties")]
        [Description("If true staff and musical symbols are not drawn. Only buttons and selection box is drawn.")]
        public bool DrawOnlySelectionAndButtons { get { return drawOnlySelectionAndButtons; }
            set { drawOnlySelectionAndButtons = value; }
        }
        [Category("Incipit viewer properties")]
        [Description("If true the control will be drawn on its container's surface to avoid clippindrawingContext. Parent control must be created without WS_CLIPCHILDREN parameter.")]
        public bool DrawOnParentControl { get { return drawOnParentControl; } set { drawOnParentControl = value; } }
        public int CountIncipitElements { get { return incipit.Count; } }
        public XmlDocument XmlIncipit { get { return xmlIncipit; } }

        #endregion

        #region Events

        public delegate void PlayExternalMidiPlayerDelegate(IncipitViewerWPF sender);
        public event PlayExternalMidiPlayerDelegate PlayExternalMidiPlayer;
        public void OnPlayExternalMidiPlayer(IncipitViewerWPF sender) { if (PlayExternalMidiPlayer != null) PlayExternalMidiPlayer(sender); }

        #endregion

        #region Constructor

        public IncipitViewerWPF()
        {
            InitializeComponent();

            xmlIncipit.XmlResolver = null;
            if (MusicXmlParser.ParseXml(this))
                buttonParseError.Visibility = Visibility.Hidden;
            else
                buttonParseError.Visibility = Visibility.Visible;
            InvalidateVisual();
        }
        #endregion

        #region Public methods

        public MusicalSymbol IncipitElement(int i)
        {
            if (i > incipit.Count) return null;
            return incipit[i];
        }

        public void LoadFromXmlFile(string fileName)
        {
            StreamReader rd = new StreamReader(fileName);
            try
            {
                xmlIncipit.LoadXml(rd.ReadToEnd());
            }
            catch 
            {
            }
            rd.Close();
            if (MusicXmlParser.ParseXml(this))
                buttonParseError.Visibility = Visibility.Hidden;
            else
                buttonParseError.Visibility = Visibility.Visible;
            InvalidateVisual();
        }

        public void LoadFromXmlString(string xml)
        {
            try
            {
            xmlIncipit.LoadXml(xml);
            }
            catch
            {
               
            }
            if (MusicXmlParser.ParseXml(this))
                buttonParseError.Visibility = Visibility.Hidden;
            else
                buttonParseError.Visibility = Visibility.Visible;
            InvalidateVisual();
        }

        public void AddMusicalSymbol(MusicalSymbol symbol)
        {
            if (incipit == null) return;
            incipit.Add(symbol);
            InvalidateVisual();
        }

        public void RemoveLastMusicalSymbol()
        {
            if (incipit == null) return;
            if (incipit.Count == 0) return;
            incipit.RemoveAt(incipit.Count - 1);
            InvalidateVisual();
        }

        public void ClearMusicalIncipit()
        {
            if (incipit == null) return;
            incipit.Clear();
            InvalidateVisual();
        }

        public int CountMusicalSymbols() { return incipit.Count; }

        public Clef GetCurrentClef()
        {
            Clef currentClef = new Clef(ClefType.GClef, 2);
            foreach (MusicalSymbol symbol in incipit) //Make one pass to determine current clef / Wykonaj jeden przebieg żeby określić bieżący klucz
            {
                if (symbol.Type == MusicalSymbolType.Clef)
                {
                    currentClef = (Clef)symbol;
                }
            }
            return currentClef;
        }




        public string SearchStringValueFromIncipit()
        {
            StringBuilder str = new StringBuilder();
            int countRests = 0;
            int countNotes = 0;
            foreach (MusicalSymbol n in incipit)
            {
                if (n.Type == MusicalSymbolType.Note)
                {
                    if (((Note)n).IsChordElement) continue;
                    if (((Note)n).IsGraceNote) continue;
                    if (((Note)n).Voice > 1) continue; //do not take aditional voices into account / nie uwzględniaj dodatkowych głosów
                    countNotes++;
                    if (((Note)n).Tuplet == TupletType.Start) str.Append("(");
                    str.Append((int)((((Note)n).Duration)));
                    str.Append(((Note)n).Step);
                    if (((Note)n).Alter > 0)
                        for (int i = 0; i < ((Note)n).Alter; i++)
                            str.Append("#");
                    else if (((Note)n).Alter < 0)
                        for (int i = 0; i > ((Note)n).Alter; i--)
                            str.Append("b");


                    for (int i = 0; i < ((Note)n).NumberOfDots; i++)
                        str.Append(".");

                    if (((Note)n).Tuplet == TupletType.Stop) str.Append(")");

                    //Ties / Łuki
                    if ((((Note)n).TieType == NoteTieType.Start) ||
                        (((Note)n).TieType == NoteTieType.StopAndStartAnother))
                        str.Append("+");
                }
                else if (n.Type == MusicalSymbolType.Rest)
                {
                    if (((Rest)n).Voice > 1) continue; //don't take aditional voices into account / nie uwzględniaj dodatkowych głosów
                    countRests++;
                    if ((countNotes == 0) && (countRests == 1))
                    {
                        if (((Rest)n).MultiMeasure > 1) continue; //Ignore multimeasure rest if it is at the beginning / Ignoruj pauzę wielotaktową, jeśli jest na początku
                    }

                    if (((Rest)n).Tuplet == TupletType.Start) str.Append("(");
                    str.Append((int)((((Rest)n).Duration)));
                    str.Append("-");
                    for (int i = 0; i < ((Rest)n).NumberOfDots; i++)
                        str.Append(".");
                    if (((Rest)n).Tuplet == TupletType.Stop) str.Append(")");
                }

            }
            if (str.Length > 253) str = str.Remove(253, str.Length - 253);
            return str.ToString();
        }

        public string MellicContourFromIncipit()
        {
            StringBuilder str = new StringBuilder();
            int lastMidiPitch = -1;
            int currentMidiPitch = 0;
            foreach (MusicalSymbol n in incipit)
            {
                if (n.Type == MusicalSymbolType.Note)
                {
                    if (((Note)n).IsChordElement) continue;
                    if (((Note)n).Voice > 1) continue; //don't take aditional voices into account / nie uwzględniaj dodatkowych głosów
                    int difference;
                    currentMidiPitch = ((Note)n).MidiPitch;
                    //MessageBox.Show(Convert.ToString(currentMidiPitch));
                    if (lastMidiPitch == -1)
                    {
                        lastMidiPitch = currentMidiPitch;
                        continue;
                    }
                    difference = currentMidiPitch - lastMidiPitch;
                    lastMidiPitch = currentMidiPitch;
                    if (difference > 0) str.Append("+");
                    else if (difference < 0) str.Append("-");
                    else if (difference == 0) continue;
                    str.Append(Math.Abs(difference));
                }
            }

            if (str.Length > 253) str = str.Remove(253, str.Length - 253);
            return str.ToString();
        }

        public string RhythmFromIncipit()
        {
            StringBuilder str = new StringBuilder();
            int count = 0;
            int countRests = 0;
            int countNotes = 0;
            foreach (MusicalSymbol n in incipit)
            {

                if (n.Type == MusicalSymbolType.Note)
                {

                    if (((Note)n).IsChordElement) continue;
                    if (((Note)n).IsGraceNote) continue;
                    if (((Note)n).Voice > 1) continue; //don't take aditional voices into account / nie uwzględniaj dodatkowych głosów
                    if (count > 0) str.Append(" ");

                    countNotes++;
                    if (((Note)n).Tuplet == TupletType.Start) str.Append("( ");
                    str.Append((int)((Note)n).Duration);
                    for (int i = 0; i < ((Note)n).NumberOfDots; i++)
                        str.Append(".");

                    //Ties / Łuki
                    if ((((Note)n).TieType == NoteTieType.Start) ||
                        (((Note)n).TieType == NoteTieType.StopAndStartAnother))
                        str.Append(" +");

                    if (((Note)n).Tuplet == TupletType.Stop) str.Append(" )");

                    count++;
                }
                else if (n.Type == MusicalSymbolType.Rest)
                {
                    if (((Rest)n).Voice > 1) continue; //don't take aditional voices into account / nie uwzględniaj dodatkowych głosów
                    countRests++;
                    if ((countNotes == 0) && (countRests == 1))
                    {
                        if (((Rest)n).MultiMeasure > 1)
                        {
                            //Nie robić count++ żeby nie postawiło spacji na początku
                            //w przypadku pauzy wielotaktowej
                            //Do not make count++ in order not to place a space at the beginning in case of multimeasure rest
                            //(I'm not sure what I exactly meant here... ;-) - J. S. )
                            continue;
                        }//Ignore a multimeasure rest it is at the beginning / Ignoruj pauzę wielotaktową, jeśli jest na początku
                    }
                    if (count > 0) str.Append(" ");

                    if (((Rest)n).Tuplet == TupletType.Start) str.Append("( ");

                    str.Append((int)((Rest)n).Duration);
                    for (int i = 0; i < ((Rest)n).NumberOfDots; i++)
                        str.Append(".");

                    if (((Rest)n).Tuplet == TupletType.Start) str.Append(" )");

                    count++;
                }
                else if (n.Type == MusicalSymbolType.Barline)
                {
                    if (count > 0)
                    {
                        str.Append(" ");

                        str.Append("|"); //Do not write a barline if it is at the beginning / Nie stawiaj kreski taktowej jeśli jest na początku
                        count++;
                    }
                }
            }
            if (str.Length > 253) str = str.Remove(253, str.Length - 253);
            return str.ToString();
        }

        public string LyricsFromIncipit()
        {
            StringBuilder[] str = new StringBuilder[4];
            for (int i = 0; i < 4; i++)
            {
                str[i] = new StringBuilder();
            }
            foreach (MusicalSymbol n in incipit)
            {
                if (n.Type == MusicalSymbolType.Note)
                {
                    for (int i = 0; i < 4; i++)
                    {
                        if (((Note)n).Lyrics.Count < i + 1) break;
                        if (((Note)n).LyricTexts.Count < i + 1) break;

                        str[i].Append(((Note)n).LyricTexts[i]);

                        if (((Note)n).Lyrics[i] == LyricsType.End)
                            str[i].Append(" ");

                        if (((Note)n).Lyrics[i] == LyricsType.Single)
                            str[i].Append(" ");
                    }
                }
            }
            string finalStr = "";
            for (int i = 0; i < 4; i++)
            {
                finalStr += str[i].ToString().Trim() + " ";
            }
            if (finalStr.Length > 80) finalStr = finalStr.Remove(78, finalStr.Length - 78);
            return finalStr.Trim();
        }

        public int IncipitFromSearchStringValue(string searchString)
        {
            xmlIncipit = new XmlDocument();
            xmlIncipit.XmlResolver = null; //Important line. What it boils down is not to download DTD from the Internet / WAŻNA LINIA. CHODZI O TO, ŻEBY NIE POBIERAŁ Z NETA DTD
            incipit.Clear();

            int lastUsedDuration = 4; //Last used rhytm value in searchString / Ostatnio użyta w searchStringu wartość rytmiczna
            string notes = searchString;
            string durations = searchString;
            string[] noteArray;
            string[] durationArray;
            for (int i = 0; i < notes.Length; i++)
            {
                if ((notes[i] >= '0') && (notes[i] <= '9'))
                {
                    notes = notes.Remove(i, 1);
                    notes = notes.Insert(i, "x");
                }
            }

            for (int i = 0; i < durations.Length; i++)
            {
                if ((durations[i] < '0') || (durations[i] > '9'))
                {
                    durations = durations.Remove(i, 1);
                    durations = durations.Insert(i, "x");
                }
            }


            noteArray = notes.Split(new char[] { 'x' }, System.StringSplitOptions.RemoveEmptyEntries);
            durationArray = durations.Split(new char[] { 'x' }, System.StringSplitOptions.RemoveEmptyEntries);

            bool nextWillStartATuplet = false;
            bool wasBeamStarted = false;

            //Temporary solution for brackets / Tymczasowe rozwiązanie dla nawiasów
            if (noteArray.Length != 0)
            {
                if (noteArray[0][0] == '(')
                {
                    string[] tmpArray = new string[noteArray.Length - 1];
                    for (int i = 1; i < noteArray.Length; i++)
                    {
                        tmpArray[i - 1] = noteArray[i];
                    }
                    noteArray = tmpArray;
                    nextWillStartATuplet = true;
                }
            }
            //End of temporary solution / koniec tymczasowego rozwiązania


            if (noteArray.Length != durationArray.Length) return 4; //Parse error / Pars eror ;P

            Clef currentClef = new Clef(ClefType.GClef, 2);
            incipit.Add(currentClef);

            NoteTieType lastTieType = NoteTieType.None;
            for (int i = 0; (i < noteArray.Length) && (i < durationArray.Length); i++)
            {
                int tmp;
                char step = 'C';
                MusicalSymbolDuration duration = MusicalSymbolDuration.Quarter;
                int alter = 0;
                int octave = 4;
                int numberOfDots = 0;
                NoteTieType tieType = NoteTieType.None;
                NoteStemDirection stemDirection = NoteStemDirection.Up;
                TupletType tupletType = TupletType.None;
                NoteBeamType beamType = NoteBeamType.Single;
                string note = noteArray[i];
                step = note[0];
                if (wasBeamStarted) beamType = NoteBeamType.Continue;
                if (nextWillStartATuplet)
                {
                    tupletType = TupletType.Start;
                    beamType = NoteBeamType.Start;
                    wasBeamStarted = true;
                    nextWillStartATuplet = false;
                }
                if (note[note.Length - 1] == ')')
                {
                    tupletType = TupletType.Stop;
                    wasBeamStarted = false;
                    beamType = NoteBeamType.End;
                }
                if (note[note.Length - 1] == '(') nextWillStartATuplet = true;


                tmp = Convert.ToInt32(durationArray[i]);
                if (((tmp % 2) != 0) && (tmp != 1)) return 4; //Pars eror ;P
                duration = (MusicalSymbolDuration)tmp;
                if (note.Length > 1)
                {
                    for (int j = 1; j < note.Length; j++)
                    {
                        if (note[j] == '#') alter++;
                        else if (note[j] == 'b') alter--;
                        else if (note[j] == '.') numberOfDots++;
                        else if (note[j] == '+')
                        {

                            if ((lastTieType == NoteTieType.Start) ||
                                (lastTieType == NoteTieType.StopAndStartAnother))
                            {
                                tieType = NoteTieType.StopAndStartAnother;
                            }
                            else if ((lastTieType == NoteTieType.None) ||
                                (lastTieType == NoteTieType.Stop))
                            {

                                tieType = NoteTieType.Start;
                            }

                        }
                    }
                }
                if (tieType == NoteTieType.None)
                {

                    if ((lastTieType == NoteTieType.Start) ||
                    (lastTieType == NoteTieType.StopAndStartAnother))
                    {
                        tieType = NoteTieType.Stop;
                    }
                }

                if (step != '-')
                {
                    if ((step.ToString().ToUpper() == "B") || (octave > 4))
                        stemDirection = NoteStemDirection.Down;
                    if ((int)duration < 8) beamType = NoteBeamType.Single;
                    Note nt = new Note(step.ToString().ToUpper(), alter, octave, duration, stemDirection,
                        NoteTieType.None, new List<NoteBeamType> { beamType });
                    nt.NumberOfDots = numberOfDots;
                    nt.TieType = tieType;
                    lastTieType = tieType;
                    nt.Tuplet = tupletType;
                    nt.CustomStemEndPosition = false;
                    nt.HasNatural = IsNaturalSignNeeded(nt, new PSAMControlLibrary.Key(0));
                    incipit.Add(nt);
                }
                else
                {
                    Rest rt = new Rest(duration);
                    rt.NumberOfDots = numberOfDots;
                    incipit.Add(rt);
                }

                lastUsedDuration = (int)duration;
            }

            return lastUsedDuration;
        }

        public bool IsNaturalSignNeeded(Note n, PSAMControlLibrary.Key k)
        {
            int i = incipit.Count - 1;
            if (incipit.Contains(n)) i = incipit.IndexOf(n);

            if (n.Alter != 0) return false; //If the note is altered it obviously doesn't need a natural / Jeśli nuta jest alterowana, to oczywiście nie potrzebuje kasownika
            if (k.StepToAlter(n.Step) != 0) //If the note is altered by key signature... / Jeśli dźwięk jest alterowany oznaczeniem przykluczowym, to...
            {
                for (; i > 0; i--) //...check if there is a natural sign already in this measure / ...sprawdź czy w tym takcie nie ma już jednego kasownika...
                {
                    if (incipit[i].Type == MusicalSymbolType.Barline) break;
                    if (incipit[i].Type == MusicalSymbolType.Note)
                    {
                        if ((((Note)incipit[i]).Step == n.Step) && (((Note)incipit[i]).HasNatural))
                            return false; //...because if it is so, the natural is not needed... / ...bo jeśli tak jest, to kasownik nie jest potrzebny...
                    }
                }
                return true; //...and if there is no natural sign it means that it is needed. / ...a jeśli nie ma kasownika, to jest potrzebny.
            }

            //Może się też zdarzyć, że nuta nie jest alterowana przez oznaczenie przykluczowe, ale
            //w takcie ten stopien jest alterowany. Sprawdzamy to...
            //It may also happen that the note is not altered by key signature but its step is altered in the measure. Let's check this...
            for (; i > 0; i--)
            {
                if (incipit[i].Type == MusicalSymbolType.Barline) break;
                if (incipit[i].Type == MusicalSymbolType.Note)
                {
                    if ((((Note)incipit[i]).Step == n.Step) && (((Note)incipit[i]).Alter != 0))
                        return true; //...if it is so then the note needs a natural... / ...jeśli tak jest, to nuta potrzebuje kasownika...
                }
            }
            return false; //...in the other case it doesn't need a natural. / ...a jeśli nie, to nie potrzebuje.
        }



        #endregion

        #region Private methods

        private void DrawString(DrawingContext d, string text, Typeface f, Brush b, float xPos, float yPos, float emSize)
        {
            //This function mimics Graphics.DrawString functionality
            d.DrawText(new FormattedText(text, Thread.CurrentThread.CurrentUICulture,
                FlowDirection.LeftToRight, f, emSize, b), new Point(
                    xPos, yPos));

        }

        #endregion

        #region Overridden methods

        protected override void OnRender(DrawingContext drawingContext)
        {
            base.OnRender(drawingContext);

            Pen pen = new Pen(Brushes.Black, 1.0f);
            Pen beamPen = new Pen(Brushes.Black, 2.0f);
            Brush textBrush = new SolidColorBrush(Colors.Black);

            float currentClefPositionY = 0;
            Clef currentClef = new Clef(ClefType.GClef, 2);
            PSAMControlLibrary.Key currentKey = new PSAMControlLibrary.Key(0);
            int currentXPosition = 0;
            int lastXPosition = 0; //for chords / dla akordów
            int lastNoteEndXPosition = 0; //for many voices / dla wielu głosów
            int firstNoteInMeasureXPosition = 0; //for many voices - starting point for all voices / dla wielu głosów - punkt rozpoczęcia wszystkich głosów
            int lastNoteInMeasureEndXPosition = 0; //for many voices - location of the last note in the measure / dla wielu głosów - punkt ostatniej nuty w takcie
            const int paddingTop = 20;
            const int lineSpacing = 6;
            float currentStemEndPositionY = 0;
            int numberOfNotesUnderTuplet = 0;
            List<float> previousStemEndPositionsY = new List<float>();
            float currentStemPositionX = 0;
            List<float> previousStemPositionsX = new List<float>();
            List<Point> beamStartPositionsY = new List<Point>();
            List<Point> beamEndPositionsY = new List<Point>();
            Point tieStartPoint = new Point();
            Point slurStartPoint = new Point();
            int currentVoice = 1;

            int[] lines = new int[5];

            //Draw selection box / Rysuj zaznaczenie:
            if (isSelected)
            {
                drawingContext.DrawRectangle(Brushes.Blue, null, new Rect(0, 0, Width, Height));
            }

            if (drawOnlySelectionAndButtons) return;

            //Draw staff lines / Rysuj pięciolinię
            string staff = MusicalCharacters.Staff5Lines;
            for (int i = 0; i < Width / 10; i++)
                staff = staff + MusicalCharacters.Staff5Lines;

            Point startPoint = new Point(0, paddingTop);
            Point endPoint = new Point(Width, paddingTop);

            for (int i = 0; i < 5; i++)
            {
                drawingContext.DrawLine(pen, startPoint, endPoint);
                lines[i] = paddingTop + i * lineSpacing;
                startPoint.Y += lineSpacing;
                endPoint.Y += lineSpacing;

            }

            try
            {
                currentClefPositionY = lines[4] - 24.4f - (currentClef.Line - 1) * lineSpacing;
                foreach (MusicalSymbol symbol in incipit) //Perform one pass to determine current clef / Wykonaj jeden przebieg żeby określić bieżący klucz
                {
                    if (symbol.Type == MusicalSymbolType.Clef)
                    {
                        currentClef = (Clef)symbol;
                        currentClefPositionY = lines[4] - 24.4f - (((Clef)symbol).Line - 1) * lineSpacing;
                        drawingContext.DrawText(new FormattedText(symbol.MusicalCharacter, Thread.CurrentThread.CurrentUICulture,
                            FlowDirection.LeftToRight, TypeFaces.MusicFont, 27.5f, Brushes.Black), new Point(
                                currentXPosition + 3.5f, currentClefPositionY));
                        currentXPosition += 20;
                        break;
                    }
                }
                int[] alterationsWithinOneBar = new int[7];
                bool firstNoteInIncipit = true;
                int currentMeasure = 0;
                foreach (MusicalSymbol symbol in incipit)
                {
                    if (symbol.Type == MusicalSymbolType.Clef)
                    {
                        if ((((Clef)symbol).ClefPitch == currentClef.ClefPitch) &&
                            (((Clef)symbol).Line == currentClef.Line)) continue;
                        currentClefPositionY = lines[4] - 24.4f - (((Clef)symbol).Line - 1) * lineSpacing;
                        currentClef = (Clef)symbol;
                        drawingContext.DrawText(new FormattedText(symbol.MusicalCharacter, Thread.CurrentThread.CurrentUICulture,
                            FlowDirection.LeftToRight, TypeFaces.MusicFont, 27.5f, Brushes.Black), new Point(
                                currentXPosition + 3.5f, currentClefPositionY));
                        currentXPosition += 20;
                    }
                    else if (symbol.Type == MusicalSymbolType.Key)
                    {
                        currentKey = (PSAMControlLibrary.Key)symbol;
                        float flatOrSharpPositionY = 0;
                        bool jumpFourth = false;
                        int jumpDirection = 1;
                        int octaveShiftSharp = 0; //In G clef sharps (not flats) should be written an octave higher / W kluczu g krzyżyki (bemole nie) powinny być zapisywane o oktawę wyżej
                        if (currentClef.TypeOfClef == ClefType.GClef) octaveShiftSharp = 1;
                        int octaveShiftFlat = 0;
                        if (currentClef.TypeOfClef == ClefType.FClef) octaveShiftFlat = -1;
                        if (currentKey.Fifths > 0)
                        {
                            flatOrSharpPositionY = currentClefPositionY + MusicalSymbol.StepDifference(currentClef,
                            (new Note("F", 0, currentClef.Octave + octaveShiftSharp, MusicalSymbolDuration.Whole, NoteStemDirection.Up,
                                NoteTieType.None, null)))
                            * (lineSpacing / 2);
                            jumpFourth = true;
                            jumpDirection = 1;

                        }
                        else if (currentKey.Fifths < 0)
                        {
                            flatOrSharpPositionY = currentClefPositionY + MusicalSymbol.StepDifference(currentClef,
                            (new Note("B", 0, currentClef.Octave + octaveShiftFlat, MusicalSymbolDuration.Whole, NoteStemDirection.Up,
                                NoteTieType.None, null)))
                            * (lineSpacing / 2);
                            jumpFourth = true;
                            jumpDirection = -1;
                        }
                        for (int i = 0; i < Math.Abs(currentKey.Fifths); i++)
                        {
                            drawingContext.DrawText(new FormattedText(symbol.MusicalCharacter,
                                Thread.CurrentThread.CurrentUICulture, FlowDirection.LeftToRight,
                                TypeFaces.MusicFont, 27.5f, Brushes.Black), new Point(currentXPosition + 3.5f, flatOrSharpPositionY));
                            if (jumpFourth) flatOrSharpPositionY += 3 * 3 * jumpDirection;
                            else flatOrSharpPositionY += 3 * 4 * jumpDirection;
                            jumpFourth = !jumpFourth;
                            jumpDirection *= -1;
                            currentXPosition += 8;
                        }
                        currentXPosition += 10;

                    }
                    else if (symbol.Type == MusicalSymbolType.TimeSignature)
                    {
                        float timeSignaturePositionY = (lines[0] - 11);
                        if (((TimeSignature)symbol).SignatureType == TimeSignatureType.Common)
                            drawingContext.DrawText(new FormattedText(MusicalCharacters.CommonTime,
                                Thread.CurrentThread.CurrentUICulture, FlowDirection.LeftToRight,
                                TypeFaces.MusicFont, 27.5f, Brushes.Black), new Point(currentXPosition + 3.5f, timeSignaturePositionY));
                        else if (((TimeSignature)symbol).SignatureType == TimeSignatureType.Cut)
                            drawingContext.DrawText(new FormattedText(MusicalCharacters.CutTime,
                                Thread.CurrentThread.CurrentUICulture, FlowDirection.LeftToRight,
                                TypeFaces.MusicFont, 27.5f, Brushes.Black), new Point(currentXPosition + 3.5f, timeSignaturePositionY));

                        else
                        {
                            drawingContext.DrawText(new FormattedText(Convert.ToString(((TimeSignature)symbol).NumberOfBeats),
                                Thread.CurrentThread.CurrentUICulture, FlowDirection.LeftToRight,
                                TypeFaces.TimeSignatureFont, 14.5, Brushes.Black), new Point(currentXPosition, timeSignaturePositionY + 9));
                            drawingContext.DrawText(new FormattedText(Convert.ToString(((TimeSignature)symbol).TypeOfBeats),
                                Thread.CurrentThread.CurrentUICulture, FlowDirection.LeftToRight,
                                TypeFaces.TimeSignatureFont, 14.5, Brushes.Black), new Point(currentXPosition, timeSignaturePositionY + 21));
                        }
                        currentXPosition += 20;
                    }
                    else if (symbol.Type == MusicalSymbolType.Direction)
                    {
                        
                        //Performance directions / Wskazówki wykonawcze:
                        Direction dir = ((Direction)symbol);
                        float dirPositionY = 0;
                        if (dir.Placement == DirectionPlacementType.Custom)
                            dirPositionY = dir.DefaultY * -1.0f / 2.0f;
                        else if (dir.Placement == DirectionPlacementType.Above)
                            dirPositionY = 0;
                        else if (dir.Placement == DirectionPlacementType.Below)
                            dirPositionY = 50;
                        DrawString(drawingContext, dir.Text, TypeFaces.DirectionFont, textBrush, currentXPosition, dirPositionY, 11);
                        
                    }
                    else if (symbol.Type == MusicalSymbolType.Note)
                    {
                        Note note = ((Note)symbol);
                        if (firstNoteInIncipit) firstNoteInMeasureXPosition = currentXPosition;
                        firstNoteInIncipit = false;

                        if (note.Voice > currentVoice)
                        {
                            currentXPosition = firstNoteInMeasureXPosition;
                            lastNoteInMeasureEndXPosition = lastNoteEndXPosition;
                        }
                        currentVoice = note.Voice;



                        if (note.Tuplet == TupletType.Start) numberOfNotesUnderTuplet = 0;
                        numberOfNotesUnderTuplet++;

                        if (note.IsChordElement) currentXPosition = lastXPosition;

                        float notePositionY = currentClefPositionY + MusicalSymbol.StepDifference(currentClef,
                            note) * ((float)lineSpacing / 2.0f);

                        int numberOfSingleAccidentals = Math.Abs(note.Alter) % 2;
                        int numberOfDoubleAccidentals =
                            Convert.ToInt32(Math.Floor((double)(Math.Abs(note.Alter) / 2)));

                        //Move the note a bit to the right if it has accidentals / Przesuń nutę trochę w prawo, jeśli nuta ma znaki przygodne
                        if (note.Alter - currentKey.StepToAlter(note.Step) != 0)
                        {
                            if (numberOfSingleAccidentals > 0) currentXPosition += 9;
                            if (numberOfDoubleAccidentals > 0)
                                currentXPosition += (numberOfDoubleAccidentals) * 9;

                        }
                        if (note.HasNatural == true) currentXPosition += 9;

                        //Draw a note / Rysuj nutę:
                        var noteBrush = new SolidColorBrush(Color.FromRgb(note.MusicalCharacterColor.R, note.MusicalCharacterColor.G, note.MusicalCharacterColor.B));
                        var notePen = new Pen(noteBrush, 1.0f);
                        var noteBeamPen = new Pen(noteBrush, 2.0f);
                        if (!note.IsGraceNote)
                            DrawString(drawingContext, symbol.MusicalCharacter, TypeFaces.MusicFont, noteBrush, currentXPosition + 3.5f, notePositionY, 27.0f);
                        else
                            DrawString(drawingContext, symbol.MusicalCharacter, TypeFaces.GraceNoteFont, noteBrush, currentXPosition + 5.5f,
                                notePositionY + 2, 24.5f);
                        lastXPosition = currentXPosition;
                        note.Location = new System.Drawing.PointF(currentXPosition, notePositionY);

                        //Ledger lines / Linie dodane
                        float tmpXPos = currentXPosition + 16;
                        if (notePositionY + 25.0f > lines[4] + lineSpacing / 2.0f)
                        {
                            for (int i = lines[4]; i < notePositionY + 24f - lineSpacing / 2.0f; i += lineSpacing)
                            {

                                drawingContext.DrawLine(pen, new Point(currentXPosition + 4, i + lineSpacing),
                                    new Point(tmpXPos, i + lineSpacing));
                            }
                        }
                        if (notePositionY + 25.0f < lines[0] - lineSpacing / 2)
                        {

                            for (int i = lines[0]; i > notePositionY + 26.0f + lineSpacing / 2.0f; i -= lineSpacing)
                            {

                                drawingContext.DrawLine(pen, new Point(currentXPosition + 4, i - lineSpacing),
                                    new Point(tmpXPos, i - lineSpacing));
                            }
                        }

                        //Draw stems (stems are vertical lines, beams are horizontal lines :P)/ Rysuj ogonki: (ogonki to są te w pionie - poziome są belki ;P ;P ;P)
                        if ((note.Duration != MusicalSymbolDuration.Whole) &&
                            (note.Duration != MusicalSymbolDuration.Unknown))
                        {
                            float tmpStemPosY;

                            tmpStemPosY = note.StemDefaultY * -1.0f / 2.0f;


                            if (note.StemDirection == NoteStemDirection.Down)
                            {
                                //Ogonki elementów akordów nie były dobrze wyświetlane, jeśli stosowałem
                                //default-y. Dlatego dla akordów zostawiam domyślne rysowanie ogonków.
                                //Stems of chord elements were displayed wrong when I used default-y
                                //so I left default stem drawing routine for chords.
                                if (((note.IsChordElement) || xmlIncipit.InnerXml.Length == 0)
                                    || (!(note.CustomStemEndPosition)))
                                    currentStemEndPositionY = notePositionY + 18;
                                else
                                    currentStemEndPositionY = tmpStemPosY - 4;
                                currentStemPositionX = currentXPosition + 7;

                                if (note.BeamList.Count > 0)
                                    if ((note.BeamList[0] != NoteBeamType.Continue) || note.CustomStemEndPosition)
                                        drawingContext.DrawLine(notePen, new Point(currentStemPositionX, notePositionY - 1 + 28),
                                            new Point(currentStemPositionX, currentStemEndPositionY + 28));
                            }
                            else
                            {
                                //Ogonki elementów akordów nie były dobrze wyświetlane, jeśli stosowałem
                                //default-y. Dlatego dla akordów zostawiam domyślne rysowanie ogonków.
                                //Stems of chord elements were displayed wrong when I used default-y
                                //so I left default stem drawing routine for chords.
                                if ((note.IsChordElement) || xmlIncipit.InnerXml.Length == 0
                                    || (!(note.CustomStemEndPosition)))
                                    currentStemEndPositionY = notePositionY - 25;

                                else
                                    currentStemEndPositionY = tmpStemPosY - 6;
                                currentStemPositionX = currentXPosition + 13;

                                if (note.BeamList.Count > 0)
                                    if ((note.BeamList[0] != NoteBeamType.Continue) || note.CustomStemEndPosition)
                                        drawingContext.DrawLine(notePen, new Point(currentStemPositionX, notePositionY - 7 + 30),
                                            new Point(currentStemPositionX, currentStemEndPositionY + 28));
                            }
                            note.StemEndLocation = new System.Drawing.PointF(currentStemPositionX, currentStemEndPositionY);
                        }
                        //Draw beams / Rysuj belki:
                        int beamOffset = 0;
                        //Powiększ listę poprzednich pozycji stemów jeśli aktualna liczba belek jest większa
                        //Extend the list of previous stem positions if current number of beams is greater than the list size
                        if (previousStemEndPositionsY.Count < ((Note)symbol).BeamList.Count)
                        {
                            int tmpCount = previousStemEndPositionsY.Count;
                            for (int i = 0; i < ((Note)symbol).BeamList.Count - tmpCount; i++)
                                previousStemEndPositionsY.Add(new int());
                        }
                        if (previousStemPositionsX.Count < ((Note)symbol).BeamList.Count)
                        {
                            int tmpCount = previousStemPositionsX.Count;
                            for (int i = 0; i < ((Note)symbol).BeamList.Count - tmpCount; i++)
                                previousStemPositionsX.Add(new int());
                        }
                        int beamLoop = 0;
                        bool alreadyPaintedNumberOfNotesInTuplet = false;
                        foreach (NoteBeamType beam in ((Note)symbol).BeamList)
                        {

                            int beamSpaceDirection = 1;
                            if (((Note)symbol).StemDirection == NoteStemDirection.Up) beamSpaceDirection = 1;
                            else beamSpaceDirection = -1;

                            if (beam == NoteBeamType.Start)
                            {
                                previousStemEndPositionsY[beamLoop] = currentStemEndPositionY;
                                previousStemPositionsX[beamLoop] = currentStemPositionX;

                            }
                            else if (beam == NoteBeamType.Continue)
                            {
                                //Do nothing
                            }
                            else if (beam == NoteBeamType.End)
                            {
                                drawingContext.DrawLine(beamPen, new Point(previousStemPositionsX[beamLoop], previousStemEndPositionsY[beamLoop] + 28
                                    + beamOffset * beamSpaceDirection),
                                    new Point(currentStemPositionX, currentStemEndPositionY + 28
                                        + beamOffset * beamSpaceDirection));
                                //drawingContext.DrawLine(pen, new Point(previousStemPositionsX[beamLoop], previousStemEndPositionsY[beamLoop]
                                //    + 28 + 1 * beamSpaceDirection + beamOffset * beamSpaceDirection),
                                //    new Point(currentStemPositionX, currentStemEndPositionY + 28
                                //        + 1 * beamSpaceDirection + beamOffset * beamSpaceDirection));
                                //Draw tuplet mark / Rysuj oznaczenie trioli:
                                if ((((Note)symbol).Tuplet == TupletType.Stop) && (!alreadyPaintedNumberOfNotesInTuplet))
                                {
                                    int tmpMod;
                                    if (((Note)symbol).StemDirection == NoteStemDirection.Up) tmpMod = 12;
                                    else tmpMod = 28;
                                    DrawString(drawingContext, Convert.ToString(numberOfNotesUnderTuplet), TypeFaces.LyricFont,
                                        textBrush,
                                        previousStemPositionsX[beamLoop] + (currentStemPositionX - previousStemPositionsX[beamLoop]) / 2 - 1,
                                            previousStemEndPositionsY[beamLoop] - (currentStemEndPositionY - previousStemEndPositionsY[beamLoop]) / 2 + tmpMod, 14);
                                    alreadyPaintedNumberOfNotesInTuplet = true;
                                }
                            }
                            else if ((beam == NoteBeamType.Single) && (!((Note)symbol).IsChordElement))
                            {   //Rysuj chorągiewkę tylko najniższego dźwięku w akordzie
                                //Draw a hook only of the lowest note in a chord
                                float xPos = currentStemPositionX - 4;
                                if (((Note)symbol).StemDirection == NoteStemDirection.Down)
                                {
                                    DrawString(drawingContext, ((Note)symbol).NoteFlagCharacterRev, TypeFaces.MusicFont, noteBrush,
                                        xPos + 3.5f, currentStemEndPositionY + 7, 27.5f);
                                }
                                else
                                {
                                    DrawString(drawingContext, ((Note)symbol).NoteFlagCharacter, TypeFaces.MusicFont, noteBrush,
                                        xPos + 3.5f, currentStemEndPositionY - 1, 27.5f);
                                }
                            }
                            else if (beam == NoteBeamType.ForwardHook)
                            {
                                drawingContext.DrawLine(noteBeamPen, new Point(currentStemPositionX + 6,
                                    currentStemEndPositionY + 28 + beamOffset * beamSpaceDirection),
                                    new Point(currentStemPositionX, currentStemEndPositionY + 28
                                    + beamOffset * beamSpaceDirection));
                                //drawingContext.DrawLine(pen, new Point(currentStemPositionX + 6,
                                //    currentStemEndPositionY + 29 + beamOffset * beamSpaceDirection),
                                //    new Point(currentStemPositionX, currentStemEndPositionY + 29
                                //    + beamOffset * beamSpaceDirection));
                            }
                            else if (beam == NoteBeamType.BackwardHook)
                            {
                                drawingContext.DrawLine(noteBeamPen, new Point(currentStemPositionX - 6,
                                    currentStemEndPositionY + 28 + beamOffset * beamSpaceDirection),
                                    new Point(currentStemPositionX, currentStemEndPositionY + 28
                                    + beamOffset * beamSpaceDirection));
                                //drawingContext.DrawLine(pen, new Point(currentStemPositionX - 6,
                                //    currentStemEndPositionY + 29 + beamOffset * beamSpaceDirection),
                                //    new Point(currentStemPositionX, currentStemEndPositionY + 29
                                //    + beamOffset * beamSpaceDirection));
                            }

                            beamOffset += 4;
                            beamLoop++;

                        }

                        //Draw ties / Rysuj łuki:
                        if (((Note)symbol).TieType == NoteTieType.Start)
                        {
                            tieStartPoint = new Point(currentXPosition, notePositionY);
                        }
                        else if (((Note)symbol).TieType != NoteTieType.None) //Stop or StopAndStartAnother / Stop lub StopAndStartAnother
                        {
                            
                            if (((Note)symbol).StemDirection == NoteStemDirection.Down)
                            {
                                PathGeometry pathGeom = new PathGeometry();
                                PathFigure pf = new PathFigure();
                                pf.StartPoint =
                                    new Point((int)tieStartPoint.X + 16, (int)tieStartPoint.Y + 18);
                                ArcSegment arcSeg = new ArcSegment(
                                    new Point((int)currentXPosition + 5, (int)tieStartPoint.Y + 18),
                                    new Size((int)currentXPosition - (int)tieStartPoint.X, 100),
                                    180, false, SweepDirection.Clockwise, true);
                                pf.Segments.Add(arcSeg);
                                pathGeom.Figures.Add(pf);
                                drawingContext.DrawGeometry(null, beamPen, pathGeom);
                            }
                            else if (((Note)symbol).StemDirection == NoteStemDirection.Up)
                            {
                                PathGeometry pathGeom = new PathGeometry();
                                PathFigure pf = new PathFigure();
                                pf.StartPoint =
                                    new Point((int)tieStartPoint.X + 16, (int)tieStartPoint.Y + 22);
                                ArcSegment arcSeg = new ArcSegment(
                                    new Point((int)currentXPosition + 5, (int)tieStartPoint.Y + 22),
                                    new Size((int)currentXPosition - (int)tieStartPoint.X, 100),
                                    180, false, SweepDirection.Clockwise, true);

                                pf.Segments.Add(arcSeg);
                                pathGeom.Figures.Add(pf);
                                drawingContext.DrawGeometry(null, beamPen, pathGeom);
                            }
                            if (((Note)symbol).TieType == NoteTieType.StopAndStartAnother)
                            {
                                tieStartPoint = new Point(currentXPosition + 2, notePositionY);
                            }
                            
                        }

                        //Draw slurs / Rysuj łuki legatowe:
                        
                        if (((Note)symbol).Slur == NoteSlurType.Start)
                        {
                            slurStartPoint = new Point(currentXPosition, notePositionY);
                        }
                        else if (((Note)symbol).Slur == NoteSlurType.Stop)
                        {
                            if (((Note)symbol).StemDirection == NoteStemDirection.Down)
                            {
                                PathGeometry pathGeom = new PathGeometry();
                                PathFigure pf = new PathFigure();
                                pf.StartPoint =
                                    new Point(slurStartPoint.X + 10, slurStartPoint.Y + 18);
                                BezierSegment bSeg = new BezierSegment(
                                    new Point(slurStartPoint.X + 12, slurStartPoint.Y + 9),
                                    new Point(currentXPosition + 10, notePositionY + 9),
                                    new Point(currentXPosition + 8, notePositionY + 18) ,true);
                                pf.Segments.Add(bSeg);
                                pathGeom.Figures.Add(pf);
                                drawingContext.DrawGeometry(null, beamPen, pathGeom);

                        
                        }
                        else if (((Note)symbol).StemDirection == NoteStemDirection.Up)
                        {
                            PathGeometry pathGeom = new PathGeometry();
                            PathFigure pf = new PathFigure();
                            pf.StartPoint =
                                new Point(slurStartPoint.X + 10, slurStartPoint.Y + 30);
                            BezierSegment bSeg = new BezierSegment(
                                new Point(slurStartPoint.X + 12, slurStartPoint.Y + 40),
                                new Point(currentXPosition + 10, notePositionY + 40),
                                new Point(currentXPosition + 8, notePositionY + 30), true);
                            pf.Segments.Add(bSeg);
                            pathGeom.Figures.Add(pf);
                            drawingContext.DrawGeometry(null, beamPen, pathGeom);

                        }
                        }

                        //Draw lyrics / Rysuj tekst:
                        int textPositionY = lines[4] + 10;
                        for (int j = 0; (j < (((Note)symbol).Lyrics.Count)) &&
                            (j < (((Note)symbol).LyricTexts.Count))
                            ; j++)
                        {
                            StringBuilder sBuilder = new StringBuilder();
                            if ((((Note)symbol).Lyrics[j] == LyricsType.End) ||
                                (((Note)symbol).Lyrics[j] == LyricsType.Middle))
                                sBuilder.Append("-");
                            sBuilder.Append(((Note)symbol).LyricTexts[j]);
                            if ((((Note)symbol).Lyrics[j] == LyricsType.Begin) ||
                                (((Note)symbol).Lyrics[j] == LyricsType.Middle))
                                sBuilder.Append("-");
                            DrawString(drawingContext, sBuilder.ToString(), TypeFaces.LyricFont, textBrush, currentXPosition, textPositionY, 11);
                            textPositionY += 12;
                        }

                        //Draw articulation / Rysuj artykulację:
                        if (((Note)symbol).Articulation != ArticulationType.None)
                        {
                            float articulationPosition = notePositionY + 10;
                            if (((Note)symbol).ArticulationPlacement == ArticulationPlacementType.Above)
                                articulationPosition = notePositionY - 10;
                            else if (((Note)symbol).ArticulationPlacement == ArticulationPlacementType.Below)
                                articulationPosition = notePositionY + 10;

                            if (((Note)symbol).Articulation == ArticulationType.Staccato)
                                DrawString(drawingContext, MusicalCharacters.Dot, TypeFaces.MusicFont, textBrush, currentXPosition + 6, articulationPosition, 26.5f);
                            else if (((Note)symbol).Articulation == ArticulationType.Accent)
                                DrawString(drawingContext, ">", TypeFaces.MiscArticulationFont, textBrush, currentXPosition + 6, articulationPosition + 16, 14);

                        }

                        //Draw trills / Rysuj tryle:
                        if (((Note)symbol).TrillMark != NoteTrillMark.None)
                        {
                            float trillPos = notePositionY - 1;
                            if (((Note)symbol).TrillMark == NoteTrillMark.Above)
                            {
                                trillPos = notePositionY - 1;
                                if (trillPos > lines[0] - 24.4f)
                                {
                                    trillPos = lines[0] - 24.4f - 1.0f;
                                }
                            }
                            else if (((Note)symbol).TrillMark == NoteTrillMark.Below)
                            {
                                trillPos = notePositionY + 10;
                            }
                            DrawString(drawingContext, "tr", TypeFaces.TrillFont, textBrush, currentXPosition + 6, trillPos, 14);
                        }

                        //Draw tremolos / Rysuj tremola:
                        float currentTremoloPos = notePositionY + 18;
                        for (int j = 0; j < ((Note)symbol).TremoloLevel; j++)
                        {
                            if (((Note)symbol).StemDirection == NoteStemDirection.Up)
                            {
                                
                                currentTremoloPos -= 4;
                                drawingContext.DrawLine(pen, new Point(currentXPosition + 9, currentTremoloPos + 1),
                                    new Point(currentXPosition + 16, currentTremoloPos - 1));
                                drawingContext.DrawLine(pen, new Point(currentXPosition + 9, currentTremoloPos + 2),
                                    new Point(currentXPosition + 16, currentTremoloPos));
                                
                            }
                            else
                            {
                                currentTremoloPos += 4;

                                drawingContext.DrawLine(pen, new Point(currentXPosition + 3, currentTremoloPos + 11 + 1),
                                    new Point(currentXPosition + 11, currentTremoloPos + 11 - 1));
                                drawingContext.DrawLine(pen, new Point(currentXPosition + 3, currentTremoloPos + 11 + 2),
                                    new Point(currentXPosition + 11, currentTremoloPos + 11));
                                 
                            }

                        }

                        //Draw fermata sign / Rysuj symbol fermaty:
                        if (((Note)symbol).HasFermataSign)
                        {
                            float ferPos = notePositionY - 9;
                            if (ferPos > lines[0] - 24.4f) ferPos = lines[0] - 24.4f - 9.0f;

                            PathGeometry pathGeom = new PathGeometry();
                            PathFigure pf = new PathFigure();
                            pf.StartPoint =
                                new Point(currentXPosition + 4, (int)ferPos + 23);
                            ArcSegment arcSeg = new ArcSegment(
                                new Point(currentXPosition + 16, (int)ferPos + 23),
                                new Size(10, 25),
                                180, false, SweepDirection.Clockwise, true);
                            pf.Segments.Add(arcSeg);
                            pathGeom.Figures.Add(pf);
                            drawingContext.DrawGeometry(null, beamPen, pathGeom);

                            DrawString(drawingContext, MusicalCharacters.Dot, TypeFaces.MusicFont, textBrush, currentXPosition + 10, ferPos, 27);
                        
                        }

                        //Draw accidental signs / Rysuj akcydencje:
                        if (((Note)symbol).Alter - currentKey.StepToAlter(((Note)symbol).Step)
                            - alterationsWithinOneBar[((Note)symbol).StepToStepNumber()] > 0)
                        {
                            alterationsWithinOneBar[((Note)symbol).StepToStepNumber()] =
                                ((Note)symbol).Alter - currentKey.StepToAlter(((Note)symbol).Step);
                            int accPlacement = currentXPosition - 9 * numberOfSingleAccidentals -
                                9 * numberOfDoubleAccidentals;
                            for (int i = 0; i < numberOfSingleAccidentals; i++)
                            {
                                DrawString(drawingContext, MusicalCharacters.Sharp, TypeFaces.MusicFont, textBrush, accPlacement, notePositionY, 26.5f);
                                accPlacement += 9;
                            }
                            for (int i = 0; i < numberOfDoubleAccidentals; i++)
                            {
                                DrawString(drawingContext, MusicalCharacters.DoubleSharp, TypeFaces.MusicFont, textBrush, accPlacement, notePositionY, 26.5f);
                                accPlacement += 9;
                            }
                        }
                        else if (((Note)symbol).Alter - currentKey.StepToAlter(((Note)symbol).Step)
                            - alterationsWithinOneBar[((Note)symbol).StepToStepNumber()] < 0)
                        {
                            alterationsWithinOneBar[((Note)symbol).StepToStepNumber()] =
                                ((Note)symbol).Alter - currentKey.StepToAlter(((Note)symbol).Step);
                            int accPlacement = currentXPosition - 9 * numberOfSingleAccidentals -
                                9 * numberOfDoubleAccidentals;
                            for (int i = 0; i < numberOfSingleAccidentals; i++)
                            {
                                DrawString(drawingContext, MusicalCharacters.Flat, TypeFaces.MusicFont, textBrush, accPlacement, notePositionY, 26.5f);
                                accPlacement += 9;
                            }
                            for (int i = 0; i < numberOfDoubleAccidentals; i++)
                            {
                                DrawString(drawingContext, MusicalCharacters.DoubleFlat, TypeFaces.MusicFont, textBrush, accPlacement, notePositionY, 26.5f);
                                accPlacement += 9;
                            }
                        }
                        if (((Note)symbol).HasNatural == true)
                        {
                            DrawString(drawingContext, MusicalCharacters.Natural, TypeFaces.MusicFont, textBrush, currentXPosition - 9, notePositionY, 26.5f);
                        }

                        //Draw dots / Rysuj kropki:
                        if (((Note)symbol).NumberOfDots > 0) currentXPosition += 16;
                        for (int i = 0; i < ((Note)symbol).NumberOfDots; i++)
                        {
                            DrawString(drawingContext, MusicalCharacters.Dot, TypeFaces.MusicFont, textBrush, currentXPosition, notePositionY, 26.5f);
                            currentXPosition += 6;
                        }


                        if (((Note)symbol).Duration == MusicalSymbolDuration.Whole) currentXPosition += 50;
                        else if (((Note)symbol).Duration == MusicalSymbolDuration.Half) currentXPosition += 30;
                        else if (((Note)symbol).Duration == MusicalSymbolDuration.Quarter) currentXPosition += 18;
                        else if (((Note)symbol).Duration == MusicalSymbolDuration.Eighth) currentXPosition += 15;
                        else if (((Note)symbol).Duration == MusicalSymbolDuration.Unknown) currentXPosition += 25;
                        else currentXPosition += 14;

                        //Przesuń trochę w prawo, jeśli nuta ma tekst, żeby litery nie wchodziły na siebie
                        //Move a bit right if the note has a lyric to prevent letters from hiding each other
                        if (((Note)symbol).Lyrics.Count > 0)
                        {
                            currentXPosition += ((Note)symbol).LyricTexts[0].Length * 2;
                        }

                        lastNoteEndXPosition = currentXPosition;



                    }
                    else if (symbol.Type == MusicalSymbolType.Rest)
                    {
                        if (firstNoteInIncipit) firstNoteInMeasureXPosition = currentXPosition;
                        firstNoteInIncipit = false;

                        if (((Rest)symbol).Voice > currentVoice)
                        {
                            currentXPosition = firstNoteInMeasureXPosition;
                            lastNoteInMeasureEndXPosition = lastNoteEndXPosition;
                        }
                        currentVoice = ((Rest)symbol).Voice;


                        float restPositionY = (lines[0] - 9);

                        DrawString(drawingContext, symbol.MusicalCharacter, TypeFaces.MusicFont, textBrush, currentXPosition, restPositionY, 26.5f);
                        lastXPosition = currentXPosition;

                        //Draw number of measures for multimeasure rests / Rysuj ilość taktów dla pauz wielotaktowych:
                        if (((Rest)symbol).MultiMeasure > 1)
                        {
                            DrawString(drawingContext, Convert.ToString(((Rest)symbol).MultiMeasure),
                                TypeFaces.LyricFontBold, textBrush, currentXPosition + 6, restPositionY, 0.8f);
                        }

                        //Draw dots / Rysuj kropki:
                        if (((Rest)symbol).NumberOfDots > 0) currentXPosition += 16;
                        for (int i = 0; i < ((Rest)symbol).NumberOfDots; i++)
                        {
                            DrawString(drawingContext, MusicalCharacters.Dot, TypeFaces.MusicFont, textBrush, currentXPosition, restPositionY, 26.5f);
                            currentXPosition += 6;
                        }

                        if (((Rest)symbol).Duration == MusicalSymbolDuration.Whole) currentXPosition += 48;
                        else if (((Rest)symbol).Duration == MusicalSymbolDuration.Half) currentXPosition += 28;
                        else if (((Rest)symbol).Duration == MusicalSymbolDuration.Quarter) currentXPosition += 17;
                        else if (((Rest)symbol).Duration == MusicalSymbolDuration.Eighth) currentXPosition += 15;
                        else currentXPosition += 14;

                        lastNoteEndXPosition = currentXPosition;
                    }
                    else if (symbol.Type == MusicalSymbolType.Barline)
                    {
                        Barline barline = (Barline)symbol;
                        if (lastNoteInMeasureEndXPosition > currentXPosition)
                        {
                            currentXPosition = lastNoteInMeasureEndXPosition;
                        }
                        if (barline.RepeatSign == RepeatSignType.None)
                        {
                            currentXPosition += 16;
                            drawingContext.DrawLine(pen, new Point(currentXPosition, lines[4]), new Point(currentXPosition, lines[0]));
                            currentXPosition += 6;
                        }
                        else if (barline.RepeatSign == RepeatSignType.Forward)
                        {
                            //Przesuń w lewo jeśli przed znakiem repetycji znajduje się zwykła kreska taktowa
                            //Move to the left if there is a plain measure bar before the repeat sign
                            if (incipit.IndexOf(symbol) > 0)
                            {
                                MusicalSymbol s = incipit[incipit.IndexOf(symbol) - 1];
                                if (s.Type == MusicalSymbolType.Barline)
                                {
                                    if (((Barline)s).RepeatSign == RepeatSignType.None)
                                        currentXPosition -= 16;
                                }
                            }
                            currentXPosition += 2;
                            DrawString(drawingContext, MusicalCharacters.RepeatForward, TypeFaces.StaffFont, textBrush, currentXPosition,
                                lines[0] - 15.5f, 1.9f);
                            currentXPosition += 20;
                        }
                        else if (barline.RepeatSign == RepeatSignType.Backward)
                        {
                            currentXPosition -= 2;
                            DrawString(drawingContext, MusicalCharacters.RepeatBackward, TypeFaces.StaffFont, textBrush, currentXPosition,
                                lines[0] - 15.5f, 1.9f);
                            currentXPosition += 6;
                        }
                        firstNoteInMeasureXPosition = currentXPosition;

                        for (int i = 0; i < 7; i++)
                            alterationsWithinOneBar[i] = 0;

                        currentMeasure++;
                    }

                    if (currentXPosition > Width - 10) break; //Fell out of control bounds / Wyszło poza długość kontrolki
                }


                //Draw missing stems / Dorysuj brakujące ogonki:
                Note lastNoteInBeam = null;
                Note firstNoteInBeam = null;
                foreach (MusicalSymbol m in incipit)
                {
                    if (m.Type != MusicalSymbolType.Note) continue;
                    Note note = (Note)m;

                    //Search for the end of the beam / Przeszukaj i znajdź koniec belki:
                    if (note.BeamList.Count > 0)
                    {
                        if (note.BeamList[0] == NoteBeamType.End) continue;
                        if (note.BeamList[0] == NoteBeamType.Start)
                        {
                            firstNoteInBeam = note;
                            continue;
                        }
                        if (note.BeamList[0] == NoteBeamType.Continue)
                        {
                            if (note.CustomStemEndPosition) continue;
                            for (int i = incipit.IndexOf(m) + 1; i < incipit.Count; i++)
                            {
                                if (incipit[i].Type != MusicalSymbolType.Note) continue;
                                Note note2 = (Note)incipit[i];
                                if (note2.BeamList.Count > 0)
                                {
                                    if (note2.BeamList[0] == NoteBeamType.End)
                                    {
                                        lastNoteInBeam = note2;
                                        break;
                                    }
                                }
                            }
                            float newStemEndPosition = Math.Abs(note.StemEndLocation.X -
                                firstNoteInBeam.StemEndLocation.X) *
                                ((Math.Abs(lastNoteInBeam.StemEndLocation.Y - firstNoteInBeam.StemEndLocation.Y)) /
                                (Math.Abs(lastNoteInBeam.StemEndLocation.X - firstNoteInBeam.StemEndLocation.X)));

                            //Jeśli ostatnia nuta jest wyżej, to odejmij y zamiast dodać
                            //If the last note is higher, subtract y instead of adding
                            if (lastNoteInBeam.StemEndLocation.Y < firstNoteInBeam.StemEndLocation.Y)
                                newStemEndPosition *= -1;

                            Point newStemEndPoint = new Point(note.StemEndLocation.X,
                                firstNoteInBeam.StemEndLocation.Y +
                                newStemEndPosition);
                            if (note.StemDirection == NoteStemDirection.Down)
                                drawingContext.DrawLine(pen, new Point(note.StemEndLocation.X, note.Location.Y + 25),
                                    new Point(newStemEndPoint.X, newStemEndPoint.Y + 23 + 5));
                            else
                                drawingContext.DrawLine(pen, new Point(note.StemEndLocation.X, note.Location.Y + 23),
                                    new Point(newStemEndPoint.X, newStemEndPoint.Y + 23 + 5));


                        }
                    }
                    if (lastNoteInBeam == null) continue;
                }


            }
            catch
            {
                return;
            }


        }

        #endregion

        #region Event subscribers


        private void UserControl_MouseEnter(object sender, MouseEventArgs e)
        {
            buttonPlay.Visibility = Visibility.Visible;
            buttonSave.Visibility = Visibility.Visible;
        }

        private void UserControl_MouseLeave(object sender, MouseEventArgs e)
        {
            buttonPlay.Visibility = Visibility.Hidden;
            buttonSave.Visibility = Visibility.Hidden;
        }

        private void buttonSave_Click(object sender, RoutedEventArgs e)
        {
            if (xmlIncipit.InnerXml.Length == 0) return;
            Microsoft.Win32.SaveFileDialog saveFileDialog = new Microsoft.Win32.SaveFileDialog();
            saveFileDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
            saveFileDialog.Filter = "Pliki XML (*.xml)|*.xml|Wszystkie pliki (*.*)|*.*";
            if (saveFileDialog.ShowDialog() == true)
            {
                string FileName = saveFileDialog.FileName;
                Cursor = Cursors.Wait;
                XmlTextWriter wr = new XmlTextWriter(saveFileDialog.FileName, Encoding.UTF8);
                wr.Formatting = Formatting.Indented;
                xmlIncipit.WriteContentTo(wr);
                wr.Close();
                Cursor = Cursors.Arrow;
            }

        }

        private void buttonPlay_Click(object sender, RoutedEventArgs e)
        {
            OnPlayExternalMidiPlayer(this);
        }
       
        #endregion
    }
}
