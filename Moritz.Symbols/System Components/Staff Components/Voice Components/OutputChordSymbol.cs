using System.Text;
using System.Collections.Generic;
using System.Diagnostics;
using Moritz.Midi;
using Moritz.Globals;
using Moritz.Spec;
using Moritz.Xml;

namespace Moritz.Symbols
{
    public class OutputChordSymbol : ChordSymbol
    {
        public OutputChordSymbol(Voice voice, MidiChordDef umcd, int absMsPosition, int minimumCrotchetDurationMS, float fontSize)
            : base(voice, umcd.MsDuration, absMsPosition, minimumCrotchetDurationMS, fontSize)
        {
            _midiChordDef = umcd;

            _msDurationToNextBarline = umcd.MsDurationToNextBarline;

            SetNoteheadPitchesAndVelocities(umcd.NotatedMidiPitches, umcd.NotatedMidiVelocities);

            if(umcd.OrnamentNumberSymbol != 0)
            {
				OrnamentText ornamentText = new OrnamentText(this, "~" + umcd.OrnamentNumberSymbol.ToString(), FontHeight);
				DrawObjects.Add(ornamentText);
            }

            if(umcd.Lyric != null)
            {
				LyricText lyric = new LyricText(this, umcd.Lyric, FontHeight);
                DrawObjects.Add(lyric);
            }
        }

        /// <summary>
        /// used by CautonaryOutputChordSymbol
        /// </summary>
        public OutputChordSymbol(Voice voice, int msDuration, int absMsPosition, int minimumCrotchetDurationMS, float fontSize)
            : base(voice,  msDuration, absMsPosition, minimumCrotchetDurationMS, fontSize)
        {

        }

        /// <summary>
        /// This function uses a sophisticated algorithm to decide whether flats or sharps are to be used to
        /// represent the chord. Chords can have naturals and either sharps or flats (but not both).
        /// The display of naturals is forced if the same notehead height also exists with a sharp or flat.
        /// (The display of other accidentals are always forced in the Head constructor.)
        /// Exceptions: This function throws an exception if
        ///     1) any of the input midiPitches is out of midi range (0..127).
        ///     2) the midiPitches are not in ascending order
        ///     3) the midiPitches are not unique.
        /// The midiPitches argument must be in order of size (ascending), but Heads are created in top-down order.
        /// </summary>
        /// <param name="midiPitches"></param>
        public void SetNoteheadPitchesAndVelocities(List<byte> midiPitches, List<byte> midiVelocities)
        {
            #region check inputs
             Debug.Assert(midiPitches.Count == midiVelocities.Count);
            int previousPitch = -1;
            foreach(int midiPitch in midiPitches)
            {
                Debug.Assert(midiPitch >= 0 && midiPitch <= 127, "midiPitch out of range.");
                Debug.Assert(midiPitch > previousPitch, "midiPitches must be unique and in ascending order.");
                previousPitch = midiPitch;
            }
            foreach(int midiVelocity in midiVelocities)
            {
                Debug.Assert(midiVelocity >= 0 && midiVelocity <= 127, "midiVelocity out of range.");
            }
            #endregion
            this.HeadsTopDown.Clear();
            bool useSharp = UseSharps(midiPitches, midiVelocities); // returns false if flats are to be used
            for(int i = midiPitches.Count - 1; i >= 0; --i)
            {
                Head head = new Head(this, midiPitches[i], midiVelocities[i], useSharp);
                this.HeadsTopDown.Add(head);
            }
            for(int i = 0; i < midiPitches.Count; i++)
            {
                if(this.HeadsTopDown[i].Alteration == 0)
                {
                    this.HeadsTopDown[i].DisplayAccidental = DisplayAccidental.suppress;
                }
            }
            for(int i = 1; i < midiPitches.Count; i++)
            {
                if(this.HeadsTopDown[i].Pitch == this.HeadsTopDown[i - 1].Pitch)
                {
                    this.HeadsTopDown[i - 1].DisplayAccidental = DisplayAccidental.force;
                    this.HeadsTopDown[i].DisplayAccidental = DisplayAccidental.force;
                }
            }
        }

        // These colours were found using a separate, self-written program.
        private static readonly List<string> noteheadColors = new List<string>()
            {
                "#FF0000", // fff
                "#D50055", // ff
                "#B5007E", // f
                "#8800B5", // mf 
                "#0000CA", // mp 
                "#0069A8", // p
                "#008474", // pp
                "#009F28", // ppp
                "#00CA00"  // pppp
            };

        internal void SetNoteheadColors()
        {
            foreach(Head head in HeadsTopDown)
            {
                Debug.Assert(head.MidiVelocity >= 0 && head.MidiVelocity <= 127);

                int velocity = head.MidiVelocity;
                if(velocity > M.MaxMidiVelocity["ff"])
                {
                    head.ColorAttribute = noteheadColors[0]; // fff    
                }
                else if(velocity > M.MaxMidiVelocity["f"])
                {
                    head.ColorAttribute = noteheadColors[1]; // ff
                }
                else if(velocity > M.MaxMidiVelocity["mf"])
                {
                    head.ColorAttribute = noteheadColors[2]; // f
                }
                else if(velocity > M.MaxMidiVelocity["mp"])
                {
                    head.ColorAttribute = noteheadColors[3]; // mf
                }
                else if(velocity > M.MaxMidiVelocity["p"])
                {
                    head.ColorAttribute = noteheadColors[4]; // mp
                }
                else if(velocity > M.MaxMidiVelocity["pp"])
                {
                    head.ColorAttribute = noteheadColors[5]; // p
                }
                else if(velocity > M.MaxMidiVelocity["ppp"])
                {
                    head.ColorAttribute = noteheadColors[6]; // pp
                }
                else if(velocity > M.MaxMidiVelocity["pppp"])
                {
                    head.ColorAttribute = noteheadColors[7]; // ppp
                }
                else // > 0 
                {
                    head.ColorAttribute = noteheadColors[8]; // pppp
                }
            }
        }

        public override void WriteSVG(SvgWriter w, bool staffIsVisible)
        {
            if(staffIsVisible && ChordMetrics.BeamBlock != null)
                ChordMetrics.BeamBlock.WriteSVG(w);

			w.SvgStartGroup("outputChord");
			if(staffIsVisible)
			{ 
				w.WriteAttributeString("score", "alignmentX", null, ChordMetrics.OriginX.ToString(M.En_USNumberFormat));
			}
            
            _midiChordDef.WriteSvg(w);

            if(staffIsVisible)
            {
				w.SvgStartGroup("graphics");
                ChordMetrics.WriteSVG(w);
                w.SvgEndGroup();
            }

            w.SvgEndGroup();
        }


        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("output chord  ");
            sb.Append(InfoString);
            return sb.ToString();
        }

        public MidiChordDef MidiChordDef { get { return _midiChordDef; } }
        protected MidiChordDef _midiChordDef = null;
    }
}
