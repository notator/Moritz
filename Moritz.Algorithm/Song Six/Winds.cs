using System.Collections.Generic;

using Krystals4ObjectLibrary;
using Moritz.Algorithm;
using Moritz.Palettes;
using Moritz.Spec;

namespace Moritz.Algorithm.SongSix
{
    /// <summary>
    /// The Wind "constructors" (part of the Song Six algorithm).
    /// </summary>
    public partial class SongSixAlgorithm : CompositionAlgorithm
    {
        private TrkDef GetWind3(byte midiChannel, Palette palette, Krystal krystal)
        {
            TrkDef wind3 = palette.NewTrkDef(midiChannel, krystal);
            wind3.Transpose(0, wind3.Count, -13);
            wind3.StepwiseGliss(74, wind3.Count, 19);
            return wind3;
        }

		private TrkDef GetWind2(byte midiChannel, TrkDef wind3, Clytemnestra clytemnestra)
        {
            List<IUniqueDef> clytLmdds = clytemnestra.UniqueDefs;
            int rotationMsPosition = clytLmdds[59].MsPosition + 700;

            TrkDef wind2 = GetRotatedWind(midiChannel, wind3, rotationMsPosition);
            wind2.Transpose(0, wind2.Count, 12);

            wind2.StepwiseGliss(0, 15, 7);
            wind2.Transpose(15, wind2.Count, 7);

            wind2.StepwiseGliss(75, wind2.Count, 12);

            wind2.AlignObjectAtIndex(0, 15, 82, rotationMsPosition);
            wind2.AlignObjectAtIndex(15, 39, 82, clytLmdds[173].MsPosition);
            wind2.AlignObjectAtIndex(39, 57, 82, clytLmdds[268].MsPosition);

            return wind2;
        }

		private TrkDef GetWind1(byte midiChannel, TrkDef wind3, TrkDef wind2, Clytemnestra clytemnestra)
        {
            List<IUniqueDef> clytLmdds = clytemnestra.UniqueDefs;
            int rotationMsPosition = clytLmdds[116].MsPosition + 700;

			TrkDef wind1 = GetRotatedWind(midiChannel, wind3, rotationMsPosition);
            wind1.Transpose(0, wind1.Count, 19);
            wind1.StepwiseGliss(0, 25, 12);
            wind1.Transpose(25, wind1.Count, 12);

            wind1.AlignObjectAtIndex(0, 15, 82, wind2[15].MsPosition);
            wind1.AlignObjectAtIndex(15, 25, 82, rotationMsPosition);
            wind1.AlignObjectAtIndex(25, 74, 82, clytLmdds[289].MsPosition);

            return wind1;
        }

        /// <summary>
        /// Returns a VoiceDef containing clones of the UniqueMidiDurationDefs in the originalVoiceDef argument,
        /// rotated so that the original first IUniqueMidiDurationDef is positioned close to rotationMsPosition.
        /// </summary>
        /// <param name="originalVoiceDef"></param>
        /// <returns></returns>
        private TrkDef GetRotatedWind(byte midiChannel, TrkDef originalVoiceDef, int rotationMsPosition)
        {
            TrkDef tempWind = originalVoiceDef.DeepClone();
			tempWind.MidiChannel = midiChannel;

            int finalBarlineMsPosition = originalVoiceDef.EndMsPosition;
            int msDurationAfterSynch = finalBarlineMsPosition - rotationMsPosition;

            List<IUniqueDef> originalLmdds = tempWind.UniqueDefs;
            List<IUniqueDef> originalStartLmdds = new List<IUniqueDef>();
            List<IUniqueDef> newWindLmdds = new List<IUniqueDef>();
            int accumulatingMsDuration = 0;
            for(int i = 0; i < tempWind.Count; ++i)
            {
                if(accumulatingMsDuration < msDurationAfterSynch)
                {
                    originalStartLmdds.Add(originalLmdds[i]);
                    accumulatingMsDuration += originalLmdds[i].MsDuration;
                }
                else
                {
                    newWindLmdds.Add(originalLmdds[i]);
                }
            }
            newWindLmdds.AddRange(originalStartLmdds);

            int msPosition = 0;
            foreach(IUniqueDef iu in newWindLmdds)
            {
                iu.MsPosition = msPosition;
                msPosition += iu.MsDuration;
            }
            TrkDef newRotatedWind = new TrkDef(midiChannel, newWindLmdds);

            return newRotatedWind;
        }
    }
}
