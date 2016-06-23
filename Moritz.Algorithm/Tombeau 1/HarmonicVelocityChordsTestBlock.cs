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
        private Block HarmonicVelocityChordsTestBlock(List<MidiChordDef> majorCircularPalette, List<MidiChordDef> minorCircularPalette)
        {
            List<Trk> trks = new List<Trk>();

            List<Block> blocks = new List<Block>();

            blocks.Add(HarmonicVelocityTestBlock(majorCircularPalette));
            blocks.Add(HarmonicVelocityTestBlock(minorCircularPalette));

            Block mainBlock = blocks[0];
            for(int i = 1; i < blocks.Count; ++i)
            {
                mainBlock.Concat(blocks[i]);
            }

            return mainBlock;           
        }

        private Block HarmonicVelocityTestBlock(List<MidiChordDef> staffMidiChordDefs)
        {
            List<Trk> trks = new List<Trk>();
            int velocityFactorsListsCount = 0;
            while(true)
            {
                try
                {
                    M.GetVelocityFactors(velocityFactorsListsCount);
                    velocityFactorsListsCount++;
                }
                catch
                {
                    break;
                }
            }
            for(int vfIndex = 0; vfIndex < velocityFactorsListsCount; ++vfIndex)
            {
                Trk trk = new Trk(vfIndex);

                List<int> absolutePitchHierarchy = M.GetAbsolutePitchHierarchy(0, 0);
                List<int> velocityPerAbsolutePitch = M.GetVelocityPerAbsolutePitch(absolutePitchHierarchy, vfIndex);

                foreach(MidiChordDef paletteMcd in staffMidiChordDefs)
                {
                    MidiChordDef mcd = ((MidiChordDef)paletteMcd.Clone());
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

        private List<MidiChordDef> UpsideDownChords(List<MidiChordDef> midiChordDefs, int density)
        {
            List<MidiChordDef> minor = new List<MidiChordDef>();
            foreach(MidiChordDef mcd in midiChordDefs)
            {
                MidiChordDef upsideDownMCD = mcd.UpsideDown();

                minor.Add(upsideDownMCD);
            }
            return minor;
        }



    }
}
