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
            return ("MidiRestDef:" + " MsDuration=" + MsDuration.ToString() + " MsPositionReFirstIUD =" + MsPositionReFirstUD.ToString());
        }

        public override object Clone()
        {
            MidiRestDef umrd = new MidiRestDef(this.MsPositionReFirstUD, this.MsDuration);
            return umrd;
        }

        public void WriteSvg(SvgWriter w)
        {
            w.WriteStartElement("score", "midi", null);

            //Debug.Assert(BasicMidiChordDefs != null && BasicMidiChordDefs.Count > 0);

            //if(BasicMidiChordDefs[0].BankIndex == null && Bank != null)
            //{
            //    BasicMidiChordDefs[0].BankIndex = Bank;
            //}
            //if(BasicMidiChordDefs[0].PatchIndex == null && Patch != null)
            //{
            //    BasicMidiChordDefs[0].PatchIndex = Patch;
            //}
            //if(HasChordOff == false)
            //    w.WriteAttributeString("hasChordOff", "0");
            //if(PitchWheelDeviation != null && PitchWheelDeviation != M.DefaultPitchWheelDeviation)
            //    w.WriteAttributeString("pitchWheelDeviation", PitchWheelDeviation.ToString());
            //if(MinimumBasicMidiChordMsDuration != M.DefaultMinimumBasicMidiChordMsDuration)
            //    w.WriteAttributeString("minBasicChordMsDuration", MinimumBasicMidiChordMsDuration.ToString());

            //w.WriteStartElement("basicChords");
            //foreach(BasicMidiChordDef basicMidiChord in BasicMidiChordDefs) // containing basic <midiChord> elements
            //    basicMidiChord.WriteSVG(w);
            //w.WriteEndElement();

            //if(MidiChordSliderDefs != null)
            //    MidiChordSliderDefs.WriteSVG(w); // writes sliders element

            w.WriteEndElement(); // score:midi
        }
    }
}
