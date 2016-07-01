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

        private Block GamutTestBar(int chordDensity, bool useConjugate)
        {
            List<Trk> trks = new List<Trk>();

            int relativePitchHierarchyIndex = 0;
            int nPitchesPerOctave = 8;
            int lyricNumber = 1;

            for(int midiChannel = 0; midiChannel < 8; ++midiChannel)
            {
                List<int> absolutePitchHierarchy = M.GetAbsolutePitchHierarchy(relativePitchHierarchyIndex++, midiChannel);
                Gamut gamut = new Gamut(absolutePitchHierarchy, nPitchesPerOctave);

                Trk trk = new Trk(midiChannel);

                for(int gamutIndex = 40; gamutIndex < 52; ++gamutIndex)
                {
                    int mcdRootPitch = gamut.List[gamutIndex];
                    MidiChordDef mcd = new MidiChordDef(1000, gamut, mcdRootPitch, 3, null);
                    if(useConjugate == true)
                    {
                        mcd = mcd.Conjugate();
                    }

                    mcd.Lyric = (lyricNumber++).ToString();

                    List<byte> velocityPerAbsolutePitch = gamut.GetVelocityPerAbsolutePitch(20, true);
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
