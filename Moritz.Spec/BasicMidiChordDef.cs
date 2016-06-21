using System.Collections.Generic;
using System.Xml;
using System.Diagnostics;
using Moritz.Globals;

namespace Moritz.Spec
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
            Pitches = new List<byte>(pitches);
            Velocities = new List<byte>(velocities);

            #region check values
            Debug.Assert(_msDuration > 0, "msDuration out of range");
            Debug.Assert(BankIndex == null || BankIndex == M.MidiValue((int)BankIndex), "Bank out of range.");
            Debug.Assert(PatchIndex == null || PatchIndex == M.MidiValue((int)PatchIndex), "Patch out of range.");
            Debug.Assert(Pitches.Count == Velocities.Count, "There must be the same number of pitches and velocities.");
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
            Pitches = new List<byte>(original.Pitches);
            Velocities = new List<byte>(original.Velocities);
        }

        public void WriteSVG(XmlWriter w)
        {
            w.WriteStartElement("basicChord");

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

        /// <summary>
        /// The argument contains a list of 12 velocity values (range [0..127] in order of absolute pitch.
        /// For example: If the MidiChordDef contains one or more C#s, they will be given velocity velocityPerAbsolutePitch[1].
        /// Middle-C is midi pitch 60 (60 % 12 == absolute pitch 0), middle-C# is midi pitch 61 (61 % 12 == absolute pitch 1), etc. 
        /// </summary>
        /// <param name="velocityPerAbsolutePitch">A list of 12 velocity values (range [0..127] in order of absolute pitch</param>
        public void SetVelocityPerAbsolutePitch(List<int> velocityPerAbsolutePitch)
        {
            for(int pitchIndex = 0; pitchIndex < Pitches.Count; ++pitchIndex)
            {
                int absPitch = Pitches[pitchIndex] % 12;
                Velocities[pitchIndex] = (byte)velocityPerAbsolutePitch[absPitch];
            }
        }

        public override string ToString() => $"BasicMidiChordDef: MsDuration={MsDuration.ToString()} BasePitch={Pitches[0]} ";

        public int MsDuration { get { return _msDuration; } set { _msDuration = value; } }
        private int _msDuration = 0;

        public List<byte> Pitches = new List<byte>();
        public List<byte> Velocities = new List<byte>();
        public byte? BankIndex = null; // optional. If null, bank commands are not sent
        public byte? PatchIndex = null; // optional. If null, patch commands are not sent
        public bool HasChordOff = true;
    }
}
