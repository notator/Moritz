using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;

using Moritz.Score.Notation;

namespace Moritz.Score
{
    public class InputVoice : Voice
    {
        public InputVoice(InputStaff inputStaff)
            : base(inputStaff, Byte.MaxValue)
        {
        }

        /// <summary>
        /// Writes out the noteObjects, and possibly the performanceOptions for an InputVoice.
        /// </summary>
        public override void WriteSVG(SvgWriter w)
        {
            w.SvgStartGroup("inputVoice", null);

            base.WriteSVG(w);

            if(PerformanceControlDef != null)
            {
                PerformanceControlDef.WriteSvg(w);
            }

            w.SvgEndGroup(); // inputVoice
        }

        public PerformanceControlDef PerformanceControlDef = null;
    }
}
