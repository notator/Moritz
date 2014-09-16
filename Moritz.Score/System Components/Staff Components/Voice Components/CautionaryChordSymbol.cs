using System;
using System.Text;

using Moritz.Score.Notation;

namespace Moritz.Score
{
    internal class CautionaryChordSymbol : ChordSymbol
    {
        public CautionaryChordSymbol(Voice voice, CautionaryChordDef lccd, float fontSize)
            : base(voice, lccd.MsDuration, lccd.MsPosition, 600, fontSize)
        {
            SetNoteheadPitches(lccd.MidiPitches);

            _durationClass = DurationClass.cautionary;
            _msDuration = 0;
            Stem.Draw = false;
        }

        public override void WriteSVG(SvgWriter w)
        {
            if(Visible)
            {
                w.SvgStartGroup("cautionaryChord", null);
                w.SvgStartGroup(null, null);

                this.ChordMetrics.WriteSvg(w);

                w.SvgEndGroup();
                w.SvgEndGroup();
            }
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("cautionaryChord  ");
            sb.Append(InfoString);
            return sb.ToString();
        }

        public float LeftBracketLeft = float.MaxValue;
        public float RightBracketRight = float.MinValue;
 
        /// <summary>
        /// This value is set to false if this symbol is not
        /// the first chordSymbol in the voice in the final score.
        /// </summary>
        public bool Visible = true;
    }
}
