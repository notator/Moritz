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
        private void GetFuriesInterlude3ToEnd(VoiceDef furies1, VoiceDef furies2, VoiceDef furies3, VoiceDef furies4,
            Clytemnestra clytemnestra, VoiceDef wind1, VoiceDef wind2, VoiceDef wind3, List<PaletteDef> palettes,
            Dictionary<string, int> msPositions)
        {
            VoiceDef f1Finale = GetF1Finale(palettes, msPositions);
            furies1.InsertInRest(f1Finale);
            AdjustF1Alignments(furies1, clytemnestra, wind3);
            AdjustF1Velocities(furies1, msPositions);
            AdjustF1PostludePan(furies1, msPositions["postlude"]);

            VoiceDef f2Finale = GetF2Finale(palettes, msPositions);
            furies2.InsertInRest(f2Finale);
            //AdjustF2Alignments(furies2, clytemnestra, wind3);
            //AdjustF2Velocities(furies2, msPositions);
            //AdjustF2PostludePan(furies2, msPositions["postlude"]);

            //VoiceDef f3Finale = GetF3Finale(palettes, msPositions);
            //furies3.InsertInRest(f3Finale);
            //AdjustF3Alignments(furies3, clytemnestra, wind3);
            //AdjustF3Velocities(furies3, msPositions);
            //AdjustF3PostludePan(furies3, msPositions["postlude"]);

            //VoiceDef f4Finale = GetF4Finale(palettes, msPositions);
            //furies4.InsertInRest(f4Finale);
            //AdjustF4Alignments(furies4, clytemnestra, wind3);
            //AdjustF4Velocities(furies4, msPositions);
            //AdjustF4PostludePan(furies4, msPositions["postlude"]);

            SetFuriesPitchesInterlude3ToEnd(furies1, furies2, furies3, furies4, msPositions);
        }

        private void SetFuriesPitchesInterlude3ToEnd(VoiceDef furies1, VoiceDef furies2, VoiceDef furies3, VoiceDef furies4, 
            Dictionary<string, int> msPositions)
        {
            
        }

        /// <summary>
        /// This should work for furies 1-3, and maybe even 4!
        /// </summary>
        /// <param name="f13"></param>
        private void AdjustFuriesFinalePitchWheelDeviations(VoiceDef f13)
        {
            double furies1StartPwdValue = 5, furies1EndPwdValue = 28;
            double pwdfactor = Math.Pow(furies1EndPwdValue / furies1StartPwdValue, (double)1 / f13.Count); // f13.Count'th root of furies1EndPwdValue/furies1StartPwdValue -- the last pwd should be furies1EndPwdValue

            for(int i = 0; i < f13.Count; ++i)
            {
                f13[i].PitchWheelDeviation = (int)(furies1StartPwdValue * (Math.Pow(pwdfactor, i)));
            }
        }

        /// <summary>
        /// voiceDef contains the UniqueMidiChordDefs defined by a krystal, and nothing else.
        /// </summary>
        /// <param name="voiceDef"></param>
        /// <param name="strandIndices"></param>
        /// <returns></returns>
        private List<int> GetStrandDurations(VoiceDef voiceDef, List<int> strandIndices)
        {
            List<int> strandDurations = new List<int>();
            int duration;
            for(int i = 1; i < strandIndices.Count; ++i)
            {
                duration = 0;
                for(int j = strandIndices[i-1]; j < strandIndices[i]; ++j)
                {
                    duration += voiceDef[j].MsDuration;
                }
                strandDurations.Add(duration);
            }
            duration = 0;
            for(int i = strandIndices[strandIndices.Count - 1]; i < voiceDef.Count; ++i)
            {
                duration += voiceDef[i].MsDuration;
            }
            strandDurations.Add(duration);
            return strandDurations;
        }
    }
}
