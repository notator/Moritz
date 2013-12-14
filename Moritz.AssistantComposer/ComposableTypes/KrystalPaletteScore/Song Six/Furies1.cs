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
            VoiceDef furies1 = GetFuries1Interlude2(wind1, furies2, _paletteDefs[8]);

            return furies1;
        }

        private VoiceDef GetFuries1Interlude2(VoiceDef wind1, VoiceDef furies2, PaletteDef cheepsPalette)
        {
            VoiceDef furies1 = GetEmptyVoiceDef(wind1.EndMsPosition);

            int[] cheepIndices = { 4, 8, 2, 6, 10 };
            int[] transpositions = { 2, 1, 3, 0, 4 };
            double[] velocityfactors = { 0.3, 0.33, 0.37, 0.41, 0.45 };
            int[] msPositions =
            { 
                furies2[8].MsPosition, 
                furies2[12].MsPosition, 
                furies2[24].MsPosition, 
                furies2[30].MsPosition, 
                furies2[40].MsPosition, 
            };
            for(int i = 0; i < 5; ++i)
            {
                LocalMidiDurationDef cheep = new LocalMidiDurationDef(cheepsPalette[cheepIndices[i]]);
                cheep.MsPosition = msPositions[i];
                cheep.MsDuration *= 2;
                cheep.UniqueMidiDurationDef.AdjustVelocities(velocityfactors[i]);
                cheep.UniqueMidiDurationDef.Transpose(transpositions[i]);
                furies1.InsertInRest(cheep);
            }

            return furies1;
        }

    }
}
