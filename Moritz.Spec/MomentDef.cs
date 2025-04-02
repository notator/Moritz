using Moritz.Globals;

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
        /// The maxmum msDuration of any midiChordDef in this MomentDef.
        /// </summary>
        public int MaximumMsDuration
        {
            get
            {
                int maximumMsDuration = 0;
                foreach(MidiChordDef midiChordDef in MidiChordDefs)
                {
                    maximumMsDuration = maximumMsDuration > midiChordDef.MsDuration ?
                                                    maximumMsDuration : midiChordDef.MsDuration;
                }
                return maximumMsDuration;
            }
        }

        public List<MidiChordDef> MidiChordDefs = new List<MidiChordDef>();
    }
}
