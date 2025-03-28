
using System.Diagnostics;

namespace Moritz.Spec
{
    ///<summary>
    /// A RestDef is a unique rest definition which is saved locally in an SVG file.
    /// Each rest in an SVG file will be given an ID of the form "rest"+uniqueNumber, but
    /// Moritz does not actually use the ids, so they are not set in UniqueRestDefs.
    ///<summary>
    public abstract class RestDef : DurationDef, IUniqueDef
    {
        public RestDef(int msPositionReFirstIUD, int msDuration)
            : base(msDuration)
        {
            MsPositionReFirstUD = msPositionReFirstIUD;
        }

        /// <summary>
        /// Multiplies the MsDuration by the given factor.
        /// </summary>
        /// <param name="factor"></param>
        public void AdjustMsDuration(double factor)
        {
            MsDuration = (int)(MsDuration * factor);
            Debug.Assert(MsDuration > 0, "A UniqueDef's MsDuration may not be set to zero!");
        }
    }
}
