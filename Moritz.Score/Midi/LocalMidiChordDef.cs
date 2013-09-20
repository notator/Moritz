using System.Collections.Generic;
using System.Diagnostics;

using Multimedia.Midi;

using Moritz.Globals;

namespace Moritz.Score.Midi
{
    ///<summary>
    /// A LocalMidiChordDef is a MidiChordDef which is saved locally in an SVG file.
    /// Related classes:
    /// 1. A PaletteMidiChordDef is a MidiChordDef which is saved in or retreived from a palette.
    /// PaletteMidiChordDefs can be 'used' in SVG files, but are usually converted to LocalMidiChordDef.
    /// 2. A LocalizedMidiChordDef is a LocalMidiChordDef with additional MsPositon and msDuration attributes.
    //</summary>
    public class LocalMidiChordDef : MidiChordDef
    {
        /// <summary>
        /// A deep clone of the argument. This class is saved as an individual chordDef in SVG files,
        /// so it allows ALL its attributes to be set, even after construction.
        /// </summary>
        /// <param name="midiChordDef"></param>
        public LocalMidiChordDef(MidiChordDef mcd)
        {
            ID = "localChord" + UniqueChordID.ToString();

            _bank = mcd.Bank;
            _patch = mcd.Patch;
            _volume = mcd.Volume;
            _lyric = mcd.Lyric; // this is currently not set in palettes (19.09.2013)
            _pitchWheelDeviation = mcd.PitchWheelDeviation;
            _hasChordOff = mcd.HasChordOff;
            _minimumBasicMidiChordMsDuration = mcd.MinimumBasicMidiChordMsDuration;

            _msDuration = mcd.MsDuration;
            _volume = mcd.Volume;

            _midiVelocity = mcd.MidiVelocity;
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
        /// This constructor creates a LocalMidiChordDef containing a single BasicMidiChordDef and no sliders.
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
            _midiVelocity = velocities[0];
            _ornamentNumberSymbol = 0;

            MidiChordSliderDefs = null;

            byte? bank = GetControlValue(ControllerType.BankSelect, midiControls);
            byte? patch = GetCommandValue(ChannelCommand.ProgramChange, midiControls);

            BasicMidiChordDefs.Add(new BasicMidiChordDef(msDuration, bank, patch, hasChordOff, pitches, velocities));
        }


        /// <summary>
        /// This constructor creates a LocalMidiChordDef containing a single BasicMidiChordDef and no sliders.
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
            _midiVelocity = velocities[0];
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
        /// Transpose (both notation and sound) by the number of semitones given in the argument.
        /// Negative interval values transpose down.
        /// It is not an error if Midi values would exceed the range 0..127.
        /// In this case, they are silently coerced to 0 or 127 respectively.
        /// </summary>
        public void Transpose(int interval)
        {
            for(int i = 0; i < _midiHeadSymbols.Count; ++i)
            {
                _midiHeadSymbols[i] = GetTrimmedValue(_midiHeadSymbols[i] + interval);
            }
            foreach(BasicMidiChordDef bmcd in BasicMidiChordDefs)
            {
                List<byte> notes = bmcd.Notes;
                for(int i = 0; i < notes.Count; ++i)
                {
                    notes[i] = GetTrimmedValue(notes[i] + interval);
                }
                bmcd.Notes = ReduceList(notes);
            }
        }

        private byte GetTrimmedValue(int value)
        {
            value = (value > 127) ? 127 : value;
            value = (value < 0) ? 0 : value;
            return (byte)value;
        }

        /// <summary>
        /// Returns a list in which duplicate 0 and 127 values have been removed.
        /// </summary>
        /// <param name="list"></param>
        /// <returns></returns>
        private List<byte> ReduceList(List<byte> list)
        {
            List<byte> reducedList = new List<byte>();
            bool minvalFound = false;
            bool maxvalFound = false;
            for(int i = 0; i < list.Count; ++i)
            {
                if(list[i] == 0)
                {
                    if(!minvalFound)
                    {
                        reducedList.Add(0);
                        minvalFound = true;
                    }
                }
                else if(list[i] == 127)
                {
                    if(!maxvalFound)
                    {
                        reducedList.Add(127);
                        maxvalFound = true;
                    }
                }
                else
                {
                    reducedList.Add(list[i]);
                }
            }
            if(list.Count == reducedList.Count)
                return list;
            else
                return reducedList;
        }

        // This class is saved as an individual chordDef in SVG files,
        // so it allows ALL its fields to be set, even after construction.
        public new List<byte> MidiHeadSymbols { set { _midiHeadSymbols = value; } }
        public new byte MidiVelocity { set { _midiVelocity = value; } }
        public new int OrnamentNumberSymbol { set { _ornamentNumberSymbol = value; } }
        public new byte? Bank { set { _bank = value; } }
        public new byte? Patch { set { _patch = value; } }
        public new byte? Volume { set { _volume = value; } }
        public new byte? PitchWheelDeviation { set { _pitchWheelDeviation = value; } }
        public new bool HasChordOff { set { _hasChordOff = value; } }
        public new int MinimumBasicMidiChordMsDuration { set { _minimumBasicMidiChordMsDuration = value; } }
    }
}
