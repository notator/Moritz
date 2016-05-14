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
        private Tuple<Block, List<int>> SimpleVelocityColorsTestBlock()
        {
            List<Trk> trks = new List<Trk>();
            MidiChordDef baseMidiChordDef = new MidiChordDef(new List<byte>() { (byte)64 }, new List<byte>() { (byte)127 }, 0, 1000, true);
            byte velocity = 0;
            for(int trkIndex = 0; trkIndex < 3; ++trkIndex)
            {
                Trk trk = new Trk(MidiChannelIndexPerOutputVoice[trkIndex], 0, new List<IUniqueDef>());
                for(int j = 0; j < 16; ++j)
                {
                    velocity++;
                }
                trks.Add(trk);
            }
            for(int trkIndex = 3; trkIndex < MidiChannelIndexPerOutputVoice.Count; ++trkIndex)
            {
                Trk trk = new Trk(MidiChannelIndexPerOutputVoice[trkIndex], 0, new List<IUniqueDef>());
                for(int j = 0; j < 16; ++j)
                {
                    MidiChordDef mcd = baseMidiChordDef.Clone() as MidiChordDef;
                    mcd.BasicMidiChordDefs[0].Velocities[0] = velocity;
                    mcd.NotatedMidiVelocities[0] = velocity;
                    mcd.Lyric = (velocity).ToString();
                    velocity++;

                    trk.Add(mcd);
                }

                trks.Add(trk);
            }

            Seq seq = new Seq(0, trks, MidiChannelIndexPerOutputVoice); // The Seq's MsPosition can change again later.

            Block block = new Block(seq);
            List<int> barlineMsPositions = new List<int>() { block.MsDuration };
            return new Tuple<Block, List<int>>(block, barlineMsPositions);
        }
    }
}
