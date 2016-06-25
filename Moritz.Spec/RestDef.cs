
using System.Diagnostics;

using Krystals4ObjectLibrary;
using Moritz.Globals;

namespace Moritz.Spec
{
    ///<summary>
    /// A RestDef is a unique rest definition which is saved locally in an SVG file.
    /// Each rest in an SVG file will be given an ID of the form "rest"+uniqueNumber, but
    /// Moritz does not actually use the ids, so they are not set in UniqueRestDefs.
    ///<summary>
    public class RestDef : DurationDef, IUniqueDef
    {
        public RestDef(int msPositionReFirstIUD, int msDuration)
            : base(msDuration)
        {
            MsPositionReFirstUD = msPositionReFirstIUD;
        }

        public override string ToString()
        {
            return ("MsPositionReFirstIUD=" + MsPositionReFirstUD.ToString() + " MsDuration=" + MsDuration.ToString() + " RestDef" );
        }

        public override object Clone()
        {
            RestDef umrd = new RestDef(this.MsPositionReFirstUD, this.MsDuration);
            return umrd;
        }

        /// <summary>
        /// Multiplies the MsDuration by the given factor.
        /// </summary>
        /// <param name="factor"></param>
        public void AdjustMsDuration(double factor)
        {
            MsDuration = (int)(_msDuration * factor);
            Debug.Assert(MsDuration > 0, "A UniqueDef's MsDuration may not be set to zero!");
        }

        public int MsPositionReFirstUD { get { return _msPositionReFirstIUD; } set { _msPositionReFirstIUD = value; } }
        private int _msPositionReFirstIUD = 0;
    }
}
