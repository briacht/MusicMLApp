using System;
using System.Runtime.InteropServices;
using System.Text;

namespace IntelligentDemo.Services
{
    public class MidiWrapper : IDisposable
    {
        // Wrapper over code from https://www.codeguru.com/columns/dotnet/making-music-with-midi-and-c.html
        private int _handle = 0;

        public MidiWrapper()
        {
            midiOutOpen(ref _handle, 0, null, 0, 0);
        }

        public void Dispose()
        {
            midiOutClose(_handle);
        }

        public void SelectInstrument(byte channel, byte instrument)
        {
            var message = (instrument << 8) + 0b11000000 + channel;
            midiOutShortMsg(_handle, message);
        }

        public void NoteOn(byte channel, byte note, byte velocity)
        {
            var message = (velocity << 16) + (note << 8) + 0b10010000 + channel;
            midiOutShortMsg(_handle, message);
        }

        public void NoteOff(byte channel, byte note, byte velocity)
        {
            var message = (velocity << 16) + (note << 8) + 0b10000000 + channel;
            midiOutShortMsg(_handle, message);
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct MidiOutCaps
        {
            public UInt16 wMid;
            public UInt16 wPid;
            public UInt32 vDriverVersion;

            [MarshalAs(UnmanagedType.ByValTStr,
               SizeConst = 32)]
            public String szPname;

            public UInt16 wTechnology;
            public UInt16 wVoices;
            public UInt16 wNotes;
            public UInt16 wChannelMask;
            public UInt32 dwSupport;
        }

        // MCI INterface
        [DllImport("winmm.dll")]
        private static extern long mciSendString(string command,
           StringBuilder returnValue, int returnLength,
           IntPtr winHandle);

        // Midi API
        [DllImport("winmm.dll")]
        private static extern int midiOutGetNumDevs();

        [DllImport("winmm.dll")]
        private static extern int midiOutGetDevCaps(Int32 uDeviceID,
           ref MidiOutCaps lpMidiOutCaps, UInt32 cbMidiOutCaps);

        [DllImport("winmm.dll")]
        private static extern int midiOutOpen(ref int handle,
           int deviceID, MidiCallBack proc, int instance, int flags);

        [DllImport("winmm.dll")]
        private static extern int midiOutShortMsg(int handle,
           int message);

        [DllImport("winmm.dll")]
        private static extern int midiOutClose(int handle);

        private delegate void MidiCallBack(int handle, int msg,
           int instance, int param1, int param2);

        static string Mci(string command)
        {
            StringBuilder reply = new StringBuilder(256);
            mciSendString(command, reply, 256, IntPtr.Zero);
            return reply.ToString();
        }
    }

}
