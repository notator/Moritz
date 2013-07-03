using System.Collections.Generic;
using System.Diagnostics;

using Multimedia.Midi;

using Moritz.Globals;
using Moritz.Score.Midi;

namespace Moritz.AssistantComposer
{
     //<summary>
     // A LocalMidiChordDef is saved locally in SVG files,
     // not as a 'used' reference to a MidiChordDef definition in a palette.
     //</summary>
    internal class LocalMidiChordDef : MidiChordDef
    {
        /// <summary>
        /// A deep clone of the argument
        /// </summary>
        /// <param name="midiChordDef"></param>
        public LocalMidiChordDef(MidiChordDef mcd)
        {
            ID = "localChord" + UniqueChordID.ToString();

            _bank = mcd.Bank;
            _patch = mcd.Patch;
            _volume = mcd.Volume;
            _pitchWheelDeviation = mcd.PitchWheelDeviation;
            _hasChordOff = mcd.HasChordOff;
            _minimumBasicMidiChordMsDuration = mcd.MinimumBasicMidiChordMsDuration;

            _msDuration = mcd.MsDuration;
            _volume = mcd.Volume;

             _midiVelocitySymbol = mcd.MidiVelocitySymbol;
            _ornamentNumberSymbol = mcd.OrnamentNumberSymbol;
            _midiHeadSymbols = new List<byte>(mcd.MidiHeadSymbols);

            MidiChordSliderDefs mcsd = mcd.MidiChordSliderDefs;
            if(mcsd != null)
            {
                List<byte> pwMsbs = null;
                List<byte> panMsbs = null;
                List<byte> mwMsbs = null;
                List<byte> eMsbs = null;
                if(mcsd.PitchWheelMsbs != null && mcsd.PitchWheelMsbs.Count > 0)
                {
                    pwMsbs = new List<byte>(mcsd.PitchWheelMsbs);
                }
                if(mcsd.PanMsbs != null && mcsd.PanMsbs.Count > 0)
                {
                    panMsbs = new List<byte>(mcsd.PanMsbs);
                }
                if(mcsd.ModulationWheelMsbs != null && mcsd.ModulationWheelMsbs.Count > 0)
                {
                    mwMsbs = new List<byte>(mcsd.ModulationWheelMsbs);
                }
                if(mcsd.ExpressionMsbs != null && mcsd.ExpressionMsbs.Count > 0)
                {
                    eMsbs = new List<byte>(mcsd.ExpressionMsbs);
                }
                if(pwMsbs != null || panMsbs != null || mwMsbs != null || eMsbs != null)
                {
                    MidiChordSliderDefs = new MidiChordSliderDefs(pwMsbs, panMsbs, mwMsbs, eMsbs);
                }
            }
            foreach(BasicMidiChordDef bmcd in mcd.BasicMidiChordDefs)
            {
                List<byte> pitches = new List<byte>(bmcd.Notes);
                List<byte> velocities = new List<byte>(bmcd.Velocities);
                BasicMidiChordDefs.Add(new BasicMidiChordDef(bmcd.MsDuration, bmcd.BankIndex, bmcd.PatchIndex, bmcd.HasChordOff, pitches, velocities));
            }
        }

        /// <summary>
        /// Constructor used when converting a midi file to a list of Moments.
        /// This MidiChordDef contains a single BasicMidiChordDef. It does not support sliders.
        /// </summary>
        public LocalMidiChordDef(List<byte> pitches, List<byte> velocities, int msDuration, bool hasChordOff,
            List<MidiControl> midiControls)
            : base()
        {
            ID = "localChord" + UniqueChordID.ToString();

            _msDuration = msDuration;
            _volume = GetControlHiValue(ControllerType.Volume, midiControls);
            _pitchWheelDeviation = GetControlValue(ControllerType.RegisteredParameterCoarse, midiControls);
            _hasChordOff = hasChordOff;
            _minimumBasicMidiChordMsDuration = 1; // not used (this is not an ornament)

            _midiHeadSymbols = pitches;
            _midiVelocitySymbol = velocities[0];
            _ornamentNumberSymbol = 0;

            MidiChordSliderDefs = null;

            byte? bank = GetControlValue(ControllerType.BankSelect, midiControls);
            byte? patch = GetCommandValue(ChannelCommand.ProgramChange, midiControls);

            BasicMidiChordDefs.Add(new BasicMidiChordDef(msDuration, bank, patch, hasChordOff, pitches, velocities));
        }


        /// <summary>
        /// Constructor used when converting a midi file to a list of Moments.
        /// This MidiChordDef contains a single BasicMidiChordDef. It does not support sliders.
        /// </summary>
        public LocalMidiChordDef(List<byte> pitches, List<byte> velocities, int msDuration, bool hasChordOff,
            List<MidiControlDef> midiControlDefs)
            : base()
        {
            ID = "localChord" + UniqueChordID.ToString();

            _msDuration = msDuration;
            _volume = GetControlDefHiValue(ControllerType.Volume, midiControlDefs);
            _pitchWheelDeviation = GetControlDefValue(ControllerType.RegisteredParameterCoarse, midiControlDefs);
            _hasChordOff = hasChordOff;
            _minimumBasicMidiChordMsDuration = 1; // not used (this is not an ornament)

            _midiHeadSymbols = pitches;
            _midiVelocitySymbol = velocities[0];
            _ornamentNumberSymbol = 0;

            MidiChordSliderDefs = null;

            byte? bank = GetControlDefValue(ControllerType.BankSelect, midiControlDefs);
            byte? patch = GetCommandDefValue(ChannelCommand.ProgramChange, midiControlDefs);

            BasicMidiChordDefs.Add(new BasicMidiChordDef(msDuration, bank, patch, hasChordOff, pitches, velocities));

        }

        private byte? GetControlHiValue(ControllerType controllerType, List<MidiControl> midiControls)
        {
            byte? returnValue = GetControlValue(controllerType, midiControls);
            if(returnValue != null)
            {
                byte value = (byte)(((byte)returnValue) * 16);
                returnValue = value;
            }
            return returnValue;
        }
        private byte? GetControlDefHiValue(ControllerType controllerType, List<MidiControlDef> midiControlDefs)
        {
            byte? returnValue = GetControlDefValue(controllerType, midiControlDefs);
            if(returnValue != null)
            {
                byte value = (byte)(((byte)returnValue) * 16);
                returnValue = value;
            }
            return returnValue;
        }
        /// <summary>
        /// Returns the value of a MidiControl of the given type at the given msPosition in the controls dictionary.
        /// If there are more than one MidiControl of the given type at that position, the value of the last one is returned.
        /// </summary>
        /// <returns></returns>
        private byte? GetControlValue(ControllerType controllerType, List<MidiControl> midiControls)
        {
            MidiControl returnMidiControl = null;
            foreach(MidiControl midiControl in midiControls)
            {
                switch(controllerType)
                {
                    case ControllerType.AllSoundOff:
                    {
                        if(midiControl is AllSoundOff)
                            returnMidiControl = midiControl;
                        break;
                    }
                    case ControllerType.AllNotesOff:
                    {
                        if(midiControl is AllNotesOff)
                            returnMidiControl = midiControl;
                        break;
                    }
                    case ControllerType.AllControllersOff:
                    {
                        if(midiControl is AllControllersOff)
                            returnMidiControl = midiControl;
                        break;
                    }
                    case ControllerType.Balance:
                    {
                        if(midiControl is Balance)
                            returnMidiControl = midiControl;
                        break;
                    }
                    case ControllerType.BankSelect:
                    {
                        if(midiControl is BankControl)
                            returnMidiControl = midiControl;
                        break;
                    }
                    case ControllerType.RegisteredParameterCoarse: // standard Midi for pitchwheel deviation...
                    {
                        if(midiControl is PitchWheelDeviation)
                            returnMidiControl = midiControl;
                        break;
                    }
                    case ControllerType.Expression:
                    {
                        if(midiControl is Expression)
                            returnMidiControl = midiControl;
                        break;
                    }
                    case ControllerType.ModulationWheel:
                    {
                        if(midiControl is ModulationWheel)
                            returnMidiControl = midiControl;
                        break;
                    }
                    case ControllerType.Pan:
                    {
                        if(midiControl is Pan)
                            returnMidiControl = midiControl;
                        break;
                    }
                    case ControllerType.Volume:
                    {
                        if(midiControl is Volume)
                            returnMidiControl = midiControl;
                        break;
                    }
                }
                if(returnMidiControl != null)
                    break;
            }
            if(returnMidiControl == null)
                return null;
            else
                return (byte)returnMidiControl.ChannelMessages[0].Data1;
        }
        /// <summary>
        /// Returns the value of a MidiControl of the given type at the given msPosition in the controls dictionary.
        /// If there are more than one MidiControl of the given type at that position, the value of the last one is returned.
        /// </summary>
        /// <returns></returns>
        private byte? GetControlDefValue(ControllerType controllerType, List<MidiControlDef> midiControlDefs)
        {
            byte? returnValue = null;
            foreach(MidiControlDef midiControlDef in midiControlDefs)
            {
                switch(controllerType)
                {
                    case ControllerType.AllSoundOff:
                    {
                        returnValue = midiControlDef.Value;
                        break;
                    }
                    case ControllerType.AllNotesOff:
                    {
                        returnValue = midiControlDef.Value;
                        break;
                    }
                    case ControllerType.AllControllersOff:
                    {
                        returnValue = midiControlDef.Value;
                        break;
                    }
                    case ControllerType.Balance:
                    {
                        returnValue = midiControlDef.Value;
                        break;
                    }
                    case ControllerType.BankSelect:
                    {
                        returnValue = midiControlDef.Value;
                        break;
                    }
                    case ControllerType.RegisteredParameterCoarse: // standard Midi for pitchwheel deviation...
                    {
                        returnValue = midiControlDef.Value;
                        break;
                    }
                    case ControllerType.Expression:
                    {
                        returnValue = midiControlDef.Value;
                        break;
                    }
                    case ControllerType.ModulationWheel:
                    {
                        returnValue = midiControlDef.Value;
                        break;
                    }
                    case ControllerType.Pan:
                    {
                        returnValue = midiControlDef.Value;
                        break;
                    }
                    case ControllerType.Volume:
                    {
                        returnValue = midiControlDef.Value;
                        break;
                    }
                }
                if(returnValue != null)
                    break;
            }
            return returnValue;
        }
        /// <summary>
        /// Returns the value of a MidiControl of the given type at the given msPosition in the controls dictionary.
        /// If there are more than one MidiControl of the given type at that position, the value of the last one is returned.
        /// </summary>
        /// <returns></returns>
        private byte? GetCommandValue(ChannelCommand channelCommand, List<MidiControl> midiControls)
        {
            MidiControl returnMidiControl = null;
            foreach(MidiControl midiControl in midiControls)
            {
                switch(channelCommand)
                {
                    case ChannelCommand.ProgramChange:
                    {
                        if(midiControl is PatchControl)
                            returnMidiControl = midiControl;
                        break;
                    }
                    case ChannelCommand.PitchWheel:
                    {
                        if(midiControl is PitchWheel)
                            returnMidiControl = midiControl;
                        break;
                    }
                }
                if(returnMidiControl != null)
                    break;
            }
            if(returnMidiControl == null)
                return null;
            else
                return (byte)returnMidiControl.ChannelMessages[0].Data1;
        }
        /// <summary>
        /// Returns the value of a MidiControl of the given type at the given msPosition in the controls dictionary.
        /// If there are more than one MidiControl of the given type at that position, the value of the last one is returned.
        /// </summary>
        /// <returns></returns>
        private byte? GetCommandDefValue(ChannelCommand channelCommand, List<MidiControlDef> midiControlDefs)
        {
            byte? returnValue = null;
            foreach(MidiControlDef midiControlDef in midiControlDefs)
            {
                switch(channelCommand)
                {
                    case ChannelCommand.ProgramChange:
                    {
                        returnValue = midiControlDef.Value;
                        break;
                    }
                    case ChannelCommand.PitchWheel:
                    {
                        returnValue = midiControlDef.Value;
                        break;
                    }
                }
                if(returnValue != null)
                    break;
            }
            return returnValue;
        }

        /// <summary>
        /// A MidiChordDef is used to define a midi:chord in the score:midiDefs section of an svg file.
        /// Its parameters can be overridden in the score and/or in a live performance.
        /// A chordID must be supplied when writing score:midiDefs, but is omitted when simply reading a palette.
        /// </summary>
        public LocalMidiChordDef(string chordID, Palette krystalPalette, int valueIndex)
            : base()
        {
            ID = chordID;

            _msDuration = krystalPalette.BasicChordSettings.Durations[valueIndex];

            BasicChordSettings bcs = krystalPalette.BasicChordSettings;
            BasicMidiChordDef rootChord = new ComposableBasicMidiChordDef(krystalPalette, valueIndex);
            _midiHeadSymbols = rootChord.Notes;
            _midiVelocitySymbol = bcs.Velocities[valueIndex];
            if(krystalPalette.OrnamentNumbers != null && krystalPalette.OrnamentNumbers.Count > 0)
                _ornamentNumberSymbol = krystalPalette.OrnamentNumbers[valueIndex];

            Debug.Assert(bcs.Durations != null && bcs.Durations.Count > 0);

            if(krystalPalette.BankIndices != null && krystalPalette.BankIndices.Count > 0)
                _bank = krystalPalette.BankIndices[valueIndex];

            if(krystalPalette.PatchIndices != null && krystalPalette.PatchIndices.Count > 0)
                _patch = krystalPalette.PatchIndices[valueIndex];

            if(krystalPalette.Volumes != null && krystalPalette.Volumes.Count > 0)
                _volume = krystalPalette.Volumes[valueIndex];

            if(krystalPalette.PitchwheelDeviations != null && krystalPalette.PitchwheelDeviations.Count > 0)
                _pitchWheelDeviation = krystalPalette.PitchwheelDeviations[valueIndex];

            if(bcs.ChordOffs != null && bcs.ChordOffs.Count > 0)
                _hasChordOff = bcs.ChordOffs[valueIndex];

            int ornamentNumber = 0;
            if(krystalPalette.OrnamentNumbers != null & krystalPalette.OrnamentNumbers.Count > 0)
            {
                ornamentNumber = krystalPalette.OrnamentNumbers[valueIndex];
            }

            this._minimumBasicMidiChordMsDuration = M.DefaultMinimumBasicMidiChordMsDuration;
            if(krystalPalette.OrnamentMinMsDurations != null && krystalPalette.OrnamentMinMsDurations.Count > 0)
                this._minimumBasicMidiChordMsDuration = krystalPalette.OrnamentMinMsDurations[valueIndex];

            if(krystalPalette.OrnamentSettings == null || ornamentNumber == 0)
            {
                BasicMidiChordDefs.Add(rootChord);
                BasicMidiChordDefs[0] = new BasicMidiChordDef(BasicMidiChordDefs[0], bcs.Durations[valueIndex]); 
            }
            else
            {
                BasicMidiChordDefs = GetOrnamentChords(krystalPalette, rootChord, valueIndex);
            }

            List<byte> pitchWheelEnvelopeMsbs = null;
            if(krystalPalette.PitchwheelEnvelopes != null && krystalPalette.PitchwheelEnvelopes.Count > 0)
                pitchWheelEnvelopeMsbs = krystalPalette.PitchwheelEnvelopes[valueIndex];

            List<byte> panEnvelopeMsbs = null;
            if(krystalPalette.PanEnvelopes != null && krystalPalette.PanEnvelopes.Count > 0)
                panEnvelopeMsbs = krystalPalette.PanEnvelopes[valueIndex];

            List<byte> modulationWheelEnvelopeMsbs = null;
            if(krystalPalette.ModulationWheelEnvelopes != null && krystalPalette.ModulationWheelEnvelopes.Count > 0)
                modulationWheelEnvelopeMsbs = krystalPalette.ModulationWheelEnvelopes[valueIndex];

            List<byte> expressionEnvelopeMsbs = null;
            if(krystalPalette.ExpressionEnvelopes != null && krystalPalette.ExpressionEnvelopes.Count > 0)
                expressionEnvelopeMsbs = krystalPalette.ExpressionEnvelopes[valueIndex];

            if(pitchWheelEnvelopeMsbs != null || panEnvelopeMsbs != null || modulationWheelEnvelopeMsbs != null || expressionEnvelopeMsbs != null)
            {
                MidiChordSliderDefs = new MidiChordSliderDefs(pitchWheelEnvelopeMsbs, panEnvelopeMsbs, modulationWheelEnvelopeMsbs, expressionEnvelopeMsbs);
            }
        }

        /// <summary>
        /// Returns a list of ornament chords.
        /// The msPositions of the chords are NOT set.
        /// The List contains the maximum number of durations which can be fit from relativeDurations into
        /// the msOuterDuration such that no duration is less than minimumOrnamentChordMsDuration.
        /// The sum of all ornamentChord durations in the list is exactly msOuterDuration.
        /// </summary>
        /// <param name="msOuterPosition"></param>
        /// <param name="msOuterDuration"></param>
        /// <param name="ornamentSettings"></param>
        /// <param name="ornamentIndex"></param>
        /// <returns></returns>
        private List<BasicMidiChordDef> GetOrnamentChords(Palette krystalPalette, BasicMidiChordDef rootChord, int index)
        {
            Debug.Assert(krystalPalette.OrnamentMinMsDurations != null);
            Debug.Assert(krystalPalette.OrnamentNumbers != null && krystalPalette.OrnamentNumbers.Count > index);

            int outerDuration = krystalPalette.BasicChordSettings.Durations[index];

            int ornamentMinMsDuration = 1; // default
            if(krystalPalette.OrnamentMinMsDurations.Count > index)
                ornamentMinMsDuration = krystalPalette.OrnamentMinMsDurations[index];

            int ornamentIndex = krystalPalette.OrnamentNumbers[index] - 1;
            Debug.Assert(ornamentIndex >= 0);

            List<BasicMidiChordDef> ornamentChords =
                GetOrnamentSettings(krystalPalette.OrnamentSettings, ornamentIndex, outerDuration, ornamentMinMsDuration);

            DoTranspositionAndVelocities(rootChord.Notes, rootChord.Velocities, ornamentChords);

            return ornamentChords;
        }

        /// <summary>
        /// Sets ornamentChords.Notes and ornamentChords.Velocities.
        /// These lists are parallel, and contain unique pitches (in ascending order) and their corresponding velocities.
        /// </summary>
        /// <param name="rootNotes"></param>
        /// <param name="ornamentChords"></param>
        private void DoTranspositionAndVelocities(List<byte> rootNotes, List<byte> rootVelocities, List<BasicMidiChordDef> ornamentChords)
        {
            Debug.Assert(rootNotes.Count == rootVelocities.Count);

            List<byte> newNotes = new List<byte>();
            List<byte> newVelocities = new List<byte>();

            foreach(BasicMidiChordDef ornamentChord in ornamentChords)
            {
                for(int i = 0; i < rootNotes.Count; ++i)
                {
                    byte rootPitch = rootNotes[i];
                    byte rootVelocity = rootVelocities[i];
                    newNotes.AddRange(NewByteList(rootPitch, ornamentChord.Notes));
                    newVelocities.AddRange(NewByteList(rootVelocity, ornamentChord.Velocities));
                }
                ornamentChord.Notes = new List<byte>(newNotes);
                ornamentChord.Velocities = new List<byte>(newVelocities);
                RemoveDuplicatePitches(ornamentChord.Notes, ornamentChord.Velocities);
                newNotes.Clear();
                newVelocities.Clear();
            }
        }

        private List<byte> NewByteList(byte root, List<byte> byteList)
        {
            List<byte> newByteList = new List<byte>();
            foreach(byte b in byteList)
            {
                int integer = (int)root + b;
                byte newByte = (byte)(integer > 127 ? 127 : integer);
                newByteList.Add(newByte);
            }
            return newByteList; // can contain duplicates
        }
        /// <summary>
        /// The input pitches can contain duplicate values which have to be removed.
        /// Returns a list of unique pitches in ascending order, and a parallel list of corresponding velocities.
        /// The returned corresponding velocity is the largest velocity corresponding to any of the corresponding 
        /// duplicate pitches in the input pitches list.
        /// </summary>
        /// <param name="notes"></param>
        /// <param name="velocities"></param>
        private void RemoveDuplicatePitches(List<byte> pitches, List<byte> velocities)
        {
            Debug.Assert(pitches.Count == velocities.Count);
            List<byte> uniquePitches = new List<byte>();
            foreach(byte pitch in pitches)
            {
                if(!uniquePitches.Contains(pitch))
                    uniquePitches.Add(pitch);
            }
            uniquePitches.Sort(); // the completed output pitch list.
            List<byte> newVelocities = new List<byte>();
            foreach(byte uniquePitch in uniquePitches)
            {
                byte maxVelocity = 0;
                for(int i = 0; i < pitches.Count; ++i)
                {
                    if((pitches[i] == uniquePitch) && (velocities[i] > maxVelocity))
                        maxVelocity = velocities[i];
                }
                newVelocities.Add(maxVelocity);
            }
            pitches.Clear();
            foreach(byte pitch in uniquePitches)
                pitches.Add(pitch);
            velocities.Clear();
            foreach(byte velocity in newVelocities)
                velocities.Add(velocity);
            Debug.Assert(pitches.Count == velocities.Count);
        }

        private List<BasicMidiChordDef> GetOrnamentSettings(OrnamentSettings ornamentSettings, int ornamentIndex,
            int msOuterDuration, int ornamentMinMsDuration)
        {
            Debug.Assert(ornamentSettings != null && ornamentIndex >= 0);

            List<int> ornamentNumbers = GetOrnamentNumbers(ornamentSettings, ornamentIndex);
            List<BasicMidiChordDef> firstPassChords = new List<BasicMidiChordDef>();

            foreach(int number in ornamentNumbers)
            {
                firstPassChords.Add(new ComposableBasicMidiChordDef(ornamentSettings, number - 1));
            }

            List<BasicMidiChordDef> ornamentChords = FitToDuration(firstPassChords, msOuterDuration, ornamentMinMsDuration);

            //List<int> relativeDurations = GetBasicMidiChordDurations(firstPassChords);
            //List<int> msDurations = GetDurations(msOuterDuration, relativeDurations, ornamentMinMsDuration);

            //List<BasicMidiChordDef> ornamentChords = new List<BasicMidiChordDef>();
            //// msDurations count can be less than firstPassChords.count
            //for(int i = 0; i < msDurations.Count; ++i)
            //{
            //    BasicMidiChordDef ornamentChord = firstPassChords[i];
            //    ornamentChord.MsDuration = msDurations[i];
            //    ornamentChords.Add(ornamentChord);
            //}
            return ornamentChords;
        }

        private List<int> GetOrnamentNumbers(OrnamentSettings ornamentSettings, int ornamentIndex)
        {
            List<List<int>> ornamentValuesList =
                ornamentSettings.OrnamentsKrystal.GetValues((uint)ornamentSettings.OrnamentLevel);

            return ornamentValuesList[ornamentIndex];
        }

        // This class is saved as an individual chordDef in SVG files,
        // so it allows ALL its fields to be set, even after construction.
        public List<byte> MidiHeadSymbols { set { _midiHeadSymbols = value; } }
        public byte MidiVelocitySymbol { set { _midiVelocitySymbol = value; } }
        public int OrnamentNumberSymbol { set { _ornamentNumberSymbol = value; } }
        public byte? Bank { set { _bank = value; } }
        public byte? Patch { set { _patch = value; } }
        public byte? Volume { set { _volume = value; } }
        public byte? PitchWheelDeviation { set { _pitchWheelDeviation = value; } }
        public bool HasChordOff { set { _hasChordOff = value; } }
        public int MinimumBasicMidiChordMsDuration { set { _minimumBasicMidiChordMsDuration = value; } }
    }
}
