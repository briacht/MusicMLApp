using IntelligentDemo.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Threading;

namespace IntelligentDemo.Services
{
   

    public class BarStartedEventArgs : EventArgs
    {
        public int BarNumber { get; set; }
    }

    public class SongController : IDisposable
    {
        private static byte BASS_CHANNEL = 0;
        private static byte MELODY_CHANNEL = 1;
        private static byte UNKNOWN_NOTE_CHANNEL = 2;
        private static byte PERCUSSION_CHANNEL = 9;
        private static int SIXTEENTHS_PER_BAR = 16;

        private int _noteCount = 0;
        private MidiWrapper _midi;
        private DispatcherTimer _timer;
        private List<Action<MidiWrapper>>[] _currentBar;
        private List<Action<MidiWrapper>>[] _carryOver;
        private IEnumerable<MusicNote> _nextMelodyBar;
        private IEnumerable<MusicNote> _nextBassBar;
        private IEnumerable<MusicNote> _nextPercussionBar;
        private double _bassVolume = 1;
        private double _percussionVolume = 1;
        private double _melodyVolume = 1;

        public SongController()
        {
            _midi = new MidiWrapper();
            _midi.SelectInstrument(BASS_CHANNEL, 37);
            _midi.SelectInstrument(MELODY_CHANNEL, 1);
            _midi.SelectInstrument(UNKNOWN_NOTE_CHANNEL, 124);

            _carryOver = new List<Action<MidiWrapper>>[SIXTEENTHS_PER_BAR];
            for (int i = 0; i < _carryOver.Length; i++)
            {
                _carryOver[i] = new List<Action<MidiWrapper>>();
            }

            _timer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(125) /* 1/16 notes @ 120 bpm */ };
            _timer.Tick += (s, e) => OnSixteenthNotes();
        }

        public void SetBassVolume(double volume)
        {
            if (volume > 1 || volume < 0) throw new ArgumentException();

            _bassVolume = volume;
        }

        public void SetPercussionVolume(double volume)
        {
            if (volume > 1 || volume < 0) throw new ArgumentException();

            _percussionVolume = volume;
        }

        public void SetMelodyVolume(double volume)
        {
            if (volume > 1 || volume < 0) throw new ArgumentException();

            _melodyVolume = volume;
        }

        public event EventHandler<BarStartedEventArgs> BarStarted;

        protected virtual void OnBarStarted(BarStartedEventArgs e)
        {
            BarStarted?.Invoke(this, e);
        }

        public void Start()
        {
            _timer.Start();
        }

        public void Stop()
        {
            _timer.Stop();
        }

        private List<Action<MidiWrapper>>[] BuildNextBar()
        {
            var commands = new List<Action<MidiWrapper>>[SIXTEENTHS_PER_BAR];
            for (int i = 0; i < commands.Length; i++)
            {
                commands[i] = new List<Action<MidiWrapper>>(_carryOver[i]);
                _carryOver[i].Clear();
            }

            if (_nextBassBar != null)
            {
                foreach (var note in _nextBassBar)
                {
                    commands[note.Position - 1].Add(m => m.NoteOn(BASS_CHANNEL, note.Note, Convert.ToByte(note.Velocity * _bassVolume)));
                    var off = (note.Position - 1) + note.Duration;
                    if (off < SIXTEENTHS_PER_BAR)
                    {
                        commands[off].Add(m => m.NoteOff(BASS_CHANNEL, note.Note, Convert.ToByte(note.Velocity * _bassVolume)));
                    }
                    else
                    {
                        _carryOver[off - SIXTEENTHS_PER_BAR].Add(m => m.NoteOff(BASS_CHANNEL, note.Note, Convert.ToByte(note.Velocity * _bassVolume)));
                    }
                }
            }

            if (_nextMelodyBar != null)
            {
                foreach (var note in _nextMelodyBar)
                {
                    var number = note.Note == 0
                        ? (byte)60
                        : note.Note;

                    var channel = note.Note == 0
                        ? UNKNOWN_NOTE_CHANNEL
                        : MELODY_CHANNEL;

                    commands[note.Position - 1].Add(m => m.NoteOn(channel, number, Convert.ToByte(note.Velocity * _melodyVolume)));
                    var off = (note.Position - 1) + note.Duration;
                    if (off < SIXTEENTHS_PER_BAR)
                    {
                        commands[off].Add(m => m.NoteOff(channel, number, Convert.ToByte(note.Velocity * _melodyVolume)));
                    }
                    else
                    {
                        _carryOver[off - SIXTEENTHS_PER_BAR].Add(m => m.NoteOff(channel, number, Convert.ToByte(note.Velocity * _melodyVolume)));
                    }
                }
            }

            if (_nextPercussionBar != null)
            {
                foreach (var note in _nextPercussionBar)
                {
                    // Off commands not needed for percussion
                    commands[note.Position - 1].Add(m => m.NoteOn(PERCUSSION_CHANNEL, note.Note, Convert.ToByte(note.Velocity * _percussionVolume)));
                }
            }

            if (_nextMelodyBar != null || _nextBassBar != null || _nextPercussionBar != null)
            {
                // Metronome
                // TODO calculate based on SIXTEENTHS_PER_BAR
                //commands[0].Add(m => m.NoteOn(PERCUSSION_CHANNEL, 81, 40));
                //commands[4].Add(m => m.NoteOn(PERCUSSION_CHANNEL, 80, 40));
                //commands[8].Add(m => m.NoteOn(PERCUSSION_CHANNEL, 80, 40));
                //commands[12].Add(m => m.NoteOn(PERCUSSION_CHANNEL, 80, 40));
            }

            return commands;
        }

        public void SetNextMelodyBar(IEnumerable<MusicNote> notes)
        {
            _nextMelodyBar = notes;
        }

        public void SetNextBassBar(IEnumerable<MusicNote> notes)
        {
            _nextBassBar = notes;
        }

        public void SetNextPercussionBar(IEnumerable<MusicNote> notes)
        {
            _nextPercussionBar = notes;
        }

        private void OnSixteenthNotes()
        {
            var noteInBar = _noteCount % SIXTEENTHS_PER_BAR;

            if (noteInBar == 0)
            {
                _currentBar = BuildNextBar();
            }

            if (_currentBar != null)
            {
                _currentBar[noteInBar].ForEach(m => m(_midi));
            }

            if (noteInBar == 0)
            {
                OnBarStarted(new BarStartedEventArgs { BarNumber = _noteCount / SIXTEENTHS_PER_BAR + 1 });
            }

            _noteCount++;
        }

        public void Dispose()
        {
            _midi.Dispose();
        }
    }
}
