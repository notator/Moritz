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
        private Block VelocityPerAbsolutePitchTestBlock()
        {
            List<Block> blocks = new List<Block>();
            int chordDensity = 3;

            blocks.Add(VelocityPerAbsolutePitchTestBar(chordDensity, false, false));
            blocks[0].Trks[2].InsertClefChange(0, "t");
            blocks.Add(VelocityPerAbsolutePitchTestBar(chordDensity, false, true));

            Block block = blocks[0];
            for(int i = 1; i < blocks.Count; ++i)
            {
                block.Concat(blocks[i]);
            }

            return block;
        }

        private Block VelocityPerAbsolutePitchTestBar(int chordDensity, bool useConjugate, bool invert)
        {
            List<Trk> trks = new List<Trk>();

            int relativePitchHierarchyIndex = 0;

            List<int> blockAbsolutePitchHierarchy = M.GetAbsolutePitchHierarchy(relativePitchHierarchyIndex, 0);

            for(int midiChannel = 0; midiChannel < 8; ++midiChannel)
            {
                List<int> absolutePitchHierarchy = M.GetAbsolutePitchHierarchy(relativePitchHierarchyIndex++, midiChannel);
                Gamut gamut = new Gamut(absolutePitchHierarchy, 8);

                Trk trk = new Trk(midiChannel);

                for(int gamutIndex = 40; gamutIndex < 52; ++gamutIndex)
                {
                    int mcdRootPitch = gamut.List[gamutIndex];
                    MidiChordDef mcd = new MidiChordDef(1000, gamut, mcdRootPitch, chordDensity, null);
                    if(useConjugate == true)
                    {
                        mcd = mcd.Conjugate();
                    }
                    if(invert == true)
                    {
                        mcd.Invert(7);
                    }

                    mcd.Lyric = (gamutIndex).ToString();

                    List<byte> velocityPerAbsolutePitch = gamut.GetVelocityPerAbsolutePitch(20, true);
                    mcd.SetVelocityPerAbsolutePitch(velocityPerAbsolutePitch);

                    trk.Add(mcd);
                }

                trks.Add(trk);
            }

            Seq seq = new Seq(0, trks, MidiChannelIndexPerOutputVoice); // The Seq's MsPosition can change again later.
            Block block = new Block(seq, new List<int>() { seq.MsDuration });

            return block;
        }

    }
}
