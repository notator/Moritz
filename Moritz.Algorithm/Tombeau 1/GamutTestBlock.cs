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
        private Block GamutTestBlock()
        {
            List<Block> blocks = new List<Block>();
            int chordDensity = 3;

            blocks.Add(GamutTestBar(chordDensity, false));
            blocks.Add(GamutTestBar(chordDensity, true));

            Block mainBlock = blocks[0];
            for(int i = 1; i < blocks.Count; ++i)
            {
                mainBlock.Concat(blocks[i]);
            }

            return mainBlock;           
        }

        private Block GamutTestBar(int chordDensity, bool invert)
        {
            List<Trk> trks = new List<Trk>();

            int relativePitchHierarchyIndex = 9;
            int BlockHarmonicRoot = 0;
            int nPitchesPerOctave = 8;
            int lyricNumber = 1;

            List<int> blockAbsolutePitchHierarchy = M.GetAbsolutePitchHierarchy(relativePitchHierarchyIndex, BlockHarmonicRoot);
            Gamut gamut = new Gamut(blockAbsolutePitchHierarchy, nPitchesPerOctave);

            for(int rootPitch = 0; rootPitch < 8; ++rootPitch)
            {
                List<int> staffAbsolutePitchHierarchy = M.GetAbsolutePitchHierarchy(relativePitchHierarchyIndex, rootPitch);

                Trk trk = new Trk(rootPitch);

                for(int j = 0; j < gamut.List.Count; ++j)
                {
                    int mcdRootPitch = gamut.List[j];
                    if(mcdRootPitch > 59 && mcdRootPitch < 85)
                    {
                        MidiChordDef mcd = new MidiChordDef(chordDensity, mcdRootPitch, staffAbsolutePitchHierarchy, 127, 1200, true);
                        if(invert == true)
                        {
                            mcd = mcd.Inversion();
                        }

                        mcd.Lyric = (lyricNumber++).ToString();

                        List<int> velocityPerAbsolutePitch = M.GetVelocityPerAbsolutePitch(blockAbsolutePitchHierarchy, 0);
                        mcd.SetVelocityPerAbsolutePitch(velocityPerAbsolutePitch);

                        trk.Add(mcd);
                    }
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
