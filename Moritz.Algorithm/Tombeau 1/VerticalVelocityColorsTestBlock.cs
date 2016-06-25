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

            List<int> absolutePitchHierarchy = M.GetAbsolutePitchHierarchy(0, 0);
            List<int> velocityPerAbsolutePitch = M.GetVelocityPerAbsolutePitch(absolutePitchHierarchy, 0);

            for(int i = 0; i < MidiChannelIndexPerOutputVoice.Count; ++i)
            {
                int chordDensity = 4;
                List<MidiChordDef> mcds = Tombeau1ReadonlyConstants.PaletteMidiChordDefs[0];
                List<IUniqueDef> iuds = new List<IUniqueDef>();
                int msPosReFirstUD = 0;
                for(int j = 0; j < mcds.Count; ++j)
                {

                    MidiChordDef mcd = (MidiChordDef) mcds[j].Clone();
                    mcd.MsPositionReFirstUD = msPosReFirstUD;
                    msPosReFirstUD += mcd.MsDuration;

                    mcd.Transpose(i + j - 7);
                    mcd.Lyric = (j).ToString() + "." + chordDensity.ToString();

                    mcd.SetVerticalDensity(chordDensity);

                    //mcd.SetVerticalVelocityGradient(rootVelocities[j], topVelocities[j]);

                    mcd.SetVelocityPerAbsolutePitch(velocityPerAbsolutePitch);

                    iuds.Add(mcd as IUniqueDef);
                }
                Trk trk = new Trk(MidiChannelIndexPerOutputVoice[i], 0, iuds);
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
