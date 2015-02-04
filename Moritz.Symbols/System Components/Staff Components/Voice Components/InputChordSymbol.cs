using System.Text;

using System.Diagnostics;

using Moritz.Globals;
using Moritz.Xml;
using Moritz.Spec;

namespace Moritz.Symbols
{
    public class InputChordSymbol : ChordSymbol
    {
        public InputChordSymbol(Voice voice, InputChordDef umcd, int minimumCrotchetDurationMS, float fontSize)
            : base(voice, umcd.MsDuration, umcd.MsPosition, minimumCrotchetDurationMS, fontSize)
        {
            _inputChordDef = umcd;

            _msDurationToNextBarline = umcd.MsDurationToNextBarline;

            SetNoteheadPitches(umcd.NotatedMidiPitches);

            if(umcd.Lyric != null)
            {
                TextInfo textInfo = new TextInfo(umcd.Lyric, "Arial", (float)(FontHeight / 2F), TextHorizAlign.center);
                Lyric lyricText = new Lyric(this, textInfo);
                DrawObjects.Add(lyricText);
            }
        }

        public override void WriteSVG(SvgWriter w, bool staffIsVisible)
        {
            if(ChordMetrics.BeamBlock != null)
                ChordMetrics.BeamBlock.WriteSVG(w);

            w.SvgStartGroup("inputChord", null);

            Debug.Assert(_msDuration > 0);

			if(staffIsVisible)
			{
				w.WriteAttributeString("score", "alignmentX", null, ChordMetrics.OriginX.ToString(M.En_USNumberFormat));
			}
            w.WriteAttributeString("score", "msDuration", null, _msDuration.ToString());

            if(_inputChordDef.Lyric != null)
            {
                w.WriteAttributeString("score", "lyric", null, _inputChordDef.Lyric);
            }

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
