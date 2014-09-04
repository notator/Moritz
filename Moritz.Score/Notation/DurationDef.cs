
using System.Diagnostics;

namespace Moritz.Score.Notation
{
    public abstract class DurationDef
    {
        protected DurationDef(int msDuration)
        {
            _msDuration = msDuration;
        }

        public abstract IUniqueDef DeepClone();

        protected int _msDuration = 0;
        public virtual int MsDuration { get; set; }
    }
}
