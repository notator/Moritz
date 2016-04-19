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
        private Trk GetWind3(int midiChannel, Palette palette, Krystal krystal)
        {
            Trk wind3 = palette.NewTrk(midiChannel, krystal);
            wind3.Transpose(0, wind3.Count, -13);
            wind3.StepwiseGliss(74, wind3.Count, 19);
            return wind3;
        }

		private Trk GetWind2(int midiChannel, Trk wind3, Clytemnestra clytemnestra)
        {
            List<IUniqueDef> clytLmdds = clytemnestra.UniqueDefs;
            int rotationMsPosition = clytLmdds[59].MsPositionReTrk + 700;

            Trk wind2 = GetRotatedWind(midiChannel, wind3, rotationMsPosition);
            wind2.Transpose(0, wind2.Count, 12);

            wind2.StepwiseGliss(0, 15, 7);
            wind2.Transpose(15, wind2.Count, 7);

            wind2.StepwiseGliss(75, wind2.Count, 12);

            wind2.AlignObjectAtIndex(0, 15, 82, rotationMsPosition);
            wind2.AlignObjectAtIndex(15, 39, 82, clytLmdds[173].MsPositionReTrk);
            wind2.AlignObjectAtIndex(39, 57, 82, clytLmdds[268].MsPositionReTrk);

            return wind2;
        }

		private Trk GetWind1(int midiChannel, Trk wind3, Trk wind2, Clytemnestra clytemnestra)
        {
            List<IUniqueDef> clytLmdds = clytemnestra.UniqueDefs;
            int rotationMsPosition = clytLmdds[116].MsPositionReTrk + 700;

			Trk wind1 = GetRotatedWind(midiChannel, wind3, rotationMsPosition);
            wind1.Transpose(0, wind1.Count, 19);
            wind1.StepwiseGliss(0, 25, 12);
            wind1.Transpose(25, wind1.Count, 12);

            wind1.AlignObjectAtIndex(0, 15, 82, wind2[15].MsPositionReTrk);
            wind1.AlignObjectAtIndex(15, 25, 82, rotationMsPosition);
            wind1.AlignObjectAtIndex(25, 74, 82, clytLmdds[289].MsPositionReTrk);

            return wind1;
        }

        /// <summary>
        /// Returns a VoiceDef containing clones of the UniqueDefs in the originalVoiceDef argument,
        /// rotated so that the original first IUniqueDef is positioned close to rotationMsPosition.
        /// </summary>
        /// <param name="originalVoiceDef"></param>
        /// <returns></returns>
        private Trk GetRotatedWind(int midiChannel, Trk originalVoiceDef, int rotationMsPosition)
        {
            Trk tempWind = originalVoiceDef.Clone();
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
                iu.MsPositionReTrk = msPosition;
                msPosition += iu.MsDuration;
            }
            Trk newRotatedWind = new Trk(midiChannel, newWindLmdds);

            return newRotatedWind;
        }
    }
}
