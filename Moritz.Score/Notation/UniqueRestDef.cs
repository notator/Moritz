using System.Collections.Generic;
using System.Diagnostics;
using System.Xml;

using Multimedia.Midi;

using Moritz.Globals;
using Moritz.Score.Midi;

namespace Moritz.Score.Notation
{
    ///<summary>
    /// A UniqueRestDef is a unique rest definition which is saved locally in an SVG file.
    /// Each rest in an SVG file will be given an ID of the form "rest"+uniqueNumber, but
    /// Moritz does not actually use the ids, so they are not set in UniqueRestDefs.
    ///<summary>
    public class UniqueRestDef : DurationDef, IUniqueDef
    {
        public UniqueRestDef(int msPosition, int msDuration)
            : base(msDuration)
        {
            MsPosition = msPosition;
        }

        /// <summary>
        /// Used when loading a score or getting a UniqueRestDef from a palette.
        /// The MsPosition is initially 0. When loading a score, the MsPosition will
        /// eventually be set by accumulating durations when the score has finished loading.
        /// </summary>
        /// <param name="msDuration"></param>
        public UniqueRestDef(int msDuration)
            : base(msDuration)
        {
            MsPosition = 0; // Default. This value will be set later.
        }

        public override string ToString()
        {
            return ("MsPosition=" + MsPosition.ToString() + " MsDuration=" + MsDuration.ToString() + " UniqueRestDef" );
        }

        public IUniqueDef DeepClone()
        {
            UniqueRestDef umrd = new UniqueRestDef(this.MsPosition, this.MsDuration);
            return umrd;
        }

        /// <summary>
        /// Multiplies the MsDuration by the given factor.
        /// </summary>
        /// <param name="factor"></param>
        public void AdjustDuration(double factor)
        {
            MsDuration = (int)(_msDuration * factor);
        }

        public int MsPosition { get { return _msPosition; } set { _msPosition = value; } }
        private int _msPosition = 0;
    }
}
