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
            int midiChannel = 0;
            Trk trk0 = GetChannelTrk(midiChannel++, TTTrks[0][0]); // absolutePitchHierarchy(0, 0); NPitchesPerOctave = 9
            trk0.AdjustVelocitiesHairpin(0, trk0.EndMsPositionReFirstIUD, 0.1, 1);

            Gamut gamut = ((MidiChordDef)trk0[0]).Gamut;

            int initialDelay = 1500;
            Trk trk1 = trk0.Clone();
            trk1.MidiChannel = midiChannel++;
            trk1.TransposeInGamut(8);
            trk1.MsPositionReContainer = initialDelay;
            trk1.MsDuration = trk0.MsDuration - (initialDelay / 2); 

            Trk trk2 = GetChannelTrk(midiChannel++, TTTrks[0][1]);
            trk2.AdjustVelocitiesHairpin(0, trk2.EndMsPositionReFirstIUD, 1, 0.1);
            List<byte> velocityPerAbsolutePitch = ((MidiChordDef)trk2[0]).Gamut.GetVelocityPerAbsolutePitch(5, false);
            trk2.SetVelocityPerAbsolutePitch(velocityPerAbsolutePitch);
            trk2.TransposeInGamut(-(gamut.NPitchesPerOctave));
            trk2.MsPositionReContainer = 231;

            List<Trk> trks = new List<Trk>();
            trks.Add(trk0);
            trks.Add(trk1);
            trks.Add(trk2);
            Seq seq0 = new Seq(0, trks, MidiChannelIndexPerOutputVoice);
            int barline1MsPosition = trk0.MsDuration;

            Seq seq1 = seq0.Clone();
            seq1.Trks[1].MsPositionReContainer = (initialDelay / 2);
            //seq1.Trks[2].TransposeInGamut(-((int)(gamut.NPitchesPerOctave * 1.5)));
            seq1.Trks[2].TransposeInGamut(-2);
            //seq1.Trks[2].InsertClefChange(0, "b");

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
