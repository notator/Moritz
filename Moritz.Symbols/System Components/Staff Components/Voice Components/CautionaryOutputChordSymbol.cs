using System;
using System.Text;

using Moritz.Xml;
using Moritz.Spec;

namespace Moritz.Symbols
{
    internal class CautionaryOutputChordSymbol : OutputChordSymbol
    {
        public CautionaryOutputChordSymbol(Voice voice, CautionaryChordDef lccd, int absMsPosition, float fontSize)
            : base(voice, lccd.MsDuration, absMsPosition, 600, fontSize)
        {
            SetNoteheadPitchesAndVelocities(lccd.NotatedMidiPitches, lccd.NotatedMidiVelocities);

            _durationClass = DurationClass.cautionary;
            _msDuration = 0;
            Stem.Draw = false;
        }

        public override void WriteSVG(SvgWriter w, bool staffIsVisible)
        {
            if(Visible && staffIsVisible)
            {
                w.SvgStartGroup("cautionaryChord");
                w.SvgStartGroup(null);

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