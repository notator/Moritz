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

            blocks.Add(VelocityPerAbsolutePitchTestBar(majorCircularPalette));
            blocks.Add(VelocityPerAbsolutePitchTestBar(minorCircularPalette));

            Block mainBlock = blocks[0];
            for(int i = 1; i < blocks.Count; ++i)
            {
                mainBlock.Concat(blocks[i]);
            }

            return mainBlock;           
        }

        private Block VelocityPerAbsolutePitchTestBar(List<MidiChordDef> staffMidiChordDefs)
        {
            List<Trk> trks = new List<Trk>();

            for(int rootPitch = 0; rootPitch < 8; ++rootPitch)
            {
                List<int> absolutePitchHierarchy = M.GetAbsolutePitchHeirarchy(0, rootPitch);
                Trk trk = new Trk(rootPitch);

                foreach(MidiChordDef paletteMcd in staffMidiChordDefs)
                {
                    MidiChordDef mcd = ((MidiChordDef)paletteMcd.Clone());

                    List<int> velocityPerAbsolutePitch = M.GetVelocityPerAbsolutePitch(rootPitch, 127, 0, 0);
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
