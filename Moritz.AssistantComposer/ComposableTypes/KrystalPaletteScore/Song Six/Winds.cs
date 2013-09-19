using System;
using System.Collections.Generic;
using System.Diagnostics;

using Krystals4ObjectLibrary;

using Moritz.Score;
using Moritz.Score.Midi;

namespace Moritz.AssistantComposer
{
    class Winds
    {
        // The durations of the Winds are adjusted to the blockMsDurations
        public Winds(List<Krystal> krystals, List<PaletteDef> paletteDefs, int topWindChannelIndex, List<int> blockMsDurations)
        {
            Debug.Assert(blockMsDurations.Count == 11);

            int nWindsVoices = 1; // including the baseVoice
            Voice baseVoice = GetBaseVoice(krystals[0], paletteDefs[0], topWindChannelIndex + nWindsVoices - 1, blockMsDurations);
            _voices.Insert(0, baseVoice);
            // etc. -- create and insert other wind voices, so that wind channels are ordered top down in order of channel.
        }

        private Voice GetBaseVoice(Krystal krystal, PaletteDef paletteDef, int baseWindChannelIndex, List<int> blockMsDurations)
        {
            Voice baseVoice = new Voice(null, (byte) baseWindChannelIndex);

            List<List<int>> kValues = krystal.GetValues((uint)1);
            List<int> sequence = kValues[0];

            MidiDefList midiDefList = new MidiDefList(paletteDef, sequence);
            int finalTotalDuration = 0;
            for(int i = 0; i < 11; ++i)
            {
                finalTotalDuration += blockMsDurations[i];
            }

            // The durations of the contained LocalizedMidiDurationDefs are adjusted to the blockMsDurations
            midiDefList.MsDuration = finalTotalDuration;

            baseVoice.LocalizedMidiDurationDefs = midiDefList.LocalizedMidiDurationDefs;

            return baseVoice;
        }

        // each voice has the duration of the whole piece
        public List<Voice> Voices { get { return _voices; } } // voices are ordered top to bottom
        private List<Voice> _voices = new List<Voice>(); 
    }
}
