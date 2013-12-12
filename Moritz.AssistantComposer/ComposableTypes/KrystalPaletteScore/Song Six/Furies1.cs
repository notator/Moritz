using System.Collections.Generic;
using System.Diagnostics;
using System;

using Moritz.Score;
using Moritz.Score.Midi;
using Krystals4ObjectLibrary;

namespace Moritz.AssistantComposer
{
    /// <summary>
    /// The Furies1 "constructor" (part of the Song Six algorithm).
    /// </summary>
    internal partial class SongSixAlgorithm : MidiCompositionAlgorithm
    {

        private VoiceDef GetFuries1(Clytemnestra clytemnestra, VoiceDef wind1, VoiceDef furies3, VoiceDef furies2, List<PaletteDef> _paletteDefs)
        {
            VoiceDef furies1 = GetEmptyVoiceDef(wind1.EndMsPosition);

            return furies1;
        }
    }
}
