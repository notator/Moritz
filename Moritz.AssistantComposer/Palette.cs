using System.Collections.Generic;
using System.Diagnostics;

using Krystals4ObjectLibrary;
using Moritz.Globals;


namespace Moritz.AssistantComposer
{
    public class Palette
    {
        public Palette(PaletteForm paletteForm)
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

        public Palette(PercussionPaletteForm ppf)
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

        public string Title = null;
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
