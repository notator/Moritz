
using System;
using System.Collections.Generic;

namespace Moritz.Spec
{
    public abstract class DurationDef : IUniqueDef
    {
        protected DurationDef(int msDuration)
        {
            MsDuration = msDuration;
        }

        /// <summary>
        /// This is always a deep clone.
        /// </summary>
        /// <returns>A deep clone</returns>
        public abstract object Clone();

        /// <summary>
        /// Multiplies the MsDuration by the given factor.
        /// If the result would be less than 10ms, MsDuration is set to 10ms.
        /// </summary>
        /// <param name="factor"></param>
        public void AdjustMsDuration(double factor)
        {
            int msDuration = (int)(MsDuration * factor);
            MsDuration = (msDuration < 10) ? 10 : msDuration;
        }

        public int MsDuration { get; set; } = 0;
        public int MsPositionReFirstUD { get; set; } = 0;

        public List<DurationDef> MidiDefs { get; set; } = new List<DurationDef>();
    }
}
