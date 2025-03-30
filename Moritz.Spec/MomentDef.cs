using System.Collections.Generic;
using System.Diagnostics;

namespace Moritz.Spec
{
    /// <summary>
    /// A list of synchronous MidiChordDefs.
    /// </summary>
    public class MomentDef
    {
        public MomentDef(int msPosition)
        {
            Debug.Assert(msPosition >= 0);
            _msPosition = msPosition;
        }

        /// <summary>
        /// The millisecond position of this MomentDef in the score.
        /// </summary>
        public int MsPosition { get { return _msPosition; } set { Debug.Assert(value >= 0); _msPosition = value; } }
        private int _msPosition;
        /// <summary>
        /// When the performer releases a key, the current msPosition becomes MsPosition + MsWidth.
        /// </summary>
        public int MsWidth;

        /// <summary>
        /// The maxmum msDuration of any chordDef in this MomentDef.
        /// </summary>
        public int MaximumMsDuration
        {
            get
            {
                int maximumMsDuration = 0;
                foreach(ChordDef chordDef in MidiChordDefs)
                {
                    maximumMsDuration = maximumMsDuration > chordDef.MsDuration ?
                                                    maximumMsDuration : chordDef.MsDuration;
                }
                return maximumMsDuration;
            }
        }

        public List<ChordDef> MidiChordDefs = new List<ChordDef>();
    }
}
