using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;

using Moritz.Midi;
using Moritz.Xml;
using Moritz.VoiceDef;

namespace Moritz.Symbols
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

            if(MasterVolume != null)
            {
                w.WriteAttributeString("score", "masterVolume", null, MasterVolume.ToString());
            }

            if(InputControls != null)
            {
                InputControls.WriteSvg(w);
            }

            base.WriteSVG(w);

            w.SvgEndGroup(); // outputVoice
        }

        public IEnumerable<MidiChordDef> MidiChordDefs
        {
            get
            {
                foreach(IUniqueDef iud in UniqueDefs)
                {
                    MidiChordDef midiChordDef = iud as MidiChordDef;
                    if(midiChordDef != null)
                        yield return midiChordDef;
                }
            }
        }

        /// <summary>
        /// This field is set by composition algorithms.
        /// It is stored in and retrieved from SVG files. 
        /// </summary>
        public byte MidiChannel = 0;
        /// <summary>
        /// The composition algorithm must set the MasterVolume (to a value != null)
        /// in every OutputVoice in the first bar of the score.
        /// All other OutputVoices retain the default value 0. 
        /// </summary>
        public byte? MasterVolume = null; // default value
        /// <summary>
        /// This field is set by composition algorithms.
        /// It is stored in and retrieved from SVG files. 
        /// </summary>
        public InputControls InputControls = null;
    }
}
