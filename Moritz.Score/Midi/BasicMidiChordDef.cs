using System.Collections.Generic;
using System.Xml;
using System.Diagnostics;

using Moritz.Globals;

namespace Moritz.Score.Midi
{
    public class BasicMidiChordDef
    {
        public BasicMidiChordDef()
        {
        }

        public BasicMidiChordDef(int msDuration, byte? bank, byte? patch, bool hasChordOff, List<byte> pitches, List<byte> velocities)
        {
            _msDuration = msDuration; // read-only!
            BankIndex = bank;
            PatchIndex = patch;
            HasChordOff = hasChordOff;
            Pitches = pitches;
            Velocities = velocities;

            #region check values
            Debug.Assert(_msDuration > 0, "msDuration out of range");
            Debug.Assert(BankIndex == null || BankIndex == M.MidiValue((int)BankIndex), "Bank out of range.");
            Debug.Assert(PatchIndex == null || PatchIndex == M.MidiValue((int)PatchIndex), "Patch out of range.");
            foreach(byte pitch in Pitches)
                Debug.Assert(pitch == M.MidiValue((int)pitch), "Pitch out of range.");
            foreach(byte velocity in Velocities)
                Debug.Assert(velocity == M.MidiValue((int)velocity), "velocity out of range.");
            #endregion
        }

        public BasicMidiChordDef(BasicMidiChordDef original, int msDuration)
        {
            _msDuration = msDuration; // read-only!
            BankIndex = original.BankIndex;
            PatchIndex = original.PatchIndex;
            HasChordOff = original.HasChordOff;
            Pitches = original.Pitches;
            Velocities = original.Velocities;
        }

        public BasicMidiChordDef(XmlReader r)
        {
            // The reader is at the beginning of a "score:basicChord" element
            // inside a "score:basicChords" element inside a "score:midiChord" element.
            Debug.Assert(r.Name == "score:basicChord" && r.IsStartElement() && r.AttributeCount > 0);
            int nAttributes = r.AttributeCount;
            for(int i = 0; i < nAttributes; i++)
            {
                r.MoveToAttribute(i);
                switch(r.Name)
                {
                    case "msDuration":
                        _msDuration = int.Parse(r.Value); // read-only!
                        break;
                    case "bank":
                        BankIndex = byte.Parse(r.Value);
                        break;
                    case "patch":
                        PatchIndex = byte.Parse(r.Value);
                        break;
                    case "hasChordOff":
                        // HasChordOff is true if this attribute is not present
                        byte val = byte.Parse(r.Value);
                        if(val == 0)
                            HasChordOff = false;
                        else
                            HasChordOff = true;
                        break;
                    case "pitches":
                        Pitches = M.StringToByteList(r.Value, ' ');
                        break;
                    case "velocities":
                        Velocities = M.StringToByteList(r.Value, ' ');
                        break;
                }
            }
        }

        public void WriteSVG(XmlWriter w)
        {
            w.WriteStartElement("score", "basicChord", null);

            //if(writeMsPosition) // positions are not written to the midiDefs section of an SVG-MIDI file
            //    w.WriteAttributeString("msPosition", MsPosition.ToString());
            w.WriteAttributeString("msDuration", _msDuration.ToString());
            if(BankIndex != null && BankIndex != M.DefaultBankAndPatchIndex)
                w.WriteAttributeString("bank", BankIndex.ToString());
            if(PatchIndex != null)
                w.WriteAttributeString("patch", PatchIndex.ToString());
            if(HasChordOff == false)
                w.WriteAttributeString("hasChordOff", "0");
            if(Pitches != null)
                w.WriteAttributeString("pitches", M.ByteListToString(Pitches));
            if(Velocities != null)
                w.WriteAttributeString("velocities", M.ByteListToString(Velocities));

            w.WriteEndElement();
        }

        public int MsDuration { get { return _msDuration; } set { _msDuration = value; } }
        protected int _msDuration = 0;

        public List<byte> Pitches = new List<byte>(); // A string of midiPitch numbers separated by spaces.
        public List<byte> Velocities = new List<byte>(); // A string of midi velocity values separated by spaces.
        public byte? BankIndex = null; // optional. If null, bank commands are not sent
        public byte? PatchIndex = null; // optional. If null, patch commands are not sent
        public bool HasChordOff = true;
    }
}
