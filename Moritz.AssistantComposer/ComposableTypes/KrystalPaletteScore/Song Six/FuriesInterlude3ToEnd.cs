using System.Collections.Generic;
using System.Diagnostics;
using System;

using Moritz.Score;
using Moritz.Score.Midi;
using Krystals4ObjectLibrary;

namespace Moritz.AssistantComposer
{
    /// <summary>
    /// The Algorithm for Song 6.
    /// This will develope as composition progresses...
    /// </summary>
    internal partial class SongSixAlgorithm : MidiCompositionAlgorithm
    {
        /// <summary>
        /// The arguments are all complete to the end of Verse 3
        /// </summary>
        private void GetFuriesInterlude3ToEnd(Furies1 furies1, Furies2 furies2, Furies3 furies3, Furies4 furies4,
            Clytemnestra clytemnestra, VoiceDef wind1, VoiceDef wind2, VoiceDef wind3, List<PaletteDef> palettes,
            Dictionary<string, int> msPositions)
        {
            furies1.GetFinale(palettes, msPositions);
            furies1.AdjustAlignments(clytemnestra, wind3);
            furies1.AdjustVelocities(msPositions);

            //furies2.GetFinale(palettes, msPositions);
            //furies2.AdjustAlignments(clytemnestra, wind3);
            //furies2.AdjustVelocities(msPositions);

            //furies3.GetFinale(palettes, msPositions);
            //furies3.AdjustAlignments(clytemnestra, wind3);
            //furies3.AdjustVelocities(msPositions);

            //furies4.GetFinale(palettes, msPositions);
            //furies4.AdjustAlignments(clytemnestra, wind3);
            //furies4.AdjustVelocities(msPositions);

            AdjustPostludePans(furies1, furies2, furies3, furies4, msPositions["postlude"]);
            SetFuriesFinalePitches(furies1, furies2, furies3, furies4, msPositions);
        }

        private void AdjustPostludePans(Furies1 furies1, Furies2 furies2, Furies3 furies3, Furies4 furies4, int postludeMsPosition)
        {
            double posDiff = ((double)(furies1.EndMsPosition - postludeMsPosition)) / 4;
            int postludeMsPosition1 = postludeMsPosition + (int)posDiff;
            int postludeMsPosition2 = postludeMsPosition + (int)(posDiff * 2);
            int postludeMsPosition3 = postludeMsPosition + (int)(posDiff * 3);

            AdjustFuries1PostludePan(furies1, postludeMsPosition, postludeMsPosition1, postludeMsPosition2, postludeMsPosition3);
            //AdjustFuries2PostludePan(furies2, postludeMsPosition, postludeMsPosition1, postludeMsPosition2, postludeMsPosition3);
            //AdjustFuries3PostludePan(furies3, postludeMsPosition);
            // Furies 4 pans dont change
        }

        internal void AdjustFuries1PostludePan(Furies1 furies1, int postludeMsPosition, int postludeMsPosition1, int postludeMsPosition2, int postludeMsPosition3)
        {
            furies1.SetPanGliss(postludeMsPosition, postludeMsPosition1, 64, 32);
            furies1.SetPanGliss(postludeMsPosition1, postludeMsPosition2, 32, 96);
            furies1.SetPanGliss(postludeMsPosition2, postludeMsPosition3, 96, 0);
            furies1.SetPanGliss(postludeMsPosition3, furies1.EndMsPosition, 0, 127);
        }

        /// <summary>
        /// Motion is contrary to the pan gliss in furies 1
        /// </summary>
        internal void AdjustFuries2PostludePan(Furies2 furies2, int postludeMsPosition, int postludeMsPosition1, int postludeMsPosition2, int postludeMsPosition3)
        {
            furies2.SetPanGliss(postludeMsPosition, postludeMsPosition1, 20, 69);
            furies2.SetPanGliss(postludeMsPosition1, postludeMsPosition2, 69, 35);
            furies2.SetPanGliss(postludeMsPosition2, postludeMsPosition3, 35, 127);
            furies2.SetPanGliss(postludeMsPosition3, furies2.EndMsPosition, 127, 0);
        }

        internal void AdjustFuries3PostludePan(Furies3 furies3, int postludeMsPosition)
        {
            furies3.SetPanGliss(postludeMsPosition, furies3.EndMsPosition, 107, 0);
        }

        private void SetFuriesFinalePitches(VoiceDef furies1, VoiceDef furies2, VoiceDef furies3, VoiceDef furies4, 
            Dictionary<string, int> msPositions)
        {
            
        }

    }
}
