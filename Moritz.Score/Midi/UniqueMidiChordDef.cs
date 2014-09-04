using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Xml;

using Multimedia.Midi;
using Moritz.Globals;

using Moritz.Score.Notation;

namespace Moritz.Score.Midi
{
    ///<summary>
    /// A UniqueMidiChordDef is a MidiChordDef which is saved and retrieved from voices in an SVG file.
    /// Related classes:
    /// 1. A PaletteMidiChordDef is a MidiChordDef retrieved from a palette.
    /// PaletteMidiChordDefs are immutable, and have a null MsPosition attribute.
    /// 2. A UniqueMidiChordDef is a MidiChordDef with both MsPositon and MsDuration attributes.
    //</summary>
    public class UniqueMidiChordDef : MidiChordDef, IUniqueSplittableChordDef, IUniqueCloneDef
    {
        public UniqueMidiChordDef()
            : base()
        {
            ID = "localChord" + UniqueChordID.ToString();
        }

        #region Constructor used when reading an SVG file
        public UniqueMidiChordDef(XmlReader r, string localID, int msDuration)
            : base()
        {
            // The reader is at the beginning of a "score:midiChord" element having an ID attribute
            Debug.Assert(r.Name == "score:midiChord" && r.IsStartElement() && r.AttributeCount > 0);
            int nAttributes = r.AttributeCount;
            for(int i = 0; i < nAttributes; i++)
            {
                r.MoveToAttribute(i);
                switch(r.Name)
                {
                    case "id":
                        ID = localID; // this is the local id in the score
                        break;
                    case "repeat":
                        // repeat is false if this attribute is not present
                        byte rmVal = byte.Parse(r.Value);
                        if(rmVal == 0)
                            _repeat = false;
                        else
                            _repeat = true;
                        break;
                    case "hasChordOff":
                        // hasChordOff is true if this attribute is not present
                        byte hcoVal = byte.Parse(r.Value);
                        if(hcoVal == 0)
                            _hasChordOff = false;
                        else
                            _hasChordOff = true;
                        break;
                    case "bank":
                        _bank = byte.Parse(r.Value);
                        break;
                    case "patch":
                        _patch = byte.Parse(r.Value);
                        break;
                    case "volume":
                        _volume = byte.Parse(r.Value);
                        break;
                    case "pitchWheelDeviation":
                        _pitchWheelDeviation = byte.Parse(r.Value);
                        break;
                    case "minBasicChordMsDuration":
                        this._minimumBasicMidiChordMsDuration = int.Parse(r.Value);
                        break;
                }
            }

            M.ReadToXmlElementTag(r, "score:basicChords", "score:sliders");
            while(r.Name == "score:basicChords" || r.Name == "score:sliders")
            {
                if(r.IsStartElement())
                {
                    switch(r.Name)
                    {
                        case "score:basicChords":
                            GetBasicChordDefs(r);
                            break;
                        case "score:sliders":
                            MidiChordSliderDefs = new MidiChordSliderDefs(r);
                            break;
                    }

                    M.ReadToXmlElementTag(r, "score:basicChords", "score:sliders", "score:midiChord");
                }
            }

            #region check total duration
            List<int> basicChordDurations = BasicChordDurations;
            int sumDurations = 0;
            foreach(int bcd in basicChordDurations)
                sumDurations += bcd;
            Debug.Assert(_msDuration == sumDurations);
            #endregion

            //bool isStartElement = r.IsStartElement();
            //Debug.Assert(r.Name == "score.inputChord" && !(isStartElement));
        }

        private void GetBasicChordDefs(XmlReader r)
        {
            // The reader is at the beginning of a "basicChords" element
            Debug.Assert(r.Name == "score:basicChords" && r.IsStartElement());
            M.ReadToXmlElementTag(r, "score:basicChord");
            while(r.Name == "score:basicChord")
            {
                if(r.IsStartElement())
                {
                    BasicMidiChordDefs.Add(new BasicMidiChordDef(r));
                }
                M.ReadToXmlElementTag(r, "score:basicChord", "score:basicChords");
            }
        }
        #endregion

        /// <summary>
        /// A deep clone of the argument. This class is saved as an individual chordDef in SVG files,
        /// so it allows ALL its attributes to be set, even after construction.
        /// The argument may not be null.
        /// </summary>
        /// <param name="midiChordDef"></param>
        public UniqueMidiChordDef(MidiChordDef mcd)
            : this()
        {
            Debug.Assert(mcd != null);

            _bank = mcd.Bank;
            _patch = mcd.Patch;
            _volume = mcd.Volume;
            _lyric = mcd.Lyric; // this is currently not set in palettes (19.09.2013)
            _pitchWheelDeviation = mcd.PitchWheelDeviation;
            _repeat = mcd.Repeat;
            _hasChordOff = mcd.HasChordOff;
            _minimumBasicMidiChordMsDuration = mcd.MinimumBasicMidiChordMsDuration;

            _msDuration = mcd.MsDuration;
            _volume = mcd.Volume;

            _midiVelocity = mcd.MidiVelocity;
            _ornamentNumberSymbol = mcd.OrnamentNumberSymbol;
            _midiPitches = new List<byte>(mcd.MidiPitches);

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
        /// Constructor used when making a DeepClone, and when retrieving a UniqueMidiChordDef from a palette.
        /// This constructor makes a deep clone of the values in its arguments.
        /// When called by a palette, msPosition is set to 0, and lyric is set to null.
        /// </summary>
        public UniqueMidiChordDef(
            int msPosition,
            int msDuration,
            byte? bank,
            byte? patch,
            byte? volume,
            bool repeat,
            byte? pitchWheelDeviation,
            bool hasChordOff,
            string lyric,
            int minimumBasicMidiChordMsDuration,
            List<byte> midiPitches,
            byte midiVelocity,
            int ornamentNumberSymbol,
            MidiChordSliderDefs midiChordSliderDefs,
            List<BasicMidiChordDef> basicMidiChordDefs)
            : base()
        {
            _msPosition = msPosition;
            _msDuration  = msDuration;
            _bank = bank;
            _patch = patch;
            _volume = volume;
            _repeat = repeat;
            _pitchWheelDeviation = pitchWheelDeviation;
            _hasChordOff = hasChordOff;
            _lyric = lyric;
            _minimumBasicMidiChordMsDuration = minimumBasicMidiChordMsDuration;
            _midiPitches = new List<byte>(midiPitches);
            _midiVelocity = midiVelocity;
            _ornamentNumberSymbol = ornamentNumberSymbol;
            _lyric = null;

            MidiChordSliderDefs mcsd = midiChordSliderDefs;
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

            BasicMidiChordDefs = new List<BasicMidiChordDef>();
            foreach(BasicMidiChordDef bmcd in basicMidiChordDefs)
            {
                List<byte> pitches = new List<byte>(bmcd.Notes);
                List<byte> velocities = new List<byte>(bmcd.Velocities);
                BasicMidiChordDefs.Add(new BasicMidiChordDef(bmcd.MsDuration, bmcd.BankIndex, bmcd.PatchIndex, bmcd.HasChordOff, pitches, velocities));
            }
        }
        /// <summary>
        /// This constructor creates a UniqueMidiChordDef containing a single BasicMidiChordDef and no sliders.
        /// </summary>
        public UniqueMidiChordDef(List<byte> pitches, List<byte> velocities, int msPosition, int msDuration, bool repeat, bool hasChordOff,
            List<MidiControl> midiControls)
            : this()
        {
            _msPosition = msPosition;
            _msDuration = msDuration;
            _volume = GetControlHiValue(ControllerType.Volume, midiControls);
            _pitchWheelDeviation = GetControlValue(ControllerType.RegisteredParameterCoarse, midiControls);
            _repeat = repeat;
            _hasChordOff = hasChordOff;
            _minimumBasicMidiChordMsDuration = 1; // not used (this is not an ornament)

            _midiPitches = pitches;
            _midiVelocity = velocities[0];
            _ornamentNumberSymbol = 0;

            MidiChordSliderDefs = null;

            byte? bank = GetControlValue(ControllerType.BankSelect, midiControls);
            byte? patch = GetCommandValue(ChannelCommand.ProgramChange, midiControls);

            BasicMidiChordDefs.Add(new BasicMidiChordDef(msDuration, bank, patch, hasChordOff, pitches, velocities));
        }

        /// <summary>
        /// This constructor creates a UniqueMidiChordDef containing a single BasicMidiChordDef and no sliders.
        /// </summary>
        public UniqueMidiChordDef(List<byte> pitches, List<byte> velocities, int msDuration, bool hasChordOff,
            List<MidiControlDef> midiControlDefs)
            : this()
        {
            _msDuration = msDuration;
            _volume = GetControlDefHiValue(ControllerType.Volume, midiControlDefs);
            _pitchWheelDeviation = GetControlDefValue(ControllerType.RegisteredParameterCoarse, midiControlDefs);
            _hasChordOff = hasChordOff;
            _minimumBasicMidiChordMsDuration = 1; // not used (this is not an ornament)

            _midiPitches = pitches;
            _midiVelocity = velocities[0];
            _ornamentNumberSymbol = 0;

            MidiChordSliderDefs = null;

            byte? bank = GetControlDefValue(ControllerType.BankSelect, midiControlDefs);
            byte? patch = GetCommandDefValue(ChannelCommand.ProgramChange, midiControlDefs);

            BasicMidiChordDefs.Add(new BasicMidiChordDef(msDuration, bank, patch, hasChordOff, pitches, velocities));

        }

        #region IUniqueCloneDef
        #region IUniqueSplittableChordDef

        public override IUniqueDef DeepClone()
        {
            return new UniqueMidiChordDef(this);
        }

        public int? MsDurationToNextBarline { get { return _msDurationToNextBarline; } set { _msDurationToNextBarline = value; } }
        private int? _msDurationToNextBarline = null;

        #region IUniqueChordDef
        /// <summary>
        /// Transpose (both notation and sound) by the number of semitones given in the argument.
        /// Negative interval values transpose down.
        /// It is not an error if Midi values would exceed the range 0..127.
        /// In this case, they are silently coerced to 0 or 127 respectively.
        /// </summary>
        /// <summary>
        /// Transpose (both notation and sound) by the number of semitones given in the argument.
        /// Negative interval values transpose down.
        /// It is not an error if Midi values would exceed the range 0..127.
        /// In this case, they are silently coerced to 0 or 127 respectively.
        /// </summary>
        public void Transpose(int interval)
        {
            for(int i = 0; i < _midiPitches.Count; ++i)
            {
                _midiPitches[i] = M.MidiValue(_midiPitches[i] + interval);
            }
            foreach(BasicMidiChordDef bmcd in BasicMidiChordDefs)
            {
                List<byte> notes = bmcd.Notes;
                for(int i = 0; i < notes.Count; ++i)
                {
                    notes[i] = M.MidiValue(notes[i] + interval);
                }
                bmcd.Notes = ReduceList(notes);
            }
        }

        public new List<byte> MidiPitches { get { return _midiPitches; } set { _midiPitches = value; } }
        private new List<byte> _midiPitches = null;

        #region IUniqueDef
        public override string ToString()
        {
            return ("MsPosition=" + MsPosition.ToString() + " MsDuration=" + MsDuration.ToString() + " UniqueMidiChordDef");
        }

        /// <summary>
        /// Multiplies the MsDuration by the given factor.
        /// </summary>
        /// <param name="factor"></param>
        public void AdjustDuration(double factor)
        {
            MsDuration = (int)(_msDuration * factor);
        }

        public new int MsDuration
        {
            get
            {
                return _msDuration;
            }
            set
            {
                _msDuration = value;
                BasicMidiChordDefs = FitToDuration(BasicMidiChordDefs, MsDuration, _minimumBasicMidiChordMsDuration);
            }
        }

        public int MsPosition { get { return _msPosition; } set { _msPosition = value; } }
        private int _msPosition = 0;

        #endregion IUniqueDef
        #endregion IUniqueChordDef
        #endregion IUniqueSplittableChordDef
        #endregion

        public void AdjustVelocities(double factor)
        {
            foreach(BasicMidiChordDef bmcd in BasicMidiChordDefs)
            {
                for(int i = 0; i < bmcd.Velocities.Count; ++i)
                {
                    bmcd.Velocities[i] = M.MidiValue((int)(bmcd.Velocities[i] * factor));
                }
            }
            this._midiVelocity = BasicMidiChordDefs[0].Velocities[0];
        }

        public void AdjustExpression(double factor)
        {
            List<byte> exprs = this.MidiChordSliderDefs.ExpressionMsbs;
            for(int i = 0; i < exprs.Count; ++i)
            {
                exprs[i] = M.MidiValue((int)(exprs[i] * factor));
            }
        }

        public List<byte> PanMsbs
        {
            get
            {
                List<byte> rval;
                if(this.MidiChordSliderDefs == null || this.MidiChordSliderDefs.PanMsbs == null)
                {
                    rval = new List<byte>();
                }
                else
                {
                    rval = this.MidiChordSliderDefs.PanMsbs;
                }
                return rval;
            }
            set
            {
                if(this.MidiChordSliderDefs == null)
                {
                    this.MidiChordSliderDefs = new MidiChordSliderDefs(new List<byte>(), new List<byte>(), new List<byte>(), new List<byte>());
                }
                if(this.MidiChordSliderDefs.PanMsbs == null)
                {
                    this.MidiChordSliderDefs.PanMsbs = new List<byte>();
                }
                List<byte> pans = this.MidiChordSliderDefs.PanMsbs;
                pans.Clear();
                for(int i = 0; i < value.Count; ++i)
                {
                    pans.Add(M.MidiValue((int)(value[i])));
                }
            }
        }

        public void AdjustModulationWheel(double factor)
        {
            List<byte> modWheels = this.MidiChordSliderDefs.ModulationWheelMsbs;
            for(int i = 0; i < modWheels.Count; ++i)
            {
                modWheels[i] = M.MidiValue((int)(modWheels[i] * factor));
            }
        }

        public void AdjustPitchWheel(double factor)
        {
            List<byte> pitchWheels = this.MidiChordSliderDefs.PitchWheelMsbs;
            for(int i = 0; i < pitchWheels.Count; ++i)
            {
                pitchWheels[i] = M.MidiValue((int)(pitchWheels[i] * factor));
            }
        }

        /// <summary>
        /// Gets basicMidichordDefs[0].Velocities[0].
        /// Sets BasicMidiChordDefs[0].Velocities[0] to value, and the other velocities so that the original proportions are kept.
        /// ( see also: AdjustVelocities(double factor) )
        /// </summary>
        public new byte MidiVelocity
        {
            get { return _midiVelocity; }
            set
            {
                _midiVelocity = value;
                double factor = (((double)value) / ((double)BasicMidiChordDefs[0].Velocities[0]));
                foreach(BasicMidiChordDef bmcd in BasicMidiChordDefs)
                {
                    for(int i = 0; i < bmcd.Velocities.Count; ++i)
                    {
                        bmcd.Velocities[i] = M.MidiValue((int)((double)bmcd.Velocities[i] * factor));
                    }
                }
            }
        }

        public new int? PitchWheelDeviation
        {
            get
            {
                return (int?)_pitchWheelDeviation;
            }
            set
            {
                Debug.Assert(value != null);
                int val = (int)value;
                val = (val < 127) ? val : 127;
                val = (val > 0) ? val : 0;

                _pitchWheelDeviation = (byte?)val;
            }
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

        /// <summary>
        /// Note that, unlike MidiRestDefs, MidiChordDefs do not have a msDuration attribute.
        /// Their msDuration is deduced from the contained BasicMidiChords.
        /// Patch indices already set in the BasicMidiChordDefs take priority over those set in the main UniqueMidiChordDef.
        /// However, if BasicMidiChordDefs[0].PatchIndex is null, and this.Patch is set, BasicMidiChordDefs[0].PatchIndex is set to Patch.
        /// The same is true for Bank settings.  
        /// The AssistantPerformer therefore only needs to look at BasicMidiChordDefs to find Bank and Patch changes.
        /// While constructing Tracks, the AssistantPerformer should monitor the current Bank and/or Patch, so that it can decide
        /// whether or not to actually construct and send bank and/or patch change messages.
        /// </summary>
        public void WriteSvg(SvgWriter w, string idNumber)
        {
            w.WriteStartElement("score", "midiChord", null);

            Debug.Assert(BasicMidiChordDefs != null && BasicMidiChordDefs.Count > 0);

            if(!String.IsNullOrEmpty(idNumber))
                w.WriteAttributeString("id", "midiChord" + idNumber);

            if(BasicMidiChordDefs[0].BankIndex == null && Bank != null)
            {
                BasicMidiChordDefs[0].BankIndex = Bank;
            }
            if(BasicMidiChordDefs[0].PatchIndex == null && Patch != null)
            {
                BasicMidiChordDefs[0].PatchIndex = Patch;
            }
            if(Volume != null && Volume != M.DefaultVolume)
                w.WriteAttributeString("volume", Volume.ToString());
            if(Repeat == true)
                w.WriteAttributeString("repeat", "1");
            if(HasChordOff == false)
                w.WriteAttributeString("hasChordOff", "0");
            if(PitchWheelDeviation != null && PitchWheelDeviation != M.DefaultPitchWheelDeviation)
                w.WriteAttributeString("pitchWheelDeviation", PitchWheelDeviation.ToString());
            if(MinimumBasicMidiChordMsDuration != M.DefaultMinimumBasicMidiChordMsDuration)
                w.WriteAttributeString("minBasicChordMsDuration", MinimumBasicMidiChordMsDuration.ToString());

            w.WriteStartElement("score", "basicChords", null);
            foreach(BasicMidiChordDef basicMidiChord in BasicMidiChordDefs) // containing basic <midiChord> elements
                basicMidiChord.WriteSVG(w);
            w.WriteEndElement();

            if(MidiChordSliderDefs != null)
                MidiChordSliderDefs.WriteSVG(w); // writes score:sliders element

            w.WriteEndElement(); // score:midiChord
        }

        public new int OrnamentNumberSymbol { get { return _ornamentNumberSymbol; } set { _ornamentNumberSymbol = value; } }
        public new byte? Bank { get { return _bank; } set { _bank = value; } }
        public new byte? Patch { get { return _patch; } set { _patch = value; } }
        public new byte? Volume { get { return _volume; } set { _volume = value; } }

        public new bool Repeat { get { return _repeat; } set { _repeat = value; } }
        public new bool HasChordOff { get { return _hasChordOff; } set { _hasChordOff = value; } }

        public new int MinimumBasicMidiChordMsDuration { get { return _minimumBasicMidiChordMsDuration; } set { _minimumBasicMidiChordMsDuration = value; } }
    }
}
