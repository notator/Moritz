using Moritz.Globals;

using System.Collections.Generic;
using System.Diagnostics;

namespace Moritz.Spec
{
    /// <summary>
    /// Created when a note or rest straddles a barline.
    /// This class is created while splitting systems.
    /// It is used when notating them. this class has no content!
    /// </summary>
    public class CautionaryChordDef : IUniqueChordDef
    {
        public CautionaryChordDef(IUniqueChordDef chordDef, int msPositionReFirstIUD, int msDuration)
        {
            NotatedMidiPitches = chordDef.NotatedMidiPitches;
            if(chordDef is MidiChordDef)
            {
                NotatedMidiVelocities = chordDef.NotatedMidiVelocities;
            }

            MsPositionReFirstUD = msPositionReFirstIUD;
            MsDuration = msDuration;
        }

        #region IUniqueChordDef
        /// <summary>
        /// Cautionary chords should never be transposed. They always have the same pitches as the chord from which they are constructed.
        /// </summary>
        public void Transpose(int interval) { }

        /// <summary>
        /// This NotatedMidiPitches field is used when displaying the chord's noteheads.
        /// </summary>
        public List<byte> NotatedMidiPitches
        {
            get { return _notatedMidiPitches; }
            set
            {
                foreach(byte pitch in value)
                {
                    Debug.Assert(pitch == M.MidiValue(pitch));
                }
                _notatedMidiPitches = new List<byte>(value);
            }
        }
        private List<byte> _notatedMidiPitches = null;

        /// <summary>
        /// This NotatedMidiVelocities field is used when displaying the chord's noteheads.
        /// </summary>
        public List<byte> NotatedMidiVelocities
        {
            get
            {
                return _notatedMidiVelocities;
            }
            set
            {
                foreach(byte velocity in value)
                {
                    Debug.Assert(velocity == M.MidiValue(velocity));
                }
                _notatedMidiVelocities = new List<byte>(value);
            }
        }
        private List<byte> _notatedMidiVelocities = null;

        #region IUniqueDef
        public override string ToString()
        {
            return ("MsPositionReFirstIUD=" + MsPositionReFirstUD.ToString() + " MsDuration=" + MsDuration.ToString() + " CautionaryChordDef");
        }

        public void AdjustMsDuration(double factor) { }

        public object Clone()
        {
            CautionaryChordDef deepClone = new CautionaryChordDef(this, this._msPositionReFirstIUD, this._msDuration);
            return deepClone;
        }

        public int MsDuration { get { return _msDuration; } set { _msDuration = value; } }
        private int _msDuration = 0;
        public int MsPositionReFirstUD { get { return _msPositionReFirstIUD; } set { _msPositionReFirstIUD = value; } }
        private int _msPositionReFirstIUD = 0;
        #endregion IUniqueDef
        #endregion IUniqueChordDef

    }
}
