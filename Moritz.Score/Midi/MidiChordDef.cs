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
        /// These attributes are provided by embedding a clone of the MidiChordDef in a LocalMidiChordDef.
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
                    case "repeat":
                        // repeat is true if this attribute is not present
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
                            SetFromMidiChordDefs(scoreMidiDurationDefs, midiChordDefID);
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

        private void SetFromMidiChordDefs(Dictionary<string, MidiDurationDef> scoreMidiDurationDefs, string midiChordDefID)
        {
            MidiDurationDef originalMdd = scoreMidiDurationDefs[midiChordDefID];
            MidiChordDef original = originalMdd as MidiChordDef;
            Debug.Assert(original != null);

            this._bank = original.Bank;
            this._patch = original.Patch;
            this._volume = original.Volume;
            this._ornamentNumberSymbol = original.OrnamentNumberSymbol;
            this._pitchWheelDeviation = original.PitchWheelDeviation;
            this._midiHeadSymbols = original.MidiHeadSymbols;
            this._midiVelocity = original.MidiVelocity;
            this._minimumBasicMidiChordMsDuration = original.MinimumBasicMidiChordMsDuration;

            BasicMidiChordDefs = original.BasicMidiChordDefs;
            MidiChordSliderDefs = original.MidiChordSliderDefs;
        }

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
            int numberOfOrnamentChords = GetNumberOfOrnamentChords(msDuration, relativeDurations, ornamentMinMsDuration);

            List<int> intDurations = GetIntDurations(msDuration, relativeDurations, numberOfOrnamentChords);
            return intDurations;
        }

        /// <summary>
        /// This function returns the maximum number of ornament chords that can be fit into the given msDuration
        /// using the given relativeDurations and minimumOrnamentChordMsDuration.
        /// </summary>
        private int GetNumberOfOrnamentChords(int msDuration, List<int> relativeDurations, int minimumOrnamentChordMsDuration)
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

        public void SetDuration(int msDuration)
        {
            MsDuration = msDuration;
            BasicMidiChordDefs = FitToDuration(BasicMidiChordDefs, MsDuration, _minimumBasicMidiChordMsDuration);
        }

        public override IUniqueMidiDurationDef CreateUniqueMidiDurationDef()
        {
            UniqueMidiChordDef umcd = new UniqueMidiChordDef(this); // a deep clone with a special id string.
            umcd.MsPosition = 0;
            umcd.MsDuration = this.MsDuration;
            return umcd;
        }

        private static int _uniqueChordID = 0;
        public static int UniqueChordID { get { return ++_uniqueChordID; } }

        /// <summary>
        /// Returns a new list of basicMidiChordDefs having the msOuterDuration, shortening the list if necessary.
        /// </summary>
        /// <param name="basicMidiChordDefs"></param>
        /// <param name="msOuterDuration"></param>
        /// <param name="minimumMsDuration"></param>
        /// <returns></returns>
        protected List<BasicMidiChordDef> FitToDuration(List<BasicMidiChordDef> bmcd, int msOuterDuration, int minimumMsDuration)
        {
            List<int> relativeDurations = GetBasicMidiChordDurations(bmcd);
            List<int> msDurations = GetDurations(msOuterDuration, relativeDurations, minimumMsDuration);
            
            // msDurations.Count can be less than bmcd.Count

            List<BasicMidiChordDef> rList = new List<BasicMidiChordDef>();
            BasicMidiChordDef b;
            for(int i = 0; i < msDurations.Count; ++i)
            {
                b = bmcd[i];
                rList.Add(new BasicMidiChordDef(msDurations[i], b.BankIndex, b.PatchIndex, b.HasChordOff, b.Notes, b.Velocities));
            }

            return rList;
        }

        public string ID;

        protected byte? _bank = null;
        protected byte? _patch = null;
        protected byte? _volume = null;
        protected bool _repeat = true;
        protected byte? _pitchWheelDeviation = null;
        protected bool _hasChordOff = true;
        protected int _minimumBasicMidiChordMsDuration = 1;

        public List<byte> MidiHeadSymbols { get { return _midiHeadSymbols; } }
        protected List<byte> _midiHeadSymbols = null;
        public byte MidiVelocity { get { return _midiVelocity; } }
        protected byte _midiVelocity;
        public int OrnamentNumberSymbol { get { return _ornamentNumberSymbol; } }
        protected int _ornamentNumberSymbol = 0;
        public string Lyric { get { return _lyric; } set { _lyric = value; } }
        protected string _lyric = null;

        public byte? Bank { get { return _bank; } }
        public byte? Patch { get { return _patch; } }
        public byte? Volume { get { return _volume; } }
        // If Repeat is true, the MidiChord will repeat in assisted performances
        // if the performed duration is longer than the default duration.
        public bool Repeat { get { return _repeat; } } 
        public byte? PitchWheelDeviation { get { return _pitchWheelDeviation; } }
        public bool HasChordOff { get { return _hasChordOff; } }
        public int MinimumBasicMidiChordMsDuration { get { return _minimumBasicMidiChordMsDuration; } }
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
        public MidiChordSliderDefs MidiChordSliderDefs = null;
        public List<BasicMidiChordDef> BasicMidiChordDefs = new List<BasicMidiChordDef>();
    }
}
