using System;
using System.Text;

using Moritz.Xml;
using Moritz.Spec;

namespace Moritz.Symbols
{
    internal class CautionaryInputChordSymbol : InputChordSymbol
    {
        //(Voice voice, InputChordDef umcd, int absMsPosition, int minimumCrotchetDurationMS, float fontSize)
        public CautionaryInputChordSymbol(Voice voice, CautionaryChordDef lccd, int absMsPosition, float fontSize)
            : base(voice, lccd.MsDuration, absMsPosition, 600, fontSize)
        {
            SetNoteheadPitches(lccd.NotatedMidiPitches);

            _durationClass = DurationClass.cautionary;
            _msDuration = 0;
            Stem.Draw = false;
        }

        public override void WriteSVG(SvgWriter w, bool staffIsVisible)
        {
            if(Visible && staffIsVisible)
            {
                w.SvgStartGroup(ChordMetrics.CSSClass.ToString()); // "cautionaryInputChord"
                w.SvgStartGroup(null);

                this.ChordMetrics.WriteSVG(w, true, true);

                w.SvgEndGroup();
                w.SvgEndGroup();
            }
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("cautionaryInputChord  ");
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
