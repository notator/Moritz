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
        private Block VelocityPerAbsolutePitchTestBlock(List<MidiChordDef> majorCircularPalette, List<MidiChordDef> minorCircularPalette)
        {
            List<Block> blocks = new List<Block>();
            int chordDensity = 5;

            blocks.Add(VelocityPerAbsolutePitchTestBar(chordDensity, false));
            blocks.Add(VelocityPerAbsolutePitchTestBar(chordDensity, true));

            Block mainBlock = blocks[0];
            for(int i = 1; i < blocks.Count; ++i)
            {
                mainBlock.Concat(blocks[i]);
            }

            return mainBlock;           
        }

        private Block VelocityPerAbsolutePitchTestBar(int chordDensity, bool invert)
        {
            List<Trk> trks = new List<Trk>();

            int relativePitchHierarchyIndex = 0;

            List<int> blockAbsolutePitchHierarchy = M.GetAbsolutePitchHierarchy(relativePitchHierarchyIndex, 0);

            for(int rootPitch = 0; rootPitch < 8; ++rootPitch)
            {
                List<int> staffAbsolutePitchHierarchy = M.GetAbsolutePitchHierarchy(relativePitchHierarchyIndex, rootPitch);

                Trk trk = new Trk(rootPitch);

                for(int j = 0; j < 20; ++j)
                {
                    MidiChordDef mcd = new MidiChordDef(chordDensity, 60 + j, staffAbsolutePitchHierarchy, 127, 1200, true);
                    if(invert == true)
                    {
                        mcd = mcd.UpsideDown();
                    }

                    mcd.Lyric = (j).ToString();

                    List<int> velocityPerAbsolutePitch = M.GetVelocityPerAbsolutePitch(blockAbsolutePitchHierarchy, 0);
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
