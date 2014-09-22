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
    /// A MidiChordDef can either be saved and retrieved from voices in an SVG file, or
    /// retrieved from a palette (whereby the pallete makes a deep clone of its contained values).
    /// Each midiChord in an SVG file will be given an ID of the form "midiChord"+uniqueNumber, but
    /// Moritz does not actually use the ids, so they are not read into in MidiChordDefs.
    ///</summary>
    public class MidiChordDef : DurationDef, IUniqueSplittableChordDef
    {
        public MidiChordDef()
            : base(0)
        {
        }

        #region Constructor used when reading an SVG file
        public MidiChordDef(XmlReader r, int msDuration)
            : base(msDuration)
        {
            // The reader is at the beginning of a "score:midiChord" element
            Debug.Assert(r.Name == "score:midiChord" && r.IsStartElement() && r.AttributeCount > 0);
            int nAttributes = r.AttributeCount;
            for(int i = 0; i < nAttributes; i++)
            {
                r.MoveToAttribute(i);
                switch(r.Name)
                {
                    case "id": // this attribute is ignored (not used by Moritz)
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

            CheckTotalDuration();

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

        #region Constructor used when creating a list of DurationDef templates from a Palette.
        /// <summary>
        /// The palette has created new values for all the arguments, so this 
        /// constructor simply transfers those values to the new MidiChordDef.
        /// msPosition is set to 0, lyric is set to null.
        /// </summary>
        public MidiChordDef(
            int msDuration, // the total duration (this should be the sum of the durations of the basicMidiChordDefs)
            byte volume, // default is M.DefaultVolume (=100) 
            bool repeat, // default is M.DefaultChordRepeats (=false)
            byte pitchWheelDeviation, // default is M.DefaultPitchWheelDeviation (=2)
            bool hasChordOff, // default is M.DefaultHasChordOff (=true)
            List<byte> midiPitches, // the pitches that are displayed in the score
            int ornamentNumberSymbol, // is 0 when there is no ornament
            MidiChordSliderDefs midiChordSliderDefs, // can contain empty lists
            List<BasicMidiChordDef> basicMidiChordDefs)
            : base(msDuration)
        {
            foreach(byte pitch in midiPitches)
                Debug.Assert(pitch == M.MidiValue((int)pitch), "Pitch out of range.");

            _msPosition = 0;
            _msDuration = msDuration;
            _volume = volume;
            _repeat = repeat;
            _pitchWheelDeviation = pitchWheelDeviation;
            _hasChordOff = hasChordOff;
            _displayedMidiPitches = midiPitches;
            // midiVelocities are handled via the basicMidiChordDefs
            _lyric = null;
            _ornamentNumberSymbol = ornamentNumberSymbol;
            _lyric = null;

            MidiChordSliderDefs = midiChordSliderDefs;
            BasicMidiChordDefs = basicMidiChordDefs;

            CheckTotalDuration();

        }
        #endregion

        #region No sliders constructor
        /// <summary>
        /// This constructor creates a MidiChordDef at msPosition 0, lyric = null, containing a single BasicMidiChordDef and no sliders.
        /// </summary>
        public MidiChordDef(List<byte> pitches, List<byte> velocities, int msPosition, int msDuration, bool repeat, bool hasChordOff,
            List<MidiControl> midiControls)
            : base(msDuration)
        {
            foreach(byte pitch in pitches)
                Debug.Assert(pitch == M.MidiValue((int)pitch), "Pitch out of range.");

            _msPosition = msPosition;
            _volume = GetControlHiValue(ControllerType.Volume, midiControls);
            _pitchWheelDeviation = GetControlValue(ControllerType.RegisteredParameterCoarse, midiControls);
            _repeat = repeat;
            _hasChordOff = hasChordOff;
            _minimumBasicMidiChordMsDuration = 1; // not used (this is not an ornament)

            _displayedMidiPitches = pitches;
            // midiVelocity is handled via the BasicMidiChordDefs
            _ornamentNumberSymbol = 0;

            MidiChordSliderDefs = null;

            byte? bank = GetControlValue(ControllerType.BankSelect, midiControls);
            byte? patch = GetCommandValue(ChannelCommand.ProgramChange, midiControls);

            BasicMidiChordDefs.Add(new BasicMidiChordDef(msDuration, bank, patch, hasChordOff, pitches, velocities));

            CheckTotalDuration();

        }
        #endregion

        #region Another no sliders constructor
        ///// <summary>
        ///// This constructor creates a MidiChordDef at MsPosition 0, lyric = null, containing a single BasicMidiChordDef and no sliders.
        ///// </summary>
        //public MidiChordDef(List<byte> pitches, List<byte> velocities, int msDuration, bool hasChordOff,
        //    List<MidiControlDef> midiControlDefs)
        //    : base(msDuration)
        //{
        //    _msPosition = 0;
        //    _volume = GetControlDefHiValue(ControllerType.Volume, midiControlDefs);
        //    _pitchWheelDeviation = GetControlDefValue(ControllerType.RegisteredParameterCoarse, midiControlDefs);
        //    _hasChordOff = hasChordOff;
        //    _minimumBasicMidiChordMsDuration = 1; // not used (this is not an ornament)

        //    _displayedMidiPitches = pitches;
        //    // midiVelocity is handled via the basicMidiChordDefs;

        //    _ornamentNumberSymbol = 0;

        //    MidiChordSliderDefs = null;

        //    byte? bank = GetControlDefValue(ControllerType.BankSelect, midiControlDefs);
        //    byte? patch = GetCommandDefValue(ChannelCommand.ProgramChange, midiControlDefs);

        //    BasicMidiChordDefs.Add(new BasicMidiChordDef(msDuration, bank, patch, hasChordOff, pitches, velocities));

        //    CheckTotalDuration();
        //}
        #endregion

        private void CheckTotalDuration()
        {
            List<int> basicChordDurations = BasicChordDurations;
            int sumDurations = 0;
            foreach(int bcd in basicChordDurations)
                sumDurations += bcd;
            Debug.Assert(_msDuration == sumDurations);
        }

        #region IUniqueCloneDef
        #region IUniqueSplittableChordDef

        public override IUniqueDef DeepClone()
        {
            MidiChordDef rval = new MidiChordDef();

            rval.MsPosition = this.MsPosition;
            // rval.MsDuration must be set after setting BasicMidiChordDefs See below.
            rval.Bank = this.Bank;
            rval.Patch = this.Patch;
            rval.Volume = this.Volume;
            rval.Repeat = this.Repeat;
            rval.PitchWheelDeviation = this.PitchWheelDeviation;
            rval.HasChordOff = this.HasChordOff;
            rval.Lyric = this.Lyric;
            rval.MinimumBasicMidiChordMsDuration = this.MinimumBasicMidiChordMsDuration; // required when changing a midiChord's duration
            rval.MidiPitches = new List<byte>(this.MidiPitches); // the displayed noteheads
            // rval.MidiVelocity must be set after setting BasicMidiChordDefs See below.
            rval.OrnamentNumberSymbol = this.OrnamentNumberSymbol; // the displayed ornament number

            MidiChordSliderDefs m = this.MidiChordSliderDefs;
            List<byte> pitchWheelMsbs = NewListByteOrNull(m.PitchWheelMsbs);
            List<byte> panMsbs = NewListByteOrNull(m.PanMsbs);
            List<byte> modulationWheelMsbs = NewListByteOrNull(m.ModulationWheelMsbs);
            List<byte> expressionMsbs = NewListByteOrNull(m.ExpressionMsbs);
            if(pitchWheelMsbs != null || panMsbs != null || modulationWheelMsbs != null || expressionMsbs != null)
                rval.MidiChordSliderDefs = new MidiChordSliderDefs(pitchWheelMsbs, panMsbs, modulationWheelMsbs, expressionMsbs);
            else
                rval.MidiChordSliderDefs = null;

            List<BasicMidiChordDef> newBs = new List<BasicMidiChordDef>();
            foreach(BasicMidiChordDef b in this.BasicMidiChordDefs)
            {
                List<byte> pitches = new List<byte>(b.Pitches);
                List<byte> velocities = new List<byte>(b.Velocities);
                newBs.Add(new BasicMidiChordDef(b.MsDuration, b.BankIndex, b.PatchIndex, b.HasChordOff, pitches, velocities));
            }
            rval.BasicMidiChordDefs = newBs;

            rval.MsDuration = this.MsDuration;
            rval.MidiVelocity = this.MidiVelocity; // needed for displaying dynamics (must be set *after* setting BasicMidiChordDefs)

           return rval;
        }

        /// <summary>
        /// Returns null if listToClone is null, otherwise returns a clone of the listToClone.
        /// </summary>
        /// <param name="listToClone"></param>
        /// <returns></returns>
        private List<byte> NewListByteOrNull(List<byte> listToClone)
        {
            List<byte> newListByte = null;
            if(listToClone != null)
                newListByte = new List<byte>(listToClone);
            return newListByte;
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
            for(int i = 0; i < _displayedMidiPitches.Count; ++i)
            {
                int newValue = _displayedMidiPitches[i] + interval;
                _displayedMidiPitches[i] = M.MidiValue(_displayedMidiPitches[i] + interval);
            }
            _displayedMidiPitches = ReduceList(_displayedMidiPitches);

            foreach(BasicMidiChordDef bmcd in BasicMidiChordDefs)
            {
                List<byte> notes = bmcd.Pitches;
                for(int i = 0; i < notes.Count; ++i)
                {
                    notes[i] = M.MidiValue(notes[i] + interval);
                }
                bmcd.Pitches = ReduceList(notes);
            }
        }



        #region IUniqueDef
        public override string ToString()
        {
            return ("MsPosition=" + MsPosition.ToString() + " MsDuration=" + MsDuration.ToString() + " MidiChordDef");
        }

        /// <summary>
        /// Multiplies the MsDuration by the given factor.
        /// </summary>
        /// <param name="factor"></param>
        public void AdjustMsDuration(double factor)
        {
            MsDuration = (int)(_msDuration * factor);
        }

        #endregion IUniqueDef
        #endregion IUniqueChordDef
        #endregion IUniqueSplittableChordDef
        #endregion

        public void AdjustVelocities(double factor)
        {
            byte newVelocity = M.MidiValue((int)(MidiVelocity * factor));
            MidiVelocity = newVelocity;
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
        /// Returns a list which is ascending order, and in which duplicate 0 and 127 values have been removed,
        /// </summary>
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

            reducedList.Sort();

            return reducedList;
        }

        /// <summary>
        /// Note that, unlike Rests, MidiChordDefs do not have a msDuration attribute.
        /// Their msDuration is deduced from the contained BasicMidiChords.
        /// Patch indices already set in the BasicMidiChordDefs take priority over those set in the main MidiChordDef.
        /// However, if BasicMidiChordDefs[0].PatchIndex is null, and this.Patch is set, BasicMidiChordDefs[0].PatchIndex is set to Patch.
        /// The same is true for Bank settings.  
        /// The AssistantPerformer therefore only needs to look at BasicMidiChordDefs to find Bank and Patch changes.
        /// While constructing Tracks, the AssistantPerformer should monitor the current Bank and/or Patch, so that it can decide
        /// whether or not to actually construct and send bank and/or patch change messages.
        /// </summary>
        public void WriteSvg(SvgWriter w)
        {
            w.WriteStartElement("score", "midiChord", null);

            Debug.Assert(BasicMidiChordDefs != null && BasicMidiChordDefs.Count > 0);
            
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

        private static List<int> GetBasicMidiChordDurations(List<BasicMidiChordDef> ornamentChords)
        {
            List<int> returnList = new List<int>();
            foreach(BasicMidiChordDef bmc in ornamentChords)
            {
                returnList.Add(bmc.MsDuration);
            }
            return returnList;
        }

        /// <summary>
        /// This function returns the maximum number of ornament chords that can be fit into the given msDuration
        /// using the given relativeDurations and minimumOrnamentChordMsDuration.
        /// </summary>
        private static int GetNumberOfOrnamentChords(int msDuration, List<int> relativeDurations, int minimumOrnamentChordMsDuration)
        {
            bool okay = true;
            int numberOfOrnamentChords = 1;
            float factor = 1.0F;
            // try each ornament length in turn until okay is true
            for(int numChords = relativeDurations.Count; numChords > 0; --numChords)
            {
                okay = true;
                int sum = 0;
                for(int i = 0; i < numChords; ++i)
                    sum += relativeDurations[i];
                factor = ((float)msDuration / (float)sum);

                for(int i = 0; i < numChords; ++i)
                {
                    if((relativeDurations[i] * factor) < (float)minimumOrnamentChordMsDuration)
                        okay = false;
                }
                if(okay)
                {
                    numberOfOrnamentChords = numChords;
                    break;
                }
            }
            Debug.Assert(okay);
            return numberOfOrnamentChords;
        }

        /// <summary>
        /// This function returns a List whose count is numberOfOrnamentChords.
        /// It also ensures that the sum of the ints in the List is exactly equal to msDuration.
        /// This function is also used when setting the duration of a MidiDefList.
        /// </summary>
        public static List<int> GetIntDurations(int msDuration, List<int> relativeDurations, int numberOfOrnamentChords)
        {
            int sumRelative = 0;
            for(int i = 0; i < numberOfOrnamentChords; ++i)
            {
                sumRelative += relativeDurations[i];
            }
            // basicDurations are the float durations taking into account minimumOrnamentChordMsDuration
            float factor = ((float)msDuration / (float)sumRelative);
            float fPos = 0;
            List<int> intPositions = new List<int>();
            for(int i = 0; i < numberOfOrnamentChords; ++i)
            {
                intPositions.Add((int)(Math.Floor(fPos)));
                fPos += (relativeDurations[i] * factor);
            }
            intPositions.Add((int)Math.Floor(fPos));

            List<int> intDurations = new List<int>();
            for(int i = 0; i < numberOfOrnamentChords; ++i)
            {
                int intDuration = (int)(intPositions[i + 1] - intPositions[i]);
                intDurations.Add(intDuration);
            }

            int intSum = 0;
            foreach(int i in intDurations)
                intSum += i;
            Debug.Assert(intSum <= msDuration);
            if(intSum < msDuration)
            {
                int lastDuration = intDurations[intDurations.Count - 1];
                lastDuration += (msDuration - intSum);
                intDurations.RemoveAt(intDurations.Count - 1);
                intDurations.Add(lastDuration);
            }
            return intDurations;
        }

        /// <summary>
        /// Returns a list of (millisecond) durations whose sum is msDuration.
        /// The List contains the maximum number of durations which can be fit from relativeDurations into the msDuration
        /// such that no duration is less than minimumOrnamentChordMsDuration.
        /// </summary>
        /// <param name="msDuration"></param>
        /// <param name="relativeDurations"></param>
        /// <param name="ornamentMinMsDuration"></param>
        /// <returns></returns>
        private static List<int> GetDurations(int msDuration, List<int> relativeDurations, int ornamentMinMsDuration)
        {
            int numberOfOrnamentChords = GetNumberOfOrnamentChords(msDuration, relativeDurations, ornamentMinMsDuration);

            List<int> intDurations = GetIntDurations(msDuration, relativeDurations, numberOfOrnamentChords);
            return intDurations;
        }

        /// <summary>
        /// Returns a new list of basicMidiChordDefs having the msOuterDuration, shortening the list if necessary.
        /// </summary>
        /// <param name="basicMidiChordDefs"></param>
        /// <param name="msOuterDuration"></param>
        /// <param name="minimumMsDuration"></param>
        /// <returns></returns>
        public static List<BasicMidiChordDef> FitToDuration(List<BasicMidiChordDef> bmcd, int msOuterDuration, int minimumMsDuration)
        {
            List<int> relativeDurations = GetBasicMidiChordDurations(bmcd);
            List<int> msDurations = GetDurations(msOuterDuration, relativeDurations, minimumMsDuration);

            // msDurations.Count can be less than bmcd.Count

            List<BasicMidiChordDef> rList = new List<BasicMidiChordDef>();
            BasicMidiChordDef b;
            for(int i = 0; i < msDurations.Count; ++i)
            {
                b = bmcd[i];
                rList.Add(new BasicMidiChordDef(msDurations[i], b.BankIndex, b.PatchIndex, b.HasChordOff, b.Pitches, b.Velocities));
            }

            return rList;
        }
        #region properties

        public List<int> BasicChordDurations
        {
            get
            {
                List<int> rList = new List<int>();
                foreach(BasicMidiChordDef bmcd in BasicMidiChordDefs)
                {
                    rList.Add(bmcd.MsDuration);
                }
                return rList;
            }
        }

        /****************************************************************************/

        public int MsPosition { get { return _msPosition; } set { _msPosition = value; } }
        private int _msPosition = 0;

        public override int MsDuration
        {
            get
            {
                return _msDuration;
            }
            set
            {
                Debug.Assert(BasicMidiChordDefs != null && BasicMidiChordDefs.Count > 0);
                _msDuration = value;
                int sumDurations = 0;
                foreach(int bcd in BasicChordDurations)
                    sumDurations += bcd;
                if(_msDuration != sumDurations)
                {
                    BasicMidiChordDefs = FitToDuration(BasicMidiChordDefs, _msDuration, _minimumBasicMidiChordMsDuration);
                }
            }
        }

        public byte? Bank { get { return _bank; } set { _bank = value; } }
        private byte? _bank = null;
        public byte? Patch { get { return _patch; } set { _patch = value; } }
        private byte? _patch = null;
        public byte? Volume { get { return _volume; } set { _volume = value; } }
        private byte? _volume = null;
        // If Repeat is true, the MidiChord will repeat in assisted performances
        // if the performed duration is longer than the duration of the basicChords,
        // and the performer continues to hold the key down.
        // The duration in the score is always the sum of the durations of the basicChords.
        public bool Repeat { get { return _repeat; } set { _repeat = value; } }
        private bool _repeat = false;
        public byte? PitchWheelDeviation
        {
            get
            {
                return _pitchWheelDeviation;
            }
            set
            {
                if(value == null)
                    _pitchWheelDeviation = null;
                else
                {
                    int val = (int)value;
                    _pitchWheelDeviation = M.MidiValue(val);
                }
            }
        }
        public byte? _pitchWheelDeviation = null;
        public bool HasChordOff { get { return _hasChordOff; } set { _hasChordOff = value; } }
        private bool _hasChordOff = true;
        public string Lyric { get { return _lyric; } set { _lyric = value; } }
        private string _lyric = null; 
        public int MinimumBasicMidiChordMsDuration { get { return _minimumBasicMidiChordMsDuration; } set { _minimumBasicMidiChordMsDuration = value; } }
        private int _minimumBasicMidiChordMsDuration = 1;
        /// <summary>
        /// This MidiPitches field is used when displaying the chord's noteheads.
        /// The performed pitches are set in the BasicMidiChordDefs.
        /// </summary>
        public List<byte> MidiPitches
        { 
            get { return _displayedMidiPitches; } 
            set 
            {
                foreach(byte pitch in value)
                    Debug.Assert(pitch == M.MidiValue((int)pitch));
                _displayedMidiPitches = value; 
            } 
        }
        private List<byte> _displayedMidiPitches = null;
        /// <summary>
        /// The MidiVelocity field is used when displaying dynamics in the score.
        /// Gets basicMidichordDefs[0].Velocities[0].
        /// Sets BasicMidiChordDefs[0].Velocities[0] to value, and the other velocities so that the original proportions are kept.
        /// ( see also: AdjustVelocities(double factor) )
        /// </summary>
        public byte MidiVelocity
        {
            get { return BasicMidiChordDefs[0].Velocities[0]; }
            set
            {
                Debug.Assert(BasicMidiChordDefs != null && BasicMidiChordDefs.Count > 0
                    && BasicMidiChordDefs[0].Velocities != null && BasicMidiChordDefs[0].Velocities.Count > 0);

                if(value != BasicMidiChordDefs[0].Velocities[0])
                {
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
        }

        public int OrnamentNumberSymbol { get { return _ornamentNumberSymbol; } set { _ornamentNumberSymbol = value; } }
        private int _ornamentNumberSymbol = 0;

        public MidiChordSliderDefs MidiChordSliderDefs = null;
        public List<BasicMidiChordDef> BasicMidiChordDefs = new List<BasicMidiChordDef>();
        /****************************************************************************/

        public PerformanceControlDef PerformanceControlDef = null;

        #endregion properties
    }
}
