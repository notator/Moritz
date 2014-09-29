using System.Collections.Generic;
using System.Diagnostics;

namespace Moritz.Midi
{
    /// <summary>
    /// A lists of synchronous MidiChords, Volumes and PitchWheelDeviations.
    /// </summary>
    public class MidiMoment
    {
        public MidiMoment(int msPosition)
        {
            Debug.Assert(msPosition >= 0);
            _msPosition = msPosition;
        }

        /// <summary>
        /// The millisecond position of this MidiMoment in the score.
        /// </summary>
        public int MsPosition { get { return _msPosition; } set { Debug.Assert(value >= 0); _msPosition = value; } }
        private int _msPosition;
        /// <summary>
        /// When the performer releases a key, the current msPosition becomes MsPosition + MsDuration.
        /// </summary>
        public int MsWidth;
        /// <summary>
        /// The time (in milliseconds) at which this moment begins. Set using DateTime.Now.Ticks / 10000.
        /// </summary>
        public int StartTimeMilliseconds = 0;
        /// <summary>
        /// Set during a performance (after the duration has been decided by the performer)
        /// or before a performance (in which case it can be overridden by the performer, but is the default
        /// duration for the assistant).
        /// </summary>
        public int PerformedDelay = 0;
        /// <summary>
        /// If this moment is being played by the live performer, all Midi messages in the MidiDurationSymbols
        /// are sent immediately the performer depresses a key or chord. If any of the MidiDurationSymbols are
        /// MidiChords, their corresponding MidiChord.ChordOffs are sent at an MsPosition equal to 
        ///     MidiMoment.MsPosition + MidiChord.MsDuration. 
        /// Note that the msPosition of the ChordOff need not correspond to the msPosition of any moment.
        /// </summary>
        public int PerformedDuration = 0;

        /// <summary>
        /// The maxmum msDuration of any midiChordDef in this MidiMoment.
        /// </summary>
        public int MaximumMsDuration
        {
            get
            {
                int maximumMsDuration = 0;
                foreach(MidiChord midiChord in MidiChords)
                {
                    maximumMsDuration = maximumMsDuration > midiChord.MsDuration ?
                                                    maximumMsDuration : midiChord.MsDuration;
                }
                return maximumMsDuration;
            }
        }

        public List<MidiChord> MidiChords = new List<MidiChord>();
    }
}
