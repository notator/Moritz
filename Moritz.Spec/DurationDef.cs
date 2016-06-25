
using System;

using Krystals4ObjectLibrary;
using Moritz.Globals;

namespace Moritz.Spec
{
    public abstract class DurationDef : ICloneable
    {
        protected DurationDef(int msDuration)
        {
            _msDuration = msDuration;
        }

		/// <summary>
		/// This is always a deep clone.
		/// </summary>
		/// <returns>A deep clone</returns>
        public abstract object Clone();

        public virtual int MsDuration { get { return _msDuration; } set { _msDuration = value; } }
        protected int _msDuration = 0;
    }
}
