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
        public InputRestSymbol(Voice voice, IUniqueDef iumdd, int absMsPosition, PageFormat pageFormat)
            : base(voice, iumdd, absMsPosition, pageFormat.MinimumCrotchetDuration, pageFormat.MusicFontHeight * pageFormat.InputSizeFactor)
        {
            if(iumdd is CautionaryChordDef)
            {
                Console.WriteLine("rest is CautionaryChordDef!");
            }
            LocalCautionaryChordDef = iumdd as CautionaryChordDef;
        }

		/// <summary>
		/// This function should never be used. Use the other WriteSVG() instead.
		/// </summary>
		/// <param name="w"></param>
		public override void WriteSVG(SvgWriter w)
		{
			M.Assert(false, "This function should never be called.");
		}

		public void WriteSVG(SvgWriter w, bool graphicsOnly)
        {
            if(LocalCautionaryChordDef == null)
            {
				w.SvgStartGroup(Metrics.CSSObjectClass.ToString());  // "inputRest"

                M.Assert(_msDuration > 0);
				if(!graphicsOnly)
				{
					w.WriteAttributeString("score", "alignment", null, ((Metrics.Left + Metrics.Right) / 2).ToString(M.En_USNumberFormat));
					w.WriteAttributeString("score", "msDuration", null, _msDuration.ToString());
				}

                if(this.Metrics != null)
                    ((RestMetrics)this.Metrics).WriteSVG(w);

                w.SvgEndGroup();
            }
        }

		public override string ToString() => "inputRest " + InfoString;

	}
}
