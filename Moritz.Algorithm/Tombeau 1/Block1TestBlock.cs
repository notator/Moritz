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
        private Block Block1TestBlock(List<List<Trk>> TTTrks)
        {
            Block block1 = GetBars1and2FromTTTrks(TTTrks);

            return block1;
        }

        private Block GetBars1and2FromTTTrks(List<List<Trk>> TTTrks)
        {
            Trk channel0Trk = GetChannelTrk(0, TTTrks[0][0]); // absolutePitchHierarchy(0, 0); NPitchesPerOctave = 9
            channel0Trk.AdjustVelocitiesHairpin(0, channel0Trk.EndMsPositionReFirstIUD, 0.1, 1);

            Gamut gamut = ((MidiChordDef)channel0Trk[0]).Gamut;

            Trk channel1Trk = GetChannelTrk(1, TTTrks[0][1]);
            channel1Trk.AdjustVelocitiesHairpin(0, channel1Trk.EndMsPositionReFirstIUD, 1, 0.1);
            List<byte> velocityPerAbsolutePitch = ((MidiChordDef)channel1Trk[0]).Gamut.GetVelocityPerAbsolutePitch(5, false);
            channel1Trk.SetVelocityPerAbsolutePitch(velocityPerAbsolutePitch);
            channel1Trk.TransposeInGamut(-(gamut.NPitchesPerOctave));
            channel1Trk.MsPositionReContainer = 231;

            List<Trk> trks = new List<Trk>();
            trks.Add(channel0Trk);
            trks.Add(channel1Trk);
            Seq seq0 = new Seq(0, trks, MidiChannelIndexPerOutputVoice);
            int barline1MsPosition = channel0Trk.MsDuration;

            Seq seq1 = seq0.Clone();
            seq1.Trks[1].TransposeInGamut(-((int)(gamut.NPitchesPerOctave * 1.5)));
            seq1.Trks[1].InsertClefChange(0, "b");

            seq0.Concat(seq1);
            int barline2MsPosition = seq0.MsDuration;

            Block bars1and2 = new Block(seq0, new List<int>() { barline1MsPosition, barline2MsPosition });

            return bars1and2;
        }

        private Trk GetChannelTrk(int midiChannel, Trk trkArg)
        {
            Trk trk = trkArg.Clone();
            trk.MidiChannel = midiChannel;
            Trk tt1 = trk.Clone();
            tt1.TransposeInGamut(2);
            Trk tt2 = tt1.Clone();
            tt2.TransposeInGamut(1);
            Trk tt3 = tt2.Clone();
            tt3.TransposeInGamut(2);
            Trk tt4 = tt3.Clone();
            tt4.TransposeInGamut(2);
            Trk tt5 = tt4.Clone();
            tt5.TransposeInGamut(2);
            Trk tt6 = tt5.Clone();
            tt6.TransposeInGamut(1);

            tt4.Permute(1, 5);

            trk.AddRange(tt1);
            trk.AddRange(tt2);
            trk.AddRange(tt3);
            trk.AddRange(tt4);
            trk.AddRange(tt5);
            trk.AddRange(tt6);
            trk.MsDuration = 6500;

            return trk;
        }
    }
}
