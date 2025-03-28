using Moritz.Spec;
using Moritz.Xml;

using System.Text;

namespace Moritz.Symbols
{
    internal class CautionaryChordSymbol : ChordSymbol
    {
        public CautionaryChordSymbol(Voice voice, CautionaryChordDef lccd, int absMsPosition, PageFormat pageFormat)
            : base(voice, lccd.MsDuration, absMsPosition, pageFormat.MinimumCrotchetDuration, pageFormat.MusicFontHeight * pageFormat.SmallSizeFactor)
        {
            SetNoteheadPitchesAndVelocities(lccd.Pitches, lccd.Velocities);

            _durationClass = DurationClass.cautionary;
            _msDuration = 0;
            Stem.Draw = false;
        }

        public override void WriteSVG(SvgWriter w)
        {
            if(Visible)
            {
                w.SvgStartGroup(ChordMetrics.CSSObjectClass.ToString()); // "cautionaryChord"
                w.SvgStartGroup(null);

                this.ChordMetrics.WriteSVG(w);

                w.SvgEndGroup();
                w.SvgEndGroup();
            }
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("cautionaryOutputChord  ");
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
