using System.Text;
using System.Diagnostics;

using Moritz.Score.Notation;
using Moritz.Score.Midi;

namespace Moritz.Score
{
    internal class CautionaryChordSymbol : ChordSymbol
    {
        public CautionaryChordSymbol(Voice voice, LocalizedCautionaryChordDef lccd, float fontSize)
            : base(voice, lccd, 600, fontSize)
        {
            _durationClass = DurationClass.cautionary;
            _msDuration = 0;
            Stem.Draw = false;
 
            //MidiChordDef mcd = lccd.UniqueMidiDurationDef as MidiChordDef;
            //if(mcd != null)
            //{
            //    SetHeads(mcd.MidiHeadSymbols);

            //    if(mcd.OrnamentNumberSymbol != 0)
            //    {
            //        AddOrnamentSymbol("~" + mcd.OrnamentNumberSymbol.ToString());
            //    }
            //}
        }

        public override void WriteSVG(SvgWriter w)
        {
            if(Visible)
            {
                string idNumber = SvgScore.UniqueID_Number;
                w.SvgStartGroup("cautionaryChord" + idNumber);
                w.SvgStartGroup("graphics" + idNumber);

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

        MidiChordDef CautionaryMidiChordDef = null;
        public float LeftBracketLeft = float.MaxValue;
        public float RightBracketRight = float.MinValue;
 
        /// <summary>
        /// This value is set to false if this symbol is not
        /// the first chordSymbol in the voice in the final score.
        /// </summary>
        public bool Visible = true;
    }
}
