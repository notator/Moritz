using System;
using System.Collections.Generic;
using System.Xml;
using System.Diagnostics;

using Moritz.Globals;

namespace Moritz.Score.Midi
{
    public class MidiChordDef : MidiDurationDef
    {
        /// <summary>
        /// Constructor used while constructing derived types.
        /// The derived types are responsible for initializing all the fields correctly.
        /// </summary>
        public MidiChordDef()
            : base()
        {
        }

        #region Constructors used when reading an SVG file
        /// <summary>
        /// Contains values retrieved from an SVG file score:midiChord element
        /// Note that MidiChordDefs do not have msPosition and msDuration attributes.
        /// These attributes are deduced from the contained BasicMidiChords.
        /// </summary>
        public MidiChordDef(XmlReader r, string localID, Dictionary<string, MidiDurationDef> scoreMidiDurationDefs, int msDuration)
            : base(msDuration)
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
                        if(localID != null)
                            ID = localID; // this is the local id in the score
                        else
                            ID = r.Value; // this is the id in the palleteDefs
                        break;
                    case "hasChordOff":
                        // hasChordOff is true if this attribute is not present
                        byte val = byte.Parse(r.Value);
                        if(val == 0)
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

            M.ReadToXmlElementTag(r, "score:basicChords", "score:sliders", "use");
            while(r.Name == "use" || r.Name == "score:basicChords" || r.Name == "score:sliders")
            {
                if(r.IsStartElement())
                {
                    switch(r.Name)
                    {
                        case "use":
                            r.MoveToAttribute(0);
                            Debug.Assert(r.Name == "xlink:href");
                            string midiChordDefID = r.Value.Remove(0, 1);
                            SetFromMidiChordDefs(scoreMidiDurationDefs, midiChordDefID, msDuration);
                            //midiChordDef = new MidiChordDef(this._score.MidiChordDefs[midiChordDefID], msDuration);
                            break;
                        case "score:basicChords":
                            GetBasicChordDefs(r);
                            break;
                        case "score:sliders":
                            MidiChordSliderDefs = new MidiChordSliderDefs(r);
                            break;
                    }

                    M.ReadToXmlElementTag(r, "use", "score:basicChords", "score:sliders", "score:midiChord");
                }
            }
            //bool isStartElement = r.IsStartElement();
            //Debug.Assert(r.Name == "score.midiChord" && !(isStartElement));
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

        private void SetFromMidiChordDefs(Dictionary<string, MidiDurationDef> scoreMidiDurationDefs, string midiChordDefID, int msDuration)
        {
            MidiChordDef clone = new MidiChordDef(scoreMidiDurationDefs[midiChordDefID], msDuration);

            this.MsDuration = msDuration;
            this._bank = clone.Bank;
            this._patch = clone.Patch;
            this._volume = clone.Volume;
            this._ornamentNumberSymbol = clone.OrnamentNumberSymbol;
            this._pitchWheelDeviation = clone.PitchWheelDeviation;
            this._midiHeadSymbols = clone.MidiHeadSymbols;
            this._midiVelocitySymbol = clone.MidiVelocitySymbol;
            this._minimumBasicMidiChordMsDuration = clone.MinimumBasicMidiChordMsDuration;

            BasicMidiChordDefs = clone.BasicMidiChordDefs;
            MidiChordSliderDefs = clone.MidiChordSliderDefs;
        }

        /// <summary>
        /// Creates a clone of the first argument, then fits the BasicMidiChordDefs into the given msDuration.
        /// Used when the first argument comes from a #use definition (in the midiDefs section of an SVG-MIDI file).
        /// Also used in MidiScore.GetMidiChannelMoments(SvgScore svgScore).
        /// </summary>
        /// <param name="midiChordDef"></param>
        public MidiChordDef(MidiDurationDef midiDurationDef, int msDuration)
        {
            MidiChordDef midiChordDef = midiDurationDef as MidiChordDef;
            Debug.Assert(midiChordDef != null);

            #region Clone the first argument
            BasicMidiChordDefs = new List<BasicMidiChordDef>(midiChordDef.BasicMidiChordDefs);
            this._hasChordOff = midiChordDef.HasChordOff;
            this.ID = "localChord" + UniqueChordID.ToString();

            if(midiChordDef.MidiChordSliderDefs != null)
            {
                List<byte> pitchWheelMsbs = null;
                if(midiChordDef.MidiChordSliderDefs.PitchWheelMsbs != null)
                    pitchWheelMsbs = new List<byte>(midiChordDef.MidiChordSliderDefs.PitchWheelMsbs);
                List<byte> panMsbs = null;
                if(midiChordDef.MidiChordSliderDefs.PanMsbs != null)
                    panMsbs = new List<byte>(midiChordDef.MidiChordSliderDefs.PanMsbs);
                List<byte> modulationWheelMsbs = null;
                if(midiChordDef.MidiChordSliderDefs.ModulationWheelMsbs != null)
                    modulationWheelMsbs = new List<byte>(midiChordDef.MidiChordSliderDefs.ModulationWheelMsbs);
                List<byte> expressionMsbs = null;
                if(midiChordDef.MidiChordSliderDefs.ExpressionMsbs != null)
                    expressionMsbs = new List<byte>(midiChordDef.MidiChordSliderDefs.ExpressionMsbs);
                this.MidiChordSliderDefs = new MidiChordSliderDefs(pitchWheelMsbs, panMsbs, modulationWheelMsbs, expressionMsbs);
            }

            this._midiHeadSymbols = null;
            if(midiChordDef.MidiHeadSymbols != null)
                _midiHeadSymbols = new List<byte>(midiChordDef.MidiHeadSymbols);
            this._midiVelocitySymbol = midiChordDef.MidiVelocitySymbol;
            this._minimumBasicMidiChordMsDuration = midiChordDef.MinimumBasicMidiChordMsDuration;
            this.MsDuration = msDuration;
            this._ornamentNumberSymbol = midiChordDef.OrnamentNumberSymbol;
            this._pitchWheelDeviation = midiChordDef.PitchWheelDeviation;
            this._bank = midiChordDef.Bank;
            this._patch = midiChordDef.Patch;
            this._volume = midiChordDef.Volume;
            #endregion

            FitToDuration(msDuration);
        }

        ///// <summary>
        ///// Reads a MidiChordDef from a file, then fits the BasicMidiChordDefs into the given msDuration.
        ///// (MidiChordDefs saved inline in an SVG-MIDI score should already be fit into the msDuration...)
        ///// </summary>
        ///// <param name="r"></param>
        ///// <param name="msDuration"></param>
        //public MidiChordDef(XmlReader r, int msDuration)
        //    : this(r)
        //{
        //    MsDuration = msDuration;
        //    FitToDuration(msDuration);
        //}

        private void FitToDuration(int msDuration)
        {
            int basicMidiChordsMsDuration = 0;
            foreach(BasicMidiChordDef basicMidiChordDef in BasicMidiChordDefs)
                basicMidiChordsMsDuration += basicMidiChordDef.MsDuration;

            if(BasicMidiChordDefs.Count > 1 && basicMidiChordsMsDuration != MsDuration)
            {
                // Note that FitToDuration() may shorten the BasicMidiChordDefs list.
                BasicMidiChordDefs = FitToDuration(BasicMidiChordDefs, MsDuration, MinimumBasicMidiChordMsDuration);
            }
            else if(BasicMidiChordDefs.Count == 1)
            {
                BasicMidiChordDefs[0].MsDuration = MsDuration;
            }
        }

        #endregion

        /// <summary>
        /// Returns a list of (millisecond) durations whose sum is msDuration.
        /// The List contains the maximum number of durations which can be fit from relativeDurations into the msDuration
        /// such that no duration is less than minimumOrnamentChordMsDuration.
        /// </summary>
        /// <param name="msDuration"></param>
        /// <param name="relativeDurations"></param>
        /// <param name="ornamentMinMsDuration"></param>
        /// <returns></returns>
        private List<int> GetDurations(int msDuration, List<int> relativeDurations, int ornamentMinMsDuration)
        {
            int numberOfOrnamentChords = 0;
            float factor;

            GetNumberOfOrnamentChordsAndFactor(msDuration, relativeDurations, ornamentMinMsDuration,
                out numberOfOrnamentChords, out factor);

            List<int> intDurations = GetIntDurations(msDuration, relativeDurations, numberOfOrnamentChords, factor);
            return intDurations;
        }

        /// <summary>
        /// This function returns
        /// 1. the maximum number of ornament chords that can be fit into the given msDuration using the given
        ///     relativeDurations and minimumOrnamentChordMsDuration.
        /// 2. The factor by which to multiply the (first numberOfOrnamentChords) relativeDurations to arrive at msDuration.
        /// </summary>
        private void GetNumberOfOrnamentChordsAndFactor(int msDuration, List<int> relativeDurations, int minimumOrnamentChordMsDuration,
            out int numberOfOrnamentChords, out float factor)
        {
            bool okay = true;
            numberOfOrnamentChords = 1;
            factor = 1.0F;
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
        }

        private List<int> GetBasicMidiChordDurations(List<BasicMidiChordDef> ornamentChords)
        {
            List<int> returnList = new List<int>();
            foreach(BasicMidiChordDef bmc in ornamentChords)
            {
                returnList.Add(bmc.MsDuration);
            }
            return returnList;
        }

        /// <summary>
        /// This function returns a List whose count is numberOfOrnamentChords.
        /// It also ensures that the sum of the ints in the List is exactly equal to msDuration.
        /// This function is also used when setting the duration of a MidiMelodyDef.
        /// </summary>
        public List<int> GetIntDurations(int msDuration, List<int> relativeDurations, int numberOfOrnamentChords, float factor)
        {
            List<float> floatDurations = new List<float>();
            for(int i = 0; i < numberOfOrnamentChords; ++i)
                floatDurations.Add(relativeDurations[i] * factor);
            // basicDurations are the float durations taking into account minimumOrnamentChordMsDuration

            float fSum = 0F;
            int iSum = 0;
            List<int> actualIntDurations = new List<int>();
            foreach(float fd in floatDurations)
            {
                fSum += fd;
                int integer = (int)(fSum - iSum);
                iSum += integer;
                actualIntDurations.Add(integer);
            }

            int intSum = 0;
            foreach(int i in actualIntDurations)
                intSum += i;
            Debug.Assert(intSum <= msDuration);
            if(intSum < msDuration)
            {
                int lastDuration = actualIntDurations[actualIntDurations.Count - 1];
                lastDuration += (msDuration - intSum);
                actualIntDurations.RemoveAt(actualIntDurations.Count - 1);
                actualIntDurations.Add(lastDuration);
            }
            return actualIntDurations;
        }

        public void SetDuration(int msDuration)
        {
            MsDuration = msDuration;
            BasicMidiChordDefs = FitToDuration(BasicMidiChordDefs, MsDuration, _minimumBasicMidiChordMsDuration);
        }

        /// <summary>
        /// Note that, unlike MidiRestDefs, MidiChordDefs do not have a msDuration attribute.
        /// Their msDuration is deduced from the contained BasicMidiChords.
        /// </summary>
        public void WriteSvg(SvgWriter w)
        {
            if(!String.IsNullOrEmpty(ID) && !ID.Contains("localChord"))
                w.WriteAttributeString("id", ID); // the definition ID, not the local ID of a midiChord

            if(Bank != null && Bank != M.DefaultBankIndex)
                w.WriteAttributeString("bank", Bank.ToString());
            if(Patch != null && Patch != M.DefaultBankIndex)
                w.WriteAttributeString("patch", Patch.ToString());
            if(Volume != null && Volume != M.DefaultVolume)
                w.WriteAttributeString("volume", Volume.ToString());
            if(HasChordOff == false)
                w.WriteAttributeString("hasChordOff", "0");
            if(PitchWheelDeviation != null && PitchWheelDeviation != M.DefaultPitchWheelDeviation)
                w.WriteAttributeString("pitchWheelDeviation", PitchWheelDeviation.ToString());
            if(MinimumBasicMidiChordMsDuration != M.DefaultMinimumBasicMidiChordMsDuration)
                w.WriteAttributeString("minBasicChordMsDuration", MinimumBasicMidiChordMsDuration.ToString());

            Debug.Assert(BasicMidiChordDefs != null && BasicMidiChordDefs.Count > 0);

            w.WriteStartElement("score", "basicChords", null);
            foreach(BasicMidiChordDef basicMidiChord in BasicMidiChordDefs) // containing basic <midiChord> elements
                basicMidiChord.WriteSVG(w);
            w.WriteEndElement();

            if(MidiChordSliderDefs != null)
                MidiChordSliderDefs.WriteSVG(w); // writes score:sliders element
        }

        private static int _uniqueChordID = 0;
        public static int UniqueChordID { get { return ++_uniqueChordID; } }

        /// <summary>
        /// Fits the durations of the basicMidiChordDefs to the msOuterDuration, shortening the list if necessary.
        /// </summary>
        /// <param name="basicMidiChordDefs"></param>
        /// <param name="msOuterDuration"></param>
        /// <param name="minimumMsDuration"></param>
        /// <returns></returns>
        protected List<BasicMidiChordDef> FitToDuration(List<BasicMidiChordDef> basicMidiChordDefs, int msOuterDuration, int minimumMsDuration)
        {
            List<int> relativeDurations = GetBasicMidiChordDurations(basicMidiChordDefs);
            List<int> msDurations = GetDurations(msOuterDuration, relativeDurations, minimumMsDuration);
            // msDurations count can be less than basicMidiChordDefs.Count
            while(basicMidiChordDefs.Count > msDurations.Count)
                basicMidiChordDefs.RemoveAt(basicMidiChordDefs.Count - 1);

            Debug.Assert(basicMidiChordDefs.Count == msDurations.Count);

            for(int i = 0; i < msDurations.Count; ++i)
            {
                basicMidiChordDefs[i].MsDuration = msDurations[i];
            }

            return basicMidiChordDefs;
        }

        public string ID;

        protected byte? _bank = null;
        protected byte? _patch = null;
        protected byte? _volume = null;
        protected byte? _pitchWheelDeviation = null;
        protected bool _hasChordOff = true;
        protected int _minimumBasicMidiChordMsDuration = 1;

        public List<byte> MidiHeadSymbols { get { return _midiHeadSymbols; } }
        protected List<byte> _midiHeadSymbols = null;
        public byte MidiVelocitySymbol { get { return _midiVelocitySymbol; } }
        protected byte _midiVelocitySymbol;
        public int OrnamentNumberSymbol { get { return _ornamentNumberSymbol; } }
        protected int _ornamentNumberSymbol = 0;

        public byte? Bank { get { return _bank; } }
        public byte? Patch { get { return _patch; } }
        public byte? Volume { get { return _volume; } }
        public byte? PitchWheelDeviation { get { return _pitchWheelDeviation; } }
        public bool HasChordOff { get { return _hasChordOff; } }
        public int MinimumBasicMidiChordMsDuration { get { return _minimumBasicMidiChordMsDuration; } }
        public MidiChordSliderDefs MidiChordSliderDefs = null;
        public List<BasicMidiChordDef> BasicMidiChordDefs = new List<BasicMidiChordDef>();
   }
}
