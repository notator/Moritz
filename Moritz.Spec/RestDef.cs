
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
        public RestDef(int msPositionReTrk, int msDuration)
            : base(msDuration)
        {
            MsPositionReTrk = msPositionReTrk;
        }

        public override string ToString()
        {
            return ("MsPositionReTrk=" + MsPositionReTrk.ToString() + " MsDuration=" + MsDuration.ToString() + " RestDef" );
        }

        public override IUniqueDef Clone()
        {
            RestDef umrd = new RestDef(this.MsPositionReTrk, this.MsDuration);
            return umrd;
        }

        /// <summary>
        /// Multiplies the MsDuration by the given factor.
        /// </summary>
        /// <param name="factor"></param>
        public void AdjustMsDuration(double factor)
        {
            MsDuration = (int)(_msDuration * factor);
        }

        public int MsPositionReTrk { get { return _msPositionReTrk; } set { _msPositionReTrk = value; } }
        private int _msPositionReTrk = 0;
    }
}
