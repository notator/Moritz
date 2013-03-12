using System.Text;
using System.Diagnostics;

using Moritz.Score.Notation;
using Moritz.Score.Midi;

namespace Moritz.Score
{
    internal class CautionaryChordSymbol : ChordSymbol
    {
        public CautionaryChordSymbol(Voice voice, OverlapLmddAtStartOfBar olaso, float fontSize)
            : base(voice, olaso, 600, fontSize)
        {
            _durationClass = DurationClass.cautionary;
            _msDuration = 0;
            Stem.Draw = false;
            // _localizedMidiDurationDef.MidiChordDef is null
            // the original msDuration can still be found at 
            CautionaryMidiChordDef = olaso.CautionaryMidiChordDef;
            if(CautionaryMidiChordDef != null)
            {
                SetHeads(CautionaryMidiChordDef.MidiHeadSymbols);

                if(CautionaryMidiChordDef.OrnamentNumberSymbol != 0)
                {
                    AddOrnamentSymbol("~" + CautionaryMidiChordDef.OrnamentNumberSymbol.ToString());
                }
            }

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
