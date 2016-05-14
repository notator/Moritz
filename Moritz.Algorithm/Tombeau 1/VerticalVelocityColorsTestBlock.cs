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
        private Tuple<Block, List<int>> VerticalVelocityColorsTestBlock()
        {
            List<Trk> sys1Trks = new List<Trk>();
            //List<byte> topVelocities = new List<byte>() { 1, 12, 24, 35, 47, 58, 70, 81, 93, 104, 116, 127 };
            //List<byte> rootVelocities = new List<byte>() { 127, 116, 104, 93, 81, 70, 58, 47, 35, 24, 12, 1 };

            List<int> velocityPerAbsolutePitch =
                GetVelocityPerAbsolutePitch(5,    // The base pitch for the pitch hierarchy.
                                            127,  // the velocity given to any absolute base pitch (if it exists) in the MidiChordDef
                                            circularPitchHierarchies[0], // the pitch hierarchy for the chord,
                                            velocityFactors[0]  // A list of 12 values in descending order, each value in range 1..0
                                           );

            Palette palette = GetPaletteByName("Tombeau1.1");
            for(int i = 0; i < MidiChannelIndexPerOutputVoice.Count; ++i)
            {
                int chordDensity = 4;
                List<IUniqueDef> sys1mcds = PaletteMidiChordDefs(palette);
                for(int j = 0; j < sys1mcds.Count; ++j)
                {
                    MidiChordDef mcd = sys1mcds[j] as MidiChordDef;

                    mcd.Transpose(i + j - 7);
                    mcd.Lyric = (j).ToString() + "." + chordDensity.ToString();

                    mcd.SetVerticalDensity(chordDensity);

                    //mcd.SetVerticalVelocityGradient(rootVelocities[j], topVelocities[j]);

                    mcd.SetVelocityPerAbsolutePitch(velocityPerAbsolutePitch);
                }
                Trk trk = new Trk(MidiChannelIndexPerOutputVoice[i], 0, sys1mcds);
                sys1Trks.Add(trk);
            }

            Seq seq = new Seq(0, sys1Trks, MidiChannelIndexPerOutputVoice); // The Seq's MsPosition can change again later.

            Block block = new Block(seq);
            List<int> barlineMsPositions = new List<int>() { block.MsDuration };
            return new Tuple<Block, List<int>>(block, barlineMsPositions);
        }

    }
}
