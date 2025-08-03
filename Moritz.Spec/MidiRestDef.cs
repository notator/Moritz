
using Moritz.Xml;

using System.Collections.Generic;
using System.Diagnostics;

namespace Moritz.Spec
{
	///<summary>
	/// A RestDef is a unique rest definition which is saved in an SVG file.
	///<summary>
	public class MidiRestDef : DurationDef
	{
		public MidiRestDef(int msPositionReFirstIUD, int msDuration)
			: base(msDuration)
		{
			MsPositionReFirstUD = msPositionReFirstIUD;
		}

        public override object Clone()
        {
            MidiRestDef returnVal =  new MidiRestDef(this.MsPositionReFirstUD, this.MsDuration);

            foreach(var midiDef in this.MidiDefs)
            {
                returnVal.MidiDefs.Add((MidiRestDef)midiDef.Clone());
            }

            return returnVal;
        }

        public void WriteSVG(SvgWriter w)
		{
            w.WriteStartElement("score", "midiRests", null);

            // write a list of alternative <midiRest> elements
            for(var mdIndex = 0; mdIndex < MidiDefs.Count; mdIndex++)
            {
                MidiRestDef restDef = MidiDefs[mdIndex] as MidiRestDef;
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

        public List<MidiRestDef> MidiDefs { get; set; } = new List<MidiRestDef>();
    }
}
