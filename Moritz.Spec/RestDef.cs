
using Moritz.Xml;

using System.Collections.Generic;
using System.Diagnostics;

namespace Moritz.Spec
{
	///<summary>
	/// A RestDef is a unique rest definition which is saved in an SVG file.
	///<summary>
	public class RestDef : DurationDef
	{
		public RestDef(int msPositionReFirstIUD, int msDuration)
			: base(msDuration)
		{
			MsPositionReFirstUD = msPositionReFirstIUD;
		}

        public override object Clone()
        {
            RestDef returnVal =  new RestDef(this.MsPositionReFirstUD, this.MsDuration);

            foreach(var midiDef in this.MidiDefs)
            {
                returnVal.MidiDefs.Add((RestDef)midiDef.Clone());
            }

            return returnVal;
        }

        public void WriteSVG(SvgWriter w)
		{
            w.WriteStartElement("score", "midiRests", null);

            // write a list of alternative <midiRest> elements
            for(var mdIndex = 0; mdIndex < MidiDefs.Count; mdIndex++)
            {
                RestDef restDef = MidiDefs[mdIndex] as RestDef;
                Debug.Assert(restDef != null);
                w.WriteStartElement("midiRest");
                w.WriteAttributeString("msDuration", restDef.MsDuration.ToString());
                w.WriteEndElement(); // end midiRest
            }

            w.WriteEndElement(); // end score:midiRests
		}

		public override string ToString()
		{
			return ("RestDef: MsPositionReFirstIUD=" + MsPositionReFirstUD.ToString() + " MsDuration=" + MsDuration.ToString());
		}

        public List<RestDef> MidiDefs { get; set; } = new List<RestDef>();
    }
}
