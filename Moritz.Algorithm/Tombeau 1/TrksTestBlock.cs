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
        private Block TrksTestBlock(List<MidiChordDef> paletteMidiChordDefs)
        {
            List<int> absolutePitchHierarchy = M.GetAbsolutePitchHierarchy(0, 5);
            Gamut gamut = new Gamut(absolutePitchHierarchy, 8);
            List<byte> velocityPerAbsolutePitch = gamut.GetVelocityPerAbsolutePitch(20, true);

            List<MidiChordDef> mcds = paletteMidiChordDefs; // a clone

            List<Trk> trkList = new List<Trk>();

            for(int i = 0; i < MidiChannelIndexPerOutputVoice.Count; ++i)
            {
                int chordDensity = 3;
                List<IUniqueDef> iuds = new List<IUniqueDef>();
                int msPosReFirstUD = 0;
                for(int j = 0; j < 12; ++j)
                {
                    MidiChordDef mcd = (MidiChordDef)mcds[j].Clone();
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
                trkList.Add(trk);
            }

            for(int i = 0; i < trkList.Count; ++i)
            {
                Trk trk = trkList[i];
                trk.Permute(i + 1, 7); // sets trk.AxisIndex
            }

            Seq seq = new Seq(0, trkList, MidiChannelIndexPerOutputVoice); // The Seq's MsPosition can change again later.

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
