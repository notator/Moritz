using System;
using System.Collections.Generic;

using Moritz.Globals;
using Multimedia.Midi;

namespace Moritz.Midi
{
    /// <summary>
    /// Objects of this class contain all the MIDI ChannelMessages which can be deduced from a chord or rest symbol.
    /// All MidiDurationSymbols except MidiRests can be contained in MidiMoments (which can be triggered by a live
    /// performer or "assistant").
    /// </summary>
    public abstract class MidiDurationSymbol
    {
        protected MidiDurationSymbol() { }

        public MidiDurationSymbol(int channel, int msPosition, int msDuration)
        {
            _channelIndex = channel;
            MsPosition = msPosition;
            MsDuration = msDuration;
        }

        /// <summary>
        /// Called by ChordOff.Send()
        /// </summary>
        public void SetDurationWhileRecording()
        {
            // the ultimate value of _durationMilliseconds is set by the last
            // ChordOff to be sent by this Midichord (possibly in an ornament).
            if(_startTimeMilliseconds < 0L)
                throw new ApplicationException("Error while attempting to set the duration of a MidiChord!");
            _performedDuration = (int)(M.NowMilliseconds - _startTimeMilliseconds);
        }

        protected long _performedDuration = -1;

        public int MsDuration 
        { 
            get { return _msDuration; }
            set
            {
                _msDuration = value;
            }
        }
        public int MsPosition
        { 
            get { return _msPosition; }
            set
            {
                _msPosition = value;
            }
        }

        public int Channel { get { return _channelIndex; } }

        /// <summary>
        /// Controls whose values must be set outside this MidiDurationSymbol (i.e. during the performance).
        /// Currently, these are either MidiSliders or MidiSwitches.
        /// </summary>
        public List<MidiControl> MidiControls = new List<MidiControl>();
        /// <summary>
        /// Currently, the only MoritzControl types are MoritzOrnament and MoritzKeyboardNumberControl.
        /// </summary>
        public List<MoritzControl> MoritzControls = new List<MoritzControl>();
        /// <summary>
        /// Messages to be sent when this durationSymbol ends (e.g. PedOff)
        /// </summary>
        public List<ChannelMessage> OffMessages = new List<ChannelMessage>();
        protected int _msPosition = 0;
        protected int _msDuration = 0;
        protected int _channelIndex = -1;

        public MidiMoment Container = null;
        private int _startTimeMilliseconds = -1; // error value
    }
}
