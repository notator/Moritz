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

        private VoiceDef GetFuries4(int firstRestMsDuration, Clytemnestra clytemnestra, VoiceDef wind1, List<PaletteDef> palettes)
        {
            VoiceDef furies4 = GetSnores(firstRestMsDuration, clytemnestra, wind1, palettes[1]);
            AddGrowlsToInterlude2AndVerse3(furies4, clytemnestra, palettes[3]);

            return furies4;
        }

        private void AddGrowlsToInterlude2AndVerse3(VoiceDef furies4, Clytemnestra clytemnestra, PaletteDef growlsPalette)
        {
            int[] growlIndices = { 0,2,5,1 };
            //int[] transpositions = { 0,0,0,0 };
            //double[] velocityfactors = { 1,1,1,1 };
            int[] msPositions =
            { 
                furies4[24].MsPosition + 200, 
                furies4[26].MsPosition + 200, 
                furies4[30].MsPosition + 200, 
                furies4[32].MsPosition + 200, 
            };
            int[] msDurations =
            {
                furies4[24].MsDuration / 2,
                furies4[26].MsDuration / 2,
                furies4[30].MsDuration / 2,
                furies4[32].MsDuration / 2
            };

            for(int i = 3; i >= 0; --i)
            {
                IUniqueMidiDurationDef growl = growlsPalette[growlIndices[i]].CreateUniqueMidiDurationDef();
                growl.MsPosition = msPositions[i];
                growl.MsDuration = msDurations[i];
                //growl.AdjustVelocities(velocityfactors[i]);
                //growl.Transpose(transpositions[i]);
                furies4.InsertInRest(growl);
            }

            furies4.AgglomerateRestOrChordAt(40);

            furies4.AlignObjectAtIndex(34, 35, 36, clytemnestra[140].MsPosition);
            furies4.AlignObjectAtIndex(35, 36, 37, clytemnestra[141].MsPosition);
            furies4.AlignObjectAtIndex(38, 39, 40, clytemnestra[162].MsPosition);
        }

        /// <summary>
        /// Creates the initial furies4 VoiceDef containing snores to the beginning of Interlude3.
        /// </summary>
        /// <param name="firstRestMsDuration"></param>
        private VoiceDef GetSnores(int firstRestMsDuration, Clytemnestra clytemnestra, VoiceDef wind1, PaletteDef snoresPalette)
        {
            List<IUniqueMidiDurationDef> snores = new List<IUniqueMidiDurationDef>();
            int msPosition = 0;

            IUniqueMidiDurationDef firstRest = new UniqueMidiRestDef(msPosition, firstRestMsDuration);
            snores.Add(firstRest);
            msPosition += firstRestMsDuration;

            #region prelude + verse1
            int[] transpositions1 = { 0, 0, 0, 0, 0, 1, 0 };
            for(int i = 0; i < 7; ++i)
            {
                IUniqueMidiDurationDef snore = snoresPalette[i].CreateUniqueMidiDurationDef();
                snore.MsPosition = msPosition;
                msPosition += snore.MsDuration;
                snore.Transpose(transpositions1[i]);
                snore.PitchWheelDeviation = 3;
                snores.Add(snore);

                UniqueMidiRestDef rest = new UniqueMidiRestDef(msPosition, 2500);
                msPosition += rest.MsDuration;
                snores.Add(rest);
            }
            #endregion

            double factor;
            double msDuration;
            double restDuration;
            int[] transpositions2 = { 1, 1, 2, 2, 3, 3, 4, 4, 5, 5 };
            double[] factors = { 0.93, 0.865, 0.804, 0.748, 0.696, 0.647, 0.602, 0.56, 0.52, 0.484 };
            for(int i = 0; i < 10; ++i)
            {
                IUniqueMidiDurationDef snore = snoresPalette[i / 2].CreateUniqueMidiDurationDef();
                snore.MsPosition = msPosition;
                factor = factors[i];
                msDuration = snore.MsDuration * factor;
                snore.MsDuration = (int)msDuration;
                msPosition += snore.MsDuration;
                snore.Transpose(transpositions2[i]);
                snore.PitchWheelDeviation = 20;
                //snore.MidiVelocity = (byte)((double)snore.MidiVelocity * factor * factor);
                snores.Add(snore);

                restDuration = 2500 / factor;
                UniqueMidiRestDef rest = new UniqueMidiRestDef(msPosition, (int)restDuration);
                msPosition += rest.MsDuration;
                snores.Add(rest);
            }

            snores[snores.Count - 1].MsDuration = clytemnestra.EndMsPosition - snores[snores.Count - 1].MsPosition;

            VoiceDef furies4 = new VoiceDef(snores);

            furies4.AdjustVelocitiesHairpin(13, furies4.Count, 0.25);

            #region alignments before Interlude3
            furies4.AlignObjectAtIndex(7, 8, 9, clytemnestra[3].MsPosition);
            furies4.AlignObjectAtIndex(8, 9, 10, clytemnestra[7].MsPosition);
            furies4.AlignObjectAtIndex(9, 10, 11, clytemnestra[16].MsPosition);
            furies4.AlignObjectAtIndex(10, 11, 12, clytemnestra[24].MsPosition);
            furies4.AlignObjectAtIndex(11, 12, 13, clytemnestra[39].MsPosition);
            furies4.AlignObjectAtIndex(12, 13, 14, clytemnestra[42].MsPosition);
            furies4.AlignObjectAtIndex(14, 34, furies4.Count, wind1[38].MsPosition); // rest at start of Interlude3
            #endregion

            furies4.RemoveScorePitchWheelCommands(0, 13); // pitchwheeldeviation is 20 for R2M



            return furies4;

        }
    }
}
