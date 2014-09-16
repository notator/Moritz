using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;

namespace Moritz.Score
{
    public class OutputVoice : Voice
    {
        public OutputVoice(OutputStaff outputStaff, Voice voice)
            : base(outputStaff, voice)
        {
        }

        public OutputVoice(OutputStaff outputStaff, byte midiChannel)
            : base(outputStaff, midiChannel)
        {
        }

        public override void WriteSVG(SvgWriter w)
        {
            w.SvgStartGroup("outputVoice", null);
            w.WriteAttributeString("score", "midiChannel", null, MidiChannel.ToString());

            base.WriteSVG(w);

            w.SvgEndGroup(); // outputVoice or inputVoice
        }
    }
}
