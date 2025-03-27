using Moritz.Xml;

namespace Moritz.Spec
{
    ///<summary>
    /// A MidiRestDef is a unique rest definition which is saved locally in an SVG file.
    /// Each rest in an SVG file will be given an ID of the form "rest"+uniqueNumber, but
    /// Moritz does not actually use the ids, so they are not set in UniqueRestDefs.
    ///<summary>
    public class MidiRestDef : RestDef, IUniqueDef
    {
        public MidiRestDef(int msPositionReFirstIUD, int msDuration)
            : base(msPositionReFirstIUD, msDuration)
        {
        }

        public override string ToString()
        {
            return ("MidiRestDef: MsPositionReFirstIUD=" + MsPositionReFirstUD.ToString() + " MsDuration=" + MsDuration.ToString());
        }

        public override object Clone()
        {
            MidiRestDef umrd = new MidiRestDef(this.MsPositionReFirstUD, this.MsDuration);
            return umrd;
        }

        public void WriteSVG(SvgWriter w)
        {
            w.WriteStartElement("midiRest");

            w.WriteAttributeString("msDuration", _msDuration.ToString());

            w.WriteEndElement(); // score:midiRest
        }
    }
}
