using System.Collections.Generic;

using Moritz.Globals;

namespace Moritz.Score.Midi
{
    /// <summary>
    /// This class is the type of a temporary object used while constructing the MidiScore from the Score.
    /// It is used by MidiDurationSymbols to construct the appropriate MidiCommands for setting the staff
    /// state before sending their notes.
    /// N.B. This ChannelState is not available during performance!
    /// 
    /// Further explanation: ChannelState variables are set at the end of a MidiChord so that a subsequent 
    /// MidiChord can know if it needs to send the corresponding MidiControl message before it sends its Notes.
    /// For example, if an midiChord's final basicMidiChord is played on Patch=13, then the ChannelState.Patch is
    /// set to 13. The subsequent MidiChord's patch may or may not be Patch=13. If it is 8, for example, the 
    /// MidiChord is preceded by a Patch=8 command and the ChannelState.Patch variable is set to 8.
    /// If the *next* MidiChord is also on Patch=8, it does not need to send a Patch command.
    /// </summary>
    public class ChannelState
    {
        /// <summary>
        /// This class keeps track of properties which need to be restored after MoritzControl events have changed
        /// the value of the property during a staffMoment.
        /// A staff moment's ChannelState contains the state of the staff when the moment begins.
        /// </summary>
        public ChannelState()
        {
        }
        public readonly int MidiChannel;

        #region commands
        public bool Pedal = false;
        public bool DataButtonIncremented = false;
        public bool DataButtonDecremented = false;
        public bool Gliss = false;
        public bool ThirdPedal = false;
        public bool SoftPedal = false;
        public bool LegatoPedal = false;
        public bool FadePedal = false;
        public bool GeneralPurposeButton1 = false;
        public bool GeneralPurposeButton2 = false;
        public bool GeneralPurposeButton3 = false;
        public bool GeneralPurposeButton4 = false;
        public bool LocalKeyboard = false;
        public bool OmniMode = false;
        #endregion commands

        #region switches
        public byte Patch = 0;
        public byte Bank = M.DefaultBankIndex;
        public byte PitchWheelDeviation = M.DefaultPitchWheelDeviation;
        #endregion switches

        #region sliders
        /// <summary>
        /// These public properties can also be set while constructing the MidiScore.
        /// </summary>
        public byte PitchWheel = M.DefaultPitchWheel;
        public ControlContinuation PitchWheelContinuation = ControlContinuation.NoChange;
        public byte ModulationWheel = M.DefaultModulationWheel;
        public ControlContinuation ModulationWheelContinuation = ControlContinuation.NoChange;
        public byte Volume = M.DefaultVolume;
        public ControlContinuation VolumeContinuation = ControlContinuation.NoChange;
        public byte Pan = M.DefaultPan;
        public ControlContinuation PanContinuation = ControlContinuation.NoChange;
        public byte Expression = M.DefaultExpression;      
        public ControlContinuation ExpressionContinuation = ControlContinuation.NoChange;
        #endregion sliders

        #region Moritz controls (see performOrnamentsExample.mpch patch)
        /// <summary>
        /// Range 0..Maxint
        /// </summary>
        public uint KeyboardNumber = 0;
        /// <summary>
        /// Range 0.0F..100.0F
        /// </summary>
        public float VerticalVelocityFactor = 1.0F;

        public List<List<int>> Ornaments = new List<List<int>>(); // indexed by OrnamentIndex to give indexes in the other ornament settings
        public int OrnamentChordNumberDomain = 0;
        public int OrnamentRootChordIndex = 0; // ~dp[0]: -- range unlimited (actual value is mod ornament length)

        // from old OrnamentNode (OrnamentDurationsType is new)
        public List<int> OrnamentChordDurations = new List<int>(); // ~d: [0..x] (e.g.milliseconds)
        public List<int> OrnamentChordTranspositions = new List<int>(); // ~t: [-127..+127] semitones
        public List<byte> OrnamentChordVelocities = new List<byte>(); // ~ve:  [0..100] -- relative to chord velocity (ve is vElocity, not vOlume)!

        // from old SetChordNotesNode
        public List<int> OrnamentChordDensities = new List<int>(); // ~cd: [1..128] maxDensity is maximum value
        public List<int> OrnamentChordRootInversion = new List<int>(); // ~ri: [1..128] (maxDensity-1 values)
        public List<int> OrnamentChordInversionNumbers = new List<int>(); // ~ci: (values in range [1..(2*(maxDensity-1))] )

        // from old SetChordPitchWheelNode
        public byte OrnamentPitchWheelDeviation = 2; // ~pwd: [0..127] semitones 
        public List<byte> OrnamentChordPitchWheelSettings = new List<byte>(); // ~pw: [0..127] 

        //// new, analog to OrnamentChordVelocities
        //public List<byte> OrnamentChordExpression = new List<byte>(); // ~e: [0..127]

        // from old SetChordInstrumentNode (OrnamentChordBankIndices is new)
        public List<byte> OrnamentChordPatchIndices = new List<byte>();  // ~i: [0..127] 
        public List<byte> OrnamentChordBankIndices = new List<byte>(); // ~b: [0..127]

        // from old SetChordPanNode (range changed to [0..127])
        public List<byte> OrnamentChordPanPositions = new List<byte>(); //~pan: [0..127] left..right

        // moved out of old SetChordNotesNode 
        public List<float> OrnamentChordVerticalVelocityFactors = new List<float>(); // ~vvf: [0.0..100.0]

        #endregion Moritz controls
    }
}
