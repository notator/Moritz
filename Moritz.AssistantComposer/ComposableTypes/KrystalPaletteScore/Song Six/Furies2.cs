using System.Collections.Generic;
using System.Diagnostics;
using System;

using Moritz.Score;
using Moritz.Score.Midi;
using Krystals4ObjectLibrary;

namespace Moritz.AssistantComposer
{
    /// <summary>
    /// The Furies "constructors" (part of the Song Six algorithm).
    /// </summary>
    internal partial class SongSixAlgorithm : MidiCompositionAlgorithm
    {
        private VoiceDef GetFuries2(Clytemnestra clytemnestra, VoiceDef wind1, VoiceDef furies3, List<PaletteDef> _paletteDefs)
        {
            VoiceDef furies2 = GetFuries2Interlude2(clytemnestra, wind1, furies3, _paletteDefs[8]);

            return furies2;
        }

        private VoiceDef GetFuries2Interlude2(Clytemnestra clytemnestra, VoiceDef wind1, VoiceDef furies3, PaletteDef cheepsPalette)
        {
            VoiceDef furies2 = GetEmptyVoiceDef(wind1.EndMsPosition);

            List<int> furies3TickIndices = new List<int>()
            {
                66,70,74,81,85,89,93,
                96,100,104,109,113,117,122,
                126,130,135,139,143,148,152
            };
            for(int i = 0; i < furies3TickIndices.Count; ++i)
            {
                int f3Index = furies3TickIndices[i];
                LocalMidiDurationDef ticksChord = furies3[f3Index];
                Debug.Assert(ticksChord.UniqueMidiDurationDef is UniqueMidiChordDef);
                LocalMidiDurationDef ticksRest = new LocalMidiDurationDef(ticksChord.MsPosition, ticksChord.MsDuration);
                furies3.Replace(f3Index, ticksRest);
                furies2.InsertInRest(ticksChord);
            }

            LocalMidiDurationDef lastTicksBeforeVerse3 = new LocalMidiDurationDef(furies2[39].UniqueMidiDurationDef);
            lastTicksBeforeVerse3.MsPosition = furies3[155].MsPosition + furies3[155].MsDuration;
            lastTicksBeforeVerse3.MsDuration = clytemnestra[117].MsPosition - lastTicksBeforeVerse3.MsPosition;
            lastTicksBeforeVerse3.Transpose(10);
            furies2.InsertInRest(lastTicksBeforeVerse3);

            return furies2;
        }
    }
}