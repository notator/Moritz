using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;

using Moritz.Score.Notation;

namespace Moritz.Score
{
    public class OutputVoice : Voice
    {
        public OutputVoice(OutputStaff outputStaff, byte midiChannel)
            : base(outputStaff)
        {
            MidiChannel = midiChannel;
        }

        public override void WriteSVG(SvgWriter w)
        {
            w.SvgStartGroup("outputVoice", null);
            w.WriteAttributeString("score", "midiChannel", null, MidiChannel.ToString());
            w.WriteAttributeString("score", "masterVolume", null, MasterVolume.ToString());

            if(PerformanceControlDef != null)
            {
                PerformanceControlDef.WriteSvg(w);
            }

            base.WriteSVG(w);

            w.SvgEndGroup(); // outputVoice
        }

        /// <summary>
        /// This field is set by composition algorithms.
        /// It is stored in and retrieved from SVG files. 
        /// </summary>
        public byte MidiChannel = 0;
        /// <summary>
        /// The composition algorithm must set the MasterVolume (to a value > 0)
        /// in every OutputVoice in the first bar of the score.
        /// All other OutputVoices retain the default value 0. 
        /// </summary>
        public byte MasterVolume = 0; // default value
        /// <summary>
        /// This field is set by composition algorithms.
        /// It is stored in and retrieved from SVG files. 
        /// </summary>
        public PerformanceControlDef PerformanceControlDef = null;
    }
}
