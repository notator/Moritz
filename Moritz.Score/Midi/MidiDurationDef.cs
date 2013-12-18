
using System.Diagnostics;

namespace Moritz.Score.Midi
{
    public abstract class MidiDurationDef
    {
        protected MidiDurationDef() { }
        protected MidiDurationDef(int msDuration)
        {
            _msDuration = msDuration;
        }

        public abstract IUniqueMidiDurationDef CreateUniqueMidiDurationDef();

        protected int _msDuration = 0;
        public int MsDuration { get { return _msDuration; } set { _msDuration = value; } }
    }
}
