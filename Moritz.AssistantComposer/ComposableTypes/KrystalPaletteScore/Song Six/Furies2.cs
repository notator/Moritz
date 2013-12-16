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
            VoiceDef furies2 = GetFuries2Interlude2(clytemnestra, wind1, furies3);
            furies2 = AddFuries2ChirpsForInterlude2AndVerse3(clytemnestra, wind1, furies2, _paletteDefs[7]);

            return furies2;
        }

        private VoiceDef AddFuries2ChirpsForInterlude2AndVerse3(Clytemnestra clytemnestra, VoiceDef wind1, VoiceDef furies2, PaletteDef chirpsPalette)
        {
            int[] chirpIndices = { 4, 6, 10, 0, 1, 3, 5, 7, 9, 11 };
            int[] transpositions = { 2, 0, 4, 11, 5, 10, 6, 9, 7, 8 };
            //double[] velocityfactors = { 0.3, 0.31, 0.32, 0.34, 0.35, 0.36, 0.37, 0.39, 0.4, 0.42, 0.43, 0.45 };
            double[] velocityfactors = { 0.32, 0.3, 0.35, 0.45, 0.36, 0.43, 0.37, 0.42, 0.39, 0.4 };
            int[] msPositions =
            { 
                furies2[2].MsPosition, 
                furies2[6].MsPosition, 
                furies2[16].MsPosition, 
                furies2[26].MsPosition,
                furies2[26].MsPosition + chirpsPalette[chirpIndices[3]].MsDuration,
                clytemnestra[129].MsPosition,
                clytemnestra[143].MsPosition + 110,
                clytemnestra[156].MsPosition + 220,
                clytemnestra[156].MsPosition + 220 + chirpsPalette[chirpIndices[7]].MsDuration,
                clytemnestra[168].MsPosition + 330,
            };
            for(int i = 9; i >=0 ; --i)
            {
                LocalMidiDurationDef cheep = new LocalMidiDurationDef(chirpsPalette[chirpIndices[i]]);
                cheep.MsPosition = msPositions[i];
                //cheep.MsDuration *= 2;
                cheep.UniqueMidiDurationDef.AdjustVelocities(velocityfactors[i]);
                cheep.UniqueMidiDurationDef.Transpose(transpositions[i]);
                furies2.InsertInRest(cheep);
            }

            furies2.AlignObjectAtIndex(50, 51, 52, clytemnestra[130].MsPosition);
            furies2.AlignObjectAtIndex(55, 56, 57, clytemnestra[159].MsPosition);

            furies2.AgglomerateRestOrChordAt(31);

            return furies2;
        }

        /// <summary>
        /// Steals the ticks from furies 3, then agglommerates the remaining rests in furies3...
        /// </summary>
        private VoiceDef GetFuries2Interlude2(Clytemnestra clytemnestra, VoiceDef wind1, VoiceDef furies3)
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

            furies3.AgglomerateRests();

            return furies2;
        }
    }
}