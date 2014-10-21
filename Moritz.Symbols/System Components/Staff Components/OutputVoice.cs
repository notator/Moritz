using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;

using Moritz.Midi;
using Moritz.Xml;
using Moritz.Spec;

namespace Moritz.Symbols
{
    public class OutputVoice : Voice
    {
        public OutputVoice(OutputStaff outputStaff, int? voiceID, byte midiChannel)
            : base(outputStaff)
        {
            _voiceID = voiceID;
            _midiChannel = midiChannel;
        }

        public override void WriteSVG(SvgWriter w)
        {
            w.SvgStartGroup("outputVoice", null);
            if(VoiceID != null) // VoiceID is null if there are no InputVoices in the score.
            {
                w.WriteAttributeString("score", "voiceID", null, VoiceID.ToString());
            }

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

        public int? VoiceID { get { return _voiceID; } }
        private int? _voiceID = null;

        public byte MidiChannel { get { return _midiChannel; } } 
        private byte _midiChannel;

        /// <summary>
        /// The composition algorithm must set the MasterVolume (to a value != null)
        /// in every OutputVoice in the first bar of the score.
        /// All other OutputVoices retain the default value null unless the MasterVolume
        /// is changed during a performance. 
        /// </summary>
        public byte? MasterVolume = null; // default value
        /// <summary>
        /// This field is set by composition algorithms.
        /// It is stored in and retrieved from SVG files. 
        /// </summary>
        public InputControls InputControls = null;
    }
}
