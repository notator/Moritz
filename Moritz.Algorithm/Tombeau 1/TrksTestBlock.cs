using System;
using System.Collections.Generic;
using System.Diagnostics;

using Krystals4ObjectLibrary;

using Moritz.Globals;
using Moritz.Palettes;
using Moritz.Spec;

namespace Moritz.Algorithm.Tombeau1
{
	public partial class Tombeau1Algorithm : CompositionAlgorithm
	{
        private Block TrksTestBlock()
        {
            Seq seq = new Seq(0, MidiChannelIndexPerOutputVoice); // The Seq's MsPosition can change again later.

            List<int> absolutePitchHierarchy = M.GetAbsolutePitchHierarchy(0, 5);
            List<int> velocityPerAbsolutePitch = M.GetVelocityPerAbsolutePitch(absolutePitchHierarchy, 0);

            List<MidiChordDef> mcds = Tombeau1Templates.PaletteMidiChordDefs[0]; // a clone

            for(int i = 0; i < MidiChannelIndexPerOutputVoice.Count; ++i)
            {
                int chordDensity = 3;
                List<IUniqueDef> iuds = new List<IUniqueDef>();
                int msPosReFirstUD = 0;
                for(int j = 0; j < 12; ++j)
                {
                    MidiChordDef mcd = (MidiChordDef) mcds[j].Clone();
                    mcd.MsPositionReFirstUD = msPosReFirstUD;
                    msPosReFirstUD += mcd.MsDuration;

                    mcd.Transpose(i + j - 7);
                    mcd.Lyric = (j).ToString() + "." + chordDensity.ToString();

                    mcd.SetVerticalDensity(chordDensity);

                    mcd.SetVelocityPerAbsolutePitch(velocityPerAbsolutePitch);

                    IUniqueDef iud = mcd as IUniqueDef;
                    iuds.Add(iud);
                }
                Trk trk = new Trk(MidiChannelIndexPerOutputVoice[i], 0, iuds);
                trk.SortVelocityIncreasing();
                seq.SetTrk(trk);
            }

            for(int i = 0; i < seq.Trks.Count; ++i)
            {
                Trk trk = seq.Trks[i];
                trk.Permute(i + 1, 7); // sets trk.AxisIndex
            }

            seq.AlignTrkAxes();

            /*******/

            Trk trk3 = seq.Trks[3];
            trk3.PermutePartitions(7, 1, new List<int>() { 2, 2, 1, 2, 2, 1, 2 });

            seq.Normalize();

            Block block = new Block(seq, new List<int>() { seq.MsDuration });

            return block;
        }

    }
}
