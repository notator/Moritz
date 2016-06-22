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
        private Block VerticalVelocityColorsTestBlock()
        {
            List<Trk> sys1Trks = new List<Trk>();
            //List<byte> topVelocities = new List<byte>() { 1, 12, 24, 35, 47, 58, 70, 81, 93, 104, 116, 127 };
            //List<byte> rootVelocities = new List<byte>() { 127, 116, 104, 93, 81, 70, 58, 47, 35, 24, 12, 1 };

            List<int> velocityPerAbsolutePitch =
                M.GetVelocityPerAbsolutePitch(5,    // The base pitch for the pitch hierarchy.
                                              127,  // the velocity given to any absolute base pitch (if it exists) in the MidiChordDef
                                              0,    // index in M.RelativePitchHierarchies: in range [0..21]
                                              0     // index in M.VelocityFactors: in range [0..7]
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

            List<int> barlineMsPositions = new List<int>();
            barlineMsPositions.Add(seq.Trks[0].UniqueDefs[3].MsPositionReFirstUD);
            barlineMsPositions.Add(seq.Trks[0].UniqueDefs[6].MsPositionReFirstUD);
            barlineMsPositions.Add(seq.Trks[0].UniqueDefs[9].MsPositionReFirstUD);
            barlineMsPositions.Add(seq.MsDuration);

            Block block = new Block(seq, barlineMsPositions);

            return block;
        }

    }
}
