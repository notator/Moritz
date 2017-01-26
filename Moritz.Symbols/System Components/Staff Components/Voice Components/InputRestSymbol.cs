using System;
using System.Text;
using System.Diagnostics;

using Moritz.Globals;
using Moritz.Xml;
using Moritz.Spec;

namespace Moritz.Symbols
{
	internal class InputRestSymbol : RestSymbol
	{
        public InputRestSymbol(Voice voice, IUniqueDef iumdd, int absMsPosition, int minimumCrotchetDurationMS, float fontHeight)
            : base(voice, iumdd, absMsPosition, minimumCrotchetDurationMS, fontHeight)
        {
            if(iumdd is CautionaryChordDef)
            {
                Console.WriteLine("rest is CautionaryChordDef!");
            }
            LocalCautionaryChordDef = iumdd as CautionaryChordDef;
        }

        public override void WriteSVG(SvgWriter w, bool staffIsVisible)
        {
            if(LocalCautionaryChordDef == null)
            {
				w.SvgStartGroup("inputRest");

                Debug.Assert(_msDuration > 0);
				if(staffIsVisible)
				{
					w.WriteAttributeString("score", "alignment", null, ((Metrics.Left + Metrics.Right) / 2).ToString(M.En_USNumberFormat));
				}
                w.WriteAttributeString("score", "msDuration", null, _msDuration.ToString());

                if(this.Metrics != null && staffIsVisible)
                    this.Metrics.WriteSVG(w);

                w.SvgEndGroup();
            }
        }

		public override string ToString()
		{
			StringBuilder sb = new StringBuilder();
			sb.Append("inputRest   ");
			sb.Append(InfoString);
			return sb.ToString();
		}

	}
}
