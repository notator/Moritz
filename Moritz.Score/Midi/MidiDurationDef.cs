
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

        public virtual void Transpose(int interval) { }
        public virtual void AdjustVelocities(double factor) { }
        public virtual void AdjustExpression(double factor) { }
        public virtual void AdjustModulationWheel(double factor) { }
        public virtual void AdjustPitchWheel(double factor) { }

        public virtual byte MidiVelocity { get { return 0; } set { } }

        protected int _msDuration = 0;
        public int MsDuration { get { return _msDuration; } set { _msDuration = value; } }
    }
}
