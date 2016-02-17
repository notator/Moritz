using System.Collections.Generic;

using Krystals4ObjectLibrary;
using Moritz.Globals;

namespace Moritz.Spec
{
    /// <summary>
    /// Created when a note or rest straddles a barline.
    /// This class is created while splitting systems.
    /// It is used when notating them. this class has no content!
    /// </summary>
    public class CautionaryChordDef : IUniqueChordDef
    {
        public CautionaryChordDef(IUniqueChordDef chordDef, int msPosition, int msDuration)
        {
            _midiPitches = new List<byte>();
            foreach(byte midiPitch in chordDef.NotatedMidiPitches)
            {
                _midiPitches.Add(midiPitch);
            }
            MsPosition = msPosition;
            MsDuration = msDuration;
        }

        #region IUniqueChordDef
        /// <summary>
        /// Cautionary chords should never be transposed. They always have the same pitches as the chord from which they are constructed.
        /// </summary>
        public void Transpose(int interval) { }

        public List<byte> NotatedMidiPitches { get { return _midiPitches; } set { _midiPitches = value; } }
        private List<byte> _midiPitches = null;

        #region IUniqueDef
        public override string ToString()
        {
            return ("MsPosition=" + MsPosition.ToString() + " MsDuration=" + MsDuration.ToString() + " CautionaryChordDef");
        }

        public void AdjustMsDuration(double factor) { }

        public IUniqueDef DeepClone()
        {
            CautionaryChordDef deepClone = new CautionaryChordDef(this, this._msPosition, this._msDuration);
            return deepClone;
        }

        public int MsDuration { get { return _msDuration; } set { _msDuration = value; } }
        private int _msDuration = 0;
        public int MsPosition { get { return _msPosition; } set { _msPosition = value; } }
        private int _msPosition = 0;
        #endregion IUniqueDef
        #endregion IUniqueChordDef

    }
}
