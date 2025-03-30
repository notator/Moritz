
using Moritz.Xml;

using System.Diagnostics;

namespace Moritz.Spec
{
    ///<summary>
    /// A RestDef is a unique rest definition which is saved in an SVG file.
    ///<summary>
    public class RestDef : DurationDef, IUniqueDef
    {
        public RestDef(int msPositionReFirstIUD, int msDuration)
            : base(msDuration)
        {
            MsPositionReFirstUD = msPositionReFirstIUD;
        }

        public override object Clone()
        {
            RestDef umrd = new RestDef(this.MsPositionReFirstUD, this.MsDuration);
            return umrd;
        }

        public void WriteSVG(SvgWriter w)
        {
            w.WriteStartElement("midiRest");

            w.WriteAttributeString("msDuration", MsDuration.ToString());

            w.WriteEndElement(); // score:midiRest
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

        public override string ToString()
        {
            return ("RestDef: MsPositionReFirstIUD=" + MsPositionReFirstUD.ToString() + " MsDuration=" + MsDuration.ToString());
        }
    }
}
