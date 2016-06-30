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
        private Block VerticalVelocityGradientTestBlock()
        {
            List<Block> blocks = new List<Block>();
            int chordDensity = 5;

            blocks.Add(VerticalVelocityGradientTestBar(chordDensity, false));
            blocks.Add(VerticalVelocityGradientTestBar(chordDensity, true));

            Block mainBlock = blocks[0];
            for(int i = 1; i < blocks.Count; ++i)
            {
                mainBlock.Concat(blocks[i]);
            }

            return mainBlock;           
        }

        private Block VerticalVelocityGradientTestBar(int chordDensity, bool useConjugate)
        {
            List<Trk> trks = new List<Trk>();

            int relativePitchHierarchyIndex = 0;

            for(int rootPitch = 0; rootPitch < 8; ++rootPitch)
            {
                List<int> absolutePitchHierarchy = M.GetAbsolutePitchHierarchy(relativePitchHierarchyIndex++, rootPitch);
                Gamut gamut = new Gamut(absolutePitchHierarchy, 8);

                Trk trk = new Trk(rootPitch);

                for(int gamutIndex = 40; gamutIndex < 52; ++gamutIndex)
                {
                    int mcdRootPitch = gamut.List[gamutIndex];

                    MidiChordDef mcd = new MidiChordDef(1000, gamut, mcdRootPitch, 3, null);
                    if(useConjugate == true)
                    {
                        mcd = mcd.Conjugate();
                    }

                    mcd.Lyric = (gamutIndex).ToString();

                    mcd.SetVerticalVelocityGradient(127, 12);

                    trk.Add(mcd);
                }

                for(int gamutIndex = 40; gamutIndex < 52; ++gamutIndex)
                {
                    int mcdRootPitch = gamut.List[gamutIndex];
                    MidiChordDef mcd = new MidiChordDef(1000, gamut, mcdRootPitch, chordDensity, null);
                    if(useConjugate == true)
                    {
                        mcd = mcd.Conjugate();
                    }

                    mcd.Lyric = (gamutIndex).ToString();

                    List<byte> velocityPerAbsolutePitch = gamut.GetVelocityPerAbsolutePitch(20);
                    mcd.SetVelocityPerAbsolutePitch(velocityPerAbsolutePitch);

                    trk.Add(mcd);
                }

                trks.Add(trk);
            }

            Seq seq = new Seq(0, trks, MidiChannelIndexPerOutputVoice); // The Seq's MsPosition can change again later.

            List<int> barlineMsPositions = new List<int>();
            barlineMsPositions.Add(seq.MsDuration);

            Block block = new Block(seq, barlineMsPositions);

            return block;
        }

    }
}
