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
        private Block VerticalVelocityChordsTestBlock(List<MidiChordDef> majorCircularPalette, List<MidiChordDef> minorCircularPalette)
        {
            List<Block> blocks = new List<Block>();

            blocks.Add(VerticalVelocityTestBlock(majorCircularPalette));
            blocks.Add(VerticalVelocityTestBlock(minorCircularPalette));

            Block mainBlock = blocks[0];
            for(int i = 1; i < blocks.Count; ++i)
            {
                mainBlock.Concat(blocks[i]);
            }

            return mainBlock;           
        }

        private Block VerticalVelocityTestBlock(List<MidiChordDef> staffMidiChordDefs)
        {
            List<Trk> trks = new List<Trk>();
            List<byte> topVelocities = new List<byte>();
            topVelocities.Add(M.MaxMidiVelocity[M.Dynamic.ppp]);
            topVelocities.Add(M.MaxMidiVelocity[M.Dynamic.pp]);
            topVelocities.Add(M.MaxMidiVelocity[M.Dynamic.p]);
            topVelocities.Add(M.MaxMidiVelocity[M.Dynamic.mp]);
            topVelocities.Add(M.MaxMidiVelocity[M.Dynamic.mf]);
            topVelocities.Add(M.MaxMidiVelocity[M.Dynamic.f]);
            topVelocities.Add(M.MaxMidiVelocity[M.Dynamic.ff]);
            topVelocities.Add(M.MaxMidiVelocity[M.Dynamic.fff]);

            for(int vfIndex = 0; vfIndex < topVelocities.Count; ++vfIndex)
            {
                Trk trk = new Trk(vfIndex);
                byte rootVelocity = M.MaxMidiVelocity[M.Dynamic.fff];
                byte topVelocity = topVelocities[vfIndex];

                foreach(MidiChordDef paletteMcd in staffMidiChordDefs)
                {
                    MidiChordDef mcd = ((MidiChordDef)paletteMcd.Clone());

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
