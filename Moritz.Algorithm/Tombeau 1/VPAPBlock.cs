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
        private Block VPAPBlock(List<List<Trk>> TTTrks)
        {
            Seq vpapSeq = VPAPBars(TTTrks);
            Block vpapBlock = new Block(vpapSeq);

            int blockMsDuration = vpapBlock.MsDuration;
            vpapBlock.AddBarline(blockMsDuration / 2);
            vpapBlock.AddBarline(blockMsDuration);

            return vpapBlock;
        }

        private Seq VPAPBars(List<List<Trk>> TTTrks)
        {
            Trk vpapTrka = GetVPAPChannelTrk(2, TTTrks[0][1]);
            Gamut gamut = ((MidiChordDef)vpapTrka[0]).Gamut;

            vpapTrka.AdjustVelocitiesHairpin(0, vpapTrka.EndMsPositionReFirstIUD, 1, 0.1);
            List<byte> velocityPerAbsolutePitch = ((MidiChordDef)vpapTrka[0]).Gamut.GetVelocityPerAbsolutePitch(25, false);
            vpapTrka.SetVelocityPerAbsolutePitch(velocityPerAbsolutePitch);
            vpapTrka.TransposeInGamut(-(gamut.NPitchesPerOctave));
            MidiChordDef lastTrk2MidiChordDef = (MidiChordDef)vpapTrka[vpapTrka.Count - 1];
            lastTrk2MidiChordDef.BeamContinues = false;

            Trk vpapTrkb = vpapTrka.Clone();
            vpapTrkb.TransposeInGamut(6);
            vpapTrkb.SetVelocityPerAbsolutePitch(velocityPerAbsolutePitch);
            vpapTrkb.InsertClefChange(0, "t1");

            vpapTrka.AddRange(vpapTrkb);

            List<Trk> trks = new List<Trk>();
            trks.Add(vpapTrka);

            Seq VPAPBars = new Seq(0, trks, MidiChannelIndexPerOutputVoice);

            return VPAPBars;
        }

        private Trk GetVPAPChannelTrk(int midiChannel, Trk trkArg)
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

            tt1.Permute(1, 7);
            tt3.Permute(1, 7);
            tt5.Permute(1, 7);

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
