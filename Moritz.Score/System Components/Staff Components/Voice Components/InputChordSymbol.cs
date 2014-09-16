using System.Text;

using Moritz.Score.Notation;
using Moritz.Globals;

namespace Moritz.Score
{
    public class InputChordSymbol : ChordSymbol
    {
        public InputChordSymbol(Voice voice, InputChordDef umcd, int minimumCrotchetDurationMS, float fontSize)
            : base(voice, umcd.MsDuration, umcd.MsPosition, minimumCrotchetDurationMS, fontSize)
        {
            //_uniqueMidiChordDef = umcd;
            //MidiChordDef midiChordDef = umcd as MidiChordDef;
            //if(midiChordDef != null)
            //{
            //    SetHeads(midiChordDef.MidiHeadSymbols);

            //    if(midiChordDef.OrnamentNumberSymbol != 0)
            //    {
            //        AddOrnamentSymbol("~" + midiChordDef.OrnamentNumberSymbol.ToString());
            //    }

            //    if(midiChordDef.Lyric != null)
            //    {
            //        TextInfo textInfo = new TextInfo(midiChordDef.Lyric, "Arial", (float)(FontHeight / 2F), TextHorizAlign.center);
            //        Lyric lyric = new Lyric(this, textInfo);
            //        DrawObjects.Add(lyric);
            //    }
            //}
        }

        public override void WriteSVG(SvgWriter w)
        {
            if(ChordMetrics.BeamBlock != null)
                ChordMetrics.BeamBlock.WriteSVG(w);

            w.SvgStartGroup("inputChord", null);
            //w.WriteAttributeString("score", "object", null, "inputChord");
            w.WriteAttributeString("score", "alignmentX", null, this.Metrics.OriginX.ToString(M.En_USNumberFormat));

            _inputChordDef.WriteSvg(w);

            w.SvgStartGroup("graphics", null);
            ChordMetrics.WriteSVG(w);
            w.SvgEndGroup();

            w.SvgEndGroup();
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("input chord  ");
            sb.Append(InfoString);
            return sb.ToString();
        }

        public InputChordDef InputChordDef { get { return _inputChordDef; } }
        protected InputChordDef _inputChordDef = null;
    }
}
