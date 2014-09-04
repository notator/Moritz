using System;
using System.Xml;
using System.Diagnostics;

namespace Moritz.Score.Notation
{
    /// A RestDef is a definition retrieved from a palette.
    /// RestDefs are immutable, and have no MsPosition property.
    /// UniqueRestDefs are mutable RestDefs with both MsPositon and MsDuration properties.
    public class RestDef : DurationDef
    {
        public RestDef(int msDuration)
            : base(msDuration)
        {
        }

        public override IUniqueDef DeepClone()
        {
            UniqueRestDef umrd = new UniqueRestDef(this);
            return umrd;
        }

        public override int MsDuration
        {
            get { return _msDuration; }
            set
            {
                throw new ApplicationException("RestDefs are immutable, try using a UniqueRestDef instead.");
            }
        }
    }
}
