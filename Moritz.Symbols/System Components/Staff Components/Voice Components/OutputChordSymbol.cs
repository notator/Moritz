using System.Text;
using System.Collections.Generic;
using System.Diagnostics;
using Moritz.Globals;
using Moritz.Spec;
using Moritz.Xml;
using System;

namespace Moritz.Symbols
{
    public class OutputChordSymbol : ChordSymbol
    {
        public OutputChordSymbol(Voice voice, MidiChordDef umcd, int absMsPosition, int minimumCrotchetDurationMS, float fontSize)
            : base(voice, umcd.MsDuration, absMsPosition, minimumCrotchetDurationMS, fontSize, umcd.BeamContinues)
        {
            _midiChordDef = umcd;

            _msDurationToNextBarline = umcd.MsDurationToNextBarline;

            SetNoteheadPitchesAndVelocities(umcd.NotatedMidiPitches, umcd.NotatedMidiVelocities);

            if(! string.IsNullOrEmpty(umcd.OrnamentID))
            {
				string ornamentString = "~" + umcd.OrnamentID;
				// int.TryParse only returns true here if umcd.OrnamentID contains nothing but integer characters.
				// A string such as "1a" will therefore result in a genericOrnament.  
				if(int.TryParse(umcd.OrnamentID, out int value))
				{
					if(value > 0)
					{
						NumericOrnamentText numericOrnamentText = new NumericOrnamentText(this, ornamentString, FontHeight); // The fontHeight argument is actually never used!
						DrawObjects.Add(numericOrnamentText);
					}
				}
				else
				{ 
					GenericOrnamentText genericOrnamentText = new GenericOrnamentText(this, ornamentString, FontHeight * 2.0F); // The fontHeight argument is actually never used!
					DrawObjects.Add(genericOrnamentText);
				}
            }

            if(umcd.Lyric != null)
            {
				LyricText lyric = new LyricText(this, umcd.Lyric, FontHeight);
                DrawObjects.Add(lyric);
            }
        }

        /// <summary>
        /// used by CautionaryOutputChordSymbol
        /// </summary>
        public OutputChordSymbol(Voice voice, int msDuration, int absMsPosition, int minimumCrotchetDurationMS, float fontSize)
            : base(voice, msDuration, absMsPosition, minimumCrotchetDurationMS, fontSize, false)
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

        internal void SetNoteheadColorClasses()
        {
            foreach(Head head in HeadsTopDown)
            {
                Debug.Assert(head.MidiVelocity >= 0 && head.MidiVelocity <= 127);

                int velocity = head.MidiVelocity;
                if(velocity > M.MaxMidiVelocity[M.Dynamic.ff])
                {
                    head.ColorClass = CSSColorClass.fffColor;    
                }
                else if(velocity > M.MaxMidiVelocity[M.Dynamic.f])
                {
                    head.ColorClass = CSSColorClass.ffColor;
                }
                else if(velocity > M.MaxMidiVelocity[M.Dynamic.mf])
                {
                    head.ColorClass = CSSColorClass.fColor;
				}
                else if(velocity > M.MaxMidiVelocity[M.Dynamic.mp])
                {
                    head.ColorClass = CSSColorClass.mfColor;
				}
                else if(velocity > M.MaxMidiVelocity[M.Dynamic.p])
                {
                    head.ColorClass = CSSColorClass.mpColor;
				}
                else if(velocity > M.MaxMidiVelocity[M.Dynamic.pp])
                {
                    head.ColorClass = CSSColorClass.pColor;
				}
                else if(velocity > M.MaxMidiVelocity[M.Dynamic.ppp])
                {
                    head.ColorClass = CSSColorClass.ppColor;
				}
                else if(velocity > M.MaxMidiVelocity[M.Dynamic.pppp])
                {
                    head.ColorClass = CSSColorClass.pppColor;
				}
                else // > 0 
                {
                    head.ColorClass = CSSColorClass.ppppColor;
				}
            }
        }

        /// <summary>
        /// Dont use this function.
        /// </summary>
        /// <param name="w"></param>
        public override void WriteSVG(SvgWriter w)
        {
            throw new NotImplementedException();
        }

        public void WriteSVG(SvgWriter w, int channel, CarryMsgs carryMsgs)
        {
            if(ChordMetrics.BeamBlock != null)
                ChordMetrics.BeamBlock.WriteSVG(w);

			w.SvgStartGroup(CSSObjectClass.chord.ToString()); // "chord"
			w.WriteAttributeString("score", "alignment", null, ChordMetrics.OriginX.ToString(M.En_USNumberFormat));
            
            _midiChordDef.WriteSVG(w, channel, carryMsgs);

            w.SvgStartGroup(CSSObjectClass.graphics.ToString());
            ChordMetrics.WriteSVG(w);
            w.SvgEndGroup();

            w.SvgEndGroup(); // "chord"
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("outputChord  ");
            sb.Append(InfoString);
            return sb.ToString();
        }

        public MidiChordDef MidiChordDef { get { return _midiChordDef; } }
        protected MidiChordDef _midiChordDef = null;
    }
}
