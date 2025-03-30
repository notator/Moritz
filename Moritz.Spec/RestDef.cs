
using Moritz.Xml;

using System;
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
            return new RestDef(this.MsPositionReFirstUD, this.MsDuration);
        }

        public void WriteSVG(SvgWriter w)
		{
			w.WriteStartElement("midiRest");

			w.WriteAttributeString("msDuration", MsDuration.ToString());

			w.WriteEndElement(); // score:midiRest
		}

		public override string ToString()
		{
			return ("RestDef: MsPositionReFirstIUD=" + MsPositionReFirstUD.ToString() + " MsDuration=" + MsDuration.ToString());
		}


    }
}
