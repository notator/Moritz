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
            VoiceDef furies1 = GetFuries1Interlude2AndVerse3(clytemnestra, wind1, furies2, _paletteDefs[8]);

            return furies1;
        }

        private VoiceDef GetFuries1Interlude2AndVerse3(Clytemnestra clytemnestra, VoiceDef wind1, VoiceDef furies2, PaletteDef cheepsPalette)
        {
            VoiceDef furies1 = GetEmptyVoiceDef(wind1.EndMsPosition);

            int[] cheepIndices = { 4, 8, 2, 6, 10, 0, 1, 3, 5, 7, 9, 11 };
            int[] transpositions = { 2, 1, 3, 0, 4, -3, 5, 10, 6, 9, 7, 8 };
            double[] velocityfactors = { 0.32, 0.31, 0.34, 0.3, 0.35, 0.37, 0.36, 0.43, 0.37, 0.42, 0.39, 0.4 };
            int[] msPositions =
            { 
                furies2[8].MsPosition + 200, 
                furies2[12].MsPosition + 100, 
                furies2[24].MsPosition + 300, 
                furies2[30].MsPosition + 400, 
                furies2[40].MsPosition + 500,
                clytemnestra[122].MsPosition,
                clytemnestra[132].MsPosition + 110,
                clytemnestra[141].MsPosition + 220,
                clytemnestra[150].MsPosition + 330,
                clytemnestra[158].MsPosition + 440,
                clytemnestra[164].MsPosition + 550,
                clytemnestra[173].MsPosition,
            };
            for(int i = 0; i < 12; ++i)
            {
                IUniqueMidiDurationDef cheep = cheepsPalette[cheepIndices[i]].CreateUniqueMidiDurationDef();
                cheep.MsPosition = msPositions[i];
                cheep.MsDuration *= 2;
                cheep.AdjustVelocities(velocityfactors[i]);
                cheep.Transpose(transpositions[i]);
                furies1.InsertInRest(cheep);
            }

            furies1.AlignObjectAtIndex(11, 12, 13, clytemnestra[123].MsPosition);
            furies1.AlignObjectAtIndex(21, 22, 23, clytemnestra[168].MsPosition);

            return furies1;
        }

    }
}
