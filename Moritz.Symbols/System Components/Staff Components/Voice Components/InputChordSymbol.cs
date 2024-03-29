using Moritz.Globals;
using Moritz.Spec;
using Moritz.Xml;

using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace Moritz.Symbols
{
    public class InputChordSymbol : ChordSymbol
    {
        public InputChordSymbol(Voice voice, InputChordDef umcd, int absMsPosition, PageFormat pageFormat)
            : base(voice, umcd.MsDuration, absMsPosition, pageFormat.MinimumCrotchetDuration, pageFormat.MusicFontHeight * pageFormat.InputSizeFactor, umcd.BeamContinues)
        {
            _inputChordDef = umcd;

            _msDurationToNextBarline = umcd.MsDurationToNextBarline;

            SetNoteheadPitches(umcd.NotatedMidiPitches);

            if(umcd.Lyric != null)
            {
                LyricText lyricText = new LyricText(this, umcd.Lyric, FontHeight);
                DrawObjects.Add(lyricText);
            }

            if(umcd.Dynamic != null)
            {
                DynamicText dynamicText = new DynamicText(this, umcd.Dynamic, FontHeight);
                DrawObjects.Add(dynamicText);
            }
        }

        /// <summary>
        /// used by CautionaryInputChordSymbol
        /// </summary>
        public InputChordSymbol(Voice voice, int msDuration, int absMsPosition, int minimumCrotchetDurationMS, float fontSize)
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
        public void SetNoteheadPitches(List<byte> midiPitches)
        {
            #region check inputs
            int previousPitch = -1;
            foreach(int midiPitch in midiPitches)
            {
                Debug.Assert(midiPitch >= 0 && midiPitch <= 127, "midiPitch out of range.");
                Debug.Assert(midiPitch > previousPitch, "midiPitches must be unique and in ascending order.");
                previousPitch = midiPitch;
            }
            #endregion
            this.HeadsTopDown.Clear();
            bool useSharp = UseSharps(midiPitches, null); // returns false if flats are to be used
            for(int i = midiPitches.Count - 1; i >= 0; --i)
            {
                Head head = new Head(this, midiPitches[i], -1, useSharp);
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

        public override void WriteSVG(SvgWriter w)
        {
            if(ChordMetrics.BeamBlock != null)
                ChordMetrics.BeamBlock.WriteSVG(w);

            w.SvgStartGroup(ChordMetrics.CSSObjectClass.ToString()); // "inputChord";

            Debug.Assert(_msDuration > 0);

            w.WriteAttributeString("score", "alignment", null, ChordMetrics.OriginX.ToString(M.En_USNumberFormat));
            w.WriteAttributeString("score", "msDuration", null, _msDuration.ToString());

            _inputChordDef.WriteSVG(w);

            w.SvgStartGroup(CSSObjectClass.graphics.ToString());
            ChordMetrics.WriteSVG(w);
            w.SvgEndGroup();

            w.SvgEndGroup();
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("inputChord  ");
            sb.Append(InfoString);
            return sb.ToString();
        }

        public InputChordDef InputChordDef { get { return _inputChordDef; } }
        protected InputChordDef _inputChordDef = null;

    }
}
