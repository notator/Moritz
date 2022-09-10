using Moritz.Globals;
using Moritz.Spec;
using Moritz.Xml;

using System;
using System.Diagnostics;

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

        public override void WriteSVG(SvgWriter w)
        {
            if(LocalCautionaryChordDef == null)
            {
                w.SvgStartGroup(Metrics.CSSObjectClass.ToString());  // "inputRest"

                Debug.Assert(_msDuration > 0);
                w.WriteAttributeString("score", "alignment", null, ((Metrics.Left + Metrics.Right) / 2).ToString(M.En_USNumberFormat));
                w.WriteAttributeString("score", "msDuration", null, _msDuration.ToString());

                if(this.Metrics != null)
                    ((RestMetrics)this.Metrics).WriteSVG(w);

                w.SvgEndGroup();
            }
        }

        public override string ToString() => "inputRest " + InfoString;

    }
}
