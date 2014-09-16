using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;

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

            if(PerformanceOptions != null)
            {
                PerformanceOptions.WriteSVG(w);
            }

            w.SvgEndGroup(); // inputVoice
        }

        public PerformanceOptions PerformanceOptions = null;
    }
}
