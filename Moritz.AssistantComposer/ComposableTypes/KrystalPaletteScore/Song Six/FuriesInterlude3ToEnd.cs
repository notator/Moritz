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
            Clytemnestra clytemnestra, VoiceDef wind1, VoiceDef wind2, VoiceDef wind3, List<PaletteDef> palettes)
        {
            PaletteDef f1CheepsPalette = palettes[12];
            PaletteDef f2ChirpsPalette = palettes[11];
            PaletteDef f3ChirpsPalette = palettes[10];
            PaletteDef f4SongsPalette = palettes[9];

            int interlude3StartMsPosition = wind1[38].MsPosition;
            int endOfPieceMsPosition = wind1.EndMsPosition;
            int durationFromInterlude3StartToEnd = endOfPieceMsPosition - interlude3StartMsPosition;

            //PermutationKrystal krystal = new PermutationKrystal("C://Moritz/krystals/krystals/pk4(12)-2.krys");
            ExpansionKrystal krystal = new ExpansionKrystal("C://Moritz/krystals/krystals/xk3(12.12.1)-1.krys");
            List<int> strandIndices = new List<int>();
            int index = 0;
            for(int i = 0; i < krystal.Strands.Count; ++i)
            {
                strandIndices.Add(index);
                index += krystal.Strands[i].Values.Count;
            }

            VoiceDef f1e = new VoiceDef(f1CheepsPalette, krystal);
            List<int> f1eStrandDurations = GetStrandDurations(f1e, strandIndices);

            for(int i = f1e.Count - 1; i > 0; --i)
            {
                if(strandIndices.Contains(i))
                {
                    UniqueMidiRestDef umrd = new UniqueMidiRestDef(f1e[i].MsPosition, f1eStrandDurations[strandIndices.IndexOf(i)]);
                    f1e.Insert(i, umrd);
                }
            }
            Debug.Assert(f1e.EndMsPosition <= durationFromInterlude3StartToEnd);
            f1e.StartMsPosition = wind3[40].MsPosition;
            Debug.Assert(f1e.EndMsPosition <= endOfPieceMsPosition);
            furies1.InsertInRest(f1e);

            VoiceDef f2e = new VoiceDef(f2ChirpsPalette, krystal);
            Debug.Assert(f2e.EndMsPosition <= durationFromInterlude3StartToEnd);
            f2e.StartMsPosition = furies1[33].MsPosition + (furies1[33].MsDuration / 2);
            Debug.Assert(f2e.EndMsPosition <= endOfPieceMsPosition);
            furies2.InsertInRest(f2e);

            VoiceDef f3e = new VoiceDef(f3ChirpsPalette, krystal);
            Debug.Assert(f3e.EndMsPosition <= durationFromInterlude3StartToEnd);
            f3e.StartMsPosition = wind3[42].MsPosition; // bar 63
            Debug.Assert(f3e.EndMsPosition <= endOfPieceMsPosition);
            furies3.InsertInRest(f3e);

            VoiceDef f4e = new VoiceDef(f4SongsPalette, krystal);
            Debug.Assert(f4e.EndMsPosition <= durationFromInterlude3StartToEnd);
            f4e.StartMsPosition = wind3[42].MsPosition; // bar 63
            Debug.Assert(f4e.EndMsPosition <= endOfPieceMsPosition);
            furies4.InsertInRest(f4e);
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
