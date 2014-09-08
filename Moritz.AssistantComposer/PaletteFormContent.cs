using System.Collections.Generic;
using System.Diagnostics;

using Krystals4ObjectLibrary;
using Moritz.Globals;
using Moritz.Krystals;
using Moritz.Score;
using Moritz.Score.Midi;
using Moritz.Score.Notation;
using Moritz.AssistantPerformer;

namespace Moritz.AssistantComposer
{
    public class PaletteFormContent
    {
        public PaletteFormContent(PaletteForm paletteForm)
        {
            Title = paletteForm.Text;
            _domain = paletteForm.Domain;

            BasicChordSettings bcs = BasicChordSettings;

            bcs.Durations = M.StringToIntList(paletteForm.BasicChordControl.DurationsTextBox.Text, ',');
            bcs.Velocities = M.StringToByteList(paletteForm.BasicChordControl.VelocitiesTextBox.Text, ',');
            bcs.MidiPitches = M.StringToByteList(paletteForm.BasicChordControl.MidiPitchesTextBox.Text, ',');
            bcs.ChordOffs = M.StringToBoolList(paletteForm.BasicChordControl.ChordOffsTextBox.Text, ',');
            bcs.ChordDensities = M.StringToByteList(paletteForm.BasicChordControl.ChordDensitiesTextBox.Text, ',');
            bcs.Inversions = paletteForm.GetLinearInversions(paletteForm.BasicChordControl.RootInversionTextBox.Text);
            bcs.InversionIndices = M.StringToIntList(paletteForm.BasicChordControl.InversionIndicesTextBox.Text, ',');
            bcs.VerticalVelocityFactors = M.StringToFloatList(paletteForm.BasicChordControl.VerticalVelocityFactorsTextBox.Text, ',');

            BankIndices = M.StringToByteList(paletteForm.BankIndicesTextBox.Text, ',');
            PatchIndices = M.StringToByteList(paletteForm.PatchIndicesTextBox.Text, ',');
            Volumes = M.StringToByteList(paletteForm.VolumesTextBox.Text, ',');
            Repeats = M.StringToBoolList(paletteForm.RepeatsTextBox.Text, ',');
            PitchwheelDeviations = M.StringToByteList(paletteForm.PitchwheelDeviationsTextBox.Text, ',');
            PitchwheelEnvelopes = M.StringToByteLists(paletteForm.PitchwheelEnvelopesTextBox.Text);
            PanEnvelopes = M.StringToByteLists(paletteForm.PanEnvelopesTextBox.Text);
            ModulationWheelEnvelopes = M.StringToByteLists(paletteForm.ModulationWheelEnvelopesTextBox.Text);
            ExpressionEnvelopes = M.StringToByteLists(paletteForm.ExpressionEnvelopesTextBox.Text);

            OrnamentNumbers = M.StringToIntList(paletteForm.OrnamentNumbersTextBox.Text, ',');
            OrnamentMinMsDurations = M.StringToIntList(paletteForm.MinMsDurationsTextBox.Text, ',');

            if(paletteForm.OrnamentSettingsForm != null)
            {
                OrnamentSettings = new OrnamentSettings(paletteForm);
            }

        }

        public PaletteFormContent(PercussionPaletteForm ppf)
        {
            Title = ppf.Text;
            _domain = ppf.Domain;

            BasicChordSettings bcs = BasicChordSettings;

            bcs.Durations = M.StringToIntList(ppf.BasicChordControl.DurationsTextBox.Text, ',');
            bcs.Velocities = M.StringToByteList(ppf.BasicChordControl.VelocitiesTextBox.Text, ',');
            bcs.MidiPitches = M.StringToByteList(ppf.BasicChordControl.MidiPitchesTextBox.Text, ',');

            //percussion defaults
            bcs.ChordOffs = new List<bool>();
            bcs.ChordDensities = new List<byte>();
            bcs.Inversions = new List<List<byte>>();
            bcs.InversionIndices = new List<int>();
            bcs.VerticalVelocityFactors = new List<float>();

            BankIndices = new List<byte>();
            PatchIndices = new List<byte>();
            PitchwheelDeviations = new List<byte>();
            PitchwheelEnvelopes = new List<List<byte>>();
            ModulationWheelEnvelopes = new List<List<byte>>();
 
            foreach(byte pitch in bcs.MidiPitches)
            {
                bcs.ChordOffs.Add(false); // Moritz never sends ChordOffs to channel 9
                bcs.ChordDensities.Add((byte)1); // Moritz only ever uses 1 percussion instrument per chord.
            }
            
            Volumes = M.StringToByteList(ppf.VolumesTextBox.Text, ',');
            PanEnvelopes = ppf.GetEnvelopesAsByteLists(ppf.PanEnvelopesTextBox);
            ExpressionEnvelopes = ppf.GetEnvelopesAsByteLists(ppf.ExpressionEnvelopesTextBox);

            OrnamentNumbers = M.StringToIntList(ppf.OrnamentNumbersTextBox.Text, ',');
            OrnamentMinMsDurations = M.StringToIntList(ppf.MinMsDurationsTextBox.Text, ',');
 
            // instruments in ornaments palette are absolute, not relative, so 'ignore' the
            // basic instrument index by setting it to 0.
            for(int i = 0; i < OrnamentNumbers.Count; ++i)
            {
                if(OrnamentNumbers[i] != 0) // is an ornament
                {
                    bcs.MidiPitches[i] = 0;
                }
            }

            if(ppf.PercussionOrnamentsForm != null)
            {
                OrnamentSettings = new OrnamentSettings(ppf);
            }
        }

        /// <summary>
        /// Returns either a new RestDef or a new MidiChordDef
        /// In both cases, MsPosition is set to zero, Lyric is set to null.
        /// </summary>
        public DurationDef GetDurationDef(int index)
        {
            DurationDef rval = null;

            if(BasicChordSettings.ChordDensities[index] == 0)
            {
                /// RestDefs are immutable, and have no MsPosition property.
                /// UniqueRestDefs are mutable RestDefs with both MsPositon and MsDuration properties.
                int restMsDuration = BasicChordSettings.Durations[index];
                rval = new RestDef(0, restMsDuration);
            }
            else
            {
                /// Create a new MidiChordDef (with msPosition=0, lyric=null) 
                BasicChordSettings bcd = BasicChordSettings;

                int msDuration = bcd.Durations[index];
                byte? bank = ByteOrNull(BankIndices, index);
                byte? patch = ByteOrNull(PatchIndices, index);
                byte? volume = ByteOrNull(Volumes, index);
                bool repeat = Repeats[index];
                byte? pitchwheelDeviation = ByteOrNull(PitchwheelDeviations, index);
                bool hasChordOff = (bcd.ChordOffs != null && index < bcd.ChordOffs.Count) ? bcd.ChordOffs[index] : true;
                int ornamentMinMsDuration = (OrnamentMinMsDurations != null && index < OrnamentMinMsDurations.Count) ? OrnamentMinMsDurations[index] : 1;

                List<byte> midiPitches;
                List<byte> midiVelocities;
                if(bcd.ChordDensities[index] == 1)
                {
                    midiPitches = new List<byte>();
                    midiPitches.Add(bcd.MidiPitches[index]);
                    midiVelocities = new List<byte>();
                    midiVelocities.Add(bcd.Velocities[index]); 
                }
                else
                {
                    //GetMidiPitches(byte basePitch, int density, List<List<byte>> inversions, int inversionIndex)
                    midiPitches = GetMidiPitches(bcd.MidiPitches[index], bcd.ChordDensities[index], bcd.Inversions, bcd.InversionIndices[index]);
                    midiVelocities = GetVerticalVelocities(bcd.Velocities[index], bcd.ChordDensities[index], bcd.VerticalVelocityFactors[index]);
                }

                byte midiVelocity = bcd.Velocities[index];
                int ornamentNumberSymbol = OrnamentNumbers[index];

                MidiChordSliderDefs midiChordSliderDefs = null;
                List<byte> pitchwheelEnvelope = ListByteOrNull(PitchwheelEnvelopes, index);
                List<byte> panEnvelope = ListByteOrNull(PanEnvelopes, index);
                List<byte> modulationWheelEnvelope = ListByteOrNull(ModulationWheelEnvelopes, index);
                List<byte> expressionEnvelope = ListByteOrNull(ExpressionEnvelopes, index);
                if(pitchwheelEnvelope != null || panEnvelope != null || modulationWheelEnvelope != null || expressionEnvelope != null)
                {
                    midiChordSliderDefs = new MidiChordSliderDefs(pitchwheelEnvelope,
                        panEnvelope,
                        modulationWheelEnvelope,
                        expressionEnvelope);
                }

                OrnamentSettings os = OrnamentSettings;
                Debug.Assert(os != null);
                BasicChordSettings obcs = os.BasicChordSettings;
                Debug.Assert(obcs != null);
                List<BasicMidiChordDef> basicMidiChordDefs = new List<BasicMidiChordDef>();
                for(int oIndex = 0; oIndex < this.OrnamentNumbers.Count; ++oIndex)
                {
                    int oMsDuration = obcs.Durations[oIndex];
                    byte? oBank = ByteOrNull(os.BankIndices, index);
                    byte? oPatch = ByteOrNull(os.PatchIndices, index);
                    bool oHasChordOff = (obcs.ChordOffs != null && index < obcs.ChordOffs.Count) ? obcs.ChordOffs[index] : true;

                    List<byte> oMidiPitches;
                    List<byte> oVVelocities;
                    if(obcs.ChordDensities[oIndex] == 1)
                    {
                        oMidiPitches = new List<byte>();
                        oMidiPitches.Add(obcs.MidiPitches[oIndex]);
                        oVVelocities = new List<byte>();
                        oVVelocities.Add(obcs.Velocities[oIndex]);
                    }
                    else
                    {
                        oMidiPitches = GetMidiPitches(obcs.MidiPitches[oIndex], obcs.ChordDensities[oIndex], obcs.Inversions, obcs.InversionIndices[oIndex]);
                        oVVelocities = GetVerticalVelocities(obcs.Velocities[oIndex], obcs.ChordDensities[oIndex], obcs.VerticalVelocityFactors[oIndex]);
                    }

                    BasicMidiChordDef bmcd = new BasicMidiChordDef(oMsDuration, oBank, oPatch, oHasChordOff, oMidiPitches, oVVelocities);
                    basicMidiChordDefs.Add(bmcd);
                }

                // The basicMidiChordDefs in the ornament currently contain chords having the correct vertical density,
                // but relative durations, relative piches and relative velocities.

                basicMidiChordDefs = MidiChordDef.FitToDuration(basicMidiChordDefs, msDuration, ornamentMinMsDuration);
                foreach(BasicMidiChordDef b in basicMidiChordDefs)
                {
                    List<byte> combinedPitches = new List<byte>();
                    foreach(byte pitch in b.Pitches)
                    {
                        foreach(byte rootPitch in midiPitches)
                        {
                            combinedPitches.Add(M.MidiValue(rootPitch + pitch));
                        }
                    }
                    b.Pitches = combinedPitches;

                    List<byte> combinedVelocities = new List<byte>();
                    foreach(byte velocity in b.Velocities)
                    {
                        foreach(byte rootVelocity in midiVelocities)
                        {
                            combinedVelocities.Add(M.MidiValue(rootVelocity + velocity));
                        }
                    }
                    b.Velocities = combinedVelocities;
                }

                rval = new MidiChordDef(
                    msDuration,
                    bank,
                    patch,
                    volume,
                    repeat,
                    pitchwheelDeviation,
                    hasChordOff,
                    midiPitches,
                    midiVelocities,
                    ornamentNumberSymbol,
                    midiChordSliderDefs,
                    basicMidiChordDefs);
            }
            return rval;
        }

        private byte? ByteOrNull(List<byte> values, int index)
        {
            return (values != null && index < values.Count) ? (byte?)values[index] : null;
        }

        private List<byte> ListByteOrNull(List<List<byte>> values, int index)
        {
            return (values != null && index < values.Count) ? (List<byte>) values[index] : null;
        }

        private List<byte> GetMidiPitches(byte basePitch, int density, List<List<byte>> inversions, int inversionIndex)
        {
            List<byte> midiPitches = new List<byte>();

            List<byte> primeIntervals = new List<byte>();
            Debug.Assert(inversions != null);
            if(inversions.Count > 1)
            {
                Debug.Assert(inversions.Count > inversionIndex);
                primeIntervals = inversions[inversionIndex];
            }
            else if(inversions.Count == 1)
                primeIntervals.Add(inversions[0][0]);
            // If krystalPalette.Inversions.Count == 0, primeIntervals is empty.

            byte pitch = basePitch;
            for(int p = 0; p < density; p++)
            {
                midiPitches.Add(pitch);
                if(p < (density - 1))
                {
                    int newpitch = pitch + primeIntervals[p];
                    pitch = (byte)((newpitch > 127) ? 127 : newpitch);
                }
            }
            return midiPitches;
        }

        private List<byte> GetVerticalVelocities(byte rootVelocity, int vDensity, float vvFactor)
        {
            List<byte> midiVelocities = new List<byte>();

            if(vvFactor == 1F || vDensity == 1)
            {
                for(int i = 0; i < vDensity; ++i)
                {
                    midiVelocities.Add((byte)rootVelocity);
                }
            }
            else
            {
                float bottomVelocity = rootVelocity;
                if(vvFactor > 1.0F)
                    bottomVelocity = bottomVelocity / vvFactor;
                float topVelocity = bottomVelocity * vvFactor;
                float velocityDifference = (topVelocity - bottomVelocity) / ((float)(vDensity - 1));
                float newVelocity = bottomVelocity;
                for(int i = 0; i < vDensity; ++i)
                {
                    midiVelocities.Add((byte)newVelocity);

                    newVelocity += velocityDifference;
                    newVelocity = newVelocity < 0F ? 0F : newVelocity;
                    newVelocity = newVelocity > 127F ? 127F : newVelocity;
                }
            }
            return midiVelocities;
        }

        public string Title = null; // can be used while debugging

        public BasicChordSettings BasicChordSettings = new BasicChordSettings();

        public List<byte> BankIndices;
        public List<byte> PatchIndices;
        public List<byte> Volumes;
        public List<bool> Repeats;
        public List<byte> PitchwheelDeviations;
        public List<List<byte>> PitchwheelEnvelopes;
        public List<List<byte>> PanEnvelopes;
        public List<List<byte>> ModulationWheelEnvelopes;
        public List<List<byte>> ExpressionEnvelopes; 
        
        public List<int> OrnamentNumbers;
        public List<int> OrnamentMinMsDurations;
      
        // OrnamentSettings (may be null)
        public OrnamentSettings OrnamentSettings = null;

        public int Domain { get { return _domain; } }
        private readonly int _domain = 0;

    }

    public class OrnamentSettings
    {
        public OrnamentSettings(PaletteForm kpf)
        {
            OrnamentSettingsForm osf = kpf.OrnamentSettingsForm;
            Debug.Assert(osf != null);
            OrnamentsKrystal = osf.OrnamentsKrystal;
            OrnamentLevel = int.Parse(osf.OrnamentsLevelTextBox.Text);
            BasicChordSettings bcs = BasicChordSettings;
            /// relative durations
            bcs.Durations = M.StringToIntList(osf.BasicChordControl.DurationsTextBox.Text, ',');
            /// velocity increments
            bcs.Velocities = M.StringToByteList(osf.BasicChordControl.VelocitiesTextBox.Text, ',');
            /// transposition intervals
            bcs.MidiPitches = M.StringToByteList(osf.BasicChordControl.MidiPitchesTextBox.Text, ',');
            bcs.ChordOffs = M.StringToBoolList(osf.BasicChordControl.ChordOffsTextBox.Text, ',');
            bcs.ChordDensities = M.StringToByteList(osf.BasicChordControl.ChordDensitiesTextBox.Text, ',');
            bcs.Inversions = kpf.GetLinearInversions(osf.BasicChordControl.RootInversionTextBox.Text);
            bcs.InversionIndices = M.StringToIntList(osf.BasicChordControl.InversionIndicesTextBox.Text, ',');
            bcs.VerticalVelocityFactors = M.StringToFloatList(osf.BasicChordControl.VerticalVelocityFactorsTextBox.Text, ',');

            BankIndices = M.StringToByteList(osf.BankIndicesTextBox.Text, ',');
            PatchIndices = M.StringToByteList(osf.PatchIndicesTextBox.Text, ',');

        }

        public OrnamentSettings(PercussionPaletteForm ppf)
        {
            PercussionOrnamentsForm pof = ppf.PercussionOrnamentsForm;
            Debug.Assert(pof != null);
            OrnamentsKrystal = pof.OrnamentsKrystal;
            OrnamentLevel = int.Parse(pof.OrnamentsLevelTextBox.Text);
            BasicChordSettings bcs = BasicChordSettings;

            //percussion defaults
            bcs.ChordOffs = new List<bool>();
            bcs.ChordDensities = new List<byte>();
            bcs.Inversions = new List<List<byte>>();
            bcs.InversionIndices = new List<int>();
            bcs.VerticalVelocityFactors = new List<float>();

            bcs.Durations = M.StringToIntList(pof.BasicPercussionControl.DurationsTextBox.Text, ',');
            /// velocity increments
            bcs.Velocities = M.StringToByteList(pof.BasicPercussionControl.VelocitiesTextBox.Text, ',');
            /// transposition intervals (for percussion ornaments, the "base pitch=instrument" has been set to 0.
            bcs.MidiPitches = M.StringToByteList(pof.BasicPercussionControl.MidiPitchesTextBox.Text, ',');

            foreach(byte pitch in bcs.MidiPitches)
            {
                bcs.ChordOffs.Add(false); // Moritz never sends ChordOffs to channel 9
                bcs.ChordDensities.Add((byte)1); // Moritz only ever uses 1 percussion instrument per chord.
            }

            BankIndices = new List<byte>();
            PatchIndices = new List<byte>();
        }

        /// <summary>
        /// 'byte' types are midi (msb) parameters in range 0..127.
        /// </summary>
        public Krystal OrnamentsKrystal = null;
        public int OrnamentLevel;
        public BasicChordSettings BasicChordSettings = new BasicChordSettings();
        public List<byte> BankIndices;
        public List<byte> PatchIndices;
    }

    public class BasicChordSettings
    {
        /// <summary>
        /// 'byte' types are midi (msb) parameters in range 0..127.
        /// </summary>
        public List<int> Durations;
        public List<byte> Velocities;
        public List<byte> MidiPitches;
        public List<bool> ChordOffs;
        public List<byte> ChordDensities;
        // The following is a linear array calculated from ornament.rootInversion;
        // If Inversions.Count == 0, then there is a maximum chord density of 1, and there are no inversions.
        // If Inversions.Count == 1, then the contained list contains a single interval for a 2-note chord.
        // Otherwise Inversions.Count == (n-1)*2, where the contained intLists.Count == n.
        public List<List<byte>> Inversions;
        public List<int> InversionIndices;
        public List<float> VerticalVelocityFactors;
    }
}
