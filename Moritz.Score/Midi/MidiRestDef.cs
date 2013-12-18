
using System.Xml;
using System.Diagnostics;

namespace Moritz.Score.Midi
{
    public class MidiRestDef : MidiDurationDef
    {
        public MidiRestDef(string id, int msDuration)
            :base(msDuration)
        {
            ID = id;  // this is the id of a rest in the score
        }

        public MidiRestDef(XmlReader r)
        {
            // The reader is at the beginning of a "score:midiRest" element having an ID attribute
            Debug.Assert(r.Name == "score:midiRest" && r.IsStartElement() && r.AttributeCount > 0);
            int nAttributes = r.AttributeCount;
            for(int i = 0; i < nAttributes; i++)
            {
                r.MoveToAttribute(i);
                switch(r.Name)
                {
                    case "id":
                        ID = r.Value; // this is the id of a rest in a palette definition
                        break;
                    case "msDuration":
                        _msDuration = int.Parse(r.Value);
                        break;
                }
            }
        }

        public override IUniqueMidiDurationDef CreateUniqueMidiDurationDef()
        {
            UniqueMidiRestDef umrd = new UniqueMidiRestDef(this);
            umrd.MsPosition = 0;
            umrd.MsDuration = this.MsDuration;
            return umrd;
        }

        public readonly string ID;
    }
}
