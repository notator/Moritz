using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Diagnostics;

using Moritz.Globals;

namespace Moritz.Score
{
    public class Head
    {
        /// <summary>
        /// Construct a new Head with a particular midi pitch number.
        /// </summary>
        /// <param name="chord">The containing ChordSymbol</param>
        /// <param name="midiPitch">The midiPitch. Must be in range [0..127]</param>
        /// <param name="useSharp">true means use #, false means use flat(if there is a choice).</param>
        public Head(ChordSymbol chord, int midiPitch, bool useSharp)
        {
            Chord = chord;
            KeyValuePair<string, int> sharpKVP = new KeyValuePair<string, int>();
            KeyValuePair<string, int> flatKVP = new KeyValuePair<string, int>();
            KeyValuePair<string, int> previousKVP = new KeyValuePair<string, int>("C0", 0);
            foreach(KeyValuePair<string, int> kvp in M.MidiPitchDict)
            {
                if(kvp.Value >= midiPitch)
                {
                    flatKVP = kvp;
                    sharpKVP = previousKVP;
                    break;
                }
                previousKVP = kvp;
            }
            if(flatKVP.Value == midiPitch)
            {
                Pitch = flatKVP.Key;
                Alteration = 0;
            }
            else if(useSharp)
            {
                Pitch = sharpKVP.Key;
                Alteration = midiPitch - sharpKVP.Value;
            }
            else
            {
                Pitch = flatKVP.Key;
                Alteration = midiPitch - flatKVP.Value;
            }
            if(Alteration != 0)
                DisplayAccidental = DisplayAccidental.force;

            if(chord != null)
                FontSize = chord.FontHeight;
        }

        /// <summary>
        /// Returns the head's y-coordinates wrt the chord.
        /// The chord's y-origin is the top line of the staff.
        /// (Y-Alignment of a notehead on the top line of the staff is 0.)
        /// Uses the following protected variables (in Metrics) which have been set by GetStaffParameters()
        ///     protected float _gapVBPX = 0F; 
        ///     protected int _nStafflines = 0;
        ///     protected ClefSign _clef = null;
        /// </summary>
        /// <param name="headIndex"></param>
        /// <param name="?"></param>
        /// <param name="?"></param>
        public float GetOriginY(ClefSign clef, float gap)
        {
            string[] alphabet = { "C", "D", "E", "F", "G", "A", "B" };
            float shiftFactor = 0F;
            switch(this.Pitch[0])
            {
                case 'A':
                    shiftFactor = 2.5F;
                    break;
                case 'B':
                    shiftFactor = 2F;
                    break;
                case 'C':
                    shiftFactor = 5;
                    break;
                case 'D':
                    shiftFactor = 4.5F;
                    break;
                case 'E':
                    shiftFactor = 4F;
                    break;
                case 'F':
                    shiftFactor = 3.5F;
                    break;
                case 'G':
                    shiftFactor = 3F;
                    break;
            }

            // shiftFactor is currently for the octave above middle C (octave = 5) on a normal treble clef
            // F6 is at shiftFactor 0 (the top line of the staff).
            int octave = 0;
            try
            {
                string octaveString = this.Pitch.Substring(1);
                if(octaveString == ":")
                    octave = 10;
                else
                    octave = int.Parse(octaveString);
            }
            catch
            {
                Debug.Assert(false, "Error in octave string");
            }
            float octaveShift = octave - 5F;
            shiftFactor -= (octaveShift * 3.5F); // 3.5 spaces is one octave
            // shiftFactor is currently correct for all octaves on a normal treble clef
            switch(clef.ClefName)
            {
                case "t":
                    break;
                case "t1": // trebleClef8
                    shiftFactor += 3.5F; // shift down one octave
                    break;
                case "t2": // trebleClef2x8
                    shiftFactor += 7F; // shift down two octaves
                    break;
                case "t3": // trebleClef3x8
                    shiftFactor += 10.5F; // shift down three octaves
                    break;
                case "b":
                    shiftFactor -= 6F; // shift up six spaces
                    break;
                case "b1": // bassClef8
                    shiftFactor -= 9.5F; // shift up six spaces + 1 octave
                    break;
                case "b2": // bassClef2x8
                    shiftFactor -= 13F; // shift up six spaces + 2 octaves
                    break;
                case "b3": // bassClef3x8
                    shiftFactor -= 16.5F; // shift up six spaces + 3 octaves
                    break;
                default:
                    break;
            }

            float headY = shiftFactor * gap;
            return headY;
        }


        public readonly ChordSymbol Chord;

        /// <summary>
        /// absolute diatonic pitch
        /// middle C (c'): "C5"
        /// </summary>
        public string Pitch
        {
            get { return _pitch; }
            set
            {
                Debug.Assert(Regex.Matches(value, @"^[A-G][0-9|:]$") != null);
                _pitch = value;
            }
        }
        /// <summary>
        /// Value (-2..+2) should be set to the value of a current OctaveClef (i.e. 8va----|).
        /// Setting this value changes the value returned by MidiPitch.
        /// </summary>
        public int OctaveTransposition
        {
            get { return _octaveTransposition; }
            set
            {
                int midiPitch = -1;
                if(value >= -2 && value <= 2)
                {
                    _octaveTransposition = value;
                    midiPitch = MidiPitch;
                }

                if(midiPitch < 0 || midiPitch > 127)
                {
                    throw new ErrorInScoreException("Error: Head.OctaveTransposition value out of range.");
                }
            }
        }
        private int _octaveTransposition = 0;

        /// <summary>
        /// The midi number associated with the head's pitch.
        /// Takes head.Pitch, head.Alteration, head.OctaveTransposition and Staff.CurrentContext.Transposition
        /// into account.)
        /// </summary>
        public int MidiPitch
        {
            get
            {
                int midiPitch = 0;
                if(!string.IsNullOrEmpty(_pitch) && M.MidiPitchDict.ContainsKey(_pitch))
                {
                    midiPitch = M.MidiPitchDict[_pitch];
                    midiPitch += _alteration;
                    midiPitch += (_octaveTransposition * 12);
                    //midiPitch += this.Chord.Voice.Staff.StaffLayout.Sounds[0].Transpose;
                }
                return midiPitch;
            }
        }
        public bool Silent = false;
        public HeadShape Shape = HeadShape.auto;
        public int Alteration
        {
            get { return _alteration; }
            set
            {
                Debug.Assert(value >= -2 && value <= 2);
                _alteration = value;
            }
        }
        /// <summary>
        /// Specifies if and how the accidental should be displayed. Meanings:
        ///  auto: automatically
        ///  suppress: don't display an accidental
        ///  force: always display an accidental
        ///  parenth: show accidental in parentheses
        /// </summary>
        public DisplayAccidental DisplayAccidental = DisplayAccidental.auto;
        public float XShift_Gap4 = 0F; // shifts the accidental, not the notehead!
        public float FontSize = 0F;

        private string _pitch;
        private int _alteration = 0;
    }
}
