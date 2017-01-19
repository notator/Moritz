
using System.Diagnostics;

using Krystals4ObjectLibrary;
using Moritz.Globals;

namespace Moritz.Spec
{
    ///<summary>
    /// An InputRestDef is a unique rest definition which is saved locally in an SVG file.
    /// Each rest in an SVG file will be given an ID of the form "rest"+uniqueNumber, but
    /// Moritz does not actually use the ids, so they are not set in UniqueRestDefs.
    ///<summary>
    public class InputRestDef : RestDef, IUniqueDef
    {
        public InputRestDef(int msPositionReFirstIUD, int msDuration)
            : base(msPositionReFirstIUD, msDuration)
        {
        }

        public override string ToString()
        {
            return ("InputRestDef: MsPositionReFirstIUD = " + MsPositionReFirstUD.ToString() + " MsDuration=" + MsDuration.ToString());
        }

        public override object Clone()
        {
            InputRestDef umrd = new InputRestDef(this.MsPositionReFirstUD, this.MsDuration);
            return umrd;
        }
    }
}
